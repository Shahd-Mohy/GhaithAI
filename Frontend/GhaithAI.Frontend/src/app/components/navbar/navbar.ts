import { Component }
  from '@angular/core';

import { RouterLink }
  from '@angular/router';

import { AuthService }
  from '../../services/auth';
@Component({
  selector: 'app-navbar',

  standalone: true,

  imports: [
    RouterLink
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
