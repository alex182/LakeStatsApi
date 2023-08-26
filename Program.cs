using LakeStatsApi.Services.Influx.Models;
using Serilog;
using System.Reflection;
using System.Net.NetworkInformation;
using Serilog.Sinks.Grafana.Loki;
using InfluxDB.Client;
using LakeStatsApi.Services.WaterTemperature;
using LakeStatsApi.Services.Influx;
using LakeStatsApi.Services.WaterTemperature.Models;
using Keycloak.Client.Models;
using Keycloak.Client;
using Microsoft.AspNetCore.Authorization;
using LakeStatsApi.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


var influxDbOptions = new InfluxServiceOptions()
{
    Token = Environment.GetEnvironmentVariable("InfluxKey"),
    Url = builder.Configuration.GetValue(typeof(string),"InfluxDb:Url").ToString()
};

var keylcoakClientOptions = new KeycloakOptions();

var useLokiLogging = builder.Configuration["UseLokiLogging"];

if (useLokiLogging == "True")
{
    var lokiIP = builder.Configuration["LokiIP"];

    Ping grafanPing = new Ping();
    PingReply grafanPingReply = grafanPing.Send(lokiIP);
    var applicationName = Assembly.GetExecutingAssembly().GetName().Name;

    if (grafanPingReply.Status == IPStatus.Success)
    {
        builder.Host.UseSerilog((hostContext, services, configuration) => {
            configuration
            .MinimumLevel.Debug()
            .Enrich.WithProperty("Host", Environment.MachineName)
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("TimeStamp", DateTime.UtcNow)
            .WriteTo
            .GrafanaLoki($"http://{lokiIP}:3100"
                , new List<LokiLabel> { new() { Key = "Application", Value = applicationName } }
                , credentials: null);
        });
    }

    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    Log.Information($"{applicationName} is starting");
};

var influxDbClient = InfluxDBClientFactory.Create(influxDbOptions.Url, influxDbOptions.Token);

builder.Services.AddSingleton<InfluxDBClient>(p => influxDbClient); 
builder.Services.AddSingleton<KeycloakOptions>(p => keylcoakClientOptions); 
builder.Services.AddSingleton<IKeycloakClient,KeycloakClient>(); 
builder.Services.AddTransient<IInfluxDBService, InfluxDBService>(); 
builder.Services.AddTransient<IWaterTemperatureService, WaterTemperatureService>();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("LakeFrontApi-Write", p => p.AddRequirements(new HasScopeAWRequirement(new List<string>() { "lakefrontapi-write" })));
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeAWHandler>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/WaterTemperatureProbe/Readings/{deviceId}/{take?}", async(string deviceId,
    ILoggerFactory loggerFactory, IWaterTemperatureService waterTemperatureService, int? take) =>
{
    take = take ?? 50;

    var correlationId = Guid.NewGuid().ToString();
    var logger = loggerFactory.CreateLogger("WaterTemperature");

    using (logger.BeginScope("/WaterTemperatureProbe/Readings/ {deviceId} CorrelationId {correlationId}", deviceId, correlationId))
    {
        var request = new GetWaterTemperatureRequest()
        {
            CorrelationId = correlationId,
            LocationId = deviceId,
            Take = take.Value
        };

        var waterTemp = await waterTemperatureService.GetWaterTemperatureReading(request);
        return waterTemp;
    };

}).WithOpenApi(generatedOperation =>
{
    var parameter = generatedOperation.Parameters[0];
    parameter.Description = "The location Id of where to return water temperature for";
    return generatedOperation;
});

app.MapGet("/WaterTemperatureProbe/Signal/{locationId}/{minutes?}", async (string locationId,
    ILoggerFactory loggerFactory, IWaterTemperatureService waterTemperatureService, int? minutes) =>
{
    minutes = minutes ?? 0;

    var correlationId = Guid.NewGuid().ToString();
    var logger = loggerFactory.CreateLogger("WaterTemperature");

    using (logger.BeginScope("/WaterTemperatureProbe/Status/ {locationId} CorrelationId {correlationId}", locationId, correlationId))
    {
        var request = new GetWaterTemperatureSignalStrengthRequest()
        {
            CorrelationId = correlationId,
            LocationId = locationId,
            Minutes = minutes.Value,
        };

        var probeStatus = await waterTemperatureService.WaterTemperatureProbeSignalStrength(request);
        return probeStatus;
    };

}).WithOpenApi(generatedOperation =>
{
    var parameter = generatedOperation.Parameters[0];
    parameter.Description = "The location Id of the probe to return status for";
    return generatedOperation;
});


//yes its a GET request for ingesting information....its the Wunderground standard that the device writing to this endpoint uses
app.MapGet("/Wunderground/Ingest", async (string PASSWORD, string ID,
    ILoggerFactory loggerFactory) =>
{

    var correlationId = Guid.NewGuid().ToString();
    var logger = loggerFactory.CreateLogger("Wunderground-Ingest");

})
.WithName("WundergroundIngest")
.RequireAuthorization("LakeFrontApi-Write");
//.WithOpenApi(generatedOperation =>
//{
//    var parameter = generatedOperation.Parameters[0];
//    parameter.Description = "The location Id of the probe to return status for";
//    return generatedOperation;
//});

app.Run();
