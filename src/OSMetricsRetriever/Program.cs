using OSMetricsRetriever.MetricsPlugins;
using OSMetricsRetriever.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OSMetricsRetriever
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        private static readonly string ApplicationName = "OSMetricsRetriever";

        static async Task<int> Main(string[] args)
        {
            try
            {
                // Get the url from args or use a default value
                var apiUrl = args.Length > 0 ? args[0] : "http://localhost:5169/SystemMetrics";

                var systemHealthAPIService = new SystemHealthAPIService(apiUrl);

                var retriever = new MetricsRetrieverService.MetricsReceiverServiceBuilder()
                    .AddPlugin<CPUUtilizationPlugin>()
                    .AddPlugin<MemoryUsagePlugin>()
                    .AddPlugin<StorageUsagePlugin>()
                    .Build();

                var collectedMetrics = retriever.CollectMetrics();

                await systemHealthAPIService.SendMetrics(collectedMetrics);

                return 0; // Success
            }
            catch (InvalidOperationException ex)
            {
                LogError($"Configuration or operational error: {ex.Message}", ex);
                return 1; // Configuration/operational error
            }
            catch (TimeoutException ex)
            {
                LogError($"Timeout error: {ex.Message}", ex);
                return 2; // Timeout error
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error: {ex.Message}", ex);
                return 3; // Unexpected error
            }
        }

        private static void LogError(string message, Exception ex)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            
            if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                // Log to Windows Event Log in production
                try
                {

                    using var eventLog = new EventLog("Application");
                    eventLog.Source = ApplicationName;
                    eventLog.WriteEntry($"{message}\n\nException Details: {ex}", EventLogEntryType.Error);
                }
                catch
                {
                    // Silently fail if we can't write to event log
                }
            }
            else
            {
                // Console output for development
                Console.WriteLine($"ERROR: {message}");
                Console.WriteLine($"Exception Details: {ex}");
            }
        }
    }
}
