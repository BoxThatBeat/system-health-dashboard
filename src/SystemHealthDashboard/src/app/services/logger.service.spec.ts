import { createServiceFactory, SpectatorService } from '@ngneat/spectator';
import { LoggerService } from './logger.service';

describe('LoggerService', () => {
  let spectator: SpectatorService<LoggerService>;
  let service: LoggerService;
  let consoleSpy: jasmine.Spy;

  const createService = createServiceFactory({
    service: LoggerService
  });

  beforeEach(() => {
    spectator = createService();
    service = spectator.service;
  });

  afterEach(() => {
    // Restore console methods after each test
    if (consoleSpy) {
      consoleSpy.calls.reset();
    }
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('info', () => {
    beforeEach(() => {
      consoleSpy = spyOn(console, 'info');
    });

    it('should call console.info with [INFO] prefix and message', () => {
      const message = 'This is an info message';
      
      service.info(message);
      
      expect(console.info).toHaveBeenCalledWith('[INFO]', message);
      expect(console.info).toHaveBeenCalledTimes(1);
    });

    it('should call console.info with [INFO] prefix, message and optional parameters', () => {
      const message = 'Info message with params';
      const param1 = { key: 'value' };
      const param2 = 'additional info';
      
      service.info(message, param1, param2);
      
      expect(console.info).toHaveBeenCalledWith('[INFO]', message, param1, param2);
      expect(console.info).toHaveBeenCalledTimes(1);
    });

    it('should handle undefined message', () => {
      service.info(undefined);
      
      expect(console.info).toHaveBeenCalledWith('[INFO]', undefined);
    });

    it('should handle null message', () => {
      service.info(null);
      
      expect(console.info).toHaveBeenCalledWith('[INFO]', null);
    });

    it('should handle object as message', () => {
      const objectMessage = { error: 'Something went wrong', code: 500 };
      
      service.info(objectMessage);
      
      expect(console.info).toHaveBeenCalledWith('[INFO]', objectMessage);
    });
  });

  describe('warn', () => {
    beforeEach(() => {
      consoleSpy = spyOn(console, 'warn');
    });

    it('should call console.warn with [WARN] prefix and message', () => {
      const message = 'This is a warning message';
      
      service.warn(message);
      
      expect(console.warn).toHaveBeenCalledWith('[WARN]', message);
      expect(console.warn).toHaveBeenCalledTimes(1);
    });

    it('should call console.warn with [WARN] prefix, message and optional parameters', () => {
      const message = 'Warning message with params';
      const param1 = { deprecated: true };
      const param2 = 'use new method instead';
      
      service.warn(message, param1, param2);
      
      expect(console.warn).toHaveBeenCalledWith('[WARN]', message, param1, param2);
      expect(console.warn).toHaveBeenCalledTimes(1);
    });

    it('should handle multiple optional parameters', () => {
      const message = 'Multiple params warning';
      const params = ['param1', 'param2', 'param3', { key: 'value' }];
      
      service.warn(message, ...params);
      
      expect(console.warn).toHaveBeenCalledWith('[WARN]', message, ...params);
    });
  });

  describe('error', () => {
    beforeEach(() => {
      consoleSpy = spyOn(console, 'error');
    });

    it('should call console.error with [ERROR] prefix and message', () => {
      const message = 'This is an error message';
      
      service.error(message);
      
      expect(console.error).toHaveBeenCalledWith('[ERROR]', message);
      expect(console.error).toHaveBeenCalledTimes(1);
    });

    it('should call console.error with [ERROR] prefix, message and optional parameters', () => {
      const message = 'Error message with params';
      const param1 = new Error('Something failed');
      const param2 = { stackTrace: 'line 1\nline 2' };
      
      service.error(message, param1, param2);
      
      expect(console.error).toHaveBeenCalledWith('[ERROR]', message, param1, param2);
      expect(console.error).toHaveBeenCalledTimes(1);
    });

    it('should handle Error objects as message', () => {
      const errorMessage = new Error('Critical failure');
      
      service.error(errorMessage);
      
      expect(console.error).toHaveBeenCalledWith('[ERROR]', errorMessage);
    });

    it('should handle empty parameters', () => {
      const message = 'Simple error';
      
      service.error(message);
      
      expect(console.error).toHaveBeenCalledWith('[ERROR]', message);
    });
  });
});