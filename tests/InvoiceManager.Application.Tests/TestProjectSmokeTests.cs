namespace InvoiceManager.Application.Tests;

public sealed class TestProjectSmokeTests
{
    [Fact]
    public void TestRunnerDiscoversApplicationTests()
    {
        Assert.NotNull(typeof(TestProjectSmokeTests).Assembly);
    }
}
