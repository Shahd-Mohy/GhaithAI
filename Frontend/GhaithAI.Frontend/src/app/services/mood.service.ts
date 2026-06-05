import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface MoodLogRequest {
  moodScore: number;
  emotionTags: string;
  notes: string;
  // Optional properties that we skip for now based on user instruction
  stressLevel?: number;
  sleepQuality?: number;
  source?: string;
  loggedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MoodService {
  private apiUrl = 'https://localhost:53898/api/Mood';

  constructor(private http: HttpClient) {}

  logMood(data: MoodLogRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/log`, data);
  }

  getCalendar(year: number, month: number): Observable<any> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());
    return this.http.get(`${this.apiUrl}/calendar`, { params });
  }

  getStatistics(period: string = 'month'): Observable<any> {
    const params = new HttpParams().set('period', period);
    return this.http.get(`${this.apiUrl}/statistics`, { params });
  }

  updateMood(moodLogId: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${moodLogId}`, data);
  }

  getHistory(page: number = 1, pageSize: number = 20, fromDate?: string, toDate?: string): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
      
    if (fromDate) params = params.set('from', fromDate);
    if (toDate) params = params.set('to', toDate);

    return this.http.get(`${this.apiUrl}/history`, { params });
  }
}
