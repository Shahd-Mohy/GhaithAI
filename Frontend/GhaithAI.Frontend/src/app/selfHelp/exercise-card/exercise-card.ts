import { Component, Input } from '@angular/core';
import { GhaithExercise } from '../../services/self-help';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-exercise-card',
  imports: [CommonModule],
  templateUrl: './exercise-card.html',
  styleUrl: './exercise-card.css',
})
export class ExerciseCard {
  @Input({ required: true }) exercise!: GhaithExercise;
}
