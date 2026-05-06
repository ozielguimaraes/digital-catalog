import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { extractErrorMessage } from '../utils/error.utils';
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
          isSuccess: true,
          data: response,
          message: 'Produtos carregados com sucesso',
          type: 'success'
        })),
        catchError(error => {
          console.error('Error fetching products by catalog:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar produtos');
          return throwError(() => new Error(errorMessage));
        })
      );
  }
  
  /**
   * Get products by catalog ID
   * @param catalogoId ID do catálogo para buscar produtos
   */
  getAllProducts(catalogoId?: string): Observable<ProductListResponse> {
    if (!catalogoId) {
      return throwError(() => new Error('É necessário fornecer um ID de catálogo'));
    }
    
    return this.http.get<Product[]>(`${this.API_URL}/produtos/catalogo/${catalogoId}`)
      .pipe(
        map(response => ({
          isSuccess: true,
          data: response,
          message: 'Produtos carregados com sucesso',
          type: 'success'
        })),
        catchError(error => {
          console.error('Error fetching products:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar produtos');
          return throwError(() => new Error(errorMessage));
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
          isSuccess: true,
          data: response,
          message: 'Produto carregado com sucesso',
          type: 'success'
        })),
        catchError(error => {
          console.error('Error fetching product:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao carregar produto');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Create new product
   */
  createProduct(product: ProductCreateRequest): Observable<ProductResponse> {
    return this.http.post<Product>(`${this.API_URL}/produtos`, product)
      .pipe(
        map(response => ({
          isSuccess: true,
          data: response,
          message: 'Produto criado com sucesso',
          type: 'success'
        })),
        catchError(error => {
          console.error('Error creating product:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao criar produto');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Update existing product
   */
  updateProduct(id: string, product: ProductUpdateRequest): Observable<ProductResponse> {
    return this.http.put<Product>(`${this.API_URL}/produtos/${id}`, product)
      .pipe(
        map(response => ({
          isSuccess: true,
          data: response,
          message: 'Produto atualizado com sucesso',
          type: 'success'
        })),
        catchError(error => {
          console.error('Error updating product:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao atualizar produto');
          return throwError(() => new Error(errorMessage));
        })
      );
  }

  /**
   * Delete product
   */
  deleteProduct(id: string): Observable<{ isSuccess: boolean; message: string }> {
    return this.http.delete(`${this.API_URL}/produtos/${id}`)
      .pipe(
        map(() => ({
          isSuccess: true,
          message: 'Produto excluído com sucesso'
        })),
        catchError(error => {
          console.error('Error deleting product:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao excluir produto');
          return throwError(() => new Error(errorMessage));
        })
      );
  }


  /**
   * Update product stock
   */
  updateStock(produtoId: string, estoque: EstoqueUpdateRequest): Observable<{ isSuccess: boolean; data: Estoque; message: string }> {
    return this.http.put<Estoque>(`${this.API_URL}/produtos/${produtoId}/estoque`, estoque)
      .pipe(
        map(response => ({
          isSuccess: true,
          data: response,
          message: 'Estoque atualizado com sucesso'
        })),
        catchError(error => {
          console.error('Error updating stock:', error);
          const errorMessage = extractErrorMessage(error, 'Erro ao atualizar estoque');
          return throwError(() => new Error(errorMessage));
        })
      );
  }
}
