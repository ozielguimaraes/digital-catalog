import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category } from '../models/product.model';
import { extractErrorMessage } from '../utils/error.utils';


export interface CategoryRequest {
  nome: string;
  descricao: string;
  catalogoId: string;
}

export interface CategoryUpdateRequest {
  nome: string;
  descricao: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get all categories by catalog
   */
  getCategoriesByCatalog(catalogoId: string): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.API_URL}/categorias/catalogo/${catalogoId}`)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error fetching categories:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar categorias');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Get category by ID
   */
  getCategoryById(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.API_URL}/categorias/${id}`)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error fetching category:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar categoria');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Create new category
   */
  createCategory(category: CategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${this.API_URL}/categorias`, category)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error creating category:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao criar categoria');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Update existing category
   */
  updateCategory(id: string, category: CategoryUpdateRequest): Observable<Category> {
    return this.http.put<Category>(`${this.API_URL}/categorias/${id}`, category)
      .pipe(
        map(response => {
          return response;
        }),
        catchError(error => {
          console.error('Error updating category:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao atualizar categoria');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Delete category
   */
  deleteCategory(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete(`${this.API_URL}/categorias/${id}`, { observe: 'response' })
      .pipe(
        map(response => {
          if (response.status === 204) {
            return {
              success: true,
              message: 'Categoria excluída com sucesso'
            };
          }
          throw new Error('Resposta inesperada do servidor');
        }),
        catchError(error => {
          console.error('Error deleting category:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao excluir categoria');
          return throwError(() => new Error(errorMessage));
        })
      );
  }
}
