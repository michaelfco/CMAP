using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.DTOs
{
    public class ApiSettingsDto
    {
        public Guid ConfigId { get; set; }
        public string BaseUrl { get; set; }
        public string SecretID { get; set; }
        public string SecretKey { get; set; }
        public string Environment { get; set; }
    }
}
