using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMAPTask.Application.UseCases
{
    public class RiskAnalyzerAI : IRiskAnalyzerAI
    {

        private readonly HashSet<string> HighRiskMerchants = new()
            {
                "Estacion de Autoservicio CIRISA",
                "Wonga", "QuickQuid", "Sunny Loans", "CashNetUSA", "Amigo Loans",
                "Lending Stream", "Payday UK", "Cash Advance", "Short Term Loan",
                "Loan Shop", "Money Shop", "Bitconnect", "OneCoin", "Crypto Exchange",
                "Forex Trading", "Binary Options", "Investment Club", "Luxury Outlet",
                "Pawn Shop", "Cash for Gold", "Klarna", "Afterpay", "Laybuy",
                "Ticket Scalper", "Online Auction", "Marketplace Seller", "Resale Platform",
                "Fuel Station", "Convenience Store", "Quick Mart", "Petrol Express",
                "Gas Cash"
            };
        private readonly HashSet<string> GamblingKeywords = new()
            {
                "Bet", "Casino", "Gamble", "Wager", "Stake", "Betting", "Bookmaker", "Bookie",
                "Odds", "Punt", "Gaming", "Lotto", "Lottery", "Bingo", "Poker", "Roulette",
                "Blackjack", "Slots", "Jackpot", "Bet365", "William Hill", "Paddy Power",
                "Ladbrokes", "Coral", "Betfair", "SkyBet", "Betway", "888", "Unibet",
                "Bwin", "Sportingbet", "Tabcorp", "Crown", "Star City", "Sports Betting",
                "Horse Racing", "Greyhound", "Football Bet", "Race Bet", "EGM", "Pokies"
            };
        private readonly HashSet<string> RentKeywords = new()
            {
                "Rent", "Rental", "Lease", "Tenancy", "Apartment", "Flat", "House Payment",
                "Landlord", "Property Management", "Housing", "Letting", "Tenancy Agreement",
                "Foxtons", "Knight Frank", "Savills", "Hammonds", "Lettings", "Estate Agent",
                "Real Estate", "Property Group", "Housing Association", "Embankment",
                "Residences", "Court", "Place"
            };
        private readonly HashSet<string> BenefitsKeywords = new()
            {
                "Benefit", "Government", "Welfare", "Pension", "Allowance", "Subsidy",
                "Grant", "Social Security", "Unemployment", "Disability", "Child Benefit",
                "Tax Credit", "Income Support", "Assistance", "DWP", "HMRC", "Centrelink",
                "HHS", "Social Services", "Treasury", "JobSeeker", "Universal Credit",
                "PIP", "ESA", "JSA", "SNAP", "Medicaid", "Housing Benefit", "Carer’s Allowance"
            };

        public (RiskSummaryAI, List<TransactionAI>) AnalyzeTransactionsAI(List<TransactionAI> transactions, bool? printLayout = false)
        {
            var summary = new RiskSummaryAI();
            var highRiskTransactions = new List<TransactionAI>();
            var alerts = new List<string>();

            decimal totalGambling = 0m, totalRent = 0m, totalBenefits = 0m, totalHighRiskMerchant = 0m;
            int gamblingCount = 0, rentCount = 0, benefitsCount = 0, highRiskMerchantCount = 0;

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionAmount == null ||
                    !decimal.TryParse(transaction.TransactionAmount.Amount, out var amount))
                    continue;

                var type = (transaction.ProprietaryBankTransactionCode ?? "");
                string description = transaction.RemittanceInformationUnstructured ?? "";
                string creditor = transaction.CreditorName ?? "";

                bool isHighRiskMerchant = HighRiskMerchants.Any(hrm =>
                    creditor.IndexOf(hrm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(hrm, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isGambling = GamblingKeywords.Any(kw =>
                    creditor.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isRent = RentKeywords.Any(kw =>
                    creditor.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isBenefits = BenefitsKeywords.Any(kw =>
                    creditor.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isSalary = string.Equals(type, "salary", StringComparison.OrdinalIgnoreCase);

                if (amount > 0)
                    summary.TotalInflows += amount;
                else
                    summary.TotalOutflows += Math.Abs(amount);

                if (isHighRiskMerchant && !isSalary && !isRent && !isBenefits)
                {
                    totalHighRiskMerchant += Math.Abs(amount);
                    highRiskMerchantCount++;
                }

                if (isGambling)
                {
                    totalGambling += Math.Abs(amount);
                    gamblingCount++;
                }

                if (isRent)
                {
                    totalRent += Math.Abs(amount);
                    rentCount++;
                }

                if (isBenefits)
                {
                    totalBenefits += amount; // benefits usually inflows
                    benefitsCount++;
                }

                if (Math.Abs(amount) > 20 && !isSalary)
                {
                    highRiskTransactions.Add(transaction);
                    summary.HighValueTransactionCount++;
                }
            }

            summary.NetBalance = summary.TotalInflows - summary.TotalOutflows;

            if (totalHighRiskMerchant > 0)
                alerts.Add($"{highRiskMerchantCount} transactions with high-risk merchants totaling £{totalHighRiskMerchant:F2}.");

            if (totalGambling > 0)
                alerts.Add($"{gamblingCount} gambling-related transactions totaling £{totalGambling:F2}.");

            if (totalRent > 0)
                alerts.Add($"{rentCount} rent-related transactions totaling £{totalRent:F2}.");

            if (totalBenefits > 0)
                alerts.Add($"{benefitsCount} benefit payments totaling £{totalBenefits:F2}.");

            summary.RiskLevel = alerts.Count switch
            {
                0 => "Low",
                1 => "Moderate",
                _ => "High"
            };

            summary.RiskAlerts = alerts;

            summary.TotalRent = totalRent;
            summary.TotalBenefits = totalBenefits;
            summary.TotalGambling = totalGambling;
            summary.TotalHighRiskMerchant = totalHighRiskMerchant;

            // Set insolvency and affordability assessments on summary
            summary.InsolvencyRisk = CheckInsolvencyRisk(summary, printLayout);
            summary.AffordabilityAssessment = EstimateAffordability(summary);

            return (summary, highRiskTransactions);
        }

        public string CheckInsolvencyRisk(RiskSummaryAI summary, bool? printLayout = false)
        {
            var sign = "⚠️ ";
            if (printLayout == true)
                sign = "";
            
            if (summary.TotalOutflows > summary.TotalInflows)
                return $"{sign} Spending exceeds income — risk of insolvency.";

            if (summary.NetBalance < 0)
                return $"{sign} Negative net balance — customer may be overdrawn.";

            if (summary.TotalRent > summary.TotalInflows * 0.5m)
                return $"{sign} Rent expenses exceed 50% of income — unsustainable housing cost.";

            if (summary.RiskLevel == "High")
                return $"{sign} High-risk behavior detected — review recommended.";

            return "✅ No strong signs of insolvency.";
        }

        public string EstimateAffordability(RiskSummaryAI summary)
        {
            if (summary.NetBalance <= 0)
                return "❌ Customer is not in a position to afford additional commitments (negative or zero balance).";

            if (summary.TotalOutflows > summary.TotalInflows * 0.9m)
                return "⚠️ Most of the income is being spent — low affordability margin.";

            if (summary.TotalRent > summary.TotalInflows * 0.4m)
                return "⚠️ Rent takes a significant portion of income — affordability is constrained.";

            return "✅ Customer appears to have room for additional financial commitments.";
        }
    }
}
