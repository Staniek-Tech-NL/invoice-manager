using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateCustomer>();
        services.AddTransient<UpdateCustomer>();
        services.AddTransient<ArchiveCustomer>();
        services.AddTransient<SearchCustomers>();

        services.AddTransient<CreateProductService>();
        services.AddTransient<UpdateProductService>();
        services.AddTransient<DeactivateProductService>();
        services.AddTransient<SearchProductServices>();

        return services;
    }
}
