import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpProgressEvent } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ImageUrlService } from './image-url.service';

export interface ImageUploadResponse {
  success: boolean;
  message: string;
  imageUrl: string;
  fileName: string;
}

export interface ImageUploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

@Injectable({
  providedIn: 'root'
})
export class ImageUploadService {
  private readonly API_URL = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private imageUrlService: ImageUrlService
  ) {}

  uploadImage(productId: string, file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ImageUploadResponse>(
      `${this.API_URL}/produtos/${productId}/upload-image`,
      formData
    );
  }

  uploadImageWithProgress(productId: string, file: File): Observable<ImageUploadProgress | ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ImageUploadResponse>(
      `${this.API_URL}/produtos/${productId}/upload-image`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
      }
    ).pipe(
      map((event: HttpEvent<ImageUploadResponse>) => {
        switch (event.type) {
          case HttpEventType.UploadProgress:
            const progress = event as HttpProgressEvent;
            return {
              loaded: progress.loaded || 0,
              total: progress.total || 0,
              percentage: progress.total ? Math.round((progress.loaded * 100) / progress.total) : 0
            };
          case HttpEventType.Response:
            return event.body!;
          default:
            return { loaded: 0, total: 0, percentage: 0 };
        }
      })
    );
  }

  getImageUrl(fileName: string): string {
    return this.imageUrlService.getUploadImageUrl(fileName);
  }

  getImageUrlByPath(catalogoId: string, produtoId: string, fileName: string): string {
    return this.imageUrlService.getProductImageUrl(catalogoId, produtoId, fileName);
  }

  validateFile(file: File): { valid: boolean; error?: string } {
    // Validar tipo de arquivo
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/svg+xml', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
      return {
        valid: false,
        error: 'Tipo de arquivo não permitido. Use JPG, PNG, WebP, SVG ou GIF.'
      };
    }

    // Validar tamanho (10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      return {
        valid: false,
        error: 'Arquivo muito grande. Tamanho máximo: 10MB.'
      };
    }

    return { valid: true };
  }
}
