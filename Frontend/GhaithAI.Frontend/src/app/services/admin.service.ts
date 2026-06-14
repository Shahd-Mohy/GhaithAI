import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DoctorProfile {
  id: string;
  fullName: string;
  doctorType: string;        // ✅ string مش number
  specialization: string;
  bio: string;
  yearsOfExperience: number;
  documentsPdfUrl: string;
  approvalStatus: string;    // ✅ string مش number
  rejectionReason?: string;
  averageRating: number;
  email: string;             // ✅ مباشرة مش جوا user
  phoneNumber: string;       // ✅ مباشرة مش جوا user
  profilePicture?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminService {

  private readonly baseUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) { }

  getPendingDoctors(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>(`${this.baseUrl}/doctors/pending`);
  }

  getApprovedDoctors(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>(`${this.baseUrl}/doctors/approved`);
  }

  getRejectedDoctors(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>(`${this.baseUrl}/doctors/rejected`);
  }

  getDoctorDetails(id: string): Observable<DoctorProfile> {
    return this.http.get<DoctorProfile>(`${this.baseUrl}/doctors/${id}`);
  }

  approveDoctor(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/doctors/${id}/approve`, {});
  }

  rejectDoctor(id: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/doctors/${id}/reject`, { reason });
  }
}
