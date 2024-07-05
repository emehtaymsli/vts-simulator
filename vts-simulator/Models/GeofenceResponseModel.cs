using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vts_simulator.Models
{
    public class GeofenceResponseModel
    {
     public Guid Id { get; set; }
            public string Name { get; set; }
            public List<CoordinatesModel> Coordinates { get; set; }
       
    }
   
}
