using OSMetricsRetriever.Exceptions;
using OSMetricsRetriever.Models;

namespace OSMetricsRetriever.Services
{

    /// <summary>
    /// A service that retrieves OS metrics using configured plugins.
    /// </summary>
    public class MetricsRetrieverService
    {
        private List<MetricsPlugins.IRetrieveMetricsPlugin> _plugins;

        private MetricsRetrieverService(MetricsReceiverServiceBuilder builder)
        {
            _plugins = builder.Plugins;
        }

        /// <summary>
        /// Collects all metrics using the configured plugins.
        /// </summary>
        /// <returns>A list of <see cref="OSMetric"/></returns>
        public List<OSMetric> CollectMetrics()
        {
            var metrics = new List<OSMetric>();

            // initialize a management scope for WMI queries
            var scope = new System.Management.ManagementScope(@"\\.\root\cimv2");
            scope.Connect();

            foreach (var plugin in _plugins)
            {
                try
                {
                    metrics.Add(plugin.GetMetric(scope));
                }
                catch (MetricRetrievalException ex)
                {
                    // Log and continue with other plugins. Allow general exceptions to bubble up.
                    Console.WriteLine($"Error occurred while retrieving metric {plugin.GetType().Name}: {ex.Message}");
                }
            }

            return metrics;
        }


        /// <summary>
        /// The builder for the MetricsRetrieverService
        /// </summary>
        public class MetricsReceiverServiceBuilder
        {
            public List<MetricsPlugins.IRetrieveMetricsPlugin> Plugins { get; private set; }

            public MetricsReceiverServiceBuilder()
            {
                Plugins = [];
            }

            /// <summary>
            /// Adds a plugin to be created and used by the service.
            /// </summary>
            /// <typeparam name="T">The plugin type, must implement <see cref="MetricsPlugins.IRetrieveMetricsPlugin"/></typeparam>
            /// <returns>the builder instance to allow chaining of builder calls.</returns>
            public MetricsReceiverServiceBuilder AddPlugin<T>() where T : MetricsPlugins.IRetrieveMetricsPlugin, new()
            {
                Plugins.Add(new T());

                return this;
            }

            /// <summary>
            /// Builds the <see cref="MetricsRetrieverService"/> instance.
            /// </summary>
            /// <returns>The built <see cref="MetricsRetrieverService"/></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public MetricsRetrieverService Build()
            {
                if (!Plugins.Any())
                {
                    throw new InvalidOperationException("At least one plugin must be added before building the service");
                }

                return new MetricsRetrieverService(this);
            }
        }


    }

    
}
