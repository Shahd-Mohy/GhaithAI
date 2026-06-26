import { Component }
  from '@angular/core';

import {
  RouterOutlet
} from '@angular/router';

import { NavbarComponent }
  from './components/navbar/navbar';
import { NotifecationException } from './components/NotifecationException/notifecation-exception/notifecation-exception';

@Component({
  selector: 'app-root',

  standalone: true,

  imports: [
    RouterOutlet, NotifecationException
  ],

  templateUrl: './app.html'
})
export class AppComponent { }
