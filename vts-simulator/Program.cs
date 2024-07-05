using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vts_simulator.Interfaces;
using vts_simulator.Services;

namespace vts_simulator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Create the host
                var host = CreateHostBuilder(args).Build();

                // Get the ISimulatorService from the services
                var mainService = host.Services.GetService<ISimulatorService>();

                // Start simulating
                await mainService.GPSSimulation();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during startup
                Console.WriteLine($"Some error has occurred: {ex.Message}");
                Log.Logger.Error(ex.ToString());
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureServices((hostContext, services) =>
             {
                 // Add services to the dependency injection container
                 services.AddHttpClient();
                 services.AddSingleton<IConfigurationService, ConfigurationService>();
                 services.AddSingleton<ISimulatorService, SimulatorService>();
                 services.AddSingleton<IRabbitMQService, RabbitMQService>();
             })
             .UseSerilog((hostingContext, logger) =>
             {
                 // Configure Serilog logging
                 logger.Enrich.FromLogContext()
                       .WriteTo.Console()
                       .WriteTo.File(
                           path: "../logs/EventListener.log",
                           rollingInterval: RollingInterval.Day,
                           outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                       );
             })
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 // Configure application configuration
                 config.AddJsonFile("C:\\Projects\\Gitlab\\VTS-Simulator\\vts-simulator\\vts-simulator\\vts-simulator\\appsettings.json", optional: false, reloadOnChange: true)
                      .SetBasePath(Directory.GetCurrentDirectory());
             });
    }
   
}
