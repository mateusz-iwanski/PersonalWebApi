using Microsoft.KernelMemory.DataFormats.WebPages;
using PersonalWebApi.Utilities.WebScrapper;
using PersonalWebApi.Utilities.WebScrappers;
using static PersonalWebApi.Agent.SemanticKernel.Plugins.ScrapperPlugin.WebScrapperPlugin;

namespace PersonalWebApi.Services.WebScrapper
{
    public class WebScrapperService : IWebScrapperService
    {
        private readonly IWebScrapperClient _webScraper;

        public WebScrapperService(IWebScrapperClient webScraper)
        {
            _webScraper = webScraper;
        }

        public async Task<string> ScrapePageAsync(
            string url,
            string[]? formats = null,
            bool onlyMainContent = true,
            string[] includeTags = null,
            string[] excludeTags = null,
            int waitFor = 0,
            bool mobile = false,
            bool skipTlsVerification = false,
            int timeout = 30000,
            bool removeBase64Images = true,
            ActionWaitMiliseconds actionWaitMiliseconds = null,
            ActionWaitSelector actionWaitSelector = null)
        {
            return await _webScraper.ScrapingPageAsync(
                url,
                formats,
                onlyMainContent,
                includeTags,
                excludeTags,
                waitFor,
                mobile,
                skipTlsVerification,
                timeout,
                removeBase64Images,
                actionWaitMiliseconds,
                actionWaitSelector);
        }

        public async Task<string> MapPageAsync(
            string url,
            string? search = null,
            bool ignoreSitemap = false,
            bool sitemapOnly = false,
            bool includeSubdomains = true,
            int limit = 5000)
        {
            return await _webScraper.MapPageAsync(
                url,
                search,
                ignoreSitemap,
                sitemapOnly,
                includeSubdomains,
                limit);
        }
    }
}
