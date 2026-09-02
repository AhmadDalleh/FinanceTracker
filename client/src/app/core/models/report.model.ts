export interface MonthlySummary {
  year: number;
  month: number;
  totalIncome: number;
  totalExpense: number;
  netPosition: number;
}

export interface CategorySpend {
  categoryId: string;
  categoryName: string;
  totalSpent: number;
}
