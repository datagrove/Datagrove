
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright.MSTest;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;

// these tests only show playwright working 
[TestClass]
public class PlaywrightTest
{


    [TestMethod]
    public async Task TestMethod1()
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var x = await page.EvaluateAsync("() => 2+2");
        Assert.AreEqual(4,x.As<int>());
        browser.CloseAsync().Wait();
    }
}