import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Account, CreateAccountRequest, UpdateAccountRequest } from '../models/account.model';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly baseUrl = `${environment.apiUrl}/Accounts`;

  constructor(private readonly http: HttpClient) {}

  getAccounts(includeArchived = false): Observable<Account[]> {
    return this.http.get<Account[]>(this.baseUrl, { params: { includeArchived } });
  }

  getById(id: string): Observable<Account> {
    return this.http.get<Account>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateAccountRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  update(request: UpdateAccountRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${request.id}`, request);
  }

  archive(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
