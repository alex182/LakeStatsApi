using LakeStatsApi.Services.Wunderground.Models;

namespace LakeStatsApi.Services.Wunderground
{
    public interface IWundergroundService
    {
        Task<WundergroundIngestResponse> IngestWunderground(IngestWundergroundRequest request);
    }
}