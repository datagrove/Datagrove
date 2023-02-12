using Datagrove.Testing.Selenium;
using Datagrove.Testing.Selenium;

namespace Playwright.WebDriver.WaitExtension.WaitTypeSelections
{
    public class WaitTypeSelection : IWaitTypeSelection
    {
        private readonly IWebDriver _webDriver;
        private readonly int _waitMs;

        public WaitTypeSelection(IWebDriver webDriver, int waitMs)
        {
            _webDriver = webDriver;
            _waitMs = waitMs;
        }

        public IWebElementWaitConditions ForElement(By @by)
        {
            return new WebElementWaitConditions(_webDriver, _waitMs, @by);
        }

        public IWebPageWaitConditions ForPage()
        {
            return new WebPageWaitConditions(_webDriver, _waitMs);
        }


    }
}