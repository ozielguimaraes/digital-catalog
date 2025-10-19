import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Product, ProductUpdateRequest, Category } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { CatalogService, Catalog } from '../../../core/services/catalog.service';
import { CategoryService } from '../../../core/services/category.service';
import { ImageUploadService, ImageUploadResponse } from '../../../core/services/image-upload.service';
import { ImageUploadComponent, ImageUploadData } from '../../../shared/components/image-upload/image-upload.component';
import { PageBreadcrumbComponent } from '../../../shared/components/common/page-breadcrumb/page-breadcrumb.component';
import { CategoryFormComponent } from '../../../shared/components/category-form/category-form.component';

@Component({
  selector: 'app-product-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ImageUploadComponent, PageBreadcrumbComponent, CategoryFormComponent],
  templateUrl: './product-edit.component.html',
  styleUrls: ['./product-edit.component.scss']
})
export class ProductEditComponent implements OnInit {
  catalogs: Catalog[] = [];
  categories: Category[] = [];
  product: Product | null = null;
  loading = false;
  error: string | null = null;
  success: string | null = null;
  showCategoryModal = false;
  
  productForm: FormGroup;
  uploadedImages: ImageUploadData[] = [];
  productId: string = '';

  constructor(
    private productService: ProductService,
    private catalogService: CatalogService,
    private categoryService: CategoryService,
    private imageUploadService: ImageUploadService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.productForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      categoriaId: ['', [Validators.required]],
      preco: [0, [Validators.required, Validators.min(0.01)]],
      precoComDesconto: [0, [Validators.min(0.01)]],
      informacoesAdicionais: ['', [Validators.maxLength(500)]],
      estoque: this.fb.group({
        quantidade: [0, [Validators.required, Validators.min(0)]],
        quantidadeMinima: [0, [Validators.min(0)]],
        quantidadeMaxima: [null, [Validators.min(0)]]
      })
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.productId = params['id'];
      if (this.productId) {
        this.loadProduct();
      }
    });
    
    this.loadCatalogs();
    this.loadCategories();
  }

  loadProduct() {
    this.loading = true;
    this.productService.getProductById(this.productId).subscribe({
      next: (response) => {
        this.product = response.data;
        this.populateForm(response.data);
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Erro ao carregar produto';
        this.loading = false;
        console.error('Error loading product:', error);
      }
    });
  }

  populateForm(product: Product) {
    this.productForm.patchValue({
      nome: product.nome,
      categoriaId: product.categoriaId,
      preco: product.preco,
      precoComDesconto: product.precoComDesconto || 0,
      informacoesAdicionais: product.informacoesAdicionais || '',
      estoque: {
        quantidade: product.estoque?.quantidade || 0,
        quantidadeMinima: product.estoque?.quantidadeMinima || 0,
        quantidadeMaxima: product.estoque?.quantidadeMaxima || null
      }
    });

    // TODO: Handle existing images when the Product interface includes them
    this.uploadedImages = [];
  }

  loadCatalogs() {
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
      },
      error: (error) => {
        this.error = error.message || 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  loadCategories() {
    if (!this.product?.catalogoId) return;
    
    this.categoryService.getCategoriesByCatalog(this.product.catalogoId).subscribe({
      next: (categories: Category[]) => {
        this.categories = categories;
      },
      error: (error: any) => {
        this.error = error.message || 'Erro ao carregar categorias';
        console.error('Error loading categories:', error);
      }
    });
  }

  onImagesUploaded(images: ImageUploadData[]) {
    this.uploadedImages = images;
  }

  onAddNewCategory() {
    if (!this.product?.catalogoId) {
      this.error = 'Produto não carregado corretamente';
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
    if (this.productForm.invalid || !this.product) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    const formValue = this.productForm.value;
    const productData: ProductUpdateRequest = {
      nome: formValue.nome,
      categoriaId: formValue.categoriaId,
      preco: formValue.preco,
      precoComDesconto: formValue.precoComDesconto || null,
      informacoesAdicionais: formValue.informacoesAdicionais || null,
      // Note: estoque and imagens are not part of ProductUpdateRequest
      // They should be handled separately if needed
    };

    // First update the product
    this.productService.updateProduct(this.productId, productData).subscribe({
      next: (response) => {
        if (this.uploadedImages.length > 0) {
          // Upload new images after product update
          this.uploadImages(this.productId);
        } else {
          this.success = 'Produto atualizado com sucesso!';
          this.loading = false;
          
          // Redirect to products list after 2 seconds
          setTimeout(() => {
            this.router.navigate(['/dashboard/products'], { 
              queryParams: { catalogId: this.product?.catalogoId } 
            });
          }, 2000);
        }
      },
      error: (error) => {
        this.error = error.message || 'Erro ao atualizar produto';
        this.loading = false;
        console.error('Error updating product:', error);
      }
    });
  }

  private uploadImages(productId: string) {
    const uploadPromises = this.uploadedImages.map(imageData => {
      if (imageData.file) {
        return this.imageUploadService.uploadImage(productId, imageData.file).toPromise();
      }
      return Promise.resolve(null);
    });

    Promise.all(uploadPromises).then(results => {
      this.success = 'Produto atualizado com sucesso!';
      this.loading = false;
      
      // Redirect to products list after 2 seconds
      setTimeout(() => {
        this.router.navigate(['/dashboard/products'], { 
          queryParams: { catalogId: this.product?.catalogoId } 
        });
      }, 2000);
    }).catch(error => {
      console.error('Error uploading images:', error);
      this.success = 'Produto atualizado com sucesso, mas houve erro no upload das imagens.';
      this.loading = false;
      
      // Redirect to products list after 2 seconds
      setTimeout(() => {
        this.router.navigate(['/dashboard/products'], { 
          queryParams: { catalogId: this.product?.catalogoId } 
        });
      }, 2000);
    });
  }

  onCancel() {
    this.router.navigate(['/dashboard/products'], { 
      queryParams: { catalogId: this.product?.catalogoId } 
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
