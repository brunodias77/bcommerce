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
  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Determines if a route can be activated by checking if the user is NOT authenticated.
   * Guests (non-authenticated users) can access the route.
   * Authenticated users are redirected to the dashboard.
   * 
   * @returns Observable<boolean | UrlTree> - true if user is not authenticated, UrlTree for redirect if authenticated
   */
  canActivate(): Observable<boolean | UrlTree> {
    return toObservable(this.authService.isAuthenticated).pipe(
      take(1),
      map(isAuthenticated => {
        if (isAuthenticated) {
          // User is authenticated, redirect to dashboard
          return this.router.createUrlTree(['/dashboard']);
        }
        // User is not authenticated, allow access to guest-only routes
        return true;
      })
    );
  }
}
