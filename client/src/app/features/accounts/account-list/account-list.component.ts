import { DecimalPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ACCOUNT_TYPE_LABELS, Account } from '../../../core/models/account.model';
import { AccountService } from '../../../core/services/account.service';

@Component({
  selector: 'app-account-list',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './account-list.component.html',
  styleUrl: './account-list.component.scss'
})
export class AccountListComponent implements OnInit {
  readonly accounts = signal<Account[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly accountTypeLabels = ACCOUNT_TYPE_LABELS;

  constructor(private readonly accountService: AccountService) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.loading.set(true);
    this.error.set(null);

    this.accountService.getAccounts().subscribe({
      next: (accounts) => {
        this.accounts.set(accounts);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load accounts. Are you signed in?');
        this.loading.set(false);
      }
    });
  }

  archive(account: Account): void {
    if (!confirm(`Archive "${account.name}"? It will be hidden from this list.`)) {
      return;
    }

    this.accountService.archive(account.id).subscribe({
      next: () => this.loadAccounts(),
      error: () => this.error.set('Could not archive the account.')
    });
  }
}
