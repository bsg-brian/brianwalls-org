using BW.Website.Application;
using BW.Website.Application.Common.Configuration;
using BW.Website.Infrastructure;

namespace BW.Website.WebUI.WebInfrastructure.CompositionRoot
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebUiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Strongly-typed options
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<FeatureFlags>(configuration.GetSection("FeatureFlags"));

            // Application & Infrastructure layers
            services.AddApplication();
            services.AddInfrastructure(configuration);

            // MVC / Razor
            services.AddControllersWithViews();

            // Authentication / Authorization can be wired here as you build that out
            // services.AddAuthentication(...);
            // services.AddAuthorization(...);

            return services;
        }
    }
}
