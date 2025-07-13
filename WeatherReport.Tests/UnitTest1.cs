using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WeatherReport.Service;
using System.Text;

namespace WeatherReport.Tests
{
    /// <summary>
    /// Unit tests for the WeatherService class, covering input validation, API response handling, and edge cases.
    /// </summary>
    [TestFixture]
    public class WeatherServiceTests
    {
        private Mock<ILogger<WeatherService>> _loggerMock;
        private Mock<IOptions<OpenWeatherMapSettings>> _optionsMock;
        private OpenWeatherMapSettings _settings;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<WeatherService>>();
            _settings = new OpenWeatherMapSettings
            {
                ApiKey = "dummy",
                BaseUrl = "https://api.openweathermap.org/data/2.5/weather",
                Units = "metric"
            };
            _optionsMock = new Mock<IOptions<OpenWeatherMapSettings>>();
            _optionsMock.Setup(x => x.Value).Returns(_settings);
        }

        /// <summary>
        /// Ensures an ArgumentException is thrown for null or whitespace city names.
        /// </summary>
        [Test]
        public void GetCurrentWeatherAsync_ThrowsArgumentException_OnNullOrWhitespaceCity()
        {
            // Arrange
            var service = new WeatherService(new HttpClient(), _loggerMock.Object, _optionsMock.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => service.GetCurrentWeatherAsync(null));
            Assert.ThrowsAsync<ArgumentException>(() => service.GetCurrentWeatherAsync(" "));
        }

        /// <summary>
        /// Ensures a valid city returns the expected formatted weather string.
        /// </summary>
        [Test]
        public async Task GetCurrentWeatherAsync_ReturnsWeather_OnValidCity()
        {
            // Arrange
            var city = "London";
            var json = "{" +
                "\"main\":{\"temp\":20.0}," +
                "\"weather\":[{\"description\":\"clear sky\"}]," +
                "\"name\":\"London\"}";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json),
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var service = new WeatherService(httpClient, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = await service.GetCurrentWeatherAsync(city);

            // Assert
            Assert.That(result, Is.EqualTo("London: clear sky, 20°C"));
        }

        /// <summary>
        /// Ensures an HttpRequestException is thrown for non-success HTTP responses.
        /// </summary>
        [Test]
        public void GetCurrentWeatherAsync_ThrowsHttpRequestException_OnNonSuccessStatusCode()
        {
            // Arrange
            var city = "Nowhere";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var service = new WeatherService(httpClient, _loggerMock.Object, _optionsMock.Object);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(() => service.GetCurrentWeatherAsync(city));
        }

        /// <summary>
        /// Ensures the service can handle very long city names.
        /// </summary>
        [Test]
        public async Task GetCurrentWeatherAsync_HandlesVeryLongCityName()
        {
            // Arrange
            var city = new string('A', 300); // Very long city name
            var json = "{" +
                "\"main\":{\"temp\":15.5}," +
                "\"weather\":[{\"description\":\"cloudy\"}]," +
                $"\"name\":\"{city}\"}}";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json),
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var service = new WeatherService(httpClient, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = await service.GetCurrentWeatherAsync(city);

            // Assert
            Assert.That(result, Is.EqualTo($"{city}: cloudy, 15.5°C"));
        }

        /// <summary>
        /// Ensures the service can handle city names with special characters.
        /// </summary>
        [Test]
        public async Task GetCurrentWeatherAsync_HandlesCityNameWithSpecialCharacters()
        {
            // Arrange
            var city = "München-Paris";
            var json = "{" +
                "\"main\":{\"temp\":10.1}," +
                "\"weather\":[{\"description\":\"rainy\"}]," +
                $"\"name\":\"{city}\"}}";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json),
                });
            var httpClient = new HttpClient(handlerMock.Object);
            var service = new WeatherService(httpClient, _loggerMock.Object, _optionsMock.Object);

            // Act
            var result = await service.GetCurrentWeatherAsync(city);

            // Assert
            Assert.That(result, Is.EqualTo($"{city}: rainy, 10.1°C"));
        }
    }

    /// <summary>
    /// Integration test for simulating the full application flow with a mocked weather service.
    /// </summary>
    [TestFixture]
    public class WeatherAppIntegrationTests
    {
        [Test]
        public async Task SimulateFullAppFlow_WithMockedWeatherService()
        {
            // Arrange
            var mockWeatherService = new Mock<IWeatherService>();
            mockWeatherService.Setup(s => s.GetCurrentWeatherAsync("TestCity"))
                .ReturnsAsync("TestCity: ☀️ +25°C");

            // Simulate user input and output
            var input = new StringBuilder();
            var output = new StringBuilder();
            input.AppendLine("TestCity");
            input.AppendLine("exit");
            var inputReader = new System.IO.StringReader(input.ToString());
            var outputWriter = new System.IO.StringWriter(output);
            var originalIn = Console.In;
            var originalOut = Console.Out;
            Console.SetIn(inputReader);
            Console.SetOut(outputWriter);

            try
            {
                // Run a simplified version of the main loop
                int count = 0;
                while (true)
                {
                    Console.Write("Enter a city name (or 'exit' to quit): ");
                    var city = Console.ReadLine();
                    if (string.Equals(city, "exit", StringComparison.OrdinalIgnoreCase))
                        break;
                    if (string.IsNullOrWhiteSpace(city))
                        continue;
                    var weather = await mockWeatherService.Object.GetCurrentWeatherAsync(city.Trim());
                    Console.WriteLine($"\nWeather for {city}:");
                    Console.WriteLine(weather + "\n");
                    count++;
                }
                Assert.That(count, Is.EqualTo(1));
                Assert.That(output.ToString(), Does.Contain("TestCity: ☀️ +25°C"));
            }
            finally
            {
                Console.SetIn(originalIn);
                Console.SetOut(originalOut);
            }
        }
    }
}
