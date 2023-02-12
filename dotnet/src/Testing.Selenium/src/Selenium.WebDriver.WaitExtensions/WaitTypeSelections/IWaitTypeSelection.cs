using Datagrove.Testing.Selenium;
using Datagrove.Testing.Selenium;

namespace Playwright.WebDriver.WaitExtension.WaitTypeSelections
{
    public interface IWaitTypeSelection
    {
        IWebElementWaitConditions ForElement(By @by);
        IWebPageWaitConditions ForPage();
    }
}