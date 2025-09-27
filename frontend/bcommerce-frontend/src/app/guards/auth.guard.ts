import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, map, take } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { AuthService } from '../services/auth/auth-service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Determina se uma rota pode ser ativada verificando se o usuário está autenticado.
   * Apenas usuários autenticados com tokens válidos podem acessar rotas protegidas.
   * Usuários não autenticados são redirecionados para a página de login.
   * 
   * @returns Observable<boolean | UrlTree> - true se o usuário estiver autenticado, UrlTree para redirecionamento se não autenticado
   */
  canActivate(): Observable<boolean | UrlTree> {
    return toObservable(this.authService.isAuthenticated).pipe(
      take(1),
      map(isAuthenticated => {
        // Verifica tanto o status de autenticação quanto a expiração do token
        const isUserAuthenticated = this.authService.isUserAuthenticated();
        
        if (isAuthenticated && isUserAuthenticated) {
          // Usuário está autenticado e token é válido, permitir acesso
          return true;
        }
        
        // Usuário não está autenticado ou token expirou, redirecionar para login
        return this.router.createUrlTree(['/login']);
      })
    );
  }
}
