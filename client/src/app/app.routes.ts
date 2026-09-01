import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'accounts' },
  {
    path: 'accounts',
    loadComponent: () =>
      import('./features/accounts/account-list/account-list.component').then((m) => m.AccountListComponent)
  },
  {
    path: 'accounts/new',
    loadComponent: () =>
      import('./features/accounts/account-form/account-form.component').then((m) => m.AccountFormComponent)
  },
  {
    path: 'accounts/:id/edit',
    loadComponent: () =>
      import('./features/accounts/account-form/account-form.component').then((m) => m.AccountFormComponent)
  },
  {
    path: 'transactions',
    loadComponent: () =>
      import('./features/transactions/transaction-list/transaction-list.component').then(
        (m) => m.TransactionListComponent
      )
  },
  {
    path: 'transactions/new',
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form.component').then(
        (m) => m.TransactionFormComponent
      )
  },
  {
    path: 'transactions/:id/edit',
    loadComponent: () =>
      import('./features/transactions/transaction-form/transaction-form.component').then(
        (m) => m.TransactionFormComponent
      )
  },
  { path: '**', redirectTo: 'accounts' }
];
