import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterRequest } from '../models/auth/register-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';
import { GoogleLoginRequest } from '../models/auth/google-login-request.model';
import { environment } from '../../environments/environment';
import { RegisterClinicianRequest } from '../models/auth/register-clinician-request.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly baseUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) { }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, payload);
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, credentials);
  }

  googleLogin(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/google-login`, { idToken });
  }

  registerClinician(payload: any): Observable<AuthResponse> {
    const formData = new FormData();

    formData.append('fullName', payload.fullName);
    formData.append('email', payload.email);
    formData.append('password', payload.password);
    formData.append('phoneNumber', payload.phoneNumber);
    formData.append('countryCode', payload.countryCode);
    formData.append('preferredLanguage', payload.preferredLanguage);

    // ✅ Gender: Male=1, Female=2
    const genderMap: Record<number, string> = { 1: 'Male', 2: 'Female' };
    formData.append('gender', genderMap[payload.gender] ?? 'Male');

    // ✅ DoctorType: Psychiatrist=0, Therapist=1, Counselor=2
    const doctorTypeMap: Record<number, string> = {
      0: 'Psychiatrist',
      1: 'Therapist',
      2: 'Counselor'
    };
    formData.append('doctorType', doctorTypeMap[payload.doctorType] ?? 'Psychiatrist');

    formData.append('specialization', payload.specialization);
    formData.append('bio', payload.bio);
    formData.append('yearsOfExperience', payload.yearsOfExperience.toString());

    if (payload.documentsPdf) {
      formData.append('documentsPdf', payload.documentsPdf);
    }

    return this.http.post<AuthResponse>(
      `${this.baseUrl}/register-clinician`,
      formData
    );
  }

  saveSession(response: AuthResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response));
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUser(): AuthResponse | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }
}
