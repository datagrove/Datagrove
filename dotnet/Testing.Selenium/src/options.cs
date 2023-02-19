
namespace Datagrove.Testing.Selenium;
#nullable enable
using System.Collections.Generic;
using Microsoft.Playwright;

public class DriverOptions{
}
public class SeleniumOptions
{
}
public class ChromeOptions : SeleniumOptions
{
    public PlaywrightOptions options() => new PlaywrightOptions();
}

public class FirefoxOptions : SeleniumOptions
{
    public PlaywrightOptions options() => new PlaywrightOptions();
}

public class WebkitOptions : SeleniumOptions
{
    public PlaywrightOptions options() => new PlaywrightOptions();
}

// options class is immutable data meant to be easy to serialize
public class PlaywrightOptions
{
    public enum Browser
    {
        Chrome,
        Edge,
        Safari,
        Firefox
    }

    public Browser browserType;
    public BrowserTypeLaunchOptions browserOptions;
    public BrowserNewContextOptions contextOptions;
    public APIRequestNewContextOptions apiNew = new();
    public APIRequestContextOptions api = new();
    public APIRequestContextStorageStateOptions apiStorage = new();
    public TracingStartOptions tracingOptions = new()
                {
                    Screenshots = true,
                    Snapshots = true,
                };

    public string trace { get; set; } = "";
    public PlaywrightOptions()
    {
        this.browserOptions = new BrowserTypeLaunchOptions();
        this.contextOptions = contextOptions ?? new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            RecordVideoDir = "videos"
        };
        this.browserType = Browser.Chrome;
    }
    public async ValueTask<IBrowser> createBrowser(IPlaywright playwright)
    {
        var opt = this;

        IBrowser? browser = null;

        switch (opt.browserType)
        {
            case PlaywrightOptions.Browser.Chrome:
                browser = await playwright.Chromium.LaunchAsync(opt.browserOptions);
                break;
            case PlaywrightOptions.Browser.Edge:
                // channel msedge
                browser = await playwright.Chromium.LaunchAsync(opt.browserOptions);
                break;
            case PlaywrightOptions.Browser.Safari:
                browser = await playwright.Webkit.LaunchAsync(opt.browserOptions);
                break;
            case PlaywrightOptions.Browser.Firefox:
                browser = await playwright.Firefox.LaunchAsync(opt.browserOptions);
                break;
        }

        if (browser == null)
        {
            throw new Exception("Browser not found");
        }
        return browser!;
    }

    public async ValueTask<IBrowserContext> createContext(IBrowser browser)
    {
        var context = await browser.NewContextAsync(contextOptions);
        if (context == null) throw new Exception("Playwright not available");

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
        });
        return context;
    }
}