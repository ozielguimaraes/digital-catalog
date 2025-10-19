import { ApplicationConfig, provideZoneChangeDetection, ErrorHandler } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, HTTP_INTERCEPTORS } from '@angular/common/http';
import * as Sentry from '@sentry/angular';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { DateConverterInterceptor } from './core/interceptors/date-converter.interceptor';
import { AuthGuard } from './core/guards/auth.guard';
import { GuestGuard } from './core/guards/guest.guard';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: DateConverterInterceptor,
      multi: true
    },
    AuthGuard,
    GuestGuard,
    // Sentry Error Handler
    {
      provide: ErrorHandler,
      useValue: Sentry.createErrorHandler({
        showDialog: false,
      }),
    },
    // Sentry Tracing - using provideTracing from @sentry/angular
    {
      provide: 'SENTRY_TRACING',
      useValue: true
    },
  ]
};
