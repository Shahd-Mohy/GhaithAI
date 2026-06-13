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

  // ✅ Inline reject بدل modal
  rejectOpenId: string | null = null;
  rejectReason = '';
  rejectError = '';

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
    this.rejectOpenId = null;
  }

  approve(id: string): void {
    this.actionLoading = id;
    this.adminService.approveDoctor(id).subscribe({
      next: () => { this.actionLoading = null; this.loadAll(); },
      error: () => { this.actionLoading = null; }
    });
  }

  // ✅ Inline reject
  openReject(id: string): void {
    this.rejectOpenId = this.rejectOpenId === id ? null : id;
    this.rejectReason = '';
    this.rejectError = '';
  }

  cancelReject(): void {
    this.rejectOpenId = null;
    this.rejectReason = '';
    this.rejectError = '';
  }

  confirmReject(id: string): void {
    if (!this.rejectReason.trim()) {
      this.rejectError = 'Please provide a rejection reason';
      return;
    }
    this.actionLoading = id;
    this.adminService.rejectDoctor(id, this.rejectReason).subscribe({
      next: () => {
        this.actionLoading = null;
        this.rejectOpenId = null;
        this.rejectReason = '';
        this.loadAll();
      },
      error: () => { this.actionLoading = null; }
    });
  }

  // ✅ بدل details modal - روح لصفحة منفصلة
  viewDetails(id: string): void {
    this.router.navigate(['/admin/doctor', id]);
  }

  logout(): void {
    this.tokenService.removeToken();
    this.router.navigate(['/login']);
  }
}
