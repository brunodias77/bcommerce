import { Component, OnInit, OnDestroy, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../services/auth/auth-service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { LoginUserRequest } from '../../../../models/requests';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage implements OnInit, OnDestroy {
  loginForm: FormGroup;
  private subscription = new Subscription();

  // Computed signals do AuthService
  readonly isLoading = computed(() => this.authService.isLoggingIn());
  readonly errorMessage = computed(() => this.authService.loginError() || '');
  readonly hasErrors = computed(() => this.authService.hasLoginErrors());
  readonly loginSuccess = computed(() => this.authService.loginSuccess());

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    // Effect para navegar quando login for bem-sucedido
    effect(() => {
      if (this.loginSuccess()) {
        setTimeout(() => {
          this.router.navigate(['/profile']);
        }, 1000);
      }
    });
  }

  ngOnInit(): void {
    // Limpar estado anterior do login
    this.authService.resetLoginState();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;

      const loginRequest: LoginUserRequest = {
        email,
        password,
      };

      // Usar Observable do AuthService
      const loginSub = this.authService.login(loginRequest).subscribe({
        next: (response) => {
          // O AuthService já gerencia o estado de sucesso via signals
          console.log('Login realizado com sucesso:', response);
        },
        error: (error) => {
          // O AuthService já gerencia o estado de erro via signals
          console.error('Erro no login:', error);
        },
      });

      this.subscription.add(loginSub);
    }
  }

  clearLoginError() {
    this.authService.clearLoginError();
  }
}
