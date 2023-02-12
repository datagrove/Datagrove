using Datagrove.Testing.Selenium;

namespace Datagrove.Testing.Selenium
{
    public interface IWebElementWaitConditions
    {
        IWebElement ToExist();
        void ToNotExist();
    }
}