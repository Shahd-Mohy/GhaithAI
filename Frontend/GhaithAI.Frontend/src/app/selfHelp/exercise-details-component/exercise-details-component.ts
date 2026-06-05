import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { GhaithExercise, SelfHelp as SelfHelpService } from '../../services/self-help';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-exercise-details-component',
  imports: [CommonModule, RouterModule],
  templateUrl: './exercise-details-component.html',
  styleUrl: './exercise-details-component.css',
})
export class ExerciseDetailsComponent implements OnInit, OnDestroy {
  exercise: GhaithExercise | null = null;
  isLoading = true;

  timeLeft: number = 0;              // الوقت المتبقي بالثواني
  formattedTime: string = '00:00';    // الوقت بالشكل المنسق (MM:SS)
  timerInterval: any;
  isRunning: boolean = false;

  isCompleted: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private selfHelpService: SelfHelpService,
    private cdr: ChangeDetectorRef
  ) { }



  ngOnInit() {
    this.route.paramMap.subscribe({
      next: (params) => {
        const id = params.get('id');
        if (id) {
          this.fetchExerciseData(id);
        }
      }
    });
  }

  fetchExerciseData(id: string) {
    this.isLoading = true;
    this.selfHelpService.getExerciseById(id).subscribe({
      next: (data) => {
        this.exercise = data;
        this.isLoading = false;

        // 🌟 بنجهز الوقت المبدئي فقط بناءً على الداتابيز بدون ما نشغله تلقائياً
        if (this.exercise && this.exercise.durationMinutes) {
          this.timeLeft = this.exercise.durationMinutes * 60;
          this.updateFormattedTime();
        }

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('❌ API Error:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // 🌟 1. زرار التشغيل (Start)
  startTimer() {
    if (this.isRunning) return;

    this.isRunning = true;
    this.isCompleted = false; // لو بدأنا، نضمن إن حالة الاكتمال ملغية

    this.timerInterval = setInterval(() => {
      if (this.timeLeft > 0) {
        this.timeLeft--;
        this.updateFormattedTime();
      } else {
        // 🌟 أول ما التايمر يصفر
        this.completeExercise();
      }
      this.cdr.detectChanges();
    }, 1000);
  }
  completeExercise() {
    this.isRunning = false;
    this.isCompleted = true; // قلب الحالة لـ مكتمل فوراً
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  // 🌟 2. زرار الإيقاف المؤقت (Pause)
  pauseTimer() {
    this.isRunning = false;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
    this.cdr.detectChanges();
  }

  // 🌟 3. زرار إعادة التعيين (Reset)
  resetTimer() {
    this.pauseTimer();
    this.isCompleted = false; // ريست لحالة الاكتمال
    if (this.exercise && this.exercise.durationMinutes) {
      this.timeLeft = this.exercise.durationMinutes * 60;
      this.updateFormattedTime();
    }
    this.cdr.detectChanges();
  }
  updateFormattedTime() {
    const minutes = Math.floor(this.timeLeft / 60);
    const seconds = this.timeLeft % 60;
    const strMinutes = minutes < 10 ? '0' + minutes : minutes;
    const strSeconds = seconds < 10 ? '0' + seconds : seconds;
    this.formattedTime = `${strMinutes}:${strSeconds}`;
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }
}

