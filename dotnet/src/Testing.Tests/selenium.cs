using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright.MSTest;
using System.Threading.Tasks;

/*
    What we are not using:
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Firefox;
    using OpenQA.Selenium.Support.UI;
    using OpenQA.Selenium.Support;
    using Selenium.WebDriver.WaitExtensions.WaitConditions;
    using OpenQA.Selenium.Interactions
*/

using Datagrove.Testing.Selenium;

[TestClass]
class SeleniumTest {

    [TestMethod]
    public void TestMethod1()
    {
        var driver = new ChromeDriver("D:\\3rdparty\\chrome");
        driver.Navigate().GoToUrl("https://www.google.com");
        driver.FindElement(By.Name("q")).SendKeys("cheese" + Keys.Enter);
        driver.FindElement(By.Name("btnK")).Click();
        driver.Close();
    }

    [TestMethod]
    public void TestMethod2()
    {
        // taken from guru99 example
        var driver = new ChromeDriver("G:\\");
        driver.Url = "http://demo.guru99.com/test/guru99home/";
        driver.Manage().Window.Maximize();
        IWebElement emailTextBox = driver.FindElement(By.CssSelector("input[id=philadelphia-field-email]"));
        IWebElement signUpButton = driver.FindElement(By.CssSelector("input[id=philadelphia-field-submit]"));

        emailTextBox.SendKeys("test123@gmail.com");
        signUpButton.Click();
    }
}