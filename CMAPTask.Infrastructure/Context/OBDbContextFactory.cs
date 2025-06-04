using CMAPTask.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Configuration.Json;



namespace OpenBanking.Infrastructure.Context
{
    public class OBDbContextFactory : IDesignTimeDbContextFactory<OBDbContext>
    {
        public OBDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../CMAPTask.Web");


            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .Build();

            var connectionString = configuration.GetConnectionString("SqlServer");

            var optionsBuilder = new DbContextOptionsBuilder<OBDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OBDbContext(optionsBuilder.Options);
        }
    }
}
