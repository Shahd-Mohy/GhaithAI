import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../enviroments/enviroment';
import { HttpClient } from '@angular/common/http';
import { AdminSelfHelpDetailsDto, AdminSelfHelpGetAllDto, AdminSelfHelpSaveDto, AdminSelfHelpUpdateDto } from '../interfaces/self-help.interface';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SelfHelp {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/SelfHelpAdmin`;

  getAllItems(pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Get_All`, {
      params: {
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
      }
    });
  }

  getItemById(id: string): Observable<AdminSelfHelpDetailsDto> {
    return this.http.get<AdminSelfHelpDetailsDto>(`${this.apiUrl}/Get_Content_Details/${id}`);
  }
  createItem(dto: AdminSelfHelpSaveDto): Observable<AdminSelfHelpDetailsDto> {
    return this.http.post<AdminSelfHelpDetailsDto>(`${this.apiUrl}/Create_SelfHelp_Content`, dto);
  }

  updateItem(dto: AdminSelfHelpUpdateDto): Observable<{ success: boolean, message: string }> {
    return this.http.put<{ success: boolean, message: string }>(`${this.apiUrl}/Update_SelfHelp_Content`, dto);
  }

  deleteItem(id: string): Observable<{ success: boolean, message: string }> {
    return this.http.delete<{ success: boolean, message: string }>(`${this.apiUrl}/Delete_SelfHelp_Content/${id}`);
  }
}
