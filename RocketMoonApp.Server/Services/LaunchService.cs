using Microsoft.Extensions.Logging;
using RocketMoonApp.Server.Models;
using System.Text.Json;

namespace RocketMoonApp.Server.Services
{
    public class LaunchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LaunchService> _logger;

        public LaunchService(HttpClient httpClient, ILogger<LaunchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public virtual async Task<List<Launch>> GetLaunchesFromTimeframeAsync(DateTime startingDate, DateTime endingDate)
        {
            try
            {
                _logger.LogInformation("Fetching launches from {StartingDate} to {EndingDate}", startingDate, endingDate);

                List<Launch> launches = new();
                var startDate = startingDate.ToString("yyyy-MM-dd");
                var endDate = endingDate.ToString("yyyy-MM-dd");

                var url = $"https://ll.thespacedevs.com/2.3.0/launch/?net__gte={startDate}&net__lte={endDate}&limit=100";

                JsonElement data;

                do
                {
                    _logger.LogInformation("Making API call to URL: {Url}", url);

                    var response = await _httpClient.GetStringAsync(url);

                    data = JsonSerializer.Deserialize<JsonElement>(response);

                    var newlaunches = data.GetProperty("results").EnumerateArray()
                        .Select(launch => new Launch
                        {
                            Id = launch.GetProperty("id").GetString() ?? string.Empty,
                            RocketName = launch.GetProperty("rocket").GetProperty("configuration").GetProperty("name").GetString() ?? string.Empty,
                            Date = launch.GetProperty("net").GetDateTimeOffset(),
                            Location = new Location
                            {
                                Id = launch.GetProperty("pad").GetProperty("location").GetProperty("id").GetInt32(),
                                CountryName = launch.GetProperty("pad").GetProperty("country").GetProperty("name").GetString() ?? string.Empty,
                                Latitude = launch.GetProperty("pad").GetProperty("latitude").GetDouble().ToString(),
                                Longitude = launch.GetProperty("pad").GetProperty("longitude").GetDouble().ToString(),
                            },
                            Status = launch.GetProperty("status").GetProperty("abbrev").GetString() ?? "Unknown",
                            WasSuccessful = MapToSuccess(launch.GetProperty("status").GetProperty("abbrev").GetString())
                        })
                        .ToList();

                    launches.AddRange(newlaunches);

                    url = data.GetProperty("next").GetString() ?? string.Empty;
                    _logger.LogInformation("Next URL: {NextUrl}", url);

                } while (!string.IsNullOrEmpty(url));

                _logger.LogInformation("Successfully fetched {LaunchCount} launches", launches.Count);
                return launches;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error occurred while fetching launches");
                return new List<Launch>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error occurred while processing launches");
                return new List<Launch>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching launches");
                return new List<Launch>();
            }
        }

        private static bool? MapToSuccess(string? statusAbbreviation)
        {
            if (string.IsNullOrWhiteSpace(statusAbbreviation))
            {
                return null;
            }

            var normalized = statusAbbreviation.ToLowerInvariant();

            if (normalized.Contains("success"))
            {
                return true;
            }

            if (normalized.Contains("failure"))
            {
                return false;
            }

            return null;
        }
    }
}
