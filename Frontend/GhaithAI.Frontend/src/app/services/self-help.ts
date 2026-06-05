import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';

export interface GhaithExercise {
  id: string;
  title: string;
  type: string;
  description: string;
  contentUrl: string;
  durationMinutes: number;
  difficultyLevel: string;
}
@Injectable({
  providedIn: 'root',
})
export class SelfHelp {

  private apiUrl = 'https://localhost:53898/api/SelfHelp';

  constructor(private http: HttpClient) { }

  getAllExercises(): Observable<GhaithExercise[]> {
    return this.http.get<GhaithExercise[]>(this.apiUrl);
  }
  getExerciseById(id: string): Observable<GhaithExercise> {
    return this.http.get<GhaithExercise>(`${this.apiUrl}/${id}`);
  }

}
