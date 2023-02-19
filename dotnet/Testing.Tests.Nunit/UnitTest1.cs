namespace Testing.Tests.Nunit;

public class Tests
{
    [Test]
    public async ValueTask Test1()
    {
        await Task.CompletedTask;
        Assert.Pass();
    }
}