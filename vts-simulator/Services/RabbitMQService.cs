using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Connections;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using vts_simulator.Interfaces;
using vts_simulator.Models;
using static MongoDB.Driver.WriteConcern;
using Newtonsoft.Json;

namespace vts_simulator.Services
{
    public class RabbitMQService:IRabbitMQService
    {
        private ILogger<RabbitMQService> _logger;
        private RabbitMQ.Client.IConnection connection;
        private IModel channel;
        private static RabbitMQService _instance;
        private readonly IConfigurationService _configuration;

        public RabbitMQService(ILogger<RabbitMQService> logger, IConfigurationService configurationService)
        {
            _logger = logger;
            InitializeRabbitMQConnection();//Set up the connection initally
            _configuration = configurationService;
        }
        public void Sender( GPSData device)
        {


            string queueNameOne = _configuration.appSettingModel.queueName;

            DeclareQueue(queueNameOne);
            try
            {
                // Checking if connection is available or not
                if (!IsConnectionAvailable())
                {
                    //_logger.LogWarning("Connection is not available. Retrying...");
                    InitializeRabbitMQConnection();
                }

                SendMessage(device);//Send message to the queue


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending a message.");
            }
        }
        // Method to create connection with RabbitMQ
        private void InitializeRabbitMQConnection()
        {
            int maxRetries = 10;
            int retryCount = 0;

            while (retryCount <= maxRetries)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = "10.167.96.47",
                        UserName = "eshita",
                        Password = "eshita",
                        Port = 5672

                    };
                    connection = factory.CreateConnection();
                    channel = connection.CreateModel();
                    break;
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" {DateTime.Now} Broker is unreachable: {ex.Message}. Re-Trying in 5 secs... (Attempt {retryCount + 1}/{maxRetries})");
                    Console.ResetColor();
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" {DateTime.Now} Maximum retries reached.");
                        Console.ResetColor();
                        break; // Max retries reached, exit the loop
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    // Handle other exceptions here.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" {DateTime.Now} Error setting up RabbitMQ connection. Re-Trying in 5 secs... (Attempt {retryCount + 1}/{maxRetries})");
                    Console.ResetColor();
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" {DateTime.Now} Maximum retries reached.");
                        Console.ResetColor();
                        break; // Max retries reached, exit the loop
                    }

                    Thread.Sleep(5000);
                }
            }
        }
      
     
        // Declaration of Queue
        private void DeclareQueue(string queueName)
        {
            try
            {
                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while declaring the queue: {QueueName}", queueName);
                throw;
            }
        }

        // Checking if connection exists or not
        private bool IsConnectionAvailable()
        {
            if (connection == null || !connection.IsOpen)
            {
                return false;
            }
            return true;
        }

        // Publishing Messages
        private void SendMessage(GPSData device)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(device);
                var body = Encoding.UTF8.GetBytes(jsonString);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: _configuration.appSettingModel.queueName,
                    basicProperties: null,
                    body: body
                );

                //_logger.LogInformation($"Sent: {jsonString}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{jsonString}");
                Console.ResetColor();

            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogError(ex, "An operation interrupted error occurred while sending the message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the message.");
            }
        }

    }
}
