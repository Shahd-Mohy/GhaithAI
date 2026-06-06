import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface MoodLogRequest {
  moodScore: number;
  emotionTags: string;
  stressLevel: number;    // required by backend — default 3
  sleepQuality: number;   // required by backend — default 3
  notes?: string;
  source?: string;
  loggedAt?: string;      // ISO UTC string
}

export interface UpdateMoodRequest {
  moodScore?: number;
  emotionTags?: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MoodService {
  private apiUrl = 'https://localhost:53898/api/Mood';

  constructor(private http: HttpClient) {}

  logMood(data: MoodLogRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/log`, data).pipe(
      tap(res => console.log('[MoodService] logMood response:', res))
    );
  }

  getCalendar(year: number, month: number): Observable<any> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
    return this.http.get(`${this.apiUrl}/calendar`, { params }).pipe(
      tap(res => console.log('[MoodService] calendar response:', res))
    );
  }

  getStatistics(period: string = 'month'): Observable<any> {
    const params = new HttpParams().set('period', period);
    return this.http.get(`${this.apiUrl}/statistics`, { params }).pipe(
      tap(res => console.log('[MoodService] statistics response:', res))
    );
  }

  updateMood(moodLogId: string, data: UpdateMoodRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${moodLogId}`, data).pipe(
      tap(res => console.log('[MoodService] updateMood response:', res))
    );
  }

  getHistory(page: number = 1, pageSize: number = 20, fromDate?: string, toDate?: string): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (fromDate) params = params.set('from', fromDate);
    if (toDate)   params = params.set('to', toDate);
    return this.http.get(`${this.apiUrl}/history`, { params }).pipe(
      tap(res => console.log('[MoodService] history response:', res))
    );
  }

  exportCsv(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, { responseType: 'blob' });
  }
}