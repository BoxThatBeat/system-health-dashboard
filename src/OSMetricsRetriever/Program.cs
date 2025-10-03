using OSMetricsRetriever.MetricsPlugins;
using System.Threading.Tasks;

namespace OSMetricsRetriever
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Get the url from args or use a default value
            var apiUrl = args.Length > 0 ? args[1] : "http://localhost:5169/SystemMetrics";

            SystemHealthAPIService systemHealthAPIService = new SystemHealthAPIService(apiUrl);

            MetricsRetriever retriever = new MetricsRetriever.MetricsReceiverBuilder()
                .AddPlugin<CPUUtilizationPlugin>()
                .AddPlugin<MemoryUsagePlugin>()
                .AddPlugin<StorageUsagePlugin>()
                .Build();

            var collectedMetrics = retriever.CollectMetrics();

            Console.WriteLine("Collected Metrics:");
            foreach (var metric in collectedMetrics)
            {
                Console.WriteLine($"Key: {metric.Key}, Name: {metric.Name}, Value: {metric.Value}, Total: {metric.Total}, Unit: {metric.Unit}, Timestamp: {metric.Timestamp}");
            }

            await systemHealthAPIService.SendMetrics(collectedMetrics);

            Console.WriteLine("Metrics sent successfully.");
        }
    }
}
