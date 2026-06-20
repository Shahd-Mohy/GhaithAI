
import { ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { DoctorPatientsDashboardDto } from '../interface/doctor-patient.model';
import { DoctorPatient } from '../services/services/doctor-patient';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-patients-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './patients-list.html',
  styleUrl: './patients-list.css',
  standalone: true,
})
export class PatientsList implements OnInit, OnDestroy {
  dashboardData!: DoctorPatientsDashboardDto;
  searchQuery: string = '';
  riskFilter: string = '';

  private searchSubject = new Subject<string>();
  private sub!: Subscription;

  constructor(private patientService: DoctorPatient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadDashboard();
    this.sub = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => this.loadDashboard());
  }
  onFilterChange() {
    this.loadDashboard();
  }

  onSearchInput() {
    this.searchSubject.next(this.searchQuery);
  }

  loadDashboard() {
    this.patientService.getDashboardData(this.searchQuery, this.riskFilter).subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error fetching dashboard', err)
    });
  }

  ngOnDestroy() {
    if (this.sub) this.sub.unsubscribe();
  }
}