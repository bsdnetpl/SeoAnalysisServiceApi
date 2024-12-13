using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoAnalysisServiceApi.Models;
using SeoAnalysisServiceApi.Services;

namespace SeoAnalysisServiceApi.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]
    public class SeoAnalysisController : ControllerBase
        {
        private readonly ISeoAnalysisService _seoAnalysisService;

        public SeoAnalysisController(ISeoAnalysisService seoAnalysisService)
            {
            _seoAnalysisService = seoAnalysisService;
            }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzePage([FromBody] SeoAnalysisRequest request)
            {
            if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.Keyword))
                {
                return BadRequest("URL and Keyword are required.");
                }

            var result = await _seoAnalysisService.AnalyzePageAsync(request.Url, request.Keyword);

            return Ok(result);
            }
        }
    }
