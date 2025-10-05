import { ComponentFixture } from '@angular/core/testing';
import { createComponentFactory, Spectator } from '@ngneat/spectator';
import { of } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { SystemHealthApiService } from '../../services/system-health-api.service';
import { OSMetric } from '../../models/os-metric';
import { MeasurementUnit } from '../../models/measurement-unit.enum';

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
  });

  beforeEach(() => {

    let systemHealthApiServiceSpy = jasmine.createSpyObj('SystemHealthApiService', ['getMetrics', 'deleteMetrics']);
    systemHealthApiServiceSpy.getMetrics.and.returnValue(of(mockMetrics));
    systemHealthApiServiceSpy.deleteMetrics.and.returnValue(of(null));

    spectator = createComponent(
      {
        providers: [{provide: SystemHealthApiService, useValue: systemHealthApiServiceSpy}]
      }
    );
    component = spectator.component;
    systemHealthApiService = spectator.inject(SystemHealthApiService);
    
    // Reset component state for each test
    component.chartOptions.set([]);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should retrieve and display charts on creation', () => {
    expect(systemHealthApiService.getMetrics).toHaveBeenCalled();
    expect(component.chartOptions()).toEqual(jasmine.any(Array));
  });

  it('should call deleteMetrics and clear charts when clearAllMetrics is called', () => {
    component.deleteMetrics();
    expect(systemHealthApiService.deleteMetrics).toHaveBeenCalled();
    expect(component.chartOptions()).toEqual([]);
  });
});
