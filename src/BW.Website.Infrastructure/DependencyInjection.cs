using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BW.Website.Application.Interfaces.Repositories;
using BW.Website.Infrastructure.Data;
using BW.Website.Infrastructure.Data.Contexts;
//using BW.Website.Infrastructure.Mapping;
using BW.Website.Infrastructure.Repositories;

namespace BW.Website.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
			//var assembly = typeof(InfrastructureMappingProfile).Assembly;

			// DbContext
			var connectionString = configuration.GetConnectionString("DefaultConnection")
							   ?? "Data Source=sampleapp.db";

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlite(connectionString));
				//options.UseSqlServer(connectionString));


			// Unit of Work
			services.AddScoped<IUnitOfWork, EfUnitOfWork>();

			// Repositories
			services.AddScoped<ISampleWorkOrderRepository, EfSampleWorkOrderRepository>();

			// Add more repositories as needed

			// AutoMapper profile scanning
			//services.AddAutoMapper(cfg => { }, assembly);

			// External services (email, queues, etc.)
			// services.AddTransient<IEmailSender, SmtpEmailSender>();
			// or services.AddScoped<IEmailSender, SmtpEmailSender>();

			return services;
        }
    }
}
