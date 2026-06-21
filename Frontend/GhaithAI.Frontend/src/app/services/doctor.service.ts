import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PublicDoctorCard {
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
  availableSessionType: string;  // 'Online' | 'InPerson' | 'Both'
  specialties: string[];
  languages: string[];
  nextAvailableSlot: string;
}

export interface ProfessionalsResponse {
  items: PublicDoctorCard[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface PublicDoctorProfile {
  weeklySchedule: {
    id: string;
    dayOfWeek: string;
    startTime: string;
    endTime: string;
    isActive: boolean;
  }[];
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
  availableSessionType: string;
  specialties: string[];
  languages: string[];
  nextAvailableSlot: string;
}

export interface AvailableSlot {
  date: string;
  startTime: string;
  endTime: string;
}

export interface UserBooking {
  bookingId: string;
  doctorId: string;
  doctorName: string;
  doctorSpecialization: string;
  bookingDate: string;
  slotTime: string;
  displayTime: string;
  sessionType: string;
  status: string;
  notes: string;
  canCancel: boolean;
}

export interface DoctorFilters {
  search?: string;
  specialty?: string;
  language?: string;
  sessionType?: string;
  city?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class DoctorService {

  private readonly baseUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) { }

  // ─── GET /api/professionals ───────────────────────
  getDoctors(filters?: DoctorFilters): Observable<ProfessionalsResponse> {
    let params = new HttpParams();
    if (filters?.search) params = params.set('search', filters.search);
    if (filters?.specialty) params = params.set('specialty', filters.specialty);
    if (filters?.language) params = params.set('language', filters.language);
    if (filters?.sessionType) params = params.set('sessionType', filters.sessionType);
    if (filters?.city) params = params.set('city', filters.city);
    params = params.set('page', filters?.page ?? 1);
    params = params.set('pageSize', filters?.pageSize ?? 10);

    return this.http.get<ProfessionalsResponse>(
      `${this.baseUrl}/professionals`,
      { params }
    );
  }

  // ─── GET /api/professionals/{doctorId} ────────────
  getDoctorProfile(doctorId: string): Observable<PublicDoctorProfile> {
    return this.http.get<PublicDoctorProfile>(
      `${this.baseUrl}/professionals/${doctorId}`
    );
  }

  // ─── Get Available Slots ──────────────────────────
  getAvailableSlots(doctorId: string, date: string): Observable<AvailableSlot[]> {
    // Build a from/to range covering the full selected day
    const from = `${date}T00:00:00`;
    const to   = `${date}T23:59:59`;
    return this.http.get<AvailableSlot[]>(
      `${this.baseUrl}/professionals/${doctorId}/available-slots`,
      { params: { from, to } }
    );
  }

  // ─── Book Doctor ──────────────────────────────────
  bookDoctor(payload: {
    doctorId: string;
    bookingDate: string;
    slotTime: string;
    sessionType: number;
    bookingNotes: string;
  }): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(
      `${this.baseUrl}/Booking/book`,
      payload
    );
  }

  // ─── Get My Bookings ──────────────────────────────
  getMyBookings(timeFilter = 'all', pageIndex = 0, pageSize = 10): Observable<UserBooking[]> {
    return this.http.get<UserBooking[]>(
      `${this.baseUrl}/Booking/my-bookings`,
      { params: { timeFilter, pageIndex, pageSize } }
    );
  }

  // ─── Cancel Booking ───────────────────────────────
  cancelBooking(bookingId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/Booking/${bookingId}/cancel`,
      {}
    );
  }
}
