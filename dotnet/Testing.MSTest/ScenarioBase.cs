namespace Datagrove.Testing.MSTest;
using Datagrove.Testing.Selenium;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// we want this to be base class because that's convenient to get the services.
// but we might want 
public class ScenarioBase : PlaywrightScenarioBase, IAsyncDisposable
{
    public TestContext TestContext { get; set; }

    public ScenarioBase(TestContext context){
        TestContext = context;
    }


}
