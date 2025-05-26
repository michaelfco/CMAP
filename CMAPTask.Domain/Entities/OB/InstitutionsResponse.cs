using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities.OB
{
    public class InstitutionsResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("bic")]
        public string Bic { get; set; } = null!;

        [JsonPropertyName("transaction_total_days")]
        public string TransactionTotalDays { get; set; } = null!;

        [JsonPropertyName("countries")]
        public List<string> Countries { get; set; } = new();

        [JsonPropertyName("logo")]
        public string Logo { get; set; } = null!;

        [JsonPropertyName("max_access_valid_for_days")]
        public string MaxAccessValidForDays { get; set; } = null!;
    }
    
}
