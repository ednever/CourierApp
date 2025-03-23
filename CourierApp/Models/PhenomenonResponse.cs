using System.Text.Json.Serialization;

namespace CourierApp.Models
{
    // Represents the response structure for weather phenomenon data from the API
    public class PhenomenonResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
