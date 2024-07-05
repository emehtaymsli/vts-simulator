using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vts_simulator.Models
{
    public class AppSettingsModel
    {
        public string FetchDeviceCodeListUrl { get; set; }
        public string queueName { get; set; }
        public string HostName { get; set; } 
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get ; set; }
    }
}
