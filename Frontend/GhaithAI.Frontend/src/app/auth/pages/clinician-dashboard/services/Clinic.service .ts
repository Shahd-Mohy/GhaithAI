import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { AuthService } from '../../../../services/auth';
import { environment } from '../../../../../environments/environment';

export type SessionTypeEnum = 'Online' | 'Offline' | 'both';
export type DaysOfWeek = 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday';

export interface SpecialtyItem { id: string; name: string; }
export interface LanguageItem { id: string; name: string; } 

export interface DefaultScheduleDto {
    id?: string;
    dayOfWeek: DaysOfWeek;
    startTime: string;
    endTime: string;
    isActive: boolean;
}

export interface CustomScheduleDto {
    id: string;
    customDate: string;
    startTime: string;
    endTime: string;
    isOffDay: boolean;
}

export interface UpsertCustomScheduleDto {
    customDate: string;
    startTime: string;
    endTime: string;
    isOffDay: boolean;
}

export interface DoctorClinicProfileDto {
    practiceType: string;
    displayName: string;
    professionalTitle: string;
    yearsOfExperience: number;
    bio: string;
    clinicName: string | null;
    clinicAddress: string | null;
    city: string | null;
    countryCode: string | null;
    phone: string | null;
    contactEmail: string | null;
    isPublicListed: boolean;
    feePerSession: number;
    sessionDurationMinutes: number;
    availableSessionType: SessionTypeEnum;
    specialties: string[];
    specialtyIds: string[];
    languages: string[];
    languageIds: string[];
    weeklySchedule: DefaultScheduleDto[];
    customSchedules: CustomScheduleDto[];
}

export interface UpdateDoctorClinicProfileDto {
    practiceType: string;
    displayName: string;
    professionalTitle: string;
    yearsOfExperience: number;
    bio: string;
    clinicName: string;
    clinicAddress?: string | null;
    city?: string | null;
    countryCode?: string | null;
    phone: string;
    contactEmail: string;
    isPublicListed: boolean;
    feePerSession: number;
    sessionDurationMinutes: number;
    availableSessionType: SessionTypeEnum;
    specialtyIds: string[];
    languageIds: string[];
    weeklySchedule: DefaultScheduleDto[];
}

@Injectable({ providedIn: 'root' })
export class ClinicService {
    private readonly http = inject(HttpClient);
    private readonly authService = inject(AuthService);

    // environment.apiUrl already ends with /api.
    // It already ends with /api — do NOT add /api again in any path below
    private readonly base = environment.apiUrl;

    private authHeaders(): HttpHeaders {
        return new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    }

    // GET {environment.apiUrl}/BaseSpecialty/getAll/dropDown
    getAllSpecialties(): Observable<SpecialtyItem[]> {
        return this.http
            .get<{ id: string; specialtyName: string }[]>(
                `${this.base}/BaseSpecialty/getAll/dropDown`
            )
            .pipe(map(list => list.map(i => ({ id: i.id, name: i.specialtyName }))));
    }

    // GET {environment.apiUrl}/BaseLanguage/Dropdown
    getAllLanguages(): Observable<LanguageItem[]> {
        return this.http
            .get<{ id: string; languageName: string }[]>(
                `${this.base}/BaseLanguage/Dropdown`
            )
            .pipe(map(list => list.map(i => ({ id: i.id, name: i.languageName }))));
    }

    // GET {environment.apiUrl}/doctor/clinic/profile
    getProfile(): Observable<DoctorClinicProfileDto> {
        return this.http.get<DoctorClinicProfileDto>(
            `${this.base}/doctor/clinic/profile`,
            { headers: this.authHeaders() }
        );
    }

    // PUT {environment.apiUrl}/doctor/clinic/profile
    updateProfile(dto: UpdateDoctorClinicProfileDto): Observable<DoctorClinicProfileDto> {
        return this.http.put<DoctorClinicProfileDto>(
            `${this.base}/doctor/clinic/profile`,
            dto,
            { headers: this.authHeaders() }
        );
    }

    // PATCH {environment.apiUrl}/doctor/clinic/profile/public-listing
    setPublicListing(isPublicListed: boolean): Observable<DoctorClinicProfileDto> {
        return this.http.patch<DoctorClinicProfileDto>(
            `${this.base}/doctor/clinic/profile/public-listing`,
            { isPublicListed },
            { headers: this.authHeaders() }
        );
    }

    // GET {environment.apiUrl}/doctor/clinic/schedule/custom
    getCustomSchedules(from?: string, to?: string): Observable<CustomScheduleDto[]> {
        let params = new HttpParams();
        if (from) params = params.set('from', from);
        if (to) params = params.set('to', to);
        return this.http.get<CustomScheduleDto[]>(
            `${this.base}/doctor/clinic/schedule/custom`,
            { headers: this.authHeaders(), params }
        );
    }

    // POST {environment.apiUrl}/doctor/clinic/schedule/custom
    addCustomSchedule(dto: UpsertCustomScheduleDto): Observable<CustomScheduleDto> {
        return this.http.post<CustomScheduleDto>(
            `${this.base}/doctor/clinic/schedule/custom`,
            dto,
            { headers: this.authHeaders() }
        );
    }

    // PUT {environment.apiUrl}/doctor/clinic/schedule/custom/{id}
    updateCustomSchedule(id: string, dto: UpsertCustomScheduleDto): Observable<CustomScheduleDto> {
        return this.http.put<CustomScheduleDto>(
            `${this.base}/doctor/clinic/schedule/custom/${id}`,
            dto,
            { headers: this.authHeaders() }
        );
    }

    // DELETE {environment.apiUrl}/doctor/clinic/schedule/custom/{id}
    deleteCustomSchedule(id: string): Observable<void> {
        return this.http.delete<void>(
            `${this.base}/doctor/clinic/schedule/custom/${id}`,
            { headers: this.authHeaders() }
        );
    }
}
