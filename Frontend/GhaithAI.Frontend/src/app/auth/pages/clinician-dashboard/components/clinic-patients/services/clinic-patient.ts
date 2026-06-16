import { Injectable } from '@angular/core';
import { environment } from '../../../../../../../environments/environment';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CreateClinicPatientDto, DoctorClinicPatientListDto, UpdateClinicPatientDto } from '../interface/clinic-patient.model';

@Injectable({
  providedIn: 'root',
})
export class ClinicPatientService {
  private readonly apiUrl = `${environment.apiUrl}/ClinicPatient`;

  constructor(private http: HttpClient) { }

  getPatients(searchTerm?: string, pageNumber: number = 1, pageSize: number = 10): Observable<DoctorClinicPatientListDto[]> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm && searchTerm.trim() !== '') {
      params = params.set('searchTerm', searchTerm.trim());
    }

    return this.http.get<DoctorClinicPatientListDto[]>(`${this.apiUrl}/My_Patients`, { params });
  }

  // 2. إنشاء مريض جديد
  createPatient(dto: CreateClinicPatientDto): Observable<DoctorClinicPatientListDto> {
    return this.http.post<DoctorClinicPatientListDto>(`${this.apiUrl}/Create_Patient`, dto);
  }

  // 3. تعديل بيانات مريض
  updatePatient(dto: UpdateClinicPatientDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Update_Patient`, dto);
  }

  // 4. حذف مريض نهائياً من السيستم
  deletePatient(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/Delete_Patients/${id}`);
  }
}

