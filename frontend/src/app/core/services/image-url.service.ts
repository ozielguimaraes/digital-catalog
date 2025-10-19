import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ImageUrlService {
  private readonly API_URL = environment.apiUrl;

  constructor() {}

  /**
   * Gets the base URL for static files (removes /api from the API URL)
   */
  private getBaseUrl(): string {
    return this.API_URL.replace('/api', '');
  }

  /**
   * Gets the current origin (protocol + host + port) from the browser
   * This ensures the app works regardless of the deployment URL
   */
  private getCurrentOrigin(): string {
    return window.location.origin;
  }

  /**
   * Builds a complete image URL from a relative path
   * @param imagePath - The relative path to the image (e.g., '/uploads/catalogo/...')
   * @param useCurrentOrigin - Whether to use the current browser origin instead of API base URL
   * @returns Complete image URL
   */
  getImageUrl(imagePath: string, useCurrentOrigin: boolean = false): string {
    // If the URL already includes the full path, use it as is
    if (imagePath.startsWith('http') || imagePath.startsWith('//')) {
      return imagePath;
    }

    // If the path already starts with /uploads, use it as is
    if (imagePath.startsWith('/uploads')) {
      const baseUrl = useCurrentOrigin ? this.getCurrentOrigin() : this.getBaseUrl();
      return `${baseUrl}${imagePath}`;
    }

    // If the path doesn't start with /, add it
    const normalizedPath = imagePath.startsWith('/') ? imagePath : `/${imagePath}`;
    const baseUrl = useCurrentOrigin ? this.getCurrentOrigin() : this.getBaseUrl();
    return `${baseUrl}${normalizedPath}`;
  }

  /**
   * Gets image URL for product images
   * @param catalogoId - Catalog ID
   * @param produtoId - Product ID
   * @param fileName - Image file name
   * @param useCurrentOrigin - Whether to use the current browser origin instead of API base URL
   * @returns Complete product image URL
   */
  getProductImageUrl(catalogoId: string, produtoId: string, fileName: string, useCurrentOrigin: boolean = false): string {
    const imagePath = `/uploads/catalogo/${catalogoId}/produtos/${produtoId}/${fileName}`;
    return this.getImageUrl(imagePath, useCurrentOrigin);
  }

  /**
   * Gets image URL for catalog images
   * @param catalogoId - Catalog ID
   * @param fileName - Image file name
   * @param useCurrentOrigin - Whether to use the current browser origin instead of API base URL
   * @returns Complete catalog image URL
   */
  getCatalogImageUrl(catalogoId: string, fileName: string, useCurrentOrigin: boolean = false): string {
    const imagePath = `/uploads/catalogo/${catalogoId}/${fileName}`;
    return this.getImageUrl(imagePath, useCurrentOrigin);
  }

  /**
   * Gets image URL for general uploads
   * @param fileName - Image file name
   * @param useCurrentOrigin - Whether to use the current browser origin instead of API base URL
   * @returns Complete upload image URL
   */
  getUploadImageUrl(fileName: string, useCurrentOrigin: boolean = false): string {
    const imagePath = `/uploads/products/${fileName}`;
    return this.getImageUrl(imagePath, useCurrentOrigin);
  }
}
