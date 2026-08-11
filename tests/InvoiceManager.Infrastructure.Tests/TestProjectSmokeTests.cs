namespace InvoiceManager.Infrastructure.Tests;

public sealed class TestProjectSmokeTests
{
    [Fact]
    public void TestRunnerDiscoversInfrastructureTests()
    {
        Assert.NotNull(typeof(TestProjectSmokeTests).Assembly);
    }
}
