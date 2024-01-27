using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Services;

namespace CleanArchitecture.Core;
public static class DependencyInjection
{
  public static IServiceCollection AddCore(this IServiceCollection services)
  {
    services
      .AddScoped<IDeleteContributorService, DeleteContributorService>();

    return services;
  }
}
