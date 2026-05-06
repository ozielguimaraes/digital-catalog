import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { LabelComponent } from '../../form/label/label.component';
import { CheckboxComponent } from '../../form/input/checkbox.component';
import { InputFieldComponent } from '../../form/input/input-field.component';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { AuthState } from '../../../../core/models/user.model';


@Component({
  selector: 'app-signup-form',
  imports: [
    CommonModule,
    LabelComponent,
    CheckboxComponent,
    InputFieldComponent,
    RouterModule,
    FormsModule,
  ],
  templateUrl: './signup-form.component.html',
  styles: ``
})
export class SignupFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  showPassword = false;
  isChecked = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  fname = '';
  lname = '';
  email = '';
  password = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.authService.authState$
      .pipe(takeUntil(this.destroy$))
      .subscribe((authState: AuthState) => {
        this.isLoading = authState.isLoading;
        this.errorMessage = authState.error || '';
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  onSignUp() {
    this.errorMessage = '';
    this.successMessage = '';

    // Validações
    if (!this.fname || !this.lname || !this.email || !this.password) {
      this.errorMessage = 'Por favor, preencha todos os campos';
      return;
    }

    if (!this.isValidEmail(this.email)) {
      this.errorMessage = 'Por favor, insira um email válido';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'A senha deve ter pelo menos 6 caracteres';
      return;
    }

    if (!this.isChecked) {
      this.errorMessage = 'Você deve aceitar os Termos e Condições';
      return;
    }

    // Chamar o serviço de registro
    this.authService.register({
      nome: `${this.fname} ${this.lname}`,
      userName: this.email, // Usar email como userName
      email: this.email,
      password: this.password
    }).subscribe({
      next: (response: any) => {
        console.log('Registration successful:', response);
        this.successMessage = 'Conta criada com sucesso! Redirecionando...';
        // Redirecionar para o dashboard após 2 segundos
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 2000);
      },
      error: (error: any) => {
        console.error('Registration error:', error);
        // A mensagem de erro será exibida automaticamente via authState$
      }
    });
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onFirstNameChange(value: string | number) {
    this.fname = value as string;
  }

  onLastNameChange(value: string | number) {
    this.lname = value as string;
  }

  onEmailChange(value: string | number) {
    this.email = value as string;
  }

  onPasswordChange(value: string | number) {
    this.password = value as string;
  }
}
