import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Product, ProductCreateRequest, ProductUpdateRequest, Category, EstoqueCreateRequest } from '../../../../core/models/product.model';
import { CategoryService } from '../../../../core/services/category.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {
  @Input() product: Product | null = null;
  @Input() catalogoId: string = '';
  @Input() isEdit: boolean = false;
  @Output() save = new EventEmitter<ProductCreateRequest | ProductUpdateRequest>();
  @Output() cancel = new EventEmitter<void>();

  productForm: FormGroup;
  categories: Category[] = [];
  loading = false;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService
  ) {
    this.productForm = this.createForm();
  }

  ngOnInit() {
    this.loadCategories();
    if (this.product && this.isEdit) {
      this.populateForm();
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2)]],
      categoriaId: ['', [Validators.required]],
      preco: [0, [Validators.required, Validators.min(0.01)]],
      precoComDesconto: [null, [Validators.min(0.01)]],
      informacoesAdicionais: [''],
      estoque: this.fb.group({
        quantidade: [0, [Validators.required, Validators.min(0)]],
        quantidadeMinima: [0, [Validators.min(0)]],
        quantidadeMaxima: [null, [Validators.min(1)]]
      })
    });
  }

  populateForm() {
    if (this.product) {
      this.productForm.patchValue({
        nome: this.product.nome,
        categoriaId: this.product.categoriaId,
        preco: this.product.preco,
        precoComDesconto: this.product.precoComDesconto,
        informacoesAdicionais: this.product.informacoesAdicionais,
        estoque: {
          quantidade: this.product.estoque?.quantidade || 0,
          quantidadeMinima: this.product.estoque?.quantidadeMinima || 0,
          quantidadeMaxima: this.product.estoque?.quantidadeMaxima || null
        }
      });
    }
  }

  loadCategories() {
    if (this.catalogoId) {
      this.categoryService.getCategoriesByCatalog(this.catalogoId).subscribe({
        next: (categories) => {
          this.categories = categories;
        },
        error: (error) => {
          console.error('Error loading categories:', error);
        }
      });
    }
  }

  onSubmit() {
    if (this.productForm.valid) {
      this.loading = true;
      const formValue = this.productForm.value;

      if (this.isEdit && this.product) {
        // Update product
        const updateRequest: ProductUpdateRequest = {
          nome: formValue.nome,
          categoriaId: formValue.categoriaId,
          preco: formValue.preco,
          precoComDesconto: formValue.precoComDesconto,
          informacoesAdicionais: formValue.informacoesAdicionais
        };
        this.save.emit(updateRequest);
      } else {
        // Create product
        const createRequest: ProductCreateRequest = {
          nome: formValue.nome,
          categoriaId: formValue.categoriaId,
          catalogoId: this.catalogoId,
          preco: formValue.preco,
          precoComDesconto: formValue.precoComDesconto,
          informacoesAdicionais: formValue.informacoesAdicionais,
          estoque: {
            quantidade: formValue.estoque.quantidade,
            quantidadeMinima: formValue.estoque.quantidadeMinima,
            quantidadeMaxima: formValue.estoque.quantidadeMaxima
          }
        };
        this.save.emit(createRequest);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  onCancel() {
    this.cancel.emit();
  }

  markFormGroupTouched() {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach(nestedKey => {
          const nestedControl = control.get(nestedKey);
          nestedControl?.markAsTouched();
        });
      }
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return 'Este campo é obrigatório';
      }
      if (field.errors['minlength']) {
        return `Mínimo de ${field.errors['minlength'].requiredLength} caracteres`;
      }
      if (field.errors['min']) {
        return `Valor mínimo é ${field.errors['min'].min}`;
      }
    }
    return '';
  }

  getNestedFieldError(groupName: string, fieldName: string): string {
    const group = this.productForm.get(groupName);
    const field = group?.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return 'Este campo é obrigatório';
      }
      if (field.errors['min']) {
        return `Valor mínimo é ${field.errors['min'].min}`;
      }
    }
    return '';
  }
}
