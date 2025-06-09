using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class RiskSummary
    {
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetBalance { get; set; }
        public int HighValueTransactionCount { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public List<string> RiskAlerts { get; set; } = new List<string>();

        // Add these:
        public decimal TotalRent { get; set; }
        public decimal TotalGambling { get; set; }
        public decimal TotalBenefits { get; set; }
        public decimal TotalHighRiskMerchant { get; set; }

        public string AffordabilityAssessment { get; set; } = "Unknown";
        public int AffordabilityScore { get; set; }
        public string InsolvencyRisk { get; set; } = "Unknown";

        public decimal DebtToIncomeRatio { get; set; } 
        public decimal DisposableIncome { get; set; }

        public int HighRiskTransactionCount { get; set; } 
       
    }
}
