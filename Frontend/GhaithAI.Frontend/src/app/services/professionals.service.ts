import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

/* ──────────────────────────────────────────────────────────────
   Models
   ────────────────────────────────────────────────────────────── */

export interface ProfessionalCard {
  doctorId: string;
  displayName: string;
  professionalTitle: string;
  averageRating: number;
  reviewCount: number;
  yearsOfExperience: number;
  city: string | null;
  countryCode: string | null;
  bio: string;
  feePerSession: number;
  availableSessionType: string; // 'Online' | 'InPerson' | 'Both'
  specialties: string[];
  languages: string[];
  nextAvailableSlot: string;
}

export interface ProfessionalsResponse {
  items: ProfessionalCard[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ProfessionalProfile {
  weeklySchedule: {
    id: string;
    dayOfWeek: string;
    startTime: string;
    endTime: string;
    isActive: boolean;
  }[];
  // Fields that may also be returned from the profile endpoint
  doctorId?: string;
  displayName?: string;
  professionalTitle?: string;
  averageRating?: number;
  reviewCount?: number;
  yearsOfExperience?: number;
  city?: string | null;
  countryCode?: string | null;
  bio?: string;
  feePerSession?: number;
  availableSessionType?: string;
  specialties?: string[];
  languages?: string[];
  nextAvailableSlot?: string;
}

export interface ProfessionalsFilter {
  search?: string;
  specialty?: string;
  language?: string;
  sessionType?: string;
  city?: string;
  page?: number;
  pageSize?: number;
}

/* ──────────────────────────────────────────────────────────────
   Service
   ────────────────────────────────────────────────────────────── */

@Injectable({ providedIn: 'root' })
export class ProfessionalsService {

  private readonly base = `${environment.apiUrl}/professionals`;

  constructor(private http: HttpClient) { }

  /**
   * GET /api/professionals
   * Supports: search, specialty, language, sessionType, city, page, pageSize
   */
  getAll(filters?: ProfessionalsFilter): Observable<ProfessionalsResponse> {
    let params = new HttpParams()
      .set('page', filters?.page ?? 1)
      .set('pageSize', filters?.pageSize ?? 10);

    if (filters?.search) params = params.set('search', filters.search);
    if (filters?.specialty) params = params.set('specialty', filters.specialty);
    if (filters?.language) params = params.set('language', filters.language);
    if (filters?.sessionType) params = params.set('sessionType', filters.sessionType);
    if (filters?.city) params = params.set('city', filters.city);

    return this.http.get<ProfessionalsResponse>(this.base, { params });
  }

  /**
   * GET /api/professionals/{doctorId}
   * Returns the doctor's weekly schedule (and optionally full profile data)
   */
  getById(doctorId: string): Observable<ProfessionalProfile> {
    return this.http.get<ProfessionalProfile>(`${this.base}/${doctorId}`);
  }
}
