using CMAPTask.Application.Interfaces;
using CMAPTask.Application.UseCases;
using CMAPTask.Infrastructure.Context;
using CMAPTask.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBanking.Application.Interfaces;
using OpenBanking.Domain.Interfaces;
using OpenBanking.Infrastructure.Repository;
using OpenBanking.Infrastructure.Services;

namespace CMAPTask.Infrastructure.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

       
       // Secondary: SQL Server
       services.AddDbContext<OBDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("SqlServer"));
        });

      
        //OB
        services.AddScoped<IOpenBankingService, OpenBankingService>();
        services.AddScoped<OBTokenService>();
        services.AddScoped<IRiskAnalyzer, RiskAnalyzer>();
        services.AddScoped<IDapperGenericRepository, DapperGenericRepository>();
        services.AddScoped<ICompanyEndUserRepository, CompanyEndUserRepository>();
        services.AddScoped<ITransactionsRepository, TransactionsRepository>();
        services.AddScoped<ICreditRepository, CreditRepository>();

        services.AddTransient<EmailService>();

        return services;

    }
}
