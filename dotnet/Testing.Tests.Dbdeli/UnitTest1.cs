using Testing.Dbdeli;

namespace Testing.Tests.Dbdeli;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var x = await DbdeliClient.connect(new Uri("ws://localhost:5174"));
        using (var lease = x.reserve("v10", "test #1"))
        {
            System.Console.Write("I have a lease");
        }
    }
}