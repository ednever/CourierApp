using System;
using System.Windows;
using CourierApp.Data;
using CourierApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CourierApp
{
    public partial class MainWindow : Window
    {
        public AppDbContext _context;
        public MainWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;

            LoadData();
        }

        private void LoadData()
        {
            var weatherList = _context.Weather
                .Include(w => w.Phenomenon)
                .ToList();

            foreach (var weather in weatherList)
            {
                MessageBox.Show($"Station: {weather.StationName}, Phenomenon: {weather.Phenomenon?.Name}");
            }
        }

        private void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            var weather = new Weather
            {
                StationName = "Station A",
                WMOCode = 12345,
                AirTemperature = 20,
                WindSpeed = 5,
                PhenomenonID = 1,
                Timestamp = 1234567890
            };
            _context.Weather.Add(weather);
            _context.SaveChanges();

            MessageBox.Show("Data added successfully!");
            LoadData();
        }
    }
}