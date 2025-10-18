import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { 
  Product, 
  ProductCreateRequest, 
  ProductUpdateRequest, 
  ProductResponse, 
  ProductListResponse, 
  Category, 
  CategoryResponse,
  EstoqueUpdateRequest,
  Estoque
} from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get products by catalog ID
   */
  getProductsByCatalog(catalogoId: string): Observable<ProductListResponse> {
    return this.http.get<Product[]>(`${this.API_URL}/produtos/catalogo/${catalogoId}`)
      .pipe(
        map(response => ({
          return response;
        })),
        catchError(error => {
          console.error('Error fetching products by catalog:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get product by ID
   */
  getProductById(id: string): Observable<ProductResponse> {
    return this.http.get<Product>(`${this.API_URL}/produtos/${id}`)
      .pipe(
        map(response => ({
          isSuccess: response.isSuccess,
          data: response.data,
          message: response.message,
          type: response.type,
          errors: response.errors
        })),
        catchError(error => {
          console.error('Error fetching product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Create new product
   */
  createProduct(product: ProductCreateRequest): Observable<ProductResponse> {
    return this.http.post<ApiResponse<Product>>(`${this.API_URL}/produtos`, product)
      .pipe(
        map(response => ({
          isSuccess: response.isSuccess,
          data: response.data,
          message: response.message,
          type: response.type,
          errors: response.errors
        })),
        catchError(error => {
          console.error('Error creating product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Update existing product
   */
  updateProduct(id: string, product: ProductUpdateRequest): Observable<ProductResponse> {
    return this.http.put<ApiResponse<Product>>(`${this.API_URL}/produtos/${id}`, product)
      .pipe(
        map(response => ({
          isSuccess: response.isSuccess,
          data: response.data,
          message: response.message,
          type: response.type,
          errors: response.errors
        })),
        catchError(error => {
          console.error('Error updating product:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Delete product
   */
  deleteProduct(id: string): Observable<{ isSuccess: boolean; message: string }> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/produtos/${id}`)
      .pipe(
        map(response => ({
          isSuccess: response.isSuccess,
          message: response.message
        })),
        catchError(error => {
          console.error('Error deleting product:', error);
          return throwError(() => error);
        })
      );
  }


  /**
   * Update product stock
   */
  updateStock(produtoId: string, estoque: EstoqueUpdateRequest): Observable<{ isSuccess: boolean; data: Estoque; message: string }> {
    return this.http.put<ApiResponse<Estoque>>(`${this.API_URL}/produtos/${produtoId}/estoque`, estoque)
      .pipe(
        map(response => ({
          isSuccess: response.isSuccess,
          data: response.data,
          message: response.message
        })),
        catchError(error => {
          console.error('Error updating stock:', error);
          return throwError(() => error);
        })
      );
  }
}
