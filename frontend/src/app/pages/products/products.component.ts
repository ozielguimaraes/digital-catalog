import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Product, EstoqueUpdateRequest } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CatalogService, Catalog } from '../../core/services/catalog.service';
import { ProductListComponent } from '../../shared/components/products/product-list/product-list.component';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductListComponent, PageBreadcrumbComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  @ViewChild(ProductListComponent) productListComponent!: ProductListComponent;
  
  catalogs: Catalog[] = [];
  selectedCatalogId: string = '';
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private productService: ProductService,
    private catalogService: CatalogService,
    private router: Router
  ) {}

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
        this.error = error.message || 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  onCatalogChange() {
    this.clearMessages();
  }

  onAddProduct() {
    if (!this.selectedCatalogId) {
      this.error = 'Selecione um catálogo primeiro';
      return;
    }
    
    // Navigate to product create page
    this.router.navigate(['/dashboard/products/create'], { 
      queryParams: { catalogId: this.selectedCatalogId } 
    });
  }

  onEditProduct(product: Product) {
    // Navigate to product edit page
    this.router.navigate(['/dashboard/products/edit', product.id]);
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
        this.error = error.message || 'Erro ao excluir produto';
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
        this.error = error.message || 'Erro ao atualizar estoque';
        console.error('Error updating stock:', error);
      }
    });
  }

  private reloadProductList() {
    if (this.productListComponent) {
      this.productListComponent.loadProducts();
    }
  }

  private clearMessages() {
    this.error = null;
    this.success = null;
  }
}