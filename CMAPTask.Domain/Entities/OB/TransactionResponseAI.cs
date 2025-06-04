using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class TransactionResponseAI
    {
        public TransactionsAI Transactions { get; set; } = new TransactionsAI();
    }

    public class TransactionsAI
    {
        public List<TransactionAI> Booked { get; set; } = new List<TransactionAI>();
        public List<TransactionAI> Pending { get; set; } = new List<TransactionAI>();
    }


}
