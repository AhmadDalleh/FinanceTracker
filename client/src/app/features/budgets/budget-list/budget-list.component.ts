import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Budget } from '../../../core/models/budget.model';
import { Category } from '../../../core/models/category.model';
import { BudgetService } from '../../../core/services/budget.service';
import { CategoryService } from '../../../core/services/category.service';

@Component({
  selector: 'app-budget-list',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './budget-list.component.html',
  styleUrl: './budget-list.component.scss'
})
export class BudgetListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly budgetService = inject(BudgetService);
  private readonly categoryService = inject(CategoryService);

  readonly monthControl = this.fb.nonNullable.control(this.currentMonth());

  readonly budgets = signal<Budget[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly editingId = signal<string | null>(null);
  readonly savingEdit = signal(false);
  readonly editAmount = this.fb.nonNullable.control(0, [Validators.required, Validators.min(0.01)]);

  readonly adding = signal(false);
  readonly createForm = this.fb.nonNullable.group({
    categoryId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]]
  });

  readonly availableCategories = computed(() => {
    const budgetedCategoryIds = new Set(this.budgets().map((b) => b.categoryId));
    return this.categories().filter((c) => !budgetedCategoryIds.has(c.id));
  });

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  onMonthChange(): void {
    this.load();
  }

  toggleAdd(): void {
    this.adding.update((value) => !value);
    this.createForm.reset({ categoryId: '', amount: 0 });
  }

  addBudget(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const { year, month } = this.parseSelectedMonth();
    const value = this.createForm.getRawValue();

    this.error.set(null);
    this.budgetService.create({ categoryId: value.categoryId, year, month, amount: value.amount }).subscribe({
      next: () => {
        this.adding.set(false);
        this.createForm.reset({ categoryId: '', amount: 0 });
        this.load();
      },
      error: (response: HttpErrorResponse) => this.error.set(this.extractErrorMessage(response, 'Could not create this budget.'))
    });
  }

  startEdit(budget: Budget): void {
    this.editingId.set(budget.id);
    this.editAmount.setValue(budget.budgetedAmount);
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(budget: Budget): void {
    if (this.editAmount.invalid) {
      this.editAmount.markAsTouched();
      return;
    }

    this.savingEdit.set(true);
    this.error.set(null);

    this.budgetService.update({ id: budget.id, amount: this.editAmount.getRawValue() }).subscribe({
      next: () => {
        this.savingEdit.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => {
        this.savingEdit.set(false);
        this.error.set('Could not update this budget.');
      }
    });
  }

  deleteBudget(budget: Budget): void {
    if (!confirm(`Delete the budget for "${budget.categoryName}"?`)) {
      return;
    }

    this.budgetService.delete(budget.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not delete this budget.')
    });
  }

  private load(): void {
    const { year, month } = this.parseSelectedMonth();

    this.loading.set(true);
    this.error.set(null);

    this.budgetService.getBudgets(year, month).subscribe({
      next: (budgets) => {
        this.budgets.set(budgets);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load budgets. Are you signed in?');
        this.loading.set(false);
      }
    });
  }

  private parseSelectedMonth(): { year: number; month: number } {
    const [year, month] = this.monthControl.getRawValue().split('-').map(Number);
    return { year, month };
  }

  private currentMonth(): string {
    return new Date().toISOString().slice(0, 7);
  }

  private extractErrorMessage(response: HttpErrorResponse, fallback: string): string {
    const errors = response.error?.errors as Record<string, string[]> | undefined;
    const firstMessage = errors ? Object.values(errors)[0]?.[0] : undefined;
    return firstMessage ?? fallback;
  }
}
