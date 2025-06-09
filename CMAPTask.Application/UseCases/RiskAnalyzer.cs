using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities.OB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CMAPTask.Application.UseCases
{
    public class RiskAnalyzer : IRiskAnalyzer
    {
        private readonly HashSet<string> HighRiskMerchants = new()
    {
        "Wonga", "QuickQuid", "Sunny Loans", "CashNetUSA", "Amigo Loans",
        "Lending Stream", "Payday UK", "Cash Advance", "Short Term Loan",
        "Loan Shop", "Money Shop", "Bitconnect", "OneCoin", "Crypto Exchange",
        "Forex Trading", "Binary Options", "Investment Club", "Luxury Outlet",
        "Pawn Shop", "Cash for Gold", "Klarna", "Afterpay", "Laybuy",
        "Ticket Scalper", "Online Auction", "Marketplace Seller", "Resale Platform",
        "Fuel Station", "Convenience Store", "Quick Mart", "Petrol Express",
        "Gas Cash", "Freshto Ideal", "Dortshine Dentists", "Estacion de Autoservicio CIRISA"
    };
        private readonly HashSet<string> GamblingKeywords = new()
    {
        "Bet", "Casino", "Gamble", "Wager", "Stake", "Betting", "Bookmaker", "Bookie",
        "Odds", "Punt", "Gaming", "Lotto", "Lottery", "Bingo", "Poker", "Roulette",
        "Blackjack", "Slots", "Jackpot", "Bet365", "William Hill", "Paddy Power",
        "Ladbrokes", "Coral", "Betfair", "SkyBet", "Betway", "888", "Unibet",
        "Bwin", "Sportingbet", "Tabcorp", "Crown", "Star City", "Sports Betting",
        "Horse Racing", "Greyhound", "Football Bet", "Race Bet", "EGM", "Pokies",
        "Bet Runebor"
    };
        private readonly HashSet<string> RentKeywords = new()
    {
        "Rent", "Rental", "Lease", "Tenancy", "Apartment", "Flat", "House Payment",
        "Landlord", "Property Management", "Housing", "Letting", "Tenancy Agreement",
        "Foxtons", "Knight Frank", "Savills", "Hammonds", "Lettings", "Estate Agent",
        "Real Estate", "Property Group", "Housing Association", "Embankment",
        "Residences", "Court", "Place", "Monthly Rent"
    };
        private readonly HashSet<string> BenefitsKeywords = new()
    {
        "Benefit", "Government", "Welfare", "Pension", "Allowance", "Subsidy",
        "Grant", "Social Security", "Unemployment", "Disability", "Child Benefit",
        "Tax Credit", "Income Support", "Assistance", "DWP", "HMRC", "Centrelink",
        "HHS", "Social Services", "Treasury", "JobSeeker", "Universal Credit",
        "PIP", "ESA", "JSA", "SNAP", "Medicaid", "Housing Benefit", "Carer’s Allowance"
    };

        public (RiskSummary, List<Transaction>) AnalyzeTransactions(List<Transaction> transactions, bool? printLayout = false)
        {
            var summary = new RiskSummary();
            var highRiskTransactions = new List<Transaction>();
            var alerts = new List<string>();

            decimal totalGambling = 0m, totalRent = 0m, totalBenefits = 0m, totalHighRiskMerchant = 0m;
            int gamblingCount = 0, rentCount = 0, benefitsCount = 0, highRiskMerchantCount = 0;

            // Combine booked and pending transactions for analysis
            var allTransactions = transactions;
            var container = new TransactionsContainer { Booked = transactions, Pending = new List<Transaction>() };
            // Note: Assuming transactions parameter includes only booked transactions; pending must be handled separately if provided

            // Precompute recent transactions and average amount for efficiency
            var recentTransactions = allTransactions
                .Where(t => t.BookingDateTime >= DateTime.Now.AddDays(-30) && decimal.TryParse(t.TransactionAmount?.Amount, out _))
                .ToList();
            var averageAmount = recentTransactions.Any()
                ? recentTransactions.Average(t => Math.Abs(decimal.Parse(t.TransactionAmount.Amount)))
                : 0m;
            var unusualThreshold = averageAmount * 2; // 2x average for sensitivity

            foreach (var transaction in allTransactions)
            {
                if (transaction.TransactionAmount == null ||
                    !decimal.TryParse(transaction.TransactionAmount.Amount, out var amount))
                    continue;

                var type = transaction.ProprietaryBankTransactionCode ?? "";
                string description = transaction.RemittanceInformationUnstructured ?? "";
                string creditor = transaction.CreditorName ?? "";

                bool isHighRiskMerchant = HighRiskMerchants.Any(hrm =>
                    creditor?.IndexOf(hrm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description?.IndexOf(hrm, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isGambling = GamblingKeywords.Any(kw =>
                    creditor?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

                bool isRent = RentKeywords.Any(kw =>
                    Regex.IsMatch(creditor ?? "", $@"\b{Regex.Escape(kw)}\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(description ?? "", $@"\b{Regex.Escape(kw)}\b", RegexOptions.IgnoreCase));

                bool isBenefits = BenefitsKeywords.Any(kw =>
                    creditor?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    description?.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);

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
                    totalBenefits += amount; // Benefits usually inflows
                    benefitsCount++;
                }

                // New high-risk transaction logic
                bool isHighValue = Math.Abs(amount) > 500;
                bool isUnusual = Math.Abs(amount) > unusualThreshold && IsUnusualTransaction(transaction, allTransactions);
                bool isFrequentHighValue = allTransactions
                    .Where(t => t.CreditorName == transaction.CreditorName &&
                               t.BookingDateTime >= DateTime.Now.AddDays(-7))
                    .Count(t => decimal.TryParse(t.TransactionAmount?.Amount, out var amt) && Math.Abs(amt) > 500) > 3;
                bool isForeign = creditor?.Contains("CIRISA") == true ||
                                description?.Contains("Spain") == true ||
                                description?.Contains("Alles Gute") == true;
                bool isPending = container.Pending.Contains(transaction);

                if ((isHighValue || isUnusual || isHighRiskMerchant || isGambling || isFrequentHighValue || isForeign) &&
                    !isSalary && !isRent && !isBenefits && !isPending)
                {
                    highRiskTransactions.Add(transaction);
                    summary.HighValueTransactionCount++;
                    //alerts.Add($"High-risk transaction detected: £{Math.Abs(amount):F2} to {creditor} " +
                    //          $"(HighValue: {isHighValue}, Unusual: {isUnusual}, HighRiskMerchant: {isHighRiskMerchant}, " +
                    //          $"Gambling: {isGambling}, FrequentHighValue: {isFrequentHighValue}, Foreign: {isForeign})");
                }
                else if (isPending && Math.Abs(amount) > 50)
                {
                    highRiskTransactions.Add(transaction);
                    summary.HighValueTransactionCount++;
                    //alerts.Add($"Pending high-risk transaction detected: £{Math.Abs(amount):F2} to {creditor}");
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

            // Calculate Debt-to-Income Ratio and Disposable Income
            decimal monthlyIncome = summary.TotalInflows / 3;
            decimal monthlyDebtPayments = totalRent / 3;
            summary.DebtToIncomeRatio = monthlyIncome > 0 ? (monthlyDebtPayments / monthlyIncome) * 100 : 0;
            summary.DisposableIncome = monthlyIncome - summary.TotalOutflows / 3;

            if (summary.DebtToIncomeRatio > 40)
                alerts.Add($"High Debt-to-Income Ratio: {summary.DebtToIncomeRatio:F2}% exceeds 40%.");
            else if (summary.DebtToIncomeRatio > 36)
                alerts.Add($"Moderate Debt-to-Income Ratio: {summary.DebtToIncomeRatio:F2}% is between 36% and 40%.");

            if (summary.DisposableIncome < 0)
                alerts.Add($"Negative Disposable Income: £{summary.DisposableIncome:F2} indicates insufficient funds after expenses.");

            summary.InsolvencyRisk = CheckInsolvencyRisk(summary, printLayout);
            summary.AffordabilityAssessment = EstimateAffordability(summary);

            return (summary, highRiskTransactions);
        }

        private bool IsUnusualTransaction(Transaction transaction, List<Transaction> transactions)
        {
            if (transaction.TransactionAmount == null ||
                !decimal.TryParse(transaction.TransactionAmount.Amount, out var amount))
                return false;

            var recentTransactions = transactions
                .Where(t => t.BookingDateTime >= DateTime.Now.AddDays(-30))
                .Where(t => decimal.TryParse(t.TransactionAmount?.Amount, out _))
                .ToList();

            if (!recentTransactions.Any()) return false;

            var averageAmount = recentTransactions
                .Average(t => Math.Abs(decimal.Parse(t.TransactionAmount.Amount)));
            var threshold = averageAmount * 2; // Adjusted to 2x for sensitivity

            return Math.Abs(amount) > threshold;
        }

        public string CheckInsolvencyRisk(RiskSummary summary, bool? printLayout = false)
        {
            var sign = printLayout == true ? "" : "⚠️ ";

            if (summary.TotalOutflows > summary.TotalInflows)
                return $"{sign}Spending exceeds income — risk of insolvency.";

            if (summary.NetBalance < 0)
                return $"{sign}Negative net balance — customer may be overdrawn.";

            if (summary.TotalRent > summary.TotalInflows * 0.5m)
                return $"{sign}Rent expenses exceed 50% of income — unsustainable housing cost.";

            if (summary.DebtToIncomeRatio > 40)
                return $"{sign}High Debt-to-Income Ratio ({summary.DebtToIncomeRatio:F2}%) — increased insolvency risk.";

            if (summary.DisposableIncome < 0)
                return $"{sign}Negative Disposable Income (£{summary.DisposableIncome:F2}) — high insolvency risk.";

            if (summary.RiskLevel == "High")
                return $"{sign}High-risk behavior detected — review recommended.";

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

            if (summary.DebtToIncomeRatio > 40)
                return "⚠️ High Debt-to-Income Ratio — limited affordability for new commitments.";

            if (summary.DisposableIncome < 100)
                return "⚠️ Low Disposable Income (£{summary.DisposableIncome:F2}) — constrained affordability.";

            return "✅ Customer appears to have room for additional financial commitments.";
        }
    }
}
