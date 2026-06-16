// File: src/components/layout/Navbar.ts

import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  signal,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { SupportSidebar } from './SupportSidebar';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, SupportSidebar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .navbar-support {
      background-color: var(--background, #fff);
      border-bottom: 1px solid var(--border, #e5e5e5);
      min-height: 56px;
    }
    .navbar-btn {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 0.5rem;
      background: transparent;
      border: none;
      transition: background-color 0.15s ease;
      color: var(--foreground, #1a1a1a);
    }
    .navbar-btn:hover {
      background-color: var(--accent, #f5f5f5);
    }
    .crisis-btn {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 0.5rem;
      background-color: rgba(220, 38, 38, 0.1);
      color: var(--destructive, #dc2626);
      text-decoration: none;
      transition: background-color 0.15s ease;
    }
    .crisis-btn:hover {
      background-color: rgba(220, 38, 38, 0.18);
      color: var(--destructive, #dc2626);
    }
    /* Mobile drawer */
    .mobile-overlay {
      position: fixed;
      inset: 0;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 1040;
      animation: fadeIn 0.2s ease-out;
    }
    .mobile-drawer {
      position: fixed;
      top: 0;
      bottom: 0;
      left: 0;
      width: 280px;
      background-color: var(--sidebar, #f9f9f9);
      z-index: 1050;
      animation: slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
      box-shadow: 4px 0 24px rgba(0,0,0,0.1);
      display: flex;
      flex-direction: column;
    }
    @keyframes fadeIn {
      from { opacity: 0; }
      to   { opacity: 1; }
    }
    @keyframes slideIn {
      from { transform: translateX(-100%); }
      to   { transform: translateX(0); }
    }
    .logo-icon {
      width: 32px;
      height: 32px;
      border-radius: 0.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: var(--primary, #1a1a1a);
    }
  `],
  template: `
    <!-- Top Bar — Bootstrap navbar; hidden on md+ via d-md-none on host -->
    <header class="navbar-support d-flex align-items-center justify-content-between px-3">

      <!-- Hamburger -->
      <button type="button"
              class="navbar-btn"
              (click)="toggleMobileMenu()"
              aria-label="Open navigation menu">
        <i class="bi bi-list fs-4"></i>
      </button>

      <!-- Center Logo -->
      <a routerLink="/support/dashboard" class="d-flex align-items-center gap-2 text-decoration-none">
        <div class="logo-icon">
          <i class="bi bi-cpu-fill small" style="color: var(--primary-foreground, #f8f8f8);"></i>
        </div>
        <span class="fw-semibold fs-5" style="color: var(--foreground, #1a1a1a);">GhaithAI</span>
      </a>

      <!-- Crisis action -->
      <a routerLink="/support/crisis" class="crisis-btn" aria-label="Crisis Support">
        <i class="bi bi-heart-pulse-fill fs-5"></i>
      </a>
    </header>

    <!-- Mobile Navigation Drawer -->
    @if (isMobileMenuOpen()) {
      <!-- Backdrop -->
      <div class="mobile-overlay" (click)="toggleMobileMenu()"></div>

      <!-- Drawer -->
      <div class="mobile-drawer">
        <app-support-sidebar class="w-100 h-100 d-block" />
      </div>
    }
  `,
})
export class Navbar implements OnInit, OnDestroy {
  readonly isMobileMenuOpen = signal<boolean>(false);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.isMobileMenuOpen.set(false);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((v) => !v);
  }
}
