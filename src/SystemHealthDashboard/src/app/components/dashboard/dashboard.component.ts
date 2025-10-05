import { Component, inject, signal } from "@angular/core";
import { Subscription } from "rxjs";
import { OSMetric } from "../../models/os-metric";
import { SystemHealthApiService } from "../../services/system-health-api.service";
import { AgCharts } from "ag-charts-angular";
import { AgChartOptions } from 'ag-charts-community';
import { chartTypesConfigMap, measurementUnitMap } from "../../configuration/chart-types.config";
import { ChartType } from "../../models/chart-type.enum";
import { MeasurementUnit } from "../../models/measurement-unit.enum";

/**
 * A simple dashboard of charts representing various OS metrics.
 */
@Component({
  selector: 'app-dashboard',
  imports: [AgCharts],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent { 
  private readonly systemHealthApiService = inject(SystemHealthApiService);
  private subscriptions = new Subscription();

  /**
   * Holds the chart options for each metric chart to be rendered.
   */
  chartOptions = signal<AgChartOptions[]>([]);

  constructor() {
    this.retrieveOSMetrics();
  }

  /**
   * Refreshes the metrics by re-fetching from the API and updating the charts.
   */
  public refreshMetrics() {
    this.retrieveOSMetrics();
  }

  /**
   * Deletes all metrics via the API and clears the charts.
   */
  public deleteMetrics() {
    this.subscriptions.add(
      this.systemHealthApiService
        .deleteMetrics()
        .subscribe(() => this.chartOptions.set([]))
    );
  }

  private retrieveOSMetrics() {
    this.subscriptions.add(
      this.systemHealthApiService
        .getMetrics()
        .subscribe(metrics => this.setupCharts(metrics))
    );
  }

  private setupCharts(metrics: OSMetric[]) {

    // clear existing charts
    this.chartOptions.set([]);

    // Get list of unique metric keys
    const uniqueMetricKeys = new Set(metrics.map(m => m.key));

    uniqueMetricKeys.forEach(key => {
      const metricData = metrics.filter(m => m.key === key);
      metricData.sort((a, b) => a.timestamp - b.timestamp);

      // Make a deep copy of the default chart configuration
      const defaultConfig = chartTypesConfigMap.get(ChartType.Line);
      let lineChart = JSON.parse(JSON.stringify(defaultConfig));
      if (!lineChart || !lineChart.title || !lineChart.axes) {
        return;
      }

      const firstDataPoint = metricData[0];

      if (firstDataPoint.unit === MeasurementUnit.Bytes) {
        // Convert to GB and round to 2 decimal places
        metricData.forEach(m => {
          m.value = this.convertBytesToGB(m.value);
          m.total = this.convertBytesToGB(m.total);
        });
      }

      lineChart.axes.push(
        {
          type: "number",
          min: 0,
          max: firstDataPoint.total,
          position: "left",
          label: {
            format: `#{~f} ${measurementUnitMap.get(firstDataPoint.unit) || ""}`,
          },
        },
      )

      lineChart.title.text = firstDataPoint.name;
      lineChart.data = metricData;

      this.chartOptions.update(options => [...options, lineChart as AgChartOptions]);
    });
  }

  private convertBytesToGB(bytes: number): number {
    return +(bytes / (1024 * 1024 * 1024)).toFixed(2);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}