using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace WeatherReport.Service
{
    /// <summary>
    /// Concrete implementation of IWeatherService. Fetches weather data from the OpenWeatherMap API.
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient; // Used to send HTTP requests
        private readonly ILogger<WeatherService> _logger; // Logger for diagnostic messages
        private readonly OpenWeatherMapSettings _settings; // Settings for API access

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherService"/> class.
        /// </summary>
        /// <param name="httpClient">HttpClient instance for making HTTP requests.</param>
        /// <param name="logger">Logger instance for logging.</param>
        /// <param name="options">Options containing OpenWeatherMap settings.</param>
        public WeatherService(
            HttpClient httpClient,
            ILogger<WeatherService> logger,
            IOptions<OpenWeatherMapSettings> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Set a custom User-Agent for API requests
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpWeatherClient/1.0");
        }

        /// <summary>
        /// Gets the current weather for a city using the OpenWeatherMap API.
        /// </summary>
        /// <param name="city">City name to fetch weather for.</param>
        /// <returns>Formatted weather information as a string.</returns>
        /// <exception cref="ArgumentException">Thrown if city name is null or whitespace.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        public async Task<string> GetCurrentWeatherAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City name cannot be empty.");

            // Build the API request URL
            var encodedCity = Uri.EscapeDataString(city);
            var url = $"{_settings.BaseUrl}?q={encodedCity}&appid={_settings.ApiKey}&units={_settings.Units}";

            _logger.LogInformation($"Sending request to OpenWeatherMap: {url}");

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Received raw JSON: {json}");

            // Parse the JSON response
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            var description = root.GetProperty("weather")[0].GetProperty("description").GetString();
            var cityName = root.GetProperty("name").GetString();

            var formatted = $"{cityName}: {description}, {temp}°C";
            _logger.LogInformation($"Parsed weather info: {formatted}");

            return formatted;
        }
    }
}
