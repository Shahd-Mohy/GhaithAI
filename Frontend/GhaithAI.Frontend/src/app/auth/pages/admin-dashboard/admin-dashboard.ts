import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AdminService, DoctorProfile } from '../../../services/admin.service';
import { TokenService } from '../../../services/token';

type Tab = 'pending' | 'approved' | 'rejected';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard.html',
  styleUrls: ['./admin-dashboard.css']
})
export class AdminDashboardComponent implements OnInit {

  activeTab: Tab = 'pending';

  pending: DoctorProfile[] = [];
  approved: DoctorProfile[] = [];
  rejected: DoctorProfile[] = [];

  loading = false;
  actionLoading: string | null = null;

  // Reject modal
  showRejectModal = false;
  rejectTargetId = '';
  rejectReason = '';
  rejectError = '';

  // Details modal
  showDetailsModal = false;
  selectedDoctor: DoctorProfile | null = null;

  doctorTypeLabels = [
    'Psychiatrist', 'Psychologist',
    'Therapist / Counselor', 'Social Worker', 'Other'
  ];

  constructor(
    private adminService: AdminService,
    private tokenService: TokenService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.loading = true;
    this.loadPending();
    this.loadApproved();
    this.loadRejected();
  }

  loadPending(): void {
    this.adminService.getPendingDoctors().subscribe({
      next: (data) => { this.pending = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  loadApproved(): void {
    this.adminService.getApprovedDoctors().subscribe({
      next: (data) => { this.approved = data; }
    });
  }

  loadRejected(): void {
    this.adminService.getRejectedDoctors().subscribe({
      next: (data) => { this.rejected = data; }
    });
  }

  get currentList(): DoctorProfile[] {
    if (this.activeTab === 'pending') return this.pending;
    if (this.activeTab === 'approved') return this.approved;
    return this.rejected;
  }

  setTab(tab: Tab): void {
    this.activeTab = tab;
  }

  // ─── Approve ──────────────────────────────────────────
  approve(id: string): void {
    this.actionLoading = id;
    this.adminService.approveDoctor(id).subscribe({
      next: () => {
        this.actionLoading = null;
        this.loadAll();
      },
      error: () => { this.actionLoading = null; }
    });
  }

  // ─── Reject Modal ─────────────────────────────────────
  openRejectModal(id: string): void {
    this.rejectTargetId = id;
    this.rejectReason = '';
    this.rejectError = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.rejectTargetId = '';
    this.rejectReason = '';
  }

  confirmReject(): void {
    if (!this.rejectReason.trim()) {
      this.rejectError = 'Please provide a rejection reason';
      return;
    }
    this.actionLoading = this.rejectTargetId;
    this.adminService.rejectDoctor(this.rejectTargetId, this.rejectReason).subscribe({
      next: () => {
        this.actionLoading = null;
        this.closeRejectModal();
        this.loadAll();
      },
      error: () => { this.actionLoading = null; }
    });
  }

  // ─── Details Modal ────────────────────────────────────
  openDetails(doctor: DoctorProfile): void {
    this.selectedDoctor = doctor;
    this.showDetailsModal = true;
  }

  closeDetails(): void {
    this.showDetailsModal = false;
    this.selectedDoctor = null;
  }

  // ─── Logout ───────────────────────────────────────────
  logout(): void {
    this.tokenService.removeToken();
    this.router.navigate(['/login']);
  }

  getStatusLabel(status: number): string {
    if (status === 0) return 'Pending';
    if (status === 1) return 'Approved';
    return 'Rejected';
  }

  getDoctorTypeLabel(type: number): string {
    return this.doctorTypeLabels[type] || 'Other';
  }
}