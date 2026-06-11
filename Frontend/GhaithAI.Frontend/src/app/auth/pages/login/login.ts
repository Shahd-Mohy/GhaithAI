import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {

  loginForm: FormGroup;

  submitting = false;
  apiError = '';
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get emailCtrl() { return this.loginForm.get('email')!; }
  get passwordCtrl() { return this.loginForm.get('password')!; }

  get emailError(): string {
    const c = this.emailCtrl;
    if (!c.touched) return '';
    if (c.hasError('required')) return 'Email is required';
    if (c.hasError('email')) return 'Enter a valid email address';
    return '';
  }

  get passwordError(): string {
    const c = this.passwordCtrl;
    if (!c.touched) return '';
    if (c.hasError('required')) return 'Password is required';
    if (c.hasError('minlength')) return 'Password must be at least 6 characters';
    return '';
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  login(): void {
  this.loginForm.markAllAsTouched();
  if (this.loginForm.invalid) return;

  this.submitting = true;
  this.apiError = '';

  this.auth.login(this.loginForm.value).subscribe({

    next: (res) => {
      this.auth.saveSession(res);
      this.submitting = false;

      // ✅ توجيه حسب الـ role
      if (res.role === 'Admin') {
        this.router.navigate(['/admin']);
      } else if (res.role === 'Clinician') {
        this.router.navigate(['/doctor-dashboard']);
      } else {
        this.router.navigate(['/dashboard']);
      }
    },

    error: (err) => {
      this.submitting = false;
      const message = err.error?.message || '';

      if (err.status === 400 && message.toLowerCase().includes('google')) {
        this.apiError = 'This account uses Google Sign-In. Please login with Google.';
      } else if (err.status === 400 && message.toLowerCase().includes('pending')) {
        this.apiError = 'Your account is pending approval. Please wait for admin review.';
      } else if (err.status === 400 && message.toLowerCase().includes('rejected')) {
        this.apiError = message;
      } else if (err.status === 400) {
        this.apiError = message || 'Invalid email or password.';
      } else if (err.status === 500) {
        this.apiError = 'Something went wrong. Please try again later.';
      } else {
        this.apiError = 'Login failed. Please try again.';
      }
    }
  });
}
}

