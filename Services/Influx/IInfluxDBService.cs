using InfluxDB.Client;

namespace LakeStatsApi.Services.Influx
{
    public interface IInfluxDBService
    {
        Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action);
        void Write(Action<WriteApi> action);
    }
}