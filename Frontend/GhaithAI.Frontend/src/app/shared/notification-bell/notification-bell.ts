import { Component, OnInit, OnDestroy, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NotificationService } from '../../services/notification.service';
import { NotificationDto, NotificationType } from '../../models/notification.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notification-bell.html',
  styleUrls: ['./notification-bell.css']
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications: NotificationDto[] = [];
  unreadCount = 0;
  isDropdownOpen = false;
  private subs: Subscription = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private eRef: ElementRef
  ) {}

  ngOnInit(): void {
    // Start SignalR Connection
    this.notificationService.startConnection();

    // Subscribe to notifications and unread count
    this.subs.add(
      this.notificationService.notifications$.subscribe(n => {
        this.notifications = n;
      })
    );

    this.subs.add(
      this.notificationService.unreadCount$.subscribe(count => {
        this.unreadCount = count;
      })
    );

    // Initial fetch
    this.notificationService.fetchMyNotifications(0, 10).subscribe();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.notificationService.stopConnection();
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if(!this.eRef.nativeElement.contains(event.target)) {
      this.isDropdownOpen = false;
    }
  }

  markAsRead(notification: NotificationDto, event: Event): void {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }
  }

  markAllAsRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllAsRead().subscribe();
  }

  deleteNotification(id: string, event: Event): void {
    event.stopPropagation();
    this.notificationService.deleteNotification(id).subscribe();
  }

  getIconForType(type: NotificationType): string {
    switch (type) {
      case NotificationType.BookingConfirmed:
      case NotificationType.NewBooking:
        return 'bi-calendar-check text-success';
      case NotificationType.BookingCancelled:
      case NotificationType.BookingCancelledByDoctor:
      case NotificationType.BookingCancelledByPatient:
        return 'bi-calendar-x text-danger';
      case NotificationType.AppointmentReminder:
        return 'bi-alarm text-warning';
      case NotificationType.SessionStarted:
        return 'bi-camera-video text-primary';
      case NotificationType.SessionReportReady:
        return 'bi-file-earmark-medical text-info';
      case NotificationType.ProfileApproved:
        return 'bi-person-check text-success';
      case NotificationType.ProfileRejected:
        return 'bi-person-x text-danger';
      case NotificationType.NewDoctorRegistration:
        return 'bi-person-plus text-primary';
      case NotificationType.NewReportFlagged:
        return 'bi-flag text-danger';
      default:
        return 'bi-bell text-secondary';
    }
  }
}
