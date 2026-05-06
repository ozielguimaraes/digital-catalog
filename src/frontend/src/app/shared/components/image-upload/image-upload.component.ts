import { Component, Input, Output, EventEmitter, HostListener, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageEditorComponent } from '../image-editor/image-editor.component';

export interface ImageUploadData {
  file: File | null;
  preview: string;
  isEdited: boolean;
}

@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [CommonModule, ImageEditorComponent],
  templateUrl: './image-upload.component.html',
  styleUrls: ['./image-upload.component.scss']
})
export class ImageUploadComponent implements OnInit {
  @Input() maxFiles: number = 10;
  @Input() maxFileSize: number = 10 * 1024 * 1024; // 10MB
  @Input() acceptedTypes: string[] = ['image/png', 'image/jpeg', 'image/webp', 'image/svg+xml'];
  @Input() existingImages: string[] = [];
  @Output() imagesUploaded = new EventEmitter<ImageUploadData[]>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  isDragActive = false;
  uploadedImages: ImageUploadData[] = [];
  showImageEditor = false;
  editingImage: File | null = null;
  editingIndex = -1;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isProcessing = false;

  ngOnInit() {
    // Process existing images when component initializes
    if (this.existingImages && this.existingImages.length > 0) {
      this.uploadedImages = this.existingImages.map(url => ({
        file: null as any, // Existing images don't have files
        preview: url,
        isEdited: false
      }));
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length) {
      this.handleFiles(Array.from(input.files));
      // Clear the input to prevent duplicate processing
      input.value = '';
    }
  }

  @HostListener('dragover', ['$event'])
  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragActive = true;
  }

  @HostListener('dragleave', ['$event'])
  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragActive = false;
  }

  @HostListener('drop', ['$event'])
  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragActive = false;
    if (event.dataTransfer && event.dataTransfer.files.length && !this.isProcessing) {
      const files = Array.from(event.dataTransfer.files).filter(file =>
        this.acceptedTypes.includes(file.type)
      );
      this.handleFiles(files);
    }
  }

  handleFiles(files: File[]) {
    // Prevent multiple simultaneous processing
    if (this.isProcessing) {
      return;
    }
    
    this.clearMessages();
    this.isProcessing = true;
    
    const validFiles: File[] = [];
    const invalidFiles: string[] = [];
    const tooLargeFiles: string[] = [];
    const duplicateFiles: string[] = [];

    files.forEach(file => {
      // Check for duplicates (only check against images that have files)
      const isDuplicate = this.uploadedImages.some(img => img.file && img.file.name === file.name && img.file.size === file.size);
      if (isDuplicate) {
        duplicateFiles.push(file.name);
        return;
      }

      if (!this.acceptedTypes.includes(file.type)) {
        invalidFiles.push(file.name);
        return;
      }
      if (file.size > this.maxFileSize) {
        tooLargeFiles.push(file.name);
        return;
      }
      if (this.uploadedImages.length + validFiles.length >= this.maxFiles) {
        this.errorMessage = `Máximo de ${this.maxFiles} arquivos permitidos`;
        return;
      }
      validFiles.push(file);
    });

    // Show error messages
    if (duplicateFiles.length > 0) {
      this.errorMessage = `Arquivos duplicados ignorados: ${duplicateFiles.join(', ')}`;
    }
    if (invalidFiles.length > 0) {
      this.errorMessage = `Arquivos não suportados: ${invalidFiles.join(', ')}. Tipos aceitos: PNG, JPG, WebP, SVG, GIF`;
    }
    if (tooLargeFiles.length > 0) {
      const sizeMB = Math.round(this.maxFileSize / (1024 * 1024));
      this.errorMessage = `Arquivos muito grandes: ${tooLargeFiles.join(', ')}. Tamanho máximo: ${sizeMB}MB`;
    }

    if (validFiles.length > 0) {
      this.successMessage = `${validFiles.length} imagem(ns) carregada(s) com sucesso!`;
      
      let processedCount = 0;
      validFiles.forEach(file => {
        const reader = new FileReader();
        reader.onload = (e) => {
          const preview = e.target?.result as string;
          this.uploadedImages.push({
            file,
            preview,
            isEdited: false
          });
          processedCount++;
          if (processedCount === validFiles.length) {
            this.isProcessing = false;
          }
          this.emitImages();
        };
        reader.onerror = () => {
          this.errorMessage = `Erro ao carregar ${file.name}`;
          this.isProcessing = false;
        };
        reader.readAsDataURL(file);
      });
    } else {
      this.isProcessing = false;
    }

    // Clear success message after 3 seconds
    if (this.successMessage) {
      setTimeout(() => {
        this.successMessage = null;
      }, 3000);
    }
  }

  removeImage(index: number) {
    this.uploadedImages.splice(index, 1);
    this.emitImages();
  }

  editImage(index: number) {
    const imageData = this.uploadedImages[index];
    if (!imageData.file) {
      // Cannot edit existing images that don't have files
      this.errorMessage = 'Não é possível editar imagens existentes. Remova e adicione novamente para editar.';
      return;
    }
    this.editingIndex = index;
    this.editingImage = imageData.file;
    this.showImageEditor = true;
  }

  onImageEdited(data: { file: File; preview: string }) {
    if (this.editingIndex >= 0) {
      this.uploadedImages[this.editingIndex] = {
        file: data.file,
        preview: data.preview,
        isEdited: true
      };
      this.emitImages();
    }
    this.showImageEditor = false;
    this.editingImage = null;
    this.editingIndex = -1;
  }

  onEditCancel() {
    this.showImageEditor = false;
    this.editingImage = null;
    this.editingIndex = -1;
  }

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  private emitImages() {
    this.imagesUploaded.emit([...this.uploadedImages]);
  }

  get canAddMore(): boolean {
    return this.uploadedImages.length < this.maxFiles;
  }

  get acceptedTypesString(): string {
    return this.acceptedTypes.join(',');
  }

  private clearMessages() {
    this.errorMessage = null;
    this.successMessage = null;
  }
}
