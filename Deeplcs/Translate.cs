using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;

namespace Deeplcs
{
    public class Translate
    {
        private static string[] Json()
        {
            if (!File.Exists("paths.json"))
            {
                Paths paths = new Paths();
                Console.WriteLine("Location of your browser: ");
                paths.Browser = Console.ReadLine();
                Console.WriteLine("Path to your driver: ");
                paths.Driver = Console.ReadLine();
                using (StreamWriter file = File.CreateText("paths.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, paths);
                }
            }
            Paths Paths = JsonConvert.DeserializeObject<Paths>(File.ReadAllText("paths.json"));
            string Browser = Paths.Browser;
            string Driver = Paths.Driver;
            return new string[] { Browser, Driver };
        }

        public static string[] translate(string targetLang, string[] stringArray, string[] srcTextArray = null, string[] targetTextArray = null)
        {
            string[] paths = Json();
            string Browser = paths[0];
            string Driver = paths[1];
            ChromeOptions options = new ChromeOptions();
            options.BinaryLocation = Browser;
            options.AddArguments("--disable-extensions", "--window-size=1920,1080", "--start-maximized", "--headless");
            IWebDriver driver = new ChromeDriver(Driver, options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            List<string> outputList = new List<string>();

            driver.Navigate().GoToUrl(@"https://deepl.com/translator");
            var languageMenuButtonElement = driver.FindElement(By.XPath("//*[@id='dl_translator']/div[1]/div[4]/div[1]/div[1]/div[1]/button"));
            languageMenuButtonElement.Click();
            Thread.Sleep(500);
            var languageMenuButtonElements = driver.FindElements(By.CssSelector(string.Format("button[dl-lang = {0}", targetLang)));
            languageMenuButtonElements[1].Click();
            if (srcTextArray != null)
            {
                var glossaryElement = driver.FindElement(By.XPath("//*[@id='glossaryButton']/button"));
                glossaryElement.Click();
                try
                {
                    var element = wait.Until(drv => drv.FindElement(By.XPath("//*[@id='glossaryButton']/button")));
                }
                finally
                {
                    for (int i = 0; i < srcTextArray.Length; i++)
                    {
                        var srcTextElement = driver.FindElement(By.XPath("//*[@id='glossaryEditor']/div[4]/form/div[1]/input[1]"));
                        srcTextElement.SendKeys(srcTextArray[i]);
                        var targetTextElement = driver.FindElement(By.XPath("//*[@id='glossaryEditor']/div[4]/form/div[1]/input[2]"));
                        targetTextElement.SendKeys(targetTextArray[i]);
                        var glossaryAcceptElement = driver.FindElement(By.XPath("//*[@id='glossaryEditor']/div[4]/form/div[1]/button[2]"));
                        glossaryAcceptElement.Click();
                    }
                    var glossaryCloseElement = driver.FindElement(By.XPath("//*[@id='glossaryEditor']/div[2]/div[3]"));
                    glossaryCloseElement.Click();
                }
            }
            for (int i = 0; i < stringArray.Length; i++)
            {
                try
                {
                    var clearElement = driver.FindElement(By.XPath("//*[@id='dl_translator']/div[1]/div[3]/div[2]/button"));
                    clearElement.Click();
                }
                catch
                {
                    { }
                }
                var textInputElement = driver.FindElement(By.XPath("//*[@id='dl_translator']/div[1]/div[3]/div[2]/div/textarea"));
                textInputElement.SendKeys(stringArray[i]);
                while (driver.FindElement(By.XPath("//*[@id='dl_translator']/div[1]/div[6]")).GetAttribute("class") == "lmt__mobile_share_container lmt__mobile_share_container--inactive")
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(100);
                var outputElements = driver.FindElements(By.CssSelector("button[class='lmt__translations_as_text__text_btn']"));
                outputList.Add(outputElements[0].GetAttribute("innerHTML"));
            }
            driver.Quit();
            return outputList.ToArray();
        }
    }
}
