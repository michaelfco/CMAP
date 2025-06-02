using CMAPTask.Domain.Entities.OB;

namespace CMAPTask.web.ViewModel
{
    public class AccountTransactionsViewModel
    {
        public string AccountId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public TransactionResponse Transactions { get; set; } = new TransactionResponse();
        public DateTime LastUpdated { get; set; }
        public DateTime? CreatedAt { get; set; }
        public RiskSummary RiskSummary { get; set; } = new RiskSummary();
        public List<Transaction> HighRiskTransactions { get; set; } = new List<Transaction>();

        public Guid EndUserId { get; set; }
        public Guid UserId { get; set; }
    }
}
