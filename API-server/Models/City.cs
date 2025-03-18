namespace API_server.Models
{
    public class City
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal PriceForCar { get; set; }
        public decimal PriceForScooter { get; set; }
        public decimal PriceForBicycle { get; set; }
    }
}