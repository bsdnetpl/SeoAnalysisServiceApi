using SeoAnalysisServiceApi.Models;

namespace SeoAnalysisServiceApi.Services
    {
    public interface ISeoAnalysisService
        {
        Task<SeoAnalysisResult> AnalyzePageAsync(string url, string keyword);
        }
    }