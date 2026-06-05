// File: src/app/support/layout.ts

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SupportSidebar } from '../../components/layout/SupportSidebar';
import { Navbar } from '../../components/layout/Navbar';

@Component({
  selector: 'app-support-layout',
  standalone: true,
  imports: [RouterOutlet, SupportSidebar, Navbar],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100vh;
      overflow: hidden;
      background-color: var(--chat-bg);

      /* ── Design tokens scoped to the support layout ────────────── */
      --chat-bg:                 #ffffff;
      --chat-fg:                 #1a1a1a;
      --chat-primary:            #0d9488;
      --chat-primary-fg:         #ffffff;
      --chat-secondary:          #e6f4ea;
      --chat-secondary-fg:       #064e3b;
      --chat-muted:              #f3f4f6;
      --chat-muted-fg:           #4b5563;
      --chat-accent:             #e6f4ea;
      --chat-accent-fg:          #0f766e;
      --chat-destructive:        #dc2626;
      --chat-destructive-fg:     #fff8f8;
      --chat-border:             #e5e7eb;
      --chat-sidebar:            #f4fbf8;
      --chat-sidebar-fg:         #1e293b;
      --chat-sidebar-border:     #e2e8f0;
      --chat-ring:               #0d9488;

      /* Fallback aliases so child components using old var() names still resolve */
      --background:              var(--chat-bg);
      --foreground:              var(--chat-fg);
      --primary:                 var(--chat-primary);
      --primary-foreground:      var(--chat-primary-fg);
      --secondary:               var(--chat-secondary);
      --secondary-foreground:    var(--chat-secondary-fg);
      --muted:                   var(--chat-muted);
      --muted-foreground:        var(--chat-muted-fg);
      --accent:                  var(--chat-accent);
      --accent-foreground:       var(--chat-accent-fg);
      --destructive:             var(--chat-destructive);
      --destructive-foreground:  var(--chat-destructive-fg);
      --border:                  var(--chat-border);
      --input:                   var(--chat-border);
      --ring:                    var(--chat-ring);
      --sidebar:                 var(--chat-sidebar);
      --sidebar-foreground:      var(--chat-sidebar-fg);
      --sidebar-primary:         var(--chat-primary);
      --sidebar-primary-foreground: var(--chat-primary-fg);
      --sidebar-accent:          var(--chat-accent);
      --sidebar-accent-foreground: var(--chat-accent-fg);
      --sidebar-border:          var(--chat-sidebar-border);
      --sidebar-ring:            var(--chat-ring);
      --radius:                  0.625rem;
    }

    :host ::-webkit-scrollbar       { width: 5px; }
    :host ::-webkit-scrollbar-track { background: transparent; }
    :host ::-webkit-scrollbar-thumb { background: var(--chat-border); border-radius: 3px; }
    :host ::-webkit-scrollbar-thumb:hover { background: var(--chat-muted-fg); }

    /* ── Bootstrap override: ensure sidebar/main fill full height ── */
    .support-sidebar-col {
      width: 280px;
      min-width: 280px;
      max-width: 280px;
      border-right: 1px solid var(--chat-sidebar-border);
      height: 100%;
      overflow: hidden;
    }
    .support-main-col {
      flex: 1 1 0%;
      min-width: 0;
      height: 100%;
      overflow: hidden;
    }
    .support-body-row {
      flex: 1 1 0%;
      min-height: 0;
      overflow: hidden;
    }
  `],
  template: `
    <!-- Mobile top nav (visible only below md breakpoint via Bootstrap d-md-none) -->
    <app-navbar class="d-md-none flex-shrink-0" />

    <!-- Main body: sidebar + content -->
    <div class="d-flex support-body-row w-100">

      <!-- Sidebar: hidden on mobile, visible on md+ -->
      <aside class="support-sidebar-col d-none d-md-flex flex-column">
        <app-support-sidebar class="w-100 h-100" />
      </aside>

      <!-- Page content rendered by child route -->
      <main class="support-main-col d-flex flex-column overflow-hidden">
        <router-outlet />
      </main>
    </div>
  `,
})
export class SupportLayout {}
