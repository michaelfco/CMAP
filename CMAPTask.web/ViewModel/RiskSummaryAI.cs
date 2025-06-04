namespace OpenBanking.web.ViewModel
{
    public class RiskSummaryAI
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
}
