using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DndRpg.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DndRpg.Infrastructure.Clients
{
    public class DndApiClient : IDndApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DndApiClient> _logger;
        private const string BaseUrl = "https://www.dnd5eapi.co/api/";

        public DndApiClient(HttpClient httpClient, ILogger<DndApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<T> GetAsync<T>(string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data from endpoint {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiListResponse<T>>(content);
                return result?.Results ?? new List<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list from endpoint {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<T> SearchAsync<T>(string endpoint, string query) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync($"{endpoint}?name={Uri.EscapeDataString(query)}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching in endpoint {Endpoint} with query {Query}", endpoint, query);
                throw;
            }
        }

        private class ApiListResponse<T>
        {
            public int Count { get; set; }
            public List<T> Results { get; set; }
        }
    }
} 