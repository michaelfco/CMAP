using CMAPTask.Domain.Entities.OB;
using OpenBanking.web.ViewModel;
using System;
using System.Collections.Generic;

namespace OpenBanking.web.ViewModel
{
    public class AccountTransactionsAIViewModel
    {
        public string AccountId { get; set; }
        public string Currency { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public RiskSummary RiskSummary { get; set; }
        public TransactionResponseAI Transactions { get; set; }
        public IEnumerable<TransactionAI> HighRiskTransactions { get; set; }
        public Guid EndUserId { get; set; }
        public Guid UserId { get; set; }
        public RiskAssessmentModel RiskAssessment { get; set; }
    }
}