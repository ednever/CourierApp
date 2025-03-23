using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CourierApp
{
    public partial class LoginWindow : Window
    {
        private readonly TariffService _tariffService;

        public LoginWindow()
        {
            InitializeComponent();
            _tariffService = new TariffService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the username and password fields are empty
            if (UsernameTextBox.Text == null || PasswordBox.Password == null)
            {
                MessageBox.Show("Please fill in the username and password fields.");
                return;
            }

            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Authenticate the user with the API
            var success = await _tariffService.AuthenticateAsync(username, password);
            if (success)
            {
                // Open the admin window if authentication is successful
                var adminWindow = new AdminWindow();
                adminWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid Username or Password");
            }
        }
    }
}
