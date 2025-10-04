import { ChartType } from "../models/chart-type.enum";
import { AgChartOptions } from 'ag-charts-community';
import { MeasurementUnit } from "../models/measurement-unit.enum";

export const measurementUnitMap = new Map<MeasurementUnit, string>([
  [MeasurementUnit.Percentage, "%"],
  [MeasurementUnit.Bytes, "Bytes"],
]);

/**
 * Configuration map for different chart types. Additional 
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