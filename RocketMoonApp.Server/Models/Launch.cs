namespace RocketMoonApp.Server.Models
{
    public class Launch
    {
        public string Id { get; set; } = string.Empty;

        public string RocketName { get; set; } = string.Empty;

        public DateTimeOffset Date { get; set; }

        public Location Location { get; set; } = new Location();

        public string Status { get; set; } = string.Empty;

        public bool? WasSuccessful { get; set; }
    }
}
