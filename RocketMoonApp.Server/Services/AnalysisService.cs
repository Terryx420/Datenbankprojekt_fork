using Microsoft.Extensions.Logging;
using RocketMoonApp.Server.Models;

namespace RocketMoonApp.Server.Services
{
    public class AnalysisService
    {
        private readonly LaunchService _launchService;
        private readonly MoonService _moonService;
        private readonly ILogger<AnalysisService> _logger;
        private readonly Dictionary<DateOnly, string> _phaseCache = new();

        public AnalysisService(LaunchService launchService, MoonService moonService, ILogger<AnalysisService> logger)
        {
            _launchService = launchService;
            _moonService = moonService;
            _logger = logger;
        }

        public async Task<LaunchSuccessAnalysisResponse> GetLaunchSuccessByMoonPhaseAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("Building success analysis for launches between {From} and {To}", startDate, endDate);

            var launches = await _launchService.GetLaunchesFromTimeframeAsync(startDate, endDate);
            var analyzedLaunches = new List<(Launch launch, string eightPhase)>();

            foreach (var launch in launches)
            {
                var phaseName = await GetPhaseNameForDateAsync(launch.Date);
                var eightPhase = MoonPhaseClassifier.ClassifyToEightPhase(phaseName, launch.Date);
                analyzedLaunches.Add((launch, eightPhase));
            }

            var completedLaunches = analyzedLaunches.Where(l => l.launch.WasSuccessful.HasValue).ToList();

            var eightPhaseRates = BuildPhaseRates(completedLaunches, groupingSelector: l => l.eightPhase);
            var fourPhaseRates = BuildPhaseRates(completedLaunches, groupingSelector: l => MoonPhaseClassifier.ClassifyToFourPhase(l.eightPhase));

            return new LaunchSuccessAnalysisResponse
            {
                GeneratedAt = DateTimeOffset.UtcNow,
                From = startDate,
                To = endDate,
                LaunchCount = completedLaunches.Count,
                FourPhase = fourPhaseRates,
                EightPhase = eightPhaseRates
            };
        }

        private async Task<string> GetPhaseNameForDateAsync(DateTimeOffset date)
        {
            var key = DateOnly.FromDateTime(date.UtcDateTime.Date);
            if (_phaseCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var phase = await _moonService.GetMoonPhaseForDayAsync(date.UtcDateTime);
            var normalized = string.IsNullOrWhiteSpace(phase.PhaseName) ? "Unknown" : phase.PhaseName.Trim();
            _phaseCache[key] = normalized;
            return normalized;
        }

        private static List<PhaseSuccessRate> BuildPhaseRates(IEnumerable<(Launch launch, string phase)> launches, Func<(Launch launch, string phase), string> groupingSelector)
        {
            return launches
                .GroupBy(groupingSelector)
                .Select(group => new PhaseSuccessRate
                {
                    Phase = group.Key,
                    LaunchCount = group.Count(),
                    SuccessCount = group.Count(l => l.launch.WasSuccessful == true),
                    FailureCount = group.Count(l => l.launch.WasSuccessful == false)
                })
                .OrderBy(rate => rate.Phase)
                .ToList();
        }
    }
}
