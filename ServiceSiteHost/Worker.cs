using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceSiteHost.Configuration;
using ServiceSiteHost.Models;

namespace ServiceSiteHost
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceConfigurations _serviceConfigurations;


        public Worker(IHostEnvironment env, IConfiguration configuration, ILogger<Worker> logger)
        {
            _logger = logger;
                       
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            
            builder.AddEnvironmentVariables();
            configuration = builder.Build();

            _serviceConfigurations = new ServiceConfigurations(); 
            
            new ConfigureFromConfigurationOptions<ServiceConfigurations>(
                configuration .GetSection("ServiceConfigurations"))
                    .Configure(_serviceConfigurations);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Worker in: {time}", DateTimeOffset.Now.ToLocalTime());

                foreach (string host in _serviceConfigurations.Hosts)
                {
                    var exception = new Exception();
                    _logger.LogDebug($"Checking Host Availability: {host}");

                    var resultado = new ResultMonitor
                    {
                        Host = host,
                        Hour = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    try
                    {
                        using Ping p = new Ping();
                        resultado.Status = p.Send(host).Status.ToString();
                    }
                    catch (Exception ex)
                    {
                        resultado.Status = "Exception";
                        resultado.Exception = ex;
                        exception = ex;
                    }
                    string jsonResultado = JsonConvert.SerializeObject(resultado);


                    if (resultado.Exception == null)
                        _logger.LogInformation(200,jsonResultado);
                    else
                        _logger.LogError(400, exception, jsonResultado);
                }

                _logger.LogInformation("Worker finish in: {time}", DateTimeOffset.Now.ToLocalTime());
                await Task.Delay(_serviceConfigurations.Interval, stoppingToken);
            }
        }
    }
}
