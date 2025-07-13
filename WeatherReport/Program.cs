using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using WeatherReport.Service;

namespace WeatherReport
{
    /// <summary>
    /// The main Program class to run the Weather Console Application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Application entry point. Sets up dependency injection, logging, configuration, and runs the main user interaction loop.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        static async Task Main(string[] args)
        {
            // Build configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure dependency injection and logging
            var services = new ServiceCollection();

            services.AddLogging(configure => configure.AddConsole());
            services.Configure<OpenWeatherMapSettings>(configuration.GetSection("OpenWeatherMap"));
            services.AddHttpClient<IWeatherService, WeatherService>();

            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var weatherService = serviceProvider.GetRequiredService<IWeatherService>();

            logger.LogInformation("Weather Console Application started.");

            // Main user interaction loop
            while (true)
            {
                Console.Write("Enter a city name (or 'exit' to quit): ");
                var city = Console.ReadLine();

                // Check for exit command
                if (string.Equals(city, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Application terminated by user.");
                    break;
                }

                // Validate input: not empty
                if (string.IsNullOrWhiteSpace(city))
                {
                    logger.LogWarning("Empty city name entered.");
                    Console.WriteLine("City name cannot be empty. Please try again.");
                    continue;
                }

                // Validate input: only letters, spaces, and hyphens allowed
                if (!Regex.IsMatch(city, "^[a-zA-Z\\s-]+$"))
                {
                    logger.LogWarning($"Invalid city name entered: {city}");
                    Console.WriteLine("Invalid city name. Only letters, spaces, and hyphens are allowed. Please try again.");
                    continue;
                }

                try
                {
                    // Fetch and display weather information
                    var weather = await weatherService.GetCurrentWeatherAsync(city.Trim());
                    Console.WriteLine($"\nWeather for {city}:");
                    Console.WriteLine(weather + "\n");
                    logger.LogInformation($"Successfully fetched weather for {city}.");
                }
                catch (Exception ex)
                {
                    // Handle and log errors
                    logger.LogError(ex, "An error occurred while fetching weather information.");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}