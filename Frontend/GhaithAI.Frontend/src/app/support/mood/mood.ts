import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MoodService } from '../../services/mood.service';

interface MoodLog {
  moodLogId?: string;
  mood: string;       // 'very-low', 'low', 'okay', 'good', 'great'
  emotions: string[];  // ['Calm', 'Anxious', 'Grateful', 'Tired', etc.]
  notes: string;
}

interface CalendarDay {
  date: Date | null;
  dayNumber: number | null;
  isCurrentMonth: boolean;
  isToday: boolean;
  isFuture: boolean;
  moodKey: string | null;
}

@Component({
  selector: 'app-mood-tracker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mood.html',
  styleUrl: './mood.css'
})
export class MoodTrackerComponent implements OnInit {
  // Calendar month state
  currentDate = new Date();
  selectedDate = new Date();
  daysGrid: CalendarDay[] = [];

  // Logs state
  logs: { [dateStr: string]: MoodLog } = {};

  // Statistics
  avgMoodScore = 0;
  daysLoggedCount = 0;
  currentStreak = 0;
  topEmotion = 'None';
  topEmotionCount = 0;
  moodTrendLabel = '+0.0 from last week';
  moodTrendClass = 'trend-neutral';

  // Insights & Top Emotions
  insightsList: { type: string, title: string, desc: string, class: string }[] = [];
  topEmotionsList: { name: string; count: number }[] = [];

  // Dialog State
  showLogModal = false;
  modalDateStr = '';
  modalMood = 'okay';
  modalEmotions: string[] = [];
  modalNotes = '';
  isEditing = false;

  // Config lists
  moodOptions = [
    { key: 'very-low', label: 'Very Low', emoji: '🌧️', score: 1 },
    { key: 'low', label: 'Low', emoji: '🌥️', score: 2 },
    { key: 'okay', label: 'Okay', emoji: '⛅', score: 3 },
    { key: 'good', label: 'Good', emoji: '🌤️', score: 4 },
    { key: 'great', label: 'Great', emoji: '✨', score: 5 }
  ];

  emotionOptions = [
    'Calm', 'Anxious', 'Grateful', 'Tired',
    'Happy', 'Sad', 'Energetic', 'Stressed',
    'Relaxed', 'Frustrated', 'Excited', 'Lonely'
  ];

  maxDateStr: string = '';

  constructor(private moodService: MoodService) {}

  historyLogs: any[] = [];

  ngOnInit() {
    this.maxDateStr = this.formatDateStr(new Date());
    this.loadLogs();
    this.loadStatistics();
    this.loadHistory();
  }

  loadHistory() {
    this.moodService.getHistory(1, 10).subscribe({
      next: (res) => {
        this.historyLogs = (res.data || res || []).filter((item: any) => item.moodLogId);
      },
      error: (err) => console.error('Error fetching history', err)
    });
  }

  loadStatistics() {
    this.moodService.getStatistics('month').subscribe({
      next: (res) => {
        const data = res.data || res;
        if (data) {
          this.avgMoodScore = data.avgMoodScore || 0;
          this.daysLoggedCount = data.totalLogs || 0;
          this.currentStreak = data.streakDays || 0;
          this.topEmotion = data.topEmotion || 'None';
          this.topEmotionCount = data.topEmotionCount || 0;
          
          const diff = data.changeFromLastWeek || 0;
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
        }
      },
      error: (err) => console.error('Error fetching statistics', err)
    });
  }

  // Load from API, fallback to localStorage
  loadLogs() {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth() + 1; // 1-12

    this.moodService.getCalendar(year, month).subscribe({
      next: (res) => {
        const tempLogs: { [dateStr: string]: MoodLog } = {};
        const dataArr = res.data && Array.isArray(res.data) ? res.data : (Array.isArray(res) ? res : []);
        
        dataArr.forEach((item: any) => {
          const rawDate = item.date || item.loggedAt;
          if (rawDate) {
            const dateStr = rawDate.split('T')[0];
            const moodOpt = this.moodOptions.find(o => o.score === item.moodScore);
            
            let ems: string[] = [];
            if (Array.isArray(item.emotionTags)) {
              ems = item.emotionTags;
            } else if (typeof item.emotionTags === 'string') {
              ems = item.emotionTags.split(',').map((e: string) => e.trim()).filter((e: string) => e.length > 0);
            }

            tempLogs[dateStr] = {
              moodLogId: item.moodLogId,
              mood: moodOpt ? moodOpt.key : 'okay',
              emotions: ems,
              notes: item.notes || ''
            };
          }
        });
        
        this.logs = tempLogs;
        this.buildCalendarGrid();
        this.calculateLocalTopEmotions();
        this.generateDynamicInsights();
      },
      error: (err) => {
        console.error('Error fetching calendar', err);
        const saved = localStorage.getItem('ghaith_mood_logs');
        if (saved) {
          try {
            this.logs = JSON.parse(saved);
          } catch (e) {
            console.error('Error parsing mood logs', e);
            this.logs = {};
          }
        } else {
          this.seedMockData();
        }
        this.buildCalendarGrid();
        this.calculateLocalTopEmotions();
        this.generateDynamicInsights();
      }
    });
  }

  saveLogs() {
    localStorage.setItem('ghaith_mood_logs', JSON.stringify(this.logs));
  }

  // Prepopulate data for demonstration
  seedMockData() {
    const tempLogs: { [dateStr: string]: MoodLog } = {};
    const today = new Date();
    const mockEmotions = ['Calm', 'Anxious', 'Grateful', 'Tired', 'Happy', 'Relaxed', 'Energetic'];
    const moods = ['very-low', 'low', 'okay', 'good', 'great'];

    // Seed around 18 entries in the last 30 days
    for (let i = 1; i <= 30; i++) {
      // 60% chance to log a day
      if (Math.random() > 0.4) {
        const d = new Date(today.getFullYear(), today.getMonth(), today.getDate() - i);
        // Exclude future days (if any logic overflows)
        if (d > today) continue;

        const dateStr = this.formatDateStr(d);
        
        // Random mood (biased towards okay/good/great)
        let mood = 'okay';
        const rand = Math.random();
        if (rand < 0.1) mood = 'very-low';
        else if (rand < 0.25) mood = 'low';
        else if (rand < 0.6) mood = 'okay';
        else if (rand < 0.85) mood = 'good';
        else mood = 'great';

        // 1 or 2 random emotions
        const emotions: string[] = [];
        const numEmotions = Math.floor(Math.random() * 2) + 1;
        for (let e = 0; e < numEmotions; e++) {
          const randEm = mockEmotions[Math.floor(Math.random() * mockEmotions.length)];
          if (!emotions.includes(randEm)) {
            emotions.push(randEm);
          }
        }

        tempLogs[dateStr] = {
          mood,
          emotions,
          notes: 'Felt like a typical day, checked in with GhaithAI.'
        };
      }
    }

    // Always seed today as logged so it displays nicely
    const todayStr = this.formatDateStr(today);
    tempLogs[todayStr] = {
      mood: 'good',
      emotions: ['Calm', 'Grateful'],
      notes: 'Feeling good today, looking forward to using the mood tracker!'
    };

    this.logs = tempLogs;
    this.saveLogs();
  }

  // Math to generate calendar cells
  buildCalendarGrid() {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth();

    const firstDayIndex = new Date(year, month, 1).getDay(); // 0 is Sunday
    const daysInMonthCount = new Date(year, month + 1, 0).getDate();

    const grid: CalendarDay[] = [];
    const today = new Date();
    const todayStr = this.formatDateStr(today);

    // Padding for days of previous month
    for (let i = 0; i < firstDayIndex; i++) {
      grid.push({
        date: null,
        dayNumber: null,
        isCurrentMonth: false,
        isToday: false,
        isFuture: false,
        moodKey: null
      });
    }

    // Days of current month
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
        moodKey: log ? log.mood : null
      });
    }

    // Padding for days of next month to round to 7 cols grid
    const totalCells = Math.ceil(grid.length / 7) * 7;
    while (grid.length < totalCells) {
      grid.push({
        date: null,
        dayNumber: null,
        isCurrentMonth: false,
        isToday: false,
        isFuture: false,
        moodKey: null
      });
    }

    this.daysGrid = grid;
  }

  // Top Emotions (calculated locally from calendar data)
  calculateLocalTopEmotions() {
    const year = this.currentDate.getFullYear();
    const month = this.currentDate.getMonth() + 1;
    
    // Filter logs for this month
    const thisMonthLogs = Object.keys(this.logs).filter(dateStr => {
      const [y, m, d] = dateStr.split('-');
      return parseInt(y, 10) === year && parseInt(m, 10) === month;
    });

    const counts: { [em: string]: number } = {};
    
    thisMonthLogs.forEach(dateStr => {
      const log = this.logs[dateStr];
      if (log.emotions) {
        log.emotions.forEach(em => {
          counts[em] = (counts[em] || 0) + 1;
        });
      }
    });

    // Convert to sorted list
    const list = Object.keys(counts).map(name => ({
      name,
      count: counts[name]
    })).sort((a, b) => b.count - a.count);

    this.topEmotionsList = list.slice(0, 4);
  }

  // Generate dynamic insights from loaded calendar data
  generateDynamicInsights() {
    const dayScores: { [day: number]: { total: number, count: number } } = {};
    for (let i = 0; i < 7; i++) dayScores[i] = { total: 0, count: 0 };
    
    // Analyze all loaded logs
    Object.keys(this.logs).forEach(dateStr => {
      const log = this.logs[dateStr];
      const opt = this.moodOptions.find(o => o.key === log.mood);
      if (opt) {
        // Adjust for timezone issues by parsing parts
        const [y, m, d] = dateStr.split('-');
        const dateObj = new Date(parseInt(y), parseInt(m) - 1, parseInt(d));
        const day = dateObj.getDay();
        dayScores[day].total += opt.score;
        dayScores[day].count += 1;
      }
    });

    const dayNames = ['Sundays', 'Mondays', 'Tuesdays', 'Wednesdays', 'Thursdays', 'Fridays', 'Saturdays'];
    let bestDay = -1;
    let bestAvg = 0;
    let worstDay = -1;
    let worstAvg = 6;

    for (let i = 0; i < 7; i++) {
      if (dayScores[i].count > 0) {
        const avg = dayScores[i].total / dayScores[i].count;
        if (avg > bestAvg) { bestAvg = avg; bestDay = i; }
        if (avg < worstAvg) { worstAvg = avg; worstDay = i; }
      }
    }

    const newInsights = [];

    if (bestDay !== -1 && dayScores[bestDay].count >= 2) {
      newInsights.push({
        type: 'best-day',
        title: 'Best Day',
        desc: `Your mood tends to be highest on ${dayNames[bestDay]}.`,
        class: 'insight-green'
      });
    }

    if (worstDay !== -1 && worstDay !== bestDay && dayScores[worstDay].count >= 2) {
      newInsights.push({
        type: 'watch-out',
        title: 'Watch Out',
        desc: `${dayNames[worstDay]} show a pattern of lower mood scores.`,
        class: 'insight-peach'
      });
    }

    if (newInsights.length === 0) {
      newInsights.push({
        type: 'info',
        title: 'Keep Logging',
        desc: 'Log your mood for a few more days to unlock personalized patterns and insights.',
        class: 'insight-green'
      });
    }
    
    this.insightsList = newInsights;
  }

  // Navigation
  prevMonth() {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1);
    this.loadLogs();
  }

  nextMonth() {
    this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1);
    this.loadLogs();
  }

  selectDay(day: CalendarDay) {
    if (day.date && !day.isFuture) {
      this.selectedDate = day.date;
      // Auto-fill form with existing logs if they click a day
      this.openLogModalForDate(day.date);
    }
  }

  isSelected(day: CalendarDay): boolean {
    if (!day.date) return false;
    return this.formatDateStr(day.date) === this.formatDateStr(this.selectedDate);
  }

  // Modal actions
  openLogModal() {
    this.openLogModalForDate(this.selectedDate);
  }

  openLogModalForDate(date: Date) {
    this.modalDateStr = this.formatDateStr(date);
    
    const existing = this.logs[this.modalDateStr];
    if (existing) {
      this.isEditing = true;
      this.modalMood = existing.mood;
      this.modalEmotions = [...existing.emotions];
      this.modalNotes = existing.notes || '';
    } else {
      this.isEditing = false;
      this.modalMood = 'okay';
      this.modalEmotions = [];
      this.modalNotes = '';
    }

    this.showLogModal = true;
  }

  closeLogModal() {
    this.showLogModal = false;
  }

  setModalMood(moodKey: string) {
    this.modalMood = moodKey;
  }

  toggleModalEmotion(em: string) {
    const index = this.modalEmotions.indexOf(em);
    if (index > -1) {
      this.modalEmotions.splice(index, 1);
    } else {
      this.modalEmotions.push(em);
    }
  }

  saveMoodLog() {
    if (!this.modalDateStr) return;

    const opt = this.moodOptions.find(o => o.key === this.modalMood);
    const score = opt ? opt.score : 3;

    const reqData: any = {
      moodScore: score,
      emotionTags: this.modalEmotions.join(', '),
    };

    // Only send notes if user typed something, preserving old notes if empty
    if (this.modalNotes && this.modalNotes.trim().length > 0) {
      reqData.notes = this.modalNotes;
    }

    const existingLog = this.logs[this.modalDateStr];

    if (existingLog && existingLog.moodLogId) {
      // Update existing mood log
      this.moodService.updateMood(existingLog.moodLogId, reqData).subscribe({
        next: (res) => {
          this.loadLogs();
          this.loadStatistics();
          this.loadHistory();
          this.closeLogModal();
        },
        error: (err) => {
          console.error('Error updating mood log', err);
          this.logs[this.modalDateStr] = {
            moodLogId: existingLog.moodLogId,
            mood: this.modalMood,
            emotions: [...this.modalEmotions],
            notes: this.modalNotes
          };
          this.saveLogs();
          this.buildCalendarGrid();
          this.calculateLocalTopEmotions();
          this.closeLogModal();
        }
      });
    } else {
      // Create new mood log
      reqData.loggedAt = new Date(this.modalDateStr).toISOString();

      this.moodService.logMood(reqData).subscribe({
        next: (res) => {
          this.loadLogs();
          this.loadStatistics();
          this.loadHistory();
          this.closeLogModal();
        },
        error: (err) => {
          console.error('Error saving mood log', err);
          this.logs[this.modalDateStr] = {
            mood: this.modalMood,
            emotions: [...this.modalEmotions],
            notes: this.modalNotes
          };
          this.saveLogs();
          this.buildCalendarGrid();
          this.calculateLocalTopEmotions();
          this.closeLogModal();
        }
      });
    }
  }

  // Utility helpers
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
}
