import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService, PublicDoctorCard } from '../../../services/doctor.service';
import { DoctorBookingComponent } from "./doctor-booking/doctor-booking";

@Component({
  selector: 'app-professionals',
  standalone: true,
  imports: [CommonModule, FormsModule, DoctorBookingComponent],
  templateUrl: './professionals.html',
  styleUrls: ['./professionals.css']
})
export class ProfessionalsComponent implements OnInit {

  doctors: PublicDoctorCard[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  selectedSpecialty = '';
  selectedLanguage = '';

  specialties = [
    'Anxiety & Stress', 'Depression', 'Trauma & PTSD',
    'Addiction & Recovery', 'Child & Adolescent',
    'Couples & Family', 'OCD', 'Eating Disorders', 'Grief & Loss'
  ];

  languages = ['English', 'Arabic', 'French'];

  // ✅ Output للـ parent عشان يفتح profile
  selectedDoctorId: string | null = null;

  constructor(private doctorService: DoctorService) {}

  ngOnInit(): void {
    this.loadDoctors();
  }

  loadDoctors(): void {
    this.loading = true;
    this.error = '';

    this.doctorService.getDoctors({
      searchTerm: this.searchTerm || undefined,
      specialty: this.selectedSpecialty || undefined,
      language: this.selectedLanguage || undefined
    }).subscribe({
      next: (data) => {
        this.doctors = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load doctors. Please try again.';
        this.loading = false;
      }
    });
  }

  search(): void {
    this.loadDoctors();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedSpecialty = '';
    this.selectedLanguage = '';
    this.loadDoctors();
  }

  selectDoctor(doctorId: string): void {
    this.selectedDoctorId = doctorId;
  }

  goBack(): void {
    this.selectedDoctorId = null;
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }

  getSessionTypeLabel(type: number): string {
    if (type === 0) return 'In-Person';
    if (type === 1) return 'Online';
    return 'Both';
  }
}
