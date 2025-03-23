using System.Text.Json.Serialization;

namespace CourierApp.Models
{
    // Represents the response structure for delivery cost calculations from the API
    public class DeliveryResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
    }
}
