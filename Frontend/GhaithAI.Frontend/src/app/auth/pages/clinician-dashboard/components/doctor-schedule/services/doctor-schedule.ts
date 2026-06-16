import { Injectable } from '@angular/core';
import { environment } from '../../../../../../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DoctorBookingResponseDto, TimeFilterType } from '../interface/DoctorBookingResponseDto';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class DoctorScheduleService {
  private readonly apiUrl = `${environment.apiUrl}/Booking`;

  constructor(private http: HttpClient) { }

  getDoctorBookingsPaged(
    timeFilter: TimeFilterType = 'all',
    pageIndex: number = 0,
    pageSize: number = 10
  ): Observable<DoctorBookingResponseDto[]> {
    let params = new HttpParams()
      .set('timeFilter', timeFilter)
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<DoctorBookingResponseDto[]>(`${this.apiUrl}/my-bookings-paged`, { params });
  }
}

