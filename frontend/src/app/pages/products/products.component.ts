import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Product, ProductCreateRequest, ProductUpdateRequest, EstoqueUpdateRequest, Category } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CatalogService, Catalog } from '../../core/services/catalog.service';
import { CategoryService } from '../../core/services/category.service';
import { ProductListComponent } from '../../shared/components/products/product-list/product-list.component';
import { ImageUploadComponent, ImageUploadData } from '../../shared/components/image-upload/image-upload.component';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ProductListComponent, ImageUploadComponent, PageBreadcrumbComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  @ViewChild(ProductListComponent) productListComponent!: ProductListComponent;
  
  catalogs: Catalog[] = [];
  categories: Category[] = [];
  selectedCatalogId: string = '';
  showForm = false;
  editingProduct: Product | null = null;
  isEdit = false;
  loading = false;
  error: string | null = null;
  success: string | null = null;
  
  productForm: FormGroup;
  uploadedImages: ImageUploadData[] = [];

  constructor(
    private productService: ProductService,
    private catalogService: CatalogService,
    private categoryService: CategoryService,
    private fb: FormBuilder
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
    this.loadCatalogs();
  }

  loadCatalogs() {
    this.loading = true;
    this.clearMessages();

    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        this.loading = false;
        if (catalogs.length > 0) {
          this.selectedCatalogId = catalogs[0].id;
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  onCatalogChange() {
    this.clearMessages();
    this.loadCategories();
  }

  loadCategories() {
    if (!this.selectedCatalogId) return;

    this.categoryService.getCategoriesByCatalog(this.selectedCatalogId).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  onAddProduct() {
    if (!this.selectedCatalogId) {
      this.error = 'Selecione um catálogo primeiro';
      return;
    }
    this.editingProduct = null;
    this.isEdit = false;
    this.showForm = true;
    this.productForm.reset();
    this.uploadedImages = [];
    this.clearMessages();
  }

  onEditProduct(product: Product) {
    this.editingProduct = product;
    this.isEdit = true;
    this.showForm = true;
    this.productForm.patchValue({
      nome: product.nome,
      categoriaId: product.categoriaId,
      preco: product.preco,
      precoComDesconto: product.precoComDesconto || 0,
      informacoesAdicionais: product.informacoesAdicionais || '',
      estoque: {
        quantidade: product.estoque.quantidade || 0,
        quantidadeMinima: product.estoque.quantidadeMinima || 0,
        quantidadeMaxima: product.estoque.quantidadeMaxima || null
      }
    });
    this.uploadedImages = []; // TODO: Load existing images
    this.clearMessages();
  }

  onDeleteProduct(productId: string) {
    this.loading = true;
    this.clearMessages();

    this.productService.deleteProduct(productId).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = 'Produto excluído com sucesso!';
        this.reloadProductList();
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Erro ao excluir produto';
        console.error('Error deleting product:', error);
      }
    });
  }

  onUpdateStock(data: {productId: string, stock: any}) {
    this.loading = true;
    this.clearMessages();

    const stockUpdate: EstoqueUpdateRequest = {
      quantidade: data.stock.quantidade || 0,
      quantidadeMinima: data.stock.quantidadeMinima || 0,
      quantidadeMaxima: data.stock.quantidadeMaxima || null
    };

    this.productService.updateStock(data.productId, stockUpdate).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = 'Estoque atualizado com sucesso!';
        this.reloadProductList();
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Erro ao atualizar estoque';
        console.error('Error updating stock:', error);
      }
    });
  }

  onSaveProduct() {
    if (this.productForm.valid) {
      this.loading = true;
      this.clearMessages();

      const formData = this.productForm.value;

      if (this.isEdit && this.editingProduct) {
        // Update product
        const updateRequest: ProductUpdateRequest = {
          nome: formData.nome,
          categoriaId: formData.categoriaId,
          preco: formData.preco,
          precoComDesconto: formData.precoComDesconto || undefined,
          informacoesAdicionais: formData.informacoesAdicionais || undefined
        };

        this.productService.updateProduct(this.editingProduct.id, updateRequest).subscribe({
          next: (response) => {
            this.loading = false;
            this.success = 'Produto atualizado com sucesso!';
            this.showForm = false;
            this.editingProduct = null;
            this.isEdit = false;
            this.reloadProductList();
          },
          error: (error) => {
            this.loading = false;
            this.error = 'Erro ao atualizar produto';
            console.error('Error updating product:', error);
          }
        });
      } else {
        // Create product
        const createRequest: ProductCreateRequest = {
          nome: formData.nome,
          categoriaId: formData.categoriaId,
          catalogoId: this.selectedCatalogId,
          preco: formData.preco,
          precoComDesconto: formData.precoComDesconto || undefined,
          informacoesAdicionais: formData.informacoesAdicionais || undefined,
          estoque: {
            quantidade: formData.estoque.quantidade,
            quantidadeMinima: formData.estoque.quantidadeMinima || undefined,
            quantidadeMaxima: formData.estoque.quantidadeMaxima || undefined
          }
        };
        
        this.productService.createProduct(createRequest).subscribe({
          next: (response) => {
            this.loading = false;
            this.success = 'Produto criado com sucesso!';
            this.showForm = false;
            this.reloadProductList();
          },
          error: (error) => {
            this.loading = false;
            this.error = 'Erro ao criar produto';
            console.error('Error creating product:', error);
          }
        });
      }
    } else {
      this.markFormGroupTouched();
      this.error = 'Por favor, preencha todos os campos obrigatórios corretamente';
    }
  }

  onCancelForm() {
    this.showForm = false;
    this.editingProduct = null;
    this.isEdit = false;
    this.productForm.reset();
    this.uploadedImages = [];
    this.clearMessages();
  }

  onImagesUploaded(images: ImageUploadData[]) {
    this.uploadedImages = images;
  }

  private clearMessages() {
    this.error = null;
    this.success = null;
  }

  private reloadProductList() {
    if (this.productListComponent) {
      this.productListComponent.loadProducts();
    }
  }

  private markFormGroupTouched() {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} é obrigatório`;
      }
      if (field.errors['minlength']) {
        return `${this.getFieldLabel(fieldName)} deve ter pelo menos ${field.errors['minlength'].requiredLength} caracteres`;
      }
      if (field.errors['maxlength']) {
        return `${this.getFieldLabel(fieldName)} deve ter no máximo ${field.errors['maxlength'].requiredLength} caracteres`;
      }
      if (field.errors['min']) {
        return `${this.getFieldLabel(fieldName)} deve ser maior que ${field.errors['min'].min}`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      'nome': 'Nome',
      'categoriaId': 'Categoria',
      'preco': 'Preço',
      'precoComDesconto': 'Preço com Desconto',
      'informacoesAdicionais': 'Informações Adicionais'
    };
    return labels[fieldName] || fieldName;
  }
}
