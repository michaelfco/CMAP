using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace CMAPTask.Infrastructure.Services
{
    public class OpenBankingService : IOpenBankingService
    {
        private readonly OBTokenService _obTokenService;
        private readonly HttpClient _httpClient;
        private readonly OBSettings _settings;

        public OpenBankingService(
            OBTokenService obTokenService,
            HttpClient httpClient,
            IOptions<OBSettings> options)
        {
            _obTokenService = obTokenService;
            _httpClient = httpClient;
            _settings = options.Value;

            _httpClient.BaseAddress = new Uri(_settings.BaseURL);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<OBTokenResponse> UseTokenAsync()
        {
            var token = await _obTokenService.GetTokenAsync();

            if (token == null)
                throw new Exception("Failed to retrieve token.");

            return token;
        }

        public async Task<List<InstitutionsResponse>> GetInstitutionsAsync(string accessToken, string country = "gb")
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"institutions/?country={country}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var institutions = JsonSerializer.Deserialize<List<InstitutionsResponse>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return institutions ?? throw new Exception("Failed to deserialize institutions response.");
        }

        public async Task<string> CreateConsentSessionAsync(string institutionId)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
            {
                Console.WriteLine("[DEBUG] Failed to retrieve token in CreateConsentSessionAsync.");
                throw new Exception("Failed to retrieve token.");
            }

            var requestBody = new
            {
                institution_id = institutionId,
                max_historical_days = "90", // Changed from 180
                access_valid_for_days = "30",
                access_scope = new string[] { "balances", "details", "transactions" }
            };

            var json = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"[DEBUG] Agreement request body: {json}");
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            var response = await _httpClient.PostAsync("agreements/enduser/", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Failed to create agreement for institution {institutionId}: {response.StatusCode}, {errorContent}");
                throw new Exception($"Failed to create agreement: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Agreement response: {responseContent}");
            using var doc = JsonDocument.Parse(responseContent);

            var agreementId = doc.RootElement.GetProperty("id").GetString();
            Console.WriteLine($"[DEBUG] Created agreement ID: {agreementId} for institution: {institutionId}");

            return agreementId ?? throw new Exception("Failed to retrieve agreement ID.");
        }



        public async Task<string> ExchangeRefForAccessTokenAsync(string requisitionRef)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
                throw new Exception("Failed to retrieve token.");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            // Example endpoint to get requisition details - adjust URL as per GoCardless API
            var response = await _httpClient.GetAsync($"requisitions/{requisitionRef}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to retrieve requisition details: {response.StatusCode}");

            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);

            // Extract access token or accounts info here, depends on API response structure
            // For example, if token or account info is inside:
            // { "accounts": [ { "access_token": "xyz" }, ... ] }

            if (doc.RootElement.TryGetProperty("accounts", out var accountsElement))
            {
                // Assuming first account contains access_token
                var firstAccount = accountsElement.EnumerateArray().FirstOrDefault();
                if (firstAccount.ValueKind != JsonValueKind.Undefined &&
                    firstAccount.TryGetProperty("access_token", out var accessTokenElement))
                {
                    return accessTokenElement.GetString();
                }
            }

            // If access token isn't directly provided, you might need to use account IDs to request transactions
            // You will need to adjust according to the actual API response format

            throw new Exception("Access token not found in requisition response.");
        }

        public async Task<List<TransactionResponse>> GetTransactionsAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("transactions");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var transactions = JsonSerializer.Deserialize<List<TransactionResponse>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return transactions ?? new List<TransactionResponse>();
        }

        public async Task<RequisitionResponse> CreateRequisitionAsync(string institutionId, string agreementId, string redirectUri)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
            {
                Console.WriteLine("[DEBUG] Failed to retrieve token in CreateRequisitionAsync.");
                throw new Exception("Failed to retrieve token.");
            }

            var requestBody = new
            {
                redirect = redirectUri,
                institution_id = institutionId,
                agreement = agreementId,
                user_language = "EN",
                reference = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"[DEBUG] Requisition request body: {json}");
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            var response = await _httpClient.PostAsync("requisitions/", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Failed to create requisition for institution {institutionId}: {response.StatusCode}, {errorContent}");
                throw new Exception($"Failed to create requisition: {response.StatusCode}, {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Requisition response: {responseContent}");

            using var doc = JsonDocument.Parse(responseContent);

            var id = doc.RootElement.GetProperty("id").GetString();
            var link = doc.RootElement.GetProperty("link").GetString();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(link))
            {
                Console.WriteLine("[DEBUG] Missing 'id' or 'link' in requisition response.");
                throw new Exception("Missing 'id' or 'link' in requisition response");
            }

            // Check if link contains the correct requisition ID
            var linkIdMatch = System.Text.RegularExpressions.Regex.Match(link, @"start/([a-f0-9-]+)/");
            var linkId = linkIdMatch.Success ? linkIdMatch.Groups[1].Value : "unknown";
            Console.WriteLine($"[DEBUG] Created requisition ID: {id}, Link: {link}, Link ID: {linkId}, RedirectUri: {redirectUri}");
            if (linkId != id)
                Console.WriteLine($"[DEBUG] Warning: Requisition ID ({id}) does not match ID in link ({linkId})");

            return new RequisitionResponse { Id = id, Link = link, Agreement = agreementId };
        }


        public async Task<(List<BankAccount> Accounts, string Status)> GetAccountsByRequisitionIdAsync(string requisitionId)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
                throw new Exception("Failed to retrieve token.");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            var response = await _httpClient.GetAsync($"requisitions/{requisitionId}/");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to retrieve requisition details for ID {requisitionId}: {response.StatusCode}, {errorContent}");
                throw new Exception($"Failed to retrieve requisition details: {response.StatusCode}, {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var status = doc.RootElement.GetProperty("status").GetString();
            Console.WriteLine($"Requisition {requisitionId} status: {status}");

            var accounts = new List<BankAccount>();
            if (doc.RootElement.TryGetProperty("accounts", out var accountsElement) && accountsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var accountElement in accountsElement.EnumerateArray())
                {
                    var account = new BankAccount
                    {
                        Id = accountElement.GetString() ?? string.Empty
                    };
                    if (!string.IsNullOrEmpty(account.Id))
                        accounts.Add(account);
                }
            }

            return (accounts, status ?? string.Empty);
        }


        public async Task<TransactionResponse> GetTransactionsByAccountIdAsync(string accountId)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
                throw new Exception("Failed to retrieve token.");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            var response = await _httpClient.GetAsync($"accounts/{accountId}/transactions/");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to retrieve transactions for account {accountId}: {response.StatusCode}, {errorContent}");
                throw new Exception($"Failed to retrieve transactions: {response.StatusCode}, {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var transactionsResponse = JsonSerializer.Deserialize<TransactionResponse>(content, options);

            return transactionsResponse ?? throw new Exception("Failed to deserialize transactions response.");
        }


        public async Task<AccountDetails> GetAccountDetailsAsync(string accountId)
        {
            var token = await _obTokenService.GetTokenAsync();
            if (token == null)
            {
                System.Console.WriteLine("[DEBUG] Failed to retrieve token in GetAccountDetailsAsync.");
                throw new System.Exception("Failed to retrieve token.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Access);

            // Fetch account details
            var accountResponse = await _httpClient.GetAsync($"accounts/{accountId}/");
            if (!accountResponse.IsSuccessStatusCode)
            {
                var errorContent = await accountResponse.Content.ReadAsStringAsync();
                System.Console.WriteLine($"[DEBUG] Failed to retrieve details for account {accountId}: {accountResponse.StatusCode}, {errorContent}");
                throw new System.Exception($"Failed to retrieve account details: {accountResponse.StatusCode}, {errorContent}");
            }

            var accountContent = await accountResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var accountDetails = JsonSerializer.Deserialize<AccountDetails>(accountContent, options);

            if (accountDetails == null)
            {
                System.Console.WriteLine("[DEBUG] Failed to deserialize account details response.");
                throw new System.Exception("Failed to deserialize account details response.");
            }

            // Fetch balances to get currency
            try
            {
                var balanceResponse = await _httpClient.GetAsync($"accounts/{accountId}/balances/");
                if (balanceResponse.IsSuccessStatusCode)
                {
                    var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"[DEBUG] Balance response for account {accountId}: {balanceContent}");
                    using var doc = JsonDocument.Parse(balanceContent);
                    var balances = doc.RootElement.GetProperty("balances");
                    if (balances.GetArrayLength() > 0)
                    {
                        var currency = balances[0].GetProperty("balanceAmount").GetProperty("currency").GetString();
                        accountDetails.Currency = currency ?? string.Empty;
                        System.Console.WriteLine($"[DEBUG] Fetched currency for account {accountId}: {accountDetails.Currency}");
                    }
                    else
                    {
                        System.Console.WriteLine($"[DEBUG] No balances found for account {accountId}");
                        accountDetails.Currency = string.Empty;
                    }
                }
                else
                {
                    var errorContent = await balanceResponse.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"[DEBUG] Failed to retrieve balances for account {accountId}: {balanceResponse.StatusCode}, {errorContent}");
                    accountDetails.Currency = string.Empty;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[DEBUG] Failed to fetch currency for account {accountId}: {ex.Message}");
                accountDetails.Currency = string.Empty;
            }

            // Set Currency to GBP if empty and Iban starts with GB
            if (string.IsNullOrEmpty(accountDetails.Currency) && accountDetails.Iban != null && accountDetails.Iban.StartsWith("GB", System.StringComparison.OrdinalIgnoreCase))
            {
                accountDetails.Currency = "GBP";
                System.Console.WriteLine($"[DEBUG] Set currency to GBP for account {accountId} based on Iban: {accountDetails.Iban}");
            }

            System.Console.WriteLine($"[DEBUG] Account {accountId}: Currency={accountDetails.Currency}, IBAN={accountDetails.Iban}, Status={accountDetails.Status}");
            return accountDetails;
        }


    }
}
