namespace LakeStatsApi.Services.WaterTemperature.Models
{
    public class PagedResult<T>
    {
        public int TotalResults { get; set; }
        public int Take { get; set; }
        public List<T> Results { get; set; } = new List<T>();
    }
}
