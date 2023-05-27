namespace LakeStatsApi.Services.Influx.Models
{
    public class InfluxServiceOptions : IInfluxServiceOptions
    {
        public string Token { get; set; }
        public string Url { get; set; } = "http://192.168.1.151:8086";
    }
}
