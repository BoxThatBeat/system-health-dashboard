using OSMetricsRetriever.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMetricsRetriever
{


    public class MetricsRetriever
    {
        private List<MetricsPlugins.IRetrieveMetricsPlugin> _plugins;

        private MetricsRetriever(MetricsReceiverBuilder builder)
        {
            this._plugins = builder.Plugins;
        }

        public List<OSMetric> CollectMetrics()
        {
            var metrics = new List<OSMetric>();

            // initilize a management scope for WMI queries
            var scope = new System.Management.ManagementScope(@"\\.\root\cimv2");

            foreach (var plugin in this._plugins)
            {
                metrics.Add(plugin.GetMetric(scope));
            }
            return metrics;
        }


        public class MetricsReceiverBuilder
        {
            public List<MetricsPlugins.IRetrieveMetricsPlugin> Plugins;

            public MetricsReceiverBuilder()
            {
                this.Plugins = new List<MetricsPlugins.IRetrieveMetricsPlugin>();
            }

            public MetricsReceiverBuilder AddPlugin<T>() where T : MetricsPlugins.IRetrieveMetricsPlugin, new()
            {
                this.Plugins.Add(new T());

                return this;
            }

            //public MetricsReceiverBuilder AddConfiguration()


            public MetricsRetriever Build()
            {
                // Implementation to build and return a MetricsRetriever instance
                return new MetricsRetriever(this);
            }
        }


    }

    
}
