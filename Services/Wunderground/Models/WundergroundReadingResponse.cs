namespace LakeStatsApi.Services.Wunderground.Models
{
    public class WundergroundReadingResponse
    {
        public List<WundergroundReading> Results { get; set; } = new List<WundergroundReading>();
        public string CorrelationId { get; set; }
    }
}
