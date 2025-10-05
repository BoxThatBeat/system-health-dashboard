import { ChartType } from "../models/chart-type.enum";
import { MeasurementUnit } from "../models/measurement-unit.enum";

/**
 * A mapping of MeasurementUnit enum values to their string representations for chart labeling.
 */
export const measurementUnitMap = new Map<MeasurementUnit, string>([
  [MeasurementUnit.Percentage, "%"],
  [MeasurementUnit.Bytes, "GB"],
]);

/**
 * Configuration map for different chart types.
 */
export const chartTypesConfigMap = new Map<ChartType, any>([
  [ChartType.Line, {
      title: {
        text: "Line Chart",
      },
      series: [
        {
          type: "line",
          xKey: "timestamp",
          yKey: "value",
        },
      ],
      axes: [
        {
          type: "time", // Continuous Time Axis
          nice: false,
          position: "bottom",
        }
      ],
    }
  ],   
]);