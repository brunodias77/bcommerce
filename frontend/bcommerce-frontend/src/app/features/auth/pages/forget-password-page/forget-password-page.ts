import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../services/auth/auth-service';
import { Button } from '../../../../shared/components/button/button';
import { ForgetPasswordRequest } from '../../../../models/requests';

@Component({
  selector: 'app-forget-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, Button],
  templateUrl: './forget-password-page.html',
  styleUrl: './forget-password-page.css'
})
export class ForgetPasswordPage {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  forgetPasswordForm: FormGroup;

  // Computed signals do AuthService
  isLoading = this.authService.isSendingForgetPassword;
  hasError = this.authService.hasForgetPasswordErrors;
  errorMessage = this.authService.forgetPasswordError;
  successMessage = this.authService.forgetPasswordMessage;
  isSuccess = this.authService.forgetPasswordSuccess;

  constructor() {
    this.forgetPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]]
    });
  }

  /**
   * Submete o formulário de esqueci minha senha
   */
  onSubmit(): void {
    if (this.forgetPasswordForm.valid && !this.isLoading()) {
      const request: ForgetPasswordRequest = {
        email: this.forgetPasswordForm.get('email')?.value
      };

      this.authService.forgetPassword(request).subscribe({
        next: () => {
          // Sucesso é tratado pelo AuthService via signals
        },
        error: (error) => {
          console.error('Erro ao enviar solicitação de redefinição de senha:', error);
        }
      });
    } else {
      // Marcar todos os campos como touched para mostrar erros de validação
      this.forgetPasswordForm.markAllAsTouched();
    }
  }

  /**
   * Verifica se um campo específico tem erro
   */
  hasFieldError(fieldName: string): boolean {
    const field = this.forgetPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Obtém a mensagem de erro para um campo específico
   */
  getFieldErrorMessage(fieldName: string): string {
    const field = this.forgetPasswordForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return 'Este campo é obrigatório.';
      }
      if (field.errors['email']) {
        return 'Digite um email válido.';
      }
      if (field.errors['maxlength']) {
        return 'Email deve ter no máximo 255 caracteres.';
      }
    }
    return '';
  }

  /**
   * Navega de volta para a página de login
   */
  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  /**
   * Limpa os erros quando o usuário começa a digitar
   */
  clearErrors(): void {
    if (this.hasError()) {
      this.authService.clearForgetPasswordError();
    }
  }
}
