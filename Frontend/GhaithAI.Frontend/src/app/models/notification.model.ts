export enum NotificationType {
  BookingConfirmed = 'BookingConfirmed',
  BookingCancelled = 'BookingCancelled',
  BookingCancelledByDoctor = 'BookingCancelledByDoctor',
  BookingCancelledByPatient = 'BookingCancelledByPatient',
  AppointmentReminder = 'AppointmentReminder',
  SessionStarted = 'SessionStarted',
  SessionReportReady = 'SessionReportReady',
  NewBooking = 'NewBooking',
  ProfileApproved = 'ProfileApproved',
  ProfileRejected = 'ProfileRejected',
  NewDoctorRegistration = 'NewDoctorRegistration',
  NewReportFlagged = 'NewReportFlagged'
}

export interface NotificationDto {
  id: string;
  title: string;
  body: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: Date;
  targetId?: string;
}

export interface PagedNotificationsResponseDto {
  notifications: NotificationDto[];
  totalCount: number;
  unreadCount: number;
}
