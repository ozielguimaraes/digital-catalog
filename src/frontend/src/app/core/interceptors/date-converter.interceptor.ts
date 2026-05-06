import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DateUtils } from '../utils/date.utils';

@Injectable()
export class DateConverterInterceptor implements HttpInterceptor {
  /**
   * Intercepts HTTP requests to convert local dates to UTC before sending to backend
   */
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Only process requests to our API
    if (req.url.includes('/api/') || req.url.includes('localhost:5000')) {
      const convertedBody = this.convertDatesToUtc(req.body);
      
      if (convertedBody !== req.body) {
        const newReq = req.clone({
          body: convertedBody
        });
        return next.handle(newReq);
      }
    }

    return next.handle(req);
  }

  /**
   * Recursively converts all date strings in an object to UTC
   * @param obj - Object to process
   * @returns Object with converted dates
   */
  private convertDatesToUtc(obj: any): any {
    if (obj === null || obj === undefined) {
      return obj;
    }

    if (obj instanceof Date) {
      return DateUtils.toUtc(obj);
    }

    if (typeof obj === 'string') {
      // Check if it's a date string that needs conversion
      if (this.isDateString(obj)) {
        const localDate = new Date(obj);
        if (!isNaN(localDate.getTime())) {
          return DateUtils.toUtc(localDate);
        }
      }
      return obj;
    }

    if (Array.isArray(obj)) {
      return obj.map(item => this.convertDatesToUtc(item));
    }

    if (typeof obj === 'object') {
      const converted: any = {};
      for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
          converted[key] = this.convertDatesToUtc(obj[key]);
        }
      }
      return converted;
    }

    return obj;
  }

  /**
   * Checks if a string looks like a date string
   * @param str - String to check
   * @returns True if the string looks like a date
   */
  private isDateString(str: string): boolean {
    // Check for common date patterns
    const datePatterns = [
      /^\d{4}-\d{2}-\d{2}$/, // YYYY-MM-DD
      /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/, // YYYY-MM-DDTHH:mm:ss
      /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/, // YYYY-MM-DDTHH:mm
    ];

    return datePatterns.some(pattern => pattern.test(str));
  }
}
