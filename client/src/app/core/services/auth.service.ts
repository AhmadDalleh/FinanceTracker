import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, RegisterRequest } from '../models/auth.model';

const TOKEN_KEY = 'access_token';
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

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    this.email.set(null);
  }

  private storeSession(result: AuthResult): void {
    localStorage.setItem(TOKEN_KEY, result.token);
    localStorage.setItem(EMAIL_KEY, result.email);
    this.email.set(result.email);
  }
}
