// File: src/app/auth/layout.ts

import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    :host {
      display: flex;
      min-height: 100vh;
      align-items: center;
      justify-content: center;
      background-color: var(--background);
    }
  `],
  template: `<router-outlet />`,
})
export class AuthLayout {}
