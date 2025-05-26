using CMAPTask.Application.Interfaces;
using CMAPTask.Application.UseCases;
using CMAPTask.Domain.Interfaces;
using CMAPTask.Infrastructure.Context;
using CMAPTask.Infrastructure.Repository;
using CMAPTask.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMAPTask.Infrastructure.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        // Primary: SQLite
        services.AddDbContext<CMAPDbContext>(options =>
       options.UseSqlite("Data Source=CmapTask.db"));

       // Secondary: SQL Server
       services.AddDbContext<OBDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("SqlServer"));
        });

        // DI
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<ITimesheetService, TimesheetService>();

        //OB
        services.AddScoped<IOpenBankingService, OpenBankingService>();
        services.AddScoped<OBTokenService>();
        services.AddScoped<IRiskAnalyzer, RiskAnalyzer>();
        return services;


        //DP - Dependency Injection
        //        services.AddDbContext<CMAPDbContext>(opt =>
        //        {
        //            //opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        //        });

        //        services.AddDbContext<CMAPDbContext>(options =>
        //options.UseSqlite("Data Source=CmapTask.db"));

        //        //DP - Register service and repository Dependency Injection
        //        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        //        services.AddScoped<ITimesheetService,TimesheetService>();

        //        return services;
    }
}
