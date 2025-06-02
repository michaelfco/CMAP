using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.DTOs
{
    public class CreditDto
    {
        public Guid CreditId { get; set; }
        public Guid UserId { get; set; }
        public int? Quantity { get; set; }
        public int ActiveCredit { get; set; }
        public int PendingCredit { get; set; }
    }
}
