using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class AccountDetails
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime LastAccessed { get; set; }
        public string Iban { get; set; } = string.Empty;
        public string InstitutionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string? Bban { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty; // Populated from GET /accounts/{accountId}/balances/
    }
}
