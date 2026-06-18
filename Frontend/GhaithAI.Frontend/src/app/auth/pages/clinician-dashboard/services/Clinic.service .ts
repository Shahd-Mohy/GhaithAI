import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../../../../services/auth';
import { environment } from '../../../../../environments/environment';

export type SessionTypeEnum = 'Online' | 'Offline' | 'both';
export type DaysOfWeek = 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday';

export interface SpecialtyItem  { id: string; name: string; }
export interface LanguageItem   { id: string; name: string; }

export interface DefaultScheduleDto {
  id?:        string;
  dayOfWeek:  DaysOfWeek;
  startTime:  string;   // "09:00:00"
  endTime:    string;   // "17:00:00"
  isActive:   boolean;
}

export interface CustomScheduleDto {
  id:         string;
  customDate: string;   // ISO date string
  startTime:  string;
  endTime:    string;
  isOffDay:   boolean;
}

export interface UpsertCustomScheduleDto {
  customDate: string;
  startTime:  string;
  endTime:    string;
  isOffDay:   boolean;
}

export interface DoctorClinicProfileDto {
  practiceType:           string;
  displayName:            string;
  professionalTitle:      string;
  yearsOfExperience:      number;
  bio:                    string;
  clinicName:             string | null;
  clinicAddress:          string | null;
  city:                   string | null;
  countryCode:            string | null;
  phone:                  string | null;
  contactEmail:           string | null;
  isPublicListed:         boolean;
  feePerSession:          number;
  sessionDurationMinutes: number;
  availableSessionType:   SessionTypeEnum;
  specialties:            string[];
  specialtyIds:           string[];
  languages:              string[];
  languageIds:            string[];
  weeklySchedule:         DefaultScheduleDto[];
  customSchedules:        CustomScheduleDto[];
}

export interface UpdateDoctorClinicProfileDto {
  practiceType:           string;
  displayName:            string;
  professionalTitle:      string;
  yearsOfExperience:      number;
  bio:                    string;
  clinicName:             string;
  clinicAddress?:         string | null;
  city?:                  string | null;
  countryCode?:           string | null;
  phone:                  string;
  contactEmail:           string;
  isPublicListed:         boolean;
  feePerSession:          number;
  sessionDurationMinutes: number;
  availableSessionType:   SessionTypeEnum;
  specialtyIds:           string[];
  languageIds:            string[];
  weeklySchedule:         DefaultScheduleDto[];
}

@Injectable({ providedIn: 'root' })
export class ClinicService {
  private readonly http        = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly base        = environment.apiUrl;

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
  }

  // ── Lookups ──────────────────────────────────────────────────
  getAllSpecialties(): Observable<SpecialtyItem[]> {
    return this.http.get<SpecialtyItem[]>(
      `${this.base}/api/clinic/specialties`, { headers: this.headers() });
  }

  getAllLanguages(): Observable<LanguageItem[]> {
    return this.http.get<LanguageItem[]>(
      `${this.base}/api/clinic/languages`, { headers: this.headers() });
  }

  // ── Profile ──────────────────────────────────────────────────
  getProfile(): Observable<DoctorClinicProfileDto> {
    return this.http.get<DoctorClinicProfileDto>(
      `${this.base}/api/doctor/clinic/profile`, { headers: this.headers() });
  }

  updateProfile(dto: UpdateDoctorClinicProfileDto): Observable<DoctorClinicProfileDto> {
    return this.http.put<DoctorClinicProfileDto>(
      `${this.base}/api/doctor/clinic/profile`, dto, { headers: this.headers() });
  }

  setPublicListing(isPublicListed: boolean): Observable<DoctorClinicProfileDto> {
    return this.http.patch<DoctorClinicProfileDto>(
      `${this.base}/api/doctor/clinic/profile/public-listing`,
      { isPublicListed }, { headers: this.headers() });
  }

  // ── Custom Schedules ──────────────────────────────────────────
  getCustomSchedules(from?: string, to?: string): Observable<CustomScheduleDto[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to)   params = params.set('to', to);
    return this.http.get<CustomScheduleDto[]>(
      `${this.base}/api/doctor/clinic/schedule/custom`,
      { headers: this.headers(), params });
  }

  addCustomSchedule(dto: UpsertCustomScheduleDto): Observable<CustomScheduleDto> {
    return this.http.post<CustomScheduleDto>(
      `${this.base}/api/doctor/clinic/schedule/custom`, dto, { headers: this.headers() });
  }

  updateCustomSchedule(id: string, dto: UpsertCustomScheduleDto): Observable<CustomScheduleDto> {
    return this.http.put<CustomScheduleDto>(
      `${this.base}/api/doctor/clinic/schedule/custom/${id}`, dto, { headers: this.headers() });
  }

  deleteCustomSchedule(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.base}/api/doctor/clinic/schedule/custom/${id}`, { headers: this.headers() });
  }
}