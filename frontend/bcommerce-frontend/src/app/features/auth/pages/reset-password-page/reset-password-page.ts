import { Component, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../services/auth/auth-service';
import { Button } from '../../../../shared/components/button/button';
import { ResetPasswordRequest } from '../../../../models/requests';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Button],
  templateUrl: './reset-password-page.html',
  styleUrl: './reset-password-page.scss'
})
export class ResetPasswordPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  resetPasswordForm: FormGroup;
  token: string | null = null;
  showPassword = false;
  showConfirmPassword = false;

  constructor() {
    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    // Effect para redirecionar após sucesso
    effect(() => {
      if (this.authService.resetPasswordSuccess()) {
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    });
  }

  ngOnInit(): void {
    // Capturar o token da URL
    this.route.queryParams.subscribe(params => {
      this.token = params['token'];
      if (!this.token) {
        // Se não há token, redirecionar para a página de esqueci minha senha
        this.router.navigate(['/forget-password']);
      }
    });
  }

  /**
   * Validador customizado para verificar se as senhas coincidem
   */
  private passwordMatchValidator(group: FormGroup) {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  /**
   * Alterna a visibilidade da senha
   */
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  /**
   * Alterna a visibilidade da confirmação de senha
   */
  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  /**
   * Limpa os erros quando o usuário começa a digitar
   */
  clearErrors(): void {
    this.authService.clearResetPasswordError();
  }

  /**
   * Submete o formulário de redefinição de senha
   */
  onSubmit(): void {
    if (this.resetPasswordForm.valid && this.token) {
      const request: ResetPasswordRequest = {
        token: this.token,
        newPassword: this.resetPasswordForm.get('newPassword')?.value,
        confirmPassword: this.resetPasswordForm.get('confirmPassword')?.value
      };

      this.authService.resetPassword(request).subscribe({
        next: (response) => {
          // O sucesso é tratado pelo effect
        },
        error: (error) => {
          // Os erros são tratados pelo AuthService
        }
      });
    } else {
      // Marcar todos os campos como touched para mostrar erros
      this.resetPasswordForm.markAllAsTouched();
    }
  }

  /**
   * Getters para facilitar o acesso aos campos do formulário
   */
  get newPassword() {
    return this.resetPasswordForm.get('newPassword');
  }

  get confirmPassword() {
    return this.resetPasswordForm.get('confirmPassword');
  }

  /**
   * Verifica se há erro de senhas não coincidentes
   */
  get hasPasswordMismatch(): boolean {
    return this.resetPasswordForm.hasError('passwordMismatch') && 
           this.confirmPassword?.touched && 
           this.confirmPassword?.value;
  }

  /**
   * Getters para acessar os signals do AuthService
   */
  get isLoading() {
    return this.authService.isResetPasswordLoading();
  }

  get error() {
    return this.authService.resetPasswordError();
  }

  get validationErrors() {
    return this.authService.resetPasswordValidationErrors();
  }

  get success() {
    return this.authService.resetPasswordSuccess();
  }
}
