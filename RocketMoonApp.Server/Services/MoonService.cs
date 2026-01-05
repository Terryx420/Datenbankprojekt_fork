using Microsoft.Extensions.Logging;
using RocketMoonApp.Server.Models;
using System.Globalization;
using System.Text.Json;

namespace RocketMoonApp.Server.Services
{
    public class MoonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoonService> _logger;

        public MoonService(HttpClient httpClient, ILogger<MoonService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public virtual async Task<List<MoonPhase>> GetMoonPhasesForYearAsync(int year)
        {
            try
            {
                _logger.LogInformation("Fetching moon phases for year {Year}", year);

                var url = $"https://aa.usno.navy.mil/api/moon/phases/year?year={year}";

                _logger.LogInformation("Making API call to URL: {Url}", url);

                var response = await _httpClient.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);

                var phases = data.GetProperty("phasedata").EnumerateArray()
                    .Select(phase => new MoonPhase
                    {
                        PhaseName = phase.GetProperty("phase").GetString() ?? string.Empty,
                        Date = new DateTime(
                            phase.GetProperty("year").GetInt32(),
                            phase.GetProperty("month").GetInt32(),
                            phase.GetProperty("day").GetInt32())
                    })
                    .ToList();

                _logger.LogInformation("Successfully fetched {PhaseCount} moon phases", phases.Count);
                return phases;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching moon phases");
                return new List<MoonPhase>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error occurred while processing moon phases");
                return new List<MoonPhase>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching moon phases");
                return new List<MoonPhase>();
            }
        }

        public virtual async Task<MoonPhase> GetMoonPhaseForDayAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Fetching moon phase for date {Date}", date);

                var formattedDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var url = $"https://aa.usno.navy.mil/api/rstt/oneday?date={formattedDate}";

                _logger.LogInformation("Making API call to URL: {Url}", url);

                var response = await _httpClient.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);

                var properties = data.GetProperty("properties");

                var moonPhase = new MoonPhase
                {
                    PhaseName = properties.GetProperty("curphase").GetString() ?? string.Empty,
                    Date = date
                };

                _logger.LogInformation("Successfully fetched moon phase: {PhaseName}", moonPhase.PhaseName);
                return moonPhase;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching moon phase");
                return new MoonPhase();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error occurred while processing moon phase");
                return new MoonPhase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching moon phase");
                return new MoonPhase();
            }
        }
    }
}
