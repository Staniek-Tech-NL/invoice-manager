using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
