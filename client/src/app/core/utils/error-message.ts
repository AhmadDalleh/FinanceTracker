import { HttpErrorResponse } from '@angular/common/http';

/**
 * Surfaces a fault-specific message instead of one generic string for every error.
 * The backend's ExceptionHandlingMiddleware already gives each fault (not found,
 * forbidden, invalid credentials, validation, ...) its own `title` / `errors`
 * body, so this mostly just picks the right part of that body - `fallback` only
 * covers the rare case where the response has neither.
 */
export function extractErrorMessage(response: HttpErrorResponse, fallback: string): string {
  if (response.status === 0) {
    return 'Could not reach the server. Check your connection and try again.';
  }

  const errors = response.error?.errors as Record<string, string[]> | undefined;
  const firstValidationMessage = errors ? Object.values(errors)[0]?.[0] : undefined;
  if (firstValidationMessage) {
    return firstValidationMessage;
  }

  if (response.error?.title) {
    return response.error.title;
  }

  // A gateway/proxy error (nginx's reverse proxy in production, ng serve's dev
  // proxy locally) when only the API is unreachable, not the frontend itself -
  // it has no JSON body, so it can't be told apart from a real 5xx by title alone.
  if (response.status >= 500) {
    return 'Something went wrong on our end. Please try again in a moment.';
  }

  return fallback;
}
