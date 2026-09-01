export enum TransactionType {
  Income = 0,
  Expense = 1
}

export const TRANSACTION_TYPE_LABELS: Record<TransactionType, string> = {
  [TransactionType.Income]: 'Income',
  [TransactionType.Expense]: 'Expense'
};

export interface Transaction {
  id: string;
  accountId: string;
  categoryId: string;
  amount: number;
  type: TransactionType;
  /** ISO date string (yyyy-MM-dd) - the backend models this as a date-only value. */
  date: string;
  note?: string | null;
}

export interface CreateTransactionRequest {
  accountId: string;
  categoryId: string;
  amount: number;
  type: TransactionType;
  date: string;
  note?: string | null;
}

export interface UpdateTransactionRequest {
  id: string;
  categoryId: string;
  amount: number;
  type: TransactionType;
  date: string;
  note?: string | null;
}

export interface TransactionFilter {
  accountId?: string;
  categoryId?: string;
  fromDate?: string;
  toDate?: string;
  minAmount?: number;
  maxAmount?: number;
  pageNumber?: number;
  pageSize?: number;
}
