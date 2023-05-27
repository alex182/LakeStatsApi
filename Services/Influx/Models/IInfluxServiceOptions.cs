namespace LakeStatsApi.Services.Influx.Models
{
    public interface IInfluxServiceOptions
    {
        string Token { get; set; }
        string Url { get; set; }
    }
}