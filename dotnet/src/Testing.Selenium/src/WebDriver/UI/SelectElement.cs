

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Datagrove.Testing.Selenium;
using Microsoft.Playwright;
/*
So for text selector, there are two behaviours, that you can look into:

text=Log in - default matching is case-insensitive and searches for a substring.
text="Log in" - text body can be escaped with single or double quotes to search for a text node with exact content.
So to search for a substring you can use the string without the double quotes:

myElement = self.page.locator('text=Some Text 123')
*/

namespace Datagrove.Testing.Selenium
{
    /// <summary>
    /// Provides a convenience method for manipulating selections of options in an HTML select element.
    /// </summary>
    public class SelectElement
    {
        public PWebElement element;
        public bool IsMultiple { get; private set; }

        public SelectElement(IWebElement element)
        {
            this.element = (PWebElement)element;
        }


        public IList<IWebElement> Options
        {
            get
            {
                return this.element.FindElements(By.TagName("option"));
            }
        }
        public IWebElement SelectedOption
        {
            get
            {
                foreach (IWebElement option in this.Options)
                {
                    if (option.Selected)
                    {
                        return option;
                    }
                }

                throw new NoSuchElementException("No option is selected");
            }
        }

        /// <summary>
        /// Gets all of the selected options within the select element.
        /// </summary>
        public IList<IWebElement> AllSelectedOptions
        {
            get
            {
                List<IWebElement> returnValue = new List<IWebElement>();
                foreach (IWebElement option in this.Options)
                {
                    if (option.Selected)
                    {
                        returnValue.Add(option);
                    }
                }

                return returnValue;
            }
        }

        public void SelectByText(string text, bool partialMatch = false)
        {
            element.driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                // there's no point in waiting here, and we want to throw a 
                var s="";
                try
                {
                    s = await element.h.InnerHTMLAsync();
                    await element.h.SelectOptionAsync(new SelectOptionValue()
                    {
                        Label = text,
                    }, new ElementHandleSelectOptionOptions()
                    {
                        Timeout = 30000,
                    });
                }
                catch (Exception)
                {
                    throw new NoSuchElementException($"No option with text '{text}' found {s}");
                }
                return true;
            });
        }


        public void SelectByValue(string value)
        {
            element.driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                await element.h.SelectOptionAsync(new SelectOptionValue()
                {
                    Value = value,
                });
                return true;
            });
        }


        public void SelectByIndex(int index)
        {
            element.driver.exec<bool>(async Task<object> (PlaywrightDriver p) =>
            {
                await element.h.SelectOptionAsync(new SelectOptionValue()
                {
                    Index = index,
                });
                return true;
            });
        }


        public void DeselectAll()
        {
            SelectByIndex(-1);
        }


        public void DeselectByText(string text)
        {
            throw new NotImplementedException();
        }

        public void DeselectByValue(string value)
        {
            throw new NotImplementedException();
        }
    }
}
