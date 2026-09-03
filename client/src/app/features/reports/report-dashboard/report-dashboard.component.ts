import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CategorySpend, MonthlySummary } from '../../../core/models/report.model';
import { ReportService } from '../../../core/services/report.service';
import { extractErrorMessage } from '../../../core/utils/error-message';

@Component({
  selector: 'app-report-dashboard',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './report-dashboard.component.html',
  styleUrl: './report-dashboard.component.scss'
})
export class ReportDashboardComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly reportService = inject(ReportService);

  readonly monthControl = this.fb.nonNullable.control(this.currentMonth());

  readonly summary = signal<MonthlySummary | null>(null);
  readonly categorySpend = signal<CategorySpend[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly maxCategorySpend = computed(() => Math.max(...this.categorySpend().map((c) => c.totalSpent), 0));

  ngOnInit(): void {
    this.load();
  }

  onMonthChange(): void {
    this.load();
  }

  barWidth(amount: number): number {
    const max = this.maxCategorySpend();
    return max === 0 ? 0 : Math.round((amount / max) * 100);
  }

  private load(): void {
    const { year, month } = this.parseSelectedMonth();

    this.loading.set(true);
    this.error.set(null);

    this.reportService.getMonthlySummary(year, month).subscribe({
      next: (summary) => this.summary.set(summary),
      error: (response: HttpErrorResponse) => this.error.set(extractErrorMessage(response, 'Could not load the monthly summary.'))
    });

    this.reportService.getSpendByCategory(year, month).subscribe({
      next: (categorySpend) => {
        this.categorySpend.set(categorySpend);
        this.loading.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(extractErrorMessage(response, 'Could not load spend by category.'));
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
}
