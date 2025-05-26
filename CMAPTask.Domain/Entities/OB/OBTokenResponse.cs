using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class OBTokenResponse
    {
        public string Access { get; set; } = null!;
        public int AccessExpires { get; set; }
        public string Refresh { get; set; } = null!;
        public int RefreshExpires { get; set; }
    }
}
