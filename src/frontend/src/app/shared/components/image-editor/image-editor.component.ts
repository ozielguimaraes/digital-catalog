import { Component, Input, Output, EventEmitter, OnInit, OnChanges, AfterViewInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ImageCropData {
  x: number;
  y: number;
  width: number;
  height: number;
  scale: number;
}

@Component({
  selector: 'app-image-editor',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-editor.component.html',
  styleUrls: ['./image-editor.component.scss']
})
export class ImageEditorComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() imageFile: File | null = null;
  @Input() maxWidth: number = 800;
  @Input() maxHeight: number = 600;
  @Output() imageEdited = new EventEmitter<{ file: File; preview: string }>();
  @Output() cancel = new EventEmitter<void>();

  @ViewChild('canvas', { static: false }) canvasRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('imagePreview', { static: false }) imageRef!: ElementRef<HTMLImageElement>;

  imagePreview: string = '';
  isDragging = false;
  startX = 0;
  startY = 0;
  private loadingTimeout: any;
  private loadedImage: HTMLImageElement | null = null;
  cropData: ImageCropData = {
    x: 0,
    y: 0,
    width: 0,
    height: 0,
    scale: 1
  };
  imageLoaded = false;
  originalImageWidth = 0;
  originalImageHeight = 0;
  canvasWidth = 0;
  canvasHeight = 0;

  ngOnInit() {
    if (this.imageFile) {
      this.loadImage();
    }
    // Prevent body scroll when modal is open
    document.body.style.overflow = 'hidden';
  }

  ngOnChanges() {
    if (this.imageFile) {
      this.loadImage();
    }
  }

  loadImage() {
    if (!this.imageFile) return;

    // Clear any existing timeout and loaded image
    if (this.loadingTimeout) {
      clearTimeout(this.loadingTimeout);
    }
    this.loadedImage = null;
    this.imageLoaded = false;

    // Set a safety timeout
    this.loadingTimeout = setTimeout(() => {
      console.error('Image loading timeout');
      this.imageLoaded = false;
      this.loadedImage = null;
    }, 10000); // 10 seconds timeout

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        this.imagePreview = e.target?.result as string;
        this.imageLoaded = false; // Reset loading state
        
        // Wait for the image to be loaded in the DOM and then initialize canvas
        setTimeout(() => {
          this.initializeCanvas();
        }, 200);
      } catch (error) {
        console.error('Error processing image file:', error);
        this.imageLoaded = false;
        this.loadedImage = null;
        if (this.loadingTimeout) {
          clearTimeout(this.loadingTimeout);
        }
      }
    };
    reader.onerror = () => {
      console.error('Error loading image file');
      this.imageLoaded = false;
      this.loadedImage = null;
      if (this.loadingTimeout) {
        clearTimeout(this.loadingTimeout);
      }
    };
    reader.readAsDataURL(this.imageFile);
  }

  initializeCanvas() {
    if (!this.imageRef?.nativeElement) {
      console.log('Image ref not found, retrying...');
      setTimeout(() => this.initializeCanvas(), 100);
      return;
    }

    const img = this.imageRef.nativeElement;
    
    // Clear any existing event handlers
    img.onload = null;
    img.onerror = null;
    
    // Setup canvas directly
    this.setupCanvas();
  }

  private setupCanvas() {
    try {
      // Use default dimensions and let the canvas handle the image loading
      this.originalImageWidth = this.maxWidth;
      this.originalImageHeight = this.maxHeight;
      
      // Calculate canvas dimensions
      this.canvasWidth = this.maxWidth;
      this.canvasHeight = this.maxHeight;

      // Initialize crop data to 80% of the canvas
      const cropWidth = this.canvasWidth * 0.8;
      const cropHeight = this.canvasHeight * 0.8;
      const cropX = (this.canvasWidth - cropWidth) / 2;
      const cropY = (this.canvasHeight - cropHeight) / 2;

      this.cropData = {
        x: cropX,
        y: cropY,
        width: cropWidth,
        height: cropHeight,
        scale: 1
      };

      this.imageLoaded = true;
      
      // Clear loading timeout
      if (this.loadingTimeout) {
        clearTimeout(this.loadingTimeout);
      }
      
      // Draw canvas after a small delay to ensure DOM is ready
      setTimeout(() => {
        this.drawCanvas();
      }, 100);
    } catch (error) {
      console.error('Error setting up canvas:', error);
      this.imageLoaded = false;
      if (this.loadingTimeout) {
        clearTimeout(this.loadingTimeout);
      }
    }
  }

  drawCanvas() {
    if (!this.canvasRef?.nativeElement) {
      console.log('Canvas ref not found');
      return;
    }

    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d');

    if (!ctx) {
      console.log('Canvas context not found');
      return;
    }

    try {
      // Use the imagePreview directly if available
      if (this.imagePreview) {
        const img = new Image();
        img.crossOrigin = 'anonymous'; // Add CORS support
        
        img.onload = () => {
          try {
            // Store the loaded image for reuse
            this.loadedImage = img;
            
            // Update dimensions based on actual image
            this.originalImageWidth = img.naturalWidth;
            this.originalImageHeight = img.naturalHeight;
            
            // Calculate proper canvas dimensions maintaining aspect ratio
            const aspectRatio = this.originalImageWidth / this.originalImageHeight;
            if (aspectRatio > 1) {
              this.canvasWidth = Math.min(this.maxWidth, this.originalImageWidth);
              this.canvasHeight = this.canvasWidth / aspectRatio;
            } else {
              this.canvasHeight = Math.min(this.maxHeight, this.originalImageHeight);
              this.canvasWidth = this.canvasHeight * aspectRatio;
            }
            
            // Update canvas size
            canvas.width = this.canvasWidth;
            canvas.height = this.canvasHeight;
            
            // Update crop data to 80% of the canvas
            const cropWidth = this.canvasWidth * 0.8;
            const cropHeight = this.canvasHeight * 0.8;
            const cropX = (this.canvasWidth - cropWidth) / 2;
            const cropY = (this.canvasHeight - cropHeight) / 2;

            this.cropData = {
              x: cropX,
              y: cropY,
              width: cropWidth,
              height: cropHeight,
              scale: this.canvasWidth / this.originalImageWidth
            };
            
            // Draw the canvas
            this.redrawCanvas();
          } catch (error) {
            console.error('Error drawing image on canvas:', error);
            this.imageLoaded = false;
          }
        };
        
        img.onerror = () => {
          console.error('Error loading image for canvas');
          this.imageLoaded = false;
        };
        
        // Set src after setting up handlers
        img.src = this.imagePreview;
      }
    } catch (error) {
      console.error('Error drawing canvas:', error);
      this.imageLoaded = false;
    }
  }

  redrawCanvas() {
    if (!this.canvasRef?.nativeElement || !this.loadedImage || !this.imageLoaded) {
      console.log('Cannot redraw canvas: missing requirements');
      return;
    }

    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d');

    if (!ctx) {
      console.error('Canvas context not available');
      return;
    }

    try {
      // Validate image state
      if (this.loadedImage.complete && this.loadedImage.naturalWidth > 0) {
        // Clear and redraw
        ctx.clearRect(0, 0, this.canvasWidth, this.canvasHeight);
        
        // Draw image
        ctx.drawImage(this.loadedImage, 0, 0, this.canvasWidth, this.canvasHeight);
        // Draw crop overlay using the loaded image
        this.drawCropOverlay(ctx, this.loadedImage);
      } else {
        console.warn('Image not fully loaded, skipping redraw');
      }
    } catch (error) {
      console.error('Error redrawing canvas:', error);
      this.imageLoaded = false;
    }
  }

  drawCropOverlay(ctx: CanvasRenderingContext2D, img: HTMLImageElement) {
    const { x, y, width, height } = this.cropData;

    // Semi-transparent overlay
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(0, 0, this.canvasWidth, this.canvasHeight);

    // Clear the crop area
    ctx.clearRect(x, y, width, height);
    
    // Draw the cropped portion of the image using the loaded image
    ctx.drawImage(img, x, y, width, height, x, y, width, height);

    // Draw crop border
    ctx.strokeStyle = '#3b82f6';
    ctx.lineWidth = 2;
    ctx.strokeRect(x, y, width, height);

    // Draw corner handles
    const handleSize = 8;
    ctx.fillStyle = '#3b82f6';
    ctx.fillRect(x - handleSize/2, y - handleSize/2, handleSize, handleSize);
    ctx.fillRect(x + width - handleSize/2, y - handleSize/2, handleSize, handleSize);
    ctx.fillRect(x - handleSize/2, y + height - handleSize/2, handleSize, handleSize);
    ctx.fillRect(x + width - handleSize/2, y + height - handleSize/2, handleSize, handleSize);
  }

  onMouseDown(event: MouseEvent) {
    if (!this.imageLoaded) return;

    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    this.isDragging = true;
    this.startX = x;
    this.startY = y;
  }

  onMouseMove(event: MouseEvent) {
    if (!this.isDragging || !this.imageLoaded) return;

    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    const deltaX = x - this.startX;
    const deltaY = y - this.startY;

    this.cropData.x = Math.max(0, Math.min(this.canvasWidth - this.cropData.width, this.cropData.x + deltaX));
    this.cropData.y = Math.max(0, Math.min(this.canvasHeight - this.cropData.height, this.cropData.y + deltaY));

    this.startX = x;
    this.startY = y;

    this.redrawCanvas();
  }

  onMouseUp() {
    this.isDragging = false;
  }

  onWheel(event: WheelEvent) {
    if (!this.imageLoaded) return;

    // Prevent default scroll behavior and stop propagation
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    // Calculate zoom delta based on scroll speed for smoother control
    const zoomSpeed = 0.02; // Reduced from 0.05 for even smoother zoom
    const delta = event.deltaY > 0 ? -zoomSpeed : zoomSpeed;
    
    // Apply exponential scaling for more natural zoom feel
    const currentScale = this.cropData.scale;
    const scaleFactor = 1 + delta;
    const newScale = Math.max(0.1, Math.min(3, currentScale * scaleFactor));

    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const centerX = event.clientX - rect.left;
    const centerY = event.clientY - rect.top;

    // Calculate new dimensions based on the new scale
    const newWidth = this.originalImageWidth * newScale;
    const newHeight = this.originalImageHeight * newScale;

    // Update crop data
    this.cropData.scale = newScale;
    this.cropData.width = Math.min(newWidth, this.canvasWidth);
    this.cropData.height = Math.min(newHeight, this.canvasHeight);

    // Keep crop centered on mouse position for better UX
    this.cropData.x = Math.max(0, Math.min(this.canvasWidth - this.cropData.width, centerX - this.cropData.width / 2));
    this.cropData.y = Math.max(0, Math.min(this.canvasHeight - this.cropData.height, centerY - this.cropData.height / 2));

    this.redrawCanvas();
  }

  zoomIn() {
    if (!this.imageLoaded) return;
    
    const zoomSpeed = 0.1;
    const newScale = Math.min(3, this.cropData.scale + zoomSpeed);
    this.updateZoom(newScale);
  }

  zoomOut() {
    if (!this.imageLoaded) return;
    
    const zoomSpeed = 0.1;
    const newScale = Math.max(0.1, this.cropData.scale - zoomSpeed);
    this.updateZoom(newScale);
  }

  private updateZoom(newScale: number) {
    const centerX = this.canvasWidth / 2;
    const centerY = this.canvasHeight / 2;

    // Calculate new dimensions based on the new scale
    const newWidth = this.originalImageWidth * newScale;
    const newHeight = this.originalImageHeight * newScale;

    // Update crop data
    this.cropData.scale = newScale;
    this.cropData.width = Math.min(newWidth, this.canvasWidth);
    this.cropData.height = Math.min(newHeight, this.canvasHeight);

    // Keep crop centered
    this.cropData.x = Math.max(0, Math.min(this.canvasWidth - this.cropData.width, centerX - this.cropData.width / 2));
    this.cropData.y = Math.max(0, Math.min(this.canvasHeight - this.cropData.height, centerY - this.cropData.height / 2));

    this.redrawCanvas();
  }

  resetCrop() {
    // Set crop area to 80% of the canvas
    const cropWidth = this.canvasWidth * 0.8;
    const cropHeight = this.canvasHeight * 0.8;
    const cropX = (this.canvasWidth - cropWidth) / 2;
    const cropY = (this.canvasHeight - cropHeight) / 2;
    
    this.cropData = {
      x: cropX,
      y: cropY,
      width: cropWidth,
      height: cropHeight,
      scale: this.canvasWidth / this.originalImageWidth
    };
    this.redrawCanvas();
  }

  applyCrop() {
    if (!this.imageFile || !this.imagePreview) return;

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');

    if (!ctx) return;

    // Create a new image to ensure it's properly loaded
    const img = new Image();
    img.crossOrigin = 'anonymous';
    
    img.onload = () => {
      // Set canvas size to crop dimensions
      canvas.width = this.cropData.width;
      canvas.height = this.cropData.height;

      // Calculate source coordinates in original image
      const sourceX = this.cropData.x / this.cropData.scale;
      const sourceY = this.cropData.y / this.cropData.scale;
      const sourceWidth = this.cropData.width / this.cropData.scale;
      const sourceHeight = this.cropData.height / this.cropData.scale;

      // Draw cropped image
      ctx.drawImage(
        img,
        sourceX, sourceY, sourceWidth, sourceHeight,
        0, 0, this.cropData.width, this.cropData.height
      );

      // Convert to blob and create new file
      canvas.toBlob((blob) => {
        if (blob && this.imageFile) {
          const croppedFile = new File([blob], this.imageFile.name, {
            type: this.imageFile.type,
            lastModified: Date.now()
          });

          const preview = canvas.toDataURL();
          this.imageEdited.emit({ file: croppedFile, preview });
        }
      }, this.imageFile?.type || 'image/jpeg', 0.9);
    };
    
    img.onerror = () => {
      console.error('Error loading image for crop');
    };
    
    img.src = this.imagePreview;
  }

  onCancel() {
    this.cancel.emit();
  }

  ngOnDestroy() {
    this.cleanup();
  }

  private cleanup() {
    if (this.loadingTimeout) {
      clearTimeout(this.loadingTimeout);
      this.loadingTimeout = null;
    }
    this.loadedImage = null;
    this.imageLoaded = false;
    this.imagePreview = '';
    // Restore body scroll when modal is closed
    document.body.style.overflow = '';
  }

  // Force redraw when component becomes visible
  ngAfterViewInit() {
    if (this.imageFile && this.imagePreview) {
      setTimeout(() => {
        this.initializeCanvas();
      }, 300);
    }
  }
}
