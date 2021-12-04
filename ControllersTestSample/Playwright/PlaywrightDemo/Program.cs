using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace PlaywrightDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var playwright = await Playwright.CreateAsync())
            {
                await using (var browser = await playwright.Firefox.LaunchAsync(new() { Headless = false }))
                {
                    var page = await browser.NewPageAsync();
                    await page.GotoAsync("https://playwright.dev/dotnet");
                    var className = await page.EvalOnSelectorAsync("h1", "h1=>h1.className");
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = "sample.png" });
                }
            }
        }
    }
}
