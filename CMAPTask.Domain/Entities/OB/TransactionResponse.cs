using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class TransactionResponse
    {
        public TransactionsContainer Transactions { get; set; } = new TransactionsContainer();
    }

    public class TransactionsContainer
    {
        public List<Transaction> Booked { get; set; } = new List<Transaction>();
        public List<Transaction> Pending { get; set; } = new List<Transaction>();
        public DateTime LastUpdated { get; set; }
    }

    public class Transaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string BookingDate { get; set; } = string.Empty;
        public DateTime BookingDateTime { get; set; }
        public TransactionAmount TransactionAmount { get; set; } = new TransactionAmount();
        public string? CreditorName { get; set; }
        public AccountDetails CreditorAccount { get; set; } = new AccountDetails();
        public string? DebtorName { get; set; }
        public AccountDetails DebtorAccount { get; set; } = new AccountDetails();
        public string RemittanceInformationUnstructured { get; set; } = string.Empty;
        public string AdditionalInformation { get; set; } = string.Empty;
        public string ProprietaryBankTransactionCode { get; set; } = string.Empty;
        public string InternalTransactionId { get; set; } = string.Empty;
    }

    public class TransactionAmount
    {
        public string Amount { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
    }

    //public class AccountDetails
    //{
    //    public string? Bban { get; set; }
    //    public string? Pan { get; set; }
    //}
}
