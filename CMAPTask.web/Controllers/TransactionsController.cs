using CMAPTask.web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using OpenBanking.web.ViewModel;
using System.Text.Json;

namespace CMAPTask.web.Controllers
{
    public class TransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DisplayTransactions()
        {
            if (TempData["TransactionsViewModel"] is string viewModelJson)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var viewModel = JsonSerializer.Deserialize<AccountTransactionsViewModel>(viewModelJson, options);
                if (viewModel == null)
                {
                    Console.WriteLine("[DEBUG] Failed to deserialize TransactionsViewModel from TempData.");
                    return StatusCode(500, "Failed to load transaction data.");
                }

                Console.WriteLine($"[DEBUG] Rendering Transactions view for account {viewModel.AccountId} (Currency: {viewModel.Currency})");
                Console.WriteLine($"[DEBUG] Risk Summary: Level={viewModel.RiskSummary.RiskLevel}, Inflows={viewModel.RiskSummary.TotalInflows}, Outflows={viewModel.RiskSummary.TotalOutflows}, Net={viewModel.RiskSummary.NetBalance}");
                return View("Transactions", viewModel);
            }

            Console.WriteLine("[DEBUG] No TransactionsViewModel found in TempData.");
            return BadRequest("No transaction data available.");
        }
    }
}
