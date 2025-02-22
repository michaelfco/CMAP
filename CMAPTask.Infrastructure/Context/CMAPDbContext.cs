using CMAPTask.Domain.Entities;
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
        public DbSet<Timesheet> Timesheet { get; set; }
        public CMAPDbContext(DbContextOptions options) : base(options) { }

        public CMAPDbContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Timesheet>()
                .HasIndex(e => new { e.UserName, e.Date });
        }
    }
}
