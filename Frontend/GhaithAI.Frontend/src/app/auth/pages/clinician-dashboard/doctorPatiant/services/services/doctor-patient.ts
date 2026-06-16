import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DoctorPatientsDashboardDto } from '../../interface/doctor-patient.model';
@Injectable({
  providedIn: 'root',
})
export class DoctorPatient {
  private baseUrl = 'https://localhost:53898/api/DoctorDashboardPatiant';

  constructor(private http: HttpClient) { }

  getDashboardData(
    search?: string,
    riskFilter?: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<DoctorPatientsDashboardDto> {

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (search) params = params.set('search', search);
    if (riskFilter) params = params.set('riskFilter', riskFilter);

    return this.http.get<DoctorPatientsDashboardDto>(`${this.baseUrl}/patients-dashboard`, { params });
  }
}

