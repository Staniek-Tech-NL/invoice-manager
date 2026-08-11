using InvoiceManager.Domain.Customers;

namespace InvoiceManager.Domain.Tests;

public sealed class CustomerTests
{
    [Fact]
    public void CreateNormalizesValuesAndAuditTime()
    {
        var localTime = new DateTimeOffset(2026, 8, 11, 14, 0, 0, TimeSpan.FromHours(2));

        var customer = CreateCustomer(localTime, companyName: "  Example BV  ", email: "  info@example.com ");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Example BV", customer.CompanyName);
        Assert.Equal("info@example.com", customer.Email);
        Assert.Equal(TimeSpan.Zero, customer.CreatedAt.Offset);
        Assert.Equal(customer.CreatedAt, customer.UpdatedAt);
        Assert.False(customer.IsArchived);
    }

    [Fact]
    public void CreateRejectsMissingCompanyName()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateCustomer(DateTimeOffset.UtcNow, companyName: " "));

        Assert.Equal("companyName", exception.ParamName);
    }

    [Fact]
    public void CreateRejectsInvalidEmail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateCustomer(DateTimeOffset.UtcNow, email: "not-an-email"));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void UpdateDetailsChangesEditableValues()
    {
        var customer = CreateCustomer(new DateTimeOffset(2026, 8, 11, 10, 0, 0, TimeSpan.Zero));
        var updatedAt = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);

        customer.UpdateDetails(
            "Updated BV",
            "Alex Example",
            "New Street 2",
            "2000 AB",
            "Rotterdam",
            "Netherlands",
            "alex@example.com",
            "+31 10 123 4567",
            "NL123",
            "Updated notes",
            updatedAt);

        Assert.Equal("Updated BV", customer.CompanyName);
        Assert.Equal("Rotterdam", customer.City);
        Assert.Equal(updatedAt, customer.UpdatedAt);
    }

    [Fact]
    public void ArchiveIsIdempotent()
    {
        var customer = CreateCustomer(new DateTimeOffset(2026, 8, 11, 10, 0, 0, TimeSpan.Zero));
        var archivedAt = new DateTimeOffset(2026, 8, 12, 10, 0, 0, TimeSpan.Zero);

        customer.Archive(archivedAt);
        customer.Archive(archivedAt.AddDays(1));

        Assert.True(customer.IsArchived);
        Assert.Equal(archivedAt, customer.UpdatedAt);
    }

    private static Customer CreateCustomer(
        DateTimeOffset utcNow,
        string companyName = "Example BV",
        string? email = "info@example.com")
    {
        return Customer.Create(
            companyName,
            "Alex Example",
            "Main Street 1",
            "1000 AA",
            "Amsterdam",
            "Netherlands",
            email,
            "+31 20 123 4567",
            "NL123",
            null,
            utcNow);
    }
}
