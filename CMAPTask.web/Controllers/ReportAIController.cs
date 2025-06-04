using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using CMAPTask.Infrastructure;
using CMAPTask.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenBanking.Application.Interfaces;
using OpenBanking.Infrastructure.Services;
using OpenBanking.web.ViewModel;
using System.Text.Json;

using DomainRiskSummary = CMAPTask.Domain.Entities.OB.RiskSummaryAI;
using ViewModelRiskSummary = OpenBanking.web.ViewModel.RiskSummary;

namespace OpenBanking.web.Controllers
{
    public class ReportAIController : Controller
    {
        private readonly IRiskAnalyzerAI _riskAnalyzer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly OBSettings _settings;

        public ReportAIController(IRiskAnalyzerAI riskAnalyzer, IHttpContextAccessor httpContextAccessor, IOptions<OBSettings> options, ITransactionsRepository transactionsRepository)
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

            var domainTransactions = JsonSerializer.Deserialize<TransactionResponseAI>(transaction.JsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Map domain TransactionResponse to view model structure
            var viewTransactions = new TransactionResponseAI
            {
                Transactions = new TransactionsAI
                {
                    Booked = domainTransactions.Transactions.Booked.Select(t => new TransactionAI
                    {
                        TransactionId = t.TransactionId,
                        BookingDateString = t.BookingDateString,
                        TransactionAmount = new TransactionAmountAI
                        {
                            Amount = t.TransactionAmount?.Amount?.ToString(),
                            Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                        },
                        CreditorName = t.CreditorName,
                        DebtorName = t.DebtorName,
                        RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                        ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
                    }).ToList(),
                    Pending = domainTransactions.Transactions.Pending.Select(t => new TransactionAI
                    {
                        TransactionId = t.TransactionId,
                        BookingDateString = t.BookingDateString,
                        TransactionAmount = new TransactionAmountAI
                        {
                            Amount = t.TransactionAmount?.Amount?.ToString(),
                            Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                        },
                        CreditorName = t.CreditorName,
                        DebtorName = t.DebtorName,
                        RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                        ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
                    }).ToList()
                }
            };

            var view = new AccountTransactionsAIViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = viewTransactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId
            };

            var (domainRiskSummary, domainHighRiskTransactions) = _riskAnalyzer.AnalyzeTransactionsAI(domainTransactions.Transactions.Booked);

            // Map domain RiskSummary to view model RiskSummary
            var castedDomainRiskSummary = (DomainRiskSummary)domainRiskSummary;
            var viewRiskSummary = new ViewModelRiskSummary
            {
                TotalInflows = castedDomainRiskSummary.TotalInflows,
                TotalOutflows = castedDomainRiskSummary.TotalOutflows,
                NetBalance = castedDomainRiskSummary.NetBalance,
                RiskLevel = castedDomainRiskSummary.RiskLevel,
                HighValueTransactionCount = castedDomainRiskSummary.HighValueTransactionCount,
                TotalRent = castedDomainRiskSummary.TotalRent,
                TotalGambling = castedDomainRiskSummary.TotalGambling,
                TotalBenefits = castedDomainRiskSummary.TotalBenefits,
                TotalHighRiskMerchant = castedDomainRiskSummary.TotalHighRiskMerchant,
                AffordabilityAssessment = castedDomainRiskSummary.AffordabilityAssessment,
                InsolvencyRisk = castedDomainRiskSummary.InsolvencyRisk,
                RiskAlerts = castedDomainRiskSummary.RiskAlerts
            };


            // Map domain HighRiskTransactions to view model Transaction
            var viewHighRiskTransactions = domainHighRiskTransactions.Select(t => new TransactionAI
            {
                TransactionId = t.TransactionId,
                BookingDateString = t.BookingDateString,
                TransactionAmount = new TransactionAmountAI
                {
                    Amount = t.TransactionAmount?.Amount?.ToString(),
                    Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                },
                CreditorName = t.CreditorName,
                DebtorName = t.DebtorName,
                RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
            }).ToList();

            view.RiskSummary = viewRiskSummary;
            view.HighRiskTransactions = viewHighRiskTransactions;

            // Placeholder for AI-specific risk assessment
            view.RiskAssessment = new web.ViewModel.RiskAssessmentModel
            {
                RiskScorePercentage = 0.0m, // AI logic would populate this
                RiskFactors = new List<string> { "Placeholder AI Risk Factor" },
                Recommendations = new List<string> { "Placeholder AI Recommendation" }
            };

            // ✅ Now render the view using the AI model
            Console.WriteLine($"[DEBUG] Rendering AI Transactions view for account {view.AccountId} (Currency: {view.Currency})");
            Console.WriteLine($"[DEBUG] AI Risk Summary: Level={view.RiskSummary.RiskLevel}, Inflows={view.RiskSummary.TotalInflows}, Outflows={view.RiskSummary.TotalOutflows}, Net={view.RiskSummary.NetBalance}");

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

            var domainTransactions = JsonSerializer.Deserialize<TransactionResponseAI>(transaction.JsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Map domain TransactionResponse to view model structure
            var viewTransactions = new TransactionResponseAI
            {
                Transactions = new TransactionsAI
                {
                    Booked = domainTransactions.Transactions.Booked.Select(t => new TransactionAI
                    {
                        TransactionId = t.TransactionId,
                        BookingDateString = t.BookingDateString,
                        TransactionAmount = new TransactionAmountAI
                        {
                            Amount = t.TransactionAmount?.Amount?.ToString(),
                            Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                        },
                        CreditorName = t.CreditorName,
                        DebtorName = t.DebtorName,
                        RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                        ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
                    }).ToList(),
                    Pending = domainTransactions.Transactions.Pending.Select(t => new TransactionAI
                    {
                        TransactionId = t.TransactionId,
                        BookingDateString = t.BookingDateString,
                        TransactionAmount = new TransactionAmountAI
                        {
                            Amount = t.TransactionAmount?.Amount?.ToString(),
                            Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                        },
                        CreditorName = t.CreditorName,
                        DebtorName = t.DebtorName,
                        RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                        ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
                    }).ToList()
                }
            };

            var model = new AccountTransactionsAIViewModel
            {
                AccountId = null,
                Currency = transaction.Currency,
                Transactions = viewTransactions,
                LastUpdated = transaction.LastUpdated,
                CreatedAt = transaction.CreatedAt,
                EndUserId = endUserId,
                UserId = userId
            };

            var (domainRiskSummary, domainHighRiskTransactions) = _riskAnalyzer.AnalyzeTransactionsAI(domainTransactions.Transactions.Booked, true);

            // Map domain RiskSummary to view model RiskSummary
            DomainRiskSummary castedDomainRiskSummary = domainRiskSummary;

            // Then map to the ViewModelRiskSummary manually
            var viewRiskSummary = new ViewModelRiskSummary
            {
                TotalInflows = castedDomainRiskSummary.TotalInflows,
                TotalOutflows = castedDomainRiskSummary.TotalOutflows,
                NetBalance = castedDomainRiskSummary.NetBalance,
                RiskLevel = castedDomainRiskSummary.RiskLevel,
                HighValueTransactionCount = castedDomainRiskSummary.HighValueTransactionCount,
                TotalRent = castedDomainRiskSummary.TotalRent,
                TotalGambling = castedDomainRiskSummary.TotalGambling,
                TotalBenefits = castedDomainRiskSummary.TotalBenefits,
                TotalHighRiskMerchant = castedDomainRiskSummary.TotalHighRiskMerchant,
                AffordabilityAssessment = castedDomainRiskSummary.AffordabilityAssessment,
                InsolvencyRisk = castedDomainRiskSummary.InsolvencyRisk,
                RiskAlerts = castedDomainRiskSummary.RiskAlerts
            };

            // Map domain HighRiskTransactions to view model Transaction
            var viewHighRiskTransactions = domainHighRiskTransactions.Select(t => new TransactionAI
            {
                TransactionId = t.TransactionId,
                BookingDateString = t.BookingDateString,
                TransactionAmount = new TransactionAmountAI
                {
                    Amount = t.TransactionAmount?.Amount?.ToString(),
                    Currency = t.TransactionAmount?.Currency ?? transaction.Currency
                },
                CreditorName = t.CreditorName,
                DebtorName = t.DebtorName,
                RemittanceInformationUnstructured = t.RemittanceInformationUnstructured,
                ProprietaryBankTransactionCode = t.ProprietaryBankTransactionCode
            }).ToList();

            model.RiskSummary = viewRiskSummary;
            model.HighRiskTransactions = viewHighRiskTransactions;

            // Placeholder for AI-specific risk assessment
            model.RiskAssessment = new web.ViewModel.RiskAssessmentModel
            {
                RiskScorePercentage = 0.00m, // AI logic would compute this
                RiskFactors = new List<string> { "Placeholder AI Risk Factor" },
                Recommendations = new List<string> { "Placeholder AI Recommendation" }
            };

            return new Rotativa.AspNetCore.ViewAsPdf("_ReportExportPDFAI", model)
            {
                FileName = $"AI_Account_Transactions_{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10),
                CustomSwitches = "--footer-center \"Page [page] of [topage]\" --footer-font-size 10 --footer-spacing 5"
            };
        }
    }
}