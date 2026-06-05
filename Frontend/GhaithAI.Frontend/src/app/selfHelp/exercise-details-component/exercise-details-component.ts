import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GhaithExercise, SelfHelp as SelfHelpService } from '../../services/self-help';
@Component({
  selector: 'app-exercise-details-component',
  imports: [],
  templateUrl: './exercise-details-component.html',
  styleUrl: './exercise-details-component.css',
})
export class ExerciseDetailsComponent implements OnInit {
  exercise: GhaithExercise | null = null; constructor(
    private route: ActivatedRoute,
    private selfHelpService: SelfHelpService
  ) { }
  ngOnInit() {
    // لقط الـ id من الـ URL
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      // مناداة الدالة بعد ما ضفناها في الـ Service بنجاح
      this.selfHelpService.getExerciseById(id).subscribe({
        next: (data) => {
          this.exercise = data;
        },
        error: (err) => {
          console.error('Error fetching exercise details:', err);
        }
      });
    }
  }
}

