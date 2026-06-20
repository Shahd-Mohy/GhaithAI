import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPasswordComponent implements OnInit {

  email = '';
  token = '';

  loading = false;
  successMessage = '';
  errorMessage = '';
  submitted = false;
  showPassword = false;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    this.form = this.fb.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*\d)(?=.*[!@#$%^&*(),.?":{}|<>_\-])/)
      ]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordsMatch });
  }

  passwordsMatch(group: FormGroup) {
    const pass = group.get('newPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return pass === confirm ? null : { mismatch: true };
  }

  get passCtrl() { return this.form.get('newPassword')!; }
  get confirmCtrl() { return this.form.get('confirmPassword')!; }

  get passError(): string {
    const c = this.passCtrl;
    if (!c.touched) return '';
    if (c.hasError('required')) return 'Password is required';
    if (c.hasError('minlength')) return 'Password must be at least 8 characters';
    if (c.hasError('pattern')) return 'Password must contain a number and a symbol';
    return '';
  }

  get confirmError(): string {
    const c = this.confirmCtrl;
    if (!c.touched) return '';
    if (c.hasError('required')) return 'Please confirm your password';
    if (this.form.hasError('mismatch')) return 'Passwords do not match';
    return '';
  }

  togglePassword(): void { this.showPassword = !this.showPassword; }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    if (!this.email || !this.token) {
      this.errorMessage = 'Invalid reset link. Please request a new one.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.auth.resetPassword(
      this.email,
      this.token,
      this.form.value.newPassword
    ).subscribe({
      next: () => {
        this.loading = false;
        this.submitted = true;
        this.successMessage = 'Password changed successfully!';
        setTimeout(() => this.router.navigate(['/login']), 2500);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message ?? 'Reset failed. The link may have expired.';
      }
    });
  }
}