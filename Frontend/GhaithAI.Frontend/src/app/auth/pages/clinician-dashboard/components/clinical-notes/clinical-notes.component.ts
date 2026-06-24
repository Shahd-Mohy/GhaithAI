import {
  Component, OnInit, OnDestroy,
  NgZone, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { ClinicalNotesService, ClinicalNote } from './clinical-notes.service';

@Component({
  selector: 'app-clinical-notes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clinical-notes.component.html',
  styleUrls: ['./clinical-notes.component.css']
})
export class ClinicalNotesComponent implements OnInit, OnDestroy {

  notes: ClinicalNote[] = [];
  filtered: ClinicalNote[] = [];
  selected: ClinicalNote | null = null;

  loading = true;
  errorMessage = '';
  searchQuery = '';
  activeFilter: 'all' | 'pending' | 'approved' = 'all';

  private sub: Subscription | null = null;

  constructor(
    private svc: ClinicalNotesService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadNotes();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  loadNotes(): void {
    this.sub?.unsubscribe();
    this.ngZone.run(() => {
      this.loading = true;
      this.errorMessage = '';
      this.cdr.detectChanges();
    });

    this.sub = this.svc.getMine()
      .pipe(timeout(15_000))
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            this.notes = Array.isArray(data) ? data : [];
            this.applyFilter();
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.loading = false;
            this.errorMessage = err?.name === 'TimeoutError'
              ? 'The server took too long. Please try again.'
              : 'Could not load clinical notes.';
            this.cdr.detectChanges();
          });
        }
      });
  }

  applyFilter(): void {
    let list = [...this.notes];

    if (this.activeFilter === 'pending') {
      list = list.filter(n => !n.updatedAt);
    } else if (this.activeFilter === 'approved') {
      list = list.filter(n => !!n.updatedAt);
    }

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(n =>
        n.patientDisplayName.toLowerCase().includes(q) ||
        n.content.toLowerCase().includes(q) ||
        n.noteType.toLowerCase().includes(q)
      );
    }

    this.filtered = list;

    // Deselect if selected note no longer visible
    if (this.selected && !this.filtered.find(n => n.id === this.selected!.id)) {
      this.selected = null;
    }
  }

  setFilter(f: 'all' | 'pending' | 'approved'): void {
    this.activeFilter = f;
    this.applyFilter();
  }

  onSearch(): void {
    this.applyFilter();
  }

  selectNote(note: ClinicalNote): void {
    this.selected = note;
  }

  /** Status derived from updatedAt field — null means it was never edited/approved */
  isApproved(note: ClinicalNote): boolean {
    return !!note.updatedAt;
  }

  /** Initials from patient display name */
  initials(name: string): string {
    return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();
  }

  /** Colour bucket for patient avatar based on name hash */
  avatarColor(name: string): string {
    const colors = [
      '#0B8FAC', '#7C3AED', '#059669', '#D97706',
      '#DC2626', '#2563EB', '#9333EA', '#0891B2'
    ];
    let hash = 0;
    for (const c of name) hash = (hash * 31 + c.charCodeAt(0)) & 0xffffff;
    return colors[Math.abs(hash) % colors.length];
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffH = diffMs / 3_600_000;

    if (diffH < 24 && d.getDate() === now.getDate()) {
      return `Today, ${d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}`;
    }
    if (diffH < 48) return 'Yesterday';
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  pendingCount(): number {
    return this.notes.filter(n => !n.updatedAt).length;
  }
}
