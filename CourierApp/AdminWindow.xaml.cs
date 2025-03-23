using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using CourierApp.Models;

namespace CourierApp
{
    public partial class AdminWindow : Window
    {
        // Service for interacting with tariff-related API endpoints
        private readonly TariffService _tariffService;

        // Observable collection for displaying tariffs in the UI
        private ObservableCollection<Tariff> _tariffs;
        public AdminWindow()
        {
            InitializeComponent();
            _tariffService = new TariffService();
            _tariffs = new ObservableCollection<Tariff>();

            // Bind the tariffs collection to the DataGrid
            tariffsGrid.ItemsSource = _tariffs;
            tariffsGrid.CellEditEnding += TariffsGrid_CellEditEnding;
            LoadTariffs();
            
        }

        // Loading tariffs from the API and updating the observable collection
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
                // Create a new tariff and send it to the server
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

        // Handling the event when a cell edit is committed, updating the tariff on the server
        private async void TariffsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                try
                {
                    var tariff = e.Row.Item as Tariff;
                    if (tariff != null)
                    {
                        // Retrieve the edited value
                        var editingElement = e.EditingElement as TextBox;
                        if (editingElement != null)
                        {
                            string newValue = editingElement.Text;
                            string columnName = e.Column.Header.ToString();

                            // Update the appropriate tariff property based on the edited column
                            switch (columnName)
                            {
                                case "City":
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

                            // Send the updated tariff to the server
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
}
