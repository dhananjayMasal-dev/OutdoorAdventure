using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalModels
{
    internal class GeocodingResponse
    {
        public List<LocationResult>? results { get; set; }
    }

    internal class LocationResult
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string? name { get; set; }
        public string? country { get; set; }
    }

    internal class WeatherResponse
    {
        public DailyUnits? daily_units { get; set; }
        public DailyData? daily { get; set; }
    }

    internal class DailyUnits
    {
        public string? temperature_2m_max { get; set; }
    }

    internal class DailyData
    {
        public List<int> weathercode { get; set; } = new();
        public List<double> temperature_2m_max { get; set; } = new();
    }
}
