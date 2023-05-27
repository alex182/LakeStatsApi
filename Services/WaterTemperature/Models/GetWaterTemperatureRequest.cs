namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class GetWaterTemperatureRequest
    {
        public string CorrelationId { get; set; }
        public string LocationId { get; set; }
        public int Take { get; set; }
    }
}
