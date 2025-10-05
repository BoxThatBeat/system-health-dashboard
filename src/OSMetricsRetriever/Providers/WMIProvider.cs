using System.Management;

namespace OSMetricsRetriever.Providers
{
    /// <inheritdoc/>
    public class WMIProvider: IWMIProvider
    {
        private readonly ManagementScope scope;

        /// <summary>
        /// Creates a reusable WMI provider with the given management scope.
        /// </summary>
        /// <param name="scope">The WMI scope</param>
        public WMIProvider(ManagementScope scope)
        {
            this.scope = scope;
        }

        /// <inheritdoc/>
        public IEnumerable<ManagementObject> QueryWMI(ObjectQuery query)
        {
            return new ManagementObjectSearcher(scope, query).Get().Cast<ManagementObject>();
        }
    }
}
