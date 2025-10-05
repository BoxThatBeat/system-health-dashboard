using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.Models;
using OSMetricsRetriever.Providers;
using System.Management;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// Plugin for retrieving memory usage metrics from the operating system.
    /// </summary>
    public class MemoryUsagePlugin : IRetrieveMetricsPlugin
    {
        public static readonly string Name = "Memory Usage";

        private static readonly string Key = "memory_usage_metric";
        private static readonly string Description = "The amount of memory currently in use by the system.";
        
        private static readonly string TotalMemoryKeyString = "TotalVisibleMemorySize";
        private static readonly string FreeMemoryKeyString = "FreePhysicalMemory";
        private static readonly string WMIQueryString = $"SELECT {TotalMemoryKeyString}, {FreeMemoryKeyString} FROM Win32_OperatingSystem";
        private readonly ObjectQuery _WMIObjectQuery = new(WMIQueryString);

        /// <inheritdoc/>
        public OSMetric GetMetric(IWMIProvider provider)
        {
            var memoryInfo = provider.QueryWMI(_WMIObjectQuery).FirstOrDefault() ?? throw new MetricRetrievalException("Unable to retrieve memory information");

            // Validate that the required properties are available
            if (memoryInfo[TotalMemoryKeyString] == null)
                throw new MetricRetrievalException("TotalVisibleMemorySize property not available");

            if (memoryInfo[FreeMemoryKeyString] == null)
                throw new MetricRetrievalException("FreePhysicalMemory property not available");


            var totalMemoryKB = Convert.ToDouble(memoryInfo["TotalVisibleMemorySize"]);
            var freeMemoryKB = Convert.ToDouble(memoryInfo["FreePhysicalMemory"]);
            var usedMemoryKB = totalMemoryKB - freeMemoryKB;
            

            // Convert from KB to Bytes
            var usedMemoryBytes = usedMemoryKB * 1024;
            var totalMemoryBytes = totalMemoryKB * 1024;

            return new OSMetric()
            {
                Key = Key,
                Name = Name,
                Description = Description,
                Value = usedMemoryBytes,
                Total = totalMemoryBytes,
                Timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                Unit = MeasurementUnit.Bytes
            };
        }
    }
}
