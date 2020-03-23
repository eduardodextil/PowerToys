using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace PowerToysTests
{
    public class PowerToysSession
    {
        protected const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        protected static WindowsDriver<WindowsElement> session;
        protected static bool isPowerToysLaunched = false;
        protected static WindowsElement trayButton;

        protected static string _settingsFolderPath = "";
        protected static string _settingsPath = ""; 
        protected static string _zoneSettingsPath = "";
        protected static string _initialSettings = "";
        protected static string _initialZoneSettings = "";

        public static void Setup(TestContext context, bool isLaunchRequired = true)
        {
            //read settings before running tests to restore them after
            _settingsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\PowerToys\\FancyZones");
            _settingsPath = _settingsFolderPath + "\\settings.json";
            _zoneSettingsPath = _settingsFolderPath + "\\zones-settings.json";
            try
            {
                _initialSettings = File.ReadAllText(_settingsPath);
                _initialZoneSettings = File.ReadAllText(_zoneSettingsPath);
            }
            catch(Exception)
            {
                //failed to read settings
            }
            

            if (session == null)
            {
                // Create a new Desktop session to use PowerToys.
                AppiumOptions appiumOptions = new AppiumOptions();
                appiumOptions.PlatformName = "Windows";
                appiumOptions.AddAdditionalCapability("app", "Root");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appiumOptions);
                Assert.IsNotNull(session);

                trayButton = session.FindElementByAccessibilityId("1502");

                isPowerToysLaunched = CheckPowerToysLaunched();
                if (!isPowerToysLaunched && isLaunchRequired)
                {
                    LaunchPowerToys();
                }
            }

        }

        public static void TearDown()
        {
            //restore initial settings files
            if (_initialSettings.Length > 0)
            {
                File.WriteAllText(_settingsPath, _initialSettings);
            }
            else
            {
                File.Delete(_settingsPath);
            }

            if (_initialZoneSettings.Length > 0)
            {
                File.WriteAllText(_zoneSettingsPath, _initialZoneSettings);
            }
            else
            {
                File.Delete(_zoneSettingsPath);
            }

            if (session != null)
            {
                session.Quit();
                session = null;
            }
        }

        public static void WaitSeconds(double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public static void ShortWait()
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }

        //Trying to find element by XPath
        protected WindowsElement WaitElementByXPath(string xPath, double maxTime = 10)
        {
            WindowsElement result = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (timer.Elapsed < TimeSpan.FromSeconds(maxTime))
            {
                try
                {
                    result = session.FindElementByXPath(xPath);
                }
                catch { }
                if (result != null)
                {
                    return result;
                }
            }
            Assert.IsNotNull(result);
            return null;
        }

        //Trying to find element by AccessibilityId
        protected WindowsElement WaitElementByAccessibilityId(string accessibilityId, double maxTime = 10)
        {
            WindowsElement result = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (timer.Elapsed < TimeSpan.FromSeconds(maxTime))
            {
                try
                {
                    result = session.FindElementByAccessibilityId(accessibilityId);
                }
                catch { }
                if (result != null)
                {
                    return result;
                }
            }
            Assert.IsNotNull(result);
            return null;
        }

        public static void OpenSettings()
        {
            trayButton.Click();
            session.FindElementByXPath("//Button[@Name=\"PowerToys\"]").Click();
            trayButton.Click();
        }

        public static void OpenFancyZonesSettings()
        {
            WindowsElement fzNavigationButton = session.FindElementByXPath("//Button[@Name=\"FancyZones\"]");
            Assert.IsNotNull(fzNavigationButton);

            fzNavigationButton.Click();
            fzNavigationButton.Click();

            ShortWait();
        }

        public static void CloseSettings()
        {
            try
            {
                WindowsElement settings = session.FindElementByName("PowerToys Settings");
                if (settings != null)
                {
                    settings.SendKeys(Keys.Alt + Keys.F4);
                }
            }
            catch(Exception)
            {

            }
        }

        private static bool CheckPowerToysLaunched()        
        {
            trayButton.Click();
            bool isLaunched = false;

            try
            {
                WindowsElement pt = session.FindElementByXPath("//Button[@Name=\"PowerToys\"]");
                isLaunched = (pt != null);
            }
            catch(OpenQA.Selenium.WebDriverException)
            {
                //PowerToys not found
            }

            trayButton.Click(); //close
            return isLaunched;
        }

        public static void LaunchPowerToys()
        {
            try
            {
                AppiumOptions opts = new AppiumOptions();
                opts.PlatformName = "Windows";
                opts.AddAdditionalCapability("app", "Microsoft.PowerToys_8wekyb3d8bbwe!PowerToys");
                
                WindowsDriver<WindowsElement> driver = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), opts);
                Assert.IsNotNull(driver);
                driver.LaunchApp();
                isPowerToysLaunched = true;
            }
            catch (OpenQA.Selenium.WebDriverException)
            {
                //exception could be thrown even if app launched successfully
            }
        }

        public static void ExitPowerToys()
        {
            trayButton.Click();
            ShortWait();

            WindowsElement pt = session.FindElementByXPath("//Button[@Name=\"PowerToys\"]");
            new Actions(session).MoveToElement(pt).ContextClick().Perform();
            ShortWait();

            session.FindElementByXPath("//MenuItem[@Name=\"Exit\"]").Click();
            trayButton.Click(); //close tray
            isPowerToysLaunched = false;
        }
    }
}
