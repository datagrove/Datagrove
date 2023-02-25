using Testing.Dbdeli;

public class MainClass
{
    static async Task Main(string[] args)
    {
        var x = await DbdeliClient.connect(new Uri("ws://localhost:5174/ws"));
        await using (var lease = await x.reserve("v10", "test #1"))
        {
            System.Console.Write("I have a lease");
        }
    }
}