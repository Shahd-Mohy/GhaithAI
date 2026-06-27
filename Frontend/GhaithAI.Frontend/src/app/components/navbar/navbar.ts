import { Component }
  from '@angular/core';

import { RouterLink }
  from '@angular/router';

import { AuthService }
  from '../../services/auth';

import { NotificationBellComponent } from '../../shared/notification-bell/notification-bell';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',

  standalone: true,

  imports: [
    RouterLink,
    CommonModule,
    NotificationBellComponent
  ],

  templateUrl: './navbar.html',

  styleUrls: ['./navbar.css']
})
export class NavbarComponent {

  constructor(
    public authService: AuthService
  ) { }

  logout() {

    this.authService.logout();
  }
}
