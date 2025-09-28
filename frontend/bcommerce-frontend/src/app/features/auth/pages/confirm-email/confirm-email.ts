import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../services/auth/auth-service';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.css',
})
export class ConfirmEmailComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  // Signals para gerenciar estados
  userEmail = signal<string>('');
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');
  successMessage = signal<string>('');
  resendCooldown = signal<number>(0);
  message = signal<string>('');

  // Computed para verificar se pode reenviar
  canResend = computed(() => !this.isLoading() && this.resendCooldown() === 0);

  private cooldownInterval?: number;

  ngOnInit(): void {
    console.log('🔄 ConfirmEmail: ngOnInit executado');

    // Recuperar parâmetros da query string
    this.route.queryParams.subscribe((params) => {
      if (params['message']) {
        this.message.set(params['message']);
        console.log('📩 ConfirmEmail: Mensagem dos params:', params['message']);
      }
    });

    // Recuperar email do estado de navegação ou localStorage
    const navigation = this.router.getCurrentNavigation();
    const emailFromState = navigation?.extras?.state?.['email'];
    const emailFromStorage = localStorage.getItem('pendingConfirmationEmail');

    console.log('📧 ConfirmEmail: Email do estado:', emailFromState);
    console.log('📧 ConfirmEmail: Email do localStorage:', emailFromStorage);

    const email = emailFromState || emailFromStorage || '';
    this.userEmail.set(email);

    // Se não há email, redirecionar para registro
    if (!email) {
      console.log('❌ ConfirmEmail: Nenhum email encontrado, redirecionando para /register');
      this.router.navigate(['/register']);
      return;
    }

    // Salvar email no localStorage para persistência
    if (email) {
      localStorage.setItem('pendingConfirmationEmail', email);
      console.log('💾 ConfirmEmail: Email salvo no localStorage:', email);
    }

    // Definir mensagem padrão se não houver
    if (!this.message()) {
      this.message.set('Verifique seu email para ativar sua conta.');
    }
  }

  /**
   * Reenvia o email de confirmação
   */
  async resendConfirmation(): Promise<void> {
    if (!this.canResend() || !this.userEmail()) {
      return;
    }

    console.log('📤 ConfirmEmail: Reenviando confirmação para:', this.userEmail());

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    try {
      // Simular reenvio de email
      await this.simulateResendEmail(this.userEmail());

      this.successMessage.set('Email de confirmação reenviado com sucesso!');
      this.startCooldown();
    } catch (error: any) {
      console.error('❌ ConfirmEmail: Erro ao reenviar email:', error);
      this.errorMessage.set(error?.message || 'Erro ao reenviar email. Tente novamente.');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Simula o reenvio de email (já que não há endpoint específico)
   */
  private async simulateResendEmail(email: string): Promise<void> {
    // Simular delay de requisição
    await new Promise((resolve) => setTimeout(resolve, 1500));

    // Em um cenário real, aqui faria uma chamada para um endpoint de reenvio
    // Por enquanto, apenas simulamos sucesso
    console.log(`✅ ConfirmEmail: Simulando reenvio de email para: ${email}`);
  }

  /**
   * Inicia o cooldown para reenvio
   */
  private startCooldown(): void {
    this.resendCooldown.set(60); // 60 segundos

    this.cooldownInterval = window.setInterval(() => {
      const current = this.resendCooldown();
      if (current <= 1) {
        this.resendCooldown.set(0);
        if (this.cooldownInterval) {
          clearInterval(this.cooldownInterval);
          this.cooldownInterval = undefined;
        }
      } else {
        this.resendCooldown.set(current - 1);
      }
    }, 1000);
  }

  /**
   * Redireciona para a página de login
   */
  goToLogin(): void {
    console.log('🔄 ConfirmEmail: Redirecionando para /login');
    this.router.navigate(['/login']);
  }

  /**
   * Redireciona para a página inicial
   */
  goToHome(): void {
    console.log('🔄 ConfirmEmail: Redirecionando para /home');
    this.router.navigate(['/home']);
  }

  /**
   * Limpa dados e intervalos quando o componente é destruído
   */
  ngOnDestroy(): void {
    console.log('🧹 ConfirmEmail: ngOnDestroy - Limpando recursos');

    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }

    // Manter o email no localStorage para caso o usuário volte à página
    // localStorage.removeItem('pendingConfirmationEmail');
  }
}
