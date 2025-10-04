import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { createServiceFactory, SpectatorService } from '@ngneat/spectator';
import { of, throwError } from 'rxjs';
import { OSMetric } from '../models/os-metric';
import { MeasurementUnit } from '../models/measurement-unit.enum';
import { LoggerService } from './logger.service';
import { SystemHealthApiService } from './system-health-api.service';

describe('SystemHealthApiService', () => {
  let spectator: SpectatorService<SystemHealthApiService>;
  let service: SystemHealthApiService;
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
      timestamp: 1696435200000,
      value: 8589934592,
      total: 17179869184,
      unit: MeasurementUnit.Bytes
    }
  ];

  const createService = createServiceFactory({
    service: SystemHealthApiService,
    mocks: [HttpClient, LoggerService]
  });

  beforeEach(() => {
    spectator = createService();
    service = spectator.service;
    loggerService = spectator.inject(LoggerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getMetrics', () => {
    it('should return metrics when API call is successful', (done) => {
      const mockResponse = {
        body: mockMetrics,
        status: 200,
        statusText: 'OK'
      };

      spectator.inject(HttpClient).get.and.returnValue(of(mockResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual(mockMetrics);
          expect(metrics).toHaveLength(2);
          expect(metrics[0].key).toBe('cpu_usage');
          expect(metrics[1].key).toBe('available_memory');
          done();
        },
        error: () => fail('Expected successful response')
      });
    });

    it('should return empty array when response body is null', (done) => {
      const mockResponse = {
        body: null,
        status: 200,
        statusText: 'OK'
      };

      spectator.inject(HttpClient).get.and.returnValue(of(mockResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          expect(metrics).toHaveLength(0);
          done();
        },
        error: () => fail('Expected successful response with empty array')
      });
    });

    it('should return empty array when response does not have body property', (done) => {
      const mockResponse = {
        data: mockMetrics, // Different property name
        status: 200,
        statusText: 'OK'
      };

      spectator.inject(HttpClient).get.and.returnValue(of(mockResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          expect(metrics).toHaveLength(0);
          done();
        },
        error: () => fail('Expected successful response with empty array')
      });
    });

    it('should handle HTTP error and log error message', (done) => {
      const errorResponse = new HttpErrorResponse({
        error: { message: 'Internal Server Error' },
        status: 500,
        statusText: 'Internal Server Error',
        url: 'http://localhost:5169/SystemMetrics'
      });

      spectator.inject(HttpClient).get.and.returnValue(throwError(() => errorResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          expect(loggerService.error).toHaveBeenCalledWith(
            'Error retrieving system metrics: ',
            'Internal Server Error'
          );
          done();
        },
        error: () => fail('Expected error to be caught and handled')
      });
    });

    it('should handle HTTP error without error message', (done) => {
      const errorResponse = new HttpErrorResponse({
        error: {},
        status: 404,
        statusText: 'Not Found',
        url: 'http://localhost:5169/SystemMetrics'
      });

      spectator.inject(HttpClient).get.and.returnValue(throwError(() => errorResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          expect(loggerService.error).toHaveBeenCalledWith(
            'Error retrieving system metrics: ',
            undefined
          );
          done();
        },
        error: () => fail('Expected error to be caught and handled')
      });
    });

    it('should make HTTP GET request with correct parameters', () => {
      const httpClient = spectator.inject(HttpClient);
      httpClient.get.and.returnValue(of({ body: [] }));

      service.getMetrics().subscribe();

      expect(httpClient.get).toHaveBeenCalledWith(
        'http://localhost:5169/SystemMetrics',
        { observe: 'response' }
      );
    });

    it('should return empty array when metrics array is falsy but body exists', (done) => {
      const mockResponse = {
        body: null, // Falsy metrics
        status: 200,
        statusText: 'OK'
      };

      spectator.inject(HttpClient).get.and.returnValue(of(mockResponse));

      service.getMetrics().subscribe({
        next: (metrics) => {
          expect(metrics).toEqual([]);
          done();
        },
        error: () => fail('Expected successful response with empty array')
      });
    });
  });
});