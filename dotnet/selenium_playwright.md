# Selenium_playwright


## using selenium_playwright in an existing dotnet test project
Install
```
dotnet add package selenium_playwright
```
Now you need to change the namespace from OpenQA.Selenium to Datagrove.Selenium.

## Boa constrictor

Boa constrictor is a popular add on to Selenium. To use Boa you will need to use the Datagrove.Boa namespace in place of Boa.Constrictor.Selenium.


# Why?

In our experience Playwright compared to Selenium will
1. Run faster
2. Run easier in CI pipelines

If you have a significant base of Selenium code, this library will let you move it to Playwright easily.

