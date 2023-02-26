using Boa.Constrictor.Screenplay;
using Datagrove.Testing.Selenium;


namespace Datagrove.Testing.Boa
{
    /// <summary>
    /// Clicks a Web element.
    /// </summary>
    public class Click : AbstractWebLocatorTask
    {
        #region Constructors

        bool force = true;
        /// <summary>
        /// Private constructor.
        /// (Use static builder methods to construct.)
        /// </summary>
        /// <param name="locator">The target Web element's locator.</param>
        private Click(IWebLocator locator,bool force=true) : base(locator) {
            this.force = force;
         }

        #endregion
        
        #region Builder Methods

        /// <summary>
        /// Constructs the Task object.
        /// </summary>
        /// <param name="locator">The target Web element's locator.</param>
        /// <returns></returns>
        public static Click On(IWebLocator locator,bool force=true) => new Click(locator,force);

        #endregion

        #region Methods

        /// <summary>
        /// Clicks the web element.
        /// Use browser actions instead of direct click (due to IE).
        /// </summary>
        /// <param name="actor">The Screenplay Actor.</param>
        /// <param name="driver">The WebDriver.</param>
        public override void PerformAs(IActor actor, IWebDriver driver)
        {
            var s = Locator.Query.Criteria;
            var d = (PlaywrightDriver)driver;
            // by default forcing, or won't be compatible. but sometimes we want stronger playwright conditions.
            d.Click(s,this.force);
            // actor.WaitsUntil(Appearance.Of(Locator), IsEqualTo.True());
            // new Actions(driver).MoveToElement(driver.FindElement(Locator.Query)).Click().Perform();
        }

        /// <summary>
        /// Returns a description of the Task.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"click on '{Locator.Description}'\n{Locator.Query.description}";

        #endregion
    }
}
