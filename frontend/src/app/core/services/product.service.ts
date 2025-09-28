import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Product, ProductRequest, ProductResponse, ProductListResponse, Category, CategoryResponse } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get all products with pagination and filters
   */
  getProducts(page: number = 1, limit: number = 10, search?: string, categoriaId?: string): Observable<ProductListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('limit', limit.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (categoriaId) {
      params = params.set('categoriaId', categoriaId);
    }

    return this.http.get<ProductListResponse>(`${this.API_URL}/produtos`, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching products:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get product by ID
   */
  getProductById(id: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.API_URL}/produtos/${id}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Create new product
   */
  createProduct(product: ProductRequest): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(`${this.API_URL}/produtos`, product)
      .pipe(
        catchError(error => {
          console.error('Error creating product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Update existing product
   */
  updateProduct(id: string, product: ProductRequest): Observable<ProductResponse> {
    return this.http.put<ProductResponse>(`${this.API_URL}/produtos/${id}`, product)
      .pipe(
        catchError(error => {
          console.error('Error updating product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Delete product
   */
  deleteProduct(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.API_URL}/produtos/${id}`)
      .pipe(
        catchError(error => {
          console.error('Error deleting product:', error);
          return throwError(() => error);
        })
      );
  }


  /**
   * Upload product image
   */
  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ url: string }>(`${this.API_URL}/produtos/upload-image`, formData)
      .pipe(
        catchError(error => {
          console.error('Error uploading image:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Toggle product active status
   */
  toggleProductStatus(id: string, ativo: boolean): Observable<ProductResponse> {
    return this.http.patch<ProductResponse>(`${this.API_URL}/produtos/${id}/status`, { ativo })
      .pipe(
        catchError(error => {
          console.error('Error toggling product status:', error);
          return throwError(() => error);
        })
      );
  }
}
