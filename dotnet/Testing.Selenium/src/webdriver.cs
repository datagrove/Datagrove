namespace Datagrove.Testing.Selenium;
using System.Collections.ObjectModel;
using System.Drawing;
using System;
using System.Globalization;
using System.Runtime.Serialization;

public enum WindowType
{
    Window = 0, Tab = 1
}

public interface IWrapsDriver
{
    IWebDriver WrappedDriver { get; }
}
public interface IWrapsElement
{
    IWebElement WrappedElement { get; }
}


public enum ScreenshotImageFormat
{
    Png, Jpeg, Gif, Tiff, Bmp
}
public interface ITakesScreenshot
{
    Screenshot GetScreenshot();
}

public class Screenshot
{
    public byte[] data;
    // public Screenshot(string base64EncodedScreenshot) 
    // {
    //   this.data = Convert.FromBase64String(base64EncodedScreenshot)
    // }
    public Screenshot(byte[] data)
    {
        this.data = data;
    }

    public void SaveAsFile(string fileName, ScreenshotImageFormat format = ScreenshotImageFormat.Png)
    {
        if (format != ScreenshotImageFormat.Png)
        {
            throw new WebDriverException(".NET Core does not support image manipulation, so only Portable Network Graphics (PNG) format is supported");
        }

        using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
        {
            fileStream.Write(data, 0, data.Length);
        }
    }
}

public interface IFindsElement
{
    IWebElement FindElement(string mechanism, string value);
    ReadOnlyCollection<IWebElement> FindElements(string mechanism, string value);
}

public class By
{
    public string description;

    public By(string description)
    {
        this.description = description;
    }
    public override int GetHashCode()
    {
        return this.description.GetHashCode();
    }


    public string Criteria
    {
        get { return this.description; }
    }

    public static bool operator ==(By one, By two)
    {
        return one.description == two.description;
    }

    public static bool operator !=(By one, By two)
    {
        return !(one == two);
    }
    public override bool Equals(object obj)
    {
        return description.Equals(obj);
    }

    public static By Id(string idToFind)
    {
        return new By($"#{idToFind}");
    }


    public static By LinkText(string linkTextToFind)
    {
        return new By($"text={linkTextToFind}");
    }


    public static By Name(string nameToFind)
    {
        return new By($"[name='{nameToFind}']");
    }

    public static By XPath(string xpathToFind)
    {
        return new By($"xpath={xpathToFind}");
    }

    public static By Placeholder(string x) => new By($"[placeholder='{x}']");
    public static By ClassName(string classNameToFind)
    {
        return new By($".{classNameToFind}");
    }

    public static By Value(string x) => By.CssSelector($"input[value='{x}']");

    public static By PartialLinkText(string partialLinkTextToFind)
    {
        return new By($"a:has-text(\"{partialLinkTextToFind}\")");
    }

    public static By TagName(string tagNameToFind)
    {
        return new By(tagNameToFind);
    }


    public static By CssSelector(string cssSelectorToFind)
    {
        return new By($"{cssSelectorToFind}");
    }

    public virtual IWebElement FindElement(ISearchContext context)
    {
        return context.FindElement(this);
    }

    public virtual ReadOnlyCollection<IWebElement> FindElements(ISearchContext context)
    {
        return context.FindElements(this);
    }

    public override string ToString()
    {
        return this.description;
    }
}


// modified for nullable
public interface IWebElement : ISearchContext
{
    string TagName { get; }
    string Text { get; }
    bool Enabled { get; }
    bool Selected { get; }
    Point Location { get; }
    Size Size { get; }
    bool Displayed { get; }

    void Clear();
    void Click();
    string GetAttribute(string attributeName);
    string GetCssValue(string propertyName);
    string GetDomAttribute(string attributeName);
    string GetDomProperty(string propertyName);
    ISearchContext GetShadowRoot();
    void SendKeys(string text);
    void Submit();
}

// each webelement is a search context.
public interface ISearchContext
{
    IWebElement FindElement(By by);
    ReadOnlyCollection<IWebElement> FindElements(By by);
}
public interface IAlert
{
    string Text { get; }

    void Accept();
    void Dismiss();
    void SendKeys(string keysToSend);
}

public interface ITargetLocator
{
    IWebElement ActiveElement();
    IAlert Alert();
    IWebDriver DefaultContent();
    IWebDriver Frame(int frameIndex);
    IWebDriver Frame(string frameName);
    IWebDriver Frame(IWebElement frameElement);
    IWebDriver NewWindow(WindowType typeHint);
    IWebDriver ParentFrame();
    IWebDriver Window(string windowName);
}
public interface INavigation
{
    void Back();
    void Forward();
    void GoToUrl(string url);
    void GoToUrl(Uri url);
    void Refresh();
}
public interface IWebDriver : ISearchContext, IDisposable
{
    string Url { get; set; }
    string Title { get; }
    string PageSource { get; }
    string CurrentWindowHandle { get; }
    ReadOnlyCollection<string> WindowHandles { get; }

    void Close();
    //IOptions Manage();
    INavigation Navigate();
    void Quit();
    ITargetLocator SwitchTo();

    IOptions Manage();
}

public interface IWait<T>
{
    /// <summary>
    /// Gets or sets how long to wait for the evaluated condition to be true.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Gets or sets how often the condition should be evaluated.
    /// </summary>
    TimeSpan PollingInterval { get; set; }

    /// <summary>
    /// Gets or sets the message to be displayed when time expires.
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Configures this instance to ignore specific types of exceptions while waiting for a condition.
    /// Any exceptions not whitelisted will be allowed to propagate, terminating the wait.
    /// </summary>
    /// <param name="exceptionTypes">The types of exceptions to ignore.</param>
    void IgnoreExceptionTypes(params Type[] exceptionTypes);

    /// <summary>
    /// Waits until a condition is true or times out.
    /// </summary>
    /// <typeparam name="TResult">The type of result to expect from the condition.</typeparam>
    /// <param name="condition">A delegate taking a TSource as its parameter, and returning a TResult.</param>
    /// <returns>If TResult is a boolean, the method returns <see langword="true"/> when the condition is true, and <see langword="false"/> otherwise.
    /// If TResult is an object, the method returns the object when the condition evaluates to a value other than <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when TResult is not boolean or an object type.</exception>
    TResult Until<TResult>(Func<T, TResult> condition);
}
public interface IClock
{
    /// <summary>
    /// Gets the current date and time values.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> at a specified offset in the future.
    /// </summary>
    /// <param name="delay">The offset to use.</param>
    /// <returns>The <see cref="DateTime"/> at the specified offset in the future.</returns>
    DateTime LaterBy(TimeSpan delay);

    /// <summary>
    /// Gets a value indicating whether the current date and time is before the specified date and time.
    /// </summary>
    /// <param name="otherDateTime">The date and time values to compare the current date and time values to.</param>
    /// <returns><see langword="true"/> if the current date and time is before the specified date and time; otherwise, <see langword="false"/>.</returns>
    bool IsNowBefore(DateTime otherDateTime);
}
public class SystemClock : IClock
{
    /// <summary>
    /// Gets the current date and time values.
    /// </summary>
    public DateTime Now
    {
        get { return DateTime.Now; }
    }

    /// <summary>
    /// Calculates the date and time values after a specific delay.
    /// </summary>
    /// <param name="delay">The delay after to calculate.</param>
    /// <returns>The future date and time values.</returns>
    public DateTime LaterBy(TimeSpan delay)
    {
        return DateTime.Now.Add(delay);
    }

    /// <summary>
    /// Gets a value indicating whether the current date and time is before the specified date and time.
    /// </summary>
    /// <param name="otherDateTime">The date and time values to compare the current date and time values to.</param>
    /// <returns><see langword="true"/> if the current date and time is before the specified date and time; otherwise, <see langword="false"/>.</returns>
    public bool IsNowBefore(DateTime otherDateTime)
    {
        return DateTime.Now < otherDateTime;
    }
}
public class DefaultWait<T> : IWait<T>
{
    private T input;
    private IClock clock;

    private TimeSpan timeout = DefaultSleepTimeout;
    private TimeSpan sleepInterval = DefaultSleepTimeout;
    private string message = string.Empty;

    private List<Type> ignoredExceptions = new List<Type>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultWait&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="input">The input value to pass to the evaluated conditions.</param>
    public DefaultWait(T input)
        : this(input, new SystemClock())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultWait&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="input">The input value to pass to the evaluated conditions.</param>
    /// <param name="clock">The clock to use when measuring the timeout.</param>
    public DefaultWait(T input, IClock clock)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input), "input cannot be null");
        }

        if (clock == null)
        {
            throw new ArgumentNullException(nameof(clock), "clock cannot be null");
        }

        this.input = input;
        this.clock = clock;
    }

    /// <summary>
    /// Gets or sets how long to wait for the evaluated condition to be true. The default timeout is 500 milliseconds.
    /// </summary>
    public TimeSpan Timeout
    {
        get { return this.timeout; }
        set { this.timeout = value; }
    }

    /// <summary>
    /// Gets or sets how often the condition should be evaluated. The default timeout is 500 milliseconds.
    /// </summary>
    public TimeSpan PollingInterval
    {
        get { return this.sleepInterval; }
        set { this.sleepInterval = value; }
    }

    /// <summary>
    /// Gets or sets the message to be displayed when time expires.
    /// </summary>
    public string Message
    {
        get { return this.message; }
        set { this.message = value; }
    }

    private static TimeSpan DefaultSleepTimeout
    {
        get { return TimeSpan.FromMilliseconds(500); }
    }

    /// <summary>
    /// Configures this instance to ignore specific types of exceptions while waiting for a condition.
    /// Any exceptions not whitelisted will be allowed to propagate, terminating the wait.
    /// </summary>
    /// <param name="exceptionTypes">The types of exceptions to ignore.</param>
    public void IgnoreExceptionTypes(params Type[] exceptionTypes)
    {
        if (exceptionTypes == null)
        {
            throw new ArgumentNullException(nameof(exceptionTypes), "exceptionTypes cannot be null");
        }

        foreach (Type exceptionType in exceptionTypes)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException("All types to be ignored must derive from System.Exception", nameof(exceptionTypes));
            }
        }

        this.ignoredExceptions.AddRange(exceptionTypes);
    }

    /// <summary>
    /// Repeatedly applies this instance's input value to the given function until one of the following
    /// occurs:
    /// <para>
    /// <list type="bullet">
    /// <item>the function returns neither null nor false</item>
    /// <item>the function throws an exception that is not in the list of ignored exception types</item>
    /// <item>the timeout expires</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The delegate's expected return type.</typeparam>
    /// <param name="condition">A delegate taking an object of type T as its parameter, and returning a TResult.</param>
    /// <returns>The delegate's return value.</returns>
    public virtual TResult Until<TResult>(Func<T, TResult> condition)
    {
        return Until(condition, CancellationToken.None);
    }

    /// <summary>
    /// Repeatedly applies this instance's input value to the given function until one of the following
    /// occurs:
    /// <para>
    /// <list type="bullet">
    /// <item>the function returns neither null nor false</item>
    /// <item>the function throws an exception that is not in the list of ignored exception types</item>
    /// <item>the timeout expires</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <typeparam name="TResult">The delegate's expected return type.</typeparam>
    /// <param name="condition">A delegate taking an object of type T as its parameter, and returning a TResult.</param>
    /// <param name="token">A cancellation token that can be used to cancel the wait.</param>
    /// <returns>The delegate's return value.</returns>
    public virtual TResult Until<TResult>(Func<T, TResult> condition, CancellationToken token)
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition), "condition cannot be null");
        }

        var resultType = typeof(TResult);
        if ((resultType.IsValueType && resultType != typeof(bool)) || !typeof(object).IsAssignableFrom(resultType))
        {
            throw new ArgumentException("Can only wait on an object or boolean response, tried to use type: " + resultType.ToString(), nameof(condition));
        }

        Exception lastException = null;
        var endTime = this.clock.LaterBy(this.timeout);
        while (true)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var result = condition(this.input);
                if (resultType == typeof(bool))
                {
                    var boolResult = result as bool?;
                    if (boolResult.HasValue && boolResult.Value)
                    {
                        return result;
                    }
                }
                else
                {
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!this.IsIgnoredException(ex))
                {
                    throw;
                }

                lastException = ex;
            }

            // Check the timeout after evaluating the function to ensure conditions
            // with a zero timeout can succeed.
            if (!this.clock.IsNowBefore(endTime))
            {
                string timeoutMessage = string.Format(CultureInfo.InvariantCulture, "Timed out after {0} seconds", this.timeout.TotalSeconds);
                if (!string.IsNullOrEmpty(this.message))
                {
                    timeoutMessage += ": " + this.message;
                }

                this.ThrowTimeoutException(timeoutMessage, lastException);
            }

            Thread.Sleep(this.sleepInterval);
        }
    }

    /// <summary>
    /// Throws a <see cref="WebDriverTimeoutException"/> with the given message.
    /// </summary>
    /// <param name="exceptionMessage">The message of the exception.</param>
    /// <param name="lastException">The last exception thrown by the condition.</param>
    /// <remarks>This method may be overridden to throw an exception that is
    /// idiomatic for a particular test infrastructure.</remarks>
    protected virtual void ThrowTimeoutException(string exceptionMessage, Exception lastException)
    {
        throw new WebDriverTimeoutException(exceptionMessage, lastException);
    }

    private bool IsIgnoredException(Exception exception)
    {
        return this.ignoredExceptions.Any(type => type.IsAssignableFrom(exception.GetType()));
    }
}
public class WebDriverWait : DefaultWait<IWebDriver>
{

    public WebDriverWait(IWebDriver driver, TimeSpan timeout)
        : this(new SystemClock(), driver, timeout, DefaultSleepTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverWait"/> class.
    /// </summary>
    /// <param name="clock">An object implementing the <see cref="IClock"/> interface used to determine when time has passed.</param>
    /// <param name="driver">The WebDriver instance used to wait.</param>
    /// <param name="timeout">The timeout value indicating how long to wait for the condition.</param>
    /// <param name="sleepInterval">A <see cref="TimeSpan"/> value indicating how often to check for the condition to be true.</param>
    public WebDriverWait(IClock clock, IWebDriver driver, TimeSpan timeout, TimeSpan sleepInterval)
        : base(driver, clock)
    {
        this.Timeout = timeout;
        this.PollingInterval = sleepInterval;
        //this.IgnoreExceptionTypes(typeof(NotFoundException));
    }

    private static TimeSpan DefaultSleepTimeout
    {
        get { return TimeSpan.FromMilliseconds(500); }
    }
}

public class WebDriverException : Exception
{
    public string message;
    public WebDriverException(string message)
    {
        this.message = message;
    }
    public WebDriverException()
    {
        this.message = "WebDriverException";
    }
    public WebDriverException(string message, Exception innerException)
    {
        this.message = message;
    }
    public WebDriverException(SerializationInfo info, StreamingContext context)
    {
    }
}

public class WebDriverTimeoutException : WebDriverException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class.
    /// </summary>
    public WebDriverTimeoutException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WebDriverTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverTimeoutException"/> class with
    /// a specified error message and a reference to the inner exception that is the
    /// cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception,
    /// or <see langword="null"/> if no inner exception is specified.</param>
    public WebDriverTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }



}

public interface IOptions
{
    /// <summary>
    /// Gets an object allowing the user to manipulate cookies on the page.
    /// </summary>
    ICookieJar Cookies { get; }

    /// <summary>
    /// Gets an object allowing the user to manipulate the currently-focused browser window.
    /// </summary>
    /// <remarks>"Currently-focused" is defined as the browser window having the window handle
    /// returned when IWebDriver.CurrentWindowHandle is called.</remarks>
    IWindow Window { get; }

    /// <summary>
    /// Gets an object allowing the user to examine the logs for this driver instance.
    /// </summary>
    ILogs Logs { get; }

    /// <summary>
    /// Gets an object allowing the user to manage network communication by the browser.
    /// </summary>
    //INetwork Network { get; }

    /// <summary>
    /// Provides access to the timeouts defined for this driver.
    /// </summary>
    /// <returns>An object implementing the <see cref="ITimeouts"/> interface.</returns>
    ITimeouts Timeouts();
}
public interface ICookieJar
{
    /// <summary>
    /// Gets all cookies defined for the current page.
    /// </summary>
    ReadOnlyCollection<Cookie> AllCookies { get; }

    /// <summary>
    /// Adds a cookie to the current page.
    /// </summary>
    /// <param name="cookie">The <see cref="Cookie"/> object to be added.</param>
    void AddCookie(Cookie cookie);

    /// <summary>
    /// Gets a cookie with the specified name.
    /// </summary>
    /// <param name="name">The name of the cookie to retrieve.</param>
    /// <returns>The <see cref="Cookie"/> containing the name. Returns <see langword="null"/>
    /// if no cookie with the specified name is found.</returns>
    Cookie GetCookieNamed(string name);

    /// <summary>
    /// Deletes the specified cookie from the page.
    /// </summary>
    /// <param name="cookie">The <see cref="Cookie"/> to be deleted.</param>
    void DeleteCookie(Cookie cookie);

    /// <summary>
    /// Deletes the cookie with the specified name from the page.
    /// </summary>
    /// <param name="name">The name of the cookie to be deleted.</param>
    void DeleteCookieNamed(string name);

    /// <summary>
    /// Deletes all cookies from the page.
    /// </summary>
    void DeleteAllCookies();
}

/// <summary>
/// Defines the interface through which the user can define timeouts.
/// </summary>
public interface ITimeouts
{
    /// <summary>
    /// Gets or sets the implicit wait timeout, which is the  amount of time the
    /// driver should wait when searching for an element if it is not immediately
    /// present.
    /// </summary>
    /// <remarks>
    /// When searching for a single element, the driver should poll the page
    /// until the element has been found, or this timeout expires before throwing
    /// a <see cref="NoSuchElementException"/>. When searching for multiple elements,
    /// the driver should poll the page until at least one element has been found
    /// or this timeout has expired.
    /// <para>
    /// Increasing the implicit wait timeout should be used judiciously as it
    /// will have an adverse effect on test run time, especially when used with
    /// slower location strategies like XPath.
    /// </para>
    /// </remarks>
    TimeSpan ImplicitWait { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous script timeout, which is the amount
    /// of time the driver should wait when executing JavaScript asynchronously.
    /// This timeout only affects the <see cref="IJavaScriptExecutor.ExecuteAsyncScript(string, object[])"/>
    /// method.
    /// </summary>
    TimeSpan AsynchronousJavaScript { get; set; }

    /// <summary>
    /// Gets or sets the page load timeout, which is the amount of time the driver
    /// should wait for a page to load when setting the <see cref="IWebDriver.Url"/>
    /// property.
    /// </summary>
    TimeSpan PageLoad { get; set; }
}

public interface ILogs
{
    /// <summary>
    /// Gets the list of available log types for this driver.
    /// </summary>
    ReadOnlyCollection<string> AvailableLogTypes { get; }

    /// <summary>
    /// Gets the set of <see cref="LogEntry"/> objects for a specified log.
    /// </summary>
    /// <param name="logKind">The log for which to retrieve the log entries.
    /// Log types can be found in the <see cref="LogType"/> class.</param>
    /// <returns>The list of <see cref="LogEntry"/> objects for the specified log.</returns>
    ReadOnlyCollection<LogEntry> GetLog(string logKind);
}

public class LogEntry
{
    private LogLevel level = LogLevel.All;
    private DateTime timestamp = DateTime.MinValue;
    private string message = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// </summary>
    private LogEntry()
    {
    }

    /// <summary>
    /// Gets the timestamp value of the log entry.
    /// </summary>
    public DateTime Timestamp
    {
        get { return this.timestamp; }
    }

    /// <summary>
    /// Gets the logging level of the log entry.
    /// </summary>
    public LogLevel Level
    {
        get { return this.level; }
    }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    public string Message
    {
        get { return this.message; }
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="LogEntry"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="LogEntry"/>.</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "[{0:yyyy-MM-ddTHH:mm:ssZ}] [{1}] {2}", this.timestamp, this.level, this.message);
    }

    /// <summary>
    /// Creates a <see cref="LogEntry"/> from a dictionary as deserialized from JSON.
    /// </summary>
    /// <param name="entryDictionary">The <see cref="Dictionary{TKey, TValue}"/> from
    /// which to create the <see cref="LogEntry"/>.</param>
    /// <returns>A <see cref="LogEntry"/> with the values in the dictionary.</returns>
    internal static LogEntry FromDictionary(Dictionary<string, object> entryDictionary)
    {
        LogEntry entry = new LogEntry();
        if (entryDictionary.ContainsKey("message"))
        {
            entry.message = entryDictionary["message"].ToString();
        }

        if (entryDictionary.ContainsKey("timestamp"))
        {
            DateTime zeroDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            double timestampValue = Convert.ToDouble(entryDictionary["timestamp"], CultureInfo.InvariantCulture);
            entry.timestamp = zeroDate.AddMilliseconds(timestampValue);
        }

        if (entryDictionary.ContainsKey("level"))
        {
            string levelValue = entryDictionary["level"].ToString();
            try
            {
                entry.level = (LogLevel)Enum.Parse(typeof(LogLevel), levelValue, true);
            }
            catch (ArgumentException)
            {
                // If the requested log level string is not a valid log level,
                // ignore it and use LogLevel.All.
            }
        }

        return entry;
    }
}
public enum LogLevel
{
    /// <summary>
    /// Show all log messages.
    /// </summary>
    All,

    /// <summary>
    /// Show messages with information useful for debugging.
    /// </summary>
    Debug,

    /// <summary>
    /// Show informational messages.
    /// </summary>
    Info,

    /// <summary>
    /// Show messages corresponding to non-critical issues.
    /// </summary>
    Warning,

    /// <summary>
    /// Show messages corresponding to critical issues.
    /// </summary>
    Severe,

    /// <summary>
    /// Show no log messages.
    /// </summary>
    Off
}

public class ManageOptions : IOptions
{
    public ICookieJar Cookies => throw new NotImplementedException();

    public IWindow Window => throw new NotImplementedException();

    public ILogs Logs => throw new NotImplementedException();

    public ITimeouts Timeouts()
    {
        throw new NotImplementedException();
    }
}

public class InvalidElementStateException : Exception
{
}
public class ElementNotVisibleException : Exception
{
}