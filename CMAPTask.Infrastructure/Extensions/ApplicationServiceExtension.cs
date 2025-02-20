using CMAPTask.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {

        services.AddDbContext<CMAPDbContext>(opt =>
        {
            //opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });

        services.AddDbContext<CMAPDbContext>(options =>
options.UseSqlite("Data Source=CmapTask.db"));
       
        //services.AddScoped<IValidationService, ValidationService>();

        return services;
    }
}
