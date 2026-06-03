import { Injectable } from '@angular/core';

import {
  HttpClient
} from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl =
    'https://localhost:53898/api';

  constructor(
    private http: HttpClient
  ) { }

  register(data: any) {

    return this.http.post(
      `${this.apiUrl}/Auth/register`,
      data
    );
  }

  login(data: any) {

    return this.http.post(
      `${this.apiUrl}/Auth/login`,
      data
    );
  }

  getProfile() {

    return this.http.get(
      `${this.apiUrl}/User/profile`
    );
  }

  updateProfile(data: any) {

    return this.http.put(
      `${this.apiUrl}/User/profile`,
      data
    );
  }

  logout(){
    localStorage.removeItem('token');
  }

  googleLogin(data:any){

  return this.http.post(

    `${this.apiUrl}/auth/google-login`,

    data
  );
}
}