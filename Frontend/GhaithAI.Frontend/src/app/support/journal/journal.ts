import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import {
  JournalService,
  JournalDTO,
  CreateJournalRequest,
  UpdateJournalRequest,
  GetAllResponse,
  SingleResponse,
  CreateResponse,
  ActionResponse
} from '../../services/journal.service';

// ─── View mode ───────────────────────────────────────────────────────────────

type ViewMode = 'list' | 'create' | 'detail' | 'edit';

// ─── Component ───────────────────────────────────────────────────────────────

@Component({
  selector: 'app-journal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './journal.html',
  styleUrl: './journal.css',
  changeDetection: ChangeDetectionStrategy.OnPush   // ← explicit; cdr.markForCheck() drives all updates
})
export class JournalComponent implements OnInit, OnDestroy {

  // ─── View state ────────────────────────────────────────────────────────────
  viewMode: ViewMode = 'list';

  // ─── Loading / error ───────────────────────────────────────────────────────
  isLoading = false;
  isSaving = false;
  isDeleting = false;
  errorMessage = '';
  successMessage = '';

  // ─── Search & pagination ───────────────────────────────────────────────────
  searchQuery = '';
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;
  private searchSubject = new Subject<string>();
  private searchSub!: Subscription;

  // ─── Entries ───────────────────────────────────────────────────────────────
  entries: JournalDTO[] = [];
  selectedEntry: JournalDTO | null = null;

  // ─── Create form state ─────────────────────────────────────────────────────
  newEntryMood = '';
  newEntryTitle = '';
  newEntryBody = '';
  newEntryTags: string[] = [];
  currentTagInput = '';
  activePrompt = '';

  // ─── Edit form state ───────────────────────────────────────────────────────
  editTitle = '';
  editContent = '';
  editTagsInput = '';
  editTags: string[] = [];
  editCurrentTagInput = '';

  // ─── Delete confirmation ───────────────────────────────────────────────────
  showDeleteConfirm = false;
  entryToDelete: JournalDTO | null = null;

  // ─── Mood options ──────────────────────────────────────────────────────────
  moods = [
    { label: 'Happy',    icon: '🙂', value: 'happy' },
    { label: 'Sad',      icon: '😢', value: 'sad' },
    { label: 'Neutral',  icon: '😐', value: 'neutral' },
    { label: 'Grateful', icon: '🙏', value: 'grateful' },
    { label: 'Anxious',  icon: '😰', value: 'anxious' },
    { label: 'Peaceful', icon: '😌', value: 'peaceful' },
    { label: 'Tired',    icon: '😴', value: 'tired' },
    { label: 'Hopeful',  icon: '✨', value: 'hopeful' }
  ];

  // ─── Prompt suggestions ────────────────────────────────────────────────────
  prompts = [
    'What are you grateful for today?',
    "What's been on your mind lately?",
    'Describe a moment that made you smile today',
    'What challenge are you currently facing?',
    'Write a letter to your future self',
    'What would make today better?'
  ];

  // ─── Constructor ───────────────────────────────────────────────────────────

  constructor(
    private journalService: JournalService,
    private cdr: ChangeDetectorRef   // ← injected
  ) {}

  // ─── Lifecycle ─────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.loadEntries();

    // Debounced search — waits 400ms after user stops typing
    this.searchSub = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadEntries();
    });
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  // ─── Load entries (GET /api/Journal) ───────────────────────────────────────

  loadEntries(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.journalService.getAll(this.searchQuery, this.currentPage, this.pageSize).subscribe({
      next: (res: GetAllResponse) => {
        this.entries = res.data;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.isLoading = false;
        this.cdr.markForCheck();   // ← tell Angular the view is dirty
      },
      error: (err: Error) => {
        this.errorMessage = err.message || 'Failed to load journal entries.';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ─── Search ────────────────────────────────────────────────────────────────

  onSearchChange(): void {
    this.searchSubject.next(this.searchQuery);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.currentPage = 1;
    this.loadEntries();
  }

  // ─── Pagination ────────────────────────────────────────────────────────────

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadEntries();
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) pages.push(i);
    return pages;
  }

  // ─── View entry detail (GET /api/Journal/{id}) ─────────────────────────────

  viewEntry(entry: JournalDTO): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.journalService.getById(entry.journalId).subscribe({
      next: (res: SingleResponse) => {
        this.selectedEntry = res.data;
        this.viewMode = 'detail';
        this.isLoading = false;
        this.cdr.markForCheck();
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: (err: Error) => {
        this.errorMessage = err.message;
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ─── Create entry ──────────────────────────────────────────────────────────

  openCreateForm(): void {
    this.resetCreateForm();
    this.viewMode = 'create';
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  saveEntry(): void {
    if (!this.newEntryBody.trim()) {
      this.errorMessage = 'Please write something before saving.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const tagsString = this.newEntryTags.length > 0
      ? this.newEntryTags.join(',')
      : null;

    const selectedMoodObj = this.moods.find(m => m.label === this.newEntryMood);

    const payload: CreateJournalRequest = {
      title: this.newEntryTitle.trim() || null,
      content: this.newEntryBody.trim(),
      promptType: this.activePrompt ? 'prompted' : 'free',
      moodBefore: selectedMoodObj?.value ?? '',
      tags: tagsString
    };

    this.journalService.create(payload).subscribe({
      next: (_res: CreateResponse) => {
        this.isSaving = false;
        this.viewMode = 'list';
        this.resetCreateForm();
        this.currentPage = 1;
        this.showSuccess('Journal entry saved successfully.');
        this.loadEntries(); // loadEntries calls markForCheck internally
      },
      error: (err: Error) => {
        this.isSaving = false;
        this.errorMessage = err.message || 'Failed to save entry.';
        this.cdr.markForCheck();
      }
    });
  }

  cancelCreate(): void {
    this.resetCreateForm();
    this.viewMode = 'list';
    this.errorMessage = '';
  }

  private resetCreateForm(): void {
    this.newEntryMood = '';
    this.newEntryTitle = '';
    this.newEntryBody = '';
    this.newEntryTags = [];
    this.currentTagInput = '';
    this.activePrompt = '';
  }

  // ─── Edit entry ────────────────────────────────────────────────────────────

  openEditForm(entry: JournalDTO): void {
    this.selectedEntry = entry;
    this.editTitle = entry.title ?? '';
    this.editContent = entry.content;
    this.editTags = [...(entry.tags ?? [])];
    this.editCurrentTagInput = '';
    this.errorMessage = '';
    this.viewMode = 'edit';
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  saveEdit(): void {
    if (!this.selectedEntry) return;
    if (!this.editContent.trim()) {
      this.errorMessage = 'Content cannot be empty.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const tagsString = this.editTags.length > 0
      ? this.editTags.join(',')
      : null;

    const payload: UpdateJournalRequest = {
      title: this.editTitle.trim() || null,
      content: this.editContent.trim(),
      tags: tagsString
    };

    // Capture id before the async chain so selectedEntry can safely change
    const journalId = this.selectedEntry.journalId;

    this.journalService.update(journalId, payload).subscribe({
      next: (_res: ActionResponse) => {
        this.isSaving = false;

        // Re-fetch full detail to get updated content
        this.journalService.getById(journalId).subscribe({
          next: (res: SingleResponse) => {
            this.selectedEntry = res.data;
            this.viewMode = 'detail';
            this.showSuccess('Entry updated successfully.');
            this.cdr.markForCheck();
            this.loadEntries(); // refresh list counts/previews; marks for check internally
          },
          error: () => {
            this.viewMode = 'list';
            this.cdr.markForCheck();
            this.loadEntries();
          }
        });
      },
      error: (err: Error) => {
        this.isSaving = false;
        this.errorMessage = err.message || 'Failed to update entry.';
        this.cdr.markForCheck();
      }
    });
  }

  cancelEdit(): void {
    this.errorMessage = '';
    if (this.selectedEntry) {
      this.viewMode = 'detail';
    } else {
      this.viewMode = 'list';
    }
  }

  // ─── Delete entry ──────────────────────────────────────────────────────────

  confirmDelete(entry: JournalDTO): void {
    this.entryToDelete = entry;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.entryToDelete = null;
    this.showDeleteConfirm = false;
  }

  executeDelete(): void {
    if (!this.entryToDelete) return;

    this.isDeleting = true;
    this.errorMessage = '';

    this.journalService.delete(this.entryToDelete.journalId).subscribe({
      next: (_res: ActionResponse) => {
        this.isDeleting = false;
        this.showDeleteConfirm = false;
        this.entryToDelete = null;
        this.selectedEntry = null;
        this.viewMode = 'list';
        this.currentPage = 1;
        this.showSuccess('Entry deleted.');
        this.loadEntries(); // marks for check internally
      },
      error: (err: Error) => {
        this.isDeleting = false;
        this.errorMessage = err.message || 'Failed to delete entry.';
        this.showDeleteConfirm = false;
        this.cdr.markForCheck();
      }
    });
  }

  // ─── Mood helpers ──────────────────────────────────────────────────────────

  selectMood(label: string): void {
    this.newEntryMood = this.newEntryMood === label ? '' : label;
  }

  getMoodIcon(moodValue: string): string {
    const mood = this.moods.find(m => m.value === moodValue || m.label.toLowerCase() === moodValue.toLowerCase());
    return mood?.icon ?? '📝';
  }

  getMoodLabel(moodValue: string): string {
    const mood = this.moods.find(m => m.value === moodValue || m.label.toLowerCase() === moodValue.toLowerCase());
    return mood?.label ?? moodValue;
  }

  // ─── Prompt helpers ────────────────────────────────────────────────────────

  applyPrompt(prompt: string): void {
    this.activePrompt = prompt;
    const addition = this.newEntryBody ? '\n\n' + prompt + '\n' : prompt + '\n';
    this.newEntryBody += addition;
  }

  // ─── Tag helpers (create form) ─────────────────────────────────────────────

  addTag(): void {
    const tag = this.currentTagInput.trim().toLowerCase();
    if (tag && !this.newEntryTags.includes(tag)) {
      this.newEntryTags.push(tag);
    }
    this.currentTagInput = '';
  }

  removeTag(index: number): void {
    this.newEntryTags.splice(index, 1);
  }

  // ─── Tag helpers (edit form) ───────────────────────────────────────────────

  addEditTag(): void {
    const tag = this.editCurrentTagInput.trim().toLowerCase();
    if (tag && !this.editTags.includes(tag)) {
      this.editTags.push(tag);
    }
    this.editCurrentTagInput = '';
  }

  removeEditTag(index: number): void {
    this.editTags.splice(index, 1);
  }

  // ─── Navigation ────────────────────────────────────────────────────────────

  backToList(): void {
    this.selectedEntry = null;
    this.viewMode = 'list';
    this.errorMessage = '';
  }

  // ─── Computed getters ──────────────────────────────────────────────────────

  get totalEntries(): number {
    return this.totalCount;
  }

  get todayLabel(): string {
    return new Date().toLocaleDateString('en-US', {
      weekday: 'long', month: 'long', day: 'numeric', year: 'numeric'
    });
  }

  get wordCount(): number {
    return this.newEntryBody
      ? this.newEntryBody.trim().split(/\s+/).filter(Boolean).length
      : 0;
  }

  get editWordCount(): number {
    return this.editContent
      ? this.editContent.trim().split(/\s+/).filter(Boolean).length
      : 0;
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    try {
      return new Date(dateStr).toLocaleDateString('en-US', {
        weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
      });
    } catch {
      return dateStr;
    }
  }

  formatRelativeDate(dateStr: string): string {
    if (!dateStr) return '';
    try {
      const date = new Date(dateStr);
      const now = new Date();
      const diffMs = now.getTime() - date.getTime();
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

      if (diffDays === 0) return 'Today';
      if (diffDays === 1) return 'Yesterday';
      if (diffDays < 7) return `${diffDays} days ago`;
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    } catch {
      return dateStr;
    }
  }

  // ─── Success toast ─────────────────────────────────────────────────────────

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.cdr.markForCheck();   // ← show toast immediately
    setTimeout(() => {
      this.successMessage = '';
      this.cdr.markForCheck(); // ← clear toast after 3s
    }, 3000);
  }

  // ─── Dismiss error ─────────────────────────────────────────────────────────

  dismissError(): void {
    this.errorMessage = '';
  }
}