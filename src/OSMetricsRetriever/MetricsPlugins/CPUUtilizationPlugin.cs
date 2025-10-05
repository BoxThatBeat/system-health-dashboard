using OSMetricsRetriever.Models;
using OSMetricsRetriever.Providers;
using System.Management;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// Plugin for retrieving CPU utilization metrics from the operating system.
    /// </summary>
    public class CPUUtilizationPlugin : IRetrieveMetricsPlugin
    {
        public static readonly string Name = "CPU Utilization";

        private static readonly string Key = "cpu_utilization_metric";
        private static readonly string Description = "The percentage of CPU utilization.";
        
        private static readonly string LoadKeyString = "LoadPercentage";
        private static readonly string WMIQueryString = $"SELECT {LoadKeyString} FROM Win32_Processor";
        private readonly ObjectQuery _WMIObjectQuery = new(WMIQueryString);

        /// <inheritdoc/>
        public OSMetric GetMetric(IWMIProvider provider)
        {
            var averageCpuLoad = provider.QueryWMI(_WMIObjectQuery)
                .Select(managementObject => Convert.ToInt32(managementObject[LoadKeyString]))
                .Average();

            return new OSMetric()
            {
                Key = Key,
                Name = Name,
                Description = Description,
                Value = averageCpuLoad,
                Total = 100,
                Timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                Unit = MeasurementUnit.Percentage
            };

        }
    }
}
