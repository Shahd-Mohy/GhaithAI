import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelfHelp as SelfHelpService, GhaithExercise } from '../services/self-help';
import { ExerciseCard } from './exercise-card/exercise-card';
import { Router } from '@angular/router';

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

  // quickTools = [
  //   { title: 'Quick Calm', description: '60-second anxiety relief', icon: '🌊', duration: '1 min' },
  //   { title: 'Energy Boost', description: 'Quick energizing exercise', icon: '☀️', duration: '2 min' },
  //   { title: 'Focus Reset', description: 'Clear your mind and refocus', icon: '🎯', duration: '3 min' },
  //   { title: 'Nature Sounds', description: 'Calming ambient soundscape', icon: '🍃', duration: '∞' }
  // ];
  categoryMeta: { [key: string]: { icon: string, class: string, desc: string } } = {
    'Breathing Exercise': { icon: '💨', class: 'breathing-bg', desc: 'Calm your mind with guided breathing techniques' },
    'Meditation': { icon: '🧠', class: 'mindfulness-bg', desc: 'Practice being present in the moment' },
    'Relaxation': { icon: '🌙', class: 'relaxation-bg', desc: 'Techniques to help you unwind and de-stress' }
  };
  get quickTools() {
    if (!this.exercises || this.exercises.length === 0) {
      return [
        { id: '00000000-0000-0000-0000-000000000001', title: 'Quick Calm', description: '60-second anxiety relief', icon: '🌊', duration: '1 min' },
        { id: '00000000-0000-0000-0000-000000000002', title: 'Energy Boost', description: 'Quick energizing exercise', icon: '☀️', duration: '2 min' },
        { id: '00000000-0000-0000-0000-000000000003', title: 'Focus Reset', description: 'Clear your mind and refocus', icon: '🎯', duration: '3 min' },
        { id: '00000000-0000-0000-0000-000000000004', title: 'Nature Sounds', description: 'Calming ambient soundscape', icon: '🍃', duration: '∞' }
      ];
    }
    return this.exercises.slice(0, 4).map(item => ({
      id: item.id,
      title: item.title,
      description: item.description,
      icon: this.categoryMeta[item.type]?.icon || '✨',
      duration: `${item.durationMinutes} min`
    }));
  }

  // get categories() {
  //   if (!this.exercises || !Array.isArray(this.exercises) || this.exercises.length === 0) {
  //     return [];
  //   }

  //   const types = [...new Set(this.exercises.map(item => item.type))];
  //   return types.map(type => ({
  //     name: type,
  //     icon: this.categoryMeta[type]?.icon || '✨',
  //     bgClass: this.categoryMeta[type]?.class || 'relaxation-bg',
  //     description: this.categoryMeta[type]?.desc || 'Evidence-based techniques for your wellbeing',
  //     count: this.exercises.filter(e => e.type === type).length
  //   }));
  // }
  constructor(
    private selfHelpService: SelfHelpService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) { }
  ngOnInit() {
    this.loadExercises();
  }

  loadExercises() {
    this.isLoading = true;
    this.selfHelpService.getAllExercises().subscribe({
      next: (data) => {
        console.log('API Response received:', data);
        this.exercises = Array.isArray(data) ? data : (data as any).data || [];

        // 🌟 بناء الـ Categories بشكل مباشر وصريح هنا فوراً
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
        this.cdr.detectChanges(); // 🌟 إجبار الأنجلر على تحديث الـ UI حالاً
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
      this.activeCategoryName = null; // لو مضغوط عليها يقفلها
    } else {
      this.activeCategoryName = categoryName; // يفتح الفئة الجديدة
    }
  }
  goToExercise(exerciseId: string) {
    if (!exerciseId) return;
    this.router.navigate(['dashboard/self-help/exercise', exerciseId]);
  }
}
