using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CourierApp
{
    public partial class AdminWindow : Window
    {
        private readonly TariffService _tariffService;
        private ObservableCollection<Tariff> _tariffs;
        public AdminWindow()
        {
            InitializeComponent();
            _tariffService = new TariffService();
            _tariffs = new ObservableCollection<Tariff>();
            tariffsGrid.ItemsSource = _tariffs;
            tariffsGrid.CellEditEnding += TariffsGrid_CellEditEnding;
            LoadTariffs();
            
        }
        private async void LoadTariffs()
        {
            try
            {
                var tariffList = await _tariffService.GetAllTariffsAsync();
                _tariffs.Clear();
                foreach (var tariff in tariffList)
                {
                    _tariffs.Add(tariff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tariffs: {ex.Message}");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newTariff = new Tariff { Name = "New city" };
                await _tariffService.CreateTariffAsync(newTariff);
                LoadTariffs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tariff: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTariffs();
        }
        private async void TariffsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                try
                {
                    var tariff = e.Row.Item as Tariff;
                    if (tariff != null)
                    {
                        // Получаем измененное значение
                        var editingElement = e.EditingElement as TextBox;
                        if (editingElement != null)
                        {
                            string newValue = editingElement.Text;
                            string columnName = e.Column.Header.ToString();

                            // Проверяем, какое поле было изменено и обновляем соответствующее свойство
                            switch (columnName)
                            {
                                case "Name":
                                    tariff.Name = newValue;
                                    break;
                                case "Car Price":
                                    if (decimal.TryParse(newValue, out decimal carPrice))
                                        tariff.PriceForCar = carPrice;
                                    else
                                        return;
                                    break;
                                case "Scooter Price":
                                    if (decimal.TryParse(newValue, out decimal scooterPrice))
                                        tariff.PriceForScooter = scooterPrice;
                                    else
                                        return;
                                    break;
                                case "Bicycle Price":
                                    if (decimal.TryParse(newValue, out decimal bicyclePrice))
                                        tariff.PriceForBicycle = bicyclePrice;
                                    else
                                        return;
                                    break;
                            }

                            // Отправляем обновленный тариф на сервер
                            await _tariffService.UpdateTariffAsync(tariff.Id, tariff);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating tariff: {ex.Message}");
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int id)
                {
                    await _tariffService.DeleteTariffAsync(id);
                    LoadTariffs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting tariff: {ex.Message}");
            }
        }
    }
    // Класс для десериализации ответа от API
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
    public class TariffService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7148/api/Tariffs/";

        public TariffService()
        {
            _httpClient = new HttpClient();
        }

        // GET: api/Tariffs
        public async Task<List<Tariff>> GetAllTariffsAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tariff>>(content);
        }

        // GET: api/Tariffs/5
        public async Task<Tariff> GetTariffByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Tariff>(content);
        }

        // POST: api/Tariffs
        public async Task CreateTariffAsync(Tariff tariff)
        {
            var json = JsonSerializer.Serialize(tariff);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();
        }

        // PUT: api/Tariffs/5
        public async Task UpdateTariffAsync(int id, Tariff tariff)
        {
            var json = JsonSerializer.Serialize(tariff);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}{id}", content);
            response.EnsureSuccessStatusCode();
        }

        // DELETE: api/Tariffs/5
        public async Task DeleteTariffAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
