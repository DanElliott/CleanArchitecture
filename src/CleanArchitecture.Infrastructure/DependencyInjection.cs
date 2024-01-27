using System.Reflection;
using Ardalis.SharedKernel;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.Core.ContributorAggregate;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Data.Queries;
using CleanArchitecture.Infrastructure.Email;
using CleanArchitecture.UseCases.Contributors.Create;
using CleanArchitecture.UseCases.Contributors.List;

namespace CleanArchitecture.Infrastructure;
public static class DependencyInjection
{
  private static bool _isDevelopment;
  private static readonly List<Assembly> _assemblies = [];

  public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool isDevelopment, Assembly? callingAssembly = null)
  {
    _isDevelopment = isDevelopment;
    AddToAssembliesIfNotNull(callingAssembly);

    LoadAssemblies();

    if (isDevelopment)
    {
      services.RegisterDevelopmentOnlyDependencies();
    }
    else
    {
      services.RegisterProductionOnlyDependencies();
    }

    services
      .RegisterEF()
      .RegisterQueries()
      .RegisterMediatR();

    return services;
  }

  private static void AddToAssembliesIfNotNull(Assembly? assembly)
  {
    if (assembly != null)
    {
      _assemblies.Add(assembly);
    }
  }

  private static void LoadAssemblies()
  {
    // TODO: Replace these types with any type in the appropriate assembly/project
    var coreAssembly = Assembly.GetAssembly(typeof(Contributor));
    var infrastructureAssembly = Assembly.GetAssembly(typeof(DependencyInjection));
    var useCasesAssembly = Assembly.GetAssembly(typeof(CreateContributorCommand));

    AddToAssembliesIfNotNull(coreAssembly);
    AddToAssembliesIfNotNull(infrastructureAssembly);
    AddToAssembliesIfNotNull(useCasesAssembly);
  }

  private static IServiceCollection RegisterEF(this IServiceCollection services)
  {
    services
      .AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

    services
      .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

    return services;
  }

  private static IServiceCollection RegisterQueries(this IServiceCollection services)
  {
    services
      .AddScoped<IListContributorsQueryService, ListContributorsQueryService>();

    return services;
  }

  private static IServiceCollection RegisterMediatR(this IServiceCollection services)
  {
    services
      .AddMediatR(config =>
      {
        config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
      });

    services.AddTransient<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    var mediatrOpenTypes = new[]
    {
      typeof(IRequestHandler<,>),
      typeof(IRequestExceptionHandler<,,>),
      typeof(IRequestExceptionAction<,>),
      typeof(INotificationHandler<>),
    };

    foreach (var mediatrOpenType in mediatrOpenTypes)
    {
      // register all the open types (concrete types) that implement the interfaces above using Scrutor
      services
        .Scan(selector => selector
        .FromAssemblies(_assemblies)
        .AddClasses(classes => classes.AssignableTo(mediatrOpenType))
        .AsImplementedInterfaces()
        .WithScopedLifetime());
    }

    return services;
  }

  private static IServiceCollection RegisterDevelopmentOnlyDependencies(this IServiceCollection services)
  {
    // NOTE: Add any development only services here
    services
      .AddScoped<IEmailSender, FakeEmailSender>();

    //services
    //  .AddScoped<IListContributorsQueryService, FakeListContributorsQueryService>();

    return services;
  }

  private static IServiceCollection RegisterProductionOnlyDependencies(this IServiceCollection services)
  {
    // NOTE: Add any production only (real) services here
    services
      .AddScoped<IEmailSender, FakeEmailSender>();

    return services;
  }
}
