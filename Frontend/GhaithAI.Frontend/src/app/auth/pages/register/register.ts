import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
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

  // Step 1
  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  showPassword = false;
  termsAccepted = false;

  errors: Record<string, boolean> = {};

  // Step 2
  ec1name = '';
  ec1phone = '';
  ec1rel = '';
  ec2name = '';
  ec2phone = '';
  ec2rel = '';
  ecError = false;

  // Step 3
  age: number | null = null;
  lang = 'en';
  selectedConcerns: string[] = [];
  sleep = '';
  stress = '';
  therapy = '';
  medications = '';
  submitting = false;
  submitted = false;

  concerns = [
    'Anxiety', 'Depression', 'Stress', 'Sleep Issues',
    'Relationship Problems', 'Work/School Pressure',
    'Grief/Loss', 'Self-Esteem', 'Anger Management', 'Other'
  ];

  relationships = ['Parent', 'Spouse / Partner', 'Sibling', 'Friend', 'Colleague', 'Other'];

  constructor (private route: ActivatedRoute,
  private authService: AuthService) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.accountType = params['type'] || 'individual';
    });
  }

  get typeBadgeLabel(): string {
    return this.accountType === 'clinician' ? 'Clinician Account' : 'Personal Support Account';
  }

  goToStep(n: number) {
    this.currentStep = n;
  }

  isStepActive(n: number)  { return n === this.currentStep; }
  isStepDone(n: number)    { return n < this.currentStep; }
  isStepPending(n: number) { return n > this.currentStep; }

  togglePassword() { this.showPassword = !this.showPassword; }

  toggleConcern(concern: string) {
    const idx = this.selectedConcerns.indexOf(concern);
    if (idx > -1) this.selectedConcerns.splice(idx, 1);
    else this.selectedConcerns.push(concern);
  }

  isConcernSelected(concern: string) {
    return this.selectedConcerns.includes(concern);
  }

  step1Next() {
    this.errors = {};
    let valid = true;

    if (!this.firstName.trim()) { this.errors['firstName'] = true; valid = false; }
    if (!this.lastName.trim())  { this.errors['lastName']  = true; valid = false; }

    const emailOk = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim());
    if (!emailOk) { this.errors['email'] = true; valid = false; }

    if (!this.phone.trim()) { this.errors['phone'] = true; valid = false; }

    const pwOk = this.password.length >= 8 &&
                 /\d/.test(this.password) &&
                 /[!@#$%^&*(),.?":{}|<>_\-]/.test(this.password);
    if (!pwOk) { this.errors['password'] = true; valid = false; }

    if (!this.termsAccepted) { this.errors['terms'] = true; valid = false; }

    if (valid) this.goToStep(2);
  }

  step2Next() {
    if (!this.ec1name.trim() || !this.ec1phone.trim()) {
      this.ecError = true;
      return;
    }
    this.ecError = false;
    this.goToStep(3);
  }

  submitForm() {

  const payload: RegisterRequest = {

    fullName:
      `${this.firstName} ${this.lastName}`,

    email: this.email,

    password: this.password,

    phoneNumber: this.phone,

    countryCode: 'EG',

    preferredLanguage: this.lang,

    acceptedTerms: true,

    acceptedPrivacyPolicy: true,

    acceptedAiChat: true,

    acceptedMoodTracking: true,

    acceptedDataCollection: true,

    firstContact: {
      fullName: this.ec1name,
      phoneNumber: this.ec1phone,
      relationship: this.ec1rel
    },

    secondContact: {
      fullName: this.ec2name,
      phoneNumber: this.ec2phone,
      relationship: this.ec2rel
    },

    age: this.age ?? undefined,

    concerns: this.selectedConcerns,

    sleepQuality: this.sleep,

    stressLevel: this.stress,

    hasTherapyHistory:
      this.therapy === 'yes',

    takesMedication:
      this.medications === 'yes'
  };

  this.submitting = true;

  this.authService
      .register(payload)
      .subscribe({

        next: (res) => {

          this.submitting = false;
          this.submitted = true;

          console.log(res);
        },

        error: (err) => {

          console.error(err);

          this.submitting = false;
        }
      });
}
}
