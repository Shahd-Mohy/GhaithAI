import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { GhaithExercise, SelfHelp } from '../../services/self-help';

@Component({
  selector: 'app-tools-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 max-w-6xl mx-auto bg-background min-h-screen text-foreground">

      <div class="mb-8">
        <h1 class="text-2xl font-bold text-slate-800">Self-Help Tools</h1>
        <p class="text-slate-500 text-sm">Evidence-based techniques for your wellbeing</p>
      </div>

      <div class="mb-10">
        <h2 class="text-md font-semibold text-slate-700 mb-4">Quick Relief</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
          @for (tool of quickTools; track tool.title) {
            <div class="border border-slate-200 rounded-xl p-5 bg-white shadow-sm flex flex-col items-center text-center justify-between hover:shadow-md transition-shadow">
              <div class="w-12 h-12 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center text-xl mb-3">
                {{ tool.icon }}
              </div>
              <h3 class="font-semibold text-sm text-slate-800">{{ tool.title }}</h3>
              <p class="text-xs text-slate-400 my-1 line-clamp-1">{{ tool.description }}</p>
              <span class="text-[11px] text-emerald-600 font-medium mt-2">⏱ {{ tool.duration }}</span>
            </div>
          }
        </div>
      </div>

      <div>
        <h2 class="text-md font-semibold text-slate-700 mb-4">Browse by Category</h2>

        @if (isLoading) {
          <div class="flex flex-col items-center justify-center py-10 space-y-2">
            <div class="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-emerald-600"></div>
            <p class="text-xs text-slate-400">Loading your database records...</p>
          </div>
        } @else {

          <div class="border border-slate-200 rounded-xl bg-white mb-6 overflow-hidden shadow-sm">
            <div class="p-4 bg-slate-50/50 border-b border-slate-100 flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center text-lg">💨</div>
              <div>
                <h3 class="font-semibold text-sm text-slate-800">Breathing Exercises</h3>
                <p class="text-xs text-slate-400">Calm your mind with guided breathing techniques</p>
              </div>
            </div>

            <div class="p-5 grid grid-cols-1 md:grid-cols-3 gap-4">
              @for (item of exercises; track item.id) {
                @if (item.type === 'Breathing Exercise') {
                  <div class="border border-slate-100 rounded-xl p-4 bg-slate-50/30 flex flex-col justify-between hover:border-emerald-200 transition-colors">
                    <div>
                      <h4 class="font-semibold text-sm text-slate-800 mb-1">{{ item.title }}</h4>
                      <p class="text-xs text-slate-500 line-clamp-3 mb-4">{{ item.description }}</p>
                    </div>
                    <div class="space-y-3">
                      <div class="flex gap-2 text-[11px] text-slate-400">
                        <span class="bg-white px-2 py-0.5 rounded border border-slate-100">⏱ {{ item.durationMinutes }} min</span>
                        <span class="bg-white px-2 py-0.5 rounded border border-slate-100">★ {{ item.difficultyLevel }}</span>
                      </div>
                      <button class="w-full py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium transition-colors shadow-sm">
                        Start Exercise
                      </button>
                    </div>
                  </div>
                }
              } @empty {
                <p class="text-xs text-slate-400 col-span-3 text-center py-4">No breathing exercises found in DB.</p>
              }
            </div>
          </div>

          <div class="border border-slate-200 rounded-xl bg-white mb-6 overflow-hidden shadow-sm">
            <div class="p-4 bg-slate-50/50 border-b border-slate-100 flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center text-lg">🧠</div>
              <div>
                <h3 class="font-semibold text-sm text-slate-800">Mindfulness & Meditation</h3>
                <p class="text-xs text-slate-400">Practice being present in the moment</p>
              </div>
            </div>

            <div class="p-5 grid grid-cols-1 md:grid-cols-3 gap-4">
              @for (item of exercises; track item.id) {
                @if (item.type === 'Meditation') {
                  <div class="border border-slate-100 rounded-xl p-4 bg-slate-50/30 flex flex-col justify-between hover:border-emerald-200 transition-colors">
                    <div>
                      <h4 class="font-semibold text-sm text-slate-800 mb-1">{{ item.title }}</h4>
                      <p class="text-xs text-slate-500 line-clamp-3 mb-4">{{ item.description }}</p>
                    </div>
                    <div class="space-y-3">
                      <div class="flex gap-2 text-[11px] text-slate-400">
                        <span class="bg-white px-2 py-0.5 rounded border border-slate-100">⏱ {{ item.durationMinutes }} min</span>
                        <span class="bg-white px-2 py-0.5 rounded border border-slate-100">★ {{ item.difficultyLevel }}</span>
                      </div>
                      <button class="w-full py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-medium transition-colors shadow-sm">
                        Listen to Session
                      </button>
                    </div>
                  </div>
                }
              } @empty {
                <p class="text-xs text-slate-400 col-span-3 text-center py-4">No meditation sessions found in DB.</p>
              }
            </div>
          </div>

        }
      </div>
    </div>
  `
})
export class ToolsPageComponent implements OnInit {
  exercises: GhaithExercise[] = [];
  isLoading = true;

  quickTools = [
    { title: 'Quick Calm', description: '60-second anxiety relief', icon: '🌊', duration: '1 min' },
    { title: 'Energy Boost', description: 'Quick energizing exercise', icon: '☀️', duration: '2 min' },
    { title: 'Focus Reset', description: 'Clear your mind and refocus', icon: '🎯', duration: '3 min' },
    { title: 'Nature Sounds', description: 'Calming ambient soundscape', icon: '🍃', duration: '∞' }
  ];

  constructor(private selfHelpService: SelfHelp) { }

  ngOnInit() {
    this.loadExercises();
  }

  loadExercises() {
    this.selfHelpService.getAllExercises().subscribe({
      next: (data) => {
        this.exercises = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('API Error:', err);
        this.isLoading = false;
      }
    });
  }
}
