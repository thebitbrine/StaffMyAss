using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StaffMyAss
{
    class Program
    {
        static IWebDriver driver;
        static void Main(string[] args)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("user-data-dir=User Data");
            driver = (IWebDriver)new ChromeDriver(chromeOptions);

            Console.Clear();

            Console.WriteLine("Starting...");
            List<string> potlink = new List<string>();
            potlink.AddRange(File.ReadAllLines("Linx.txt"));
            Console.WriteLine("Loaded Linx.txt.");
            for (int i = 1; i < 10; i++)
            {

                driver.Navigate().GoToUrl($"https://staff.am/en/jobs/categories/index?JobsFilter%5Bkey_word%5D=&JobsFilter%5Bcategory%5D%5B0%5D=1&JobsFilter%5Bjob_city%5D%5B0%5D=1&page={i}");
                var source = driver.PageSource;
                string re1 = "((?:\\/[\\w\\.\\-]+)+)";

                Regex r = new Regex(re1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = r.Matches(source);

                string[] filter = new[] { "/en/jobs", "/en/terms-of-use", "/en/privacy-policy", "/en/trainings", "/en/companies", "/en/goverment", "/en/our-packages", "/en/training-packages", "/en/about", "/en/how-we-work", "/en/how-to-verify-skills", "/en/aptis-testing", "/en/hot-jobs" };
                foreach (var match in m)
                {
                    if (match.ToString().StartsWith("/en/") && match.ToString().ToCharArray().Where(x => x == '/').Count() == 2 && !potlink.Contains(match.ToString()) && !filter.Contains(match.ToString()))
                    {
                        potlink.Add(match.ToString());
                    }
                }
                Console.WriteLine($"Page {i}: Scraping done.");
            }

            potlink = potlink.OrderBy(y => y.ToString()).ToList();
            File.WriteAllLines("Linx.txt", potlink.ToArray());
            Console.WriteLine("Saved Linx.txt.");

            List<string> Emails = new List<string>();
            Emails.AddRange(File.ReadAllLines("Emails.csv"));
            Console.WriteLine("Loaded Emails.csv");
            foreach (var Listing in potlink)
            {
                Console.WriteLine($"Scraping {Listing.Split('/').Last()}");
                driver.Navigate().GoToUrl($"https://staff.am{Listing}");
                var pagesource = driver.PageSource;

                Match m = new Regex("([\\w-+]+(?:\\.[\\w-+]+)*@(?:[\\w-]+\\.)+[a-zA-Z]{2,7})", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(pagesource);
                if (m.Success)
                {
                    var em = m.Groups[1].ToString();
                    if (!Emails.Contains(em))
                    {
                        Emails.Add(em);
                        Console.WriteLine($"Found: {em}");
                    }
                }
                Console.WriteLine("No new emails found.");
            }

            Emails = Emails.OrderBy(z => z.ToString()).ToList();
            File.WriteAllLines("Emails.csv", Emails);
            Console.WriteLine("Saved Emails.csv");
            Console.WriteLine("\nDone, exiting...");
            
        }
    }
}
