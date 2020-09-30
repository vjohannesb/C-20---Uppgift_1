using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Uppg1_WorkerService.Models;

namespace Uppg1_WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private HttpClient _client;
        private HttpResponseMessage _response;

        private readonly static string _baseUrl = "https://api.openweathermap.org/data/2.5/";
        private readonly static string _apiUrl = $"{_baseUrl}weather?q=Örebro,se&units=metric"
                                                 + $"&appid={Config.API_KEY}";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _logger.LogInformation($"Worker service successfully started. "
                                   + $"Logging in {Program.LogFileDirectory}.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FetchAndLogWeatherData();
                await Task.Delay(60 * 1000);
            }
        }

        public async Task FetchAndLogWeatherData()
        {
            try
            {
                _response = await _client.GetAsync(_apiUrl);

                if(_response.IsSuccessStatusCode)
                {
                    try
                    {
                        var content = await _response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<WeatherModel>(content);

                        _logger.LogInformation($"Current temperature in {data.name}: "
                            + $"{data.main.temp}C ({data.main.temp_min}-{data.main.temp_max}C)");

                        if (data.main.temp < 15)
                            _logger.LogInformation($"Autumn alert! Temperatures below 15C indicate autumn is on its way.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error parsing data from API - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{_baseUrl} is not responding - {ex.Message}");
            }
        }
    }
}
