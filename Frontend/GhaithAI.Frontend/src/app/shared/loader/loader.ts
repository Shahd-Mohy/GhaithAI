import { Component } from '@angular/core';

import { NgxSpinnerModule }
from 'ngx-spinner';

@Component({
  selector: 'app-loader',

  standalone: true,

  imports: [
    NgxSpinnerModule
  ],

  templateUrl: './loader.html',

  styleUrls: ['./loader.css']
})
export class LoaderComponent {

}