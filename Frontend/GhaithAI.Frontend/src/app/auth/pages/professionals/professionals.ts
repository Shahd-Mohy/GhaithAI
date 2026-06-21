import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, merge, debounceTime, switchMap, takeUntil, of, catchError } from 'rxjs';
import {
  ProfessionalsService,
  ProfessionalCard,
  ProfessionalsResponse,
  ProfessionalsFilter
} from '../../../services/professionals.service';
import { DoctorBookingComponent } from './doctor-booking/doctor-booking';

@Component({
  selector: 'app-professionals',
  standalone: true,
  imports: [CommonModule, FormsModule, DoctorBookingComponent],
  templateUrl: './professionals.html',
  styleUrls: ['./professionals.css']
})
export class ProfessionalsComponent implements OnInit, OnDestroy {

  doctors: ProfessionalCard[] = [];
  totalCount = 0;
  loading = true;
  error = '';

  // Filter state
  searchTerm = '';
  selectedSpecialty = '';
  selectedLanguage = '';
  selectedSessionType = '';
  selectedCity = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;

  specialtyChips = [
    'All Specialties', 'Anxiety', 'Depression', 'Trauma',
    'Relationships', 'Stress', 'CBT', 'Family Therapy', 'OCD', 'Addiction'
  ];

  sessionTypes = [
    { label: 'Any Type', value: '' },
    { label: 'Online', value: 'Online' },
    { label: 'In-Person', value: 'InPerson' },
    { label: 'Both', value: 'Both' }
  ];

  languages = ['English', 'Arabic', 'French', 'Spanish'];

  selectedDoctorId: string | null = null;

  // Typing/dropdowns are debounced; chips, pagination, clear, retry fire instantly.
  // Both merge into one switchMap pipeline so a stale in-flight request never
  // overwrites a newer one, and markForCheck keeps OnPush ancestors in sync.
  private debouncedTrigger$ = new Subject<void>();
  private instantTrigger$ = new Subject<void>();
  private destroy$ = new Subject<void>();

  constructor(
    private professionalsService: ProfessionalsService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    merge(
      this.debouncedTrigger$.pipe(debounceTime(350)),
      this.instantTrigger$
    ).pipe(
      switchMap(() => {
        this.loading = true;
        this.error = '';
        this.cdr.markForCheck();
        return this.professionalsService.getAll(this.buildFilters()).pipe(
          catchError(() => of(null))
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe((res: ProfessionalsResponse | null) => {
      this.loading = false;
      if (res) {
        this.doctors = res.items ?? [];
        this.totalCount = res.totalCount ?? 0;
      } else {
        this.doctors = [];
        this.error = 'Failed to load professionals. Please try again.';
      }
      this.cdr.markForCheck();
    });

    this.instantTrigger$.next();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildFilters(): ProfessionalsFilter {
    const filters: ProfessionalsFilter = {
      page: this.currentPage,
      pageSize: this.pageSize
    };
    if (this.searchTerm.trim()) filters.search = this.searchTerm.trim();
    if (this.selectedSpecialty) filters.specialty = this.selectedSpecialty;
    if (this.selectedLanguage) filters.language = this.selectedLanguage;
    if (this.selectedSessionType) filters.sessionType = this.selectedSessionType;
    if (this.selectedCity.trim()) filters.city = this.selectedCity.trim();
    return filters;
  }

  /** Kept public so the Retry button in the template still works unchanged. */
  loadDoctors(): void {
    this.instantTrigger$.next();
  }

  // ── Filter triggers (ngModelChange) ──

  onSearchChange(): void {
    this.currentPage = 1;
    this.debouncedTrigger$.next();
  }

  onDropdownChange(): void {
    this.currentPage = 1;
    this.debouncedTrigger$.next();
  }

  onCityInput(): void {
    this.currentPage = 1;
    this.debouncedTrigger$.next();
  }

  selectChip(chip: string): void {
    this.selectedSpecialty = (chip === 'All Specialties') ? '' : chip;
    this.currentPage = 1;
    this.instantTrigger$.next();
  }

  isChipActive(chip: string): boolean {
    return chip === 'All Specialties' ? !this.selectedSpecialty : this.selectedSpecialty === chip;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedSpecialty = '';
    this.selectedLanguage = '';
    this.selectedSessionType = '';
    this.selectedCity = '';
    this.currentPage = 1;
    this.instantTrigger$.next();
  }

  get hasActiveFilters(): boolean {
    return !!(this.searchTerm || this.selectedSpecialty || this.selectedLanguage ||
      this.selectedSessionType || this.selectedCity);
  }

  // Pagination
  get totalPages(): number { return Math.ceil(this.totalCount / this.pageSize); }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, start + 4);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.instantTrigger$.next();
  }

  // Card helpers
  selectDoctor(id: string): void { this.selectedDoctorId = id; }
  goBack(): void { this.selectedDoctorId = null; }

  trackByDoctorId(_index: number, doc: ProfessionalCard): string {
    return doc.doctorId;
  }

  getInitials(name: string): string {
    return (name || '').split(' ').slice(0, 2).map(n => n[0]).join('').toUpperCase();
  }

  getStars(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < Math.round(rating ?? 0));
  }

  getAvatarGradient(index: number): string {
    const gradients = [
      'linear-gradient(135deg,#0B8FAC,#076E86)',
      'linear-gradient(135deg,#6366F1,#4F46E5)',
      'linear-gradient(135deg,#0EA5E9,#0284C7)',
      'linear-gradient(135deg,#14B8A6,#0D9488)',
      'linear-gradient(135deg,#8B5CF6,#7C3AED)',
      'linear-gradient(135deg,#F59E0B,#D97706)',
    ];
    return gradients[index % gradients.length];
  }
}
