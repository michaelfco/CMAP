using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class BankTransaction
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public Guid BankAccountId { get; set; }
       
    }
}
