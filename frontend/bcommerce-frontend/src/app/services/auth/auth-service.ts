import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError, finalize, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUserResponse, LoginUserResponse } from '../../models/responses';
import { CreateUserRequest, LoginUserRequest } from '../../models/requests';





/**
 * Estados possíveis do registro
 */
export type RegisterState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Estados possíveis do login
 */
export type LoginState = 'idle' | 'loading' | 'success' | 'error';

/**
 * Estados possíveis da autenticação
 */
export type AuthState = 'authenticated' | 'unauthenticated' | 'checking';

@Injectable({
  providedIn: 'root'
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
    this._registerState.set('loading');
    this._registerError.set(null);
    this._validationErrors.set([]);
    
    return this.http.post<CreateUserResponse>(`${this.baseUrl}/api/auth/register`, request)
      .pipe(
        catchError((error: HttpErrorResponse) => this.handleRegisterError(error)),
        finalize(() => {
          if (this._registerState() === 'loading') {
            this._registerState.set('idle');
          }
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
    
    return this.http.post<LoginUserResponse>(`${this.baseUrl}/api/auth/login`, request)
      .pipe(
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
        this._registerError.set('Este email já está cadastrado. Tente fazer login ou use outro email.');
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
    const expiresAt = new Date(Date.now() + response.expiresIn * 1000);
    
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('token_expires_at', expiresAt.toISOString());
    localStorage.setItem('token_type', response.tokenType);
    
    // Atualizar signals
    this._accessToken.set(response.accessToken);
    this._refreshToken.set(response.refreshToken);
    this._tokenExpiresAt.set(expiresAt);
    this._authState.set('authenticated');
    this._loginState.set('success');
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
   * Realiza logout do usuário, limpando tokens e estado
   */
  logout(): void {
    this.clearTokens();
    this.resetLoginState();
    this.resetRegisterState();
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
