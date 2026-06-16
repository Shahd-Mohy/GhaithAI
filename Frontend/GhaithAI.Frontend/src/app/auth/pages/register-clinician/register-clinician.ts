import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-register-clinician',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './register-clinician.html',
  styleUrls: ['./register-clinician.css']
})
export class RegisterClinicianComponent {

  // ─── Step control ─────────────────────────────────────
  currentStep = 1;

  // ─── Step 1: Basic Info ───────────────────────────────
  fullName = '';
  email = '';
  phone = '';
  password = '';
  showPassword = false;
  gender = '';
  preferredLanguage = 'en';

  // ─── Step 2: Professional Info ────────────────────────
  doctorType = '';
  specialization = '';
  bio = '';
  yearsOfExperience: number | null = null;

  // ─── Step 3: Documents ────────────────────────────────
  documentsPdf: File | null = null;
  fileName = '';
  termsAccepted = false;

  // ─── State ────────────────────────────────────────────
  errors: Record<string, boolean> = {};
  errorMessages: Record<string, string> = {};
  submitting = false;
  submitted = false;
  apiError = '';

  // ─── Options ──────────────────────────────────────────
  doctorTypes = [
    { label: 'Psychiatrist', value: 0 },
    { label: 'Psychologist', value: 1 },
    { label: 'Therapist / Counselor', value: 2 },
    { label: 'Social Worker', value: 3 },
    { label: 'Other', value: 4 }
  ];

  specializations = [
    'Anxiety & Stress',
    'Depression',
    'Trauma & PTSD',
    'Addiction & Recovery',
    'Child & Adolescent',
    'Couples & Family',
    'OCD',
    'Eating Disorders',
    'Grief & Loss',
    'Other'
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  // ─── Helpers ──────────────────────────────────────────
  isStepActive(step: number) { return this.currentStep === step; }
  isStepDone(step: number) { return step < this.currentStep; }
  goToStep(step: number) { this.currentStep = step; }
  togglePassword() { this.showPassword = !this.showPassword; }

  // ─── File Upload ──────────────────────────────────────
  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.type !== 'application/pdf') {
        this.errors['pdf'] = true;
        this.errorMessages['pdf'] = 'Only PDF files are accepted';
        return;
      }
      if (file.size > 10 * 1024 * 1024) {
        this.errors['pdf'] = true;
        this.errorMessages['pdf'] = 'File size must be less than 10MB';
        return;
      }
      this.documentsPdf = file;
      this.fileName = file.name;
      this.errors['pdf'] = false;
    }
  }

  removePdf(): void {
    this.documentsPdf = null;
    this.fileName = '';
  }

  // ─── Blur Validation ──────────────────────────────────
  validateField(field: string): void {
    switch (field) {

      case 'fullName':
        if (!this.fullName.trim()) {
          this.errors['fullName'] = true;
          this.errorMessages['fullName'] = 'Full name is required';
        } else {
          this.errors['fullName'] = false;
        }
        break;

      case 'email':
        if (!this.email.trim()) {
          this.errors['email'] = true;
          this.errorMessages['email'] = 'Email is required';
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim())) {
          this.errors['email'] = true;
          this.errorMessages['email'] = 'Enter a valid email address';
        } else {
          this.errors['email'] = false;
        }
        break;

      case 'phone':
        const phoneClean = this.phone.trim().replace(/\s/g, '');
        if (!phoneClean) {
          this.errors['phone'] = true;
          this.errorMessages['phone'] = 'Phone number is required';
        } else if (!/^01[0-9]{9}$/.test(phoneClean)) {
          this.errors['phone'] = true;
          this.errorMessages['phone'] = 'Phone must start with 01 and be exactly 11 digits';
        } else {
          this.errors['phone'] = false;
        }
        break;

      case 'password':
        if (this.password.length < 8) {
          this.errors['password'] = true;
          this.errorMessages['password'] = 'Password must be at least 8 characters';
        } else if (!/\d/.test(this.password)) {
          this.errors['password'] = true;
          this.errorMessages['password'] = 'Password must contain at least one number';
        } else if (!/[!@#$%^&*(),.?":{}|<>_\-]/.test(this.password)) {
          this.errors['password'] = true;
          this.errorMessages['password'] = 'Password must contain at least one symbol';
        } else {
          this.errors['password'] = false;
        }
        break;

      case 'bio':
        if (!this.bio.trim()) {
          this.errors['bio'] = true;
          this.errorMessages['bio'] = 'Bio is required';
        } else if (this.bio.trim().length < 50) {
          this.errors['bio'] = true;
          this.errorMessages['bio'] = 'Bio must be at least 50 characters';
        } else {
          this.errors['bio'] = false;
        }
        break;

      case 'yearsOfExperience':
        if (this.yearsOfExperience === null) {
          this.errors['yearsOfExperience'] = true;
          this.errorMessages['yearsOfExperience'] = 'Years of experience is required';
        } else if (this.yearsOfExperience < 0 || this.yearsOfExperience > 60) {
          this.errors['yearsOfExperience'] = true;
          this.errorMessages['yearsOfExperience'] = 'Enter a valid number of years';
        } else {
          this.errors['yearsOfExperience'] = false;
        }
        break;
    }
  }

  // ─── Step 1 Validation ────────────────────────────────
  step1Next(): void {
    this.errors = {};
    this.errorMessages = {};
    let valid = true;

    if (!this.fullName.trim()) {
      this.errors['fullName'] = true;
      this.errorMessages['fullName'] = 'Full name is required';
      valid = false;
    }

    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim());
    if (!emailValid) {
      this.errors['email'] = true;
      this.errorMessages['email'] = 'Enter a valid email address';
      valid = false;
    }

    const phoneClean = this.phone.trim().replace(/\s/g, '');
    if (!/^01[0-9]{9}$/.test(phoneClean)) {
      this.errors['phone'] = true;
      this.errorMessages['phone'] = 'Phone must start with 01 and be exactly 11 digits';
      valid = false;
    }

    if (this.password.length < 8 || !/\d/.test(this.password) || !/[!@#$%^&*(),.?":{}|<>_\-]/.test(this.password)) {
      this.errors['password'] = true;
      this.errorMessages['password'] = 'Password must be at least 8 chars with a number and symbol';
      valid = false;
    }

    if (!this.gender) {
      this.errors['gender'] = true;
      this.errorMessages['gender'] = 'Please select your gender';
      valid = false;
    }

    if (valid) this.goToStep(2);
  }

  // ─── Step 2 Validation ────────────────────────────────
  step2Next(): void {
    this.errors = {};
    this.errorMessages = {};
    let valid = true;

    if (!this.doctorType) {
      this.errors['doctorType'] = true;
      this.errorMessages['doctorType'] = 'Please select your doctor type';
      valid = false;
    }

    if (!this.specialization) {
      this.errors['specialization'] = true;
      this.errorMessages['specialization'] = 'Please select your specialization';
      valid = false;
    }

    if (!this.bio.trim() || this.bio.trim().length < 50) {
      this.errors['bio'] = true;
      this.errorMessages['bio'] = 'Bio must be at least 50 characters';
      valid = false;
    }

    if (this.yearsOfExperience === null || this.yearsOfExperience < 0 || this.yearsOfExperience > 60) {
      this.errors['yearsOfExperience'] = true;
      this.errorMessages['yearsOfExperience'] = 'Enter a valid number of years';
      valid = false;
    }

    if (valid) this.goToStep(3);
  }

  // ─── Submit ───────────────────────────────────────────
  submitForm(): void {
    this.errors = {};
    this.apiError = '';

    if (!this.documentsPdf) {
      this.errors['pdf'] = true;
      this.errorMessages['pdf'] = 'Please upload your credentials PDF';
      return;
    }

    if (!this.termsAccepted) {
      this.errors['terms'] = true;
      return;
    }

    const payload = {
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      phoneNumber: this.phone,
      countryCode: 'EG',
      preferredLanguage: this.preferredLanguage,
      gender: Number(this.gender),
      doctorType: Number(this.doctorType),
      specialization: this.specialization,
      bio: this.bio,
      yearsOfExperience: this.yearsOfExperience!,
      documentsPdf: this.documentsPdf
    };

    this.submitting = true;

    this.authService.registerClinician(payload).subscribe({

      next: () => {
        this.submitting = false;
        this.submitted = true;
      },

      error: (err) => {
        console.error(err);
        this.submitting = false;
        const message = err.error?.message || '';

        if (message.toLowerCase().includes('email')) {
          this.errors['email'] = true;
          this.errorMessages['email'] = 'This email is already registered';
          this.goToStep(1);
          return;
        }

        if (err.status === 400) {
          this.apiError = message || 'Please check your data and try again.';
        } else if (err.status === 500) {
          this.apiError = 'Something went wrong. Please try again later.';
        } else {
          this.apiError = 'Registration failed. Please try again.';
        }
      }
    });
  }
}
