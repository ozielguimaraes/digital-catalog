import { Component, OnInit, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Product, ProductRequest, Category } from '../../../../core/models/product.model';
import { ProductService } from '../../../../core/services/product.service';

export interface ProductFormData {
  product?: Product;
  isEdit: boolean;
}

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  categories: Category[] = [];
  isLoading = false;
  isSubmitting = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<ProductFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProductFormData
  ) {
    this.productForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCategories();
    
    if (this.data.isEdit && this.data.product) {
      this.populateForm(this.data.product);
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      preco: [0, [Validators.required, Validators.min(0.01)]],
      quantidade: [0, [Validators.required, Validators.min(0)]],
      categoriaId: ['', [Validators.required]],
      imagemUrl: [''],
      ativo: [true]
    });
  }

  private populateForm(product: Product): void {
    this.productForm.patchValue({
      nome: product.nome,
      descricao: product.descricao,
      preco: product.preco,
      quantidade: product.quantidade,
      categoriaId: product.categoriaId,
      imagemUrl: product.imagemUrl || '',
      ativo: product.ativo
    });

    if (product.imagemUrl) {
      this.imagePreview = product.imagemUrl;
    }
  }

  private loadCategories(): void {
    this.isLoading = true;
    
    this.productService.getCategories().subscribe({
      next: (response) => {
        this.categories = response.data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.showError('Erro ao carregar categorias');
        this.isLoading = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.showError('Por favor, selecione apenas arquivos de imagem');
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.showError('O arquivo deve ter no máximo 5MB');
        return;
      }

      this.selectedFile = file;
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
    this.productForm.patchValue({ imagemUrl: '' });
  }

  onSubmit(): void {
    if (this.productForm.valid) {
      this.isSubmitting = true;
      
      const formData = this.productForm.value;
      
      // If editing and no new file selected, keep existing image
      if (this.data.isEdit && !this.selectedFile && this.data.product?.imagemUrl) {
        formData.imagemUrl = this.data.product.imagemUrl;
      }

      if (this.selectedFile) {
        // Upload image first
        this.productService.uploadImage(this.selectedFile).subscribe({
          next: (response) => {
            formData.imagemUrl = response.url;
            this.saveProduct(formData);
          },
          error: (error) => {
            console.error('Error uploading image:', error);
            this.showError('Erro ao fazer upload da imagem');
            this.isSubmitting = false;
          }
        });
      } else {
        this.saveProduct(formData);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  private saveProduct(formData: ProductRequest): void {
    const operation = this.data.isEdit 
      ? this.productService.updateProduct(this.data.product!.id, formData)
      : this.productService.createProduct(formData);

    operation.subscribe({
      next: (response) => {
        const message = this.data.isEdit 
          ? 'Produto atualizado com sucesso!' 
          : 'Produto criado com sucesso!';
        
        this.showSuccess(message);
        this.dialogRef.close(response.data);
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Error saving product:', error);
        this.showError('Erro ao salvar produto');
        this.isSubmitting = false;
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  getErrorMessage(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} é obrigatório`;
    }
    
    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength']?.requiredLength;
      return `${this.getFieldLabel(fieldName)} deve ter pelo menos ${minLength} caracteres`;
    }
    
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength']?.requiredLength;
      return `${this.getFieldLabel(fieldName)} deve ter no máximo ${maxLength} caracteres`;
    }
    
    if (field?.hasError('min')) {
      return `${this.getFieldLabel(fieldName)} deve ser maior que zero`;
    }
    
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      nome: 'Nome',
      descricao: 'Descrição',
      preco: 'Preço',
      quantidade: 'Quantidade',
      categoriaId: 'Categoria',
      imagemUrl: 'Imagem'
    };
    return labels[fieldName] || fieldName;
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 5000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
}
