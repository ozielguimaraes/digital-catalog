import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CategoryService, CategoryRequest } from '../../../../core/services/category.service';
import { Catalog } from '../../../../core/services/catalog.service';
import { extractErrorMessage } from '../../../../core/utils/error.utils';

export interface CategoryFormData {
  catalogoId: string;
  catalogoNome: string;
}

@Component({
  selector: 'app-category-form',
  templateUrl: './category-form.component.html',
  styleUrls: ['./category-form.component.scss']
})
export class CategoryFormComponent implements OnInit {
  categoryForm: FormGroup;
  isLoading = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<CategoryFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CategoryFormData
  ) {
    this.categoryForm = this.createForm();
  }

  ngOnInit(): void {
    // Form is already initialized in constructor
  }

  private createForm(): FormGroup {
    return this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      descricao: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]]
    });
  }

  onSubmit(): void {
    if (this.categoryForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.isLoading = true;

      const categoryData: CategoryRequest = {
        nome: this.categoryForm.value.nome,
        descricao: this.categoryForm.value.descricao,
        catalogoId: this.data.catalogoId
      };

      this.categoryService.createCategory(categoryData).subscribe({
        next: (newCategory) => {
          this.showSuccess('Categoria criada com sucesso!');
          this.dialogRef.close(newCategory);
        },
        error: (error) => {
          console.error('Error creating category:', error);
          this.handleError(error);
          this.isSubmitting = false;
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.categoryForm.controls).forEach(key => {
      const control = this.categoryForm.get(key);
      control?.markAsTouched();
    });
  }

  private handleError(error: any): void {
    const errorMessage = extractErrorMessage(error, 'Erro ao criar categoria');
    this.showError(errorMessage);
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.categoryForm.get(fieldName);
    if (field?.hasError('required')) {
      return 'Este campo é obrigatório';
    }
    if (field?.hasError('minlength')) {
      const requiredLength = field.errors?.['minlength']?.requiredLength;
      return `Mínimo de ${requiredLength} caracteres`;
    }
    if (field?.hasError('maxlength')) {
      const requiredLength = field.errors?.['maxlength']?.requiredLength;
      return `Máximo de ${requiredLength} caracteres`;
    }
    return '';
  }
}
