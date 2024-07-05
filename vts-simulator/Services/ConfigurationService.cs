using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vts_simulator.Interfaces;
using vts_simulator.Models;

namespace vts_simulator.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public AppSettingsModel _appSettings;
        AppSettingsModel IConfigurationService.appSettingModel => _appSettings;
        public ConfigurationService(IConfiguration configuration)
        {
            _appSettings = configuration.GetSection("AppSettings").Get<AppSettingsModel>();
        }
    }
}
