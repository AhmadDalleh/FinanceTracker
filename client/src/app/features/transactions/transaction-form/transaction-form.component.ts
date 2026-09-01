import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Account } from '../../../core/models/account.model';
import { Category } from '../../../core/models/category.model';
import { TRANSACTION_TYPE_LABELS, TransactionType } from '../../../core/models/transaction.model';
import { AccountService } from '../../../core/services/account.service';
import { CategoryService } from '../../../core/services/category.service';
import { TransactionService } from '../../../core/services/transaction.service';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.scss'
})
export class TransactionFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly transactionService = inject(TransactionService);
  private readonly accountService = inject(AccountService);
  private readonly categoryService = inject(CategoryService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly transactionTypes = Object.entries(TRANSACTION_TYPE_LABELS).map(([value, label]) => ({
    value: Number(value) as TransactionType,
    label
  }));
  readonly accounts = signal<Account[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly isEditMode = signal(false);
  readonly addingCategory = signal(false);

  private transactionId: string | null = null;

  readonly form = this.fb.nonNullable.group({
    accountId: ['', Validators.required],
    categoryId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    type: [TransactionType.Expense, Validators.required],
    date: [this.today(), Validators.required],
    note: ['']
  });

  readonly newCategoryName = this.fb.nonNullable.control('');

  ngOnInit(): void {
    this.accountService.getAccounts().subscribe((accounts) => this.accounts.set(accounts));
    this.loadCategories();

    this.transactionId = this.route.snapshot.paramMap.get('id');
    if (this.transactionId) {
      this.isEditMode.set(true);
      this.form.controls.accountId.disable();

      this.transactionService.getById(this.transactionId).subscribe({
        next: (transaction) => {
          this.form.patchValue({
            accountId: transaction.accountId,
            categoryId: transaction.categoryId,
            amount: transaction.amount,
            type: transaction.type,
            date: transaction.date,
            note: transaction.note ?? ''
          });
        },
        error: () => this.error.set('Could not load this transaction.')
      });
    }
  }

  toggleAddCategory(): void {
    this.addingCategory.update((value) => !value);
  }

  addCategory(): void {
    const name = this.newCategoryName.value.trim();
    if (!name) {
      return;
    }

    this.categoryService.create({ name }).subscribe({
      next: (id) => {
        this.newCategoryName.setValue('');
        this.addingCategory.set(false);
        this.loadCategories(id);
      },
      error: () => this.error.set('Could not create this category.')
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const value = this.form.getRawValue();

    const onSuccess = () => this.router.navigate(['/transactions']);
    const onError = () => {
      this.error.set('Could not save this transaction.');
      this.saving.set(false);
    };

    if (this.isEditMode()) {
      this.transactionService
        .update({
          id: this.transactionId!,
          categoryId: value.categoryId,
          amount: value.amount,
          type: value.type,
          date: value.date,
          note: value.note || null
        })
        .subscribe({ next: onSuccess, error: onError });
    } else {
      this.transactionService
        .create({
          accountId: value.accountId,
          categoryId: value.categoryId,
          amount: value.amount,
          type: value.type,
          date: value.date,
          note: value.note || null
        })
        .subscribe({ next: onSuccess, error: onError });
    }
  }

  private loadCategories(selectAfterLoadId?: string): void {
    this.categoryService.getCategories().subscribe((categories) => {
      this.categories.set(categories);
      if (selectAfterLoadId) {
        this.form.controls.categoryId.setValue(selectAfterLoadId);
      }
    });
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
