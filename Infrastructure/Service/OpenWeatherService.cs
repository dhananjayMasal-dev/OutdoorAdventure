using Domain.Interfaces;
using Infrastructure.ExternalModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class OpenWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly WeatherSettings _settings;
        private readonly IMemoryCache _cache;

        public OpenWeatherService(HttpClient httpClient, IOptions<WeatherSettings> options, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _cache = cache;
        }

        public async Task<(bool IsGoodWeather, string Message)> CheckWeatherAsync(string location, DateTime date)
        {
            string cacheKey = $"Weather_{location.ToLower().Trim()}_{date:yyyy-MM-dd}";

            if (_cache.TryGetValue(cacheKey, out (bool IsGood, string Msg) cachedResult))
            {
                return cachedResult;
            }

            try
            {
                var coordinates = await GetCoordinatesAsync(location);

                if (coordinates == null)
                {
                    return (false, $"Location '{location}' could not be found.");
                }

                var result = await GetWeatherForecastAsync(coordinates.Value.Lat, coordinates.Value.Lon, date);

                if (result.IsGoodWeather)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

                    _cache.Set(cacheKey, result, cacheOptions);
                }

                return result;
            }
            catch (Exception)
            {
                return (false, "Error connecting to Weather Service.");
            }
        }

        private async Task<(double Lat, double Lon)?> GetCoordinatesAsync(string locationName)
        {
            var encodedLocation = Uri.EscapeDataString(locationName);

            var url = $"{_settings.GeocodingBaseUrl}?name={encodedLocation}&count=1&language=en&format=json";

            var response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);

            if (response != null && response.results != null && response.results.Count > 0)
            {
                var bestMatch = response.results[0];
                return (bestMatch.latitude, bestMatch.longitude);
            }

            return null;
        }

        private async Task<(bool IsGoodWeather, string Message)> GetWeatherForecastAsync(double lat, double lon, DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd");

            var url = $"{_settings.ForecastBaseUrl}?latitude={lat}&longitude={lon}&daily=weathercode,temperature_2m_max&start_date={dateString}&end_date={dateString}&timezone=auto";

            var response = await _httpClient.GetFromJsonAsync<WeatherResponse>(url);

            if (response == null || response.daily == null)
            {
                return (false, "Unable to fetch weather data.");
            }

            int weatherCode = response.daily.weathercode[0];
            double maxTemp = response.daily.temperature_2m_max[0];

            if (maxTemp < 5.0)
            {
                return (false, $"Too cold! Forecast is {maxTemp}°C.");
            }

            if (weatherCode >= 51)
            {
                return (false, "Forecast predicts rain or storm.");
            }

            return (true, "Weather looks great!");
        }
    }
}
