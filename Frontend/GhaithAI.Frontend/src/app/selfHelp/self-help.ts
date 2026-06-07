import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelfHelp as SelfHelpService, GhaithExercise } from '../services/self-help';
import { ExerciseCard } from './exercise-card/exercise-card';
import { Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-self-help-dashboard',
  standalone: true,
  imports: [CommonModule, ExerciseCard],
  templateUrl: './self-help.html',
  styleUrl: './self-help.css'
})
export class SelfHelpComponent implements OnInit {
  exercises: GhaithExercise[] = [];
  categoriesList: any[] = [];
  isLoading = true;
  activeCategoryName: string | null = null;

  categoryMeta: { [key: string]: { icon: string, class: string, desc: string } } = {
    'Breathing Exercise': {
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M17.7 7.7a2.5 2.5 0 1 1 1.8 4.3H2"/>
      <path d="M9.6 4.6A2 2 0 1 1 11 8H2"/>
      <path d="M12.6 19.4A2 2 0 1 0 14 16H2"/>
    </svg>`,
      class: 'breathing-bg',
      desc: 'Calm your mind with guided breathing techniques'
    },
    'Meditation': {
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M12 5a3 3 0 1 0-5.997.125 4 4 0 0 0-2.526 5.77 4 4 0 0 0 .556 6.588A4 4 0 1 0 12 18Z"/>
      <path d="M12 5a3 3 0 1 1 5.997.125 4 4 0 0 1 2.526 5.77 4 4 0 0 1-.556 6.588A4 4 0 1 1 12 18Z"/>
      <path d="M15 13a4.5 4.5 0 0 1-3-4 4.5 4.5 0 0 1-3 4"/>
      <path d="M17.599 6.5a3 3 0 0 0 .399-1.375"/>
      <path d="M6.003 5.125A3 3 0 0 0 6.401 6.5"/>
      <path d="M3.477 10.896a4 4 0 0 1 .585-.396"/>
      <path d="M19.938 10.5a4 4 0 0 1 .585.396"/>
      <path d="M6 18a4 4 0 0 1-1.967-.516"/>
      <path d="M19.967 17.484A4 4 0 0 1 18 18"/>
    </svg>`,
      class: 'mindfulness-bg',
      desc: 'Practice being present in the moment'
    },
    'Relaxation': {
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/>
    </svg>`,
      class: 'relaxation-bg',
      desc: 'Techniques to help you unwind and de-stress'
    }
  };
  get quickTools() {
    if (!this.exercises || this.exercises.length === 0) {
      return [
        {
          id: '1', title: 'Quick Calm', description: '60-second anxiety relief', duration: '1 min',
          icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M2 6c.6.5 1.2 1 2.5 1C7 7 7 5 9.5 5c2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2"/>
          <path d="M2 12c.6.5 1.2 1 2.5 1 2.5 0 2.5-2 5-2 2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2"/>
          <path d="M2 18c.6.5 1.2 1 2.5 1 2.5 0 2.5-2 5-2 2.6 0 2.4 2 5 2 2.5 0 2.5-2 5-2"/>
        </svg>`
        },
        {
          id: '2', title: 'Energy Boost', description: 'Quick energizing exercise', duration: '2 min',
          icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="4"/>
          <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41"/>
        </svg>`
        },
        {
          id: '3', title: 'Focus Reset', description: 'Clear your mind and refocus', duration: '3 min',
          icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10"/>
          <circle cx="12" cy="12" r="6"/>
          <circle cx="12" cy="12" r="2"/>
        </svg>`
        },
        {
          id: '4', title: 'Nature Sounds', description: 'Calming ambient soundscape', duration: '∞',
          icon: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.48 19 2c1 2 2 4.18 2 8 0 5.5-4.78 10-10 10Z"/>
          <path d="M2 21c0-3 1.85-5.36 5.08-6C9.5 14.52 12 13 13 12"/>
        </svg>`
        },
      ];
    }
    return this.exercises.slice(0, 4).map(item => ({
      id: item.id,
      title: item.title,
      description: item.description,
      icon: this.categoryMeta[item.type]?.icon || '',
      duration: `${item.durationMinutes} min`
    }));
  }

  constructor(
    private selfHelpService: SelfHelpService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private sanitizer: DomSanitizer
  ) { }

  getSafeIcon(icon: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(icon);
  }
  ngOnInit() {
    this.loadExercises();
  }

  loadExercises() {
    this.isLoading = true;
    this.selfHelpService.getAllExercises().subscribe({
      next: (data) => {
        console.log('API Response received:', data);
        this.exercises = Array.isArray(data) ? data : (data as any).data || [];

        if (this.exercises.length > 0) {
          const types = [...new Set(this.exercises.map(item => item.type))];
          this.categoriesList = types.map(type => ({
            name: type,
            icon: this.categoryMeta[type]?.icon || '✨',
            bgClass: this.categoryMeta[type]?.class || 'relaxation-bg',
            description: this.categoryMeta[type]?.desc || 'Evidence-based techniques for your wellbeing',
            count: this.exercises.filter(e => e.type === type).length,
            subExercises: this.exercises.filter(e => e.type === type)
          }));
        } else {
          this.categoriesList = [];
        }

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('API Error:', err);
        this.exercises = [];
        this.categoriesList = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
  toggleCategory(categoryName: string) {
    if (this.activeCategoryName === categoryName) {
      this.activeCategoryName = null;
    } else {
      this.activeCategoryName = categoryName;
    }
  }

  goToExercise(exerciseId: string) {
    if (!exerciseId || exerciseId.startsWith('00000000')) return;
    this.router.navigate(['dashboard/self-help/exercise', exerciseId]);
  }
}
