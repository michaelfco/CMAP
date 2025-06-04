namespace OpenBanking.web.ViewModel
{
    public class TransactionAmount
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
    }

    public class Transaction123
    {
        public string TransactionId { get; set; }
        public string BookingDateString { get; set; }
        public TransactionAmount TransactionAmount { get; set; } // Reference to TransactionAmount class
        public string CreditorName { get; set; }
        public string DebtorName { get; set; }
        public string RemittanceInformationUnstructured { get; set; }
        public string ProprietaryBankTransactionCode { get; set; }
    }

    public class RiskSummary
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetBalance { get; set; }
        public string RiskLevel { get; set; }
        public int HighValueTransactionCount { get; set; }
        public decimal TotalRent { get; set; }
        public decimal TotalGambling { get; set; }
        public decimal TotalBenefits { get; set; }
        public decimal TotalHighRiskMerchant { get; set; }
        public string AffordabilityAssessment { get; set; }
        public string InsolvencyRisk { get; set; }
        public IEnumerable<string> RiskAlerts { get; set; }
    }

    public class RiskAssessmentModel
    {
        public decimal RiskScorePercentage { get; set; }
        public IEnumerable<string> RiskFactors { get; set; }
        public IEnumerable<string> Recommendations { get; set; }
    }
}