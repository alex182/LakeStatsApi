namespace LakeStatsApi.Services.Wunderground.Models
{
    public class IngestWundergroundRequest
    {
        public string StationId { get; set; }
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public double Dewpoint { get; set; }
        public double WindChill { get; set; }
        public int WindDirection { get; set; }
        public double WindSpeed { get; set; }
        public double WindGust { get; set; }
        public double TotalRain { get; set; }
        public double DailyRain { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; }
    }
}