using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vts_simulator.Models
{
    public class GPSData
    {
        public string DeviceId { get; set; }
        public string VehiclePlateNo { get; set; }
        public DateTime CurrentTime { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

    }
}
