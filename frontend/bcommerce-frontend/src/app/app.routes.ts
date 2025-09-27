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
    path: 'confirm-email',
    loadComponent: () =>
      import('./features/auth/pages/confirm-email/confirm-email').then((m) => m.ConfirmEmail),
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
  // Rotas administrativas (apenas para admins)
];
