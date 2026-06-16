import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorService, PublicDoctorProfile, AvailableSlot } from '../../../../services/doctor.service';

@Component({
  selector: 'app-doctor-booking',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctor-booking.html',
  styleUrls: ['./doctor-booking.css']
})
export class DoctorBookingComponent implements OnInit, OnChanges {

  @Input() doctorId!: string;
  @Output() back = new EventEmitter<void>();

  doctor: PublicDoctorProfile | null = null;
  loading = true;
  error = '';

  // Booking state
  selectedDate = '';
  slots: AvailableSlot[] = [];
  slotsLoading = false;
  selectedSlot = '';
  sessionType = 0;
  notes = '';

  bookingLoading = false;
  bookingSuccess = false;
  bookingError = '';

  minDate = new Date().toISOString().split('T')[0];

  constructor(private doctorService: DoctorService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  ngOnChanges(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.error = '';
    this.doctorService.getDoctorProfile(this.doctorId).subscribe({
      next: (data) => { this.doctor = data; this.loading = false; },
      error: () => { this.error = 'Failed to load doctor profile.'; this.loading = false; }
    });
  }

  onDateChange(): void {
    if (!this.selectedDate) return;
    this.slotsLoading = true;
    this.slots = [];
    this.selectedSlot = '';

    this.doctorService.getAvailableSlots(this.doctorId, this.selectedDate).subscribe({
      next: (data) => { this.slots = data; this.slotsLoading = false; },
      error: () => { this.slotsLoading = false; }
    });
  }

  selectSlot(slot: AvailableSlot): void {
    if (!slot.isAvailable) return;
    this.selectedSlot = slot.slotTime;
  }

  get canBook(): boolean {
    return !!this.selectedDate && !!this.selectedSlot;
  }

  book(): void {
    if (!this.canBook) return;

    this.bookingLoading = true;
    this.bookingError = '';

    this.doctorService.bookDoctor({
      doctorId: this.doctorId,
      bookingDate: this.selectedDate,
      slotTime: this.selectedSlot,
      sessionType: this.sessionType,
      bookingNotes: this.notes
    }).subscribe({
      next: () => {
        this.bookingLoading = false;
        this.bookingSuccess = true;
      },
      error: (err) => {
        this.bookingLoading = false;
        this.bookingError = err.error?.message || 'Booking failed. Please try again.';
      }
    });
  }

  getStars(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i < Math.round(rating) ? 1 : 0);
  }

  getDayLabel(day: string): string {
    const days: Record<string, string> = {
      '0': 'Sun', '1': 'Mon', '2': 'Tue', '3': 'Wed',
      '4': 'Thu', '5': 'Fri', '6': 'Sat'
    };
    return days[day] ?? day;
  }
}