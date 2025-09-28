import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../../core/services/auth.service';
import { LoginRequest } from '../../../../core/models/user.model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.createForm();
  }

  ngOnInit(): void {
    // Subscribe to auth state to handle loading and errors
    this.authService.authState$.subscribe(authState => {
      this.isLoading = authState.isLoading;
      
      if (authState.error) {
        this.showError(authState.error);
      }
      
      if (authState.isAuthenticated) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const credentials: LoginRequest = this.loginForm.value;
      this.authService.login(credentials).subscribe({
        next: (response) => {
          this.showSuccess('Login realizado com sucesso!');
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.showError('Erro ao fazer login. Verifique suas credenciais.');
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
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

  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} é obrigatório`;
    }
    
    if (field?.hasError('email')) {
      return 'Email inválido';
    }
    
    if (field?.hasError('minlength')) {
      return `${this.getFieldLabel(fieldName)} deve ter pelo menos 6 caracteres`;
    }
    
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      email: 'Email',
      password: 'Senha'
    };
    return labels[fieldName] || fieldName;
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }
}
