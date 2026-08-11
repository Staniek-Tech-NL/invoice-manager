using InvoiceManager.Application.Customers;
using InvoiceManager.Application.Invoices;
using InvoiceManager.Application.Payments;
using InvoiceManager.Application.Products;
using InvoiceManager.Application.Quotes;
using InvoiceManager.Application.Settings;
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

        services.AddTransient<GetCompanySettings>();
        services.AddTransient<SaveCompanySettings>();

        services.AddTransient<CreateQuote>();
        services.AddTransient<UpdateQuote>();
        services.AddTransient<SearchQuotes>();
        services.AddTransient<ChangeQuoteStatus>();

        services.AddTransient<CreateInvoice>();
        services.AddTransient<UpdateInvoice>();
        services.AddTransient<SearchInvoices>();
        services.AddTransient<ChangeInvoiceStatus>();
        services.AddTransient<ConvertQuoteToInvoice>();
        services.AddTransient<RegisterPayment>();
        services.AddTransient<VoidPayment>();

        return services;
    }
}
