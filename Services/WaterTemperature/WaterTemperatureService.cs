using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using LakeStatsApi.Services.Influx;
using LakeStatsApi.Services.WaterTemperature.Models;
using System.Globalization;
using System.Reactive.Linq;

namespace LakeStatsApi.Services.WaterTemperature
{
    public class WaterTemperatureService : IWaterTemperatureService
    {
        private ILogger _logger;
        private IInfluxDBService _influxDBService;

        public WaterTemperatureService(ILoggerFactory loggerFactory, IInfluxDBService influxDBService)
        {
            _logger = loggerFactory.CreateLogger<WaterTemperatureService>();
            _influxDBService = influxDBService;
        }

        public async Task<GetWaterTemperatureResponse> GetWaterTemperatureReading(GetWaterTemperatureRequest request)
        {
            _logger.LogInformation($"{nameof(WaterTemperatureService)} {nameof(GetWaterTemperatureReading)} {request.LocationId} {request.CorrelationId}");

            var response = new GetWaterTemperatureResponse(){
                CorrelationId =request.CorrelationId,
            }; 
            var results = await _influxDBService.QueryAsync(async query =>
            {
                var flux = $"from(bucket: \"dock\")\r\n |> range(start: 0)\r\n  |> filter(fn: (r) => r[\"_measurement\"] == \"device_frmpayload_data_TempC1\")\r\n  " +
                $"|> filter(fn: (r) => r[\"_field\"] == \"value\")\r\n  |> filter(fn: (r) => r[\"application_name\"] == \"Dock\")\r\n  |> filter(fn: (r) => r[\"dev_eui\"] == \"{request.LocationId}\")\r\n  " +
                "|> sort(columns:[\"_time\"],desc:true)" +
                $"|> limit(n:{request.Take})";
                var tables = await query.QueryAsync(flux, "7ee4d7f6e9f7c11e");
                return tables.SelectMany(t =>
                    t.Records.OrderByDescending(r => r.GetValueByKey("_time").ToString()).Select(r => new GetWaterTemperatureResult()
                    {
                        Temperature = Math.Round((double.Parse(r.GetValue().ToString()) * 1.80) + 32, 2),
                        TimeStamp = DateTime.ParseExact(r.GetValueByKey("_time").ToString(), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
                    })
                );
            });

            response.Results = results.OrderByDescending(t => t.TimeStamp).ToList(); 
            _logger.LogInformation("Result {pagedResults}", response);

            return response;
        }

        public async Task<GetWaterTemperatureSignalStrengthResponse> WaterTemperatureProbeSignalStrength(GetWaterTemperatureSignalStrengthRequest request)
        {
            _logger.LogInformation($"{nameof(WaterTemperatureService)} {nameof(GetWaterTemperatureReading)} {request.LocationId} {request.CorrelationId}");

            var response = new GetWaterTemperatureSignalStrengthResponse()
            {
                CorrelationId = request.CorrelationId,
            };
            var results = await _influxDBService.QueryAsync(async query =>
            {
                var flux = $"from(bucket: \"dock\")\r\n  |> range(start: 0)\r\n  " +
                $"|> filter(fn: (r) => r[\"_measurement\"] == \"device_uplink\")\r\n  " +
                $"|> filter(fn: (r) => r[\"_field\"] == \"rssi\")\r\n  " +
                $"|> filter(fn: (r) => r[\"application_name\"] == \"Dock\")\r\n  " +
                $"|> filter(fn: (r) => r[\"dev_eui\"] == \"{request.LocationId}\")\r\n  " +
                $"|> sort(columns:[\"_time\"],desc:true)"+
                $"|> yield(name: \"last\")";
                var tables = await query.QueryAsync(flux, "7ee4d7f6e9f7c11e");
                return tables.SelectMany(t =>
                    t.Records.OrderByDescending(r => r.GetValueByKey("_time").ToString()).Select(r => new GetWaterTemperatureSignalStrengthResult()
                    {
                        Rssi = int.Parse(r.GetValue().ToString()),
                        TimeStamp = DateTime.ParseExact(r.GetValueByKey("_time").ToString(), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                        Frequency = int.Parse(r.GetValueByKey("frequency").ToString()),
                        DR = int.Parse(r.GetValueByKey("dr").ToString())
                    })
                );
            });

            response.Results = results.OrderByDescending(t => t.TimeStamp).ToList();
            _logger.LogInformation("Result {pagedResults}", response);

            return response;
        }
    }
}
