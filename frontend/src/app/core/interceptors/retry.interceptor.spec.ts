import { HttpErrorResponse, HttpHeaders, HttpRequest } from '@angular/common/http';
import { firstValueFrom, of, throwError } from 'rxjs';
import { retryInterceptor } from './retry.interceptor';

// Usa timers reais. Os retries são curtos (500ms base) e o total <= 3.5s,
// dentro do limite default do Jasmine (5s).
describe('retryInterceptor', () => {
  function makeReq(): HttpRequest<unknown> {
    return new HttpRequest('GET', '/api/v1/x');
  }

  function makeError(status: number, headers?: { [k: string]: string }): HttpErrorResponse {
    return new HttpErrorResponse({
      status,
      headers: headers ? new HttpHeaders(headers) : undefined
    });
  }

  it('does not retry on 4xx errors (except 429)', async () => {
    let calls = 0;
    const next = () => {
      calls++;
      return throwError(() => makeError(400));
    };

    await expectAsync(firstValueFrom(retryInterceptor(makeReq(), next))).toBeRejected();
    expect(calls).toBe(1);
  });

  it('does not retry on non-HttpErrorResponse errors', async () => {
    let calls = 0;
    const next = () => {
      calls++;
      return throwError(() => new Error('plain error'));
    };

    await expectAsync(firstValueFrom(retryInterceptor(makeReq(), next))).toBeRejected();
    expect(calls).toBe(1);
  });

  it('retries on 503 up to 3 times then propagates (1+3 calls)', async () => {
    let calls = 0;
    const next = () => {
      calls++;
      return throwError(() => makeError(503));
    };

    await expectAsync(firstValueFrom(retryInterceptor(makeReq(), next))).toBeRejected();
    expect(calls).toBe(4);
  });

  it('retries on network error (status 0) and succeeds', async () => {
    let calls = 0;
    const next = () => {
      calls++;
      if (calls === 1) return throwError(() => makeError(0));
      return of({} as any);
    };

    const result = await firstValueFrom(retryInterceptor(makeReq(), next));
    expect(calls).toBe(2);
    expect(result).toBeDefined();
  });

  it('retries on 429 (rate limit) and succeeds on second attempt', async () => {
    let calls = 0;
    const next = () => {
      calls++;
      if (calls === 1) return throwError(() => makeError(429));
      return of({} as any);
    };

    await firstValueFrom(retryInterceptor(makeReq(), next));
    expect(calls).toBe(2);
  });
});
