import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  GuardResult,
  MaybeAsync,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { Observable, map, take } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';
import { AuthService } from '../services/auth/auth-service';

@Injectable({
  providedIn: 'root',
})
export class GuestGuard implements CanActivate {
  private readonly isAuthenticated$: Observable<boolean>;

  constructor(private authService: AuthService, private router: Router) {
    // Converte o signal para Observable no contexto de injeção
    this.isAuthenticated$ = toObservable(this.authService.isAuthenticated);
  }

  /**
   * Determines if a route can be activated by checking if the user is NOT authenticated.
   * Guests (non-authenticated users) can access the route.
   * Authenticated users are redirected to the dashboard.
   *
   * @returns Observable<boolean | UrlTree> - true if user is not authenticated, UrlTree for redirect if authenticated
   */
  canActivate(): Observable<boolean | UrlTree> {
    return this.isAuthenticated$.pipe(
      take(1),
      map((isAuthenticated) => {
        if (isAuthenticated) {
          // User is authenticated, redirect to dashboard
          return this.router.createUrlTree(['/profile']);
        }
        // User is not authenticated, allow access to guest-only routes
        return true;
      })
    );
  }
}
