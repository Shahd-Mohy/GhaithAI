import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

export interface AppNotification {
  message: string;
  type: 'error' | 'success' | 'info' | 'warning'; // ضفنا warning
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private notificationSubject = new Subject<AppNotification>();
  notification$ = this.notificationSubject.asObservable();

  show(message: string, type: 'error' | 'success' | 'info' | 'warning' = 'error') {
    this.notificationSubject.next({ message, type });
  }
}
