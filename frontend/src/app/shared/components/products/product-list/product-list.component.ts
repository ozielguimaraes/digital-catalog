import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../../../core/models/product.model';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { Category } from '../../../../core/models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit, OnChanges {
  @Input() catalogoId: string = '';
  @Output() editProduct = new EventEmitter<Product>();
  @Output() deleteProduct = new EventEmitter<string>();
  @Output() updateStock = new EventEmitter<{productId: string, stock: any}>();

  products: Product[] = [];
  categories: Category[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService
  ) {}

  ngOnInit() {
    if (this.catalogoId) {
      this.loadProducts();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['catalogoId'] && changes['catalogoId'].currentValue) {
      this.loadProducts();
    }
  }

  loadProducts() {
    this.loading = true;
    this.error = null;
    
    // Load both products and categories
    this.productService.getProductsByCatalog(this.catalogoId).subscribe({
      next: (response) => {
        this.products = response.data || [];
        this.loading = false;
        // Load categories after products are loaded
        this.loadCategories();
      },
      error: (error) => {
        this.error = error.message || 'Erro ao carregar produtos';
        this.loading = false;
        this.products = []; // Ensure products is always an array
        console.error('Error loading products:', error);
      }
    });
  }

  loadCategories() {
    this.categoryService.getCategoriesByCatalog(this.catalogoId).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  onEdit(product: Product) {
    this.editProduct.emit(product);
  }

  onDelete(productId: string) {
    if (confirm('Tem certeza que deseja excluir este produto?')) {
      this.deleteProduct.emit(productId);
    }
  }

  onUpdateStock(productId: string, stock: any) {
    this.updateStock.emit({ productId, stock });
  }

  getCategoryName(product: Product): string {
    // First try to use the categoriaNome from the product (comes from backend)
    if (product.categoriaNome) {
      return product.categoriaNome;
    }
    
    // Fallback to finding by ID in local categories
    const category = this.categories.find(c => c.id === product.categoriaId);
    return category ? category.nome : 'Categoria não encontrada';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(price);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR');
  }
}
