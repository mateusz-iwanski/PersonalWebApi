using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Newtonsoft.Json;
using PersonalWebApi.Utilities.WebScrappers;
using PersonalWebApi.Services.WebScrapper;

namespace PersonalWebApi.Agent.SemanticKernel.Plugins.ScrapperPlugin
{
    public class WebScrapperPlugin
    {
        public class ActionWaitMiliseconds
        {
            private int _milliseconds = 1;
            public string type { get => "wait"; }

            [Description("Wait for a specified amount of milliseconds. You can use only if selector is null. Must be greater than 0")]
            public int milliseconds
            {
                get => _milliseconds;
                set
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("milliseconds", "The milliseconds value must be greater than 0.");
                    }
                    _milliseconds = value;
                }
            }
        }

        public class ActionWaitSelector
        {
            public string type { get => "wait"; }
            [Description("Query selector to find the element by. You can use only milliseconds type is null")]
            public string selector { get; set; }
        }

        public class WebScrapingPlugin(
            IWebScrapperService webSpracping
            )
        {
            [KernelFunction("scraping_we_page")]
            [Description("Scrapes the content of a web page using an external online service.")]
            public async Task<string> ScrapingPageAsync(

                [Description("The URL to scrape. Required parameter.")]
                string url,

                [Description("Formats to include in the output. " +
                        "Available options: markdown, html, rawHtml, links, screenshot, extract, screenshot@fullPage. " +
                        "Default is markdown.")]
                string[]? formats = null,

                [Description("Only return the main content of the page excluding headers, navs, footers, etc. Default is true.")]
                bool onlyMainContent = true,

                [Description("Tags to include in the output.")]
                string[] includeTags = null,

                [Description("Tags to exclude from the output.")]
                string[] excludeTags = null,

                //[Description("Headers to send with the request. Can be used to send cookies, user-agent, etc.")]
                //List<> headers = null,

                [Description("Specify a delay in milliseconds before fetching the content, allowing the page sufficient time to load. Default is 0.")]
                int waitFor = 0,

                [Description("Set to true if you want to emulate scraping from a mobile device. Useful for testing responsive pages and taking mobile screenshots. Default is false.")]
                bool mobile = false,

                [Description("Skip TLS certificate verification when making requests. Default is false.")]
                bool skipTlsVerification = false,

                [Description("Timeout in milliseconds for the request. Default is 30000.")]
                int timeout = 30000,

                //[Description("Extract object containing extraction parameters.")]
                //object extract = null,

                [Description("Wait actions to perform on the page before grabbing the content. Wait for a specified amount of milliseconds. Use only if actionWaitSelector will be null")]
                ActionWaitMiliseconds actionWaitMiliseconds = null,

                [Description("Wait actions to perform on the page before grabbing the content. Query selector to find the element by. Use only if actionWait will be null")]
                ActionWaitSelector actionWaitSelector = null,

                [Description("Removes all base 64 images from the output, which may be overwhelmingly long. The image's alt text remains in the output, but the URL is replaced with a placeholder. Default is false.")]
                bool removeBase64Images = true
            )
            {
                // Define the supported formats
                var supportedFormats = new[] { "markdown", "html", "rawHtml", "links", "screenshot", "extract", "screenshot@fullPage" };

                // Check if the requested formats are supported
                if (formats != null && formats.Any(format => !supportedFormats.Contains(format)))
                {
                    return "The requested format(s) are not supported for scraping. Supported formats are: " +
                        string.Join(", ", supportedFormats);
                }

                // Call the updated ScrapingPageAsync method with the provided parameters
                var response = await webSpracping.ScrapePageAsync(
                    url,
                    formats,
                    onlyMainContent,
                    includeTags,
                    excludeTags,
                    //headers,
                    waitFor,
                    mobile,
                    skipTlsVerification,
                    timeout,
                    //extract,
                    //actions
                    removeBase64Images,
                    actionWaitMiliseconds,
                    actionWaitSelector
                );

                return response;
            }

            [KernelFunction("map_we_page")]
            [Description("" +
                "Map a website url and get all the urls on the website (extremely fast). On December 2024 Firecrawl map function is steal in demo mode, it can return wrong answers."
                )]
            public async Task<string> MapPageAsync(
                [Description("Url/Link/Uri page to scrap")] string url,
                [Description("Filter for search")] string? search = null,
                [Description("Ignore the website sitemap when crawling. Default is false.")] bool ignoreSitemap = false,
                [Description("Only return links found in the website sitemap. Default is false.")] bool sitemapOnly = false,
                [Description("Include subdomains of the website. Default is false.")] bool includeSubdomains = false,
                [Description("Maximum number of links to return. Required range: x < 5000. Default is 5000.")][Range(1, 5000)] int limit = 5000
                )
            {

                var response = await webSpracping.MapPageAsync(url, search, ignoreSitemap, sitemapOnly, includeSubdomains, limit);

                var result = new
                {
                    success = true,
                    links = response
                };

                return JsonConvert.SerializeObject(result);
            }

        }
    }
}
