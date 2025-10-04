using System.Diagnostics.CodeAnalysis;

namespace OSMetricsRetriever.Exceptions
{
    /// <summary>
    /// An exception thrown when there is an error retrieving a metric.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MetricRetrievalException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MetricRetrievalException"/> class.
        /// </summary>
        public MetricRetrievalException() : base() { }

        /// <summary>
        /// Creates a new instance of the <see cref="MetricRetrievalException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public MetricRetrievalException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of the <see cref="MetricRetrievalException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public MetricRetrievalException(string message, Exception innerException) : base(message, innerException) { }
    }
}
