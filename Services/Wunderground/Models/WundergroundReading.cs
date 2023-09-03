using LakeStatsApi.Services.Wunderground.Models.Enums;

namespace LakeStatsApi.Services.Wunderground.Models
{
    public class WundergroundReading
    {
        public string StationId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Dewpoint { get; set; }
        public double WindChill { get; set; }
        public double WindDirection { get; set; }
        public double WindSpeed { get; set; }
        public double WindGust { get; set; }
        public double TotalRain { get; set; }
        public double WeeklyRain { get; set; }
        public double DailyRain { get; set; }
        public double MonthlyRain { get; set; }
        public double YearlyRain { get; set; }
        public double SolarRadiation { get; set; }
        public double UvIndex { get; set; }
        public double AbsoluteBarom { get; set; }
        public double Pressure { get; set; }
        public long BatteryLow { get; set; }
        public string WeatherCondition { get; set; }
        public string WeatherConditionDirection { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
