# Welcome to Datagrove Testing

**Very unstable and in active development**

This is a growing collection of tools in a variety of languages. Most notably we currently have pepinillo and selenium_playwright.

We are actively developing this to use on our own projects. We welcome issues and pull requests, we will help you if we can. Help us make a better world through better software.

# Pepinillo 
A command line tool to convert Cucumber/Gherkin to compilable programs. Currently supports Mstest and dotnet, but file an issue or pull request if you need another language. If you use BDD, then pepinillo is just like gherkin with the advantages of ahead-of-time compilation. If you have a testing project you want to move out of BDD, you can use pepinillo to convert your existing tests to work without gherkin.

# Selenium_playwright
This library makes playwright work compatibly with programs written for Selenium. Currently only dotnet is supported. 

# Pepin Dashboard
The dashboard converts mstest trx logs into a static website. We use it with cloudflare pages, but you can use the host of your choice.

# Dotnet tools and libraries

Obvious, but [install dotnet first](dotnet.microsoft.com)

## Install Pepinillo

```
*TODO* doesn't work
dotnet new pepinillo --name "pepinillo"
cd pepinillio
dotnet test
```

[More](dotnet/pepin.md)

## Install Selenium_playwright

**todo, there is a playwright installation issue to address**
```
dotnet new pepinillo.playwright --name "pepin_play"
cd pepin_play
dotnet test
```

Build pepin from source
```
dotnet tool uninstall testing.pepin -g
dotnet pack dotnet/src/Testing.Pepin
dotnet tool install --global --add-source dotnet/src/Testing.Pepin/nupkg Testing.Pepin
```


[More](dotnet/selenium_playwright.md)


