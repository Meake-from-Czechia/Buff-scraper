using System;
using System.Runtime.CompilerServices;
using PuppeteerSharp;

namespace vex_shit_v2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string activeLink;
            
            double[] price = new double[2];
            Random randNum = new Random();
            int[] randLinks = Enumerable
                .Repeat(0, 10000)
                .Select(i => randNum.Next(1, 20000))
                .ToArray();
            FetchBrowser().GetAwaiter().GetResult();
            foreach (int i in randLinks)
            {
                Console.Write($"{i}: ");
                activeLink = $"https://buff.market/market/goods/{i}?game=csgo";
                GetSteamPrices(activeLink).GetAwaiter().GetResult();
            }
        }
        public static async Task FetchBrowser()
        {
            await Console.Out.WriteAsync("Downloading browser... ");
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            ColorWrite("Done. \n", ConsoleColor.Green);
        }
        public static async Task GetSteamPrices(string activeLink)
        {
            string name;
            double[] price = new double[2];
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();
            page.DefaultTimeout = 30000;
            await Console.Out.WriteAsync(".");
            try
            {
                await page.GoToAsync(activeLink);
                await Console.Out.WriteAsync(".");
                await page.WaitForNetworkIdleAsync();
                //await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                ColorWrite(ex.Message+ '\n', ConsoleColor.Red);
            }
            try
            {
                const string steamPriceEval = @"Array.from(document.querySelectorAll('.steam-price span')).map(price => price.textContent);";
                const string lowestPriceEval = @"Array.from(document.querySelectorAll('.buff')).map(price => price.textContent);";
                const string nameEval = @"Array.from(document.querySelectorAll('.name')).map(name => name.textContent);";
                string[] steamPrices = await page.EvaluateExpressionAsync<string[]>(steamPriceEval);
                if (steamPrices.Length == 0) price[0] = 0; else price[0] = CleanUpPrice(steamPrices[0]);
                string[] lowestPrices = await page.EvaluateExpressionAsync<string[]>(lowestPriceEval);
                if (lowestPrices.Length == 0) price[1] = 999999; else price[1] = CleanUpPrice(lowestPrices[0]);
                name = (await page.EvaluateExpressionAsync<string[]>(nameEval))[0];
                await Console.Out.WriteAsync(". ");
                if (price[1] / price[0] < 0.95 && price[0] - price[1] > 1)
                {
                    ColorWrite("\n|=======-Nalezen předmět-=======|\n", ConsoleColor.Green);
                    ColorWrite($"Jméno: {name}\n", ConsoleColor.Yellow);
                    Console.WriteLine($"Podíl: {price[1] / price[0]}");
                    Console.WriteLine($"Link: {activeLink}");
                }  
            }
            catch 
            {

                ColorWrite(" Invalid prices.\n", ConsoleColor.Red);
            }
            await browser.CloseAsync();
        }
        public static double CleanUpPrice(string price)
        {
            return double.Parse(price.Substring(1).Replace('.', ','));
        }
        public static void ColorWrite(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(msg);
            Console.ResetColor();
        }
    }
}