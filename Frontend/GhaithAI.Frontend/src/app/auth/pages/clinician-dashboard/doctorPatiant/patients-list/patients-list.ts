import { Component } from '@angular/core';
import { DoctorPatientsDashboardDto } from '../interface/doctor-patient.model';
import { DoctorPatient } from '../services/services/doctor-patient';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patients-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './patients-list.html',
  styleUrl: './patients-list.css',
  standalone: true,
})
export class PatientsList {
  dashboardData!: DoctorPatientsDashboardDto;
  searchQuery: string = '';
  riskFilter: string = '';
  constructor(private patientService: DoctorPatient) { }

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.patientService.getDashboardData(this.searchQuery, this.riskFilter).subscribe({
      next: (data) => {
        this.dashboardData = data;
      },
      error: (err) => console.error('Error fetching dashboard', err)
    });
  }
}

