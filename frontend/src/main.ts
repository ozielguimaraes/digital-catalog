import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { register as registerSwiperElements } from 'swiper/element/bundle';
import * as Sentry from '@sentry/angular';
import { environment } from './environments/environment';

// Save original method
// const originalAddEventListener = EventTarget.prototype.addEventListener;

// // Override
// EventTarget.prototype.addEventListener = function (
//   type: string,
//   listener: EventListenerOrEventListenerObject,
//   options?: boolean | AddEventListenerOptions
// ) {
//   // Force passive: false for specific events
//   const needsPassiveFalse = ['touchstart', 'touchmove', 'wheel'].includes(type);

//   if (needsPassiveFalse) {
//     if (typeof options === 'boolean' || options === undefined) {
//       options = { passive: false };
//     } else if (typeof options === 'object') {
//       options.passive = false;
//     }
//   }

//   return originalAddEventListener.call(this, type, listener, options);
// };

// Initialize Sentry
Sentry.init({
  dsn: environment.sentry.dsn,
  environment: environment.sentry.environment,
  tracesSampleRate: environment.sentry.tracesSampleRate,
  profilesSampleRate: environment.sentry.profilesSampleRate,
  attachStacktrace: true,
  maxBreadcrumbs: 50,
  beforeSend(event) {
    // Filter out development errors if needed
    if (event.exception) {
      const error = event.exception.values?.[0];
      if (error?.type === 'ChunkLoadError') {
        return null; // Ignore chunk load errors
      }
    }
    return event;
  },
});

registerSwiperElements();

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => {
    console.error(err);
    Sentry.captureException(err);
  });
