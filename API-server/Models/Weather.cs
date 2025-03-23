namespace API_server.Models
{
    public class Weather
    {
        public int ID { get; set; }
        public string StationName { get; set; }
        public int WMOCode { get; set; }
        public decimal AirTemperature { get; set; }
        public decimal WindSpeed { get; set; }
        public int PhenomenonID { get; set; }
        public int Timestamp { get; set; }
        public Phenomenon Phenomenon { get; set; }
    }
}
