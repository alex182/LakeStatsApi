namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class GetWaterTemperatureResponse
    {
        public List<GetWaterTemperatureResult> Results { get; set; }
        public string CorrelationId { get; set; }
    }
}
