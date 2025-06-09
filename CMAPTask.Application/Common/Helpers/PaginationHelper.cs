using OpenBanking.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBanking.Application.Common.Helpers
{
    public static class PaginationHelper
    {
        public static PagedResult<T> GetPaged<T>(
            IEnumerable<T> query,
            int page,
            int pageSize,
            string searchQuery = null,
            Func<T, bool> searchFilter = null)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery) && searchFilter != null)
            {
                query = query.Where(searchFilter);
            }

            var count = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>(items, count, page, pageSize, searchQuery);
        }
    }
}
