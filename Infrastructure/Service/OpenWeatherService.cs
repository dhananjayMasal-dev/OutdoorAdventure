using Domain.Interfaces;
using Infrastructure.ExternalModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

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
                var locationResult = await GetCoordinatesWithRetryAsync(location);

                if (locationResult.Lat == null && (locationResult.Suggestions == null || locationResult.Suggestions.Count == 0))
                {
                    return (false, $"Location '{location}' could not be found.");
                }

                if (locationResult.Lat == null && locationResult.Suggestions != null && locationResult.Suggestions.Count > 0)
                {
                    var suggestions = string.Join(", ", locationResult.Suggestions.Take(3));
                    return (false, $"Location '{location}' not found. Did you mean: {suggestions}?");
                }

                var result = await GetWeatherForecastAsync(locationResult.Lat!.Value, locationResult.Lon!.Value, date);

                if (result.IsGoodWeather)
                {
                    var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                    _cache.Set(cacheKey, result, cacheOptions);
                }

                return result;
            }
            catch (Exception)
            {
                return (false, "Error connecting to Weather Service.");
            }
        }

        private async Task<(double? Lat, double? Lon, List<string> Suggestions)> GetCoordinatesWithRetryAsync(string locationName)
        {
            var result = await FetchFromGeocodingApi(locationName);
            if (result.HasCoordinates) return (result.Lat, result.Lon, null);

            string tempLocation = locationName;

            while (tempLocation.Contains(' '))
            {
                tempLocation = tempLocation.Substring(0, tempLocation.LastIndexOf(' '));

                var retryResult = await FetchFromGeocodingApi(tempLocation);

                if (retryResult.Suggestions.Count > 0)
                {
                    return (null, null, retryResult.Suggestions);
                }
            }

            if (!string.IsNullOrWhiteSpace(tempLocation) && tempLocation != locationName)
            {
                var finalResult = await FetchFromGeocodingApi(tempLocation);

                if (finalResult.Suggestions.Count > 0)
                {
                    return (null, null, finalResult.Suggestions);
                }
            }

            return (null, null, null);
        }

        private async Task<(double? Lat, double? Lon, bool HasCoordinates, List<string> Suggestions)> FetchFromGeocodingApi(string query)
        {
            var encodedLocation = Uri.EscapeDataString(query);
            var url = $"{_settings.GeocodingBaseUrl}?name={encodedLocation}&count=5&language=en&format=json";

            var response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(url);

            if (response != null && response.results != null && response.results.Count > 0)
            {
                var exactMatch = response.results
                    .FirstOrDefault(r => r.name != null && r.name.Equals(query, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    var matchName = $"{exactMatch.name} ({exactMatch.country})";
                    return (exactMatch.latitude, exactMatch.longitude, true, new List<string> { matchName });
                }

                var suggestions = response.results
                    .Where(r => r.name != null)
                    .Select(r => $"{r.name} ({r.country})")
                    .Distinct()
                    .ToList();

                return (null, null, false, suggestions);
            }

            return (null, null, false, new List<string>());
        }

        private async Task<(bool IsGoodWeather, string Message)> GetWeatherForecastAsync(double lat, double lon, DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd");
            var url = $"{_settings.ForecastBaseUrl}?latitude={lat}&longitude={lon}&daily=weathercode,temperature_2m_max&start_date={dateString}&end_date={dateString}&timezone=auto";

            var response = await _httpClient.GetFromJsonAsync<WeatherResponse>(url);

            if (response == null || response.daily == null) return (false, "Unable to fetch weather data.");

            int weatherCode = response.daily.weathercode[0];
            double maxTemp = response.daily.temperature_2m_max[0];

            if (maxTemp < 5.0) return (false, $"Too cold! Forecast is {maxTemp}°C.");
            if (weatherCode >= 51) return (false, "Forecast predicts rain or storm.");

            return (true, "Weather looks great!");
        }
    }
}