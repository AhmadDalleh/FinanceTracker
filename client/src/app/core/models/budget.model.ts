export interface Budget {
  id: string;
  categoryId: string;
  categoryName: string;
  /** ISO date string (yyyy-MM-dd) - always the first of the budgeted month. */
  month: string;
  budgetedAmount: number;
  actualSpent: number;
  /** Percentage (0-100+), not a fraction. */
  percentageUsed: number;
}

export interface CreateBudgetRequest {
  categoryId: string;
  year: number;
  month: number;
  amount: number;
}

export interface UpdateBudgetRequest {
  id: string;
  amount: number;
}
