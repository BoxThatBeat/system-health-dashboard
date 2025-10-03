using SystemHealthAPI.Models;

namespace SystemHealthAPI.Services
{
    /// <summary>
    /// Provides access to system metrics data and handles business logic related to system metrics.
    /// TODO: When metrics come in to the system, check if they exceed any thresholds and trigger alerts if necessary.
    /// </summary>
    public class SystemMetricsService : ISystemMetricsService
    {
        private readonly IDataAccessService _dataAccessService;

        /// <summary>
        /// Creates a new instance of the <see cref="SystemMetricsService"/> class.
        /// </summary>
        /// <param name="dataAccessService">The data access service that interacts with the database</param>
        public SystemMetricsService(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        /// <inheritdoc/>
        public async Task<bool> AddMetricsAsync(IEnumerable<OSMetric> metrics)
        {
            return await _dataAccessService.AddMetricsAsync(metrics);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OSMetric>> RetrieveAllMetricsAsync()
        {
            return await _dataAccessService.RetrieveAllMetricsAsync();
        }
    }
}
