using OSMetricsRetriever.Models;
using OSMetricsRetriever.Exceptions;
using System.Management;
using OSMetricsRetriever.Providers;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// An interface for plugins that retrieve OS metrics.
    /// </summary>
    public interface IRetrieveMetricsPlugin
    {
        /// <summary>
        /// Collects a metric from the operating system.
        /// </summary>
        /// <param name="provider">The WMI provider to use for querying Windows system information.</param>
        /// <returns>A <see cref="OSMetric"/> representing the collected metric at a given time.</returns>
        /// <exception cref="MetricRetrievalException">Thrown if a possible error occurs when trying to retrieve the system metric.</exception>
        public OSMetric GetMetric(IWMIProvider provider);
    }
}
