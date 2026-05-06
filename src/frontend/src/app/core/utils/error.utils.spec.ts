import { HttpErrorResponse } from '@angular/common/http';
import * as Sentry from '@sentry/angular';
import { extractErrorMessage, extractErrorDetails } from './error.utils';

describe('extractErrorMessage', () => {
  // Sentry é importado como ES module read-only. Não dá pra spyOn direto.
  // Stub via Object.defineProperty pra evitar chamadas reais nos testes.
  beforeAll(() => {
    Object.defineProperty(Sentry, 'withScope', {
      value: (cb: any) => cb({ setTag: () => {}, setContext: () => {} }),
      writable: true,
      configurable: true
    });
    Object.defineProperty(Sentry, 'captureException', {
      value: () => {},
      writable: true,
      configurable: true
    });
  });

  beforeEach(() => {
    spyOn(console, 'error').and.stub();
  });

  function makeError(status: number, body: any = null, statusText = ''): HttpErrorResponse {
    return new HttpErrorResponse({ status, error: body, statusText });
  }

  it('returns connection error for status 0', () => {
    const result = extractErrorMessage(makeError(0));
    expect(result).toContain('Erro de conexão');
  });

  it('prefers detail over message and title', () => {
    const result = extractErrorMessage(makeError(400, {
      detail: 'Detalhe específico',
      message: 'Outra mensagem',
      title: 'Título'
    }));
    expect(result).toBe('Detalhe específico');
  });

  it('falls back to message when detail is missing', () => {
    const result = extractErrorMessage(makeError(400, {
      message: 'Mensagem do backend',
      title: 'Título ignorado'
    }));
    expect(result).toBe('Mensagem do backend');
  });

  it('falls back to title when detail and message are missing', () => {
    const result = extractErrorMessage(makeError(400, { title: 'Apenas título' }));
    expect(result).toBe('Apenas título');
  });

  it('extracts first validation error when errors object is present', () => {
    const result = extractErrorMessage(makeError(422, {
      errors: { Email: ['Email inválido', 'Email obrigatório'] }
    }));
    expect(result).toBe('Email inválido');
  });

  it('returns generic 5xx message when no body', () => {
    expect(extractErrorMessage(makeError(500))).toContain('Erro interno do servidor');
    expect(extractErrorMessage(makeError(503))).toContain('Erro interno do servidor');
  });

  it('returns session-expired message for 401 without body', () => {
    expect(extractErrorMessage(makeError(401))).toContain('Sessão expirada');
  });

  it('returns access-denied message for 403 without body', () => {
    expect(extractErrorMessage(makeError(403))).toContain('Acesso negado');
  });

  it('returns not-found message for 404 without body', () => {
    expect(extractErrorMessage(makeError(404))).toContain('não encontrado');
  });

  it('uses custom default message when no status mapping', () => {
    const result = extractErrorMessage(makeError(418), 'Custom fallback');
    expect(result).toBe('Custom fallback');
  });
});

describe('extractErrorDetails', () => {
  it('returns shaped object from error body', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        type: 'validation',
        title: 'Bad Request',
        detail: 'Campo obrigatório',
        instance: '/api/v1/foo',
        correlationId: 'abc-123',
        errors: { Nome: ['obrigatório'] }
      }
    });

    const details = extractErrorDetails(error);

    expect(details.status).toBe(400);
    expect(details.detail).toBe('Campo obrigatório');
    expect(details.correlationId).toBe('abc-123');
    expect(details.errors).toEqual({ Nome: ['obrigatório'] });
  });
});
