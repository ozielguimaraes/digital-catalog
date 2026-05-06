import { HttpErrorResponse } from '@angular/common/http';
import * as Sentry from '@sentry/angular';

export interface BackendError {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  correlationId?: string;
  message?: string;
  errors?: { [key: string]: string[] };
}

function getSafeStatus(error: Partial<HttpErrorResponse> | null | undefined): number | null {
  return typeof error?.status === 'number' ? error.status : null;
}

function getSafeMessage(error: unknown): string | null {
  if (error instanceof Error && error.message) {
    return error.message;
  }

  if (typeof error === 'object' && error !== null && 'message' in error) {
    const message = (error as { message?: unknown }).message;
    return typeof message === 'string' && message.trim().length > 0 ? message : null;
  }

  return null;
}

/**
 * Extrai a mensagem de erro mais apropriada do backend
 * Prioriza: detail > message > title > mensagem padrão
 */
export function extractErrorMessage(error: HttpErrorResponse, defaultMessage: string = 'Ocorreu um erro inesperado'): string {
  console.error('Full error object:', error);
  const status = getSafeStatus(error);
  
  // Capture error in Sentry with context
  Sentry.withScope(scope => {
    scope.setTag('errorType', 'http');
    if (status !== null) {
      scope.setTag('statusCode', status.toString());
    }
    scope.setContext('httpError', {
      status,
      statusText: error.statusText,
      url: error.url,
      method: error.error?.method || 'unknown'
    });
    scope.setContext('backendError', error.error);
    Sentry.captureException(error);
  });
  
  // Se for um erro de rede ou timeout
  if (status === 0) {
    return 'Erro de conexão. Verifique sua internet e tente novamente.';
  }

  // Extrair mensagem do corpo da resposta PRIMEIRO
  const errorBody = error.error as BackendError;
  
  if (errorBody) {
    // Prioridade: detail > message > title
    if (errorBody.detail) {
      return errorBody.detail;
    }
    
    if (errorBody.message) {
      return errorBody.message;
    }
    
    if (errorBody.title) {
      return errorBody.title;
    }

    // Se houver erros de validação específicos
    if (errorBody.errors) {
      const firstError = Object.values(errorBody.errors)[0];
      if (firstError && firstError.length > 0) {
        return firstError[0];
      }
    }
  }

  // Se não houver mensagem específica do backend, usar mensagens padrão por status
  // Se for erro de servidor (5xx)
  if (status !== null && status >= 500) {
    return 'Erro interno do servidor. Tente novamente mais tarde.';
  }

  // Se for erro de autorização (401) - mas só se não houver mensagem do backend
  if (status === 401) {
    return 'Sessão expirada. Faça login novamente.';
  }

  // Se for erro de permissão
  if (status === 403) {
    return 'Acesso negado. Você não tem permissão para esta ação.';
  }

  // Se for erro de não encontrado
  if (status === 404) {
    return 'Recurso não encontrado.';
  }

  const fallbackMessage = getSafeMessage(error);
  if (fallbackMessage) {
    return fallbackMessage;
  }

  // Fallback para mensagem padrão baseada no status
  return (status !== null && getDefaultMessageByStatus(status)) || defaultMessage;
}

/**
 * Retorna mensagem padrão baseada no status HTTP
 */
function getDefaultMessageByStatus(status: number): string | null {
  const statusMessages: { [key: number]: string } = {
    400: 'Erro de validação. Verifique os dados informados.',
    401: 'Não autorizado. Faça login novamente.',
    403: 'Acesso negado.',
    404: 'Recurso não encontrado.',
    409: 'Conflito. O recurso já existe ou está em uso.',
    422: 'Erro de validação. Verifique os dados informados.',
    429: 'Muitas tentativas. Tente novamente mais tarde.',
    500: 'Erro interno do servidor. Tente novamente mais tarde.',
    502: 'Servidor indisponível. Tente novamente mais tarde.',
    503: 'Serviço temporariamente indisponível.',
    504: 'Timeout. Tente novamente mais tarde.'
  };

  return statusMessages[status] || null;
}

/**
 * Extrai detalhes completos do erro para logging
 */
export function extractErrorDetails(error: HttpErrorResponse): BackendError {
  return {
    type: error.error?.type,
    title: error.error?.title,
    status: error.status,
    detail: error.error?.detail,
    instance: error.error?.instance,
    correlationId: error.error?.correlationId,
    message: error.error?.message,
    errors: error.error?.errors
  };
}
