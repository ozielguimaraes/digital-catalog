import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoryService, CategoryRequest } from '../../../core/services/category.service';
import { extractErrorMessage } from '../../../core/utils/error.utils';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './category-form.component.html',
  styleUrls: ['./category-form.component.scss']
})
export class CategoryFormComponent implements OnInit {
  @Input() catalogId: string = '';
  @Input() show: boolean = false;
  @Output() categoryCreated = new EventEmitter<any>();
  @Output() close = new EventEmitter<void>();

  categoryForm: FormGroup;
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService
  ) {
    this.categoryForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnInit() {
    if (this.show) {
      this.resetForm();
    }
  }

  ngOnChanges() {
    if (this.show) {
      this.resetForm();
    }
  }

  resetForm() {
    this.categoryForm.reset();
    this.error = null;
    this.success = null;
  }

  onSave() {
    if (this.categoryForm.invalid || !this.catalogId) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    const formValue = this.categoryForm.value;
    const categoryData: CategoryRequest = {
      nome: formValue.nome,
      descricao: formValue.descricao || '',
      catalogoId: this.catalogId
    };

    this.categoryService.createCategory(categoryData).subscribe({
      next: (response) => {
        this.loading = false;
        this.success = 'Categoria criada com sucesso!';
        this.categoryCreated.emit(response);
        
        // Close modal after 1.5 seconds
        setTimeout(() => {
          this.onClose();
        }, 1500);
      },
      error: (error) => {
        this.loading = false;
        this.error = extractErrorMessage(error, 'Erro ao criar categoria');
        console.error('Error creating category:', error);
      }
    });
  }

  onClose() {
    this.resetForm();
    this.close.emit();
  }

  getFieldError(fieldName: string): string | null {
    const field = this.categoryForm.get(fieldName);
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
    }
    return null;
  }

  private markFormGroupTouched() {
    Object.keys(this.categoryForm.controls).forEach(key => {
      const control = this.categoryForm.get(key);
      control?.markAsTouched();
    });
  }
}
