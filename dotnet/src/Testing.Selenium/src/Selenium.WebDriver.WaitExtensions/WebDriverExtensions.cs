using Datagrove.Testing.Selenium;
using Playwright.WebDriver.WaitExtension.WaitTypeSelections;

namespace Playwright.WebDriver.WaitExtensions
{
    public static class WebDriverExtensions
    {
        public static IWaitTypeSelection Wait(this IWebDriver webDriver, int ms = 500)
        {
            return new WaitTypeSelection(webDriver, ms);
        }
    }
}