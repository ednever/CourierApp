using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using CourierApp.Data;
using CourierApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CourierApp
{
    public partial class MainWindow : Window
    {
        public AppDbContext _context;
        private ObservableCollection<Weather> _weatherList = new ObservableCollection<Weather>();
        private DispatcherTimer _timer; // Таймер для периодического обновления


        public MainWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            DataContext = this; // Устанавливаем текущий объект как контекст данных

            // Инициализация таймера. Срабатывание для 15-й минуты каждого часа (по умолчанию)
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, 15, 0);
            if (now.Minute >= 15 || nextRun < now)
            {
                nextRun = nextRun.AddHours(1);
            }
            _timer.Interval = nextRun - now;
            _timer.Start();

            // Загружаем данные сразу при старте
            LoadData();
        }
        public ObservableCollection<Weather> WeatherList
        {
            get => _weatherList;
            set
            {
                _weatherList = value;
                OnPropertyChanged(nameof(WeatherList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadData()
        {
            try
            {
                var weatherList = _context.Weather
                    .Include(w => w.Phenomenon)
                    .ToList();

                // Обновляем коллекцию для привязки
                WeatherList.Clear();
                foreach (var weather in weatherList)
                {
                    WeatherList.Add(weather);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }
        private async void AddData()
        {
            try
            {
                await FetchAndSaveWeatherData();
                MessageBox.Show("Weather data fetched and saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching weather data: {ex.Message}");
            }

            LoadData();
        }

        private async void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            AddData();
        }

        private async Task FetchAndSaveWeatherData()
        {
            // Список станций, которые нас интересуют
            var targetStations = new[] { "Tallinn-Harku", "Tartu-Tõravere", "Pärnu" };

            // Загружаем XML с помощью HttpClient
            using var client = new HttpClient();
            var xmlString = await client.GetStringAsync("https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php");

            // Парсим XML
            var doc = XDocument.Parse(xmlString);
            var timestamp = long.Parse(doc.Root.Attribute("timestamp").Value);

            // Извлекаем данные для нужных станций
            var weatherData = doc.Descendants("station")
                .Where(s => targetStations.Contains(s.Element("name")?.Value))
                .Select(s => new
                {
                    StationName = s.Element("name")?.Value,
                    WMOCode = int.Parse(s.Element("wmocode")?.Value ?? "0"),
                    AirTemperature = decimal.Parse(s.Element("airtemperature")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                    WindSpeed = decimal.Parse(s.Element("windspeed")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                    Phenomenon = s.Element("phenomenon")?.Value,
                })
                .ToList();

            foreach (var data in weatherData)
            {
                // Проверяем, существует ли явление в базе, или добавляем новое
                var phenomenon = _context.Phenomenon.FirstOrDefault(p => p.Name == data.Phenomenon);

                // Добавляем погодные данные
                var weather = new Weather
                {
                    StationName = data.StationName,
                    WMOCode = data.WMOCode,
                    AirTemperature = data.AirTemperature,
                    WindSpeed = data.WindSpeed,
                    PhenomenonID = phenomenon.ID,
                    Timestamp = (int)timestamp
                };
                _context.Weather.Add(weather);
            }

            await _context.SaveChangesAsync();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            AddData();
        }

        private void SetFrequencyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(UpdateFrequencyTextBox.Text, out int minutes) && minutes > 0)
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromMinutes(minutes);
                _timer.Start();
            }
            else
            {
                MessageBox.Show("Please enter a valid number of minutes greater than 0.");
            }
        }
    }
}