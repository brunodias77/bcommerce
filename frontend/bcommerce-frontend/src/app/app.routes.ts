import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { GuestGuard } from './guards/guest.guard';

export const routes: Routes = [
  // Rota padrão redireciona para home
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  // Rota da página inicial
  {
    path: 'home',
    loadComponent: () => import('./pages/home-page/home-page').then((m) => m.HomePage),
  },

  // Rotas públicas (apenas para usuários não autenticados)
  {
    path: 'login',
    loadComponent: () => import('./pages/login-page/login-page').then(m => m.LoginPage),
    canActivate: [GuestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register-page/register-page').then((m) => m.RegisterPage),
    canActivate: [GuestGuard],
  },
  // Rotas protegidas (apenas para usuários autenticados)

  // Rotas administrativas (apenas para admins)
];
