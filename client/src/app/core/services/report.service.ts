import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CategorySpend, MonthlySummary } from '../models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly baseUrl = `${environment.apiUrl}/Reports`;

  constructor(private readonly http: HttpClient) {}

  getMonthlySummary(year: number, month: number): Observable<MonthlySummary> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<MonthlySummary>(`${this.baseUrl}/monthly-summary`, { params });
  }

  getSpendByCategory(year: number, month: number): Observable<CategorySpend[]> {
    const params = new HttpParams().set('year', year).set('month', month);
    return this.http.get<CategorySpend[]>(`${this.baseUrl}/spend-by-category`, { params });
  }
}
