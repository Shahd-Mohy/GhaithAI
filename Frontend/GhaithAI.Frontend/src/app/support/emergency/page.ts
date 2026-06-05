import { Component } from '@angular/core';
import { CrisisOverlay } from '../../../components/chat/CrisisOverlay';

@Component({
  selector: 'app-emergency-page',
  standalone: true,
  imports: [CrisisOverlay],
  template: `
    <div class="p-8">
      <app-crisis-overlay />
    </div>
  `
})
export class EmergencyPage {}
