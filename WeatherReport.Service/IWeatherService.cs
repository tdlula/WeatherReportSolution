namespace WeatherReport.Service
{
    /// <summary>
    /// Interface for weather service abstraction. Defines contract for fetching current weather information.
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Gets the current weather for a given city.
        /// </summary>
        /// <param name="city">City name to fetch weather for.</param>
        /// <returns>Weather information as a string.</returns>
        Task<string> GetCurrentWeatherAsync(string city);
    }
}