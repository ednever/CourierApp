namespace CourierApp.Models
{
    // Represents an authentication token returned by the API
    class AuthToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
