import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdminService, DoctorProfile } from '../../../services/admin.service';

@Component({
  selector: 'app-doctor-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './doctor-detail.html',
  styleUrls: ['./doctor-detail.css']
})
export class DoctorDetailComponent implements OnInit {

  doctor: DoctorProfile | null = null;
  loading = true;
  actionLoading = false;

  showRejectForm = false;
  rejectReason = '';
  rejectError = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private adminService: AdminService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadDoctor(id);
  }

  loadDoctor(id: string): void {
    this.loading = true;
    this.adminService.getDoctorDetails(id).subscribe({
      next: (data) => { this.doctor = data; this.loading = false; },
      error: () => { this.loading = false; this.router.navigate(['/admin']); }
    });
  }

  approve(): void {
    if (!this.doctor) return;
    this.actionLoading = true;
    this.adminService.approveDoctor(this.doctor.id).subscribe({
      next: () => {
        this.actionLoading = false;
        this.successMessage = 'Doctor approved successfully!';
        this.loadDoctor(this.doctor!.id);
      },
      error: () => { this.actionLoading = false; }
    });
  }

  toggleRejectForm(): void {
    this.showRejectForm = !this.showRejectForm;
    this.rejectReason = '';
    this.rejectError = '';
  }

  confirmReject(): void {
    if (!this.rejectReason.trim()) {
      this.rejectError = 'Please provide a rejection reason';
      return;
    }
    if (!this.doctor) return;
    this.actionLoading = true;
    this.adminService.rejectDoctor(this.doctor.id, this.rejectReason).subscribe({
      next: () => {
        this.actionLoading = false;
        this.showRejectForm = false;
        this.successMessage = 'Doctor rejected.';
        this.loadDoctor(this.doctor!.id);
      },
      error: () => { this.actionLoading = false; }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin']);
  }
}
