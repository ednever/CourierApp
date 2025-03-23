using System.Text.Json.Serialization;

namespace CourierApp.Models
{
    // Represents a tariff entity with properties for city name and transport prices
    public class Tariff
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("priceForCar")]
        public decimal PriceForCar { get; set; }
        [JsonPropertyName("priceForScooter")]
        public decimal PriceForScooter { get; set; }
        [JsonPropertyName("priceForBicycle")]
        public decimal PriceForBicycle { get; set; }
    }
}
