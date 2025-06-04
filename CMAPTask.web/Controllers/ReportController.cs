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

namespace OpenBanking.web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IRiskAnalyzer _riskAnalyzer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly OBSettings _settings;

        public ReportController(IRiskAnalyzer riskAnalyzer, IHttpContextAccessor httpContextAccessor, IOptions<OBSettings> options, ITransactionsRepository transactionsRepository)
        {

            _riskAnalyzer = riskAnalyzer;
            _httpContextAccessor = httpContextAccessor;
            _settings = options.Value;
            _transactionsRepository = transactionsRepository;

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

            var view = new AccountTransactionsViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = transactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,                
                EndUserId = endUserId,
                UserId = userId
            };

            var (riskSummary, highRiskTransactions) = _riskAnalyzer.AnalyzeTransactions(transactions.Transactions.Booked);
            view.RiskSummary = riskSummary;
            view.HighRiskTransactions = highRiskTransactions;

            // ✅ Now render the view using the actual model from the DB
            Console.WriteLine($"[DEBUG] Rendering Transactions view for account {view.AccountId} (Currency: {view.Currency})");
            Console.WriteLine($"[DEBUG] Risk Summary: Level={view.RiskSummary.RiskLevel}, Inflows={view.RiskSummary.TotalInflows}, Outflows={view.RiskSummary.TotalOutflows}, Net={view.RiskSummary.NetBalance}");

            return View("Index", view);
        }

     

        [Authorize]
        public async Task<IActionResult> ExportToPDF(string eid, string uid)
        {
            if (!Guid.TryParse(eid, out Guid endUserId) || !Guid.TryParse(uid, out Guid userId))
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

            var model = new AccountTransactionsViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = transactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId
            };

            var (riskSummary, highRiskTransactions) = _riskAnalyzer.AnalyzeTransactions(transactions.Transactions.Booked, true);
            model.RiskSummary = riskSummary;
            model.HighRiskTransactions = highRiskTransactions;

            return new Rotativa.AspNetCore.ViewAsPdf("_ReportExportPDF", model)
            {
                FileName = $"Account_Transactions_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10), // Top, Right, Bottom, Left in mm
                CustomSwitches = "--footer-center \"Page [page] of [topage]\" --footer-font-size 10 --footer-spacing 5"
            };
        }
    }
}
