using Microsoft.AspNetCore.Mvc;
using RocketMoonApp.Server.Models;
using RocketMoonApp.Server.Services;

namespace RocketMoonApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly AnalysisService _analysisService;

        public AnalysisController(AnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpGet("success-rates")]
        [ProducesResponseType(typeof(LaunchSuccessAnalysisResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<LaunchSuccessAnalysisResponse>> GetSuccessRates([FromQuery] int months = 6)
        {
            if (months <= 0 || months > 36)
            {
                months = 6;
            }

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-months);

            var response = await _analysisService.GetLaunchSuccessByMoonPhaseAsync(startDate, endDate);
            return Ok(response);
        }
    }
}
