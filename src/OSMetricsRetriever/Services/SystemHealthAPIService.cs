using OSMetricsRetriever.Models;
using System.Text;
using System.Text.Json;

namespace OSMetricsRetriever.Services
{
    /// <summary>
    /// A service to send OS metrics to a System Health API.
    /// </summary>
    public class SystemHealthAPIService : IDisposable
    {
        private readonly string _apiEndpoint;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor for SystemHealthAPIService
        /// </summary>
        /// <param name="apiEndpoint"></param>
        public SystemHealthAPIService(string apiEndpoint)
        {
            _apiEndpoint = apiEndpoint;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set reasonable timeout
        }

        /// <summary>
        /// Sends a list of collected OS metrics to the System Health API.
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public async Task SendMetrics(List<OSMetric> metrics)
        {
            try
            {
                if (metrics == null || metrics.Count == 0)
                {
                    throw new ArgumentException("Metrics list cannot be null or empty", nameof(metrics));
                }

                var json = JsonSerializer.Serialize(metrics);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(_apiEndpoint, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to send metrics to API endpoint '{_apiEndpoint}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Dispose of the HttpClient
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
