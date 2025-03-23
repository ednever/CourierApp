using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CourierApp.Models;

namespace CourierApp
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;

        // Dictionary mapping city names to weather station names
        private readonly Dictionary<string, string> _cityMapping = new Dictionary<string, string>
        {
            { "Tallinn", "Tallinn-Harku" },
            { "Tartu", "Tartu-Tõravere" },
            { "Pärnu", "Pärnu" }
        };
        private string _selectedTransport;

        // Observable collection for displaying weather data in the UI
        private ObservableCollection<WeatherResponse> _weatherResponse;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7148/");
            _weatherResponse = new ObservableCollection<WeatherResponse>();

            // Bind the weather data collection to the DataGrid
            WeatherDataGrid.ItemsSource = _weatherResponse;            
        }

        // Loading weather data from the API and updating the observable collection
        private async void LoadWeatherData()
        {
            try
            {
                // Send a GET request to the API
                var response = await _httpClient.GetAsync("api/Weather");

                // Process the response
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<WeatherResponse>>(jsonResponse);

                    _weatherResponse.Clear();
                    foreach (var weatherData in result)
                    {
                        // Convert Unix timestamp to a readable format
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

        // Sending a request to calculate delivery cost based on the selected city and transport
        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate that a city and transport type have been selected
                if (CityComboBox.SelectedItem == null || _selectedTransport == null)
                {
                    MessageBox.Show("Please select both a city and a transport type.");
                    return;
                }

                var selectedCity = (CityComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();                
                var city = _cityMapping[selectedCity]; // Map the selected city to its station name
                var transport = _selectedTransport;

                // Create the request
                var request = new
                {
                    city,
                    transport
                };
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Send a POST request to the API
                var response = await _httpClient.PostAsync("api/Delivery/calculate", content);

                // Process the response
                string resultText;
                if (response.IsSuccessStatusCode)
                {                    
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DeliveryResponse>(jsonResponse);
                    resultText = $"Result: {result.Message} Cost: {result.Cost} EUR";
                }
                else
                {                    
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    resultText = $"Error: {errorMessage}";
                }

                // Display the result in the UI
                ResultTextBlock.Text = resultText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Sending a request to set the weather data update frequency and reloading data after the specified interval
        private async void SetFrequencyButton_Click(object sender, RoutedEventArgs e) 
        {
            if (int.TryParse(UpdateFrequencyTextBox.Text, out int minutes) && minutes > 0)
            {
                // Construct the URL with the query parameter
                var url = $"api/Weather/setfrequency?minutes={minutes}";

                // Send a POST request without a body
                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Update frequency set successfully.");
                    await Task.Delay(minutes * 60000); 
                    LoadWeatherData(); // Reload weather data after the delay
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
            // Open the LoginWindow for authentication to access the AdminWindow
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }
        private void TransportBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            // Reset the border of all transport options
            CarBorder.BorderBrush = Brushes.Transparent;
            ScooterBorder.BorderBrush = Brushes.Transparent;
            BicycleBorder.BorderBrush = Brushes.Transparent;

            // Highlight the selected transport option
            border.BorderBrush = Brushes.Blue;
            border.BorderThickness = new Thickness(3);

            // Store the selected transport type
            _selectedTransport = border.Tag.ToString();
        }
    }
}
