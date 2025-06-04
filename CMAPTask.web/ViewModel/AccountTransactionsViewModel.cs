using CMAPTask.Domain.Entities.OB;

namespace OpenBanking.web.ViewModel
{
    public class AccountTransactionsViewModel
    {
        public string AccountId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public TransactionResponse Transactions { get; set; } = new TransactionResponse();
        public DateTime LastUpdated { get; set; }
        public DateTime? CreatedAt { get; set; }
        public CMAPTask.Domain.Entities.OB.RiskSummary RiskSummary { get; set; } = new CMAPTask.Domain.Entities.OB.RiskSummary();
        public List<Transaction> HighRiskTransactions { get; set; } = new List<Transaction>();

        public Guid EndUserId { get; set; }
        public Guid UserId { get; set; }
    }
}
