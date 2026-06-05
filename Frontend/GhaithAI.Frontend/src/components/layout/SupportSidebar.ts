// File: src/components/layout/SupportSidebar.ts

import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ChatStore } from '../../store/chat.store';
import { ChatHttpService } from '../../services/chat.service';
import { SessionModel } from '../../types/chat.types';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  queryParams?: { page: string };
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Home',            icon: 'bi-house',         path: '/dashboard', queryParams: { page: 'home' } },
  { label: 'Talk to AI',      icon: 'bi-chat',          path: '/support/chat' },
  { label: 'Mood Tracker',    icon: 'bi-graph-up',      path: '/dashboard', queryParams: { page: 'mood' } },
  { label: 'Journal',         icon: 'bi-journal',       path: '/dashboard', queryParams: { page: 'journal' } },
  { label: 'Self-Help Tools', icon: 'bi-sliders',       path: '/dashboard', queryParams: { page: 'tools' } },
  { label: 'Learn',           icon: 'bi-book',          path: '/dashboard', queryParams: { page: 'learn' } },
];

const GET_HELP_ITEMS: NavItem[] = [
  { label: 'Find a Professional', icon: 'bi-person-badge', path: '/dashboard', queryParams: { page: 'professionals' } },
];

@Component({
  selector: 'app-support-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      height: 100%;
      background-color: var(--sidebar, #f9f9f9);
      overflow: hidden;
      font-family: 'Sora', sans-serif;
    }

    /* ── Scrollable area ───────────────────────────────────────────── */
    .sidebar-scroll {
      flex: 1 1 0%;
      overflow-y: auto;
      min-height: 0;
    }

    /* ── Nav links ─────────────────────────────────────────────────── */
    .sidebar-nav-link {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.5rem 0.75rem;
      border-radius: 0.5rem;
      font-size: 0.875rem;
      font-weight: 500;
      text-decoration: none;
      color: var(--sidebar-foreground, #1a1a1a);
      background-color: transparent;
      transition: background-color 0.15s ease, color 0.15s ease;
      margin-bottom: 2px;
    }
    .sidebar-nav-link:hover {
      background-color: var(--accent, #f0f0f0);
      color: var(--accent-foreground, #1a1a1a);
    }
    .sidebar-nav-link.active-nav {
      background-color: var(--accent, #f0f0f0);
      color: var(--accent-foreground, #1a1a1a);
    }

    /* ── Session items ─────────────────────────────────────────────── */
    .session-item {
      position: relative;
      border-radius: 0.5rem;
      cursor: pointer;
      padding: 0.625rem 0.75rem;
      margin-bottom: 4px;
      transition: background-color 0.15s ease;
    }
    .session-item:hover {
      background-color: rgba(0,0,0,0.04);
    }
    .session-item .delete-btn {
      opacity: 0;
      transition: opacity 0.15s ease;
    }
    .session-item:hover .delete-btn {
      opacity: 1;
    }

    /* ── New Conversation button ───────────────────────────────────── */
    .new-btn {
      background-color: var(--primary, #1a1a1a);
      color: var(--primary-foreground, #f8f8f8);
      border: none;
      border-radius: 0.75rem;
      padding: 0.625rem 1rem;
      font-size: 0.875rem;
      font-weight: 500;
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      transition: opacity 0.15s ease, transform 0.1s ease;
    }
    .new-btn:hover {
      opacity: 0.9;
      transform: translateY(-1px);
    }

    /* ── Crisis button ─────────────────────────────────────────────── */
    .crisis-link {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      width: 100%;
      padding: 0.625rem 1rem;
      border-radius: 0.75rem;
      font-size: 0.875rem;
      font-weight: 500;
      text-decoration: none;
      border: 1px solid rgba(220, 38, 38, 0.3);
      background-color: rgba(220, 38, 38, 0.08);
      color: var(--destructive, #dc2626);
      transition: background-color 0.15s ease;
    }
    .crisis-link:hover {
      background-color: rgba(220, 38, 38, 0.14);
      color: var(--destructive, #dc2626);
    }

    /* ── Risk badges ───────────────────────────────────────────────── */
    .risk-high {
      font-size: 0.7rem;
      padding: 1px 6px;
      border-radius: 999px;
      background-color: rgba(220, 38, 38, 0.12);
      color: #dc2626;
      font-weight: 600;
      white-space: nowrap;
    }
    .risk-medium {
      font-size: 0.7rem;
      padding: 1px 6px;
      border-radius: 999px;
      background-color: rgba(234, 179, 8, 0.15);
      color: #a17800;
      font-weight: 600;
      white-space: nowrap;
    }

    /* ── Loading skeleton ──────────────────────────────────────────── */
    .skeleton {
      height: 56px;
      border-radius: 0.5rem;
      background: linear-gradient(
        90deg,
        #ebebeb 25%,
        #d6d6d6 50%,
        #ebebeb 75%
      );
      background-size: 200% 100%;
      animation: shimmer 1.4s infinite;
      margin-bottom: 8px;
    }
    @keyframes shimmer {
      0%   { background-position: -200% 0; }
      100% { background-position:  200% 0; }
    }

    /* ── Section labels ────────────────────────────────────────────── */
    .section-label {
      padding: 0.25rem 0.5rem;
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--muted-foreground, #737373);
    }

    /* ── Footer account row ────────────────────────────────────────── */
    .account-link {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.5rem 0.5rem;
      border-radius: 0.5rem;
      text-decoration: none;
      transition: background-color 0.15s ease;
      width: 100%;
    }
    .account-link:hover {
      background-color: var(--accent, #f0f0f0);
    }
    .avatar-circle {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: var(--muted, #f5f5f5);
      flex-shrink: 0;
    }
    .logo-icon {
      width: 38px;
      height: 38px;
      background-color: var(--primary, #0B8FAC);
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .logo-icon svg {
      width: 20px;
      height: 20px;
      fill: white;
    }
  `],
  template: `
    <!-- ── Header / Logo ──────────────────────────────────────── -->
    <div class="p-3 flex-shrink-0">
      <a routerLink="/dashboard"
         [queryParams]="{ page: 'home' }"
         class="d-flex align-items-center gap-2 px-2 py-2 rounded text-decoration-none"
         style="transition: background-color 0.15s ease;"
         onmouseenter="this.style.backgroundColor='var(--accent)'"
         onmouseleave="this.style.backgroundColor='transparent'">
        <div class="logo-icon">
          <svg viewBox="0 0 24 24" fill="white">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 14.5v-9l6 4.5-6 4.5z" />
          </svg>
        </div>
        <div>
          <p class="mb-0 fw-bold lh-sm" style="color: var(--foreground); font-size: 15px;">GhaithAI</p>
          <p class="mb-0 lh-sm" style="font-size: 11px; color: var(--muted-foreground);">Personal Support</p>
        </div>
      </a>
    </div>

    <!-- ── New Conversation Button ─────────────────────────── -->
    <div class="px-3 pb-3 flex-shrink-0">
      <button type="button" class="new-btn" (click)="startNewSession()">
        <i class="bi bi-plus-lg"></i>
        New Conversation
      </button>
    </div>

    <!-- ── Scrollable Content ──────────────────────────────── -->
    <div class="sidebar-scroll px-2 pb-3">

      <!-- Recent Conversations -->
      <div class="mb-3">
        <p class="section-label">Recent Conversations</p>

        <!-- Loading skeletons -->
        @if (chatStore.isLoadingSessions()) {
          @for (_ of skeletons; track $index) {
            <div class="skeleton mx-1"></div>
          }
        }

        <!-- Session list -->
        @if (!chatStore.isLoadingSessions()) {
          @if (chatStore.sessions().length === 0) {
            <p class="text-center py-3" style="font-size: 0.75rem; color: var(--muted-foreground);">
              No conversations yet
            </p>
          }
          @for (session of chatStore.sessions(); track session.id) {
            <div class="session-item"
                 [style.background-color]="isActiveSession(session) ? 'var(--accent)' : 'transparent'"
                 (click)="selectSession(session)">

              <!-- Title row -->
              <div class="d-flex align-items-center justify-content-between gap-2">
                <span class="small fw-medium text-truncate flex-grow-1"
                      style="color: var(--sidebar-foreground);">
                  {{ session.title || 'Chat Session' }}
                </span>

                <!-- Risk badge -->
                @if (session.riskLevel === 'high') {
                  <span class="risk-high flex-shrink-0">High</span>
                }
                @if (session.riskLevel === 'medium') {
                  <span class="risk-medium flex-shrink-0">Med</span>
                }

                <!-- Delete -->
                <button type="button"
                        class="delete-btn btn btn-sm p-1 border-0 bg-transparent"
                        style="color: var(--muted-foreground);"
                        (click)="deleteSession(session.id, $event)"
                        title="Delete session">
                  <i class="bi bi-trash3" style="font-size: 0.7rem;"></i>
                </button>
              </div>

              <!-- Meta row -->
              <div class="d-flex align-items-center gap-2 mt-1">
                <span style="font-size: 0.72rem; color: var(--muted-foreground);">
                  {{ session.startedAt | date:'MMM d' }}
                </span>
                <span style="font-size: 0.72rem; color: var(--muted-foreground);">·</span>
                <span style="font-size: 0.72rem; color: var(--muted-foreground);">
                  {{ session.messageCount }} msg{{ session.messageCount !== 1 ? 's' : '' }}
                </span>
              </div>

              <!-- Inline delete confirm -->
              @if (confirmDeleteId() === session.id) {
                <div class="d-flex align-items-center gap-2 mt-2 pt-2"
                     style="border-top: 1px solid var(--border);">
                  <span class="flex-grow-1" style="font-size: 0.72rem; color: var(--muted-foreground);">
                    Delete this session?
                  </span>
                  <button type="button"
                          class="btn btn-sm py-0 px-2"
                          style="background-color: var(--destructive); color: #fff; font-size: 0.72rem;"
                          (click)="confirmDelete(session.id)">
                    Yes
                  </button>
                  <button type="button"
                          class="btn btn-sm py-0 px-2 border"
                          style="border-color: var(--border); color: var(--foreground); font-size: 0.72rem;"
                          (click)="cancelDelete()">
                    No
                  </button>
                </div>
              }
            </div>
          }
        }
      </div>

      <!-- Navigation -->
      <div class="mt-4">
        <p class="section-label">Your Space</p>
        @for (item of navItems; track item.path + (item.queryParams?.page || '')) {
          <a [routerLink]="item.path"
             [queryParams]="item.queryParams"
             routerLinkActive="active-nav"
             class="sidebar-nav-link"
             [routerLinkActiveOptions]="{ exact: true }">
            <i class="bi {{ item.icon }} small"></i>
            {{ item.label }}
          </a>
        }
      </div>

      <!-- Get Help Section -->
      <div class="mt-4">
        <p class="section-label">Get Help</p>
        @for (item of getHelpItems; track item.path + (item.queryParams?.page || '')) {
          <a [routerLink]="item.path"
             [queryParams]="item.queryParams"
             routerLinkActive="active-nav"
             class="sidebar-nav-link"
             [routerLinkActiveOptions]="{ exact: true }">
            <i class="bi {{ item.icon }} small"></i>
            {{ item.label }}
          </a>
        }
      </div>
    </div>

    <!-- ── Crisis Button ────────────────────────────────────── -->
    <div class="px-3 py-3 flex-shrink-0">
      <a routerLink="/support/crisis" class="crisis-link" style="border-radius: 0.5rem; justify-content: flex-start; padding-left: 1rem;">
        <i class="bi bi-telephone"></i>
        Crisis Support
      </a>
    </div>

    <!-- ── Footer / Account ─────────────────────────────────── -->
    <div class="px-3 py-3 flex-shrink-0" style="border-top: 1px solid var(--sidebar-border);">
      <a routerLink="/support/settings" class="account-link">
        <div class="avatar-circle">
          <i class="bi bi-person-fill small" style="color: var(--muted-foreground);"></i>
        </div>
        <div class="d-flex flex-column align-items-start">
          <span class="small fw-medium" style="color: var(--sidebar-foreground);">My Account</span>
          <span style="font-size: 0.7rem; color: var(--muted-foreground);">Settings</span>
        </div>
        <i class="bi bi-chevron-right ms-auto small" style="color: var(--muted-foreground);"></i>
      </a>
    </div>
  `,
})
export class SupportSidebar implements OnInit {
  readonly chatStore       = inject(ChatStore);
  private readonly chatHttpService = inject(ChatHttpService);
  private readonly router          = inject(Router);

  readonly confirmDeleteId = signal<string | null>(null);

  readonly navItems = NAV_ITEMS;
  readonly getHelpItems = GET_HELP_ITEMS;
  readonly skeletons = [1, 2, 3];

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions(): void {
    this.chatStore.isLoadingSessions.set(true);
    this.chatHttpService.getSessions(1, 30).subscribe({
      next: (result) => {
        this.chatStore.setSessions(result.items);
      },
      error: (err) => {
        console.error('[SupportSidebar] Failed to load sessions:', err);
      },
      complete: () => {
        this.chatStore.isLoadingSessions.set(false);
      },
    });
  }

  selectSession(session: SessionModel): void {
    this.router.navigate(['/support/chat'], {
      queryParams: { sessionId: session.id },
    });
  }

  startNewSession(): void {
    this.router.navigate(['/support/chat']);
  }

  deleteSession(sessionId: string, event: MouseEvent): void {
    event.stopPropagation();
    this.confirmDeleteId.set(sessionId);
  }

  confirmDelete(sessionId: string): void {
    this.chatHttpService.deleteSession(sessionId).subscribe({
      next: () => {
        this.chatStore.removeSession(sessionId);
        this.confirmDeleteId.set(null);
      },
      error: (err) => {
        console.error('[SupportSidebar] Failed to delete session:', err);
        this.confirmDeleteId.set(null);
      },
    });
  }

  cancelDelete(): void {
    this.confirmDeleteId.set(null);
  }

  isActiveSession(session: SessionModel): boolean {
    return this.chatStore.activeSession()?.id === session.id;
  }
}
