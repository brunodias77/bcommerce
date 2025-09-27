import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError, finalize, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateUserResponse,
  LoginUserResponse,
  ActivateAccountResponse,
  ConfirmEmailResponse,
} from '../../models/responses';
import {
  CreateUserRequest,
  LoginUserRequest,
  ActivateAccountRequest,
  ConfirmEmailRequest,
} from '../../models/requests';

/**
 * Estados possíveis do registro
 */
export type RegisterState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Estados possíveis do login
 */
export type LoginState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Estados possíveis da ativação de conta
 */
export type ActivateState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Estados possíveis da autenticação
 */
export type AuthState = 'authenticated' | 'unauthenticated' | 'checking';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly baseUrl = environment.apiUrl || 'http://localhost:5000';

  // Signals para gerenciamento de estado do registro
  private readonly _registerState = signal<RegisterState>('idle');
  private readonly _registerError = signal<string | null>(null);
  private readonly _validationErrors = signal<string[]>([]);
  private readonly _lastRegisteredUser = signal<CreateUserResponse | null>(null);

  // Signals para gerenciamento de estado do login
  private readonly _loginState = signal<LoginState>('idle');
  private readonly _loginError = signal<string | null>(null);
  private readonly _loginValidationErrors = signal<string[]>([]);

  // Signals para gerenciamento de estado da ativação
  private readonly _activateState = signal<ActivateState>('idle');
  private readonly _activateError = signal<string | null>(null);
  private readonly _activateMessage = signal<string | null>(null);

  // Signals para confirmação de email
  private readonly _confirmEmailState = signal<'idle' | 'loading' | 'success' | 'error'>('idle');
  private readonly _confirmEmailError = signal<string | null>(null);
  private readonly _confirmEmailMessage = signal<string | null>(null);

  // Signals para gerenciamento de autenticação
  private readonly _authState = signal<AuthState>('unauthenticated');
  private readonly _accessToken = signal<string | null>(null);
  private readonly _refreshToken = signal<string | null>(null);
  private readonly _tokenExpiresAt = signal<Date | null>(null);
  private readonly _currentUser = signal<any | null>(null);

  // Computed signals para estado derivado do registro
  readonly isRegistering = computed(() => this._registerState() === 'loading');
  readonly registerSuccess = computed(() => this._registerState() === 'success');
  readonly registerError = computed(() => this._registerError());
  readonly validationErrors = computed(() => this._validationErrors());
  readonly lastRegisteredUser = computed(() => this._lastRegisteredUser());
  readonly hasRegisterErrors = computed(() => this._registerState() === 'error');

  // Computed signals para estado derivado do login
  readonly isLoggingIn = computed(() => this._loginState() === 'loading');
  readonly loginSuccess = computed(() => this._loginState() === 'success');
  readonly loginError = computed(() => this._loginError());
  readonly loginValidationErrors = computed(() => this._loginValidationErrors());
  readonly hasLoginErrors = computed(() => this._loginState() === 'error');

  // Computed signals para estado derivado da ativação
  readonly isActivating = computed(() => this._activateState() === 'loading');
  readonly activateSuccess = computed(() => this._activateState() === 'success');
  readonly activateError = computed(() => this._activateError());
  readonly activateMessage = computed(() => this._activateMessage());
  readonly hasActivateErrors = computed(() => this._activateState() === 'error');

  // Computed signals para confirmação de email
  readonly isConfirmingEmail = computed(() => this._confirmEmailState() === 'loading');
  readonly confirmEmailSuccess = computed(() => this._confirmEmailState() === 'success');
  readonly confirmEmailError = computed(() => this._confirmEmailError());
  readonly confirmEmailMessage = computed(() => this._confirmEmailMessage());
  readonly hasConfirmEmailErrors = computed(() => this._confirmEmailState() === 'error');

  // Computed signals para estado derivado da autenticação
  readonly isAuthenticated = computed(() => this._authState() === 'authenticated');
  readonly isCheckingAuth = computed(() => this._authState() === 'checking');
  readonly accessToken = computed(() => this._accessToken());
  readonly refreshToken = computed(() => this._refreshToken());
  readonly currentUser = computed(() => this._currentUser());
  readonly isTokenExpired = computed(() => {
    const expiresAt = this._tokenExpiresAt();
    return expiresAt ? new Date() >= expiresAt : true;
  });

  constructor(private http: HttpClient) {
    // Inicializar estado de autenticação verificando tokens salvos
    this.initializeAuthState();
  }

  /**
   * Inicializa o estado de autenticação verificando tokens salvos no localStorage
   */
  private initializeAuthState(): void {
    const accessToken = this.getStoredAccessToken();
    const refreshToken = this.getStoredRefreshToken();
    const expiresAt = this.getStoredTokenExpiration();

    if (accessToken && refreshToken) {
      this._accessToken.set(accessToken);
      this._refreshToken.set(refreshToken);
      this._tokenExpiresAt.set(expiresAt);

      // Verificar se o token ainda é válido
      if (expiresAt && new Date() < expiresAt) {
        this._authState.set('authenticated');
      } else {
        this.clearTokens();
        this._authState.set('unauthenticated');
      }
    } else {
      this._authState.set('unauthenticated');
    }
  }

  /**
   * Registra um novo usuário no sistema
   * @param request Dados do usuário para registro
   * @returns Observable com a resposta do registro
   */
  register(request: CreateUserRequest): Observable<CreateUserResponse> {
    // Reset do estado anterior
    console.log('🔄 AuthService: Iniciando registro...');
    this._registerState.set('loading');
    this._registerError.set(null);
    this._validationErrors.set([]);

    return this.http.post<CreateUserResponse>(`${this.baseUrl}/api/auth/register`, request).pipe(
      tap((response: CreateUserResponse) => {
        // Definir estado de sucesso e armazenar resposta
        console.log('✅ AuthService: Registro bem-sucedido, definindo estado como success');
        this._registerState.set('success');
        this._lastRegisteredUser.set(response);
        console.log('📊 AuthService: registerSuccess() =', this.registerSuccess());
      }),
      catchError((error: HttpErrorResponse) => {
        console.log('❌ AuthService: Erro no registro:', error);
        return this.handleRegisterError(error);
      }),
      finalize(() => {
        if (this._registerState() === 'loading') {
          this._registerState.set('idle');
        }
        console.log('🏁 AuthService: Finalizando registro, estado final:', this._registerState());
      })
    );
  }

  /**
   * Realiza login do usuário no sistema
   * @param request Dados de login (email e senha)
   * @returns Observable com a resposta do login
   */
  login(request: LoginUserRequest): Observable<LoginUserResponse> {
    // Reset do estado anterior
    this._loginState.set('loading');
    this._loginError.set(null);
    this._loginValidationErrors.set([]);

    return this.http.post<LoginUserResponse>(`${this.baseUrl}/api/auth/login`, request).pipe(
      tap((response: LoginUserResponse) => {
        // Armazenar tokens e atualizar estado de autenticação
        this.storeTokens(response);
      }),
      catchError((error: HttpErrorResponse) => this.handleLoginError(error)),
      finalize(() => {
        if (this._loginState() === 'loading') {
          this._loginState.set('idle');
        }
      })
    );
  }

  /**
   * Trata erros específicos do registro baseado nos códigos HTTP do backend
   * @param error Erro HTTP recebido
   * @returns Observable com erro tratado
   */
  private handleRegisterError(error: HttpErrorResponse): Observable<never> {
    this._registerState.set('error');

    switch (error.status) {
      case 400:
        // Erros de validação
        if (error.error?.errors && Array.isArray(error.error.errors)) {
          this._validationErrors.set(error.error.errors);
          this._registerError.set('Dados inválidos. Verifique os campos e tente novamente.');
        } else if (error.error?.message) {
          this._registerError.set(error.error.message);
        } else {
          this._registerError.set('Dados inválidos. Verifique os campos e tente novamente.');
        }
        break;

      case 409:
        // Conflito - usuário já existe
        this._registerError.set(
          'Este email já está cadastrado. Tente fazer login ou use outro email.'
        );
        break;

      case 500:
        // Erro interno do servidor
        this._registerError.set('Erro interno do servidor. Tente novamente mais tarde.');
        break;

      default:
        // Outros erros
        this._registerError.set('Erro inesperado. Tente novamente mais tarde.');
        break;
    }

    return throwError(() => error);
  }

  /**
   * Trata erros específicos do login baseado nos códigos HTTP do backend
   * @param error Erro HTTP recebido
   * @returns Observable com erro tratado
   */
  private handleLoginError(error: HttpErrorResponse): Observable<never> {
    this._loginState.set('error');

    switch (error.status) {
      case 400:
        // Erros de validação
        if (error.error?.errors && Array.isArray(error.error.errors)) {
          this._loginValidationErrors.set(error.error.errors);
          this._loginError.set('Dados inválidos. Verifique email e senha.');
        } else if (error.error?.message) {
          this._loginError.set(error.error.message);
        } else {
          this._loginError.set('Dados inválidos. Verifique email e senha.');
        }
        break;

      case 401:
        // Não autorizado - credenciais inválidas
        this._loginError.set('Email ou senha incorretos. Tente novamente.');
        break;

      case 500:
        // Erro interno do servidor
        this._loginError.set('Erro interno do servidor. Tente novamente mais tarde.');
        break;

      default:
        // Outros erros
        this._loginError.set('Erro inesperado. Tente novamente mais tarde.');
        break;
    }

    return throwError(() => error);
  }

  /**
   * Limpa o estado de erro do registro
   */
  clearRegisterError(): void {
    this._registerError.set(null);
    this._validationErrors.set([]);
    if (this._registerState() === 'error') {
      this._registerState.set('idle');
    }
  }

  /**
   * Limpa todos os estados do registro
   */
  resetRegisterState(): void {
    this._registerState.set('idle');
    this._registerError.set(null);
    this._validationErrors.set([]);
    this._lastRegisteredUser.set(null);
  }

  /**
   * Limpa o estado de erro do login
   */
  clearLoginError(): void {
    this._loginError.set(null);
    this._loginValidationErrors.set([]);
    if (this._loginState() === 'error') {
      this._loginState.set('idle');
    }
  }

  /**
   * Limpa todos os estados do login
   */
  resetLoginState(): void {
    this._loginState.set('idle');
    this._loginError.set(null);
    this._loginValidationErrors.set([]);
  }

  /**
   * Armazena os tokens de autenticação no localStorage
   * @param response Resposta do login contendo os tokens
   */
  private storeTokens(response: LoginUserResponse): void {
    console.log('Dados recebidos para armazenamento:', response);

    // Validar se expires_in é um número válido e maior que 0
    const expiresInSeconds =
      response.expires_in && response.expires_in > 0 ? response.expires_in : 3600; // Default: 1 hora
    const expiresAt = new Date(Date.now() + expiresInSeconds * 1000);

    // Validar se a data criada é válida
    if (isNaN(expiresAt.getTime())) {
      console.error('Erro ao calcular data de expiração do token:', {
        expires_in: response.expires_in,
        expiresInSeconds,
        currentTime: Date.now(),
      });
      // Usar data padrão de 1 hora a partir de agora
      const fallbackExpiresAt = new Date(Date.now() + 3600 * 1000);
      localStorage.setItem('token_expires_at', fallbackExpiresAt.toISOString());
      this._tokenExpiresAt.set(fallbackExpiresAt);
    } else {
      localStorage.setItem('token_expires_at', expiresAt.toISOString());
      this._tokenExpiresAt.set(expiresAt);
    }

    // Armazenar tokens no localStorage
    if (response.access_token) {
      localStorage.setItem('access_token', response.access_token);
      this._accessToken.set(response.access_token);
    }

    if (response.refresh_token) {
      localStorage.setItem('refresh_token', response.refresh_token);
      this._refreshToken.set(response.refresh_token);
    }

    if (response.token_type) {
      localStorage.setItem('token_type', response.token_type);
    }

    // Atualizar signals de estado
    this._authState.set('authenticated');
    this._loginState.set('success');

    console.log('Tokens armazenados com sucesso no localStorage');
  }

  /**
   * Recupera o access token armazenado no localStorage
   * @returns Access token ou null se não existir
   */
  private getStoredAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }

  /**
   * Recupera o refresh token armazenado no localStorage
   * @returns Refresh token ou null se não existir
   */
  private getStoredRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  /**
   * Recupera a data de expiração do token armazenada no localStorage
   * @returns Data de expiração ou null se não existir
   */
  private getStoredTokenExpiration(): Date | null {
    const expiresAt = localStorage.getItem('token_expires_at');
    return expiresAt ? new Date(expiresAt) : null;
  }

  /**
   * Remove todos os tokens do localStorage
   */
  private clearTokens(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expires_at');
    localStorage.removeItem('token_type');

    // Limpar signals
    this._accessToken.set(null);
    this._refreshToken.set(null);
    this._tokenExpiresAt.set(null);
    this._currentUser.set(null);
    this._authState.set('unauthenticated');
  }

  /**
   * Ativa a conta do usuário usando o token fornecido
   * @param request - Dados da requisição de ativação
   * @returns Observable com a resposta da ativação
   */
  activate(request: ActivateAccountRequest): Observable<ActivateAccountResponse> {
    // Reset do estado
    this._activateState.set('loading');
    this._activateError.set(null);
    this._activateMessage.set(null);

    const url = `${this.baseUrl}/api/auth/activate`;
    const params = new HttpParams().set('token', request.token);

    return this.http.get<ActivateAccountResponse>(url, { params }).pipe(
      tap((response) => {
        this._activateState.set('success');
        this._activateMessage.set(response.message || 'Conta ativada com sucesso!');
      }),
      catchError((error) => {
        this._activateState.set('error');

        // Tratamento específico dos códigos de status
        switch (error.status) {
          case 400:
            this._activateError.set('Token inválido ou expirado.');
            break;
          case 404:
            this._activateError.set('Token não encontrado ou já foi utilizado.');
            break;
          case 409:
            this._activateError.set('Esta conta já foi ativada anteriormente.');
            break;
          case 500:
            this._activateError.set('Erro interno do servidor. Tente novamente mais tarde.');
            break;
          default:
            this._activateError.set('Erro desconhecido durante a ativação da conta.');
        }

        return throwError(() => error);
      }),
      finalize(() => {
        // Não resetamos o estado aqui para manter a informação de sucesso/erro
      })
    );
  }

  /**
   * Limpa o estado de erro da confirmação de email
   */
  clearConfirmEmailError(): void {
    this._confirmEmailError.set(null);
    if (this._confirmEmailState() === 'error') {
      this._confirmEmailState.set('idle');
    }
  }

  /**
   * Limpa todos os estados da confirmação de email
   */
  resetConfirmEmailState(): void {
    this._confirmEmailState.set('idle');
    this._confirmEmailError.set(null);
    this._confirmEmailMessage.set(null);
  }

  /**
   * Realiza logout do usuário, limpando tokens e estado
   */
  logout(): void {
    this.clearTokens();
    this.resetLoginState();
    this.resetRegisterState();
    this.resetConfirmEmailState();
  }

  /**
   * Verifica se o usuário está autenticado
   * @returns True se autenticado, false caso contrário
   */
  isUserAuthenticated(): boolean {
    return this.isAuthenticated() && !this.isTokenExpired();
  }

  /**
   * Obtém informações do usuário atual (se disponível)
   * @returns Dados do usuário ou null
   */
  getCurrentUser(): any | null {
    return this._currentUser();
  }
}
