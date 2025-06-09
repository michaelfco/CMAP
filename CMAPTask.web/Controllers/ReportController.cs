using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure;
using CMAPTask.Infrastructure.Services;
using OpenBanking.web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Interfaces;
using OpenBanking.Infrastructure.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenBanking.Infrastructure.Repository;
using System.Text;

namespace OpenBanking.web.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IRiskAnalyzer _riskAnalyzer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ICompanyEndUserRepository _companyEndUserRepository;
        private readonly OBSettings _settings;
        private readonly ICreditRepository _creditRepository;

        public ReportController(IRiskAnalyzer riskAnalyzer, IHttpContextAccessor httpContextAccessor, IOptions<OBSettings> options, ITransactionsRepository transactionsRepository, ICompanyEndUserRepository companyEndUserRepository, ICreditRepository creditRepository) : base(creditRepository)
        {           
            _riskAnalyzer = riskAnalyzer;
            _httpContextAccessor = httpContextAccessor;
            _settings = options.Value;
            _transactionsRepository = transactionsRepository;
            _companyEndUserRepository = companyEndUserRepository;
            _creditRepository = creditRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string eid, string uid)
        {
            if (!Guid.TryParse(eid, out Guid endUserId) || !Guid.TryParse(uid, out Guid userId))
            {
                return View("Invalid");
            }

            var transaction = await _transactionsRepository.GetCompleteTransactionAsync(endUserId, userId);

            var transactions = JsonSerializer.Deserialize<TransactionResponse>(transaction.JsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var endUser = await _companyEndUserRepository.GetByEndUserIsync(endUserId);
            var view = new AccountTransactionsViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = transactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId,
                CustomerName = $"{endUser.FirstName} {endUser.LastName}"
            };

            var (riskSummary, highRiskTransactions) = _riskAnalyzer.AnalyzeTransactions(transactions.Transactions.Booked);
            view.RiskSummary = riskSummary;
            view.HighRiskTransactions = highRiskTransactions;

            Console.WriteLine($"[DEBUG] Rendering Transactions view for account {view.AccountId} (Currency: {view.Currency})");
            Console.WriteLine($"[DEBUG] Risk Summary: Level={view.RiskSummary.RiskLevel}, Inflows={view.RiskSummary.TotalInflows}, Outflows={view.RiskSummary.TotalOutflows}, Net={view.RiskSummary.NetBalance}");

            return View("Index", view);
        }

        public async Task<IActionResult> ExportToCSV(string eid, string uid, string type)
        {
            if (!Guid.TryParse(eid, out Guid endUserId) || !Guid.TryParse(uid, out Guid userId))
            {
                return BadRequest("Invalid end user ID or user ID.");
            }

            if (type != "booked" && type != "pending" && type != "all")
            {
                return BadRequest("Invalid transaction type. Must be 'booked', 'pending', or 'all'.");
            }

            // Build view model
            var transaction = await _transactionsRepository.GetCompleteTransactionAsync(endUserId, userId);
            var transactions = JsonSerializer.Deserialize<TransactionResponse>(transaction.JsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var endUser = await _companyEndUserRepository.GetByEndUserIsync(endUserId);
            var view = new AccountTransactionsViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = transactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId,
                CustomerName = $"{endUser.FirstName} {endUser.LastName}"
            };

            var (riskSummary, highRiskTransactions) = _riskAnalyzer.AnalyzeTransactions(transactions.Transactions.Booked);
            view.RiskSummary = riskSummary;
            view.HighRiskTransactions = highRiskTransactions;

            // Build CSV
            var csv = new StringBuilder();
            csv.AppendLine("Transaction ID,Date,Amount,Party,Description,Type,Risk,Status");

            // Process transactions based on type
            var transactionSets = new List<(IEnumerable<Transaction> Transactions, string Status)>();
            if (type == "all")
            {
                transactionSets.Add((view.Transactions.Transactions.Booked, "Booked"));
                transactionSets.Add((view.Transactions.Transactions.Pending, "Pending"));
            }
            else
            {
                var selectedTransactions = type == "booked" ? view.Transactions.Transactions.Booked : view.Transactions.Transactions.Pending;
                transactionSets.Add((selectedTransactions, type == "booked" ? "Booked" : "Pending"));
            }

            foreach (var (transactionList, status) in transactionSets)
            {
                foreach (var t in transactionList)
                {
                    var date = t.BookingDate != null && DateTime.TryParse(t.BookingDate, out var parsedDate)
                        ? parsedDate.ToString("dd MMM yyyy")
                        : t.BookingDate ?? "N/A";
                    var amount = decimal.TryParse(t.TransactionAmount?.Amount, out var parsedAmount)
                        ? parsedAmount.ToString("F2")
                        : t.TransactionAmount?.Amount ?? "N/A";
                    var party = t.CreditorName ?? t.DebtorName ?? "Unknown";
                    var description = t.RemittanceInformationUnstructured ?? "";
                    var transactionType = t.ProprietaryBankTransactionCode ?? "";
                    var risk = view.HighRiskTransactions.Contains(t) ? "High Risk" : "Low Risk";

                    // Escape commas and quotes
                    description = description.Contains(",") || description.Contains("\"")
                        ? $"\"{description.Replace("\"", "\"\"")}\""
                        : description;
                    party = party.Contains(",") || party.Contains("\"")
                        ? $"\"{party.Replace("\"", "\"\"")}\""
                        : party;
                    transactionType = transactionType.Contains(",") || transactionType.Contains("\"")
                        ? $"\"{transactionType.Replace("\"", "\"\"")}\""
                        : transactionType;

                    csv.AppendLine($"{t.TransactionId},{date},{amount},{party},{description},{transactionType},{risk},{status}");
                }
            }

            // Return CSV as file
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var stream = new MemoryStream(bytes);
            var filename = type == "all" ? $"transactions_{view.CustomerName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv" : $"transactions_{type}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(stream, "text/csv", filename);
        }



        [Authorize]
        public async Task<IActionResult> ExportToPDF(string eid, string uid)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!Guid.TryParse(eid, out Guid endUserId) || !Guid.TryParse(uid, out Guid userId))
            {
                return NotFound("Invalid end user ID or user ID.");
            }

            if(userIdClaim != userId.ToString())
            {
                return NotFound("Invalid end user ID or user ID.");

            }

            var transaction = await _transactionsRepository.GetCompleteTransactionAsync(endUserId, userId);
            if (transaction == null)
            {
                return NotFound("Transaction data not found.");
            }

            var transactions = JsonSerializer.Deserialize<TransactionResponse>(transaction.JsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var endUser = await _companyEndUserRepository.GetByEndUserIsync(endUserId);

            var model = new AccountTransactionsViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = transactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId,
                CustomerName = $"{endUser.FirstName} {endUser.LastName}"
            };

            var (riskSummary, highRiskTransactions) = _riskAnalyzer.AnalyzeTransactions(transactions.Transactions.Booked, true);
            model.RiskSummary = riskSummary;
            model.HighRiskTransactions = highRiskTransactions;
            //view reporting
            //return View("_ReportExportPDF", model);
            return new Rotativa.AspNetCore.ViewAsPdf("_ReportExportPDF", model)
            {
                FileName = $"OB_Reporting_{model.CustomerName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10), // Top, Right, Bottom, Left in mm
                CustomSwitches = "--footer-center \"Page [page] of [topage]\" --footer-font-size 10 --footer-spacing 5"
            };
        }
    }
}
