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
    return this.http.get<Catalog[]>(`${this.API_URL}/catalogos`)
      .pipe(
        catchError(error => {
          console.error('Error fetching catalogs:', error);
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
    return this.http.delete<{ success: boolean; message: string }>(`${this.API_URL}/catalogos/${id}`)
      .pipe(
        catchError(error => {
          console.error('Error deleting catalog:', error);
          return throwError(() => error);
        })
      );
  }
}
