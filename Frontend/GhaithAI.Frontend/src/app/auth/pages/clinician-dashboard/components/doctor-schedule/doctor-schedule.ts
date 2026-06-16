import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; // 👈 ضفنا ChangeDetectorRef هنا
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DoctorBookingResponseDto, TimeFilterType } from './interface/DoctorBookingResponseDto';
import { DoctorScheduleService } from './services/doctor-schedule';

@Component({
  selector: 'app-doctor-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctor-schedule.html',
  styleUrls: ['./doctor-schedule.css']
})
export class DoctorScheduleComponent implements OnInit {
  // ─── داتا الـ API والـ Pagination فقط ──────────────────────────
  bookings: DoctorBookingResponseDto[] = [];
  loading: boolean = false;
  currentTimeFilter: TimeFilterType = 'all';
  currentPageIndex: number = 0;
  currentPageSize: number = 10;
  hasNextPage: boolean = true;

  // 👈 عملنا Inject للـ ChangeDetectorRef جوه الـ constructor
  constructor(
    private doctorScheduleService: DoctorScheduleService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadBookings();
  }

  // جلب البيانات من الباك إند
  loadBookings(): void {
    this.loading = true;
    this.cdr.detectChanges(); // تحديث الـ UI ليظهر الـ Loading Spinner فوراً

    this.doctorScheduleService.getDoctorBookingsPaged(
      this.currentTimeFilter,
      this.currentPageIndex,
      this.currentPageSize
    ).subscribe({
      next: (data) => {
        this.bookings = data;
        this.hasNextPage = data.length === this.currentPageSize;
        this.loading = false;

        // 👈 السر هنا: بنجبر أنجلر يلتفت للتغيير ويحدث الشاشة أول ما الداتا توصل
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching doctor bookings:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // فلترة الداتا (All, Today, Tomorrow...)
  onFilterChange(filter: TimeFilterType): void {
    this.currentTimeFilter = filter;
    this.currentPageIndex = 0;
    this.loadBookings();
  }

  // فلترة بناءً على تاريخ محدد من الـ Date Picker
  onDateFilterChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.value) {
      this.currentTimeFilter = input.value;
      this.currentPageIndex = 0;
      this.loadBookings();
    }
  }

  // الانتقال للصفحة التالية
  nextPage(): void {
    if (this.hasNextPage) {
      this.currentPageIndex++;
      this.loadBookings();
    }
  }

  // العودة للصفحة السابقة
  previousPage(): void {
    if (this.currentPageIndex > 0) {
      this.currentPageIndex--;
      this.loadBookings();
    }
  }
}