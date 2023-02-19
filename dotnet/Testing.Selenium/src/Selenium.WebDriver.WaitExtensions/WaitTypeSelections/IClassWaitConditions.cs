namespace Playwright.WebDriver.WaitExtension.WaitTypeSelections
{
    public interface IClassWaitConditions
    {
        bool ToContain(string className);
        bool ToContainMatch(string regex);
        bool ToNotContain(string className);
        bool ToNotContainMatch(string regexPattern);
    }
}