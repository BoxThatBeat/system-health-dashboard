using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.Models;
using System.Management;
using System.Linq;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// Plugin for retrieving memory usage metrics from the operating system.
    /// </summary>
    public class MemoryUsagePlugin : IRetrieveMetricsPlugin
    {
        private static readonly string Key = "memory_usage_metric";
        private static readonly string Name = "Memory Usage";
        private static readonly string Description = "The amount of memory currently in use by the system.";
        
        private static readonly string WMIQueryString = "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem";
        private readonly ObjectQuery _WMIObjectQuery = new(WMIQueryString);

        /// <inheritdoc/>
        public static readonly string Name = "Memory Usage";

        /// <inheritdoc/>
        public OSMetric GetMetric(ManagementScope scope)
        {
            var searcher = new ManagementObjectSearcher(scope, _WMIObjectQuery);

            var memoryInfo = searcher.Get().Cast<ManagementObject>().FirstOrDefault() ?? throw new MetricRetrievalException("Unable to retrieve memory information");

            // Validate that the required properties are available
            if (memoryInfo["TotalVisibleMemorySize"] == null)
                throw new MetricRetrievalException("TotalVisibleMemorySize property not available");

            if (memoryInfo["FreePhysicalMemory"] == null)
                throw new MetricRetrievalException("FreePhysicalMemory property not available");
            
            if (memoryInfo == null)
                throw new InvalidOperationException("Unable to retrieve memory information");

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
