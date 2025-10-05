using System.Management;

namespace OSMetricsRetriever.Providers
{
    /// <summary>
    /// Handles creating and executing WMI queries. Removes the need to create new ManagementObjectSearcher instances directly in the core logic. This also serves to make code more testable.
    /// Note: a new ManagementObjectSearcher needs to be created for each query, it cannot be reused.
    /// </summary>
    public interface IWMIProvider
    {
        /// <summary>
        /// Queries WMI using the provided scope and query, returning the results as an enumerable of ManagementObject.
        /// </summary>
        /// <param name="query">The query for the desired metric</param>
        /// <returns>An enumeration of <see cref="ManagementObject"/></returns>
        IEnumerable<ManagementObject> QueryWMI(ObjectQuery query);
    }
}
