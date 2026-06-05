import { Component } from '@angular/core';

@Component({
  selector: 'app-chat-session-page',
  standalone: true,
  template: `
    <div class="p-8">
      <h1 class="text-3xl font-bold mb-6">Chat Sessions</h1>
      <p class="text-muted-foreground">Your chat history and sessions will appear here.</p>
    </div>
  `
})
export class ChatSessionPage {}
