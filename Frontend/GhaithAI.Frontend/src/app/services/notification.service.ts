import { Injectable, signal, computed, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { NotificationDto, PagedNotificationsResponseDto } from '../models/notification.model';
import { AuthService } from './auth';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection: signalR.HubConnection | null = null;
  private readonly apiUrl = `${environment.apiUrl}/notifications`;
  
  private notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  public startConnection(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/hubs/notifications`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('NotificationHub connection started'))
      .catch(err => console.error('Error while starting NotificationHub connection: ' + err));

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      // Add new notification to the beginning of the list
      const currentNotifications = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...currentNotifications]);
      
      // Increment unread count
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('NotificationHub connection stopped'))
        .catch(err => console.error('Error stopping NotificationHub connection: ' + err));
    }
  }

  public fetchMyNotifications(page: number = 0, size: number = 10): Observable<PagedNotificationsResponseDto> {
    return this.http.get<PagedNotificationsResponseDto>(`${this.apiUrl}?page=${page}&size=${size}`).pipe(
      tap(res => {
        if (page === 0) {
          this.notificationsSubject.next(res.notifications);
        } else {
          const current = this.notificationsSubject.value;
          this.notificationsSubject.next([...current, ...res.notifications]);
        }
        this.unreadCountSubject.next(res.unreadCount);
      })
    );
  }

  public getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/unread-count`).pipe(
      tap(count => this.unreadCountSubject.next(count))
    );
  }

  public markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value.map(n => 
          n.id === id ? { ...n, isRead: true } : n
        );
        this.notificationsSubject.next(notifications);
        this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
      })
    );
  }

  public markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(notifications);
        this.unreadCountSubject.next(0);
      })
    );
  }

  public deleteNotification(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const notification = this.notificationsSubject.value.find(n => n.id === id);
        const notifications = this.notificationsSubject.value.filter(n => n.id !== id);
        this.notificationsSubject.next(notifications);
        
        if (notification && !notification.isRead) {
          this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
        }
      })
    );
  }
}
