namespace RocketMoonApp.Server.Models
{
    public class LaunchSuccessAnalysisResponse
    {
        public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public int LaunchCount { get; set; }

        public List<PhaseSuccessRate> FourPhase { get; set; } = new();

        public List<PhaseSuccessRate> EightPhase { get; set; } = new();
    }
}
