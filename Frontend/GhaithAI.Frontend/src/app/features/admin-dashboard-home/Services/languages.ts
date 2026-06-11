import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateLanguageDto, LanguageListDto } from '../interfaces/language.interface';
@Injectable({
  providedIn: 'root',
})
export class Languages {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/BaseLanguage`;

  getAll(): Observable<{ success: boolean; data: LanguageListDto[] }> {
    return this.http.get<{ success: boolean; data: LanguageListDto[] }>(`${this.apiUrl}/Dropdown`).pipe(
      catchError(this.handleError)
    );
  }

  create(dto: CreateLanguageDto): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/Create_Language`, dto).pipe(
      catchError(this.handleError)
    );
  }

  delete(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/Delete_Language/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let friendlyMessage = 'Something went wrong; please try again later.';

    if (error.error) {
      if (error.error.message) {
        friendlyMessage = error.error.message;
      }
      else if (error.error.errors) {
        const errorList = Object.values(error.error.errors).flat();
        friendlyMessage = errorList.join(' | ');
      }
    }

    return throwError(() => new Error(friendlyMessage));
  }
}
