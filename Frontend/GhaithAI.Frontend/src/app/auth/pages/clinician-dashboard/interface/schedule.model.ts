export interface ScheduleItem {
  bookingId: string;
  patientName: string;
  sessionType: string;
  time: string;
  status: string;

  initials?: string;
  color?: string;
}
