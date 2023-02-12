namespace Datagrove.Testing.Sample;
using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Datagrove.Testing.Selenium;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using Datagrove.Testing.MSTest;

using static Microsoft.Playwright.Assertions;

//  this shows steps that don't use any services
[Binding]
public class CalculatorSteps : IAsyncDisposable
{
    //StepState state;
    // note that a step class is initialized once per test. It is not shared.
    int sum = 0;

    // Note that you can "inject" state variable on any step. The construct or will be called at the beginning of the test (scenario) and the dispose at the end.
    public CalculatorSteps()
    {
        //this.state = state;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    // note that we can have a mix of async and sync steps. 

    [Given(@"I have a calculator")]
    public void I_have_a_calculator()
    {
        sum = 0;
    }

    [Given(@"I have numbers (.*) and (.*) as input")]
    public async Task I_have_and_as_input(int p0, int p1)
    {
        sum = p0 + p1;
        await Task.CompletedTask;
    }
    [Given(@"I add more numbers")]
    public async Task I_add_more_numbers(Table table)
    {
        foreach (var row in table.Rows)
        {
            sum += int.Parse(row[0]);
        }
        await Task.CompletedTask;
    }

    [Then(@"I should get an output of (.*)")]
    public void I_should_get_an_output_of(int p0)
    {
        Assert.AreEqual(p0, sum);
    }

}

// this shows rest api steps using Playwright rest api directly
[Binding]
public class RestSteps
{
    IAPIRequestContext api;
    RestSteps(IAPIRequestContext api)
    {
        this.api = api;
    }
    private async Task CreateAPIRequestContext(ScenarioState state)
    {
        var url1 = @"https://dog.ceo/api/breeds/image/random";
        var url2 = @"https://images.dog.ceo/breeds/schipperke/n02104365_9489.jpg";
        var o = await api.GetAsync(url1);
        var j = await o.JsonAsync();
        await api.GetAsync(url2);
        Assert.AreEqual("Bug description", j?.GetProperty("body").GetString());
    }
}

// this shows steps using boa > webdriver > playwright
[Binding]
public class BoaSteps
{

}


// this shows steps using playwright directly.
[Binding]
public class PlaywrightSteps
{
    IPage page;
    PlaywrightSteps(IPage page)
    {
        this.page = page;
    }


    [When(@"on (.*) page")]
    public async Task OnHomePage(string url)
    {
        await page.WaitForURLAsync(url);
    }

    [Then(@"the page should have a title of (.*)")]
    public async Task ThePageShouldHaveATitleOf(string title)
    {
        await Assertions.Expect(page).ToHaveTitleAsync(new Regex(title));
    }

    [Then(@"the url should be (.*)")]
    public async Task Urlshouldbe(string title)
    {
        // Expect a title "to contain" a substring.
        await Expect(page).ToHaveTitleAsync(new Regex(title));
    }

    [When(@"I click link (.*)")]
    public async Task ClickLink(string title)
    {
        // create a locator
        var getStarted = page.GetByRole(AriaRole.Link, new() { Name = "Get started" });

        // Expect an attribute "to be strictly equal" to the value.
        await Expect(getStarted).ToHaveAttributeAsync("href", "/docs/intro");

        // Click the get started link.
        await getStarted.ClickAsync();

        // Expects the URL to contain intro.
        await Expect(page).ToHaveURLAsync(new Regex(".*intro"));
    }
}

