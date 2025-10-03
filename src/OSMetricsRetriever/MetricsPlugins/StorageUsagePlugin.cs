using OSMetricsRetriever.Models;
using System.Management;
using System.Linq;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// Plugin for retrieving storage usage metrics from the operating system.
    /// </summary>
    public class StorageUsagePlugin : IRetrieveMetricsPlugin
    {
        private static readonly string Key = "storage_usage_metric";
        private static readonly string Name = "Storage Usage";
        private static readonly string Description = "The amount of storage space currently used on the primary drive.";
        
        private static readonly string WMIQueryString = "select Size, FreeSpace from Win32_LogicalDisk where DriveType = 3";

        private ObjectQuery _WMIObjectQuery;

        public StorageUsagePlugin()
        {
            _WMIObjectQuery = new ObjectQuery(WMIQueryString);
        }

        /// <inheritdoc/>
        public OSMetric GetMetric(ManagementScope scope)
        {
            var searcher = new ManagementObjectSearcher(scope, _WMIObjectQuery);

            var drives = searcher.Get().Cast<ManagementObject>().ToList();
            
            if (!drives.Any())
                throw new InvalidOperationException("Unable to retrieve storage information");

            // Calculate total used and total space across all fixed drives
            double totalSizeBytes = 0;
            double totalFreeBytes = 0;

            foreach (var drive in drives)
            {
                var driveSize = Convert.ToDouble(drive["Size"]);
                var freeSpace = Convert.ToDouble(drive["FreeSpace"]);
                
                totalSizeBytes += driveSize;
                totalFreeBytes += freeSpace;
            }

            var usedBytes = totalSizeBytes - totalFreeBytes;

            return new OSMetric()
            {
                Key = Key,
                Name = Name,
                Description = Description,
                Value = usedBytes,
                Total = totalSizeBytes,
                Timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                Unit = MeasurementUnit.Bytes
            };
        }
    }
}
