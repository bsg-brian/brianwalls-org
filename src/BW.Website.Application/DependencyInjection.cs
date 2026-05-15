using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using BW.Website.Application.Common.Behaviors;
using BW.Website.Application.Mapping;

namespace BW.Website.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			var assembly = typeof(ApplicationMappingProfile).Assembly;

			// Mediator (Commands/Queries/Handlers) - source generator auto-discovers handlers
			// Use Scoped lifetime since handlers depend on scoped services (repositories, DbContext)
			services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

			// FluentValidation (validators in the same assembly)
			services.AddValidatorsFromAssembly(assembly);

			// Pipeline behaviors (cross-cutting concerns)
			// Scoped to match handler lifetime and allow scoped validator dependencies
			services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			// services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
			// services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

			// AutoMapper profile scanning
			services.AddAutoMapper(cfg => { }, assembly);

			return services;
		}
	}
}
