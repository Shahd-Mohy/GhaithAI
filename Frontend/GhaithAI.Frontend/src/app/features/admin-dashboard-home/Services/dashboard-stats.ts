import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AdminDashboardStatsDto } from '../interfaces/dashboard-stats.interface';

@Injectable({
  providedIn: 'root',
})
export class DashboardStats {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Admin`;

  getStats(): Observable<{ success: boolean; data: AdminDashboardStatsDto }> {
    return this.http.get<{ success: boolean; data: AdminDashboardStatsDto }>(`${this.apiUrl}/Dashboard_Stats`);
  }
}
