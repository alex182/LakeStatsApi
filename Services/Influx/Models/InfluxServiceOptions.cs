namespace LakeStatsApi.Services.Influx.Models
{
    public class InfluxServiceOptions
    {
        public string Token { get; set; }
        public string Url { get; set; } = "http://192.168.1.151:8086";
        public string BucketName { get; set; }
        public string Organization { get; set; }
    }
}
