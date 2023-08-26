using System.Net;

namespace LakeStatsApi.Services.Wunderground.Models
{
    public class WundergroundIngestResponse
    {
        public HttpStatusCode StatusCode{ get; set; }
        public string CorrelationId { get; set; }
        public string Message { get; set; }
    }
}