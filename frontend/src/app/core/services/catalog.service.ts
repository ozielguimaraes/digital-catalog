import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { extractErrorMessage } from '../utils/error.utils';


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
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar catálogos');
          return throwError(() => new Error(errorMessage));
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
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar catálogo');
          return throwError(() => new Error(errorMessage));
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
          const errorMessage = extractErrorMessage(error, 'Erro ao criar catálogo');
          return throwError(() => new Error(errorMessage));
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
          const errorMessage = extractErrorMessage(error, 'Erro ao atualizar catálogo');
          return throwError(() => new Error(errorMessage));
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
          const errorMessage = extractErrorMessage(error, 'Erro ao excluir catálogo');
          return throwError(() => new Error(errorMessage));
        })
      );
  }
}
