export enum AccountType {
  Checking = 0,
  Savings = 1,
  CreditCard = 2,
  Cash = 3,
  Investment = 4
}

export const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  [AccountType.Checking]: 'Checking',
  [AccountType.Savings]: 'Savings',
  [AccountType.CreditCard]: 'Credit Card',
  [AccountType.Cash]: 'Cash',
  [AccountType.Investment]: 'Investment'
};

export interface Account {
  id: string;
  name: string;
  type: AccountType;
  balance: number;
  currency: string;
  isArchived: boolean;
}

export interface CreateAccountRequest {
  name: string;
  type: AccountType;
  startingBalance: number;
  currency: string;
}

export interface UpdateAccountRequest {
  id: string;
  name: string;
  type: AccountType;
}
