import {
  Component, OnInit, AfterViewInit, OnDestroy,
  ElementRef, ViewChild, inject, ChangeDetectorRef, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SelfHelpComponent } from '../../../selfHelp/self-help';
import { MoodTrackerComponent } from '../../../support/mood/mood';
import { JournalComponent } from '../../../support/journal/journal';
import { AuthService } from '../../../services/auth';
import {
  InsightService,
  DashboardViewModel,
  DailyMoodDTO
} from '../../../services/insight.service';
import { DashboardRefreshService } from '../../../services/dashboard-refresh.service';

interface QuickAction {
  name: string; desc: string; page: string; colorClass: string; icon: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SelfHelpComponent, MoodTrackerComponent, JournalComponent],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {

  @ViewChild('moodChart') moodChartRef!: ElementRef<HTMLCanvasElement>;

  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly insightService = inject(InsightService);
  private readonly refreshService = inject(DashboardRefreshService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly ngZone = inject(NgZone);
  private readonly authService = inject(AuthService);
  private readonly destroy$ = new Subject<void>();

  // ── state ────────────────────────────────────────────────────────
  activePage = 'home';
  selectedMood: string | null = null;
  isLoading = true;
  hasError = false;
  vm: DashboardViewModel | null = null;
  showUserMenu = false;
  /** Config passed to <app-mood-tracker> to auto-open the modal.
   *  A new object reference on every call guarantees ngOnChanges fires. */
  moodOpenConfig: { date: string; moodKey: string | null } | null = null;
  /** Tooltip for chart hover */
  chartTooltip: { x: number; y: number; label: string; emoji: string; moodLabel: string; moodColor: string } | null = null;

  // ── computed getters ─────────────────────────────────────────────
  get greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 17) return 'Good afternoon';
    return 'Good evening';
  }

  get pageTitle(): string {
    if (!this.vm) return 'Welcome Back';
    return `${this.greeting}, ${this.vm.displayName.split(' ')[0]}`;
  }

  get todayLabel(): string {
    return this.vm?.todayLabel ?? new Date().toLocaleDateString('en-US', {
      weekday: 'long', month: 'long', day: 'numeric'
    });
  }

  get insights(): string[] {
    return this.vm?.personalInsights.map(i => i.text) ?? [];
  }

  // ── static UI config ─────────────────────────────────────────────
  moods = [
    { key: 'very-low', emoji: '😔', label: 'Very Low' },
    { key: 'low', emoji: '😕', label: 'Low' },
    { key: 'okay', emoji: '😐', label: 'Okay' },
    { key: 'good', emoji: '🙂', label: 'Good' },
    { key: 'great', emoji: '😄', label: 'Great' },
  ];

  quickActions: QuickAction[] = [
    { name: 'Talk to AI', desc: 'Have a supportive conversation', page: 'chat', colorClass: 'teal', icon: 'chat' },
    { name: 'Breathing Exercise', desc: 'Practical self-help techniques', page: 'breathing', colorClass: 'green', icon: 'breath' },
    { name: 'Journal Entry', desc: 'Write your thoughts', page: 'journal', colorClass: 'amber', icon: 'journal' },
    { name: 'Learn Something', desc: 'Explore psychoeducation', page: 'learn', colorClass: 'purple', icon: 'learn' },
  ];

  // ── lifecycle ─────────────────────────────────────────────────────
  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['page']) this.activePage = params['page'];
    });

    // Initial load
    this.loadDashboard();

    // ✅ Listen for mood/journal saves — reload dashboard automatically
    // even if the user stays on the mood page without navigating home
    this.refreshService.refresh$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        // Small delay so the backend has committed the new record
        setTimeout(() => this.loadDashboard(), 300);
      });
  }

  ngAfterViewInit(): void { /* chart drawn inside loadDashboard */ }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── data loading ─────────────────────────────────────────────────
  loadDashboard(): void {
    this.isLoading = true;
    this.hasError = false;

    this.insightService.getDashboard().subscribe({
      next: data => {
        this.vm = data;
        this.isLoading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.activePage === 'home' && this.drawMoodChart(), 50);
      },
      error: () => {
        this.isLoading = false;
        this.hasError = true;
      }
    });
  }

  // ── navigation ───────────────────────────────────────────────────
  navigate(page: string): void {
    if (page === 'chat') {
      this.router.navigate(['/support/chat']); return;
    }
    if (page === 'crisis') {
      this.router.navigate(['/support/crisis']); return;
    }

    const previousPage = this.activePage;
    this.activePage = page;

    // Also reload when user explicitly navigates back to home
    if (page === 'home' && previousPage !== 'home') {
      this.loadDashboard();
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page },
      queryParamsHandling: 'merge'
    });
  }

  selectMood(key: string): void { this.selectedMood = key; }
  isActive(page: string): boolean { return this.activePage === page; }

  navigateToMoodLog(dateStr: string | null = null, moodKey: string | null = null): void {
    // Remove chart listeners before navigating — prevents stale canvas exceptions
    const canvas = this.moodChartRef?.nativeElement;
    if (canvas) {
      const c = canvas as any;
      if (c.__hoverHandler) canvas.removeEventListener('mousemove', c.__hoverHandler);
      if (c.__leaveHandler) canvas.removeEventListener('mouseleave', c.__leaveHandler);
      if (c.__clickHandler) canvas.removeEventListener('click', c.__clickHandler);
    }
    this.moodOpenConfig = null;
    this.navigate('mood');
    setTimeout(() => {
      setTimeout(() => {
        this.moodOpenConfig = { date: dateStr ?? this.formatTodayStr(), moodKey };
        this.cdr.detectChanges();
      }, 0);
    }, 0);
  }

  private formatTodayStr(): string {
    const d = new Date();
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  // ── mood chart ───────────────────────────────────────────────────
  drawMoodChart(): void {
    const canvas = this.moodChartRef?.nativeElement;
    if (!canvas || !this.vm) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const moods: DailyMoodDTO[] = this.vm.weeklySummary.dailyMoods;
    const days = moods.map(m => m.dayLabel);
    const scores = moods.map(m => m.moodScore ?? 0);
    const hasData = moods.map(m => m.moodScore !== null);

    const W = canvas.parentElement?.clientWidth ?? 400;
    const H = 160;
    canvas.width = W;
    canvas.height = H;

    const pad = { top: 16, right: 16, bottom: 32, left: 24 };
    const maxV = 5;
    const n = scores.length;
    const cW = n > 1 ? (W - pad.left - pad.right) / (n - 1) : W - pad.left - pad.right;
    const cH = H - pad.top - pad.bottom;

    const xOf = (i: number) => pad.left + i * cW;
    const yOf = (v: number) => pad.top + cH - (v / maxV) * cH;

    ctx.clearRect(0, 0, W, H);

    const grad = ctx.createLinearGradient(0, pad.top, 0, H - pad.bottom);
    grad.addColorStop(0, 'rgba(11,143,172,.18)');
    grad.addColorStop(1, 'rgba(11,143,172,0)');

    ctx.beginPath();
    ctx.moveTo(xOf(0), yOf(scores[0]));
    for (let i = 1; i < n; i++) {
      const cpx = (xOf(i - 1) + xOf(i)) / 2;
      ctx.bezierCurveTo(cpx, yOf(scores[i - 1]), cpx, yOf(scores[i]), xOf(i), yOf(scores[i]));
    }
    ctx.lineTo(xOf(n - 1), H - pad.bottom);
    ctx.lineTo(xOf(0), H - pad.bottom);
    ctx.closePath();
    ctx.fillStyle = grad;
    ctx.fill();

    ctx.beginPath();
    ctx.moveTo(xOf(0), yOf(scores[0]));
    for (let i = 1; i < n; i++) {
      const cpx = (xOf(i - 1) + xOf(i)) / 2;
      ctx.bezierCurveTo(cpx, yOf(scores[i - 1]), cpx, yOf(scores[i]), xOf(i), yOf(scores[i]));
    }
    ctx.strokeStyle = '#0B8FAC';
    ctx.lineWidth = 2.5;
    ctx.stroke();

    scores.forEach((v, i) => {
      ctx.beginPath();
      ctx.arc(xOf(i), yOf(v), 4.5, 0, Math.PI * 2);
      if (hasData[i]) {
        ctx.fillStyle = '#0B8FAC'; ctx.fill();
        ctx.strokeStyle = '#fff'; ctx.lineWidth = 2; ctx.stroke();
      } else {
        ctx.fillStyle = '#fff'; ctx.fill();
        ctx.strokeStyle = '#CBD5E0'; ctx.lineWidth = 1.5; ctx.stroke();
      }
    });

    ctx.fillStyle = '#64748B';
    ctx.font = '11px Sora, sans-serif';
    ctx.textAlign = 'center';
    days.forEach((d, i) => ctx.fillText(d, xOf(i), H - 8));

    // ── Hover tooltip ──────────────────────────────────────────────
    const moodEmojiMap: { [score: number]: { emoji: string; label: string; color: string } } = {
      1: { emoji: '😔', label: 'Very Low', color: '#fca5a5' },
      2: { emoji: '😕', label: 'Low', color: '#fdba74' },
      3: { emoji: '😐', label: 'Okay', color: '#fde68a' },
      4: { emoji: '🙂', label: 'Good', color: '#86efac' },
      5: { emoji: '😄', label: 'Great', color: '#6ee7b7' },
    };

    // Remove any previous listener to avoid stacking
    const oldHandler = (canvas as any).__hoverHandler;
    if (oldHandler) canvas.removeEventListener('mousemove', oldHandler);
    canvas.removeEventListener('mouseleave', (canvas as any).__leaveHandler);

    const hoverHandler = (e: MouseEvent) => {
      this.ngZone.run(() => {
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;   // canvas px per CSS px
        const scaleY = canvas.height / rect.height;
        const mx = (e.clientX - rect.left) * scaleX;  // now in canvas pixels
        const my = (e.clientY - rect.top) * scaleY;

        let found = false;
        for (let i = 0; i < n; i++) {
          const dx = mx - xOf(i);
          const dy = my - yOf(scores[i]);
          if (Math.sqrt(dx * dx + dy * dy) < 24) {
            // Convert canvas-pixel point back to CSS pixels for overlay positioning
            const cssx = xOf(i) / scaleX;
            const cssy = yOf(hasData[i] ? scores[i] : maxV / 2) / scaleY;
            const moodInfo = hasData[i] ? moodEmojiMap[scores[i]] : null;
            this.chartTooltip = {
              x: cssx,
              y: cssy,
              label: days[i],
              emoji: moodInfo?.emoji ?? '—',
              moodLabel: moodInfo?.label ?? '—',
              moodColor: moodInfo?.color ?? '#64748B',
            };
            found = true;
            break;
          }
        }
        if (!found) {
          this.chartTooltip = null;
        }
        this.cdr.detectChanges();
      });
    };

    const leaveHandler = () => {
      this.ngZone.run(() => {
        this.chartTooltip = null;
        this.cdr.detectChanges();
      });
    };

    (canvas as any).__hoverHandler = hoverHandler;
    (canvas as any).__leaveHandler = leaveHandler;
    canvas.addEventListener('mousemove', hoverHandler);
    canvas.addEventListener('mouseleave', leaveHandler);

    // ── Click on a chart day → open mood log for that date ─────────
    const oldClickHandler = (canvas as any).__clickHandler;
    if (oldClickHandler) canvas.removeEventListener('click', oldClickHandler);

    const clickHandler = (e: MouseEvent) => {
      this.ngZone.run(() => {
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const mx = (e.clientX - rect.left) * scaleX;
        for (let i = 0; i < n; i++) {
          if (Math.abs(mx - xOf(i)) < 24) {
            const moodData = moods[i];
            if (moodData.date) {
              this.navigateToMoodLog(moodData.date);
            }
            break;
          }
        }
      });
    };
    (canvas as any).__clickHandler = clickHandler;
    canvas.addEventListener('click', clickHandler);
    canvas.style.cursor = 'pointer';
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

}