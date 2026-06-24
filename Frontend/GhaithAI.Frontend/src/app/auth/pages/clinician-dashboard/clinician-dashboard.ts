import {
  Component, OnInit, OnDestroy, inject,
  ChangeDetectorRef, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../services/auth';
import { MyClinicComponent } from './my-clinic/my-clinic';
import { DoctorScheduleComponent } from './components/doctor-schedule/doctor-schedule';
import { ClinicPatientsComponent } from './components/clinic-patients/clinic-patients';
import { PatientsList } from './doctorPatiant/patients-list/patients-list';
import { ScheduleService } from './services/schedule';
import { SessionRoomComponent } from '../../session-room.component/session-room.component';
import { ClinicalNotesComponent } from './components/clinical-notes/clinical-notes.component';
// ── Interfaces ─────────────────────────────────────────────────────────────

interface ClinicianInfo {
  name: string;
  specialty: string;
  initial: string;
}

interface StatCard {
  label: string;
  value: string | number;
  sub: string;
  icon: 'patients' | 'sessions' | 'notes' | 'risk';
  color: 'teal' | 'green' | 'amber' | 'red';
}

interface ScheduleSession {
  bookingId: string;
  initials: string;
  name: string;
  type: string;
  time: string;
  color: string;
  age?: string;
  condition?: string;
  prevSession?: string;
}

interface RiskAlert {
  name: string;
  level: 'High' | 'Medium' | 'Low';
  description: string;
  timeAgo: string;
}

// ── Component ──────────────────────────────────────────────────────────────

@Component({
  selector: 'app-clinician-dashboard',
  standalone: true,
  imports: [CommonModule, MyClinicComponent, DoctorScheduleComponent, ClinicPatientsComponent, PatientsList, SessionRoomComponent, ClinicalNotesComponent],
  templateUrl: './clinician-dashboard.html',
  styleUrls: ['./clinician-dashboard.css']
})
export class ClinicianDashboardComponent implements OnInit, OnDestroy {

  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();
  private readonly scheduleService = inject(ScheduleService);
  // ── state ────────────────────────────────────────────────────────
  activePage = 'dashboard';
  isLoading = true;
  showUserMenu = false;
  sidebarOpen = false;   // ← controls off-canvas drawer on mobile
  selectedBookingId = '';  // ← set when doctor clicks Start on a session
  selectedPatient: ScheduleSession | null = null;

  clinician: ClinicianInfo = {
    name: 'Clinician',
    specialty: 'Doctor',
    initial: 'C'
  };

  /** Email shown in the sidebar user area */
  clinicianEmail = '';

  // ── computed getters ─────────────────────────────────────────────
  get greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Good Morning';
    if (h < 17) return 'Good Afternoon';
    return 'Good Evening';
  }

  get todayLabel(): string {
    return new Date().toLocaleDateString('en-US', {
      weekday: 'long', month: 'long', day: 'numeric'
    });
  }

  // ── static UI data ─────────────────────────────────────────────
  statCards: StatCard[] = [
    { label: 'Total Patients', value: 48, sub: '+3 this week', icon: 'patients', color: 'teal' },
    { label: 'Sessions Today', value: 6, sub: '2 remaining', icon: 'sessions', color: 'green' },
    { label: 'Notes Pending', value: 4, sub: 'Review needed', icon: 'notes', color: 'amber' },
    { label: 'Risk Alerts', value: 2, sub: 'High priority', icon: 'risk', color: 'red' },
  ];

  todaySchedule: ScheduleSession[] = []

  riskAlerts: RiskAlert[] = [
    { name: 'Ahmed Hassan', level: 'High', description: 'Suicidal ideation expressed', timeAgo: '2 hours ago' },
    { name: 'Fatima Khaled', level: 'Medium', description: 'Increased hopelessness language', timeAgo: 'Yesterday' },
  ];

  // ── lifecycle ─────────────────────────────────────────────────────
  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['page']) this.activePage = params['page'];
    });

    // ── Load real user data from the login response ──────────────
    this.loadClinicianProfile();


    this.loadTodaySchedule();
    this.isLoading = false;

  }
  private loadTodaySchedule(): void {
    this.isLoading = true;
    this.scheduleService.getTodaySchedule(0, 6)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {

          this.todaySchedule = data.map(item => ({
            bookingId: item.bookingId,
            initials: item.patientName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase(),
            name: item.patientName,
            type: item.sessionType,
            time: item.time,
            color: '#0B8FAC'
          }));
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading schedule:', err);
          this.todaySchedule = [];
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }
  // ── Populate clinician info from stored session ───────────────────
  private loadClinicianProfile(): void {
    const user = this.authService.getUser();
    if (!user) return;

    const fullName = user.fullName?.trim() || 'Clinician';

    // Derive the display specialty:
    // 1. Prefer `specialization` (e.g. "Child & Adolescent Psychiatry") if present
    // 2. Fall back to `doctorType` (e.g. "Psychiatrist")
    // 3. Default to role name
    const specialty =
      user.specialization?.trim() ||
      user.doctorType?.trim() ||
      user.role ||
      'Doctor';

    // Build initials from the first letter of the first and last name word
    const nameParts = fullName.split(/\s+/).filter(Boolean);
    let initial = nameParts[0]?.[0]?.toUpperCase() ?? 'C';
    if (nameParts.length > 1) {
      initial += (nameParts[nameParts.length - 1]?.[0]?.toUpperCase() ?? '');
    }

    this.clinician = { name: fullName, specialty, initial };
    this.clinicianEmail = user.email || '';
    this.cdr.detectChanges();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Sidebar (responsive) ──────────────────────────────────────────
  openSidebar(): void { this.sidebarOpen = true; }
  closeSidebar(): void { this.sidebarOpen = false; this.showUserMenu = false; }

  /** Close sidebar when window is resized back to desktop width */
  @HostListener('window:resize', ['$event'])
  onResize(e: Event): void {
    if ((e.target as Window).innerWidth > 1024) {
      this.sidebarOpen = false;
    }
  }

  // ── Navigation ───────────────────────────────────────────────────
  navigate(page: string, filter: string = 'all'): void {
    this.activePage = page;
    this.closeSidebar();

    // نمرر الـ filter الجديد كـ queryParam
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page, filter },
      queryParamsHandling: 'merge'
    });
  }

  isActive(page: string): boolean {
    return this.activePage === page;
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  // ── Logout → goes to landing page ────────────────────────────────
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);   // ← landing page
  }

  startSession(patient: ScheduleSession): void {
    this.selectedBookingId = patient.bookingId;
    this.selectedPatient = patient;
    this.activePage = 'sessions';
  }
}
