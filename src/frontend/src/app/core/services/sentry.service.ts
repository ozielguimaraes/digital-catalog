import { Injectable } from '@angular/core';
import * as Sentry from '@sentry/angular';

@Injectable({
  providedIn: 'root'
})
export class SentryService {
  
  /**
   * Captura uma exceção no Sentry
   */
  captureException(error: Error | any, context?: any): void {
    Sentry.withScope(scope => {
      if (context) {
        scope.setContext('customContext', context);
      }
      Sentry.captureException(error);
    });
  }

  /**
   * Captura uma mensagem no Sentry
   */
  captureMessage(message: string, level: 'info' | 'warning' | 'error' | 'debug' = 'info', context?: any): void {
    Sentry.withScope(scope => {
      if (context) {
        scope.setContext('customContext', context);
      }
      scope.setLevel(level);
      Sentry.captureMessage(message);
    });
  }

  /**
   * Adiciona breadcrumb para rastreamento
   */
  addBreadcrumb(message: string, category: string = 'user', level: 'info' | 'warning' | 'error' | 'debug' = 'info'): void {
    Sentry.addBreadcrumb({
      message,
      category,
      level,
      timestamp: Date.now() / 1000
    });
  }

  /**
   * Define contexto do usuário
   */
  setUser(user: { id?: string; email?: string; username?: string; [key: string]: any }): void {
    Sentry.setUser(user);
  }

  /**
   * Define tags para filtragem
   */
  setTag(key: string, value: string): void {
    Sentry.setTag(key, value);
  }

  /**
   * Define contexto adicional
   */
  setContext(key: string, context: any): void {
    Sentry.setContext(key, context);
  }

  /**
   * Captura performance transaction
   */
  startTransaction(name: string, op: string = 'navigation') {
    // Use Sentry.startSpan for performance monitoring
    return Sentry.startSpan({ name, op }, (span) => {
      return span;
    });
  }

  /**
   * Captura erro de API com contexto específico
   */
  captureApiError(error: any, endpoint: string, method: string = 'GET', requestData?: any): void {
    Sentry.withScope(scope => {
      scope.setTag('errorType', 'api');
      scope.setTag('endpoint', endpoint);
      scope.setTag('method', method);
      scope.setContext('apiError', {
        endpoint,
        method,
        requestData,
        error: error
      });
      Sentry.captureException(error);
    });
  }

  /**
   * Captura erro de validação de formulário
   */
  captureValidationError(formName: string, errors: any): void {
    Sentry.withScope(scope => {
      scope.setTag('errorType', 'validation');
      scope.setTag('formName', formName);
      scope.setContext('validationError', {
        formName,
        errors
      });
      Sentry.captureMessage(`Validation error in form: ${formName}`, 'warning');
    });
  }
}
