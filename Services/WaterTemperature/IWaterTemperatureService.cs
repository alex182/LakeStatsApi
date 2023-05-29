using LakeStatsApi.Services.WaterTemperature.Models;

namespace LakeStatsApi.Services.WaterTemperature
{
    public interface IWaterTemperatureService
    {
        Task<GetWaterTemperatureResponse> GetWaterTemperatureReading(GetWaterTemperatureRequest request);
        Task<GetWaterTemperatureSignalStrengthResponse> WaterTemperatureProbeSignalStrength(GetWaterTemperatureSignalStrengthRequest request);
    }
}