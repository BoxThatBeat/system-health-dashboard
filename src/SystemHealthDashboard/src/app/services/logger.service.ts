import { Injectable } from '@angular/core';

/**
 * A simple logging service that wraps console methods.
 */
@Injectable({
  providedIn: 'root'
})
export class LoggerService {

  /**
   * Logs an informational message to the console.
   * @param message The main message to log
   * @param optionalParams Additional parameters to log
   */
  info(message: any, ...optionalParams: any[]): void {
    console.info('[INFO]', message, ...optionalParams);
  }

  /**
   * Logs a warning message to the console.
   * @param message The main message to log
   * @param optionalParams Additional parameters to log
   */
  warn(message: any, ...optionalParams: any[]): void {
    console.warn('[WARN]', message, ...optionalParams);
  }

  /**
   * Logs an error message to the console.
   * @param message The main message to log
   * @param optionalParams Additional parameters to log
   */
  error(message: any, ...optionalParams: any[]): void {
    console.error('[ERROR]', message, ...optionalParams);
  }
}