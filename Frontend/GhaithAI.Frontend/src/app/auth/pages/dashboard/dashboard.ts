import { Component, OnInit, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

interface NavItem {
  label: string;
  page: string;
  icon: string;
}

interface QuickAction {
  name: string;
  desc: string;
  page: string;
  colorClass: string;
  icon: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit, AfterViewInit {

  @ViewChild('moodChart') moodChartRef!: ElementRef<HTMLCanvasElement>;

  activePage = 'home';
  selectedMood: string | null = null;
  today = new Date();

  get todayLabel(): string {
    return this.today.toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
  }

  navItemsMain: NavItem[] = [
    { label: 'Home',           page: 'home',          icon: 'home'          },
    { label: 'Talk to AI',     page: 'chat',          icon: 'chat'          },
    { label: 'Mood Tracker',   page: 'mood',          icon: 'mood'          },
    { label: 'Journal',        page: 'journal',       icon: 'journal'       },
    { label: 'Self-Help Tools',page: 'tools',         icon: 'tools'         },
    { label: 'Learn',          page: 'learn',         icon: 'learn'         },
  ];

  navItemsHelp: NavItem[] = [
    { label: 'Find a Professional', page: 'professionals', icon: 'professionals' },
    { label: 'Crisis Support',      page: 'crisis',        icon: 'crisis'        },
  ];

  moods = [
    { key: 'very-low', emoji: '🌧️', label: 'Very Low' },
    { key: 'low',      emoji: '🌥️', label: 'Low'      },
    { key: 'okay',     emoji: '⛅',  label: 'Okay'     },
    { key: 'good',     emoji: '🌤️', label: 'Good'     },
    { key: 'great',    emoji: '✨',  label: 'Great'    },
  ];

  quickActions: QuickAction[] = [
    { name: 'Talk to AI',         desc: 'Have a supportive conversation', page: 'chat',     colorClass: 'teal',   icon: 'chat'     },
    { name: 'Breathing Exercise', desc: 'Quick 4-7-8 technique',          page: 'breathing',colorClass: 'green',  icon: 'breath'   },
    { name: 'Journal Entry',      desc: 'Write your thoughts',            page: 'journal',  colorClass: 'amber',  icon: 'journal'  },
    { name: 'Learn Something',    desc: 'Explore psychoeducation',        page: 'learn',    colorClass: 'purple', icon: 'learn'    },
  ];

  insights = [
    'Your mood tends to improve on days you exercise',
    "You've logged 5 days in a row — great consistency!",
    'Writing in your journal helps reduce anxiety levels',
  ];

  // Chart data
  moodData    = [3, 4, 2, 5, 3, 4, 4];
  moodDays    = ['Mon','Tue','Wed','Thu','Fri','Sat','Sun'];

  ngOnInit() {}

  ngAfterViewInit() {
    this.drawMoodChart();
  }

  navigate(page: string) {
    this.activePage = page;
  }

  selectMood(key: string) {
    this.selectedMood = key;
  }

  isActive(page: string) { return this.activePage === page; }

  drawMoodChart() {
    const canvas = this.moodChartRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const W = canvas.offsetWidth || 400;
    const H = 160;
    canvas.width  = W;
    canvas.height = H;

    const pad   = { top: 16, right: 16, bottom: 32, left: 24 };
    const data  = this.moodData;
    const days  = this.moodDays;
    const maxV  = 5;
    const cW    = (W - pad.left - pad.right) / (data.length - 1);
    const cH    = H - pad.top - pad.bottom;

    const xOf = (i: number) => pad.left + i * cW;
    const yOf = (v: number) => pad.top + cH - (v / maxV) * cH;

    // gradient fill
    const grad = ctx.createLinearGradient(0, pad.top, 0, H - pad.bottom);
    grad.addColorStop(0,   'rgba(11,143,172,.18)');
    grad.addColorStop(1,   'rgba(11,143,172,0)');

    ctx.beginPath();
    ctx.moveTo(xOf(0), yOf(data[0]));
    for (let i = 1; i < data.length; i++) {
      const cpx = (xOf(i - 1) + xOf(i)) / 2;
      ctx.bezierCurveTo(cpx, yOf(data[i-1]), cpx, yOf(data[i]), xOf(i), yOf(data[i]));
    }
    ctx.lineTo(xOf(data.length - 1), H - pad.bottom);
    ctx.lineTo(xOf(0), H - pad.bottom);
    ctx.closePath();
    ctx.fillStyle = grad;
    ctx.fill();

    // line
    ctx.beginPath();
    ctx.moveTo(xOf(0), yOf(data[0]));
    for (let i = 1; i < data.length; i++) {
      const cpx = (xOf(i - 1) + xOf(i)) / 2;
      ctx.bezierCurveTo(cpx, yOf(data[i-1]), cpx, yOf(data[i]), xOf(i), yOf(data[i]));
    }
    ctx.strokeStyle = '#0B8FAC';
    ctx.lineWidth   = 2.5;
    ctx.stroke();

    // dots
    data.forEach((v, i) => {
      ctx.beginPath();
      ctx.arc(xOf(i), yOf(v), 4, 0, Math.PI * 2);
      ctx.fillStyle   = '#0B8FAC';
      ctx.fill();
      ctx.strokeStyle = '#fff';
      ctx.lineWidth   = 2;
      ctx.stroke();
    });

    // day labels
    ctx.fillStyle  = '#64748B';
    ctx.font       = '11px Sora, sans-serif';
    ctx.textAlign  = 'center';
    days.forEach((d, i) => ctx.fillText(d, xOf(i), H - 8));
  }
}