import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, ForgotPasswordRequest, LoginRequest, RegisterRequest, ResetPasswordRequest } from '../models/auth.model';

const TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const EMAIL_KEY = 'auth_email';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/Auth`;

  private readonly email = signal<string | null>(localStorage.getItem(EMAIL_KEY));
  readonly isAuthenticated = computed(() => this.email() !== null);
  readonly currentEmail = this.email.asReadonly();

  constructor(private readonly http: HttpClient) {}

  register(request: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/register`, request).pipe(tap((result) => this.storeSession(result)));
  }

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/login`, request).pipe(tap((result) => this.storeSession(result)));
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reset-password`, request);
  }

  refreshAccessToken(): Observable<AuthResult> {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    return this.http
      .post<AuthResult>(`${this.baseUrl}/refresh`, { refreshToken })
      .pipe(tap((result) => this.storeSession(result)));
  }

  hasRefreshToken(): boolean {
    return localStorage.getItem(REFRESH_TOKEN_KEY) !== null;
  }

  logout(): void {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (refreshToken) {
      this.http
        .post(`${this.baseUrl}/logout`, { refreshToken })
        .pipe(catchError(() => of(void 0)))
        .subscribe();
    }

    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    this.email.set(null);
  }

  private storeSession(result: AuthResult): void {
    localStorage.setItem(TOKEN_KEY, result.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
    localStorage.setItem(EMAIL_KEY, result.email);
    this.email.set(result.email);
  }
}
