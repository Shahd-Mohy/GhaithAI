import { ChangeDetectorRef , Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterOutlet, RouterModule } from '@angular/router';

import {
  AdminService,
  DoctorProfile
} from '../../../services/admin.service';

import { TokenService } from '../../../services/token';

type Tab = 'pending' | 'approved' | 'rejected';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterOutlet,
    RouterModule
  ],
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

  // Inline Reject
  rejectOpenId: string | null = null;
  rejectReason = '';
  rejectError = '';

  constructor(
    private adminService: AdminService,
    private tokenService: TokenService,
    private router: Router ,
     private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  isBaseAdminRoute(): boolean {
    const currentPath = this.router.url.split('?')[0];

    return (
      currentPath === '/admin' ||
      currentPath === '/admin/'
    );
  }

  navigateToDoctors(tab: Tab): void {
    this.setTab(tab);
    this.router.navigate(['/admin']);
  }

  setTab(tab: Tab): void {
    this.activeTab = tab;

    this.rejectOpenId = null;
    this.rejectReason = '';
    this.rejectError = '';
  }

  loadAll(): void {
    this.loading = true;

    this.loadPending();
    this.loadApproved();
    this.loadRejected();
  }

  loadPending(): void {
    this.adminService.getPendingDoctors().subscribe({
      next: (data) => {
        console.log('Pending Doctors =>', data);

        this.pending = data;
        this.loading = false;
          this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  loadApproved(): void {
    this.adminService.getApprovedDoctors().subscribe({
      next: (data) => {
        this.approved = data;
          this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  loadRejected(): void {
    this.adminService.getRejectedDoctors().subscribe({
      next: (data) => {
        this.rejected = data;
          this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  get currentList(): DoctorProfile[] {

    switch (this.activeTab) {

      case 'pending':
        return this.pending;

      case 'approved':
        return this.approved;

      case 'rejected':
        return this.rejected;

      default:
        return [];
    }
  }

  approve(id: string): void {

    this.actionLoading = id;

    this.adminService.approveDoctor(id).subscribe({
      next: () => {

        this.actionLoading = null;

        this.loadAll();
      },
      error: (err) => {

        console.error(err);

        this.actionLoading = null;
      }
    });
  }

  openReject(id: string): void {

    if (this.rejectOpenId === id) {

      this.cancelReject();
      return;
    }

    this.rejectOpenId = id;
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

      this.rejectError =
        'Please provide a rejection reason';

      return;
    }

    this.actionLoading = id;

    this.adminService
      .rejectDoctor(id, this.rejectReason)
      .subscribe({
        next: () => {

          this.actionLoading = null;

          this.cancelReject();

          this.loadAll();
        },
        error: (err) => {

          console.error(err);

          this.actionLoading = null;
        }
      });
  }

  viewDetails(id: string): void {

    this.router.navigate([
      '/admin/doctor',
      id
    ]);
  }

  logout(): void {

    this.tokenService.removeToken();

    this.router.navigate(['/login']);
  }

  getStatusClass(status: string): string {

    switch (status?.toLowerCase()) {

      case 'approved':
        return 'approved';

      case 'rejected':
        return 'rejected';

      default:
        return 'pending';
    }
  }
}