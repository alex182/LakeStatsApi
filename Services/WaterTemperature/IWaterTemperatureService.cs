using LakeStatsApi.Services.WaterTemperature.Models;

namespace LakeStatsApi.Services.WaterTemperature
{
    public interface IWaterTemperatureService
    {
        Task<GetWaterTemperatureResponse> GetWaterTemperature(GetWaterTemperatureRequest request);
    }
}