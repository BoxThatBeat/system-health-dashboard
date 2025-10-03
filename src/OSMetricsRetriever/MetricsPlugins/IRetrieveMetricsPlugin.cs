using OSMetricsRetriever.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace OSMetricsRetriever.MetricsPlugins
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRetrieveMetricsPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public OSMetric GetMetric(ManagementScope scope);
    }
}
