using InfluxDB.Client;


namespace LakeStatsApi.Services.Influx
{
    public class InfluxDBService : IInfluxDBService
    {
        private InfluxDBClient _influxDBClient;
        private ILogger _logger;

        public InfluxDBService(InfluxDBClient influxDBClient, ILoggerFactory loggerFactory)
        {
            _influxDBClient = influxDBClient;
            _logger = loggerFactory.CreateLogger<InfluxDBService>();
        }

        public void Write(Action<WriteApi> action)
        {
            _logger.LogInformation("Writing action {action} to Influx", action);
            using var write = _influxDBClient.GetWriteApi();
            action(write);
        }

        public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
        {
            _logger.LogInformation("Reading from Influx {action} ", action);
            var query = _influxDBClient.GetQueryApi();
            return await action(query);
        }
    }
}
