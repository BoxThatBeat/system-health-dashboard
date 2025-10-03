using OSMetricsRetriever.Models;
using System.Management;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// 
    /// </summary>
    public class CPUUtilizationPlugin : IRetrieveMetricsPlugin
    {
        private static readonly string Key = "cpu_utilization_metric";
        private static readonly string Name = "CPU Utilization";
        private static readonly string Description = "The percentage of CPU utilization.";
        
        private static readonly string WMIQueryString = "SELECT LoadPercentage FROM Win32_Processor";

        private ObjectQuery _WMIObjectQuery;

        public CPUUtilizationPlugin()
        {
            _WMIObjectQuery = new ObjectQuery(WMIQueryString);
        }

        /// <inheritdoc/>
        public OSMetric GetMetric(ManagementScope scope)
        {
            var searcher = new ManagementObjectSearcher(scope, _WMIObjectQuery);

            var averageCpuLoad = searcher.Get().Cast<ManagementObject>()
                .Select(mo => Convert.ToInt32(mo["LoadPercentage"]))
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
