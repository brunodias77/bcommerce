import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { GuestGuard } from './guards/guest.guard';

export const routes: Routes = [
  // Rota padrão redireciona para home
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  // Rota da página inicial
  {
    path: 'home',
    // ./features/home/pages/home-page/home-page
    loadComponent: () =>
      import('./features/home/pages/home-page/home-page').then((m) => m.HomePage),
  },

  // Rotas públicas (apenas para usuários não autenticados)
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/pages/login-page/login-page').then((m) => m.LoginPage),
    canActivate: [GuestGuard],
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/pages/register-page/register-page').then((m) => m.RegisterPage),
    canActivate: [GuestGuard],
  },
  {
    path: 'forget-password',
    loadComponent: () =>
      import('./features/auth/pages/forget-password-page/forget-password-page').then((m) => m.ForgetPasswordPage),
    canActivate: [GuestGuard],
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./features/auth/pages/reset-password-page/reset-password-page').then((m) => m.ResetPasswordPageComponent),
    canActivate: [GuestGuard],
  },
  {
    path: 'confirm-email',
    loadComponent: () =>
      import('./features/auth/pages/confirm-email/confirm-email').then((m) => m.ConfirmEmailComponent),
  },
  {
    path: 'activate',
    loadComponent: () =>
      import('./features/auth/pages/activate-page/activate-page').then((m) => m.ActivatePageComponent),
  },
  // Rotas protegidas (apenas para usuários autenticados)
  {
    path: 'profile',
    loadComponent: () =>
      import('./features/profile/page/profile-page/profile-page').then((m) => m.ProfilePage),
    canActivate: [AuthGuard],
  },
  {
    path: 'favorites',
    loadComponent: () =>
      import('./features/profile/page/favorites/favorites').then((m) => m.Favorites),
    canActivate: [AuthGuard],
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./features/cart/pages/cart-page/cart-page').then((m) => m.CartPage),
  },
  // Rotas administrativas (apenas para admins)
];
