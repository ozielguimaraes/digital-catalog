import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';


export interface Catalog {
  id: string;
  nome: string;
  descricao: string;
  nomeCurto: string;
  numeroWhatsapp: string;
  email: string;
  dataCriacao: string;
  dataAtualizacao?: string;
}

export interface CatalogCreateRequest {
  nome: string;
  descricao: string;
  nomeCurto: string;
  numeroWhatsapp: string;
  email: string;
}

export interface CatalogUpdateRequest {
  id: string;
  nome: string;
  descricao: string;
  nomeCurto: string;
  numeroWhatsapp: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get all catalogs for the current user
   */
  getCatalogs(): Observable<Catalog[]> {
    console.log('Making request to:', `${this.API_URL}/catalogos`);
    return this.http.get<Catalog[]>(`${this.API_URL}/catalogos`)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error fetching catalogs:', error);
          console.error('Error details:', error.error);
          console.error('Error status:', error.status);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get catalog by ID
   */
  getCatalogById(id: string): Observable<Catalog> {
    return this.http.get<Catalog>(`${this.API_URL}/catalogos/${id}`)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error fetching catalog:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Create new catalog
   */
  createCatalog(catalog: CatalogCreateRequest): Observable<Catalog> {
    return this.http.post<Catalog>(`${this.API_URL}/catalogos`, catalog)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error creating catalog:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Update existing catalog
   */
  updateCatalog(id: string, catalog: CatalogUpdateRequest): Observable<Catalog> {
    return this.http.put<Catalog>(`${this.API_URL}/catalogos/${id}`, catalog)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error updating catalog:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Delete catalog
   */
  deleteCatalog(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete(`${this.API_URL}/catalogos/${id}`, { observe: 'response' })
      .pipe(
        map(response => {
          if (response.status === 204) {
            return {
              success: true,
              message: 'Catálogo excluído com sucesso'
            };
          }
          throw new Error('Resposta inesperada do servidor');
        }),
        catchError(error => {
          console.error('Error deleting catalog:', error);
          
          let message = 'Erro ao excluir catálogo';
          
          if (error.status === 401) {
            message = 'Não autorizado. Faça login novamente.';
          } else if (error.status === 403) {
            message = 'Acesso negado. Você não tem permissão para excluir este catálogo.';
          } else if (error.status === 404) {
            message = 'Catálogo não encontrado.';
          } else if (error.status === 400) {
            message = error.error?.message || 'Erro de validação.';
          } else if (error.error?.message) {
            message = error.error.message;
          }
          
          return throwError(() => new Error(message));
        })
      );
  }
}
