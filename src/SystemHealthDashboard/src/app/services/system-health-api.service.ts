import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { catchError, map, Observable, of } from "rxjs";
import { OSMetric } from "../models/os-metric";
import { LoggerService } from "./logger.service";
import { environment } from "../../environments/environment";

/**
 * A service to interact with the System Health API.
 */
@Injectable({
  providedIn: 'root',
})
export class SystemHealthApiService {

  private readonly httpClient = inject(HttpClient);
  private readonly loggerService = inject(LoggerService);

  private readonly endpointUrl = `${environment.apiUrl}/SystemMetrics`;

  /**
   * Retrieves OS metrics from the API.
   * @returns An observable stream of OS metrics retrieved from the API.
   */
  public getMetrics(): Observable<OSMetric[]> {
    return this.httpClient
      .get<OSMetric[]>(this.endpointUrl, {
        observe: 'response',
      })
      .pipe(
        map((responseBody: object) => {
          if (responseBody?.hasOwnProperty('body')) {
            const metrics = (responseBody as any)['body'] as OSMetric[];
            if (metrics) {
              return metrics;
            }
          }
          return [];
        }),
        catchError((error: HttpErrorResponse) => {
          this.loggerService.error('Error retrieving system metrics: ', error.error?.message);
          return of([]);
        }),
      );
  }

  /**
   * Deletes all system metrics.
   * @returns An observable that completes when the delete operation is done.
   */
  public deleteMetrics(): Observable<void> {
    return this.httpClient
      .delete<void>(this.endpointUrl)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.loggerService.error('Error deleting system metrics: ', error.error?.message);
          return of(undefined);
        }),
      );
  }
}