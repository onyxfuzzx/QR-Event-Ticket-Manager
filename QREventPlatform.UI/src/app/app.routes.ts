import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/login/login.component')
        .then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./auth/forgot-password/forgot-password.component')
        .then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./auth/reset-password/reset-password.component')
        .then(m => m.ResetPasswordComponent)
  },

  {
    path: 'superadmin',
    canActivate: [authGuard, roleGuard(['SuperAdmin'])],
    loadComponent: () =>
      import('./dashboards/superadmin/superadmin.component')
        .then(m => m.SuperAdminComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard(['Admin'])],
    loadComponent: () =>
      import('./dashboards/admin/admin.component')
        .then(m => m.AdminComponent)
  },
  {
    path: 'worker',
    canActivate: [authGuard, roleGuard(['Worker'])],
    loadComponent: () =>
      import('./dashboards/worker/worker.component')
        .then(m => m.WorkerComponent)
  },
  {
    path: 'event/:eventId',
    loadComponent: () =>
      import('./public/event-register/event-register.component')
        .then(m => m.EventRegisterComponent)
  },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];
