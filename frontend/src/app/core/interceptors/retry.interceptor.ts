import { HttpRequest, HttpHandlerFn, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, timer, retry } from 'rxjs';

const MAX_RETRIES = 3;
const BASE_DELAY_MS = 500;
const RETRYABLE_STATUSES = new Set<number>([429, 502, 503, 504]);

export function retryInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  return next(req).pipe(
    retry({
      count: MAX_RETRIES,
      delay: (error, retryCount) => {
        if (!shouldRetry(error)) {
          return throwError(() => error);
        }
        return timer(computeDelay(error, retryCount));
      }
    })
  );
}

function shouldRetry(error: unknown): boolean {
  const status = (error as { status?: unknown })?.status;
  if (typeof status !== 'number') return false;
  // status 0 = network/CORS/offline
  if (status === 0) return true;
  return RETRYABLE_STATUSES.has(status);
}

function computeDelay(error: HttpErrorResponse, retryCount: number): number {
  const headers = (error as { headers?: { get?: (name: string) => string | null } })?.headers;
  const retryAfterMs = parseRetryAfter(headers?.get?.('Retry-After'));
  if (retryAfterMs !== null) return retryAfterMs;
  return BASE_DELAY_MS * Math.pow(2, retryCount - 1);
}

function parseRetryAfter(header: string | null | undefined): number | null {
  if (!header) return null;
  const seconds = Number(header);
  if (Number.isFinite(seconds)) return Math.max(0, seconds * 1000);
  const dateMs = Date.parse(header);
  if (!Number.isNaN(dateMs)) return Math.max(0, dateMs - Date.now());
  return null;
}
