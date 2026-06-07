import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth';
import { RegisterRequest } from '../../../models/auth/register-request.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent implements OnInit {

  currentStep = 1;
  accountType = 'individual';

  firstName = '';
  lastName = '';
  email = '';
  emailExists = false;
  emailChecking = false;
  phone = '';
  password = '';
  showPassword = false;
  termsAccepted = false;

  errors: Record<string, boolean> = {};
  errorMessages: Record<string, string> = {};

  ec1name = '';
  ec1phone = '';
  ec1rel = '';

  ec2name = '';
  ec2phone = '';
  ec2rel = '';
  ecError = false;

  // ✅ flag لإظهار/إخفاء Contact 2
  showSecondContact = false;

  age: number | null = null;
  lang = 'en';
  selectedConcerns: string[] = [];
  sleep = '';
  stress = '';
  therapy = '';
  medications = '';

  submitting = false;
  submitted = false;
  apiError = '';

  concerns = [
    'Anxiety', 'Depression', 'Stress', 'Sleep Issues',
    'Relationship Problems', 'Work/School Pressure',
    'Grief/Loss', 'Self-Esteem', 'Anger Management', 'Other'
  ];

  relationships = [
    'Parent', 'Spouse / Partner', 'Sibling',
    'Friend', 'Colleague', 'Other'
  ];

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.accountType = params['type'] || 'individual';
    });
  }

  get typeBadgeLabel(): string {
    return this.accountType === 'clinician'
      ? 'Clinician Account'
      : 'Personal Support Account';
  }

  goToStep(step: number): void { this.currentStep = step; }
  isStepActive(step: number) { return this.currentStep === step; }
  isStepDone(step: number) { return step < this.currentStep; }
  isStepPending(step: number) { return step > this.currentStep; }
  togglePassword(): void { this.showPassword = !this.showPassword; }

  toggleConcern(concern: string): void {
    const index = this.selectedConcerns.indexOf(concern);
    if (index > -1) this.selectedConcerns.splice(index, 1);
    else this.selectedConcerns.push(concern);
  }

  isConcernSelected(concern: string): boolean {
    return this.selectedConcerns.includes(concern);
  }

  // ✅ toggle الـ second contact
  toggleSecondContact(): void {
    this.showSecondContact = !this.showSecondContact;
    if (!this.showSecondContact) {
      // لو أخفاه يمسح البيانات
      this.ec2name = '';
      this.ec2phone = '';
      this.ec2rel = '';
    }
  }

  // ─── Step 1 Validation ────────────────────────────────
  step1Next(): void {

    this.errors = {};
    this.errorMessages = {};
    this.emailExists = false;

    let valid = true;

    if (!this.firstName.trim()) {
      this.errors['firstName'] = true;
      this.errorMessages['firstName'] = 'First name is required';
      valid = false;
    }

    if (!this.lastName.trim()) {
      this.errors['lastName'] = true;
      this.errorMessages['lastName'] = 'Last name is required';
      valid = false;
    }

    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim());
    if (!emailValid) {
      this.errors['email'] = true;
      this.errorMessages['email'] = 'Enter a valid email address';
      valid = false;
    }

    const phoneClean = this.phone.trim().replace(/\s/g, '');
if (!phoneClean) {
  this.errors['phone'] = true;
  this.errorMessages['phone'] = 'Phone number is required';
  valid = false;
} else if (!/^01[0-9]{9}$/.test(phoneClean)) {
  this.errors['phone'] = true;
  this.errorMessages['phone'] = 'Phone number must start with 01 and be exactly 11 digits';
  valid = false;
}

    if (this.password.length < 8) {
      this.errors['password'] = true;
      this.errorMessages['password'] = 'Password must be at least 8 characters';
      valid = false;
    } else if (!/\d/.test(this.password)) {
      this.errors['password'] = true;
      this.errorMessages['password'] = 'Password must contain at least one number';
      valid = false;
    } else if (!/[!@#$%^&*(),.?":{}|<>_\-]/.test(this.password)) {
      this.errors['password'] = true;
      this.errorMessages['password'] = 'Password must contain at least one symbol';
      valid = false;
    }

    if (!this.termsAccepted) {
      this.errors['terms'] = true;
      valid = false;
    }

    if (valid) this.goToStep(2);
  }

  // ─── Step 2 Validation ────────────────────────────────
  step2Next(): void {

    const phoneClean = this.ec1phone.trim().replace(/\s/g, '');

    // ✅ Contact 1 إجباري بس
    if (!this.ec1name.trim() || !phoneClean) {
      this.ecError = true;
      return;
    }

    // ✅ لو Contact 2 ظاهر، لازم يكون كامل
    if (this.showSecondContact) {
      const ec2phoneClean = this.ec2phone.trim().replace(/\s/g, '');
      if (!this.ec2name.trim() || !ec2phoneClean) {
        this.ecError = true;
        return;
      }
    }

    this.ecError = false;
    this.goToStep(3);
  }

  // ─── Step 3 Validation ────────────────────────────────
  step3Valid(): boolean {
    return (
      this.age !== null &&
      this.age >= 13 &&
      this.age <= 100 &&
      this.sleep !== '' &&
      this.stress !== ''
    );
  }

  // ─── Submit ───────────────────────────────────────────
  submitForm(): void {

    this.apiError = '';

    // ✅ secondContact بس لو showSecondContact وفيه بيانات
    const secondContact =
      this.showSecondContact &&
      this.ec2name.trim() &&
      this.ec2phone.trim()
        ? {
            fullName: this.ec2name,
            phoneNumber: this.ec2phone,
            relationship: this.ec2rel || 'Other'
          }
        : null;

    const payload: RegisterRequest = {
      fullName: `${this.firstName} ${this.lastName}`,
      email: this.email,
      password: this.password,
      phoneNumber: this.phone,
      countryCode: 'EG',
      preferredLanguage: this.lang,
      acceptedTerms: this.termsAccepted,
      acceptedPrivacyPolicy: true,
      acceptedAiChat: true,
      acceptedMoodTracking: true,
      acceptedDataCollection: true,
      firstContact: {
        fullName: this.ec1name,
        phoneNumber: this.ec1phone,
        relationship: this.ec1rel
      },
      secondContact,
      age: this.age ?? undefined,
      concerns: this.selectedConcerns,
      sleepQuality: this.sleep,
      stressLevel: this.stress,
      hasTherapyHistory: this.therapy === 'current' || this.therapy === 'past',
      takesMedication: this.medications === 'yes'
    };

    this.submitting = true;

    this.authService.register(payload).subscribe({

      next: (response) => {
        this.authService.saveSession(response);
        this.submitting = false;
        this.submitted = true;
        this.router.navigateByUrl('/dashboard');
      },

      error: (err) => {
        console.error(err);
        this.submitting = false;

        const message = err.error?.message || '';

        if (message.toLowerCase().includes('email')) {
          this.emailExists = true;
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