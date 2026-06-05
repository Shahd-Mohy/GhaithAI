// File: src/components/layout/SupportSidebar.ts

import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ChatStore } from '../../store/chat.store';
import { ChatHttpService } from '../../services/chat.service';
import { SessionModel } from '../../types/chat.types';
import { AuthService } from '../../app/services/auth';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  queryParams?: Record<string, string>;
  exact?: boolean;
}

const YOUR_SPACE_ITEMS: NavItem[] = [
  { label: 'Home',            icon: 'bi-house-fill',    path: '/dashboard',     queryParams: { page: 'home' },          exact: true },
  { label: 'Talk to AI',      icon: 'bi-chat-dots-fill',path: '/support/chat',                                          exact: false },
  { label: 'Mood Tracker',    icon: 'bi-graph-up-arrow',path: '/dashboard',     queryParams: { page: 'mood' },          exact: true },
  { label: 'Journal',         icon: 'bi-journal-text',  path: '/dashboard',     queryParams: { page: 'journal' },       exact: true },
  { label: 'Self-Help Tools', icon: 'bi-sliders',       path: '/dashboard',     queryParams: { page: 'tools' },         exact: true },
  { label: 'Learn',           icon: 'bi-book-open',     path: '/dashboard',     queryParams: { page: 'learn' },         exact: true },
];

const GET_HELP_ITEMS: NavItem[] = [
  { label: 'Find a Professional', icon: 'bi-person-badge-fill', path: '/dashboard', queryParams: { page: 'professionals' }, exact: true },
  { label: 'Crisis Support',      icon: 'bi-telephone-fill',    path: '/support/crisis',                                    exact: false },
];

@Component({
  selector: 'app-support-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    /* ══════════════════════════════════════════════════════════
       HOST — 240px shell, exact mirror of dashboard.css
    ══════════════════════════════════════════════════════════ */
    :host {
      display: flex;
      flex-direction: column;
      width: 240px;
      min-width: 240px;
      max-width: 240px;
      height: 100%;
      background: #fff;
      border-right: 1px solid #E2ECF0;
      padding: 24px 16px 16px;
      flex-shrink: 0;
      box-sizing: border-box;
      font-family: 'Sora', sans-serif;
      overflow-y: auto;
    }

    :host::-webkit-scrollbar { width: 4px; }
    :host::-webkit-scrollbar-track { background: transparent; }
    :host::-webkit-scrollbar-thumb { background: #E2ECF0; border-radius: 10px; }

    /* ── Logo / Brand ─────────────────────────────────────────── */
    .sidebar-logo {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 32px;
      padding: 0 4px;
      flex-shrink: 0;
      text-decoration: none;
    }

    .logo-icon {
      width: 38px;
      height: 38px;
      background: #0B8FAC;
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

    .logo-name {
      font-size: 15px;
      font-weight: 700;
      color: #0D1B3E;
      line-height: 1.2;
    }

    .logo-sub {
      font-size: 11px;
      color: #64748B;
      font-weight: 400;
    }

    /* ── Nav shell ────────────────────────────────────────────── */
    .sidebar-nav {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    /* ── Section label — exact dashboard clone ────────────────── */
    .nav-section-label {
      font-size: 10px;
      font-weight: 600;
      letter-spacing: .07em;
      text-transform: uppercase;
      color: #64748B;
      padding: 0 8px;
      margin-bottom: 6px;
      margin-top: 8px;
    }

    .nav-section-label.first {
      margin-top: 0;
    }

    /* ── Nav item — exact dashboard clone ─────────────────────── */
    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 10px;
      border-radius: 9px;
      font-size: 13px;
      font-weight: 500;
      color: #4A5568;
      text-decoration: none;
      transition: background .15s, color .15s;
      font-family: 'Sora', sans-serif;
      cursor: pointer;
      margin-bottom: 2px;
    }

    .nav-item i {
      font-size: 15px;
      flex-shrink: 0;
      opacity: .7;
      width: 17px;
      text-align: center;
    }

    .nav-item:hover {
      background: #F4F9FB;
      color: #0D1B3E;
    }

    .nav-item:hover i { opacity: 1; }

    .nav-item.active,
    .nav-item.active-nav {
      background: rgba(11, 143, 172, .08);
      color: #076E86;
      font-weight: 600;
    }

    .nav-item.active i,
    .nav-item.active-nav i { opacity: 1; }

    /* ── Conversation items ────────────────────────────────────── */
    .conv-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 10px;
      border-radius: 9px;
      font-size: 13px;
      font-weight: 500;
      color: #4A5568;
      text-decoration: none;
      transition: background .15s, color .15s;
      font-family: 'Sora', sans-serif;
      cursor: pointer;
      margin-bottom: 2px;
      border: none;
      background: transparent;
      width: 100%;
      text-align: left;
    }

    .conv-item i {
      font-size: 14px;
      flex-shrink: 0;
      opacity: .65;
      width: 17px;
      text-align: center;
    }

    .conv-item:hover {
      background: #F4F9FB;
      color: #0D1B3E;
    }

    .conv-item:hover i { opacity: 1; }

    .conv-item.active {
      background: rgba(11, 143, 172, .08);
      color: #076E86;
      font-weight: 600;
    }

    .conv-item.active i { opacity: 1; }

    .conv-title {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      max-width: 150px;
      flex: 1;
      min-width: 0;
    }

    /* Risk badges */
    .risk-high {
      font-size: 0.65rem;
      padding: 1px 6px;
      border-radius: 999px;
      background: rgba(220,38,38,.12);
      color: #dc2626;
      font-weight: 600;
      white-space: nowrap;
      flex-shrink: 0;
    }
    .risk-medium {
      font-size: 0.65rem;
      padding: 1px 6px;
      border-radius: 999px;
      background: rgba(234,179,8,.15);
      color: #a17800;
      font-weight: 600;
      white-space: nowrap;
      flex-shrink: 0;
    }

    /* Delete button — reveal on hover */
    .conv-item-wrap {
      position: relative;
    }

    .conv-item-wrap .delete-btn {
      position: absolute;
      right: 6px;
      top: 50%;
      transform: translateY(-50%);
      opacity: 0;
      transition: opacity .15s;
      background: transparent;
      border: none;
      color: #94A3B8;
      padding: 2px 4px;
      border-radius: 5px;
      cursor: pointer;
      font-size: 11px;
      line-height: 1;
    }

    .conv-item-wrap:hover .delete-btn { opacity: 1; }
    .conv-item-wrap .delete-btn:hover { color: #dc2626; }

    /* Confirm delete strip */
    .delete-confirm {
      display: flex;
      align-items: center;
      gap: 6px;
      margin: 4px 10px 6px;
      padding: 6px 10px;
      border-top: 1px solid #E2ECF0;
    }
    .delete-confirm span {
      flex: 1;
      font-size: 11px;
      color: #64748B;
    }
    .delete-confirm .btn-yes {
      font-size: 11px;
      padding: 2px 10px;
      background: #DC2626;
      color: #fff;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-family: 'Sora', sans-serif;
    }
    .delete-confirm .btn-no {
      font-size: 11px;
      padding: 2px 10px;
      background: transparent;
      color: #4A5568;
      border: 1px solid #E2ECF0;
      border-radius: 6px;
      cursor: pointer;
      font-family: 'Sora', sans-serif;
    }

    /* Empty state */
    .conv-empty {
      font-size: 11px;
      color: #94A3B8;
      padding: 6px 10px;
      font-style: italic;
    }

    /* Skeleton shimmer */
    .skeleton {
      height: 36px;
      border-radius: 9px;
      background: linear-gradient(90deg, #F4F9FB 25%, #E2ECF0 50%, #F4F9FB 75%);
      background-size: 200% 100%;
      animation: shimmer 1.4s infinite;
      margin-bottom: 4px;
    }
    @keyframes shimmer {
      0%   { background-position: -200% 0; }
      100% { background-position:  200% 0; }
    }

    /* ── New conversation CTA ──────────────────────────────────── */
    .new-conv-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      padding: 9px 12px;
      font-size: 13px;
      font-weight: 600;
      font-family: 'Sora', sans-serif;
      color: #fff;
      background: #0B8FAC;
      border: none;
      border-radius: 9px;
      cursor: pointer;
      transition: background .2s;
      margin-bottom: 16px;
      flex-shrink: 0;
    }
    .new-conv-btn:hover { background: #076E86; }

    /* ── Crisis button — dashboard style ──────────────────────── */
    .crisis-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      padding: 11px;
      background: rgba(220, 38, 38, .08);
      border: 1.5px solid rgba(220, 38, 38, .2);
      border-radius: 12px;
      color: #DC2626;
      font-size: 13px;
      font-weight: 600;
      font-family: 'Sora', sans-serif;
      cursor: pointer;
      margin: 20px 0 16px;
      transition: background .2s;
      text-decoration: none;
    }
    .crisis-btn:hover {
      background: rgba(220, 38, 38, .14);
      color: #DC2626;
    }
    .crisis-btn i { font-size: 15px; flex-shrink: 0; }

    /* ── User profile footer — exact dashboard clone ───────────── */
    .sidebar-user {
      margin-top: auto;
      border-top: 1px solid #E2ECF0;
      padding-top: 16px;
      display: flex;
      align-items: center;
      gap: 9px;
      padding: 10px 8px;
      border-radius: 10px;
      cursor: pointer;
      transition: background .15s;
      flex-shrink: 0;
    }
    .sidebar-user:hover { background: #F4F9FB; }

    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: #E2ECF0;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 13px;
      font-weight: 700;
      color: #64748B;
      flex-shrink: 0;
      object-fit: cover;
      overflow: hidden;
    }

    .user-avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 50%;
    }

    .user-info { flex: 1; min-width: 0; }

    .user-name {
      font-size: 13px;
      font-weight: 600;
      color: #0D1B3E;
      line-height: 1.2;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .user-email {
      font-size: 11px;
      color: #64748B;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* Logout icon action */
    .logout-btn {
      background: transparent;
      border: none;
      cursor: pointer;
      color: #94A3B8;
      padding: 4px;
      border-radius: 6px;
      transition: color .15s, background .15s;
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .logout-btn:hover { color: #DC2626; background: rgba(220,38,38,.06); }
    .logout-btn i { font-size: 14px; }

    /* ── Spacer util ──────────────────────────────────────────── */
    .fill { flex: 1; }
  `],
  template: `
    <!-- ══════════════════════════════════════════════════════
         LOGO / BRAND — 32px bottom margin, 4px side padding
    ══════════════════════════════════════════════════════ -->
    <a class="sidebar-logo"
       routerLink="/dashboard"
       [queryParams]="{ page: 'home' }">
      <div class="logo-icon">
        <svg viewBox="0 0 24 24" fill="white">
          <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 14.5v-9l6 4.5-6 4.5z" />
        </svg>
      </div>
      <div>
        <div class="logo-name">GhaithAI</div>
        <div class="logo-sub">Personal Support</div>
      </div>
    </a>

    <!-- ══════════════════════════════════════════════════════
         NEW CONVERSATION CTA
    ══════════════════════════════════════════════════════ -->
    <button type="button" class="new-conv-btn" (click)="startNewSession()">
      <i class="bi bi-plus-lg"></i>
      New Conversation
    </button>

    <!-- ══════════════════════════════════════════════════════
         NAVIGATION — Your Space
    ══════════════════════════════════════════════════════ -->
    <nav class="sidebar-nav">
      <div class="nav-section-label first">Your Space</div>

      @for (item of yourSpaceItems; track item.label) {
        <a class="nav-item"
           [routerLink]="item.path"
           [queryParams]="item.queryParams ?? null"
           routerLinkActive="active-nav"
           [routerLinkActiveOptions]="{ exact: item.exact ?? true }">
          <i class="bi {{ item.icon }}"></i>
          {{ item.label }}
        </a>
      }

      <!-- ── Get Help ───────────────────────────────────── -->
      <div class="nav-section-label" style="margin-top: 24px;">Get Help</div>

      @for (item of getHelpItems; track item.label) {
        <a class="nav-item"
           [routerLink]="item.path"
           [queryParams]="item.queryParams ?? null"
           routerLinkActive="active-nav"
           [routerLinkActiveOptions]="{ exact: item.exact ?? true }">
          <i class="bi {{ item.icon }}"></i>
          {{ item.label }}
        </a>
      }

      <!-- ══════════════════════════════════════════════════════
           RECENT CONVERSATIONS — Dynamic injection
      ══════════════════════════════════════════════════════ -->
      <div class="nav-section-label" style="margin-top: 24px;">Recent Conversations</div>

      <!-- Loading skeletons -->
      @if (chatStore.isLoadingSessions()) {
        <div class="skeleton"></div>
        <div class="skeleton"></div>
        <div class="skeleton"></div>
      }

      <!-- Empty state -->
      @if (!chatStore.isLoadingSessions() && chatStore.sessions().length === 0) {
        <p class="conv-empty">No conversations yet</p>
      }

      <!-- Session list -->
      @if (!chatStore.isLoadingSessions()) {
        @for (session of chatStore.sessions(); track session.id) {
          <div class="conv-item-wrap">
            <button
              type="button"
              class="conv-item"
              [class.active]="chatStore.activeSession()?.id === session.id"
              (click)="selectSession(session)">
              <i class="bi bi-chat-left-text"></i>
              <span class="conv-title">{{ session.title || 'New Consultation' }}</span>

              @if (session.riskLevel === 'high') {
                <span class="risk-high">High</span>
              }
              @if (session.riskLevel === 'medium') {
                <span class="risk-medium">Med</span>
              }
            </button>

            <!-- Delete trigger -->
            <button type="button"
                    class="delete-btn"
                    title="Delete"
                    (click)="onDeleteClick(session.id, $event)">
              <i class="bi bi-trash3"></i>
            </button>
          </div>

          <!-- Inline delete confirm -->
          @if (confirmDeleteId() === session.id) {
            <div class="delete-confirm">
              <span>Delete session?</span>
              <button type="button" class="btn-yes" (click)="confirmDelete(session.id)">Yes</button>
              <button type="button" class="btn-no" (click)="cancelDelete()">No</button>
            </div>
          }
        }
      }
    </nav>

    <!-- ══════════════════════════════════════════════════════
         CRISIS BUTTON — dashboard identical
    ══════════════════════════════════════════════════════ -->
    <a routerLink="/support/crisis" class="crisis-btn">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"
           style="width:16px;height:16px;flex-shrink:0;">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
      </svg>
      I need help now
    </a>

    <!-- ══════════════════════════════════════════════════════
         USER PROFILE FOOTER — exact dashboard clone
    ══════════════════════════════════════════════════════ -->
    <div class="sidebar-user" style="border-top: 1px solid #E2ECF0; margin-top: auto;"
         routerLink="/support/settings">
      <!-- Avatar -->
      <div class="user-avatar">
        @if (userAvatarUrl()) {
          <img [src]="userAvatarUrl()" alt="Profile picture" />
        } @else {
          {{ userInitial() }}
        }
      </div>

      <!-- Info -->
      <div class="user-info">
        <div class="user-name">{{ userName() }}</div>
        <div class="user-email">{{ userEmail() }}</div>
      </div>

      <!-- Logout -->
      <button type="button"
              class="logout-btn"
              title="Sign out"
              (click)="onLogout($event)">
        <i class="bi bi-box-arrow-right"></i>
      </button>
    </div>
  `,
})
export class SupportSidebar implements OnInit {
  readonly chatStore        = inject(ChatStore);
  private readonly chatHttp = inject(ChatHttpService);
  private readonly router   = inject(Router);
  private readonly auth     = inject(AuthService);

  readonly confirmDeleteId = signal<string | null>(null);

  readonly yourSpaceItems = YOUR_SPACE_ITEMS;
  readonly getHelpItems   = GET_HELP_ITEMS;

  // ── User profile from localStorage ────────────────────────────────────────
  private readonly _user = signal<{ fullName?: string; email?: string; profilePicture?: string } | null>(
    this.readUser()
  );

  readonly userName    = computed(() => this._user()?.fullName || 'My Account');
  readonly userEmail   = computed(() => this._user()?.email   || '');
  readonly userInitial = computed(() => (this._user()?.fullName?.[0] ?? '?').toUpperCase());
  readonly userAvatarUrl = computed(() => this._user()?.profilePicture || null);

  // ─────────────────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.loadSessions();
  }

  private readUser(): { fullName?: string; email?: string; profilePicture?: string } | null {
    try {
      const raw = localStorage.getItem('user');
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  loadSessions(): void {
    this.chatStore.isLoadingSessions.set(true);
    this.chatHttp.getSessions(1, 30).subscribe({
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

  onDeleteClick(sessionId: string, event: MouseEvent): void {
    event.stopPropagation();
    this.confirmDeleteId.set(sessionId);
  }

  confirmDelete(sessionId: string): void {
    this.chatHttp.deleteSession(sessionId).subscribe({
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

  onLogout(event: MouseEvent): void {
    event.stopPropagation();
    this.auth.logout();
    this.chatStore.reset();
    this.router.navigate(['/auth/login']);
  }
}
