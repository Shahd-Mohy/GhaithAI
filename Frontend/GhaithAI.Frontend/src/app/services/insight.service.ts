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
  avgMoodLabel: string;    // "3.0 / 5"
  daysLogged: number;
  daysLoggedLabel: string;    // "6 / 7"
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
    personalInsights: [{ text: 'Start logging your mood to unlock personalised insights.' }],
    todaysSuggestion: {
      title: "Today's Suggestion",
      text: "Take a moment to check in with yourself. Logging your mood takes less than 30 seconds.",
      actionLabel: null,
      actionRoute: null
    }
  };
}

// ── Service ───────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class InsightService {

  private readonly http = inject(HttpClient);
  // CORRECT endpoint — matches GET /api/Insight/dashboard in Swagger
  private readonly baseUrl = 'https://localhost:53898/api';

  getDashboard(): Observable<DashboardViewModel> {
    return this.http
      .get<any>(`${this.baseUrl}/Insight/dashboard`)
      .pipe(
        map(res => this.mapResponse(res)),
        catchError(err => {
          console.error('[InsightService] getDashboard failed:', err);
          let name = 'User';
          try {
            const stored = localStorage.getItem('user');
            if (stored) {
              const p = JSON.parse(stored);
              name = p.fullName || p.name || p.email || 'User';
            }
          } catch { /* ignore */ }
          return of(buildFallback(name));
        })
      );
  }

  // ── Mapping ───────────────────────────────────────────────────────────────

  private mapResponse(res: any): DashboardViewModel {
    // Unwrap envelope: { data: {...}, success: true, ... }
    const raw = res?.data ?? res?.Data ?? res?.result ?? res?.Result ?? res;

    const displayName: string =
      raw?.displayName || raw?.DisplayName || 'User';

    const todayLabel: string =
      raw?.todayLabel || raw?.TodayLabel ||
      new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });

    // ── Weekly summary ──
    const ws = raw?.weeklySummary || raw?.WeeklySummary || {};
    const dailyMoods = this.mapDailyMoods(ws?.dailyMoods || ws?.DailyMoods);
    const daysLogged = dailyMoods.filter(d => d.moodScore !== null).length;

    // Use API's avgMoodScore if present; otherwise compute it from dailyMoods
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

    // ── Personal insights ──
    const rawInsights: any[] = raw?.personalInsights || raw?.PersonalInsights || [];
    const personalInsights: PersonalInsightDTO[] = Array.isArray(rawInsights) && rawInsights.length > 0
      ? rawInsights.map(i => ({ text: i?.text || i?.Text || '' })).filter(i => i.text)
      : [{ text: 'Keep logging your mood to unlock personalised insights.' }];

    // ── Today's suggestion ──
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