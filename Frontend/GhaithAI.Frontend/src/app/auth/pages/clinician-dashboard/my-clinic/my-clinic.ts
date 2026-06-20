import {
  Component, OnInit, OnDestroy, inject,
  ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin, takeUntil, finalize, of, catchError } from 'rxjs';
import { AuthService } from '../../../../services/auth';
import {
  ClinicService,
  DoctorClinicProfileDto,
  DefaultScheduleDto,
  CustomScheduleDto,
  UpsertCustomScheduleDto,
  SpecialtyItem,
  LanguageItem,
  SessionTypeEnum,
  DaysOfWeek
} from '../services/Clinic.service ';

interface DaySchedule {
  dayOfWeek: DaysOfWeek;
  enabled: boolean;
  from: string;
  to: string;
}

interface CustomScheduleForm {
  editingId: string | null;
  customDate: string;
  startTime: string;
  endTime: string;
  isOffDay: boolean;
  saving: boolean;
}

@Component({
  selector: 'app-my-clinic',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-clinic.html',
  styleUrls: ['../clinician-shared.css', './my-clinic.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyClinicComponent implements OnInit, OnDestroy {

  private readonly authService = inject(AuthService);
  private readonly clinicService = inject(ClinicService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  // ── DoctorType (Psychiatrist / Therapist / Counselor) ─────────
  // Stored separately and always sent back as-is on save.
  // NEVER conflated with availableSessionType (Online / Offline / both).
  practiceType: string = 'Psychiatrist';

  // ── Session type ──────────────────────────────────────────────
  availableSessionType: SessionTypeEnum = 'both';

  // ── Profile fields ────────────────────────────────────────────
  displayName = '';
  title = '';
  clinicName = '';
  yearsExp = 0;
  bio = '';

  // ── Contact fields ────────────────────────────────────────────
  address = '';
  city = '';
  country = '';
  phone = '';
  email = '';

  // ── Lookup data (loaded from DB) ──────────────────────────────
  allSpecialties: SpecialtyItem[] = [];
  allLanguages: LanguageItem[] = [];

  // ── Doctor's selections ───────────────────────────────────────
  selectedSpecialtyIds: string[] = [];
  selectedLanguageIds: string[] = [];

  // ── Fees ──────────────────────────────────────────────────────
  feePerSession = 0;
  sessionDuration = 50;

  // ── Weekly schedule ───────────────────────────────────────────
  readonly allDays: DaysOfWeek[] = [
    'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'
  ];

  schedule: DaySchedule[] = this.allDays.map(d => ({
    dayOfWeek: d, enabled: false, from: '09:00', to: '17:00'
  }));

  // ── Custom schedules ──────────────────────────────────────────
  customSchedules: CustomScheduleDto[] = [];
  showCustomForm = false;
  customForm: CustomScheduleForm = this.emptyCustomForm();

  // ── Public listing ────────────────────────────────────────────
  publicListing = true;

  // ── UI state ──────────────────────────────────────────────────
  isLoading = true;
  isSaving = false;
  saveSuccess = false;
  saveError = '';
  loadError = '';
  hasChanges = false;
  deletingId: string | null = null;

  // Suppresses the publicListing (ngModelChange) callback while
  // applyProfile() sets values programmatically on load.
  private _applyingProfile = false;

  // ── Lifecycle ─────────────────────────────────────────────────
  ngOnInit(): void { this.loadAll(); }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  // ── Computed ──────────────────────────────────────────────────
  get isOnline(): boolean { return this.availableSessionType === 'Online'; }
  isSpecialtySelected(id: string): boolean { return this.selectedSpecialtyIds.includes(id); }
  isLanguageSelected(id: string): boolean { return this.selectedLanguageIds.includes(id); }

  // ── Load all data ─────────────────────────────────────────────
  private loadAll(): void {
    this.isLoading = true;
    this.loadError = '';

    forkJoin({
      specialties: this.clinicService.getAllSpecialties().pipe(catchError(() => of([] as SpecialtyItem[]))),
      languages: this.clinicService.getAllLanguages().pipe(catchError(() => of([] as LanguageItem[]))),
      profile: this.clinicService.getProfile().pipe(catchError(err => of(err)))
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.isLoading = false; this.cdr.markForCheck(); })
      )
      .subscribe(({ specialties, languages, profile }) => {
        this.allSpecialties = specialties;
        this.allLanguages = languages;

        const isHttpError = profile != null && profile.status !== undefined;
        const isError = profile instanceof Error;

        if (profile && !isHttpError && !isError) {
          // Existing doctor — hydrate all fields from saved profile
          this.applyProfile(profile as DoctorClinicProfileDto);
        } else {
          // New doctor or network error — seed from auth token
          const user = this.authService.getUser() as any;
          if (user) {
            this.displayName = user.fullName?.trim() || '';
            this.title = user.specialization?.trim() || '';
            this.practiceType = user.doctorType?.trim() || 'Psychiatrist';
            this.email = user.email || '';
          }
          // Enable Save immediately — new doctor must be able to save
          this.hasChanges = true;

          // Only show a banner for real server errors; 404 just means new doctor
          if (isHttpError && profile.status !== 404) {
            this.loadError = 'Could not load your saved profile. Fill in your details and save.';
          }
        }
      });
  }

  // ── Apply a loaded/saved profile to all component fields ──────
  private applyProfile(p: DoctorClinicProfileDto): void {
    this._applyingProfile = true;

    this.practiceType = p.practiceType || 'Psychiatrist';
    this.availableSessionType = p.availableSessionType ?? 'both';
    this.displayName = p.displayName || '';
    this.title = p.professionalTitle || '';
    this.clinicName = p.clinicName || '';
    this.yearsExp = p.yearsOfExperience || 0;
    this.bio = p.bio || '';
    this.address = p.clinicAddress || '';
    this.city = p.city || '';
    this.country = p.countryCode || '';
    this.phone = p.phone || '';
    this.email = p.contactEmail || '';
    this.publicListing = p.isPublicListed;
    this.feePerSession = p.feePerSession || 0;
    this.sessionDuration = p.sessionDurationMinutes || 50;
    this.selectedSpecialtyIds = [...(p.specialtyIds || [])];
    this.selectedLanguageIds = [...(p.languageIds || [])];
    this.customSchedules = [...(p.customSchedules || [])];

    this.schedule = this.allDays.map(day => {
      const slot = p.weeklySchedule?.find(s => s.dayOfWeek === day);
      return {
        dayOfWeek: day,
        enabled: slot?.isActive ?? false,
        from: slot ? this.tsToInput(slot.startTime) : '09:00',
        to: slot ? this.tsToInput(slot.endTime) : '17:00'
      };
    });

    this.hasChanges = false;

    this._applyingProfile = false;
  }

  // ── Session type cards ────────────────────────────────────────
  setSessionType(type: SessionTypeEnum): void {
    if (this.availableSessionType === type) return;
    this.availableSessionType = type;
    this.markChanged();
  }

  // ── Public listing toggle ─────────────────────────────────────
  onPublicListingChange(): void {
    // Guard: ignore programmatic changes during applyProfile()
    if (this._applyingProfile) return;

    const intended = this.publicListing;
    this.clinicService.setPublicListing(intended)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        error: (err) => {
          this.publicListing = !intended;   // revert
          // 404 = clinic not saved yet; don't alarm the doctor
          if (err?.status !== 404) {
            this.saveError = 'Could not update public listing. Please save your profile first.';
          }
          this.cdr.markForCheck();
        }
      });
  }

  // ── Chip toggles ──────────────────────────────────────────────
  toggleSpecialty(id: string): void {
    this.selectedSpecialtyIds = this.selectedSpecialtyIds.includes(id)
      ? this.selectedSpecialtyIds.filter(x => x !== id)
      : [...this.selectedSpecialtyIds, id];
    this.markChanged();
  }

  toggleLanguage(id: string): void {
    this.selectedLanguageIds = this.selectedLanguageIds.includes(id)
      ? this.selectedLanguageIds.filter(x => x !== id)
      : [...this.selectedLanguageIds, id];
    this.markChanged();
  }

  // ── Custom schedule CRUD ──────────────────────────────────────
  openAddCustomForm(): void {
    this.customForm = this.emptyCustomForm();
    this.showCustomForm = true;
  }

  openEditCustomForm(cs: CustomScheduleDto): void {
    this.customForm = {
      editingId: cs.id,
      customDate: cs.customDate.split('T')[0],
      startTime: this.tsToInput(cs.startTime),
      endTime: this.tsToInput(cs.endTime),
      isOffDay: cs.isOffDay,
      saving: false
    };
    this.showCustomForm = true;
  }

  cancelCustomForm(): void {
    this.showCustomForm = false;
    this.customForm = this.emptyCustomForm();
  }

  saveCustomSchedule(): void {
    if (this.customForm.saving) return;
    this.customForm.saving = true;

    const dto: UpsertCustomScheduleDto = {
      customDate: this.customForm.customDate,
      startTime: this.inputToTs(this.customForm.startTime),
      endTime: this.inputToTs(this.customForm.endTime),
      isOffDay: this.customForm.isOffDay
    };

    const req$ = this.customForm.editingId
      ? this.clinicService.updateCustomSchedule(this.customForm.editingId, dto)
      : this.clinicService.addCustomSchedule(dto);

    req$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (saved) => {
        this.customSchedules = this.customForm.editingId
          ? this.customSchedules.map(cs => cs.id === saved.id ? saved : cs)
          : [...this.customSchedules, saved];
        this.showCustomForm = false;
        this.customForm = this.emptyCustomForm();
        this.cdr.markForCheck();
      },
      error: () => { this.customForm.saving = false; this.cdr.markForCheck(); }
    });
  }

  deleteCustomSchedule(id: string): void {
    if (this.deletingId) return;
    this.deletingId = id;
    this.clinicService.deleteCustomSchedule(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.deletingId = null; this.cdr.markForCheck(); })
      )
      .subscribe({
        next: () => { this.customSchedules = this.customSchedules.filter(cs => cs.id !== id); }
      });
  }

  // ── Save ──────────────────────────────────────────────────────
  saveChanges(): void {
    if (this.isSaving) return;
    this.isSaving = true;
    this.saveError = '';
    this.saveSuccess = false;

    const weeklySchedule: DefaultScheduleDto[] = this.schedule.map(d => ({
      dayOfWeek: d.dayOfWeek,
      startTime: this.inputToTs(d.from),
      endTime: this.inputToTs(d.to),
      isActive: d.enabled
    }));

    const dto = {
      // Send DoctorType (Psychiatrist/Therapist/Counselor), NOT session type
      practiceType: this.practiceType,
      displayName: this.displayName,
      professionalTitle: this.title,
      yearsOfExperience: Number(this.yearsExp),
      bio: this.bio,
      clinicName: this.clinicName || '',
      clinicAddress: this.isOnline ? null : (this.address || null),
      city: this.isOnline ? null : (this.city || null),
      countryCode: this.isOnline ? null : (this.country || null),
      phone: this.phone || '',
      contactEmail: this.email || '',
      isPublicListed: this.publicListing,
      feePerSession: Number(this.feePerSession),
      sessionDurationMinutes: Number(this.sessionDuration),
      availableSessionType: this.availableSessionType,
      specialtyIds: this.selectedSpecialtyIds,
      languageIds: this.selectedLanguageIds,
      weeklySchedule
    };

    this.clinicService.updateProfile(dto)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => { this.isSaving = false; this.cdr.markForCheck(); })
      )
      .subscribe({
        next: (updated) => {
          this.applyProfile(updated);
          this.saveSuccess = true;
          this.cdr.markForCheck();
          setTimeout(() => { this.saveSuccess = false; this.cdr.markForCheck(); }, 3000);
        },
        error: (err) => {
          this.saveError = err?.error?.message || err?.error?.title || 'Failed to save. Please try again.';
        }
      });
  }

  // ── Helpers ───────────────────────────────────────────────────
  markChanged(): void { this.hasChanges = true; }

  calcHours(from: string, to: string): string {
    if (!from || !to) return '';
    const [fh, fm] = from.split(':').map(Number);
    const [th, tm] = to.split(':').map(Number);
    const mins = (th * 60 + tm) - (fh * 60 + fm);
    if (mins <= 0) return '';
    const h = Math.floor(mins / 60), m = mins % 60;
    return h > 0 ? (m > 0 ? `${h}h ${m}m` : `${h}h`) : `${m}m`;
  }

  formatCustomDate(dateStr: string): string {
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? dateStr
      : d.toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  formatTime(ts: string): string { return this.tsToInput(ts); }

  private emptyCustomForm(): CustomScheduleForm {
    return {
      editingId: null,
      customDate: new Date().toISOString().split('T')[0],
      startTime: '09:00',
      endTime: '17:00',
      isOffDay: false,
      saving: false
    };
  }

  // "09:00:00" → "09:00"
  private tsToInput(ts: string): string {
    if (!ts) return '09:00';
    const p = ts.split(':');
    return `${p[0].padStart(2, '0')}:${(p[1] || '00').padStart(2, '0')}`;
  }

  // "09:00" → "09:00:00"
  private inputToTs(t: string): string {
    if (!t) return '00:00:00';
    return t.length === 5 ? `${t}:00` : t;
  }
}