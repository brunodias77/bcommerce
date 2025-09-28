import { Component, OnInit, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../services/auth/auth-service';
import { ActivateAccountRequest } from '../../../../models/requests';

@Component({
  selector: 'app-activate-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activate-page.html',
  styleUrls: ['./activate-page.css']
})
export class ActivatePageComponent implements OnInit {
  private token = signal<string | null>(null);
  
  // Computed signals para o template
  readonly isActivating = computed(() => this.authService.isActivating());
  readonly activateSuccess = computed(() => this.authService.activateSuccess());
  readonly activateError = computed(() => this.authService.activateError());
  readonly activateMessage = computed(() => this.authService.activateMessage());
  readonly hasActivateErrors = computed(() => this.authService.hasActivateErrors());

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Obter o token da query string
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      if (token) {
        this.token.set(token);
        // Ativar a conta imediatamente após obter o token
        this.activateAccount(token);
      } else {
        // Se não há token, redirecionar para login com erro
        this.router.navigate(['/login'], {
          queryParams: { error: 'Token de ativação não fornecido' }
        });
      }
    });
  }

  private activateAccount(token: string): void {
    const request: ActivateAccountRequest = { token };
    
    this.authService.activate(request).subscribe({
      next: (response) => {
        console.log('Conta ativada com sucesso:', response);
        // Após 3 segundos, redirecionar para login
        setTimeout(() => {
          this.router.navigate(['/login'], {
            queryParams: { message: 'Conta ativada com sucesso! Faça login para continuar.' }
          });
        }, 3000);
      },
      error: (error) => {
        console.error('Erro na ativação:', error);
        // Em caso de erro, mostrar opção de voltar ao login após 5 segundos
        setTimeout(() => {
          this.showReturnToLogin();
        }, 5000);
      }
    });
  }

  private showReturnToLogin(): void {
    // Método para mostrar botão de retorno ao login após erro
  }

  onReturnToLogin(): void {
    this.router.navigate(['/login']);
  }

  onTryAgain(): void {
    const tokenValue = this.token();
    if (tokenValue) {
      this.activateAccount(tokenValue);
    }
  }
}