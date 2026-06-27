import { Component } from '@angular/core';
import { Location } from '@angular/common';
@Component({
  selector: 'app-server-error',
  imports: [],
  templateUrl: './server-error.html',
  styleUrl: './server-error.css',
})

export class ServerError {
  constructor(private location: Location) { }
  goBack() { this.location.back(); }
}
