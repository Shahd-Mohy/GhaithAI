import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
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
})
export class AdminDashboardHome {
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

  loadingStats = false;
  loadingLanguages = false;
  loadingSpecialties = false;

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
    this.cdr.detectChanges(); // 👈 أضمن إن الـ Loader يظهر فوراً

    this.languagesService.getAll().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.languages = res.data;
        }
        this.loadingLanguages = false;
        this.cdr.detectChanges(); // 🎯 1. إجبار الـ UI يعرض الداتا الجديدة فوراً
      },
      error: (err) => {
        console.error('Failed to load languages', err);
        this.loadingLanguages = false;
        this.cdr.detectChanges();
      }
    });
  }

  addLanguage(): void {
    if (!this.newLanguageName.trim()) return;

    const dto: CreateLanguageDto = { languageName: this.newLanguageName.trim() };

    this.languagesService.create(dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.newLanguageName = ''; // فك التجميد وتفضية الإنبت
          this.loadLanguages();      // هينده الـ Get والـ Get جواها detectChanges
        }
      },
      error: (err: Error) => {
        alert(err.message);
        this.cdr.detectChanges(); // أضمن إن الـ Alert والـ State يمشوا مظبوط
      }
    });
  }

  deleteLanguage(id: string): void {
    if (confirm('Are you sure you want to delete this language?')) {
      this.languagesService.delete(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadLanguages();
          }
        },
        error: (err: Error) => {
          alert(err.message);
          this.cdr.detectChanges();
        }
      });
    }
  }
  // ─── [CRUD Specialties] ───
  loadSpecialties(): void {
    this.loadingSpecialties = true;
    this.specialtiesService.getAll().subscribe({
      next: (res) => {
        if (res && res.success) {
          this.specialties = res.data;
        }
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

  addSpecialty(): void {
    if (!this.newSpecialtyName.trim()) return;

    const dto: CreateSpecialtyDto = { specialtyName: this.newSpecialtyName.trim() };

    this.specialtiesService.create(dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.newSpecialtyName = '';
          this.loadSpecialties();
        }
      },
      error: (err: Error) => {
        alert(err.message);
      }
    });
  }

  deleteSpecialty(id: string): void {
    if (confirm('Are you sure you want to delete this specialty?')) {
      this.specialtiesService.delete(id).subscribe({
        next: (res) => {
          if (res.success) {
            this.loadSpecialties();
          }
        },
        error: (err: Error) => {
          alert(err.message);
        }
      });
    }
  }
}

