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
using RestSharp.Authenticators;
using LakeStatsApi.Services.Wunderground.Models;
using LakeStatsApi.Services.Wunderground;
using LakeStatsApi.Auth;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


var influxDbOptions = new InfluxServiceOptions()
{
    Token = Environment.GetEnvironmentVariable("InfluxKey"),
    Url = builder.Configuration.GetValue(typeof(string), "InfluxDb:Url").ToString(),
    Organization = "7ee4d7f6e9f7c11e",
    BucketName = "weatherStation"
};

var adminClientId = Environment.GetEnvironmentVariable("Keycloak-Admin-Id");
var adminClientSecret = Environment.GetEnvironmentVariable("Keycloak-Admin-Secret");

if (string.IsNullOrEmpty(adminClientId))
    throw new ArgumentNullException(adminClientId);

if (string.IsNullOrEmpty(adminClientSecret))
    throw new ArgumentNullException(adminClientSecret);

var keylcoakClientOptions = new KeycloakOptions()
{
    AdminId = adminClientId,
    AdminSecret = adminClientSecret
};

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

builder.Services.AddSingleton<IInfluxDBClient>(p => influxDbClient); 
builder.Services.AddSingleton<InfluxServiceOptions>(p => influxDbOptions); 
builder.Services.AddSingleton<KeycloakOptions>(p => keylcoakClientOptions); 
builder.Services.AddSingleton<IKeycloakClient,KeycloakClient>(); 
builder.Services.AddTransient<IWaterTemperatureService, WaterTemperatureService>();
builder.Services.AddTransient<IWundergroundService, WundergroundService>();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("LakeFrontApi-Write", p => p.AddRequirements(new HasScopeClientIdClientSecretRequirement(new List<string>() { "lakefrontapi-write" })));
    o.AddPolicy("LakeFrontApi-Jwt-Read", p => p.AddRequirements(new HasScopeJwtRequirement(new List<string>() { "lakefrontapi-read" })));
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeClientIdClientSecretHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeJwtHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionHandlerPathFeature?.Error != null)
        {
            Log.Error($"An unhandled exception occurred: {exceptionHandlerPathFeature.Error}");
            await context.Response.WriteAsync("An error occurred.");
        }
    });
});




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

}
).RequireAuthorization("LakeFrontApi-Jwt-Read")
.WithOpenApi(generatedOperation =>
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

})
.RequireAuthorization("LakeFrontApi-Jwt-Read")
.WithOpenApi(generatedOperation =>
{
    var parameter = generatedOperation.Parameters[0];
    parameter.Description = "The location Id of the probe to return status for";
    return generatedOperation;
});


//yes its a GET request for ingesting information....its the Wunderground standard that the device writing to this endpoint uses
app.MapGet("/Wunderground/Ingest", async (string PASSWORD, string ID,double tempf,int humidity,double dewptf,double windchillf, 
    int winddir, double windspeedmph, double windgustmph, double rainin, double dailyrainin,double weeklyrainin, double monthlyrainin,
    double yearlyrainin, double totalrainin, double solarradiation, int UV, double absbaromin, double baromin, int lowbatt,
    ILoggerFactory loggerFactory, IWundergroundService wundergroundService) =>
{

    var correlationId = Guid.NewGuid().ToString();
    var logger = loggerFactory.CreateLogger("Wunderground-Ingest");
    var response = new WundergroundIngestResponse();

    try
    {
        using (logger.BeginScope("/Wunderground/Ingest {deviceId} {temp} {humidity} {dewpoint} {windchill} {winddirection} {windspeed} {windgust}" +
            "{rainytd} {rain} {correlationId}", ID, tempf, humidity, dewptf, windchillf, winddir, windspeedmph, windgustmph, rainin, 
            dailyrainin, correlationId))
        {
            var request = new IngestWundergroundRequest()
            {
                CorrelationId = correlationId,
                StationId = ID,
                DailyRain = dailyrainin,
                Dewpoint = dewptf,
                Humidity = humidity,
                Temperature = tempf,
                TotalRain = totalrainin,
                WindChill = windchillf,
                WindDirection = winddir,
                WindGust = windgustmph,
                WindSpeed = windspeedmph,
                WeeklyRain = weeklyrainin,
                MonthlyRain = monthlyrainin, 
                YearlyRain = yearlyrainin, 
                SolarRadiation = solarradiation, 
                UvIndex = UV, 
                AbsoluteBarom = absbaromin, 
                Pressure = baromin, 
                BatteryLow = lowbatt
            };

            response = await wundergroundService.IngestWunderground(request);
        };
    }
    catch(Exception ex)
    {
        response.Message = "An error ocurred";
        response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
    }

    return response;

})
.WithName("WundergroundIngest")
.RequireAuthorization("LakeFrontApi-Write");


app.MapGet("/Wunderground/Reading/{stationdId}/{take?}", async (string stationdId, int? take,
    ILoggerFactory loggerFactory, IWundergroundService wundergroundService) =>
{
    take = take ?? 1;

    var correlationId = Guid.NewGuid().ToString();
    var logger = loggerFactory.CreateLogger("Wunderground-Get");
    var response = new WundergroundReadingResponse();

    using (logger.BeginScope("/Wunderground/ {deviceId} {take} {correlationId}",stationdId, take, correlationId))
    {
        var request = new WundergroundReadingRequest()
        {
            StationId = stationdId,
            Take = take,
            CorrelationId = correlationId
        };

        response = await wundergroundService.GetWundergroundReadings(request);
    };

    return response;

})
.WithName("WundergroundReading")
.RequireAuthorization("LakeFrontApi-Jwt-Read");


app.Run();
