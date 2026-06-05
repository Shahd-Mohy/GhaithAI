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
      --chat-bg:                 #F4F9FB; /* Light blue-teal tint */
      --chat-fg:                 #0D1B3E; /* Dark navy */
      --chat-primary:            #0B8FAC; /* Teal brand primary */
      --chat-primary-fg:         #ffffff; /* White text on primary buttons */
      --chat-secondary:          rgba(11, 143, 172, 0.08); /* Light teal-blue tint */
      --chat-secondary-fg:       #076E86; /* Darker brand accent */
      --chat-muted:              #F4F9FB;
      --chat-muted-fg:           #64748B; /* Slate gray */
      --chat-accent:             rgba(11, 143, 172, 0.08);
      --chat-accent-fg:          #076E86;
      --chat-destructive:        #DC2626; /* Warning/crisis red */
      --chat-destructive-fg:     #ffffff;
      --chat-border:             #E2ECF0; /* Soft border */
      --chat-sidebar:            #ffffff; /* White background sidebar */
      --chat-sidebar-fg:         #4A5568; /* Slate gray sidebar text */
      --chat-sidebar-border:     #E2ECF0;
      --chat-ring:               #0B8FAC;

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
      width: 240px;
      min-width: 240px;
      max-width: 240px;
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
