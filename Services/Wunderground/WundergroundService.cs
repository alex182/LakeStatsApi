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
                    .Field("Temperature", request.Temperature)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("Humidity")
                    .Tag("StationId", request.StationId)
                    .Field("Humidity", request.Humidity)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("Dewpoint")
                    .Tag("StationId", request.StationId)
                    .Field("Dewpoint", request.Dewpoint)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("WindChill")
                    .Tag("StationId", request.StationId)
                    .Field("WindChill", request.WindChill)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindDirection")
                    .Tag("StationId", request.StationId)
                    .Field("WindDirection", request.WindDirection)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindSpeed")
                    .Tag("StationId", request.StationId)
                    .Field("WindSpeed", request.WindSpeed)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("WindGust")
                    .Tag("StationId", request.StationId)
                    .Field("WindGust", request.WindGust)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("TotalRain")
                    .Tag("StationId", request.StationId)
                    .Field("TotalRain", request.TotalRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),
                PointData.Measurement("DailyRain")
                    .Tag("StationId", request.StationId)
                    .Field("DailyRain", request.DailyRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("WeeklyRain")
                    .Tag("StationId", request.StationId)
                    .Field("WeeklyRain", request.WeeklyRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("MonthlyRain")
                    .Tag("StationId", request.StationId)
                    .Field("MonthlyRain", request.MonthlyRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("YearlyRain")
                    .Tag("StationId", request.StationId)
                    .Field("YearlyRain", request.YearlyRain)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("SolarRadiation")
                    .Tag("StationId", request.StationId)
                    .Field("SolarRadiation", request.SolarRadiation)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("UvIndex")
                    .Tag("StationId", request.StationId)
                    .Field("UvIndex", request.UvIndex)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("AbsoluteBarom")
                    .Tag("StationId", request.StationId)
                    .Field("AbsoluteBarom", request.AbsoluteBarom)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns),                
                PointData.Measurement("Pressure")
                    .Tag("StationId", request.StationId)
                    .Field("Pressure", request.Pressure)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns), 
                PointData.Measurement("BatteryLow")
                    .Tag("StationId", request.StationId)
                    .Field("BatteryLow", request.BatteryLow)
                    .Timestamp(request.TimeStamp, WritePrecision.Ns)
            };

            await asyncWriter.WritePointsAsync(dataPoints, _influxOptions.BucketName, _influxOptions.Organization);

            return response;
        }
    }
}
