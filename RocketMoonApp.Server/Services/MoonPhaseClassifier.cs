using RocketMoonApp.Server.Models;
using System.Globalization;

namespace RocketMoonApp.Server.Services
{
    public static class MoonPhaseClassifier
    {
        private static readonly string[] EightPhaseOrder =
        [
            "New Moon",
            "Waxing Crescent",
            "First Quarter",
            "Waxing Gibbous",
            "Full Moon",
            "Waning Gibbous",
            "Last Quarter",
            "Waning Crescent"
        ];

        private static readonly Dictionary<string, string> PhaseAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            { "New", "New Moon" },
            { "New Moon", "New Moon" },
            { "Waxing Crescent", "Waxing Crescent" },
            { "First Quarter", "First Quarter" },
            { "Waxing Gibbous", "Waxing Gibbous" },
            { "Full", "Full Moon" },
            { "Full Moon", "Full Moon" },
            { "Waning Gibbous", "Waning Gibbous" },
            { "Last Quarter", "Last Quarter" },
            { "Third Quarter", "Last Quarter" },
            { "Waning Crescent", "Waning Crescent" }
        };

        public static string ClassifyToEightPhase(string phaseName, DateTimeOffset launchDate)
        {
            if (PhaseAliases.TryGetValue(phaseName, out var normalized))
            {
                return normalized;
            }

            return CalculateAstronomicalPhase(launchDate);
        }

        public static string ClassifyToFourPhase(string eightPhaseName)
        {
            return eightPhaseName switch
            {
                "New Moon" or "Waxing Crescent" or "Waning Crescent" => "New Moon",
                "First Quarter" or "Waxing Gibbous" => "First Quarter",
                "Full Moon" or "Waning Gibbous" => "Full Moon",
                "Last Quarter" => "Last Quarter",
                _ => "Unknown"
            };
        }

        private static string CalculateAstronomicalPhase(DateTimeOffset date)
        {
            // Algorithm adapted from https://en.wikipedia.org/wiki/Lunar_phase#Lunar_phase_calculation
            var knownNewMoon = new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);
            var synodicMonth = 29.530588853;
            var daysSinceKnownNewMoon = (date.UtcDateTime - knownNewMoon).TotalDays;
            var normalizedDays = daysSinceKnownNewMoon % synodicMonth;
            if (normalizedDays < 0)
            {
                normalizedDays += synodicMonth;
            }

            var phaseIndex = (int)Math.Round((normalizedDays / synodicMonth) * 8) % 8;
            return EightPhaseOrder[phaseIndex];
        }
    }
}
