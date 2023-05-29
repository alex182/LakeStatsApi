namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class GetWaterTemperatureSignalStrengthResult
    {
        public int Frequency{ get; set; }
        public int Rssi{ get; set; }
        public int DR{ get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
