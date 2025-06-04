using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Domain.Enums
{
    public class Enum
    {
        public enum UserRole
        {
            Admin = 1,
            Company = 2,
            Seller = 3
        }  
        
        public enum Status
        {
            pending = 1,
            Complete = 2,
            Other = 3,
            deleted = 4
        }
    }
}
