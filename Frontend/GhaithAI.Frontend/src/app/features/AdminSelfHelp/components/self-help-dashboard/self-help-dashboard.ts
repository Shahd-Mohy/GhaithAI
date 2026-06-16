import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SelfHelp as SelfHelpService } from '../../services/self-help';
import { AdminSelfHelpGetAllDto, AdminSelfHelpSaveDto, AdminSelfHelpUpdateDto } from '../../interfaces/self-help.interface';

@Component({
  selector: 'app-self-help-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './self-help-dashboard.html',
  styleUrl: './self-help-dashboard.css',
})
export class SelfHelpDashboardComponent implements OnInit {
  private selfHelpService = inject(SelfHelpService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  items: AdminSelfHelpGetAllDto[] = [];
  loading = false;
  actionLoading: string | null = null;

  // 🎯 متغيرات الـ Pagination الجديدة
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  hasNext = false;
  hasPrevious = false;

  showForm = false;
  isEditMode = false;
  currentId: string | null = null;

  exerciseTypes = [
    { value: 'Breathing Exercise', label: 'Breathing Exercise' },
    { value: 'Meditation', label: 'Meditation' },
    { value: 'Relaxation', label: 'Relaxation' },
    { value: 'Cognitive Tools', label: 'Cognitive Tools' },
    { value: 'Behavioral Activation', label: 'Behavioral Activation' }
  ];

  difficultyLevels = ['Beginner', 'Intermediate', 'Difficulty'];

  formData = {
    title: '',
    type: 'Breathing Exercise',
    description: '',
    contentUrl: null as string | null,
    durationMinutes: null as number | null,
    difficultyLevel: 'Easy',
    isActive: true
  };

  formTips: { id: string | null; text: string }[] = [];

  ngOnInit(): void {
    this.loadAll();
  }

  // 🎯 تعديل جلب البيانات لتباصي الـ pageNumber والـ pageSize
  loadAll(): void {
    this.loading = true;
    this.cdr.markForCheck();

    // تأكد من تعديل الـ Service لتستقبل الـ parameters دي إذا كانت تضرب إيرور كومبايلر (سأضع تعديلها أسفل الكود)
    this.selfHelpService.getAllItems(this.currentPage, this.pageSize).subscribe({
      next: (res: any) => {
        // الـ API بيرجع success: true, data: [...], pagination: {...}
        if (res && res.success) {
          this.items = res.data;

          // تشريب قيم الـ Metadata للـ UI
          this.totalCount = res.pagination.totalCount;
          this.totalPages = res.pagination.totalPages;
          this.currentPage = res.pagination.currentPage;
          this.hasNext = res.pagination.hasNext;
          this.hasPrevious = res.pagination.hasPrevious;
        } else {
          this.items = [];
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load items', err);
        this.items = [];
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // 🎯 ميثودز التنقل بين الصفحات للـ Controls في الـ HTML
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadAll();
    }
  }

  nextPage(): void {
    if (this.hasNext) {
      this.currentPage++;
      this.loadAll();
    }
  }

  prevPage(): void {
    if (this.hasPrevious) {
      this.currentPage--;
      this.loadAll();
    }
  }

  addTipInput(): void {
    this.formTips.push({ id: null, text: '' });
    this.cdr.detectChanges();
  }

  removeTipInput(index: number): void {
    this.formTips.splice(index, 1);
    this.cdr.detectChanges();
  }

  openAddMode(): void {
    this.isEditMode = false;
    this.currentId = null;
    this.showForm = true;
    this.formTips = [{ id: null, text: '' }];
    this.formData = {
      title: '',
      type: 'Breathing Exercise',
      description: '',
      contentUrl: null,
      durationMinutes: null,
      difficultyLevel: 'Easy',
      isActive: true
    };
    this.cdr.detectChanges();
  }

  openEditMode(id: string): void {
    this.actionLoading = id;
    this.cdr.detectChanges();
    this.selfHelpService.getItemById(id).subscribe({
      next: (details) => {
        this.actionLoading = null;
        this.isEditMode = true;
        this.currentId = id;
        this.showForm = true;

        this.formData = {
          title: details.title,
          type: details.type,
          description: details.description,
          contentUrl: details.contentUrl,
          durationMinutes: details.durationMinutes,
          difficultyLevel: details.difficultyLevel,
          isActive: details.isActive
        };

        this.formTips = details.exerciseTips.map(t => ({ id: t.id, text: t.text }));
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to fetch details for edit', err);
        this.actionLoading = null;
        this.cdr.detectChanges();
      }
    });
  }

  closeForm(): void {
    this.showForm = false;
    this.cdr.detectChanges();
  }

  save(): void {
    if (!this.formData.title.trim() || !this.formData.description.trim()) {
      alert('Please fill in the required fields (Title & Description).');
      return;
    }

    const validTips = this.formTips.filter(t => t.text.trim() !== '');

    if (this.isEditMode && this.currentId) {
      const updateDto: AdminSelfHelpUpdateDto = {
        id: this.currentId,
        title: this.formData.title,
        type: this.formData.type,
        description: this.formData.description,
        contentUrl: this.formData.contentUrl || null,
        durationMinutes: this.formData.durationMinutes ?? 0,
        difficultyLevel: this.formData.difficultyLevel,
        isActive: this.formData.isActive,
        exerciseTips: validTips.map(t => ({ id: t.id, text: t.text }))
      };

      this.selfHelpService.updateItem(updateDto).subscribe({
        next: (res) => {
          if (res.success) {
            this.closeForm();
            this.loadAll();
          }
        },
        error: (err) => console.error('Update failed', err)
      });

    } else {
      const saveDto: AdminSelfHelpSaveDto = {
        title: this.formData.title,
        type: this.formData.type,
        description: this.formData.description,
        contentUrl: this.formData.contentUrl || null,
        durationMinutes: this.formData.durationMinutes,
        difficultyLevel: this.formData.difficultyLevel,
        isActive: this.formData.isActive,
        exerciseTips: validTips.map(t => ({ text: t.text }))
      };

      this.selfHelpService.createItem(saveDto).subscribe({
        next: () => {
          this.closeForm();
          this.loadAll();
        },
        error: (err) => console.error('Creation failed', err)
      });
    }
  }

  deleteItem(id: string): void {
    if (confirm('Are you sure you want to delete this content? This action will mark it as deleted.')) {
      this.actionLoading = id;
      this.cdr.detectChanges();
      this.selfHelpService.deleteItem(id).subscribe({
        next: (res) => {
          this.actionLoading = null;
          if (res.success) {
            this.loadAll();
          }
        },
        error: (err) => {
          console.error('Delete failed', err);
          this.actionLoading = null;
          this.cdr.detectChanges();
        }
      });
    }
  }

  viewDetails(id: string): void {
    this.router.navigate(['/admin/self-help/details', id]);
  }
}
