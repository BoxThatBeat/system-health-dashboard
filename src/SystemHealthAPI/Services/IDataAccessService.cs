using SystemHealthAPI.Models;

namespace SystemHealthAPI.Services
{
    /// <summary>
    /// An abstraction of the data access layer for storing and retrieving system metrics.
    /// </summary>
    public interface IDataAccessService
    {
        /// <summary>
        /// Store a system metric.
        /// </summary>
        /// <param name="metric">The metric to store.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents whether the operation was successful or not.</returns>
        Task<bool> AddMetricAsync(OSMetric metric);

        /// <summary>
        /// Store multiple system metrics.
        /// </summary>
        /// <param name="metrics">A list of metrics to store</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents whether the operation was successful or not.</returns>
        Task<bool> AddMetricsAsync(IEnumerable<OSMetric> metrics);


        /// <summary>
        /// Retrieve system metrics within a specified time range.
        /// </summary>
        /// <param name="startTime">The start time of the range (inclusive).</param>
        /// <param name="endTime">The end time of the range (inclusive).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of system metrics.</returns>
        Task<IEnumerable<OSMetric>> RetrieveMetricsAsync(double startTime, double endTime);

        /// <summary>
        /// Retrieve all stored system metrics.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of system metrics.</returns>
        Task<IEnumerable<OSMetric>> RetrieveAllMetricsAsync();

        /// <summary>
        /// Deletes all stored system metrics.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result represents whether the operation was successful or not.</returns>
        Task<bool> ClearAllMetricsAsync();
    }
}
