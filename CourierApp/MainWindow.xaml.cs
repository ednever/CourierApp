using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace CourierApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _cityMapping = new Dictionary<string, string>
        {
            { "Tallinn", "Tallinn-Harku" },
            { "Tartu", "Tartu-Tõravere" },
            { "Pärnu", "Pärnu" }
        };
        private string _selectedTransport;
        private ObservableCollection<WeatherResponse> _weatherResponse;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7148/");
            _weatherResponse = new ObservableCollection<WeatherResponse>();
            WeatherDataGrid.ItemsSource = _weatherResponse;            
        }
        private async void LoadWeatherData()
        {
            try
            {
                // Отправляем запрос к API
                var response = await _httpClient.GetAsync("api/Weather");

                // Обрабатываем ответ
                if (response.IsSuccessStatusCode)
                {
                    // Успешный ответ (статус 200 OK)
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<WeatherResponse>>(jsonResponse);

                    _weatherResponse.Clear();
                    foreach (var weatherData in result)
                    {
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(weatherData.Timestamp);
                        weatherData.ConvertedTimestamp = dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
                        _weatherResponse.Add(weatherData);
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                }               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading weather data: {ex.Message}");
            }
        }
        private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWeatherData();
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что пользователь выбрал город и транспорт
                if (CityComboBox.SelectedItem == null || _selectedTransport == null)
                {
                    MessageBox.Show("Please select both a city and a transport type.");
                    return;
                }

                var selectedCity = (CityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var city = _cityMapping[selectedCity];
                var transport = _selectedTransport;

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

                // Обрабатываем ответ
                string resultText;
                if (response.IsSuccessStatusCode)
                {
                    // Успешный ответ (статус 200 OK)
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DeliveryResponse>(jsonResponse);
                    resultText = $"Result: {result.Message} Cost: {result.Cost} EUR";
                }
                else
                {
                    // Обработка ошибки (например, BadRequest с кодом 400)
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    resultText = $"Error: {errorMessage}";
                }

                // Отображаем результат
                ResultTextBlock.Text = resultText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private async void SetFrequencyButton_Click(object sender, RoutedEventArgs e) 
        {
            if (int.TryParse(UpdateFrequencyTextBox.Text, out int minutes) && minutes > 0)
            {
                // Формируем URL с параметром в строке запроса
                var url = $"api/Weather/setfrequency?minutes={minutes}";

                // Отправляем POST-запрос без тела
                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Update frequency set successfully.");
                    await Task.Delay(minutes * 60000);
                    LoadWeatherData();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {errorMessage}");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number of minutes greater than 0.");
            }
        }
        private void OpenAdminWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminWindow();
            adminWindow.ShowDialog();
        }
        private void TransportBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            // Сбрасываем выделение всех квадратов
            CarBorder.BorderBrush = Brushes.Transparent;
            ScooterBorder.BorderBrush = Brushes.Transparent;
            BicycleBorder.BorderBrush = Brushes.Transparent;

            // Выделяем выбранный квадрат
            border.BorderBrush = Brushes.Blue; // Цвет выделения
            border.BorderThickness = new Thickness(3);

            // Сохраняем выбранный транспорт
            _selectedTransport = border.Tag.ToString();
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
    public class PhenomenonResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
