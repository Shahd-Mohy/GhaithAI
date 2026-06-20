import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPasswordComponent implements OnInit {

  loading = false;
  successMessage = '';
  errorMessage = '';
  submitted = false;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get emailCtrl() { return this.form.get('email')!; }

  get emailError(): string {
    const c = this.emailCtrl;
    if (!c.touched) return '';
    if (c.hasError('required')) return 'Email is required';
    if (c.hasError('email')) return 'Enter a valid email address';
    return '';
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.auth.forgotPassword(this.form.value.email).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.submitted = true;
        this.successMessage = res.message ?? 'If the email exists, a reset link has been sent.';
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message ?? 'Something went wrong. Please try again.';
      }
    });
  }
}