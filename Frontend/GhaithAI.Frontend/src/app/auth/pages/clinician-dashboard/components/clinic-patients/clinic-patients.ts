import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateClinicPatientDto, DoctorClinicPatientListDto, UpdateClinicPatientDto } from './interface/clinic-patient.model';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { ClinicPatientService } from './services/clinic-patient';

@Component({
  selector: 'app-clinic-patients',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clinic-patients.html',
  styleUrls: ['./clinic-patients.css']
})
export class ClinicPatientsComponent implements OnInit {
  // ─── الداتا والـ Loading ───
  paginatedPatients: DoctorClinicPatientListDto[] = []; // متوافق مع الجدول في الـ HTML
  filteredPatients: DoctorClinicPatientListDto[] = [];  // متوافق مع الـ Empty state تشيك
  loading: boolean = false;

  // ─── الفلترة والـ Pagination ───
  searchTerm: string = '';
  currentPage: number = 1;     // متوافق مع الـ HTML (بيبدأ من صفحة 1)
  pageSize: number = 10;
  hasNextPage: boolean = true;
  totalPages: number = 1;      // ضفناها عشان الـ HTML بيعرض إجمالي الصفحات

  // ─── إدارة حالة الفورم (طابقنا شو وفورم داتا) ───
  showForm: boolean = false;   // متوافق مع الـ *ngIf="showForm" في الـ HTML
  isEditMode: boolean = false;
  currentPatientId: string = '';

  // كائن البيانات المربوط بالفورم [(ngModel)]="formData.X"
  formData = {
    fullName: '',
    phone: '',
    notes: ''
  };

  private searchSubject = new Subject<string>();

  constructor(
    private patientService: ClinicPatientService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPatients();

    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(value => {
      this.searchTerm = value;
      this.currentPage = 1; // تصفير عند البحث الجديد
      this.loadPatients();
    });
  }

  // ─── جلب البيانات من الـ API ───
  loadPatients(): void {
    this.loading = true;
    this.cdr.detectChanges();

    this.patientService.getPatients(this.searchTerm, this.currentPage, this.pageSize)
      .subscribe({
        next: (data: DoctorClinicPatientListDto[]) => {
          this.paginatedPatients = data;
          this.filteredPatients = data; // بنسويها بيها عشان الـ Empty state يشتغل صح

          // الباك إند بيرجع البيانات المطلوبة، وبنحدد الـ Pagination بناءً على عدد العناصر المرجوعة
          this.hasNextPage = data.length === this.pageSize;

          // حساب تقريبي للصفحات بناءً على الداتا الحالية والباك إند
          if (data.length < this.pageSize && this.currentPage === 1) {
            this.totalPages = 1;
          } else if (this.hasNextPage) {
            this.totalPages = this.currentPage + 1; // بيتيح له يضغط Next
          } else {
            this.totalPages = this.currentPage; // وقف على كدة
          }

          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          console.error('Error fetching clinic patients:', err);
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
  }

  // تفعيل السيرش التلقائي (ngModelChange)="onSearchChange()" بدون باراميتر في الـ HTML
  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  // ─── الـ Pagination ───
  nextPage(): void {
    if (this.hasNextPage) {
      this.currentPage++;
      this.loadPatients();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPatients();
    }
  }

  // ─── إدارة المودال / الفورم ───
  openAddMode(): void { // طابقنا الميثود (click)="openAddMode()"
    this.isEditMode = false;
    this.showForm = true;
    this.resetFormFields();
  }

  openEditMode(patient: DoctorClinicPatientListDto): void { // طابقنا الميثود (click)="openEditMode(patient)"
    this.isEditMode = true;
    this.showForm = true;
    this.currentPatientId = patient.id;

    // ربط الحقول بـ الـ DTO المرجوع (مع مراعاة الـ Mapped names من الباك إند)
    this.formData.fullName = patient.patientFullName;
    this.formData.phone = patient.patientPhone;
    this.formData.notes = patient.notes;
  }

  closeForm(): void {
    this.showForm = false;
    this.resetFormFields();
  }

  // // ─── حفظ البيانات (الإنشاء والتعديل) ───
  // savePatient(): void {
  //   if (!this.formData.fullName.trim() || !this.formData.phone.trim()) {
  //     alert('Patient name and phone are required.');
  //     return;
  //   }

  //   if (this.isEditMode) {
  //     const updateDto: UpdateClinicPatientDto = {
  //       id: this.currentPatientId,
  //       patientFullName: this.formData.fullName.trim(),
  //       patientPhone: this.formData.phone.trim(),
  //       notes: this.formData.notes.trim()
  //     };

  //     this.patientService.updatePatient(updateDto).subscribe({
  //       next: () => {
  //         this.closeForm();
  //         this.loadPatients();
  //       },
  //       error: (err: any) => alert(err.error?.message || 'Error updating patient')
  //     });

  //   } else {
  //     const createDto: CreateClinicPatientDto = {
  //       patientFullName: this.formData.fullName.trim(),
  //       patientPhone: this.formData.phone.trim(),
  //       notes: this.formData.notes.trim()
  //     };

  //     this.patientService.createPatient(createDto).subscribe({
  //       next: () => {
  //         this.closeForm();
  //         this.currentPage = 1; // ارجع للصفحة الأولى لمشاهدة المريض الجديد
  //         this.loadPatients();
  //       },
  //       error: (err: any) => alert(err.error?.message || 'Error creating patient')
  //     });
  //   }
  // }
  savePatient(): void {
    if (!this.formData.fullName.trim() || !this.formData.phone.trim()) {
      alert('Patient name and phone are required.');
      return;
    }

    if (this.isEditMode) {
      const updateDto: UpdateClinicPatientDto = {
        id: this.currentPatientId,
        patientFullName: this.formData.fullName.trim(),
        patientPhone: this.formData.phone.trim(),
        notes: this.formData.notes.trim()
      };

      this.patientService.updatePatient(updateDto).subscribe({
        next: () => {
          this.closeForm();
          this.loadPatients();
        },
        error: (err: any) => {
          // 🔥 هنا بنمسك الرسالة اللي جاية من الباك إند سواء من الـ Exception أو الـ ModelState
          const errorMessage = err.error?.message || err.error || 'Error updating patient';
          alert(errorMessage);
        }
      });

    } else {
      const createDto: CreateClinicPatientDto = {
        patientFullName: this.formData.fullName.trim(),
        patientPhone: this.formData.phone.trim(),
        notes: this.formData.notes.trim()
      };

      this.patientService.createPatient(createDto).subscribe({
        next: () => {
          this.closeForm();
          this.currentPage = 1;
          this.loadPatients();
        },
        error: (err: any) => {
          // 🔥 هنا بنقرأ الـ InvalidOperationException المبعوتة من الباك إند
          // الباك إند غالباً بيرجعها في أوبجكت جواه property اسمها message أو النص مباشرة
          const errorMessage = err.error?.message || err.error || 'Error creating patient';
          alert(errorMessage);
        }
      });
    }
  }
  // ─── حذف المريض ───
  deletePatient(id: string): void { // طابقنا الميثود (click)="deletePatient(patient.id)"
    if (confirm('Are you absolutely sure you want to delete this patient from the clinic records?')) {
      this.patientService.deletePatient(id).subscribe({
        next: () => {
          if (this.paginatedPatients.length === 1 && this.currentPage > 1) {
            this.currentPage--;
          }
          this.loadPatients();
        },
        error: (err: any) => alert(err.error?.message || 'Error deleting patient')
      });
    }
  }

  private resetFormFields(): void {
    this.currentPatientId = '';
    this.formData = {
      fullName: '',
      phone: '',
      notes: ''
    };
  }
}