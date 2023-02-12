namespace pw1;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task TestMethod1()
    {
       var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        // if no arguments  maybe this
        var x = await page.EvaluateAsync("() => self.name");
        var y = await page.EvaluateAsync(@" (arguments) => arguments[0]+arguments[1]",new int[]{1,2});
        browser.CloseAsync().Wait();
    }
}