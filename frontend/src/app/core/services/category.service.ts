import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category } from '../models/product.model';

interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  message: string;
  type: string;
  errors?: string[];
}

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
          return throwError(() => error);
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
          return throwError(() => error);
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
          return throwError(() => error);
        })
      );
  }

  /**
   * Update existing category
   */
  updateCategory(id: string, category: CategoryUpdateRequest): Observable<Category> {
    return this.http.put<ApiResponse<Category>>(`${this.API_URL}/categorias/${id}`, category)
      .pipe(
        map(response => {
          if (response.isSuccess && response.data) {
            return response.data;
          }
          throw new Error(response.message || 'Erro ao atualizar categoria');
        }),
        catchError(error => {
          console.error('Error updating category:', error);
          return throwError(() => error);
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
          
          let message = 'Erro ao excluir categoria';
          
          if (error.status === 401) {
            message = 'Não autorizado. Faça login novamente.';
          } else if (error.status === 403) {
            message = 'Acesso negado. Você não tem permissão para excluir esta categoria.';
          } else if (error.status === 404) {
            message = 'Categoria não encontrada.';
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
