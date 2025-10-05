using System.Diagnostics.CodeAnalysis;

namespace OSMetricsRetriever.Models
{
    /// <summary>
    /// A model that represents any time-stamped OS metric.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OSMetric()
    {
        /// <summary>
        /// A uniquely identifying key for the metric, e.g. "cpu_usage", "available_memory", etc.
        /// </summary>
        public required string Key { get; set; }

        /// <summary>
        /// The name of the metric, e.g. "CPU Usage", "Available Memory", etc.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The description of the metric.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The timestamp of the metric when it was recorded in milliseconds since Unix epoch.
        /// </summary>
        public double Timestamp { get; set; }

        /// <summary>
        /// The value of the metric. For example, if the metric is "CPU Usage", this could be a percentage value like 75.5.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The total possible value for the metric, if applicable. For example, for "CPU Usage", this could be 100 (representing 100%).
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// What this metric is measured in. E.g. Percentage, Bytes, etc.
        /// </summary>
        public MeasurementUnit Unit { get; set; }
    }
}
