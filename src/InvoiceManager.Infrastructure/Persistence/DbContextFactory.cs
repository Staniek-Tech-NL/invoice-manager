using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceManager.Infrastructure.Persistence;

public sealed class DbContextFactory : IDesignTimeDbContextFactory<InvoiceManagerDbContext>
{
    public InvoiceManagerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InvoiceManagerDbContext>()
            .UseSqlite("Data Source=invoice-manager.design.db")
            .Options;

        return new InvoiceManagerDbContext(options);
    }
}
