import { Component, inject, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardStats } from './Services/dashboard-stats';
import { Languages } from './Services/languages';
import { Specialties } from './Services/specialties';
import { AdminDashboardStatsDto } from './interfaces/dashboard-stats.interface';
import { CreateSpecialtyDto, SpecialtyListDto } from './interfaces/specialty.interface';
import { CreateLanguageDto, LanguageListDto } from './interfaces/language.interface';

@Component({
  selector: 'app-admin-dashboard-home',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard-home.html',
  styleUrl: './admin-dashboard-home.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class AdminDashboardHome implements OnInit {
  private statsService = inject(DashboardStats);
  private languagesService = inject(Languages);
  private specialtiesService = inject(Specialties);
  private cdr = inject(ChangeDetectorRef);

  stats: AdminDashboardStatsDto = {
    pendingDoctors: 0,
    approvedDoctors: 0,
    totalBookings: 0,
    totalSelfHelpContents: 0,
    totalAIChatSessions: 0,
    totalAIChatMassage: 0
  };

  languages: LanguageListDto[] = [];
  specialties: SpecialtyListDto[] = [];

  newLanguageName = '';
  newSpecialtyName = '';

  // متغيرات رسائل الخطأ تحت الانبوت
  languageError = '';
  specialtyError = '';

  loadingStats = false;
  loadingLanguages = false;
  loadingSpecialties = false;
  currentDate = new Date().toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  ngOnInit(): void {
    this.loadDashboardStats();
    this.loadLanguages();
    this.loadSpecialties();
  }

  loadDashboardStats(): void {
    this.loadingStats = true;
    this.cdr.markForCheck();
    this.statsService.getStats().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.stats = res.data;
          this.loadingStats = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('Failed to load stats', err);
        this.loadingStats = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadLanguages(): void {
    this.loadingLanguages = true;
    this.cdr.detectChanges();

    this.languagesService.getAll().subscribe({
      next: (res: LanguageListDto[]) => {
        console.log('Languages response:', res);
        this.languages = [...res]; // الـ res مصفوفة مباشرة
        this.loadingLanguages = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load languages', err);
        this.loadingLanguages = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadSpecialties(): void {
    this.loadingSpecialties = true;
    this.cdr.detectChanges();

    this.specialtiesService.getAll().subscribe({
      next: (res: SpecialtyListDto[]) => {
        this.specialties = [...res];
        this.loadingSpecialties = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load specialties', err);
        this.loadingSpecialties = false;
        this.cdr.detectChanges();
      }
    });
  }




  addLanguage(): void {
    this.languageError = '';
    if (!this.newLanguageName.trim()) {
      this.languageError = 'Language name cannot be empty.';
      return;
    }

    const dto: CreateLanguageDto = { languageName: this.newLanguageName.trim() };

    this.languagesService.create(dto).subscribe({
      next: () => {
        this.newLanguageName = '';
        this.loadLanguages();
      },
      error: (err: Error) => {
        this.languageError = err.message; // إظهار الخطأ تحت الإنبت
        this.cdr.detectChanges();
      }
    });
  }
  pendingDeleteLanguageId: string | null = null;
  pendingDeleteSpecialtyId: string | null = null;
  deleteLanguage(id: string): void {
    // لو أول مرة يدوس، يخليه في حالة انتظار التأكيد
    if (this.pendingDeleteLanguageId !== id) {
      this.pendingDeleteLanguageId = id;
      return;
    }

    // لو داس تاني والـ id متطابق، ينفذ الحذف المباشر
    this.languagesService.delete(id).subscribe({
      next: () => {
        this.pendingDeleteLanguageId = null; // تصفير الحالة
        this.loadLanguages();
      },
      error: (err: Error) => {
        this.languageError = err.message;
        this.pendingDeleteLanguageId = null;
        this.cdr.detectChanges();
      }
    });
  }

  // ─── [CRUD Specialties] ───

  addSpecialty(): void {
    this.specialtyError = ''; // تصفير الخطأ القديم
    if (!this.newSpecialtyName.trim()) {
      this.specialtyError = 'Specialty name cannot be empty.';
      return;
    }

    const dto: CreateSpecialtyDto = { specialtyName: this.newSpecialtyName.trim() };

    this.specialtiesService.create(dto).subscribe({
      next: () => {
        this.newSpecialtyName = '';
        this.loadSpecialties();
      },
      error: (err: Error) => {
        this.specialtyError = err.message; // إظهار الخطأ تحت الإنبت
        this.cdr.detectChanges();
      }
    });
  }

  deleteSpecialty(id: string): void {
    if (this.pendingDeleteSpecialtyId !== id) {
      this.pendingDeleteSpecialtyId = id;
      return;
    }

    this.specialtiesService.delete(id).subscribe({
      next: () => {
        this.pendingDeleteSpecialtyId = null;
        this.loadSpecialties();
      },
      error: (err: Error) => {
        this.specialtyError = err.message;
        this.pendingDeleteSpecialtyId = null;
        this.cdr.detectChanges();
      }
    });
  }
}