import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Product, Category } from '../../../../core/models/product.model';
import { ProductService } from '../../../../core/services/product.service';
import { CategoryService } from '../../../../core/services/category.service';
import { CatalogService, Catalog } from '../../../../core/services/catalog.service';
import { ImageUrlService } from '../../../../core/services/image-url.service';
import { ProductFormComponent, ProductFormData } from '../product-form/product-form.component';
import { CategoryFormComponent, CategoryFormData } from '../category-form/category-form.component';
import { extractErrorMessage } from '../../../../core/utils/error.utils';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['imagem', 'nome', 'categoria', 'preco', 'quantidade', 'ativo', 'acoes'];
  dataSource = new MatTableDataSource<Product>();
  
  products: Product[] = [];
  categories: Category[] = [];
  catalogs: Catalog[] = [];
  selectedCatalogId: string = '';
  
  // Pagination
  totalItems = 0;
  pageSize = 10;
  pageIndex = 0;
  pageSizeOptions = [5, 10, 25, 50];
  
  // Filters
  searchTerm = '';
  selectedCategoryId = '';
  
  // Loading states
  isLoading = false;
  isDeleting = false;
  deletingProductId: string | null = null;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private catalogService: CatalogService,
    private imageUrlService: ImageUrlService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCatalogs();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadProducts(): void {
    this.isLoading = true;
    
    this.productService.getProducts(
      this.pageIndex + 1,
      this.pageSize,
      this.searchTerm,
      this.selectedCategoryId
    ).subscribe({
      next: (response) => {
        this.products = response.data;
        this.dataSource.data = this.products;
        this.totalItems = response.pagination?.total || 0;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.showError(error.message || 'Erro ao carregar produtos');
        this.isLoading = false;
      }
    });
  }

  loadCatalogs(): void {
    this.catalogService.getCatalogs().subscribe({
      next: (catalogs) => {
        this.catalogs = catalogs;
        if (catalogs.length > 0) {
          this.selectedCatalogId = catalogs[0].id;
          this.loadCategories();
          this.loadProducts();
        }
      },
      error: (error) => {
        console.error('Error loading catalogs:', error);
        this.showError(error.message || 'Erro ao carregar catálogos');
      }
    });
  }

  loadCategories(): void {
    if (!this.selectedCatalogId) return;
    
    this.categoryService.getCategoriesByCatalog(this.selectedCatalogId).subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.showError(error.message || 'Erro ao carregar categorias');
      }
    });
  }

  onCatalogChange(): void {
    this.selectedCategoryId = '';
    this.loadCategories();
    this.loadProducts();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadProducts();
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadProducts();
  }

  onCategoryFilter(): void {
    this.pageIndex = 0;
    this.loadProducts();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategoryId = '';
    this.pageIndex = 0;
    this.loadProducts();
  }

  editProduct(product: Product): void {
    // Load categories before opening the form
    this.loadCategories();
    
    const dialogRef = this.dialog.open(ProductFormComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        product: product,
        isEdit: true,
        catalogoId: this.catalogoId
      } as ProductFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProducts();
      }
    });
  }

  viewProduct(product: Product): void {
    // TODO: Implement view dialog
    console.log('View product:', product);
  }

  deleteProduct(product: Product): void {
    if (confirm(`Tem certeza que deseja excluir o produto "${product.nome}"?`)) {
      this.isDeleting = true;
      this.deletingProductId = product.id;

      this.productService.deleteProduct(product.id).subscribe({
        next: (response) => {
          this.showSuccess('Produto excluído com sucesso!');
          this.loadProducts();
          this.isDeleting = false;
          this.deletingProductId = null;
        },
        error: (error) => {
          console.error('Error deleting product:', error);
          this.showError(error.message || 'Erro ao excluir produto');
          this.isDeleting = false;
          this.deletingProductId = null;
        }
      });
    }
  }

  toggleProductStatus(product: Product): void {
    this.productService.toggleProductStatus(product.id, !product.ativo).subscribe({
      next: (response) => {
        product.ativo = response.data.ativo;
        this.showSuccess(`Produto ${product.ativo ? 'ativado' : 'desativado'} com sucesso!`);
      },
      error: (error) => {
        console.error('Error toggling product status:', error);
        this.showError(error.message || 'Erro ao alterar status do produto');
      }
    });
  }

  addProduct(): void {
    // Load categories before opening the form
    this.loadCategories();
    
    const dialogRef = this.dialog.open(ProductFormComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: {
        isEdit: false,
        catalogoId: this.catalogoId
      } as ProductFormData
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProducts();
      }
    });
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

  getCategoryName(categoriaId: string): string {
    const category = this.categories.find(cat => cat.id === categoriaId);
    return category ? category.nome : 'Categoria não encontrada';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(price);
  }

  getProductImage(product: Product): string {
    if (product.imagens && product.imagens.length > 0) {
      // Tenta encontrar a imagem principal
      const imagens: any[] = product.imagens;
      let imageUrl = '';
      
      // Verifica se é array de strings (legado) ou objetos
      if (typeof imagens[0] === 'string') {
        imageUrl = imagens[0];
      } else {
        // Array de objetos
        const principal = imagens.find((img: any) => img.isPrincipal);
        const imageObj = principal || imagens[0];
        imageUrl = imageObj.url || '';
      }
      
      return this.imageUrlService.getImageUrl(imageUrl);
    }
    return 'assets/images/placeholder-product.png';
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
