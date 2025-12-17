using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalModels
{
    public class WeatherSettings
    {
        public string GeocodingBaseUrl { get; set; } = string.Empty;
        public string ForecastBaseUrl { get; set; } = string.Empty;
    }
}
