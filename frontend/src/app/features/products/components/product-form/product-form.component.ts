import { Component, OnInit, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Product, ProductRequest, Category } from '../../../../core/models/product.model';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { CatalogService, Catalog } from '../../../../core/services/catalog.service';
import { CategoryFormComponent, CategoryFormData } from '../category-form/category-form.component';

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
  catalogs: Catalog[] = [];
  selectedCatalogId: string = '';
  isLoading = false;
  isSubmitting = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private catalogService: CatalogService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<ProductFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProductFormData
  ) {
    this.productForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadCatalogs();
    
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

  private loadCatalogs(): void {
    this.isLoading = true;
    
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        if (catalogs.length > 0) {
          this.selectedCatalogId = catalogs[0].id;
          this.loadCategories();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading catalogs:', error);
        this.showError('Erro ao carregar catálogos');
        this.isLoading = false;
      }
    });
  }

  private loadCategories(): void {
    if (!this.selectedCatalogId) return;
    
    this.categoryService.getCategoriesByCatalog(this.selectedCatalogId).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.showError('Erro ao carregar categorias');
      }
    });
  }

  onCatalogChange(): void {
    this.loadCategories();
  }

  createNewCategory(): void {
    if (!this.selectedCatalogId) {
      this.showError('Selecione um catálogo primeiro');
      return;
    }

    const selectedCatalog = this.catalogs.find(c => c.id === this.selectedCatalogId);
    if (!selectedCatalog) {
      this.showError('Catálogo não encontrado');
      return;
    }

    const dialogRef = this.dialog.open(CategoryFormComponent, {
      width: '500px',
      maxWidth: '90vw',
      data: {
        catalogoId: this.selectedCatalogId,
        catalogoNome: selectedCatalog.nome
      } as CategoryFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCategories();
        this.showSuccess('Categoria criada com sucesso!');
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
