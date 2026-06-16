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
  city: string;
  countryCode: string;
  bio: string;
  feePerSession: number;
  availableSessionType: number;
  specialties: string[];
  languages: string[];
  nextAvailableSlot: string;
}

export interface PublicDoctorProfile extends PublicDoctorCard {
  weeklySchedule: {
    dayOfWeek: string;
    startTime: string;
    endTime: string;
  }[];
}

export interface AvailableSlot {
  slotTime: string;
  displayTime: string;
  isAvailable: boolean;
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

@Injectable({ providedIn: 'root' })
export class DoctorService {

  private readonly baseUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  // ─── Get All Doctors ──────────────────────────────
  getDoctors(filters?: {
    searchTerm?: string;
    specialty?: string;
    language?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<PublicDoctorCard[]> {
    let params = new HttpParams();
    if (filters?.searchTerm) params = params.set('searchTerm', filters.searchTerm);
    if (filters?.specialty) params = params.set('specialty', filters.specialty);
    if (filters?.language) params = params.set('language', filters.language);
    if (filters?.pageNumber) params = params.set('pageNumber', filters.pageNumber);
    if (filters?.pageSize) params = params.set('pageSize', filters.pageSize ?? 10);

    return this.http.get<PublicDoctorCard[]>(
      `${this.baseUrl}/DoctorClinicProfile/professionals`,
      { params }
    );
  }

  // ─── Get Doctor Profile ───────────────────────────
  getDoctorProfile(doctorId: string): Observable<PublicDoctorProfile> {
    return this.http.get<PublicDoctorProfile>(
      `${this.baseUrl}/DoctorClinicProfile/professionals/${doctorId}`
    );
  }

  // ─── Get Available Slots ──────────────────────────
  getAvailableSlots(doctorId: string, date: string): Observable<AvailableSlot[]> {
    return this.http.get<AvailableSlot[]>(
      `${this.baseUrl}/Booking/${doctorId}/available-slots`,
      { params: { date } }
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
