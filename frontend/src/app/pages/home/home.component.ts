import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product, Category } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CategoryService } from '../../core/services/category.service';
import { CatalogService, Catalog } from '../../core/services/catalog.service';
import { ImageUrlService } from '../../core/services/image-url.service';
import { CartService } from '../../services/cart.service';
import { Router } from '@angular/router';
import { CartModalComponent } from '../../shared/components/cart-modal/cart-modal.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, CartModalComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  @ViewChild('cartModal') cartModal!: CartModalComponent;
  
  products: Product[] = [];
  categories: Category[] = [];
  catalogs: Catalog[] = [];
  selectedCategory: string = '';
  selectedCatalog: string = '';
  loading = false;
  error: string | null = null;
  searchTerm = '';
  currentYear: number = new Date().getFullYear();
  cartItemCount = 0;
  productQuantities: { [key: string]: number } = {};
  showToast = false;
  toastMessage = '';
  
  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private catalogService: CatalogService,
    private imageUrlService: ImageUrlService,
    private cartService: CartService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loading = true;
    this.loadCatalogs();
    // Não carregamos os produtos aqui, vamos esperar os catálogos primeiro
    
    // Inscrever-se para atualizações do contador do carrinho
    this.cartService.getCartCount().subscribe(count => {
      this.cartItemCount = count;
      this.cdr.detectChanges();
    });
  }

  loadCatalogs() {
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        if (catalogs.length > 0) {
          this.selectedCatalog = catalogs[0].id;
          this.loadCategories();
        }
        // Carrega os produtos após obter os catálogos
        this.loadAllProducts();
      },
      error: (error) => {
        console.error('Error loading catalogs:', error);
        // Mesmo com erro nos catálogos, tenta carregar os produtos
        this.loadAllProducts();
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

    // Verifica se temos catálogos disponíveis
    if (this.catalogs && this.catalogs.length > 0) {
      // Usa o primeiro catálogo da lista para carregar os produtos
      const firstCatalogId = this.catalogs[0].id;
      
      this.productService.getAllProducts(firstCatalogId).subscribe({
        next: (response) => {
          this.products = response.data || [];
          this.loading = false;
        },
        error: (error: any) => {
          this.loading = false;
          this.error = error.message || 'Erro ao carregar produtos';
          console.error('Error loading products:', error);
        }
      });
    } else {
      // Não há catálogos disponíveis
      this.loading = false;
      this.error = 'Nenhum catálogo disponível para exibir produtos';
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
          let allProducts = response.data || [];
          
          // Filter by category if one is selected
          if (this.selectedCategory) {
            allProducts = allProducts.filter(product => product.categoriaId === this.selectedCategory);
          }
          
          this.products = allProducts;
          this.loading = false;
        },
        error: (error: any) => {
          this.loading = false;
          this.error = error.message || 'Erro ao carregar produtos';
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
            let allProducts = response.data || [];
            
            // Filter by category if one is selected
            if (this.selectedCategory) {
              allProducts = allProducts.filter(product => product.categoriaId === this.selectedCategory);
            }
            
            this.products = allProducts.filter(product => 
              product.nome.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
              (product.informacoesAdicionais && product.informacoesAdicionais.toLowerCase().includes(this.searchTerm.toLowerCase()))
            );
            this.loading = false;
          },
          error: (error: any) => {
            this.loading = false;
            this.error = error.message || 'Erro ao buscar produtos';
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
    if (product.imagens && product.imagens.length > 0) {
      // Use the first image (principal image)
      const firstImage: any = product.imagens[0];
      let imageUrl = '';

      // Handle both string (legacy) and object (new) formats
      if (typeof firstImage === 'string') {
        imageUrl = firstImage;
      } else if (firstImage && typeof firstImage === 'object') {
        imageUrl = (firstImage as any).url || '';
      }

      return this.imageUrlService.getImageUrl(imageUrl);
    }
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

  addToCart(product: Product): void {
    const quantity = this.productQuantities[product.id] || 1;
    this.cartService.addToCart(product, quantity);
    
    // Reset quantity to 1 after adding
    this.productQuantities[product.id] = 1;
    
    // Show success feedback
    this.showAddToCartFeedback(product.nome);
  }

  increaseQuantity(product: Product): void {
    const currentQuantity = this.productQuantities[product.id] || 1;
    this.productQuantities[product.id] = Math.min(currentQuantity + 1, 99);
  }

  decreaseQuantity(product: Product): void {
    const currentQuantity = this.productQuantities[product.id] || 1;
    this.productQuantities[product.id] = Math.max(currentQuantity - 1, 1);
  }

  openCart(): void {
    this.cartModal.open();
  }

  private showAddToCartFeedback(productName: string): void {
    this.toastMessage = `${productName} adicionado ao carrinho!`;
    this.showToast = true;
    
    // Hide toast after 3 seconds
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }

  goToLogin() {
    this.router.navigate(['/signin']);
  }

}
