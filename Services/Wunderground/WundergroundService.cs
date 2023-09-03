﻿using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using LakeStatsApi.Services.Influx.Models;
using LakeStatsApi.Services.Wunderground.Models;
using InfluxDB.Client;
using LakeStatsApi.Services.WaterTemperature.Models;
using System.Globalization;

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

            var weatherConditions = await GetWeatherConditions(request);

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


        internal async Task<WeatherCondition> GetWeatherConditions(IngestWundergroundRequest request)
        {
            var weatherCondition = new WeatherCondition();

            var averagePressures = await GetAveragePressures(request.StationId);

            if(averagePressures.Count != 13)
            {
                return weatherCondition;
            }

            var pressureDifferences = GetPressureDifferences(averagePressures);
            var pressureDifference = pressureDifferences.Sum() / pressureDifferences.Count();
            var currentPressure = averagePressures[0];

            if (pressureDifference > 0.75)
            {
                weatherCondition.StormsPossible = false;
                weatherCondition.Direction = Models.Enums.WeatherCondition.Improving;
                weatherCondition.RateOfChange = Models.Enums.WeatherConditionRateOfChange.Quick;
            }
            // Slowly rising, good weather condition, tendency rising
            else if (pressureDifference > 0.42)
            {
                weatherCondition.StormsPossible = false;
                weatherCondition.Direction = Models.Enums.WeatherCondition.Improving;
                weatherCondition.RateOfChange = Models.Enums.WeatherConditionRateOfChange.Slow;
            }
            // Change in weather condition is possible, tendency rising
            else if (pressureDifference > 0.25)
            {
                weatherCondition.StormsPossible = false;
                weatherCondition.Direction = Models.Enums.WeatherCondition.Improving;
                weatherCondition.RateOfChange = Models.Enums.WeatherConditionRateOfChange.None;

                if ((currentPressure >= 1006 && currentPressure <= 1020) || currentPressure < 1006)
                {
                    //CopyImage("/home/pi/pressure_info/SunCloud.png", "/var/www/html/Forecast.png");
                }
            }
            // Falling Conditions
            // Quickly falling, thunderstorm is highly possible
            else if (pressureDifference < -0.75)
            {
                //CopyImage("/home/pi/pressure_info/Storm.png", "/var/www/html/Forecast.png");
                //CopyImage("/home/pi/pressure_info/Down.png", "/var/www/html/Arrow.png");
            }
            // Slowly falling, rainy weather condition, tendency falling
            else if (pressureDifference < -0.42)
            {
                //CopyImage("/home/pi/pressure_info/Rain.png", "/var/www/html/Forecast.png");
                //CopyImage("/home/pi/pressure_info/DownRight.png", "/var/www/html/Arrow.png");
            }
            // Condition change is possible, tendency falling
            else if (pressureDifference < -0.25)
            {
                //CopyImage("/home/pi/pressure_info/DownRight.png", "/var/www/html/Arrow.png");
                //if ((currentPressure >= 1006 && currentPressure <= 1020) || currentPressure > 1020)
                //{
                //    CopyImage("/home/pi/pressure_info/SunCloud.png", "/var/www/html/Forecast.png");
                //}
            }
            // Steady Conditions
            // Condition is stable, don't change the weather symbol (sun, rain or sun/cloud), just change the arrow
            else if (pressureDifference <= 0.25 && pressureDifference >= -0.25)
            {
                //CopyImage("/home/pi/pressure_info/Right.png", "/var/www/html/Arrow.png");
            }

            return weatherCondition; 
        }
        internal List<double> GetPressureDifferences(List<double> pressureAverages)
        {
            var pressureDifferences = new List<double>();
            var pressuresToUse = pressureAverages.Take(new Range(1, pressureAverages.Count - 1)).ToList();

            foreach (var averagePressure in pressuresToUse)
            {
                var pressureIndex = pressuresToUse.IndexOf(averagePressure);
                var difference = (pressureAverages[0] - averagePressure) / (pressureIndex + .5);

                pressureDifferences.Add(difference);
            }

            return pressureDifferences;
        }

        internal async Task<List<double>> GetAveragePressures(string stationId)
        {
            var pressures = new List<double>() 
            {
                await GetAveragePressureFromDb(-600,stationId),
                await GetAveragePressureFromDb(-1800, stationId),
                await GetAveragePressureFromDb(-3600, stationId),
                await GetAveragePressureFromDb(-5400,stationId),
                await GetAveragePressureFromDb(-7200, stationId),
                await GetAveragePressureFromDb(-9000, stationId),
                await GetAveragePressureFromDb(-10800, stationId),
                await GetAveragePressureFromDb(-12600, stationId),
                await GetAveragePressureFromDb(-14400, stationId),
                await GetAveragePressureFromDb(-16200, stationId),
                await GetAveragePressureFromDb(-18000, stationId),
                await GetAveragePressureFromDb(-19800, stationId),
                await GetAveragePressureFromDb(-21600, stationId)
            };

            return pressures;
        }

        internal async Task<double> GetAveragePressureFromDb(int startSeconds,string stationId)
        {
            double pressureAverage = 0;

            
            var influxQuery = _influxDBClient.GetQueryApi();

            var flux = $"from(bucket: \"weatherStation\")\r\n  |> range(start: {startSeconds}, stop: now())\r\n  " +
                $"|> filter(fn: (r) => r[\"_measurement\"] == \"Pressure\")\r\n  |> filter(fn: (r) => r[\"_field\"] == \"Pressure\")\r\n  " +
                $"|> filter(fn: (r) => r[\"StationId\"] == \"{stationId}\")\r\n  |> mean()";

            var tables = await influxQuery.QueryAsync(flux, "7ee4d7f6e9f7c11e");

            if (tables.Any())
            {
                if (tables[0].Records.Any())
                {
                    pressureAverage = (double)tables[0].Records[0].GetValue(); 
                }

            }

            return pressureAverage; 
        }
    }
}
