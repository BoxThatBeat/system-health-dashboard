using System.Collections.Concurrent;
using SystemHealthAPI.Exceptions;
using SystemHealthAPI.Models;

namespace SystemHealthAPI.Services
{
    /// <summary>
    /// An implementation of IDataAccessService that stores metrics in memory.
    /// NOTE: This implementation does not need to be asynchronous but any real database would require async data access.
    /// </summary>
    public class InMemoryDataAccessService : IDataAccessService
    {
        private ConcurrentBag<OSMetric> _metrics;

        /// <inheritdoc/>
        public InMemoryDataAccessService()
        {
            _metrics = new ConcurrentBag<OSMetric>();
        }

        /// <inheritdoc/>
        public Task<bool> AddMetricAsync(OSMetric metric)
        {
            _metrics.Add(metric);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> AddMetricsAsync(IEnumerable<OSMetric> metrics)
        {
            metrics.ToList().ForEach(m => _metrics.Add(m));
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<OSMetric>> RetrieveAllMetricsAsync()
        {
            if (_metrics.Count == 0) throw new MetricsNotAvailableException("No metrics available");

            return Task.FromResult(_metrics.AsEnumerable());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<OSMetric>> RetrieveMetricsAsync(double startTime, double endTime)
        {
            if (_metrics.Count == 0) throw new MetricsNotAvailableException("No metrics available");

            return Task.FromResult(_metrics.Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime).AsEnumerable());
        }


        /// <inheritdoc/>
        public Task<bool> ClearAllMetricsAsync()
        {
            _metrics.Clear();
            return Task.FromResult(true);
        }
    }
}
