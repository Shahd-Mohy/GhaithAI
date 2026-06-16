import {
  Component, OnInit, OnDestroy, inject,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../services/auth';
import { AuthResponse } from '../../../../models/auth/auth-response.model';

interface DaySchedule {
  name: string;
  short: string;
  enabled: boolean;
  from: string;
  to: string;
}

interface SessionType {
  id: string;
  label: string;
  selected: boolean;
}

@Component({
  selector: 'app-my-clinic',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-clinic.html',
  styleUrls: ['../clinician-shared.css', './my-clinic.css']
})
export class MyClinicComponent implements OnInit, OnDestroy {

  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  // ── Practice Type ────────────────────────────────────────────────
  practiceType: 'online' | 'inperson' = 'inperson';

  // ── Profile / identity ─────────────────────────────────────────
  displayName = '';
  title = 'Psychiatrist';
  clinicName = 'Serenity Mental Health Clinic';
  yearsExp = '12';
  bio = 'Board-certified psychiatrist with expertise in mood disorders and comprehensive medication management. I provide a warm, evidence-based approach tailored to each patient.';

  // ── Contact ────────────────────────────────────────────────────
  address = '123 Wellness Avenue, Suite 400';
  city = 'Dubai';
  country = 'UAE';
  phone = '+971 50 123 4567';
  email = 'contact@serenityclinic.com';

  // ── Specialties & Languages ────────────────────────────────────
  specialties: string[] = ['Anxiety', 'Depression', 'CBT'];
  newSpecialty = '';

  languages: string[] = ['English', 'Arabic'];
  newLanguage = '';

  // ── Session Types ──────────────────────────────────────────────
  sessionTypes: SessionType[] = [
    { id: 'video', label: 'Video Call', selected: true },
    { id: 'phone', label: 'Phone Call', selected: false },
    { id: 'inperson', label: 'In-Person', selected: true }
  ];
  feePerSession = '180';
  sessionDuration = '50';
  currency = 'AED';

  // ── Weekly Availability ────────────────────────────────────────
  schedule: DaySchedule[] = [
    { name: 'Monday', short: 'Mon', enabled: true, from: '09:00', to: '17:00' },
    { name: 'Tuesday', short: 'Tue', enabled: true, from: '09:00', to: '17:00' },
    { name: 'Wednesday', short: 'Wed', enabled: true, from: '09:00', to: '17:00' },
    { name: 'Thursday', short: 'Thu', enabled: true, from: '09:00', to: '17:00' },
    { name: 'Friday', short: 'Fri', enabled: true, from: '09:00', to: '17:00' },
    { name: 'Saturday', short: 'Sat', enabled: false, from: '09:00', to: '13:00' },
    { name: 'Sunday', short: 'Sun', enabled: false, from: '09:00', to: '13:00' },
  ];

  // ── Public listing ─────────────────────────────────────────────
  publicListing = true;

  // ── UI state ───────────────────────────────────────────────────
  saveSuccess = false;
  isSaving = false;
  hasChanges = false;

  // Track original values to discard changes
  private originalData: any = {};

  ngOnInit(): void {
    const user = this.authService.getUser() as AuthResponse | null;
    if (user) {
      const fullName = user.fullName?.trim() || '';
      this.displayName = fullName || 'Dr. Sarah Ahmed';

      if (user.specialization?.trim()) this.title = user.specialization.trim();
      else if (user.doctorType?.trim()) this.title = user.doctorType.trim();

      if (user.email) this.email = user.email;
    }
    
    // Load from local storage if saved values exist
    this.loadProfile();
    this.captureOriginalState();
    this.cdr.detectChanges();
  }

  ngOnDestroy(): void { }

  // ── Set Practice Type ──────────────────────────────────────────
  setPracticeType(type: 'online' | 'inperson'): void {
    if (this.practiceType === type) return;
    this.practiceType = type;
    this.markChanged();

    // Sync session chip defaults based on practice mode
    if (type === 'online') {
      // Online: default to video/phone, deselect in-person
      const videoType = this.sessionTypes.find(s => s.id === 'video');
      if (videoType) videoType.selected = true;
      const inpersonType = this.sessionTypes.find(s => s.id === 'inperson');
      if (inpersonType) inpersonType.selected = false;
    } else {
      // In-person: keep selections as-is, ensure inperson is selected
      const inpersonType = this.sessionTypes.find(s => s.id === 'inperson');
      if (inpersonType) inpersonType.selected = true;
    }
  }

  // ── Specialties ────────────────────────────────────────────────
  addSpecialty(): void {
    const v = this.newSpecialty.trim();
    if (v && !this.specialties.includes(v)) {
      this.specialties = [...this.specialties, v];
      this.markChanged();
    }
    this.newSpecialty = '';
  }

  removeSpecialty(s: string): void {
    this.specialties = this.specialties.filter(x => x !== s);
    this.markChanged();
  }

  onSpecialtyKey(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      this.addSpecialty();
    }
  }

  // ── Languages ──────────────────────────────────────────────────
  addLanguage(): void {
    const v = this.newLanguage.trim();
    if (v && !this.languages.includes(v)) {
      this.languages = [...this.languages, v];
      this.markChanged();
    }
    this.newLanguage = '';
  }

  removeLanguage(l: string): void {
    this.languages = this.languages.filter(x => x !== l);
    this.markChanged();
  }

  onLanguageKey(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      this.addLanguage();
    }
  }

  // ── Session types ──────────────────────────────────────────────
  toggleSession(id: string): void {
    const st = this.sessionTypes.find(s => s.id === id);
    if (st) {
      st.selected = !st.selected;
      this.markChanged();
    }
  }

  // ── Change tracking ────────────────────────────────────────────
  markChanged(): void {
    this.hasChanges = true;
  }

  // ── Capture Original State ─────────────────────────────────────
  private captureOriginalState(): void {
    this.originalData = JSON.stringify({
      practiceType: this.practiceType,
      clinicName: this.clinicName,
      displayName: this.displayName,
      title: this.title,
      yearsExp: this.yearsExp,
      bio: this.bio,
      address: this.address,
      city: this.city,
      country: this.country,
      phone: this.phone,
      email: this.email,
      specialties: [...this.specialties],
      languages: [...this.languages],
      sessionTypes: this.sessionTypes.map(s => ({ ...s })),
      feePerSession: this.feePerSession,
      sessionDuration: this.sessionDuration,
      currency: this.currency,
      schedule: this.schedule.map(d => ({ ...d })),
      publicListing: this.publicListing
    });
  }

  // ── Save / Load ────────────────────────────────────────────────
  saveChanges(): void {
    this.isSaving = true;
    this.saveSuccess = false;
    
    // Simulate API call and save to localStorage
    setTimeout(() => {
      this.isSaving = false;
      this.saveSuccess = true;
      this.hasChanges = false;
      
      const profileData = {
        practiceType: this.practiceType,
        clinicName: this.clinicName,
        displayName: this.displayName,
        title: this.title,
        yearsExp: this.yearsExp,
        bio: this.bio,
        address: this.address,
        city: this.city,
        country: this.country,
        phone: this.phone,
        email: this.email,
        specialties: this.specialties,
        languages: this.languages,
        sessionTypes: this.sessionTypes,
        feePerSession: this.feePerSession,
        sessionDuration: this.sessionDuration,
        currency: this.currency,
        schedule: this.schedule,
        publicListing: this.publicListing
      };
      
      localStorage.setItem('doctor_clinic_profile', JSON.stringify(profileData));
      this.captureOriginalState();
      this.cdr.detectChanges();
      
      setTimeout(() => {
        this.saveSuccess = false;
        this.cdr.detectChanges();
      }, 3000);
    }, 800);
  }

  discardChanges(): void {
    if (!this.originalData) return;
    const data = JSON.parse(this.originalData);
    
    this.practiceType = data.practiceType;
    this.clinicName = data.clinicName;
    this.displayName = data.displayName;
    this.title = data.title;
    this.yearsExp = data.yearsExp;
    this.bio = data.bio;
    this.address = data.address;
    this.city = data.city;
    this.country = data.country;
    this.phone = data.phone;
    this.email = data.email;
    this.specialties = data.specialties;
    this.languages = data.languages;
    this.sessionTypes = data.sessionTypes;
    this.feePerSession = data.feePerSession;
    this.sessionDuration = data.sessionDuration;
    this.currency = data.currency;
    this.schedule = data.schedule;
    this.publicListing = data.publicListing;
    
    this.hasChanges = false;
    this.cdr.detectChanges();
  }

  private loadProfile(): void {
    const saved = localStorage.getItem('doctor_clinic_profile');
    if (saved) {
      try {
        const data = JSON.parse(saved);
        this.practiceType = data.practiceType || 'inperson';
        this.clinicName = data.clinicName || '';
        this.displayName = data.displayName || '';
        this.title = data.title || '';
        this.yearsExp = data.yearsExp || '';
        this.bio = data.bio || '';
        this.address = data.address || '';
        this.city = data.city || '';
        this.country = data.country || '';
        this.phone = data.phone || '';
        this.email = data.email || '';
        this.specialties = data.specialties || [];
        this.languages = data.languages || [];
        this.sessionTypes = data.sessionTypes || this.sessionTypes;
        this.feePerSession = data.feePerSession || '';
        this.sessionDuration = data.sessionDuration || '';
        this.currency = data.currency || 'AED';
        this.schedule = data.schedule || this.schedule;
        this.publicListing = data.publicListing !== undefined ? data.publicListing : true;
      } catch (e) {
        console.error('Error loading clinic profile', e);
      }
    }
  }

  // ── Calc hours between two HH:MM strings ─────────────────────
  calcHours(from: string, to: string): string {
    if (!from || !to) return '';
    const [fh, fm] = from.split(':').map(Number);
    const [th, tm] = to.split(':').map(Number);
    const mins = (th * 60 + tm) - (fh * 60 + fm);
    if (mins <= 0) return '';
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? (m > 0 ? `${h}h ${m}m` : `${h}h`) : `${m}m`;
  }
}
