using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;


namespace CourierApp
{
    public partial class TestWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _cityMapping = new Dictionary<string, string>
        {
            { "Tallinn", "Tallinn-Harku" },
            { "Tartu", "Tartu-Tõravere" },
            { "Pärnu", "Pärnu" }
        };

        public TestWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7148/"); // Укажите ваш URL API
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что пользователь выбрал город и транспорт
                if (CityComboBox.SelectedItem == null || TransportComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select both a city and a transport type.");
                    return;
                }

                var selectedCity = (CityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var city = _cityMapping[selectedCity];
                var transport = (TransportComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Формируем запрос
                var request = new
                {
                    city,
                    transport
                };
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Отправляем запрос к API
                var response = await _httpClient.PostAsync("api/Delivery/calculate", content);
                response.EnsureSuccessStatusCode();

                // Обрабатываем ответ
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DeliveryResponse>(jsonResponse);

                // Отображаем результат
                ResultTextBlock.Text = $"Result: {result.Message} Cost: {result.Cost} EUR";
                // ResultTextBlock.Text = jsonResponse;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

    // Классы для десериализации ответа от API
    public class DeliveryResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }
    }
}
