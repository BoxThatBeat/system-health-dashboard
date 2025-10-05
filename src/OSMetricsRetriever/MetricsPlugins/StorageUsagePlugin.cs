using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.Models;
using OSMetricsRetriever.Providers;
using System.Management;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// Plugin for retrieving storage usage metrics from the operating system.
    /// </summary>
    public class StorageUsagePlugin : IRetrieveMetricsPlugin
    {
        public static readonly string Name = "Storage Usage";

        private static readonly string Key = "storage_usage_metric";
        private static readonly string Description = "The amount of storage space currently used on the primary drive.";
        
        private static readonly string SizeKeyString = "Size";
        private static readonly string FreeSpaceKeyString = "FreeSpace";
        private static readonly string WMIQueryString = $"SELECT {SizeKeyString}, {FreeSpaceKeyString} FROM Win32_LogicalDisk WHERE DriveType = 3";
        private readonly ObjectQuery _WMIObjectQuery = new(WMIQueryString);


        /// <inheritdoc/>
        public OSMetric GetMetric(IWMIProvider provider)
        {
            var drives = provider.QueryWMI(_WMIObjectQuery).ToList();
            
            if (drives.Count == 0)
                throw new MetricRetrievalException("Unable to retrieve storage information. No drives were found.");

            // Calculate total used and total space across all fixed drives
            double totalSizeBytes = 0;
            double totalFreeBytes = 0;

            foreach (var drive in drives)
            {
                var driveSize = Convert.ToDouble(drive[SizeKeyString]);
                var freeSpace = Convert.ToDouble(drive[FreeSpaceKeyString]);
                
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
