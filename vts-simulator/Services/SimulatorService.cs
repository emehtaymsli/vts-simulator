using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using vts_simulator.Interfaces;
using vts_simulator.Models;

namespace vts_simulator.Services
{
    public class SimulatorService : ISimulatorService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IConfigurationService _configuration;
        private readonly ILogger<SimulatorService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, DateTime> _lastUpdateDict; 

        public SimulatorService(IRabbitMQService rabbitMQService, IConfigurationService configuration, ILogger<SimulatorService> logger, HttpClient httpClient)
        {
            _rabbitMQService = rabbitMQService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _lastUpdateDict = new Dictionary<string, DateTime>();
        }

        public async Task GPSSimulation()
        {
            string deviceId = "device123";
            string vehicleId = "v1";

            string encodedPolyline = "kb|lDofyvM_QdC}@FeANm@HOGOJ[Rs@n@y@v@gAp@_@?_AKi@YGGGMAKD]JQf@UfCc@tCaA`Bo@j@Qp@KnDg@rKcBdBi@p@MZFx@G`Em@lC_@jEo@tG[|FSdR}@pKe@|@KbAOrAGhBAdEKzI]REXKhCq@`@GxAGlDP|BAtC@v@AdAGn@KPQ|CEvALTEhAGZ@N@N?t@FFBx@ErFSxGOhFWFSrCQbIYlEK`CIjCKhAEzBO|GWfHc@";
            List<CoordinatesModel> polylinePoints = DecodePolylinePoints(encodedPolyline);

            string geofenceApiUrl = "https://localhost:7291/api/GeoFence/FetchGeofence";
            List<GeofenceResponseModel> geofences = await GetGeofencesAsync(geofenceApiUrl);
            Random random = new Random();


            foreach (var point in polylinePoints)
            {
                await SendAndLogData(deviceId, vehicleId, point.Lat, point.Lng);
            }



           Thread.Sleep(random.Next(5000, 10000)); // Wait for 5-10 seconds

       
        }
        public static List<CoordinatesModel> DecodePolylinePoints(string encodedPoints)
        {
            if (encodedPoints == null || encodedPoints == "") return null;
            List<CoordinatesModel> poly = new List<CoordinatesModel>();
            char[] polylinechars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            try
            {
                while (index < polylinechars.Length)
                {
                    // calculate next latitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    // calculate next longitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length && next5bits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                    CoordinatesModel p = new CoordinatesModel();
                    p.Lat = Convert.ToDouble(currentLat) / 100000.0;
                    p.Lng = Convert.ToDouble(currentLng) / 100000.0;
                    poly.Add(p);
                }
            }
            catch (Exception ex)
            {
                // log it
                Console.WriteLine("Error decoding polyline: " + ex.Message);
            }
            return poly;
        }
        private async Task<List<GeofenceResponseModel>> GetGeofencesAsync(string geofenceApiUrl)
        {
            var response = await _httpClient.GetStringAsync(geofenceApiUrl);
            var serviceResponse = JsonConvert.DeserializeObject<DeviceResponseModel<List<GeofenceResponseModel>>>(response);

            if (serviceResponse != null && serviceResponse.Status)
            {
                return serviceResponse.Result;
            }

            throw new Exception(serviceResponse?.ErrorMessage ?? "Failed to fetch geofences");
        }
        private async Task SendAndLogData(string deviceId, string vehicleId, double latitude, double longitude)
        {
            var gpsData = new GPSData
            {
                DeviceId = deviceId,
                VehiclePlateNo = vehicleId,
                Latitude = latitude,
                Longitude = longitude,
                CurrentTime = DateTime.Now,
            };

         
            // Send current data
            _rabbitMQService.Sender(gpsData);
         

            // Simulate delay
            Random random = new Random();
            Thread.Sleep(random.Next(5000, 10000)); 
          //   Wait for 5-10 seconds
        }
    }
}

