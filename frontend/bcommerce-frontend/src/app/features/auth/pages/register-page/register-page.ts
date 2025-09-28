import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, computed, effect } from '@angular/core';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../services/auth/auth-service';
import { CreateUserRequest } from '../../../../models/requests';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-register-page',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register-page.html',
  styleUrl: './register-page.css',
})
export class RegisterPage implements OnInit, OnDestroy {
  registerForm: FormGroup;
  private subscription = new Subscription();

  // Computed signals do AuthService
  readonly isLoading = computed(() => this.authService.isRegistering());
  readonly errorMessage = computed(() => this.authService.registerError() || '');
  readonly validationErrors = computed(() => this.authService.validationErrors());
  readonly hasErrors = computed(() => this.authService.hasRegisterErrors());
  readonly registerSuccess = computed(() => this.authService.registerSuccess());

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.registerForm = this.fb.group(
      {
        firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
        lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(155)]],
        email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(255),
            this.passwordValidator,
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        newsletterOptIn: [false, [Validators.required]],
      },
      { validators: this.passwordMatchValidator }
    );

    // Effect para navegar quando registro for bem-sucedido
    effect(() => {
      console.log('🔍 RegisterPage: Effect executado!');
      console.log('📊 RegisterPage: registerSuccess() =', this.registerSuccess());

      if (this.registerSuccess()) {
        console.log('✅ RegisterPage: Registro bem-sucedido, iniciando redirecionamento...');
        const email = this.registerForm.get('email')?.value;
        console.log('📧 RegisterPage: Email para redirecionamento:', email);

        // Salvar email no localStorage para persistência
        if (email) {
          localStorage.setItem('pendingConfirmationEmail', email);
        }

        setTimeout(() => {
          console.log('⏰ RegisterPage: Executando navegação para /confirm-email');
          this.router
            .navigate(['/confirm-email'], {
              state: { email: email },
              queryParams: {
                message:
                  'Registro realizado com sucesso! Verifique seu email para ativar sua conta.',
              },
            })
            .then((success) => {
              console.log('🚀 RegisterPage: Navegação concluída, sucesso:', success);
            })
            .catch((error) => {
              console.error('❌ RegisterPage: Erro na navegação:', error);
            });
        }, 2000);
      }
    });
  }

  ngOnInit(): void {
    console.log('🔄 RegisterPage: ngOnInit - Resetando estado do registro');
    // Limpar estado anterior do registro
    this.authService.resetRegisterState();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  /**
   * Validador customizado para senha
   * Verifica se a senha atende aos critérios de segurança
   */
  private passwordValidator(control: AbstractControl): { [key: string]: any } | null {
    const password = control.value;
    if (!password) return null;

    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumeric = /\d/.test(password);
    const hasSpecialChar = /[@$!%*?&]/.test(password);

    const valid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar;
    return valid ? null : { passwordStrength: true };
  }

  /**
   * Validador para confirmar se as senhas coincidem
   */
  private passwordMatchValidator(group: AbstractControl): { [key: string]: any } | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  /**
   * Submete o formulário de registro
   */
  onSubmit(): void {
    console.log('🚀 RegisterPage: onSubmit() chamado');
    console.log('📋 RegisterPage: Formulário válido?', this.registerForm.valid);

    if (this.registerForm.valid) {
      const formValue = this.registerForm.value;
      const request: CreateUserRequest = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        password: formValue.password,
        newsletterOptIn: formValue.newsletterOptIn,
      };

      console.log('📤 RegisterPage: Enviando requisição de registro:', request);

      const registerSub = this.authService.register(request).subscribe({
        next: (response) => {
          console.log('✅ RegisterPage: Usuário registrado com sucesso:', response);
          console.log('📊 RegisterPage: registerSuccess() após resposta =', this.registerSuccess());
        },
        error: (error) => {
          console.error('❌ RegisterPage: Erro no registro:', error);
        },
      });

      this.subscription.add(registerSub);
    } else {
      console.log('❌ RegisterPage: Formulário inválido, marcando campos como touched');
      // Marcar todos os campos como touched para mostrar erros
      this.markFormGroupTouched(this.registerForm);
    }
  }

  /**
   * Marca todos os campos do formulário como touched
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((field) => {
      const control = formGroup.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  /**
   * Verifica se um campo específico tem erro e foi touched
   */
  hasFieldError(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Obtém a mensagem de erro para um campo específico
   */
  getFieldErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (!field || !field.errors) return '';

    const errors = field.errors;

    switch (fieldName) {
      case 'firstName':
        if (errors['required']) return 'O primeiro nome é obrigatório';
        if (errors['minlength']) return 'O primeiro nome deve ter pelo menos 2 caracteres';
        if (errors['maxlength']) return 'O primeiro nome deve ter no máximo 100 caracteres';
        break;
      case 'lastName':
        if (errors['required']) return 'O último nome é obrigatório';
        if (errors['minlength']) return 'O último nome deve ter pelo menos 2 caracteres';
        if (errors['maxlength']) return 'O último nome deve ter no máximo 155 caracteres';
        break;
      case 'email':
        if (errors['required']) return 'O email é obrigatório';
        if (errors['email']) return 'Formato de email inválido';
        if (errors['maxlength']) return 'O email deve ter no máximo 255 caracteres';
        break;
      case 'password':
        if (errors['required']) return 'A senha é obrigatória';
        if (errors['minlength']) return 'A senha deve ter pelo menos 8 caracteres';
        if (errors['maxlength']) return 'A senha deve ter no máximo 255 caracteres';
        if (errors['passwordStrength'])
          return 'A senha deve conter pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial';
        break;
      case 'confirmPassword':
        if (errors['required']) return 'A confirmação de senha é obrigatória';
        break;
    }

    // Erro de senhas não coincidentes (validador do formulário)
    if (fieldName === 'confirmPassword' && this.registerForm.errors?.['passwordMismatch']) {
      return 'As senhas não coincidem';
    }

    return 'Campo inválido';
  }

  /**
   * Limpa os erros do formulário
   */
  clearErrors(): void {
    this.authService.clearRegisterError();
  }
}
