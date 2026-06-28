import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNotification, NotificationService } from '../../../services/NotificationExceptionService';

@Component({
  selector: 'app-notifecation-exception',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifecation-exception.html',
  styleUrl: './notifecation-exception.css',
})
export class NotifecationException implements OnInit {
  currentNotification: AppNotification | null = null;


  constructor(
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.notificationService.notification$.subscribe({
      next: (note) => {
        this.currentNotification = note;


        this.cdr.detectChanges();


        if (note) {
          setTimeout(() => {
            this.currentNotification = null;
            this.cdr.detectChanges();
          }, 10000);
        }
      }
    });
  }

  close() {
    this.currentNotification = null;
  }
}
