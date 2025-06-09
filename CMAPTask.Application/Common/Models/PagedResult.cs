using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.Common.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public PagedResult(List<T> items, int count, int page, int pageSize, string searchQuery, DateTime? fromDate = null, DateTime? toDate = null)
        {
            Items = items;
            Page = page;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            SearchQuery = searchQuery;
            FromDate = fromDate;
            ToDate = toDate;
        }
    }
}
