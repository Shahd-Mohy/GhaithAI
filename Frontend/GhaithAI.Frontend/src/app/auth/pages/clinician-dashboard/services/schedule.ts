import { Injectable } from '@angular/core';
import { environment } from '../../../../../environments/environment';
import { ScheduleItem } from '../interface/schedule.model';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ScheduleService {
  private apiUrl = `${environment.apiUrl}/Booking`;

  constructor(private http: HttpClient) { }

  getTodaySchedule(pageIndex: number = 0, pageSize: number = 10): Observable<ScheduleItem[]> {
    return this.http.get<ScheduleItem[]>(`${this.apiUrl}/schedule`);
  }
}
