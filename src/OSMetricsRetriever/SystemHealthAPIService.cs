using OSMetricsRetriever.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSMetricsRetriever
{
    public class SystemHealthAPIService
    {
        private string _apiEndpoint;
        public SystemHealthAPIService(string apiEndpoint)
        {
            _apiEndpoint = apiEndpoint;
        }
        public async Task SendMetrics(List<OSMetric> metrics)
        {
            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(metrics);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_apiEndpoint, content);

            response.EnsureSuccessStatusCode();
        }
    }
}
