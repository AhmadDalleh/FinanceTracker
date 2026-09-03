import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Matches the backend's MoneyLimits.MaxAmount (Application/Common/Validation/MoneyLimits.cs) - keep in sync. */
export const MAX_MONEY_AMOUNT = 999_999_999_999.99;

export function atMostTwoDecimalPlaces(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }

  const decimalPart = String(value).split('.')[1];
  return !decimalPart || decimalPart.length <= 2 ? null : { tooManyDecimals: true };
}
