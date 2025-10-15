import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Product, ProductCreateRequest, ProductUpdateRequest, EstoqueUpdateRequest } from '../../core/models/product.model';
import { ProductService } from '../../core/services/product.service';
import { CatalogService, Catalog } from '../../core/services/catalog.service';
import { ProductListComponent } from '../../shared/components/products/product-list/product-list.component';
import { ProductFormComponent } from '../../shared/components/products/product-form/product-form.component';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductListComponent, ProductFormComponent, PageBreadcrumbComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  catalogs: Catalog[] = [];
  selectedCatalogId: string = '';
  showForm = false;
  editingProduct: Product | null = null;
  isEdit = false;
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private productService: ProductService,
    private catalogService: CatalogService
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
        this.error = 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  onCatalogChange() {
    this.clearMessages();
    // A lista de produtos será recarregada automaticamente pelo ProductListComponent
  }

  onAddProduct() {
    if (!this.selectedCatalogId) {
      this.error = 'Selecione um catálogo primeiro';
      return;
    }
    this.editingProduct = null;
    this.isEdit = false;
    this.showForm = true;
    this.clearMessages();
  }

  onEditProduct(product: Product) {
    this.editingProduct = product;
    this.isEdit = true;
    this.showForm = true;
    this.clearMessages();
  }

  onDeleteProduct(productId: string) {
    this.loading = true;
    this.clearMessages();

    this.productService.deleteProduct(productId).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = 'Produto excluído com sucesso!';
        // Em produção, você recarregaria a lista aqui
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
        // Em produção, você recarregaria a lista aqui
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Erro ao atualizar estoque';
        console.error('Error updating stock:', error);
      }
    });
  }

  onSaveProduct(productData: ProductCreateRequest | ProductUpdateRequest) {
    this.loading = true;
    this.clearMessages();

    if (this.isEdit && this.editingProduct) {
      // Update product
      this.productService.updateProduct(this.editingProduct.id, productData as ProductUpdateRequest).subscribe({
        next: (response) => {
          this.loading = false;
          this.success = 'Produto atualizado com sucesso!';
          this.showForm = false;
          this.editingProduct = null;
          this.isEdit = false;
        },
        error: (error) => {
          this.loading = false;
          this.error = 'Erro ao atualizar produto';
          console.error('Error updating product:', error);
        }
      });
    } else {
      // Create product - add catalogoId to the request
      const createRequest = {
        ...productData as ProductCreateRequest,
        catalogoId: this.selectedCatalogId
      };
      
      this.productService.createProduct(createRequest).subscribe({
        next: (response) => {
          this.loading = false;
          this.success = 'Produto criado com sucesso!';
          this.showForm = false;
        },
        error: (error) => {
          this.loading = false;
          this.error = 'Erro ao criar produto';
          console.error('Error creating product:', error);
        }
      });
    }
  }

  onCancelForm() {
    this.showForm = false;
    this.editingProduct = null;
    this.isEdit = false;
    this.clearMessages();
  }

  private clearMessages() {
    this.error = null;
    this.success = null;
  }
}
