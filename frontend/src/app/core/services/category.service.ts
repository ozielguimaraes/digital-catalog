import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category } from '../models/product.model';

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
    return this.http.put<Category>(`${this.API_URL}/categorias/${id}`, category)
      .pipe(
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
    return this.http.delete<{ success: boolean; message: string }>(`${this.API_URL}/categorias/${id}`)
      .pipe(
        catchError(error => {
          console.error('Error deleting category:', error);
          return throwError(() => error);
        })
      );
  }
}
