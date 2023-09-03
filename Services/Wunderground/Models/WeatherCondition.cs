namespace LakeStatsApi.Services.Wunderground.Models
{
    public class WeatherCondition
    {
        public Enums.WeatherCondition Direction { get; set; }
        public Enums.WeatherConditionRateOfChange RateOfChange { get; set; }
        public bool StormsPossible { get; set; }
        public bool Unstable { get; set; }
    }
}
