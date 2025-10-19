import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductCreateRequest, Category } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { CatalogService, Catalog } from '../../../core/services/catalog.service';
import { CategoryService } from '../../../core/services/category.service';
import { ImageUploadComponent, ImageUploadData } from '../../../shared/components/image-upload/image-upload.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { CategoryFormComponent } from '../../../shared/components/category-form/category-form.component';

@Component({
  selector: 'app-product-create',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ImageUploadComponent, PageBreadcrumbComponent, CategoryFormComponent],
  templateUrl: './product-create.component.html',
  styleUrls: ['./product-create.component.scss']
})
export class ProductCreateComponent implements OnInit {
  catalogs: Catalog[] = [];
  categories: Category[] = [];
  selectedCatalogId: string = '';
  loading = false;
  error: string | null = null;
  success: string | null = null;
  showCategoryModal = false;
  
  productForm: FormGroup;
  uploadedImages: ImageUploadData[] = [];

  constructor(
    private productService: ProductService,
    private catalogService: CatalogService,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.productForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      categoriaId: ['', [Validators.required]],
      preco: [0, [Validators.required, Validators.min(0.01)]],
      precoComDesconto: [null, [Validators.min(0.01)]],
      informacoesAdicionais: ['', [Validators.maxLength(500)]],
      estoque: this.fb.group({
        quantidade: [0, [Validators.required, Validators.min(0)]],
        quantidadeMinima: [0, [Validators.min(0)]],
        quantidadeMaxima: [null, [Validators.min(0)]]
      })
    });
  }

  ngOnInit() {
    // Get catalog ID from query params first
    this.route.queryParams.subscribe(params => {
      if (params['catalogId']) {
        this.selectedCatalogId = params['catalogId'];
        this.productForm.patchValue({ catalogoId: this.selectedCatalogId });
      }
    });
    
    // Load catalogs and categories
    this.loadCatalogs();
  }

  loadCatalogs() {
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        
        // If no catalogId from query params, select first catalog
        if (!this.selectedCatalogId && catalogs.length > 0) {
          this.selectedCatalogId = catalogs[0].id;
        }
        
        // Load categories for the selected catalog
        if (this.selectedCatalogId) {
          this.loadCategories();
        }
      },
      error: (error) => {
        this.error = error.message || 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  loadCategories() {
    if (!this.selectedCatalogId) {
      return;
    }
    
    this.categoryService.getCategoriesByCatalog(this.selectedCatalogId).subscribe({
      next: (categories: Category[]) => {
        this.categories = categories;
      },
      error: (error: any) => {
        this.error = error.message || 'Erro ao carregar categorias';
        console.error('Error loading categories:', error);
      }
    });
  }

  onCatalogChange() {
    // Reset form when catalog changes
    this.productForm.patchValue({
      categoriaId: '',
      nome: '',
      preco: 0,
      precoComDesconto: null,
      informacoesAdicionais: '',
      estoque: {
        quantidade: 0,
        quantidadeMinima: 0,
        quantidadeMaxima: null
      }
    });
    this.uploadedImages = [];
    this.loadCategories();
  }

  onImagesUploaded(images: ImageUploadData[]) {
    this.uploadedImages = images;
  }

  onAddNewCategory() {
    if (!this.selectedCatalogId) {
      this.error = 'Selecione um catálogo primeiro';
      return;
    }
    this.showCategoryModal = true;
  }

  onCategoryCreated(category: Category) {
    // Add the new category to the list
    this.categories.push(category);
    
    // Select the new category in the form
    this.productForm.patchValue({
      categoriaId: category.id
    });
    
    this.showCategoryModal = false;
  }

  onCloseCategoryModal() {
    this.showCategoryModal = false;
  }

  onSaveProduct() {
    if (this.productForm.invalid || !this.selectedCatalogId) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    const formValue = this.productForm.value;
    const productData: ProductCreateRequest = {
      nome: formValue.nome,
      categoriaId: formValue.categoriaId,
      catalogoId: this.selectedCatalogId,
      preco: formValue.preco,
      precoComDesconto: formValue.precoComDesconto || null,
      informacoesAdicionais: formValue.informacoesAdicionais || null,
      estoque: {
        quantidade: formValue.estoque.quantidade,
        quantidadeMinima: formValue.estoque.quantidadeMinima || 0,
        quantidadeMaxima: formValue.estoque.quantidadeMaxima || null
      }
    };

    this.productService.createProduct(productData).subscribe({
      next: (response) => {
        this.success = 'Produto criado com sucesso!';
        this.loading = false;
        
        // Redirect to products list after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/dashboard/products'], { 
            queryParams: { catalogId: this.selectedCatalogId } 
          });
        }, 2000);
      },
      error: (error) => {
        this.error = error.message || 'Erro ao criar produto';
        this.loading = false;
        console.error('Error creating product:', error);
      }
    });
  }

  onCancel() {
    this.router.navigate(['/dashboard/products'], { 
      queryParams: { catalogId: this.selectedCatalogId } 
    });
  }

  getFieldError(fieldName: string): string | null {
    const field = this.productForm.get(fieldName);
    if (field && field.invalid && field.touched) {
      if (field.errors?.['required']) {
        return 'Este campo é obrigatório';
      }
      if (field.errors?.['minlength']) {
        return `Mínimo de ${field.errors['minlength'].requiredLength} caracteres`;
      }
      if (field.errors?.['maxlength']) {
        return `Máximo de ${field.errors['maxlength'].requiredLength} caracteres`;
      }
      if (field.errors?.['min']) {
        return `Valor mínimo: ${field.errors['min'].min}`;
      }
    }
    return null;
  }

  getNestedFieldError(groupName: string, fieldName: string): string | null {
    const group = this.productForm.get(groupName);
    const field = group?.get(fieldName);
    if (field && field.invalid && field.touched) {
      if (field.errors?.['required']) {
        return 'Este campo é obrigatório';
      }
      if (field.errors?.['min']) {
        return `Valor mínimo: ${field.errors['min'].min}`;
      }
    }
    return null;
  }

  private markFormGroupTouched() {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach(nestedKey => {
          control.get(nestedKey)?.markAsTouched();
        });
      }
    });
  }
}
