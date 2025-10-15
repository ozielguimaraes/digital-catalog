import { HttpInterceptorFn, HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, switchMap, catchError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Add credentials to all requests
  const reqWithCredentials = req.clone({
    withCredentials: true
  });

  // Skip auth for login and register endpoints
  if (isAuthEndpoint(req.url)) {
    return next(reqWithCredentials);
  }

  // Add token to request
  const authReq = addTokenToRequest(reqWithCredentials, authService);

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === HttpStatusCode.Unauthorized && authService.isAuthenticated()) {
        // Try to refresh token if 401 and user is authenticated
        return authService.refreshToken().pipe(
          switchMap(() => {
            // Retry the original request with new token
            const newToken = authService.getToken();
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`
              }
            });
            return next(retryReq);
          }),
          catchError(() => {
            // If refresh fails, logout and redirect to login
            authService.logout().subscribe();
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

function addTokenToRequest(request: any, authService: AuthService): any {
  const token = authService.getToken();
  
  if (token) {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return request;
}

function isAuthEndpoint(url: string): boolean {
  const authEndpoints = ['/auth/login', '/auth/register'];
  return authEndpoints.some(endpoint => url.includes(endpoint));
}
