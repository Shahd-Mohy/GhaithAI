import { Component, OnInit, OnChanges, SimpleChanges, Input, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MoodService } from '../../services/mood.service';
import { DashboardRefreshService } from '../../services/dashboard-refresh.service';

interface MoodLog {
  moodLogId?: string;
  mood: string;
  emotions: string[];
  notes: string;
}

interface CalendarDay {
  date: Date | null;
  dayNumber: number | null;
  isCurrentMonth: boolean;
  isToday: boolean;
  isFuture: boolean;
  isLoading: boolean;   // shows skeleton while API is in flight
  moodKey: string | null;
}

@Component({
  selector: 'app-mood-tracker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mood.html',
  styleUrl: './mood.css'
})
export class MoodTrackerComponent implements OnInit, OnChanges {

  // ─── Input: config object from dashboard to auto-open the modal ─────────────
  // A new object reference every call → ngOnChanges always fires reliably.
  @Input() openConfig: { date: string; moodKey: string | null } | null = null;

  // ─── Calendar ────────────────────────────────────────────────────────────────
  currentDate = new Date();
  selectedDate = new Date();
  daysGrid: CalendarDay[] = [];
  isCalendarLoading = false;

  // ─── Logs — keyed by "yyyy-MM-dd", scoped to the VISIBLE month only ──────────
  // Each time we switch months, this is rebuilt from scratch for that month.
  logs: { [dateStr: string]: MoodLog } = {};

  // ─── Statistics ──────────────────────────────────────────────────────────────
  avgMoodScore = 0;
  daysLoggedCount = 0;
  currentStreak = 0;
  topEmotion = 'None';
  topEmotionCount = 0;
  moodTrendLabel = 'Same as last week';
  moodTrendClass = 'trend-neutral';

  // ─── Insights & Top Emotions ─────────────────────────────────────────────────
  insightsList: { type: string; title: string; desc: string; class: string }[] = [];
  topEmotionsList: { name: string; count: number }[] = [];

  // ─── Modal ───────────────────────────────────────────────────────────────────
  showLogModal = false;
  modalDateStr = '';
  modalMood = 'okay';
  modalEmotions: string[] = [];
  modalNotes = '';
  isEditing = false;
  isSaving = false;
  saveError = '';

  // ─── History (used in template recent-history-section) ───────────────────────
  historyLogs: any[] = [];
  showAllHistory = false;

  // ─── Max date for the date picker input in the modal ─────────────────────────
  // Prevents the browser native date picker from allowing future dates.
  // Set properly in ngOnInit() — empty string here is just the initial placeholder.
  maxDateStr: string = '';

  // ─── Config ──────────────────────────────────────────────────────────────────
  moodOptions = [
    { key: 'very-low', label: 'Very Low', emoji: '😔', score: 1, color: '#fca5a5' },
    { key: 'low', label: 'Low', emoji: '😕', score: 2, color: '#fdba74' },
    { key: 'okay', label: 'Okay', emoji: '😐', score: 3, color: '#fde68a' },
    { key: 'good', label: 'Good', emoji: '🙂', score: 4, color: '#86efac' },
    { key: 'great', label: 'Great', emoji: '😄', score: 5, color: '#6ee7b7' }
  ];

  emotionOptions = [
    'Calm', 'Anxious', 'Grateful', 'Tired',
    'Happy', 'Sad', 'Energetic', 'Stressed',
    'Relaxed', 'Frustrated', 'Excited', 'Lonely'
  ];

  constructor(
    private moodService: MoodService,
    private cdr: ChangeDetectorRef,
    private refreshService: DashboardRefreshService
  ) { }

  ngOnInit() {
    this.maxDateStr = this.formatDateStr(new Date());

    if (this.openConfig?.date) {
      const [y, m, d] = this.openConfig.date.split('-').map(Number);
      this.currentDate = new Date(y, m - 1, 1);
      this.selectedDate = new Date(y, m - 1, d);
    }

    this.loadMonthData(false, this.openConfig?.date ?? null, this.openConfig?.moodKey ?? null);
    this.loadStatistics();
    this.loadHistory();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.maxDateStr) return;

    const configChange = changes['openConfig'];
    if (configChange && configChange.currentValue && !configChange.isFirstChange()) {
      const cfg = configChange.currentValue as { date: string; moodKey: string | null };
      const [y, m, d] = cfg.date.split('-').map(Number);
      const targetMonth = new Date(y, m - 1, 1);

      // If the calendar already shows the correct month and isn't still fetching,
      // open the modal directly — no second API call, no double-modal bug.
      const sameMonth =
        this.currentDate.getFullYear() === targetMonth.getFullYear() &&
        this.currentDate.getMonth() === targetMonth.getMonth();

      this.currentDate = targetMonth;
      this.selectedDate = new Date(y, m - 1, d);

      if (sameMonth && !this.isCalendarLoading) {
        setTimeout(() => {
          this.openLogModalForDate(new Date(y, m - 1, d), cfg.moodKey);
          this.cdr.detectChanges();
        }, 0);
      } else {
        this.loadMonthData(false, cfg.date, cfg.moodKey);
        this.loadStatistics();
      }
    }
  }

  // ─── Core: load data for the CURRENTLY VIEWED month ──────────────────────────
  // refreshStats: pass true after a save/update so stats cards update too
  private loadMonthData(refreshStats: boolean, autoOpenDate: string | null = null, autoOpenMoodKey: string | null = null) {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth() + 1;

    // Step 1: clear previous month's logs and show skeleton immediately
    this.logs = {};
    this.isCalendarLoading = true;
    this.buildCalendarGrid();   // renders skeleton cells

    // Step 2: fetch from API
    this.moodService.getCalendar(year, month).subscribe({
      next: (res) => {
        // Unwrap every possible response shape the backend might return:
        // { data: [...] }  { Data: [...] }  { result: [...] }  or the array directly
        let dataArr: any[] = [];
        if (Array.isArray(res)) dataArr = res;
        else if (Array.isArray(res?.data)) dataArr = res.data;
        else if (Array.isArray(res?.Data)) dataArr = res.Data;
        else if (Array.isArray(res?.result)) dataArr = res.result;
        else if (Array.isArray(res?.Result)) dataArr = res.Result;
        else if (Array.isArray(res?.items)) dataArr = res.items;
        // Nothing matched — log the actual shape so we can debug
        if (dataArr.length === 0 && res != null) {
          console.warn('[MoodTracker] calendar response keys:', Object.keys(res));
        }

        const freshLogs: { [dateStr: string]: MoodLog } = {};

        dataArr.forEach((item: any) => {
          const rawDate = item.date || item.loggedAt;
          if (!rawDate || typeof rawDate !== 'string') return;

          // Grab the date portion (yyyy-MM-dd) — no timezone conversion needed
          // because the backend (after its fix) returns the correct local date
          const dateStr = rawDate.substring(0, 10);
          if (!/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) return;

          // Only store dates that actually belong to this month view
          const [y, m] = dateStr.split('-').map(Number);
          if (y !== year || m !== month) return;

          const moodOpt = this.moodOptions.find(o => o.score === item.moodScore);

          let ems: string[] = [];
          if (Array.isArray(item.emotionTags)) {
            ems = item.emotionTags.filter((e: string) => this.emotionOptions.includes(e));
          } else if (typeof item.emotionTags === 'string' && item.emotionTags.trim()) {
            ems = item.emotionTags
              .split(',')
              .map((e: string) => e.trim())
              .filter((e: string) => this.emotionOptions.includes(e));
          }

          // ISSUE 2 FIX: Read moodLogId from calendar (now in CalendarDayDTO)
          // Without this every day click went to CREATE path instead of UPDATE
          freshLogs[dateStr] = {
            moodLogId: item.moodLogId || item.MoodLogId || undefined,
            mood: moodOpt ? moodOpt.key : 'okay',
            emotions: ems,
            notes: item.notes || item.Notes || ''
          };
        });

        // Step 3: replace logs, rebuild grid with real data
        this.logs = freshLogs;
        this.isCalendarLoading = false;
        this.buildCalendarGrid();
        this.calculateLocalTopEmotions();
        this.generateDynamicInsights();
        this.cdr.detectChanges(); // force Angular to re-render after async HTTP

        // Auto-open modal for a specific date (e.g. clicked from dashboard chart/check-in)
        // setTimeout defers past the current change-detection cycle → no NG0100
        if (autoOpenDate) {
          const [ay, am, ad] = autoOpenDate.split('-').map(Number);
          const dateObj = new Date(ay, am - 1, ad);
          setTimeout(() => {
            this.openLogModalForDate(dateObj, autoOpenMoodKey);
            this.cdr.detectChanges();
          }, 0);
        }

        if (refreshStats) {
          this.loadStatistics();
          this.loadHistory();
        }
      },
      error: (err) => {
        console.error('Calendar fetch error:', err);
        this.logs = {};
        this.isCalendarLoading = false;
        this.buildCalendarGrid();
        this.calculateLocalTopEmotions();
        this.generateDynamicInsights();
        this.cdr.detectChanges();
      }
    });
  }

  loadHistory() {
    this.moodService.getHistory(1, 20).subscribe({
      next: (res) => {
        // Backend returns PascalCase: { Data: [...], TotalCount: N, ... }
        // Angular HttpClient does NOT auto-camelCase — we must handle both casings
        // Unwrap history response — check every possible key
        let raw: any = null;
        if (Array.isArray(res)) raw = res;
        else if (Array.isArray(res?.data)) raw = res.data;
        else if (Array.isArray(res?.Data)) raw = res.Data;
        else if (Array.isArray(res?.result)) raw = res.result;
        else if (Array.isArray(res?.Result)) raw = res.Result;
        else if (Array.isArray(res?.items)) raw = res.items;
        else {
          console.warn('[MoodTracker] history response keys:', res ? Object.keys(res) : 'null');
          raw = [];
        }
        const arr: any[] = Array.isArray(raw) ? raw : [];
        this.historyLogs = arr
          .filter((item: any) => item.moodLogId || item.MoodLogId)
          .map((item: any) => ({
            moodLogId: item.moodLogId || item.MoodLogId,
            moodScore: item.moodScore ?? item.MoodScore,
            moodLabel: item.moodLabel || item.MoodLabel || '',
            moodBadge: item.moodBadge || item.MoodBadge || '',
            date: item.date || item.Date || '',
            loggedAt: item.loggedAt || item.LoggedAt || '',
            emotionTags: Array.isArray(item.emotionTags) ? item.emotionTags
              : Array.isArray(item.EmotionTags) ? item.EmotionTags
                : typeof item.emotionTags === 'string' ? item.emotionTags.split(',').map((e: string) => e.trim()).filter((e: string) => e)
                  : typeof item.EmotionTags === 'string' ? item.EmotionTags.split(',').map((e: string) => e.trim()).filter((e: string) => e)
                    : [],
            notes: item.notes || item.Notes || ''
          }));
        this.cdr.detectChanges(); // inside next — fires after data arrives
      },
      error: (err) => console.error('History fetch error:', err)
    });
  }

  loadStatistics() {
    this.moodService.getStatistics('month').subscribe({
      next: (res) => {
        // Backend SuccessResponse wraps in .data (camelCase) via JsonSerializerOptions
        // but depending on config it may be .Data — handle both
        // Unwrap statistics the same way — check every possible key
        let data: any = null;
        if (res?.data && typeof res.data === 'object' && !Array.isArray(res.data)) data = res.data;
        else if (res?.Data && typeof res.Data === 'object' && !Array.isArray(res.Data)) data = res.Data;
        else if (res?.result && typeof res.result === 'object' && !Array.isArray(res.result)) data = res.result;
        else if (res?.Result && typeof res.Result === 'object' && !Array.isArray(res.Result)) data = res.Result;
        else if (res && typeof res === 'object' && res.avgMoodScore !== undefined) data = res;
        else if (res && typeof res === 'object' && res.AvgMoodScore !== undefined) data = res;
        if (!data) {
          console.warn('[MoodTracker] stats response keys:', res ? Object.keys(res) : 'null');
          return;
        }

        this.avgMoodScore = data.avgMoodScore ?? data.AvgMoodScore ?? 0;
        this.daysLoggedCount = data.totalLogs ?? data.TotalLogs ?? 0;
        this.currentStreak = data.streakDays ?? data.StreakDays ?? 0;
        this.topEmotion = data.topEmotion || data.TopEmotion || 'None';
        this.topEmotionCount = data.topEmotionCount ?? data.TopEmotionCount ?? 0;

        const diff: number = data.changeFromLastWeek ?? data.ChangeFromLastWeek ?? 0;
        if (diff > 0) {
          this.moodTrendLabel = `+${diff.toFixed(1)} from last week`;
          this.moodTrendClass = 'trend-up';
        } else if (diff < 0) {
          this.moodTrendLabel = `${diff.toFixed(1)} from last week`;
          this.moodTrendClass = 'trend-down';
        } else {
          this.moodTrendLabel = 'Same as last week';
          this.moodTrendClass = 'trend-neutral';
        }
        this.cdr.detectChanges(); // inside next — fires after data arrives
      },
      error: (err) => console.error('Statistics fetch error:', err)
    });
  }

  // ─── Calendar grid builder ───────────────────────────────────────────────────

  buildCalendarGrid() {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();      // 0-indexed for Date()

    const firstDayIndex = new Date(year, month, 1).getDay();
    const daysInMonthCount = new Date(year, month + 1, 0).getDate();

    const now = new Date();
    const todayStr = this.formatDateStr(now);

    const grid: CalendarDay[] = [];

    // Leading empty cells (days before the 1st)
    for (let i = 0; i < firstDayIndex; i++) {
      grid.push({
        date: null, dayNumber: null, isCurrentMonth: false,
        isToday: false, isFuture: false, isLoading: false, moodKey: null
      });
    }

    // Actual days of the month
    for (let d = 1; d <= daysInMonthCount; d++) {
      const date = new Date(year, month, d);
      const dateStr = this.formatDateStr(date);
      const log = this.logs[dateStr];
      const isFuture = dateStr > todayStr;

      grid.push({
        date,
        dayNumber: d,
        isCurrentMonth: true,
        isToday: dateStr === todayStr,
        isFuture,
        isLoading: this.isCalendarLoading,
        moodKey: log ? log.mood : null
      });
    }

    // Trailing empty cells to fill the last row
    const totalCells = Math.ceil(grid.length / 7) * 7;
    while (grid.length < totalCells) {
      grid.push({
        date: null, dayNumber: null, isCurrentMonth: false,
        isToday: false, isFuture: false, isLoading: false, moodKey: null
      });
    }

    this.daysGrid = grid;
  }

  // ─── Top emotions (computed from visible month's loaded data) ─────────────────

  calculateLocalTopEmotions() {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth() + 1;
    const counts: { [em: string]: number } = {};

    Object.keys(this.logs).forEach(dateStr => {
      const [y, m] = dateStr.split('-').map(Number);
      if (y !== year || m !== month) return;          // safety check

      this.logs[dateStr].emotions.forEach(em => {
        if (this.emotionOptions.includes(em)) {
          counts[em] = (counts[em] || 0) + 1;
        }
      });
    });

    this.topEmotionsList = Object.keys(counts)
      .map(name => ({ name, count: counts[name] }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);
  }

  // ─── Insights (pattern detection across loaded data) ──────────────────────────

  generateDynamicInsights() {
    const dayScores: { [day: number]: { total: number; count: number } } = {};
    for (let i = 0; i < 7; i++) dayScores[i] = { total: 0, count: 0 };

    Object.keys(this.logs).forEach(dateStr => {
      const log = this.logs[dateStr];
      const opt = this.moodOptions.find(o => o.key === log.mood);
      if (!opt) return;
      const [y, m, d] = dateStr.split('-').map(Number);
      const day = new Date(y, m - 1, d).getDay();
      dayScores[day].total += opt.score;
      dayScores[day].count += 1;
    });

    const dayNames = ['Sundays', 'Mondays', 'Tuesdays', 'Wednesdays', 'Thursdays', 'Fridays', 'Saturdays'];
    let bestDay = -1, bestAvg = 0, worstDay = -1, worstAvg = 6;

    for (let i = 0; i < 7; i++) {
      if (dayScores[i].count > 0) {
        const avg = dayScores[i].total / dayScores[i].count;
        if (avg > bestAvg) { bestAvg = avg; bestDay = i; }
        if (avg < worstAvg) { worstAvg = avg; worstDay = i; }
      }
    }

    const insights = [];

    if (bestDay !== -1 && dayScores[bestDay].count >= 2) {
      insights.push({
        type: 'best-day', title: 'Best Day',
        desc: `Your mood is typically highest on ${dayNames[bestDay]}.`, class: 'insight-green'
      });
    }
    if (worstDay !== -1 && worstDay !== bestDay && dayScores[worstDay].count >= 2) {
      insights.push({
        type: 'watch-out', title: 'Watch Out',
        desc: `${dayNames[worstDay]} tend to show lower mood scores.`, class: 'insight-peach'
      });
    }
    if (insights.length === 0) {
      insights.push({
        type: 'info', title: 'Keep Logging',
        desc: 'Log your mood for a few more days to unlock personalized patterns.', class: 'insight-green'
      });
    }

    this.insightsList = insights;
  }

  // ─── Month navigation ─────────────────────────────────────────────────────────

  prevMonth() {
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() - 1,
      1
    );
    this.loadMonthData(false);
  }

  nextMonth() {
    // Block navigation into the future
    if (this.isNextMonthBlocked()) return;
    this.currentDate = new Date(
      this.currentDate.getFullYear(),
      this.currentDate.getMonth() + 1,
      1
    );
    this.loadMonthData(false);
  }

  isNextMonthBlocked(): boolean {
    const now = new Date();
    return this.currentDate.getFullYear() === now.getFullYear() &&
      this.currentDate.getMonth() === now.getMonth();
  }

  // ─── Day click ───────────────────────────────────────────────────────────────

  selectDay(day: CalendarDay) {
    if (!day.date || !day.isCurrentMonth || day.isFuture || this.isCalendarLoading) return;
    this.selectedDate = day.date;
    this.openLogModalForDate(day.date);
  }

  isSelected(day: CalendarDay): boolean {
    if (!day.date || !day.isCurrentMonth) return false;
    return this.formatDateStr(day.date) === this.formatDateStr(this.selectedDate);
  }

  // ─── Modal ───────────────────────────────────────────────────────────────────

  openLogModal() {
    this.openLogModalForDate(this.selectedDate);
  }

  openLogModalForDate(date: Date, preselectedMoodKey: string | null = null) {
    this.modalDateStr = this.formatDateStr(date);
    this.saveError = '';

    const existing = this.logs[this.modalDateStr];
    if (existing) {
      // Editing an existing log — always use the saved data, ignore preselectedMoodKey
      this.isEditing = true;
      this.modalMood = existing.mood;
      this.modalEmotions = [...existing.emotions];
      this.modalNotes = existing.notes || '';
    } else {
      // New entry — pre-select the mood the user tapped on the dashboard (if any)
      this.isEditing = false;
      this.modalMood = preselectedMoodKey ?? 'okay';
      this.modalEmotions = [];
      this.modalNotes = '';
    }

    this.showLogModal = true;
  }

  closeLogModal() {
    this.showLogModal = false;
    this.isSaving = false;
    this.saveError = '';
  }

  /** Opens the edit modal pre-filled from a history card click */
  openLogModalFromHistory(log: any) {
    const rawDate = log.date || log.loggedAt || '';
    if (!rawDate) return;
    const dateStr = typeof rawDate === 'string' ? rawDate.substring(0, 10) : this.formatDateStr(new Date(rawDate));

    // Determine the mood key from label
    const moodLabel = (log.moodLabel || 'okay').toLowerCase();
    const moodOpt = this.moodOptions.find(o => o.label.toLowerCase() === moodLabel) ||
      this.moodOptions.find(o => o.key === moodLabel);

    this.modalDateStr = dateStr;
    this.modalMood = moodOpt ? moodOpt.key : 'okay';
    this.modalEmotions = Array.isArray(log.emotionTags) ? [...log.emotionTags] : [];
    this.modalNotes = log.notes || '';
    this.isEditing = true;
    this.saveError = '';

    // Also store the moodLogId so saveMoodLog() takes the UPDATE path
    if (log.moodLogId && !this.logs[dateStr]) {
      this.logs[dateStr] = {
        moodLogId: log.moodLogId,
        mood: this.modalMood,
        emotions: this.modalEmotions,
        notes: this.modalNotes
      };
    }

    this.showLogModal = true;
  }

  setModalMood(moodKey: string) {
    this.modalMood = moodKey;
  }

  toggleModalEmotion(em: string) {
    const i = this.modalEmotions.indexOf(em);
    if (i > -1) this.modalEmotions.splice(i, 1);
    else this.modalEmotions.push(em);
  }

  saveMoodLog() {
    if (!this.modalDateStr || this.isSaving) return;
    this.isSaving = true;
    this.saveError = '';

    const opt = this.moodOptions.find(o => o.key === this.modalMood);
    const score = opt ? opt.score : 3;

    const reqData: any = {
      moodScore: score,
      // ISSUE 4 FIX: send null instead of empty string when no emotions selected
      emotionTags: this.modalEmotions.length > 0 ? this.modalEmotions.join(', ') : '',
      stressLevel: 3,    // neutral default — UI slider to be added later
      sleepQuality: 3,
    };

    if (this.modalNotes?.trim()) {
      reqData.notes = this.modalNotes.trim();
    }

    const existingLog = this.logs[this.modalDateStr];

    if (existingLog?.moodLogId) {
      // UPDATE existing log
      this.moodService.updateMood(existingLog.moodLogId, reqData).subscribe({
        next: () => {
          this.loadMonthData(true);   // true = also refresh stats
          this.loadHistory();
          this.closeLogModal();
          this.refreshService.moodLogged();
        },
        error: (err) => {
          console.error('Update error:', err);
          this.saveError = 'Failed to update. Please try again.';
          this.isSaving = false;
        }
      });
    } else {
      // CREATE new log
      // Build loggedAt: parse date parts manually + add current local time
      // This avoids any browser timezone shift and sends correct UTC to backend
      const [y, m, d] = this.modalDateStr.split('-').map(Number);
      const now = new Date();
      const localDate = new Date(y, m - 1, d, now.getHours(), now.getMinutes(), now.getSeconds());
      reqData.loggedAt = localDate.toISOString();

      this.moodService.logMood(reqData).subscribe({
        next: () => {
          this.loadMonthData(true);   // true = also refresh stats
          this.loadHistory();
          this.closeLogModal();
          this.refreshService.moodLogged();
        },
        error: (err) => {
          console.error('Save error:', err);
          this.saveError = 'Failed to save. Please try again.';
          this.isSaving = false;
        }
      });
    }
  }

  // ─── Utilities ───────────────────────────────────────────────────────────────

  // Always uses LOCAL date parts — never UTC methods
  formatDateStr(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  getMonthName(): string {
    return this.currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  getMoodEmoji(moodKey: string | null): string {
    if (!moodKey) return '';
    return this.moodOptions.find(o => o.key === moodKey)?.emoji || '';
  }

  getMoodColor(moodKey: string | null): string {
    if (!moodKey) return 'transparent';
    return this.moodOptions.find(o => o.key === moodKey)?.color || 'transparent';
  }

  getModalDateLabel(): string {
    if (!this.modalDateStr) return '';
    const [y, m, d] = this.modalDateStr.split('-').map(Number);
    return new Date(y, m - 1, d).toLocaleDateString('en-US',
      { weekday: 'long', month: 'long', day: 'numeric' });
  }

  getAvgMoodLabel(): string {
    if (this.avgMoodScore === 0) return 'N/A';
    return `${this.avgMoodScore.toFixed(1)} / 5`;
  }

  // Returns how many days in the current month have been logged
  getLoggedDaysInViewedMonth(): number {
    return Object.keys(this.logs).length;
  }
}