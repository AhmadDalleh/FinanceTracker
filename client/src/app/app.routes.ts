import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'accounts' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'accounts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/account-list/account-list.component').then((m) => m.AccountListComponent)
  },
  {
    path: 'accounts/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/account-form/account-form.component').then((m) => m.AccountFormComponent)
  },
  {
    path: 'accounts/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/accounts/account-form/account-form.component').then((m) => m.AccountFormComponent)
  },
  {
    path: 'transactions',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/transaction-list/transaction-list.component').then(
        (m) => m.TransactionListComponent
      )
  },
  {
    path: 'transactions/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form.component').then(
        (m) => m.TransactionFormComponent
      )
  },
  {
    path: 'transactions/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form.component').then(
        (m) => m.TransactionFormComponent
      )
  },
  { path: '**', redirectTo: 'accounts' }
];
