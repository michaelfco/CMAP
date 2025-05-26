using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMAPTask.Application.UseCases
{
    public class RiskAnalyzer : IRiskAnalyzer
    {
        private readonly HashSet<string> HighRiskMerchants = new() { "Freshto Ideal", "Estacion de Autoservicio CIRISA", "Dortshine Dentists" };
        private readonly HashSet<string> GamblingKeywords = new() { "Bet", "Casino", "Runebor" };
        private readonly HashSet<string> RentKeywords = new() { "Rent", "Victoria Embankment", "Liam Brown" };
        private readonly HashSet<string> BenefitsKeywords = new() { "Benefit", "Government", "Welfare" };

        public (RiskSummary, List<Transaction>) AnalyzeTransactions(List<Transaction> transactions)
        {
            var summary = new RiskSummary();
            var highRiskTransactions = new List<Transaction>();
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
            summary.InsolvencyRisk = CheckInsolvencyRisk(summary);
            summary.AffordabilityAssessment = EstimateAffordability(summary);

            return (summary, highRiskTransactions);
        }

        public string CheckInsolvencyRisk(RiskSummary summary)
        {
            if (summary.TotalOutflows > summary.TotalInflows)
                return "⚠️ Spending exceeds income — risk of insolvency.";

            if (summary.NetBalance < 0)
                return "⚠️ Negative net balance — customer may be overdrawn.";

            if (summary.TotalRent > summary.TotalInflows * 0.5m)
                return "⚠️ Rent expenses exceed 50% of income — unsustainable housing cost.";

            if (summary.RiskLevel == "High")
                return "⚠️ High-risk behavior detected — review recommended.";

            return "✅ No strong signs of insolvency.";
        }

        public string EstimateAffordability(RiskSummary summary)
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
