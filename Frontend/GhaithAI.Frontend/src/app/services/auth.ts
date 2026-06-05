import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegisterRequest } from '../models/auth/register-request.model';
import { AuthResponse } from '../models/auth/auth-response.model';
import { GoogleLoginRequest } from '../models/auth/google-login-request.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly baseUrl = 'https://localhost:53898/api/Auth';

  constructor(private http: HttpClient) {}

  // ─── Register ───────────────────────────────────────
  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/register`,
      payload
    );
  }

  // ─── Login ──────────────────────────────────────────
  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(
    `${this.baseUrl}/login`,
    credentials
  );
}

  // ─── Google Login ────────────────────────────────────
  googleLogin(idToken: string): Observable<AuthResponse> {
    const payload: GoogleLoginRequest = { idToken };
    return this.http.post<AuthResponse>(
      `${this.baseUrl}/google-login`,
      payload
    );
  }

  // ─── Token Helpers ───────────────────────────────────
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
