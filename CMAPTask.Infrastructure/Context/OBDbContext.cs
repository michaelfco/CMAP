using CMAPTask.Domain.Entities.OB;
using Microsoft.EntityFrameworkCore;
using OpenBanking.Domain.Entities.OB;
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
        public DbSet<User> Users { get; set; }
        public DbSet<CompanyEndUser> CompanyEndUsers { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<GoCardlessSetting> GoCardlessSettings { get; set; }
        public DbSet<RepositoryStorage> RepositoryStorages { get; set; }
        public DbSet<CreditUsage> CreditUsages { get; set; }
        public DbSet<OpenBanking.Domain.Entities.OB.Transaction> Transactions { get; set; }

        // Optional: Fluent API configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
