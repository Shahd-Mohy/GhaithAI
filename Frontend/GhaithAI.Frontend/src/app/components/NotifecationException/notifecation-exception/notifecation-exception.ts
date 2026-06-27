import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNotification, NotificationService } from '../../../services/NotificationExceptionService';

@Component({
  selector: 'app-notifecation-exception',
  standalone: true, // تأكد من إضافة هذه الخاصية
  imports: [CommonModule],
  templateUrl: './notifecation-exception.html',
  styleUrl: './notifecation-exception.css',
})
export class NotifecationException implements OnInit {
  currentNotification: AppNotification | null = null;

  // قمنا بحقن ChangeDetectorRef لإجبار المكون على التحديث
  constructor(
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.notificationService.notification$.subscribe({
      next: (note) => {
        this.currentNotification = note;

        // إجبار الـ UI على التحديث فور وصول البيانات
        this.cdr.detectChanges();

        // إخفاء التنبيه بعد 4 ثوانٍ
        if (note) {
          setTimeout(() => {
            this.currentNotification = null;
            this.cdr.detectChanges();
          }, 4000);
        }
      }
    });
  }

  close() {
    this.currentNotification = null;
  }
}