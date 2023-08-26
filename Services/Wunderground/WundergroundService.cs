using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using LakeStatsApi.Services.Influx.Models;
using LakeStatsApi.Services.Wunderground.Models;
using InfluxDB.Client;

namespace LakeStatsApi.Services.Wunderground
{
    public class WundergroundService : IWundergroundService
    {
        private ILogger _logger;
        private IInfluxDBClient _influxDBClient;
        private InfluxServiceOptions _influxOptions;

        public WundergroundService(ILogger<WundergroundService> logger, IInfluxDBClient influxDBClient, InfluxServiceOptions influxOptions)
        {
            _logger = logger;
            _influxDBClient = influxDBClient;
            _influxOptions = influxOptions;
        }

        public async Task<WundergroundIngestResponse> IngestWunderground(IngestWundergroundRequest request)
        {
            _logger.LogInformation("Writing action {request} to Influx", request);

            var response = new WundergroundIngestResponse()
            {
                CorrelationId = request.CorrelationId,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };

            var asyncWriter = _influxDBClient.GetWriteApiAsync();

            var dataPoints = new List<PointData>()
            {
                PointData.Measurement("Temperature")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("Temperature", request.Temperature)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("Humidity")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("Humidity", request.Humidity)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("Dewpoint")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("Dewpoint", request.Dewpoint)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("WindChill")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("WindChill", request.WindChill)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindDirection")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("WindDirection", request.WindDirection)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindSpeed")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("WindSpeed", request.WindSpeed)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindGust")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("WindGust", request.WindGust)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("TotalRain")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("TotalRain", request.TotalRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("DailyRain")
                    .Tag("StationId", request.StationId)
                    .Tag("CorrelationId", request.CorrelationId)
                    .Field("DailyRain", request.DailyRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
            };

            await asyncWriter.WritePointsAsync(dataPoints, _influxOptions.BucketName, _influxOptions.Organization);

            return response;
        }
    }
}
