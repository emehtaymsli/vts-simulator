using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vts_simulator.Models;

namespace vts_simulator.Interfaces
{
    public interface IConfigurationService
    {
        AppSettingsModel appSettingModel { get; }
    }
}
