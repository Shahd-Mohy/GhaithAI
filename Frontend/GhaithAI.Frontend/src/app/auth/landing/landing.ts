import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';
import { CommonModule } from '@angular/common';
import { NgIf } from '@angular/common'; // ← ضيف ده

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIf], // ← ضيف NgIf هنا
  templateUrl: './landing.html',
})
export class LandingComponent implements OnInit {
  isLoggedIn = false;
  dashboardRoute = '/dashboard';

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
  this.isLoggedIn = this.auth.isLoggedIn();

  if (this.isLoggedIn) {
    const user = this.auth.getUser();
    let role = user?.role?.toLowerCase();

    // fallback: اقرأ الـ role من الـ JWT لو user مش موجود
    if (!role) {
      const token = this.auth.getToken();
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          // الـ role claim في الـ JWT بتاعك
          const roleKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
          role = payload[roleKey]?.toLowerCase();
        } catch (e) {
          console.error('Failed to parse token', e);
        }
      }
    }

    console.log('✅ role:', role);

    if (role === 'admin') {
      this.dashboardRoute = '/admin';
    } else if (role === 'clinician' || role === 'doctor') {
      this.dashboardRoute = '/clinician-dashboard';
    } else {
      this.dashboardRoute = '/dashboard';
    }
  }
}

  goToDashboard() {
    this.router.navigate([this.dashboardRoute]);
  }
}
