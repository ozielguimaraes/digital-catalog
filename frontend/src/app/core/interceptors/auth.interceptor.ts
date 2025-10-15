import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Add credentials to all requests
    const reqWithCredentials = req.clone({
      withCredentials: true
    });

    // Skip auth for login and register endpoints
    if (this.isAuthEndpoint(req.url)) {
      return next.handle(reqWithCredentials);
    }

    // Add token to request
    const authReq = this.addTokenToRequest(reqWithCredentials);

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && this.authService.isAuthenticated()) {
          // If 401 and user is authenticated, logout and redirect to login
          this.authService.logout().subscribe();
        }
        return throwError(() => error);
      })
    );
  }

  private addTokenToRequest(request: HttpRequest<any>): HttpRequest<any> {
    const token = this.authService.getToken();
    
    if (token) {
      return request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return request;
  }

  private isAuthEndpoint(url: string): boolean {
    const authEndpoints = ['/auth/login', '/auth/register'];
    return authEndpoints.some(endpoint => url.includes(endpoint));
  }
}
