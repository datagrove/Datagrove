using Datagrove.Testing.Selenium;
using Datagrove.Testing.MSTest;
// steps are global; we could easily have one step class but instead we break them apart here to make it easier to see what's going on

// Holds the state for a single example of a single scenario. Steps may reference context itself or any member. Each test will initialize a TestState when it starts, and provide it to each of the steps used by the test. Be sure code is thread safe so that you can run all your tests in parallel (not hard because each thread will have its own instance).

// the Datagrove.Testing.Selenium.ScenarioState gives you most of the things you want: browser, api, selenium webdriver, direct playwright access. but add service here that you want your steps to have access to.
public class ScenarioState : ScenarioBase, IAsyncDisposable
{

    // typically all testing parameters that are not embedded in the script are read from an appsettings.json file, you can read those and provide them here.
    public override async ValueTask<PlaywrightOptions> options()
    {
        await Task.CompletedTask;

        return new PlaywrightOptions()
        {
            apiNew = new()
            {
                // All requests we send go to this API endpoint.
                // BaseURL = "https://api.github.com",
                // ExtraHTTPHeaders = headers,
            }
        };
    }
    // you can provide your own services here to the step classes
    public ScenarioState(TestContext context) : base(context) { }
}