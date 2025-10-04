import { Component, inject, signal } from "@angular/core";
import { Subscription, switchMap } from "rxjs";
import { OSMetric } from "../../models/os-metric";
import { SystemHealthApiService } from "../../services/system-health-api.service";
import { AgCharts } from "ag-charts-angular";
import { AgChartOptions } from 'ag-charts-community';
import { chartTypesConfigMap, measurementUnitMap } from "../../configuration/chart-types.config";
import { ChartType } from "../../models/chart-type.enum";

@Component({
  selector: 'app-dashboard',
  imports: [AgCharts],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent { 
  private readonly systemHealthApiService = inject(SystemHealthApiService);
  private subscriptions = new Subscription();

  chartOptions = signal<AgChartOptions[]>([]);

  constructor() {
    this.retrieveOSMetrics();
  }

  private retrieveOSMetrics() {
    this.subscriptions.add(
      this.systemHealthApiService
        .getMetrics()
        .pipe(
          switchMap(async (metrics: OSMetric[]) => {
            this.setupCharts(metrics);
          }),
        )
        .subscribe());
  }

  private setupCharts(metrics: OSMetric[]) {

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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}