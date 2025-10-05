import { HttpClient, HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { createServiceFactory, SpectatorService } from '@ngneat/spectator';
import { of, throwError } from 'rxjs';
import { OSMetric } from '../models/os-metric';
import { MeasurementUnit } from '../models/measurement-unit.enum';
import { LoggerService } from './logger.service';
import { SystemHealthApiService } from './system-health-api.service';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';

describe('SystemHealthApiService', () => {
  let spectator: SpectatorService<SystemHealthApiService>;
  let service: SystemHealthApiService;
  let mockHttpClient: HttpTestingController;
  let loggerService: jasmine.SpyObj<LoggerService>;

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
      key: 'available_memory',
      name: 'Available Memory',
      description: 'Available system memory',
      timestamp: 1696435200100,
      value: 8589934592,
      total: 17179869184,
      unit: MeasurementUnit.Bytes
    }
  ];

  const createService = createServiceFactory({
    service: SystemHealthApiService,
    providers: [provideHttpClient(), provideHttpClientTesting()],
    mocks: [LoggerService]
  });

  beforeEach(() => {
    spectator = createService();
    service = spectator.service;
    mockHttpClient = spectator.inject(HttpTestingController);
    loggerService = spectator.inject(LoggerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getMetrics', () => {
    it('should return metrics when API call is successful', (done) => {
      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual(mockMetrics);
          expect(metrics.length).toBe(2);
          expect(metrics[0].key).toBe('cpu_usage');
          expect(metrics[1].key).toBe('available_memory');
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      expect(req.request.method).toBe('GET');
      
      req.flush(mockMetrics, { status: 200, statusText: 'OK' });
    });

    it('should log an error and return empty array when API call fails', (done) => {
      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          expect(loggerService.error).toHaveBeenCalledWith('Error retrieving system metrics: ', undefined);
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      req.error(new ProgressEvent('error'), { status: 500, statusText: 'Internal Server Error' });
    });

    it('should return empty array when response body is malformed', (done) => {
      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      req.flush(null, { status: 200, statusText: 'OK' });
    });
  });

  describe('deleteMetrics', () => {
    it('should handle successful deletion', (done) => {
      service.deleteMetrics().subscribe({
        next: (result) => {
          expect(result).toBeNull();
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      req.flush(null, { status: 204, statusText: 'No Content' });
    });

    it('should log error and return undefined when delete fails', (done) => {
      service.deleteMetrics().subscribe({
        next: (result) => {
          expect(result).toBeUndefined();
          expect(loggerService.error).toHaveBeenCalledWith('Error deleting system metrics: ', undefined);
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      req.error(new ProgressEvent('error'), { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle delete with error response body', (done) => {
      service.deleteMetrics().subscribe({
        next: (result) => {
          expect(result).toBeUndefined();
          expect(loggerService.error).toHaveBeenCalledWith('Error deleting system metrics: ', 'Failed to delete metrics');
          done();
        },
        error: (error) => {
          fail('Should not have failed: ' + error);
          done();
        }
      });

      const req = mockHttpClient.expectOne('http://localhost:5169/SystemMetrics');
      req.flush({ message: 'Failed to delete metrics' }, { status: 400, statusText: 'Bad Request' });
    });
  });
});