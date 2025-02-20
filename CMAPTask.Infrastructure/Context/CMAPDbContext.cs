using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Context
{
    public class CMAPDbContext: DbContext
    {
        public CMAPDbContext(DbContextOptions options) : base(options) { }
    }
}
