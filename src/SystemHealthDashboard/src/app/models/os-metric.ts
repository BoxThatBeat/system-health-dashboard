import { MeasurementUnit } from "./measurement-unit.enum";

/**
 * A model that represents any time-stamped OS metric.
 */
export interface OSMetric {

  /** A uniquely identifying key for the metric, e.g. "cpu_usage", "available_memory", etc. */
  key: string,

  /** The name of the metric, e.g. "CPU Usage", "Available Memory", etc. */
  name: string,

  /** The description of the metric. */
  description?: string,

  /** The timestamp of the metric when it was recorded in milliseconds since Unix epoch. */
  timestamp: number,

  /** The value of the metric. For example, if the metric is "CPU Usage", this could be a percentage value like 75.5. */
  value: number,

  /** The total possible value for the metric, if applicable. For example, for "CPU Usage", this could be 100 (representing 100%). */
  total: number,

  /** What this metric is measured in. E.g. Percentage, Bytes, etc. */
  unit: MeasurementUnit
}