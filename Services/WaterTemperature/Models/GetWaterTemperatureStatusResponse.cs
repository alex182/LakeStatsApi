namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class GetWaterTemperatureSignalStrengthResponse
    {
        public List<GetWaterTemperatureSignalStrengthResult> Results { get; set; }
        public string CorrelationId { get; set; }
    }
}
