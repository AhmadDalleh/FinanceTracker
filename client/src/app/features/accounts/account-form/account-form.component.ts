import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ACCOUNT_TYPE_LABELS, AccountType } from '../../../core/models/account.model';
import { AccountService } from '../../../core/services/account.service';
import { extractErrorMessage } from '../../../core/utils/error-message';

@Component({
  selector: 'app-account-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './account-form.component.html',
  styleUrl: './account-form.component.scss'
})
export class AccountFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly accountTypes = Object.entries(ACCOUNT_TYPE_LABELS).map(([value, label]) => ({
    value: Number(value) as AccountType,
    label
  }));
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly isEditMode = signal(false);

  private accountId: string | null = null;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    type: [AccountType.Checking, Validators.required],
    startingBalance: [0, [Validators.required, Validators.min(0)]],
    currency: ['USD', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]]
  });

  ngOnInit(): void {
    this.accountId = this.route.snapshot.paramMap.get('id');

    if (this.accountId) {
      this.isEditMode.set(true);
      this.form.controls.startingBalance.disable();
      this.form.controls.currency.disable();

      this.accountService.getById(this.accountId).subscribe({
        next: (account) => {
          this.form.patchValue({
            name: account.name,
            type: account.type,
            startingBalance: account.balance,
            currency: account.currency
          });
        },
        error: (response: HttpErrorResponse) => this.error.set(extractErrorMessage(response, 'Could not load this account.'))
      });
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const value = this.form.getRawValue();

    const onSuccess = () => this.router.navigate(['/accounts']);
    const onError = (response: HttpErrorResponse) => {
      this.error.set(extractErrorMessage(response, 'Could not save this account.'));
      this.saving.set(false);
    };

    if (this.isEditMode()) {
      this.accountService
        .update({ id: this.accountId!, name: value.name, type: value.type })
        .subscribe({ next: onSuccess, error: onError });
    } else {
      this.accountService
        .create({
          name: value.name,
          type: value.type,
          startingBalance: value.startingBalance,
          currency: value.currency
        })
        .subscribe({ next: onSuccess, error: onError });
    }
  }
}
