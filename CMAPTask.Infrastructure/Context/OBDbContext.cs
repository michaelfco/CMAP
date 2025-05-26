using CMAPTask.Domain.Entities.OB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Context
{
    public class OBDbContext : DbContext
    {
        public OBDbContext(DbContextOptions<OBDbContext> options)
            : base(options) { }

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }

        // Optional: Fluent API configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
