
import {
    Component, OnInit, OnDestroy,
    inject, ChangeDetectorRef, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth';
import { PatientSessionRoomComponent } from '../../auth/patient-session-room.component/patient-session-room.component';

// ─── Patient Dashboard ────────────────────────────────────────────────────────
// This is the patient-facing shell that routes between pages.
// When activePage === 'session', it renders <app-patient-session-room> which
// handles the full join-call → in-call → ended lifecycle.
//
// The patient's session room component handles session discovery on its own
// (via discoverSessionFromBookings) so no sessionId needs to be passed here
// unless the patient arrives via a direct link with ?sessionId=xxx.
// ─────────────────────────────────────────────────────────────────────────────
@Component({
    selector: 'app-patient-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        PatientSessionRoomComponent,
    ],
    template: `
    <!-- Minimal shell — add your existing patient dashboard sidebar/nav here.
         Replace the content divs below with your actual patient pages. -->
    <div class="patient-app">

      <!-- ── SESSION PAGE ── -->
      <div *ngIf="activePage === 'session'" class="scroll-area">
        <!-- PatientSessionRoomComponent discovers the active session on its own.
             If you want to pass a specific sessionId from the URL or a list item,
             bind it here: [sessionId]="selectedSessionId" -->
        <app-patient-session-room></app-patient-session-room>
      </div>

      <!-- ── OTHER PAGES (add your existing patient pages here) ── -->
      <div *ngIf="activePage === 'dashboard'" class="scroll-area">
        <!-- patient home dashboard content -->
        <div class="placeholder-page">
          <h2>Dashboard</h2>
          <p>Your overview will appear here.</p>
          <button class="go-to-session-btn" (click)="navigate('session')" type="button">
            Join My Appointment
          </button>
        </div>
      </div>

    </div>
  `,
    styles: [`
    .patient-app {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
      font-family: 'Inter', sans-serif;
    }

    .scroll-area {
      flex: 1;
      padding: 24px;
      overflow-y: auto;
    }

    .placeholder-page {
      max-width: 600px;
      margin: 40px auto;
      text-align: center;
    }

    .placeholder-page h2 {
      font-size: 24px;
      font-weight: 700;
      color: #0F172A;
      margin-bottom: 8px;
    }

    .placeholder-page p {
      font-size: 15px;
      color: #64748B;
      margin-bottom: 24px;
    }

    .go-to-session-btn {
      display: inline-flex;
      align-items: center;
      padding: 12px 28px;
      font-size: 14px;
      font-weight: 700;
      font-family: 'Inter', sans-serif;
      color: white;
      background: linear-gradient(135deg, #0B8FAC, #0EA5C2);
      border: none;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.25s ease;
      box-shadow: 0 4px 14px rgba(11, 143, 172, 0.3);
    }

    .go-to-session-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 20px rgba(11, 143, 172, 0.4);
    }
  `]
})
export class PatientDashboardComponent implements OnInit, OnDestroy {

    private readonly router = inject(Router);
    private readonly route = inject(ActivatedRoute);
    private readonly auth = inject(AuthService);
    private readonly cdr = inject(ChangeDetectorRef);
    private readonly destroy$ = new Subject<void>();

    activePage = 'dashboard';

    // If a session was opened from a notification/link with ?sessionId=xxx,
    // this will be set and passed down to the patient session room component.
    selectedSessionId = '';

    ngOnInit(): void {
        // Read the active page from query params so direct URLs like
        // /dashboard?page=session work correctly.
        this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
            if (params['page']) {
                this.activePage = params['page'];
            }
            if (params['sessionId']) {
                this.selectedSessionId = params['sessionId'];
                this.activePage = 'session';
            }
            this.cdr.detectChanges();
        });
    }

    navigate(page: string): void {
        this.activePage = page;
        this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { page },
            queryParamsHandling: 'merge'
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }
}