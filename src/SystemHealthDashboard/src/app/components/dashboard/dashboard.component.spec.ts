import { ComponentFixture } from '@angular/core/testing';
import { createComponentFactory, Spectator } from '@ngneat/spectator';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { SystemHealthApiService } from '../../services/system-health-api.service';
import { OSMetric } from '../../models/os-metric';
import { MeasurementUnit } from '../../models/measurement-unit.enum';
import { ChartType } from '../../models/chart-type.enum';
import { AgChartOptions } from 'ag-charts-community';

describe('DashboardComponent', () => {
  let spectator: Spectator<DashboardComponent>;
  let component: DashboardComponent;
  let systemHealthApiService: jasmine.SpyObj<SystemHealthApiService>;

  const mockMetrics: OSMetric[] = [
    {
      key: 'cpu_usage',
      name: 'CPU Usage',
      description: 'Current CPU usage percentage',
      timestamp: 1696435200000,
      value: 75.5,
      total: 100,
      unit: MeasurementUnit.Percentage
    },
    {
      key: 'cpu_usage',
      name: 'CPU Usage',
      description: 'Current CPU usage percentage',
      timestamp: 1696435261000,
      value: 80.2,
      total: 100,
      unit: MeasurementUnit.Percentage
    },
    {
      key: 'available_memory',
      name: 'Available Memory',
      description: 'Available system memory',
      timestamp: 1696435200000,
      value: 8589934592, // 8 GB in bytes
      total: 17179869184, // 16 GB in bytes
      unit: MeasurementUnit.Bytes
    },
    {
      key: 'available_memory',
      name: 'Available Memory',
      description: 'Available system memory',
      timestamp: 1696435261000,
      value: 7516192768, // 7 GB in bytes
      total: 17179869184, // 16 GB in bytes
      unit: MeasurementUnit.Bytes
    }
  ];

  const createComponent = createComponentFactory({
    component: DashboardComponent,
    mocks: [SystemHealthApiService],
    shallow: true
  });

  beforeEach(() => {
    spectator = createComponent();
    component = spectator.component;
    systemHealthApiService = spectator.inject(SystemHealthApiService);
    
    // Reset component state for each test
    component.chartOptions.set([]);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty chartOptions', () => {
    expect(component.chartOptions()).toEqual([]);
  });

  describe('retrieveOSMetrics', () => {
    it('should call systemHealthApiService.getMetrics on component initialization', () => {
      expect(systemHealthApiService.getMetrics).toHaveBeenCalledTimes(1);
    });

    it('should setup charts when metrics are retrieved successfully', () => {
      // Reset chartOptions and set up mock
      component.chartOptions.set([]);
      systemHealthApiService.getMetrics.and.returnValue(of(mockMetrics));
      
      // Call the method directly to test behavior
      component['retrieveOSMetrics']();
      
      // Wait for async operations to complete
      spectator.fixture.detectChanges();
      
      expect(component.chartOptions().length).toBe(2); // 2 unique metric keys
    });
  });

  describe('setupCharts', () => {
    beforeEach(() => {
      // Reset chartOptions before each test
      component.chartOptions.set([]);
    });

    it('should create charts for unique metric keys', () => {
      systemHealthApiService.getMetrics.and.returnValue(of(mockMetrics));
      
      // Manually call setupCharts to test the method directly
      component['setupCharts'](mockMetrics);
      
      const charts = component.chartOptions();
      expect(charts.length).toBe(2); // cpu_usage and available_memory
    });

    it('should sort metrics by timestamp for each chart', () => {
      const unsortedMetrics = [...mockMetrics].reverse(); // Reverse the order
      systemHealthApiService.getMetrics.and.returnValue(of(unsortedMetrics));
      
      component['setupCharts'](unsortedMetrics);
      
      const charts = component.chartOptions();
      const cpuChart = charts.find(chart => chart.title?.text === 'CPU Usage') as any;
      
      expect(cpuChart).toBeDefined();
      expect(cpuChart?.data?.[0]?.timestamp).toBeLessThan(cpuChart?.data?.[1]?.timestamp);
    });

    it('should convert bytes to GB for memory metrics', () => {
      const memoryMetrics = mockMetrics.filter(m => m.unit === MeasurementUnit.Bytes);
      
      component['setupCharts'](memoryMetrics);
      
      const charts = component.chartOptions();
      const memoryChart = charts.find(chart => chart.title?.text === 'Available Memory') as any;
      
      expect(memoryChart).toBeDefined();
      expect(memoryChart?.data?.[0]?.value).toBe(8.00); // 8589934592 bytes = 8 GB
      expect(memoryChart?.data?.[0]?.total).toBe(16.00); // 17179869184 bytes = 16 GB
    });

    it('should not convert percentage metrics', () => {
      const percentageMetrics = mockMetrics.filter(m => m.unit === MeasurementUnit.Percentage);
      
      component['setupCharts'](percentageMetrics);
      
      const charts = component.chartOptions();
      const cpuChart = charts.find(chart => chart.title?.text === 'CPU Usage') as any;
      
      expect(cpuChart).toBeDefined();
      expect(cpuChart?.data?.[0]?.value).toBe(75.5); // Original value unchanged
      expect(cpuChart?.data?.[0]?.total).toBe(100); // Original total unchanged
    });

    it('should set chart title from first data point name', () => {
      component['setupCharts'](mockMetrics);
      
      const charts = component.chartOptions();
      const cpuChart = charts.find(chart => chart.title?.text === 'CPU Usage') as any;
      const memoryChart = charts.find(chart => chart.title?.text === 'Available Memory') as any;
      
      expect(cpuChart).toBeDefined();
      expect(memoryChart).toBeDefined();
      expect(cpuChart?.title?.text).toBe('CPU Usage');
      expect(memoryChart?.title?.text).toBe('Available Memory');
    });

    it('should add number axis with correct min/max values', () => {
      component['setupCharts'](mockMetrics);
      
      const charts = component.chartOptions();
      const cpuChart = charts.find(chart => chart.title?.text === 'CPU Usage') as any;
      
      expect(cpuChart).toBeDefined();
      
      // Find the number axis (should be the last one added)
      const numberAxis = cpuChart?.axes?.find((axis: any) => axis.type === 'number');
      expect(numberAxis).toBeDefined();
      expect(numberAxis?.min).toBe(0);
      expect(numberAxis?.max).toBe(100); // Total value for CPU usage
      expect(numberAxis?.position).toBe('left');
    });

    it('should add axis with correct label format for percentage metrics', () => {
      const percentageMetrics = mockMetrics.filter(m => m.unit === MeasurementUnit.Percentage);
      
      component['setupCharts'](percentageMetrics);
      
      const charts = component.chartOptions();
      const cpuChart = charts.find(chart => chart.title?.text === 'CPU Usage') as any;
      
      expect(cpuChart).toBeDefined();
      
      const numberAxis = cpuChart?.axes?.find((axis: any) => axis.type === 'number');
      expect(numberAxis?.label?.format).toContain('%');
    });

    it('should add axis with correct label format for memory metrics', () => {
      const memoryMetrics = mockMetrics.filter(m => m.unit === MeasurementUnit.Bytes);
      
      component['setupCharts'](memoryMetrics);
      
      const charts = component.chartOptions();
      const memoryChart = charts.find(chart => chart.title?.text === 'Available Memory') as any;
      
      expect(memoryChart).toBeDefined();
      
      const numberAxis = memoryChart?.axes?.find((axis: any) => axis.type === 'number');
      expect(numberAxis?.label?.format).toContain('GB');
    });

    it('should handle empty metrics array gracefully', () => {
      component['setupCharts']([]);
      
      const charts = component.chartOptions();
      expect(charts.length).toBe(0);
    });

    it('should handle invalid chart config gracefully', () => {
      // Test with metrics that might cause issues
      const invalidMetrics = [{ ...mockMetrics[0], unit: null as any }];
      
      expect(() => component['setupCharts'](invalidMetrics)).not.toThrow();
      
      // Should not throw error and should handle gracefully
      expect(component.chartOptions()).toBeDefined();
    });

    it('should update chartOptions signal with new chart configs', () => {
      const initialLength = component.chartOptions().length;
      
      component['setupCharts'](mockMetrics);
      
      expect(component.chartOptions().length).toBeGreaterThan(initialLength);
    });
  });

  describe('ngOnDestroy', () => {
    it('should unsubscribe from all subscriptions', () => {
      spyOn(component['subscriptions'], 'unsubscribe');
      
      component.ngOnDestroy();
      
      expect(component['subscriptions'].unsubscribe).toHaveBeenCalled();
    });
  });

  describe('integration tests', () => {
    it('should create correct chart structure from end to end', () => {
      // Reset chartOptions and set up mock
      component.chartOptions.set([]);
      systemHealthApiService.getMetrics.and.returnValue(of(mockMetrics));
      
      // Trigger the retrieval process
      component['retrieveOSMetrics']();
      spectator.fixture.detectChanges();
      
      const charts = component.chartOptions();
      
      expect(charts.length).toBe(2);
      
      // Verify chart structure
      charts.forEach(chart => {
        const chartAny = chart as any;
        expect(chart).toHaveProperty('title');
        expect(chart).toHaveProperty('data');
        expect(chartAny).toHaveProperty('axes');
        expect(chartAny.axes?.length).toBeGreaterThan(1); // Should have time axis + number axis
        expect(Array.isArray(chart.data)).toBe(true);
        expect(chart.data?.length).toBeGreaterThan(0);
      });
    });
  });
});
