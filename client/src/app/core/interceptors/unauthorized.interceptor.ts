import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const AUTH_ENDPOINTS = ['/Auth/login', '/Auth/register', '/Auth/refresh', '/Auth/forgot-password', '/Auth/reset-password'];
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';

let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

function withToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const isAuthEndpoint = AUTH_ENDPOINTS.some((endpoint) => req.url.includes(endpoint));

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error);
      }

      const refreshTokenAtFailure = localStorage.getItem(REFRESH_TOKEN_KEY);
      if (!refreshTokenAtFailure) {
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      if (isRefreshing) {
        return refreshedToken$.pipe(
          filter((token): token is string => token !== null),
          take(1),
          switchMap((token) => next(withToken(req, token)))
        );
      }

      isRefreshing = true;
      refreshedToken$.next(null);

      return authService.refreshAccessToken().pipe(
        switchMap((result) => {
          isRefreshing = false;
          refreshedToken$.next(result.token);
          return next(withToken(req, result.token));
        }),
        catchError((refreshError) => {
          isRefreshing = false;

          // A concurrent request may have already refreshed and rotated the token while
          // this attempt was in flight - if so, use the new session instead of forcing a
          // logout on what's actually still a valid, already-refreshed session.
          const currentRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
          const currentAccessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
          if (currentAccessToken && currentRefreshToken && currentRefreshToken !== refreshTokenAtFailure) {
            refreshedToken$.next(currentAccessToken);
            return next(withToken(req, currentAccessToken));
          }

          authService.logout();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
