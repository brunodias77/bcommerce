import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError, finalize } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateUserResponse } from '../../models/responses';
import { CreateUserRequest } from '../../models/requests';





/**
 * Estados possíveis do registro
 */
export type RegisterState = 'idle' | 'loading' | 'success' | 'error';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = environment.apiUrl || 'http://localhost:5000';
  
  // Signals para gerenciamento de estado
  private readonly _registerState = signal<RegisterState>('idle');
  private readonly _registerError = signal<string | null>(null);
  private readonly _validationErrors = signal<string[]>([]);
  private readonly _lastRegisteredUser = signal<CreateUserResponse | null>(null);
  
  // Computed signals para estado derivado
  readonly isRegistering = computed(() => this._registerState() === 'loading');
  readonly registerSuccess = computed(() => this._registerState() === 'success');
  readonly registerError = computed(() => this._registerError());
  readonly validationErrors = computed(() => this._validationErrors());
  readonly lastRegisteredUser = computed(() => this._lastRegisteredUser());
  readonly hasErrors = computed(() => this._registerState() === 'error');
  
  constructor(private http: HttpClient) {}
  
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
}
