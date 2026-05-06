import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Catalog, CatalogCreateRequest, CatalogUpdateRequest } from '../../core/services/catalog.service';
import { CatalogService } from '../../core/services/catalog.service';
import { PageBreadcrumbComponent } from '../../shared/components/common/page-breadcrumb/page-breadcrumb.component';

@Component({
  selector: 'app-catalogs',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageBreadcrumbComponent],
  templateUrl: './catalogs.component.html',
  styleUrls: ['./catalogs.component.scss']
})
export class CatalogsComponent implements OnInit {
  catalogs: Catalog[] = [];
  showForm = false;
  editingCatalog: Catalog | null = null;
  isEdit = false;
  loading = false;
  error: string | null = null;
  success: string | null = null;
  
  catalogForm: FormGroup;

  constructor(
    private catalogService: CatalogService,
    private fb: FormBuilder
  ) {
    this.catalogForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      nomeCurto: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      numeroWhatsapp: ['', [Validators.required, Validators.pattern(/^\(\d{2}\)\s\d{4,5}-\d{4}$/)]],
      email: ['', [Validators.required, Validators.email]]
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
      },
      error: (error) => {
        this.loading = false;
        this.error = error.message || 'Erro ao carregar catálogos';
        console.error('Error loading catalogs:', error);
      }
    });
  }

  onAddCatalog() {
    this.editingCatalog = null;
    this.isEdit = false;
    this.showForm = true;
    this.catalogForm.reset();
    this.clearMessages();
  }

  onEditCatalog(catalog: Catalog) {
    this.editingCatalog = catalog;
    this.isEdit = true;
    this.showForm = true;
    this.catalogForm.patchValue({
      nome: catalog.nome,
      descricao: catalog.descricao,
      nomeCurto: catalog.nomeCurto,
      numeroWhatsapp: catalog.numeroWhatsapp,
      email: catalog.email
    });
    this.clearMessages();
  }

  onDeleteCatalog(catalogId: string) {
    if (confirm('Tem certeza que deseja excluir este catálogo?')) {
      this.loading = true;
      this.clearMessages();

      this.catalogService.deleteCatalog(catalogId).subscribe({
        next: (response) => {
          this.loading = false;
          this.success = 'Catálogo excluído com sucesso!';
          this.loadCatalogs();
        },
        error: (error) => {
          this.loading = false;
          this.error = error.message || 'Erro ao excluir catálogo';
          console.error('Error deleting catalog:', error);
        }
      });
    }
  }

  onSaveCatalog() {
    if (this.catalogForm.valid) {
      this.loading = true;
      this.clearMessages();

      const formData = this.catalogForm.value;

      if (this.isEdit && this.editingCatalog) {
        // Update catalog
        const updateRequest: CatalogUpdateRequest = {
          id: this.editingCatalog.id,
          ...formData
        };

        this.catalogService.updateCatalog(this.editingCatalog.id, updateRequest).subscribe({
          next: (response) => {
            this.loading = false;
            this.success = 'Catálogo atualizado com sucesso!';
            this.showForm = false;
            this.editingCatalog = null;
            this.isEdit = false;
            this.loadCatalogs();
          },
          error: (error) => {
            this.loading = false;
            this.error = error.message || 'Erro ao atualizar catálogo';
            console.error('Error updating catalog:', error);
          }
        });
      } else {
        // Create catalog
        const createRequest: CatalogCreateRequest = {
          ...formData
        };

        this.catalogService.createCatalog(createRequest).subscribe({
          next: (response) => {
            this.loading = false;
            this.success = 'Catálogo criado com sucesso!';
            this.showForm = false;
            this.loadCatalogs();
          },
          error: (error) => {
            this.loading = false;
            this.error = error.message || 'Erro ao criar catálogo';
            console.error('Error creating catalog:', error);
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
    this.editingCatalog = null;
    this.isEdit = false;
    this.catalogForm.reset();
    this.clearMessages();
  }

  formatPhoneNumber(event: any) {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length >= 11) {
      value = value.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    } else if (value.length >= 7) {
      value = value.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3');
    } else if (value.length >= 3) {
      value = value.replace(/(\d{2})(\d{0,5})/, '($1) $2');
    }
    this.catalogForm.patchValue({ numeroWhatsapp: value });
  }

  private markFormGroupTouched() {
    Object.keys(this.catalogForm.controls).forEach(key => {
      const control = this.catalogForm.get(key);
      control?.markAsTouched();
    });
  }

  private clearMessages() {
    this.error = null;
    this.success = null;
  }

  getFieldError(fieldName: string): string {
    const field = this.catalogForm.get(fieldName);
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
      if (field.errors['email']) {
        return 'Email inválido';
      }
      if (field.errors['pattern']) {
        return 'Formato de telefone inválido. Use: (11) 99999-9999';
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      'nome': 'Nome',
      'descricao': 'Descrição',
      'nomeCurto': 'Nome Curto',
      'numeroWhatsapp': 'WhatsApp',
      'email': 'Email'
    };
    return labels[fieldName] || fieldName;
  }
}
