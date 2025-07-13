using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherReport.Service
{
    /// <summary>
    /// Configuration settings for accessing the OpenWeatherMap API.
    /// </summary>
    public class OpenWeatherMapSettings
    {
        /// <summary>
        /// The API key for authenticating with OpenWeatherMap.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        /// <summary>
        /// The base URL for the OpenWeatherMap API.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
        /// <summary>
        /// The units to use for temperature (e.g., "metric").
        /// </summary>
        public string Units { get; set; } = "metric";
    }
}
