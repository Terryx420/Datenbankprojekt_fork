namespace RocketMoonApp.Server.Models
{
    public class PhaseSuccessRate
    {
        public string Phase { get; set; } = string.Empty;

        public int LaunchCount { get; set; }

        public int SuccessCount { get; set; }

        public int FailureCount { get; set; }

        public double SuccessRate => LaunchCount == 0 ? 0 : Math.Round((double)SuccessCount / LaunchCount * 100, 2);
    }
}
