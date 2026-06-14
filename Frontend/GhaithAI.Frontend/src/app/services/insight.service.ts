import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface DailyMoodDTO {
  dayLabel: string;
  date: string;
  moodScore: number | null;
}

export interface WeeklySummaryDTO {
  dailyMoods: DailyMoodDTO[];
  avgMoodScore: number;
  avgMoodLabel: string;
  daysLogged: number;
  daysLoggedLabel: string;
}

export interface PersonalInsightDTO {
  text: string;
}

export interface TodaysSuggestionDTO {
  title: string;
  text: string;
  actionLabel: string | null;
  actionRoute: string | null;
}

export interface DashboardViewModel {
  displayName: string;
  todayLabel: string;
  weeklySummary: WeeklySummaryDTO;
  personalInsights: PersonalInsightDTO[];
  todaysSuggestion: TodaysSuggestionDTO | null;
}

// ── Fallback ──────────────────────────────────────────────────────────────────

function buildFallback(displayName: string): DashboardViewModel {
  const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  const today = new Date();
  const dailyMoods: DailyMoodDTO[] = Array.from({ length: 7 }, (_, i) => {
    const d = new Date(today);
    d.setDate(today.getDate() - (6 - i));
    return { dayLabel: days[d.getDay()], date: d.toISOString().split('T')[0], moodScore: null };
  });
  return {
    displayName,
    todayLabel: today.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' }),
    weeklySummary: {
      dailyMoods,
      avgMoodScore: 0, avgMoodLabel: '— / 5',
      daysLogged: 0, daysLoggedLabel: '0 / 7'
    },
    personalInsights: [{ text: "Welcome! Start by logging your first mood — it only takes 10 seconds. 🌱" }],
    todaysSuggestion: {
      title: "Get Started",
      text: "Log your first mood check-in to start tracking your wellbeing and unlock personalised insights.",
      actionLabel: "Log My Mood",
      actionRoute: "mood"
    }
  };
}

/** Read the user's first name from localStorage or JWT token claims */
function resolveDisplayName(): string {
  // ── 1. Try JSON user objects ────────────────────────────────────────────
  for (const key of ['user', 'currentUser', 'authUser', 'profile']) {
    try {
      const raw = localStorage.getItem(key) || sessionStorage.getItem(key);
      if (!raw) continue;
      const p = JSON.parse(raw);
      const name = p?.fullName || p?.name || p?.firstName || p?.displayName || p?.email;
      if (name) return String(name).split(' ')[0];
    } catch {
      // Corrupt entry — remove it so it never crashes again
      try { localStorage.removeItem(key); } catch { /* ignore */ }
    }
  }

  // ── 2. Try JWT token payload ────────────────────────────────────────────
  for (const key of ['token', 'access_token', 'authToken', 'jwt']) {
    try {
      const token = localStorage.getItem(key) || sessionStorage.getItem(key);
      // A valid JWT has exactly 3 dot-separated base64 segments
      if (!token) continue;
      const parts = token.split('.');
      if (parts.length !== 3) continue;
      const payload = JSON.parse(atob(parts[1]));
      const name =
        payload?.given_name ||
        payload?.name ||
        payload?.unique_name ||
        payload?.email ||
        payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] ||
        payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
      if (name) return String(name).split(' ')[0];
    } catch { /* ignore bad token */ }
  }

  return 'there';
}


// ── Service ───────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class InsightService {

  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:53898/api';

  getDashboard(): Observable<DashboardViewModel> {
    return this.http
      .get<any>(`${this.baseUrl}/Insight/dashboard`)
      .pipe(
        map(res => this.mapResponse(res)),
        catchError(err => {
          console.error('[InsightService] getDashboard failed:', err);
          return of(buildFallback(resolveDisplayName()));
        })
      );
  }

  // ── Mapping ───────────────────────────────────────────────────────────────

  private mapResponse(res: any): DashboardViewModel {
    const raw = res?.data ?? res?.Data ?? res?.result ?? res?.Result ?? res;

    const displayName: string = raw?.displayName || raw?.DisplayName || resolveDisplayName();

    const todayLabel: string =
      raw?.todayLabel || raw?.TodayLabel ||
      new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });

    const ws = raw?.weeklySummary || raw?.WeeklySummary || {};
    const dailyMoods = this.mapDailyMoods(ws?.dailyMoods || ws?.DailyMoods);
    const daysLogged = dailyMoods.filter(d => d.moodScore !== null).length;

    let avgScore: number = ws?.avgMoodScore ?? ws?.AvgMoodScore ?? 0;
    if (avgScore === 0 && daysLogged > 0) {
      const sum = dailyMoods.reduce((acc, d) => acc + (d.moodScore ?? 0), 0);
      avgScore = sum / daysLogged;
    }

    const weeklySummary: WeeklySummaryDTO = {
      dailyMoods,
      avgMoodScore: avgScore,
      avgMoodLabel: ws?.avgMoodLabel || ws?.AvgMoodLabel || (avgScore > 0 ? `${Number(avgScore).toFixed(1)} / 5` : '— / 5'),
      daysLogged,
      daysLoggedLabel: ws?.daysLoggedLabel || ws?.DaysLoggedLabel || `${daysLogged} / 7`
    };

    const rawInsights: any[] = raw?.personalInsights || raw?.PersonalInsights || [];
    const personalInsights: PersonalInsightDTO[] = Array.isArray(rawInsights) && rawInsights.length > 0
      ? rawInsights.map(i => ({ text: i?.text || i?.Text || '' })).filter(i => i.text)
      : [{ text: 'Keep logging your mood to unlock personalised insights.' }];

    const rawSug = raw?.todaysSuggestion || raw?.TodaysSuggestion || null;
    const todaysSuggestion: TodaysSuggestionDTO | null = rawSug ? {
      title: rawSug.title || rawSug.Title || "Today's Suggestion",
      text: rawSug.text || rawSug.Text || '',
      actionLabel: rawSug.actionLabel || rawSug.ActionLabel || null,
      actionRoute: rawSug.actionRoute || rawSug.ActionRoute || null,
    } : null;

    return { displayName, todayLabel, weeklySummary, personalInsights, todaysSuggestion };
  }

  private mapDailyMoods(raw: any): DailyMoodDTO[] {
    if (!Array.isArray(raw) || raw.length === 0) {
      const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
      const today = new Date();
      return Array.from({ length: 7 }, (_, i) => {
        const d = new Date(today);
        d.setDate(today.getDate() - (6 - i));
        return { dayLabel: days[d.getDay()], date: d.toISOString().split('T')[0], moodScore: null };
      });
    }
    return raw.map((item: any) => ({
      dayLabel: item?.dayLabel || item?.DayLabel || '',
      date: item?.date || item?.Date || '',
      moodScore: item?.moodScore ?? item?.MoodScore ?? null
    }));
  }
}