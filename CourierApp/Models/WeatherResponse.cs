using System.Text.Json.Serialization;

namespace CourierApp.Models
{
    // Represents the response structure for weather data from the API
    public class WeatherResponse
    {
        [JsonPropertyName("stationName")]
        public string StationName { get; set; }
        [JsonPropertyName("airTemperature")]
        public decimal AirTemperature { get; set; }
        [JsonPropertyName("windSpeed")]
        public decimal WindSpeed { get; set; }
        [JsonPropertyName("phenomenon")]
        public PhenomenonResponse Phenomenon { get; set; }
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
        public string ConvertedTimestamp { get; set; }
        public string PhenomenonName => Phenomenon?.Name;
    }
}
