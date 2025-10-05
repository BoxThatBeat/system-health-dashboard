using SystemHealthAPI.Models;

namespace SystemHealthAPI.Services
{
    /// <summary>
    /// Main business logic service for handling receiving and processing system metrics.
    /// </summary>
    public interface ISystemMetricsService
    {
        /// <summary>
        /// Store multiple system metrics.
        /// </summary>
        /// <param name="metrics">A list of metrics to store</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents whether the operation was successful or not.</returns>
        Task<bool> AddMetricsAsync(IEnumerable<OSMetric> metrics);

        /// <summary>
        /// Retrieve all stored system metrics.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of system metrics.</returns>
        Task<IEnumerable<OSMetric>> RetrieveAllMetricsAsync();

        /// <summary>
        /// Clears all stored system metrics.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result represents whether the operation was successful or not.</returns>
        Task<bool> ClearAllMetricsAsync();
    }
}
