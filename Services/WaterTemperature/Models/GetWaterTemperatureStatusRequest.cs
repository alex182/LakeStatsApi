namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class GetWaterTemperatureSignalStrengthRequest
    {
        public string CorrelationId { get; set; }
        public string LocationId { get; set; }
        public int Take { get; set; }
        public int Minutes { get; set; }
    }
}
