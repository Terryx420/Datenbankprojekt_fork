using Microsoft.Extensions.Logging;
using RocketMoonApp.Server.Models;
using RocketMoonApp.Server.Services;
using System.Net;
using Xunit;

namespace RocketMoonApp.Server.Tests
{
    public class AnalysisServiceTests
    {
        [Fact]
        public async Task CalculatesSuccessRatesPerPhase()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var launches = new List<Launch>
            {
                new()
                {
                    Id = "1",
                    Date = new DateTimeOffset(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)),
                    WasSuccessful = true,
                    Status = "Success"
                },
                new()
                {
                    Id = "2",
                    Date = new DateTimeOffset(new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc)),
                    WasSuccessful = false,
                    Status = "Failure"
                },
                new()
                {
                    Id = "3",
                    Date = new DateTimeOffset(new DateTime(2025, 1, 3, 14, 0, 0, DateTimeKind.Utc)),
                    WasSuccessful = true,
                    Status = "Success"
                }
            };

            var phaseMap = new Dictionary<DateOnly, string>
            {
                { DateOnly.FromDateTime(new DateTime(2025, 1, 1)), "Waxing Crescent" },
                { DateOnly.FromDateTime(new DateTime(2025, 1, 2)), "Full Moon" },
                { DateOnly.FromDateTime(new DateTime(2025, 1, 3)), "First Quarter" }
            };

            var launchService = new FakeLaunchService(launches, loggerFactory.CreateLogger<LaunchService>());
            var moonService = new FakeMoonService(phaseMap, loggerFactory.CreateLogger<MoonService>());
            var analysisService = new AnalysisService(launchService, moonService, loggerFactory.CreateLogger<AnalysisService>());

            var result = await analysisService.GetLaunchSuccessByMoonPhaseAsync(new DateTime(2025, 1, 1), new DateTime(2025, 2, 1));

            Assert.Equal(3, result.LaunchCount);

            var eightPhaseFull = result.EightPhase.First(r => r.Phase == "Full Moon");
            Assert.Equal(1, eightPhaseFull.LaunchCount);
            Assert.Equal(0, eightPhaseFull.SuccessCount);

            var fourPhaseFull = result.FourPhase.First(r => r.Phase == "Full Moon");
            Assert.Equal(1, fourPhaseFull.FailureCount);
            Assert.Equal(0, fourPhaseFull.SuccessCount);

            var newPhaseBucket = result.FourPhase.First(r => r.Phase == "New Moon");
            Assert.Equal(1, newPhaseBucket.SuccessCount);
        }
    }

    internal class FakeLaunchService : LaunchService
    {
        private readonly List<Launch> _launches;

        public FakeLaunchService(List<Launch> launches, ILogger<LaunchService> logger)
            : base(new HttpClient(new MockHttpMessageHandler()), logger)
        {
            _launches = launches;
        }

        public override Task<List<Launch>> GetLaunchesFromTimeframeAsync(DateTime startingDate, DateTime endingDate)
        {
            return Task.FromResult(_launches);
        }
    }

    internal class FakeMoonService : MoonService
    {
        private readonly Dictionary<DateOnly, string> _phaseMap;

        public FakeMoonService(Dictionary<DateOnly, string> phaseMap, ILogger<MoonService> logger)
            : base(new HttpClient(new MockHttpMessageHandler()), logger)
        {
            _phaseMap = phaseMap;
        }

        public override Task<MoonPhase> GetMoonPhaseForDayAsync(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date.Date);
            if (_phaseMap.TryGetValue(dateOnly, out var phase))
            {
                return Task.FromResult(new MoonPhase { Date = date, PhaseName = phase });
            }

            return Task.FromResult(new MoonPhase { Date = date, PhaseName = "Unknown" });
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            });
        }
    }
}
