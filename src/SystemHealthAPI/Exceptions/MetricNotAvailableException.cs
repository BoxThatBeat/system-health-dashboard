namespace SystemHealthAPI.Exceptions
{
    /// <summary>
    /// An exception thrown when there are no metrics available on the system.
    /// </summary>
    public class MetricsNotAvailableException: Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MetricsNotAvailableException"/> class.
        /// </summary>
        public MetricsNotAvailableException() : base() { }

        /// <summary>
        /// Creates a new instance of the <see cref="MetricsNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public MetricsNotAvailableException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of the <see cref="MetricsNotAvailableException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public MetricsNotAvailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}
