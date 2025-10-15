import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product, Category } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CategoryService } from '../../core/services/category.service';
import { CatalogService, Catalog } from '../../core/services/catalog.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  catalogs: Catalog[] = [];
  selectedCategory: string = '';
  selectedCatalog: string = '';
  loading = false;
  error: string | null = null;
  searchTerm = '';
  currentYear: number = new Date().getFullYear();
  
  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private catalogService: CatalogService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCatalogs();
    this.loadAllProducts();
  }

  loadCatalogs() {
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        if (catalogs.length > 0) {
          this.selectedCatalog = catalogs[0].id;
          this.loadCategories();
        }
      },
      error: (error) => {
        console.error('Error loading catalogs:', error);
      }
    });
  }

  loadCategories() {
    if (!this.selectedCatalog) return;

    this.categoryService.getCategoriesByCatalog(this.selectedCatalog).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadAllProducts() {
    this.loading = true;
    this.error = null;

    // Load products from the first catalog if available
    if (this.catalogs.length > 0) {
      this.productService.getProductsByCatalog(this.catalogs[0].id).subscribe({
        next: (response) => {
          this.products = response.data || [];
          this.loading = false;
        },
        error: (error: any) => {
          this.loading = false;
          this.error = 'Erro ao carregar produtos';
          console.error('Error loading products:', error);
        }
      });
    } else {
      this.loading = false;
      this.products = [];
    }
  }

  onCatalogChange() {
    this.selectedCategory = '';
    this.loadCategories();
    this.loadAllProducts();
  }

  onCategoryChange() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading = true;
    this.error = null;

    if (this.selectedCatalog) {
      this.productService.getProductsByCatalog(this.selectedCatalog).subscribe({
        next: (response) => {
          this.products = response.data || [];
          this.loading = false;
        },
        error: (error: any) => {
          this.loading = false;
          this.error = 'Erro ao carregar produtos';
          console.error('Error loading products:', error);
        }
      });
    } else {
      this.loadAllProducts();
    }
  }

  onSearch() {
    if (this.searchTerm.trim()) {
      // For now, just filter the current products by name
      this.loading = true;
      this.error = null;

      if (this.selectedCatalog) {
        this.productService.getProductsByCatalog(this.selectedCatalog).subscribe({
          next: (response) => {
            const allProducts = response.data || [];
            this.products = allProducts.filter(product => 
              product.nome.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
              (product.informacoesAdicionais && product.informacoesAdicionais.toLowerCase().includes(this.searchTerm.toLowerCase()))
            );
            this.loading = false;
          },
          error: (error: any) => {
            this.loading = false;
            this.error = 'Erro ao buscar produtos';
            console.error('Error searching products:', error);
          }
        });
      } else {
        this.loading = false;
        this.products = [];
      }
    } else {
      this.loadProducts();
    }
  }

  onClearSearch() {
    this.searchTerm = '';
    this.loadProducts();
  }

  getProductImage(product: Product): string {
    // Return a placeholder image since Product model doesn't have images yet
    return 'assets/images/placeholder-product.jpg';
  }

  getProductPrice(product: Product): number {
    return product.precoComDesconto && product.precoComDesconto > 0 
      ? product.precoComDesconto 
      : product.preco;
  }

  hasDiscount(product: Product): boolean {
    return !!(product.precoComDesconto && product.precoComDesconto > 0 && product.precoComDesconto < product.preco);
  }

  getDiscountPercentage(product: Product): number {
    if (!this.hasDiscount(product)) return 0;
    return Math.round(((product.preco - product.precoComDesconto!) / product.preco) * 100);
  }

  onProductClick(product: Product) {
    // Navigate to product details or open modal
    console.log('Product clicked:', product);
  }

  onAddToCart(product: Product) {
    // Add to cart logic
    console.log('Add to cart:', product);
  }

  goToLogin() {
    this.router.navigate(['/signin']);
  }

}
