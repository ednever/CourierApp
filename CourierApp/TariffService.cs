using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CourierApp.Models;

namespace CourierApp
{
    // Provides methods to interact with the tariff API for CRUD operations
    public class TariffService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7148/api/Tariffs/";
        private const string AuthUrl = "https://localhost:7148/api/Auth/login";
        private AuthToken _authToken;

        public TariffService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Retrieves all tariffs from the API.
        /// </summary>
        /// <returns>A list of Tariff objects.</returns>
        public async Task<List<Tariff>> GetAllTariffsAsync()
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tariff>>(content);
        }

        /// <summary>
        /// Retrieves a specific tariff by its ID from the API.
        /// </summary>
        /// <param name="id">The ID of the tariff to retrieve.</param>
        /// <returns>The Tariff object corresponding to the specified ID.</returns>
        public async Task<Tariff> GetTariffByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Tariff>(content);
        }

        /// <summary>
        /// Creates a new tariff via the API.
        /// </summary>
        /// <param name="tariff">The Tariff object to create.</param>
        public async Task CreateTariffAsync(Tariff tariff)
        {
            var json = JsonSerializer.Serialize(tariff);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates an existing tariff via the API.
        /// </summary>
        /// <param name="id">The ID of the tariff to update.</param>
        /// <param name="tariff">The updated Tariff object.</param>
        public async Task UpdateTariffAsync(int id, Tariff tariff)
        {
            var json = JsonSerializer.Serialize(tariff);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}{id}", content);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Deletes a tariff by its ID via the API.
        /// </summary>
        /// <param name="id">The ID of the tariff to delete.</param>
        public async Task DeleteTariffAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}{id}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Authenticates a user with the API using the provided username and password.
        /// </summary>
        /// <param name="username">The username to authenticate</param>
        /// <param name="password">The password to authenticate</param>
        /// <returns>The authentication token</returns>
        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Create a new LoginCredentials object with the provided username and password
                var credentials = new LoginCredentials
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(credentials);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(AuthUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

                // Store the authentication token and its expiration date
                _authToken = new AuthToken
                {
                    Token = tokenResponse["token"],
                    Expires = DateTime.Now.AddHours(1) 
                };

                // Set the authorization header for future requests
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken.Token);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
