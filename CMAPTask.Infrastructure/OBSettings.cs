using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure
{
    public class OBSettings
    {
        public string SecretID { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string TokenURL { get; set; } = null!;
        public string BaseURL { get; set; } = null!;
    }
}
