using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure;
using CMAPTask.web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Entities.OB;
using OpenBanking.Infrastructure.Extensions;
using OpenBanking.Infrastructure.Services;
using System.Net.Http;
using System.Text.Json;
using static OpenBanking.Domain.Enums.Enum;
using Transaction = OpenBanking.Domain.Entities.OB.Transaction;

namespace CMAPTask.web.Controllers
{
    public class StartConsentController : Controller
    {
        private readonly IOpenBankingService _openBankingService;
        private readonly IRiskAnalyzer _riskAnalyzer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OBSettings _settings;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly EmailService _emailService;
        private readonly ICreditRepository _creditRepository;

        public StartConsentController(IOpenBankingService openBankingService, IRiskAnalyzer riskAnalyzer, IHttpContextAccessor httpContextAccessor, IOptions<OBSettings> options, ITransactionsRepository transactionsRepository, EmailService emailService, ICreditRepository creditRepository)
        {
            _openBankingService = openBankingService;
            _riskAnalyzer = riskAnalyzer;
            _httpContextAccessor = httpContextAccessor;
            _settings = options.Value;
            _transactionsRepository = transactionsRepository;
            _emailService = emailService;
            _creditRepository = creditRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Consent(string institutionId, string u, string c)
        {
            institutionId = "SANDBOXFINANCE_SFIN0000";
            if (string.IsNullOrEmpty(institutionId))
            {
                Console.WriteLine("[DEBUG] Institution ID is missing in Consent.");
                return BadRequest("Institution ID is required");
            }

            Console.WriteLine($"[DEBUG] Starting consent for institution: {institutionId}");

            var agreementId = await _openBankingService.CreateConsentSessionAsync(institutionId);
            if (string.IsNullOrEmpty(agreementId))
            {
                Console.WriteLine($"[DEBUG] Failed to create agreement for institution: {institutionId}");
                return StatusCode(500, "Failed to create agreement");
            }

            var redirectUri = $"{_settings.SiteBaseURL}StartConsent/ConsentCallback?u={u}&c={c}&a={agreementId}";
            Console.WriteLine($"[DEBUG] Using redirect URI: {redirectUri}");

            var requisition = await _openBankingService.CreateRequisitionAsync(
                institutionId,
                agreementId,
                redirectUri
            );

            if (requisition == null || string.IsNullOrEmpty(requisition.Id) || string.IsNullOrEmpty(requisition.Link))
            {
                Console.WriteLine($"[DEBUG] Failed to create requisition for institution: {institutionId}");
                return StatusCode(500, "Failed to create requisition");
            }

            HttpContext.Session.SetString("RequisitionId", requisition.Id);
            Console.WriteLine($"[DEBUG] Stored requisition ID in session: {requisition.Id}");

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isDesktop = userAgent.Contains("Windows") || userAgent.Contains("Macintosh") || userAgent.Contains("Linux");

            if (isDesktop)
            {
                // Desktop: Show QR code page
                ViewBag.BankLink = requisition.Link;
                return View("ShowQRCode");
            }

            // Mobile: redirect directly
            Console.WriteLine($"[DEBUG] Redirecting to: {requisition.Link}");
            return Redirect(requisition.Link);
        }


        public async Task<IActionResult> Invalid()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> ConsentCallback(string u, string c, string a)
        {
            // Log the full callback URL and query parameters
            var fullUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            Console.WriteLine($"[DEBUG] Callback URL: {fullUrl}");
            var queryParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            Console.WriteLine($"[DEBUG] Query parameters: {string.Join(", ", queryParams.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

            // Check for error parameters
            if (queryParams.ContainsKey("error"))
            {
                var error = queryParams["error"];
                var errorDescription = queryParams.ContainsKey("error_description") ? queryParams["error_description"] : "No additional details.";
                Console.WriteLine($"[DEBUG] Callback error: {error}, Description: {errorDescription}");
                return BadRequest($"Authentication failed: {error}. {errorDescription}");
            }

            // Get the session requisition ID
            var sessionRequisitionId = HttpContext.Session.GetString("RequisitionId");
            Console.WriteLine($"[DEBUG] Session requisition ID: {sessionRequisitionId}");

            // Get the callback requisition ID
            string callbackRequisitionId = null;
            if (queryParams.ContainsKey("ref"))
                callbackRequisitionId = queryParams["ref"];
            else if (queryParams.ContainsKey("requisitionId"))
                callbackRequisitionId = queryParams["requisitionId"];
            else if (queryParams.ContainsKey("requisition"))
                callbackRequisitionId = queryParams["requisition"];
            else
                Console.WriteLine($"[DEBUG] No known requisition ID parameter found in callback. Available parameters: {string.Join(", ", queryParams.Keys)}");

            if (!string.IsNullOrEmpty(callbackRequisitionId))
                Console.WriteLine($"[DEBUG] Callback requisition ID: {callbackRequisitionId}");

            if (string.IsNullOrEmpty(sessionRequisitionId) && string.IsNullOrEmpty(callbackRequisitionId))
            {
                Console.WriteLine("[DEBUG] No requisition ID found in session or callback.");
                return BadRequest("Requisition ID is missing from both session and callback URL.");
            }

            // Use session requisition ID
            string requisitionId = sessionRequisitionId ?? callbackRequisitionId;
            Console.WriteLine($"[DEBUG] Using requisition ID: {requisitionId}");

            try
            {
                Console.WriteLine($"[DEBUG] Processing requisition ID: {requisitionId}");
                var (accounts, status) = await _openBankingService.GetAccountsByRequisitionIdAsync(requisitionId);
                Console.WriteLine($"[DEBUG] Requisition {requisitionId} status: {status}");

                if (status != "LN")
                {
                    Console.WriteLine($"[DEBUG] Requisition {requisitionId} not linked. Status: {status}");
                    return BadRequest($"Requisition not linked. Status: {status}");
                }

                if (accounts == null || !accounts.Any())
                {
                    Console.WriteLine($"[DEBUG] No accounts found for requisition ID: {requisitionId}");
                    return View("NoAccounts");
                }

                Console.WriteLine($"[DEBUG] Found {accounts.Count} accounts for requisition ID: {requisitionId}");

                // Fetch account details to identify currencies
                var accountDetailsList = new List<AccountDetails>();
                foreach (var account in accounts)
                {
                    try
                    {
                        var details = await _openBankingService.GetAccountDetailsAsync(account.Id);
                        accountDetailsList.Add(details);
                        Console.WriteLine($"[DEBUG] Account {account.Id}: Currency={details.Currency}, IBAN={details.Iban}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Failed to fetch details for account {account.Id}: {ex.Message}");
                    }
                }

                // Option 1: Fetch transactions for GBP account only
                //var gbpAccount = accountDetailsList.FirstOrDefault(a => a.Currency == "GBP");
                var gbpAccount = accountDetailsList.FirstOrDefault(a => a.Currency == "EUR");

                if (gbpAccount != null)
                {
                    var transactions = await _openBankingService.GetTransactionsByAccountIdAsync(gbpAccount.Id);
                    if (transactions == null)
                        return View("NoTransactions");

                    var viewModel = new AccountTransactionsViewModel
                    {
                        AccountId = gbpAccount.Id,
                        Currency = gbpAccount.Currency,
                        Transactions = transactions,
                        LastUpdated = transactions.Transactions.LastUpdated
                    };

                    var transactionsJson = JsonSerializer.Serialize(transactions, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });

                    if (!Guid.TryParse(u, out Guid endUserId) || !Guid.TryParse(callbackRequisitionId, out Guid refId) || !Guid.TryParse(sessionRequisitionId, out Guid reqId) || !Guid.TryParse(a, out Guid agreementId))
                    {
                        return View("Invalid");
                    }

                    var userId = User.GetUserId();

                    var entity = new Transaction
                    {
                        UserId = userId,
                        JsonData = transactionsJson,
                        Currency = viewModel.Currency,
                        LastUpdated = DateTime.UtcNow,
                        Reference = refId,
                        ConsentId = reqId,
                        AgreementId = agreementId,
                        EndUserId = endUserId

                    };

                    var transactionId = await _transactionsRepository.SaveAsync(entity);
                    await _creditRepository.UpdateCreditUsage(endUserId, transactionId);
                    await _transactionsRepository.UpdateStatusRequest(endUserId);  
                    
                  
                    return View("TransactionCompleted");
                   
                }
                else
                {
                    Console.WriteLine("[DEBUG] No GBP account found.");
                    // Fallback to first account if GBP not found
                    var firstAccountId = accounts.First().Id;
                    var transactions = await _openBankingService.GetTransactionsByAccountIdAsync(firstAccountId);
                    if (transactions == null)
                    {
                        Console.WriteLine($"[DEBUG] No transactions found for fallback account ID: {firstAccountId}");
                        return View("NoTransactions");
                    }

                    var firstAccountDetails = accountDetailsList.FirstOrDefault(a => a.Id == firstAccountId);
                    var viewModel = new AccountTransactionsViewModel
                    {
                        AccountId = firstAccountId,
                        Currency = firstAccountDetails?.Currency ?? "Unknown",
                        Transactions = transactions
                    };
                    return View("~/Views/OpenBanking/Transactions.cshtml", viewModel);
                }

                // Option 2: Fetch transactions for all accounts (uncomment to use)
                /*
                var allAccountsTransactions = new List<AccountTransactionsViewModel>();
                foreach (var account in accountDetailsList)
                {
                    try
                    {
                        var transactions = await _openBankingService.GetTransactionsByAccountIdAsync(account.Id);
                        if (transactions != null)
                        {
                            allAccountsTransactions.Add(new AccountTransactionsViewModel
                            {
                                AccountId = account.Id,
                                Currency = account.Currency,
                                Transactions = transactions
                            });
                            Console.WriteLine($"[DEBUG] Fetched transactions for account {account.Id} (Currency: {account.Currency})");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] No transactions found for account {account.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Failed to fetch transactions for account {account.Id}: {ex.Message}");
                    }
                }

                if (!allAccountsTransactions.Any())
                {
                    Console.WriteLine("[DEBUG] No transactions found for any accounts.");
                    return View("NoTransactions");
                }

                return View("Transactions", allAccountsTransactions);
                */
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                Console.WriteLine($"[DEBUG] 404 Error for requisition ID {requisitionId}: {ex.Message}");
                return StatusCode(404, $"Requisition ID {requisitionId} not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error processing requisition ID {requisitionId}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return StatusCode(500, $"Error processing requisition: {ex.Message}");
            }
        }

        public IActionResult TransactionCompleted()
        {
            return View();
        }   
       

    }

}
