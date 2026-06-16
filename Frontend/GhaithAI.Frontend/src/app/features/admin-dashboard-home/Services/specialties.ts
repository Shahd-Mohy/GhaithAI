import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateSpecialtyDto, SpecialtyListDto } from '../interfaces/specialty.interface';

@Injectable({
  providedIn: 'root',
})
export class Specialties {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/BaseSpecialty`;

  getAll(): Observable<SpecialtyListDto[]> {
    return this.http.get<SpecialtyListDto[]>(`${this.apiUrl}/getAll/dropDown`).pipe(
      catchError(this.handleError)
    );
  }

  //  getAll(): Observable<LanguageListDto[]> {
  //     return this.http.get<LanguageListDto[]>(`${this.apiUrl}/Dropdown`).pipe(
  //       catchError(this.handleError)
  //     );
  //   }

  create(dto: CreateSpecialtyDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Create_Specialty`, dto).pipe(
      catchError(this.handleError)
    );
  }

  delete(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/Delete_Specialty/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let friendlyMessage = 'Something went wrong; please try again later.';

    if (error.error) {
      if (error.error.message) {
        friendlyMessage = error.error.message;
      } else if (error.error.errors) {
        const errorList = Object.values(error.error.errors).flat();
        friendlyMessage = errorList.join(' | ');
      }
    }

    return throwError(() => new Error(friendlyMessage));
  }
}