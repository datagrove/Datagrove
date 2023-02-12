using Datagrove.Testing.Selenium;

namespace Playwright.WebDriver.WaitExtension.WaitTypeSelections
{
    public interface IElementWaitTypeSelection
    {
        ITextWaitConditions ForText();
        IClassWaitConditions ForClasses();
        IAttributeWaitConditions ForAttributes();
        IElementWaitConditions ForElement();
    }
}