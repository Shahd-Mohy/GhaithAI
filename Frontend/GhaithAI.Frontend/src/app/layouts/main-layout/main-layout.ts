import { Component } from '@angular/core';

import { RouterOutlet } from '@angular/router';

import { NavbarComponent }
from '../../shared/navbar/navbar';

import { SidebarComponent }
from '../../shared/sidebar/sidebar';

@Component({
  selector: 'app-main-layout',

  standalone: true,

  imports: [
    RouterOutlet,
    NavbarComponent,
    SidebarComponent
  ],

  templateUrl: './main-layout.html',

  styleUrls: ['./main-layout.css']
})
export class MainLayoutComponent {

}