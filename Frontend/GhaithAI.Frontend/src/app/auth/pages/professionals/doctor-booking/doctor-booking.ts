import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { timeout, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { DoctorService, PublicDoctorProfile, AvailableSlot } from '../../../../services/doctor.service';
import { PaymentService } from '../../../../services/payment.service';

@Component({
  selector: 'app-doctor-booking',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctor-booking.html',
  styleUrls: ['./doctor-booking.css']
})
export class DoctorBookingComponent implements OnInit {

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

  constructor(private doctorService: DoctorService, private cdr: ChangeDetectorRef, private paymentService: PaymentService) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.error = '';
    this.doctor = null;

    this.doctorService.getDoctorProfile(this.doctorId).pipe(
      timeout(10000),
      catchError(() => of(null))
    ).subscribe((data) => {
      this.loading = false;
      if (data) {
        this.doctor = data;
        if (data.availableSessionType === 'Online') this.sessionType = 1;
        else if (data.availableSessionType === 'InPerson') this.sessionType = 0;
      } else {
        this.error = 'Failed to load doctor profile. Please try again.';
      }
      this.cdr.detectChanges();
    });
  }

  onDateChange(): void {
    if (!this.selectedDate) return;
    this.slotsLoading = true;
    this.slots = [];
    this.selectedSlot = '';

    this.doctorService.getAvailableSlots(this.doctorId, this.selectedDate).pipe(
      timeout(10000),
      catchError(() => of([]))
    ).subscribe((data) => {
      this.slots = data;
      this.slotsLoading = false;
      this.cdr.detectChanges();
    });
  }

  selectSlot(slot: AvailableSlot): void {
    this.selectedSlot = slot.startTime;
  }

  get canBook(): boolean {
    return !!this.selectedDate && !!this.selectedSlot;
  }



book(): void {
  if (!this.canBook) return;

  this.bookingLoading = true;
  this.bookingError = '';

  // 1 - عمل الـ booking
  this.doctorService.bookDoctor({
    doctorId: this.doctorId,
    bookingDate: this.selectedDate,
    slotTime: this.selectedSlot,
    sessionType: this.sessionType,
    bookingNotes: this.notes
  }).subscribe({
    next: (res) => {
      // 2 - بعد الـ booking، ابدأ الـ payment
      this.paymentService.initiatePayment(res.id).subscribe({
        next: (paymentRes) => {
          this.bookingLoading = false;
          // 3 - حول المريض لـ Stripe
          this.paymentService.redirectToCheckout(paymentRes.checkoutUrl);
        },
        error: (err) => {
          this.bookingLoading = false;
          this.bookingError = err.error?.message || 'Payment initiation failed.';
        }
      });
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
    const map: Record<string, string> = {
      'Sunday': 'Sun', 'Monday': 'Mon', 'Tuesday': 'Tue',
      'Wednesday': 'Wed', 'Thursday': 'Thu', 'Friday': 'Fri', 'Saturday': 'Sat'
    };
    return map[day] ?? day.slice(0, 3);
  }
}