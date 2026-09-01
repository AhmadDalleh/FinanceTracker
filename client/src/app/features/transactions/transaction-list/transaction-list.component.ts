import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Account } from '../../../core/models/account.model';
import { Category } from '../../../core/models/category.model';
import { PaginatedList } from '../../../core/models/paginated-list.model';
import { TRANSACTION_TYPE_LABELS, Transaction } from '../../../core/models/transaction.model';
import { AccountService } from '../../../core/services/account.service';
import { CategoryService } from '../../../core/services/category.service';
import { TransactionService } from '../../../core/services/transaction.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './transaction-list.component.html',
  styleUrl: './transaction-list.component.scss'
})
export class TransactionListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly transactionService = inject(TransactionService);
  private readonly accountService = inject(AccountService);
  private readonly categoryService = inject(CategoryService);

  readonly result = signal<PaginatedList<Transaction> | null>(null);
  readonly accounts = signal<Account[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly transactionTypeLabels = TRANSACTION_TYPE_LABELS;

  private pageNumber = 1;

  readonly filterForm = this.fb.group({
    accountId: [''],
    categoryId: [''],
    fromDate: [''],
    toDate: [''],
    minAmount: [null as number | null],
    maxAmount: [null as number | null]
  });

  ngOnInit(): void {
    this.accountService.getAccounts().subscribe((accounts) => this.accounts.set(accounts));
    this.categoryService.getCategories().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  accountName(accountId: string): string {
    return this.accounts().find((a) => a.id === accountId)?.name ?? accountId;
  }

  categoryName(categoryId: string): string {
    return this.categories().find((c) => c.id === categoryId)?.name ?? categoryId;
  }

  applyFilters(): void {
    this.pageNumber = 1;
    this.load();
  }

  goToPage(pageNumber: number): void {
    this.pageNumber = pageNumber;
    this.load();
  }

  delete(transaction: Transaction): void {
    if (!confirm('Delete this transaction? This will reverse its effect on the account balance.')) {
      return;
    }

    this.transactionService.delete(transaction.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete this transaction.')
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    const filters = this.filterForm.getRawValue();

    this.transactionService
      .getTransactions({
        accountId: filters.accountId || undefined,
        categoryId: filters.categoryId || undefined,
        fromDate: filters.fromDate || undefined,
        toDate: filters.toDate || undefined,
        minAmount: filters.minAmount ?? undefined,
        maxAmount: filters.maxAmount ?? undefined,
        pageNumber: this.pageNumber,
        pageSize: PAGE_SIZE
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load transactions. Are you signed in?');
          this.loading.set(false);
        }
      });
  }
}
