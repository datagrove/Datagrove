namespace Datagrove.Testing.Selenium;
#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Datagrove.Testing.Selenium;

// a selenium driver can control multiple windows, but the chrome profile is selected when the driver is created. It maps closely to the idea of a playwright context. Within the context the current frame and window are set as state. api's that return a webdriver are typically just returning the same webdriver for use as fluent api.


public class FirefoxDriver : PlaywrightDriver
{
    public FirefoxDriver(string _, FirefoxOptions? options)
        : base(options?.options()) { }
}
public class ChromeDriver : PlaywrightDriver
{
    public ChromeDriver(string path = "", ChromeOptions? options = null) : base(options?.options()) { }
}
public class WebkitDriver : PlaywrightDriver
{
    public WebkitDriver(string _, WebkitOptions? options) : base(options?.options()) { }
}


// maybe one proxy should be multiple pages? multiple frames?

// If I have one of these per frame, do I refcount the context?

// all the IWebDrivers that launch from the root driver share the same proxy
public class PlaywrightDriver : IWebDriver, INavigation, IOptions,
ISearchContext, IJavaScriptExecutor, ITakesScreenshot, ITargetLocator, IDisposable, ITimeouts, IWindow
//IFindsElement,ISupportsPrint, IAllowsFileDetection, IHasCapabilities, IHasCommandExecutor, IHasSessionId, ICustomDriverCommandExecutor, IHasVirtualAuthenticator

{
    // also indicates that this driver creates and owns the context.
    public PlaywrightOptions? options;

    public IPlaywright playwright;
    public IBrowser browser;
    public IBrowserContext context;

    public IPage page;

    Semaphore rpc = new Semaphore(0, 1);
    Semaphore reply = new Semaphore(0, 1);

    List<Func<PlaywrightDriver, Task<object>>> fn = new List<Func<PlaywrightDriver, Task<object>>>();
    public bool quit = false;
    Exception? e;

    private IFrame? frame
    {
        get;
        set;
    } // null is more convenient than MainFrame because we have to refresh MainFrame on every goto
    public object? rvalue = null;
    public IElementHandle? current = null;

    public ILocator getLocator(string s)
    {
        if (frame == null)
        {
            return page.Locator(s).First;
        }
        else
        {
            return frame.Locator(s).First;
        }
    }

    public void Perform(List<Func<PlaywrightDriver, Task<object>>> fn)
    {
        e = null;
        this.fn = fn;
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            throw e;
        }
    }
    public static async Task ThreadProc(PlaywrightDriver p)
    {
        if (p.options != null)
        {
            p.playwright = await Playwright.CreateAsync();
            p.browser = await p.options.createBrowser(p.playwright);
            p.context = await p.browser.NewContextAsync(p.options.contextOptions);
            await p.context.Tracing.StartAsync(p.options.tracingOptions);
            p.page = await p.context.NewPageAsync();
        }
        while (!p.quit)
        {
            p.reply.Release();
            p.rpc.WaitOne();
            try
            {
                foreach (var e in p.fn)
                {
                    p.rvalue = await e(p);
                }
            }
            catch (Exception o)
            {
                p.e = o;
            }
        }

        if (p.options != null)
        {
            await p.context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = p.options.trace,
            });
            await p.context.DisposeAsync();
            await p.browser.DisposeAsync();
            p.playwright.Dispose();
        }

        p.reply.Release();
    }

    public string GetAttribute(string locator, string attribute)
    {
        var s = exec<string>(async Task<object> (PlaywrightDriver p) =>
         {
             var options = new LocatorGetAttributeOptions()
             {
                 Timeout = 1000
             };

             // this could be a frame or a page
             var s = "";
             for (var i = 0; i < 3; i++)
             {
                 try
                 {
                     s = await getLocator(locator).GetAttributeAsync(attribute, options);
                     break;
                 }
                 catch (Exception e)
                 {
                     var x = 3;
                 }
             }
             return s;
         });
        return s;
    }
    public void Fill(string locator, string Keystrokes, bool clear=true)
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            var l = getLocator(locator);
            bool ascii = true;
            for (var i=0; i<Keystrokes.Length; i++) {
                if (Keystrokes[i]>255) {
                    ascii=false;
                    break;
                }
            }
            if (ascii && clear) {
                await l.FillAsync(Keystrokes);
                return true;
            }
            // this could be a frame or a page
            // special keys must be handled
            if (clear) {
                await l.FillAsync("");
            }
            for (var i=0; i<Keystrokes.Length; i++)
            {
                var s = Keystrokes.Substring(i,1);
                if (s[0] < 256)
                    await l.TypeAsync(s);
                else {
                    if (s==Keys.Enter)
                        await l.PressAsync("Enter");
                    else 
                        throw new NotImplementedException();          
                }     
            }
            return true;
        });
    }
    public void Click(string locator, bool force = true)
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            // this could be a frame or a page
            var opt = new LocatorClickOptions()
            {
                Force = force
            };
            await getLocator(locator).ScrollIntoViewIfNeededAsync();
            await getLocator(locator).ClickAsync(opt);
            return true;
        });
    }
    // we need a driver constructor that lets us share a page with direct playwright calls. The caller is responsible to dispose these context.
    public PlaywrightDriver(IBrowserContext browserContext, IPage page)
    {
        context = browserContext;
        this.page = page;
        Task.Run(async () => await ThreadProc(this));
        reply.WaitOne();
    }

    // this starts in the normal selenium way; the driver will own the playwright instances and dispose of them.
    public PlaywrightDriver(PlaywrightOptions? options)
    {
        this.options = options ?? new PlaywrightOptions();
        Task.Run(async () => await ThreadProc(this));
        reply.WaitOne();
    }

    public ITargetLocator SwitchTo()
    {
        // just circles back
        return this;
    }


    public void Quit()
    {
        if (quit) return;
        quit = true;
        rpc.Release();
        reply.WaitOne();
    }
    // the root driver owns the proxy; proxy dies when root dies


    public void Dispose()
    {
        Quit();
    }

    public T exec2<T>(Func<PlaywrightDriver, Task<object>> fn)
    {
        e = null;
        this.fn.Clear();
        this.fn.Add(fn);
        rpc.Release();
        reply.WaitOne();
        if (e != null)
        {
            // we should snapshot here,  
            throw e;
        }
        return (T)rvalue!;
    }
    public T exec<T>(Func<PlaywrightDriver, Task<object>> fn)
    {
        Microsoft.Playwright.PlaywrightException ethrow;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                return exec2<T>(fn);
            }
            catch (Microsoft.Playwright.PlaywrightException e)
            {
                ethrow = e;
                if (e.Message.Contains("context was destroyed") || e.Message.Contains("attached") || e.Message.Contains("navigating") || e.Message.Contains("detached"))
                {
                    Thread.Sleep(100);

                }
                else
                {
                    throw e;
                }
            }
        }
        throw new StaleElementReferenceException();
    }

    public string Url
    {
        get
        {
            return exec<string>(async Task<object> (PlaywrightDriver p) =>
            {
                await Task.CompletedTask;
                return p.page.Url;
            });
        }
        set
        {
            GoToUrl(Url);
        }
    }

    public string Title
    {
        get
        {
            return exec<string>(async Task<object> (PlaywrightDriver p) =>
                 await p.page.TitleAsync())!;
        }
    }
    public INavigation Navigate()
    {
        // this just circles back to this driver.
        return this;
    }
    public void Back()
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await p.page.GoBackAsync();
            return true;
        });
    }
    public void Forward()
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await p.page.GoForwardAsync();
            return true;
        });

    }
    public void GoToUrl(string url)
    {
        frame = null;
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            var opt = new PageGotoOptions()
            {
                WaitUntil = WaitUntilState.NetworkIdle
            };
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    System.Threading.Thread.Sleep(100);
                    await p.page.GotoAsync(url, opt);
                    await p.page.ContentAsync();
                    System.Threading.Thread.Sleep(100);
                    return true;
                }
                catch (Exception e)
                {
                    if (i == 9) throw e;
                    System.Threading.Thread.Sleep(1000);
                }

            }
            return false;
        });
    }
    public void GoToUrl(Uri url)
    {
        GoToUrl(url.ToString());
    }
    public void Refresh()
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await p.page.ReloadAsync();
            return true;
        });
    }

    public string PageSource
    {
        get
        {
            return exec<string>(async Task<object> (PlaywrightDriver p) =>
            {
                return await p.frame.ContentAsync();
            });
        }
    }

    // // window handles are unique string ids assigned the window.
    // https://www.selenium.dev/documentation/webdriver/interactions/windows/
    // Clicking a link which opens in a new window will focus the new window or tab on screen, but WebDriver will not know which window the Operating System considers active. To work with the new window you will need to switch to it. If you have only two tabs or windows open, and you know which window you start with, by the process of elimination you can loop over both windows or tabs that WebDriver can see, and switch to the one which is not the original.


    public string CurrentWindowHandle
    {
        get
        {
            return "";
        }
    }

    public ReadOnlyCollection<string> WindowHandles
    {
        get
        {
            // https://playwright.dev/dotnet/docs/pages#multiple-pages
            // Get all new pages (including popups) in the context
            // context.Page += async  (_, page) => {
            //     await page.WaitForLoadStateAsync();
            //     Console.WriteLine(await page.TitleAsync());
            // };

            return new ReadOnlyCollection<string>(new List<string>());
        }
    }

    public TimeSpan ImplicitWait { get; set; } = TimeSpan.FromSeconds(0);


    public TimeSpan AsynchronousJavaScript { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public TimeSpan PageLoad { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ICookieJar Cookies => throw new NotImplementedException();

    IWindow IOptions.Window => this;


    public ILogs Logs => throw new NotImplementedException();

    public Point Position
    {
        get
        {
            return new Point(0, 0);
        }
        set
        {

        }
    }

    public Size Size
    {
        get
        {
            return new Size(1920, 1080);
        }
        set
        {

        }
    }

    public void Close()
    {
        exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await p.page.CloseAsync();
            return true;
        });
    }
    public IWebElement ActiveElement()
    {
        return exec<PWebElement>(async Task<object> (PlaywrightDriver p) =>
        {
            var h = (IElementHandle)await p.page.EvaluateHandleAsync("document.activeElement");
            return new PWebElement(this, h!);
        });
    }

    public IWebElement FindElement(By by)
    {
        var hs = FindElements(by);
        if (hs.Count == 0)
        {
            throw new NoSuchElementException("wd");
        }
        return hs[0];
    }
    // if this doesn't find, it throws.
    public IWebElement FindElement(By by, IElementHandle root)
    {
        var hs = FindElements(by, root);
        if (hs.Count == 0)
        {
            throw new NoSuchElementException("wd");
        }
        return hs[0];
    }
    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        var f = frame;
        var r = exec<ReadOnlyCollection<IWebElement>>(async Task<object> (PlaywrightDriver p) =>
        {
            IReadOnlyList<IElementHandle> h;
            if (f != null)
            {
                h = await f.Page.Locator(by.description).ElementHandlesAsync();
                if (h.Count == 0)
                {
                    h = await f.Locator(by.description).ElementHandlesAsync();
                }
            }
            else
            {
                h = await page.Locator(by.description).ElementHandlesAsync();
            }
            var lst = h.Select(e => (IWebElement)new PWebElement(this, e)).ToList();
            return new ReadOnlyCollection<IWebElement>(lst);
        })!;
        return r;
    }
    public ReadOnlyCollection<IWebElement> FindElements(By by, IElementHandle root)
    {
        var r = exec<ReadOnlyCollection<IWebElement>>(async Task<object> (PlaywrightDriver p) =>
        {
            IReadOnlyCollection<IElementHandle>? a;

            a = await root.QuerySelectorAllAsync(by.description);
            var lst = a.Select(e => (IWebElement)new PWebElement(this, e)).ToList();
            return new ReadOnlyCollection<IWebElement>(lst);

        })!;
        if (r.Count == 0)
        {
            return r;
        }
        return r;
    }



    public Screenshot GetScreenshot()
    {
        return exec<Screenshot>(async Task<object> (PlaywrightDriver p) =>
        {
            var s = await p.page.ScreenshotAsync(new()
            {
                Type = ScreenshotType.Png,
                FullPage = true
            });
            return new Screenshot(s);
        });
    }


    public object ExecuteScript(string script, params object[] args)
    {
        // if args are PWebElements we need to convert them to IJSHandles
        args = args.Select(a =>
        {
            if (a is PWebElement pwe)
            {
                return pwe.h;
            }
            return a;
        }).ToArray();

        if (args.Count() == 0)
        {
            script = $"()=> {{ {script} }}";
        }
        else
        {
            script = $"arguments=> {{ {script} }}";
        }

        return exec<object>(async Task<object> (PlaywrightDriver p) =>
        {
            return await p.page.EvaluateAsync<object>(script, args);
        });
    }

    public object ExecuteScript(PinnedScript script, params object[] args)
    {
        throw new NotImplementedException();
    }

    public object ExecuteAsyncScript(string script, params object[] args)
    {
        return ExecuteScript(script, args);
    }



    public IAlert Alert()
    {
        return new PwAlert(this);
    }

    public IWebDriver DefaultContent()
    {
        frame = null;
        return this;
    }

    public IWebDriver Frame(int frameIndex)
    {
        frame = page.Frames[frameIndex];
        return this;
    }

    public ILocator L(By by)
    {
        if (frame == null)
        {
            return page.Locator(by.description);
        }
        else
        {
            return frame.Page.Locator(by.description);
        }
    }
    public IWebDriver Frame(string frameName)
    {
        for (var i = 0; i < 30; i++)
        {
            var h = FindElements(By.Name(frameName));
            if (h.Count > 0)
            {
                Frame(h[0]);
                break;
            }
        }
        return this;
    }

    // this doesn't return a new driver, it returns this for fluidity?
    public IWebDriver Frame(IWebElement frameElement)
    {
        var e = (PWebElement)frameElement;
        var h = e.h;

        exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                frame = await h.ContentFrameAsync();
                return true;
            });
        return this;
    }

    public IWebDriver NewWindow(WindowType typeHint)
    {
        throw new NotImplementedException();
        //return new PlaywrightDriver(this, );
    }

    public IWebDriver ParentFrame()
    {
        if (frame != null)
            frame = frame.ParentFrame;
        return this;
    }

    public IWebDriver Window(string windowName)
    {
        throw new NotImplementedException();
    }

    public IOptions Manage()
    {
        return this;
    }

    public ITimeouts Timeouts()
    {
        return this;
    }

    public void Maximize()
    {

    }

    public void Minimize()
    {

    }

    public void FullScreen()
    {

    }
}

public class PWebElement : IWebElement, IFindsElement, IWrapsDriver //, IWebDriverObjectReference ILocatable
{
    // we might need some frame sudo reference for context?
    public PlaywrightDriver driver;
    public IElementHandle h;

    public PWebElement(PlaywrightDriver driver, IElementHandle h)
    {
        this.driver = driver;
        this.h = h;
    }

    public string TagName
    {
        get
        {
            return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
            {
                return (await h.GetPropertyAsync("tagName"))?.ToString() ?? "";
            });
        }
    }
    public string Text
    {
        get
        {

            return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
            {
                var s = await h.InnerTextAsync();
                return s?.Trim() ?? "";
            });

        }
    }
    public bool Enabled
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                return await h.IsEnabledAsync();
            });
        }
    }
    public bool Selected
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                await Task.CompletedTask;
                return true;
            });
        }
    }

    public Point Location
    {
        get
        {
            return driver.exec<Point>(async Task<object> (PlaywrightDriver p) =>
            {
                var o = await h.BoundingBoxAsync();
                return new Point((int)o!.X, (int)o.Y);
            });
        }
    }

    public Size Size
    {
        get
        {
            return driver.exec<Size>(async Task<object> (PlaywrightDriver p) =>
            {
                var o = await h.BoundingBoxAsync();
                return new Size((int)o!.Width, (int)o.Height);
            });
        }
    }
    public bool Displayed
    {
        get
        {
            return driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                return await h.IsVisibleAsync();
            });
        }
    }

    public IWebDriver WrappedDriver => throw new NotImplementedException();

    public Point LocationOnScreenOnceScrolledIntoView => throw new NotImplementedException();

    //public ICoordinates Coordinates => throw new NotImplementedException();

    public string ObjectReferenceId => throw new NotImplementedException();

    public void Clear()
    {
        SendKeys("");
    }
    public void SendKeys(string text)
    {
        driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await h.TypeAsync(text);
            return true;
        });
    }


    public void Click()
    {
        driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await h.ClickAsync();
            return true;
        });
    }
    public void Submit()
    {
        // clasms that we can submit on any element in the form
        driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
        {
            await h.ClickAsync();
            return true;
        });

    }

    public IWebElement FindElement(By by)
    {
        return driver.FindElement(by, h);
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        return driver.FindElements(by, h);
    }



    // java: System.out.println(page.locator("body").evaluate("element => getComputedStyle(element)['background-color']"));
    public string GetCssValue(string propertyName)
    {
        // window.getComputedStyle(e).getPropertyValue("color")
        return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
        {
            return await p.page.Locator("div").EvaluateAsync($"e => window.getComputedStyle(e).{propertyName}");
        });
    }

    public string GetAttribute(string attributeName)
    {
        // getAttribute is pre-w3c way, use DomAttribute instead
        return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
        {
            if (attributeName == "class")
            {
                var s = await h.GetPropertyAsync("className");
                return s?.ToString() ?? "";
            }
            var prop = await h.GetPropertyAsync(attributeName);
            return prop?.ToString() ?? "";
        });
    }
    public string GetDomAttribute(string attributeName)
    {
        return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
      {
          return await h.GetAttributeAsync(attributeName) ?? "";
      });
    }

    public string GetDomProperty(string propertyName)
    {
        return driver.exec<string>(async Task<object> (PlaywrightDriver p) =>
         {
             var o = await h.GetPropertyAsync(propertyName);
             var s = o.ToString();
             return s!;
         });
    }

    public ISearchContext GetShadowRoot()
    {
        return this;
    }

    public IWebElement FindElement(string mechanism, string value)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyCollection<IWebElement> FindElements(string mechanism, string value)
    {
        throw new NotImplementedException();
    }
}


// these are auto dismissed by playwright, but can be handled by event.
// page.Dialog += async (_, dialog) =>
// {
//     System.Console.WriteLine(dialog.Message);
//     await dialog.DismissAsync();
// };
public class PwAlert : IAlert
{
    PlaywrightDriver driver;
    public PwAlert(PlaywrightDriver driver)
    {
        this.driver = driver;
    }
    public string Text
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public void Accept()
    {
        throw new NotImplementedException();
    }

    public void Dismiss()
    {
        throw new NotImplementedException();
    }

    public void SendKeys(string keysToSend)
    {
        throw new NotImplementedException();
    }
}

// playwright has a very different model of windows.

// // Get page after a specific action (e.g. clicking a link)
// var newPage = await context.RunAndWaitForPageAsync(async () =>
// {
//     await page.GetByText("open new tab").ClickAsync();
// });

// needs to hold a reference (IElementHandle) to the parent frame
// in order to peform parentFrame

// selenium returns a web driver, not a target locator, more work for us :(
