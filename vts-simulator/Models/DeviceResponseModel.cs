using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vts_simulator.Models
{
    public class DeviceResponseModel<T>
    {
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public T Result { get; set; }
    }
}
