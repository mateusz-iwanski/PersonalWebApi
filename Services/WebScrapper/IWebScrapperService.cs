using PersonalWebApi.Agent.SemanticKernel.Plugins.ScrapperPlugin;

namespace PersonalWebApi.Services.WebScrapper
{
    public interface IWebScrapperService
    {
        Task<string> MapPageAsync(string url, string? search = null, bool ignoreSitemap = false, bool sitemapOnly = false, bool includeSubdomains = true, int limit = 5000);
        Task<string> ScrapePageAsync(string url, string[]? formats = null, bool onlyMainContent = true, string[] includeTags = null, string[] excludeTags = null, int waitFor = 0, bool mobile = false, bool skipTlsVerification = false, int timeout = 30000, bool removeBase64Images = true, WebScrapperPlugin.ActionWaitMiliseconds actionWaitMiliseconds = null, WebScrapperPlugin.ActionWaitSelector actionWaitSelector = null);
    }
}