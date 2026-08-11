namespace InvoiceManager.Domain.Tests;

public sealed class TestProjectSmokeTests
{
    [Fact]
    public void TestRunnerDiscoversDomainTests()
    {
        Assert.NotNull(typeof(TestProjectSmokeTests).Assembly);
    }
}
