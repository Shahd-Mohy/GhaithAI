import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface JournalEntry {
  id: string;
  dateStr: string;
  title: string;
  body: string;
  tags: string[];
  moodEmoji: string;
  moodIconColor: string;
}

@Component({
  selector: 'app-journal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './journal.html',
  styleUrl: './journal.css'
})
export class JournalComponent implements OnInit {
  searchQuery: string = '';
  isCreatingEntry: boolean = false;
  
  // New entry form state
  newEntryMood: string = '';
  newEntryTitle: string = '';
  newEntryBody: string = '';
  newEntryTags: string[] = [];
  currentTagInput: string = '';

  moods = [
    { label: 'Happy', icon: '🙂' },
    { label: 'Sad', icon: '😢' },
    { label: 'Neutral', icon: '😐' },
    { label: 'Grateful', icon: '🙏' },
    { label: 'Anxious', icon: '😰' },
    { label: 'Peaceful', icon: '😌' },
    { label: 'Tired', icon: '😴' },
    { label: 'Hopeful', icon: '✨' }
  ];

  prompts = [
    'What are you grateful for today?',
    "What's been on your mind lately?",
    'Describe a moment that made you smile today',
    'What challenge are you currently facing?',
    'Write a letter to your future self',
    'What would make today better?'
  ];

  get todayLabel(): string {
    return new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' });
  }

  entries: JournalEntry[] = [
    {
      id: '1',
      dateStr: 'Sunday, May 24, 2026',
      title: 'A Good Day',
      body: 'Today was a really good day. I managed to complete my work on time and had a nice conversation with a friend. Feeling grateful for the small things.',
      tags: ['work', 'gratitude', 'friends'],
      moodEmoji: '🙂',
      moodIconColor: '#059669' // green-ish
    },
    {
      id: '2',
      dateStr: 'Saturday, May 23, 2026',
      title: 'Feeling Overwhelmed',
      body: 'Work has been stressful lately. I need to find better ways to manage my time and set boundaries. Tomorrow will be better.',
      tags: ['work', 'stress'],
      moodEmoji: '☁️',
      moodIconColor: '#64748B' // gray-ish
    }
  ];

  get totalEntries(): number {
    return this.entries.length;
  }

  ngOnInit() {
  }

  toggleNewEntry() {
    this.isCreatingEntry = !this.isCreatingEntry;
    if (this.isCreatingEntry) {
      this.resetForm();
    }
  }

  resetForm() {
    this.newEntryMood = '';
    this.newEntryTitle = '';
    this.newEntryBody = '';
    this.newEntryTags = [];
    this.currentTagInput = '';
  }

  selectMood(mood: string) {
    this.newEntryMood = mood;
  }

  applyPrompt(prompt: string) {
    if (this.newEntryBody) {
      this.newEntryBody += '\n\n' + prompt + '\n';
    } else {
      this.newEntryBody = prompt + '\n';
    }
  }

  addTag() {
    const tag = this.currentTagInput.trim();
    if (tag && !this.newEntryTags.includes(tag)) {
      this.newEntryTags.push(tag);
    }
    this.currentTagInput = '';
  }

  removeTag(index: number) {
    this.newEntryTags.splice(index, 1);
  }

  saveEntry() {
    if (!this.newEntryBody.trim()) return;

    const newEntry: JournalEntry = {
      id: Math.random().toString(36).substr(2, 9),
      dateStr: this.todayLabel,
      title: this.newEntryTitle || 'Untitled Entry',
      body: this.newEntryBody,
      tags: [...this.newEntryTags],
      moodEmoji: this.moods.find(m => m.label === this.newEntryMood)?.icon || '📝',
      moodIconColor: '#0B8FAC'
    };

    this.entries.unshift(newEntry);
    this.isCreatingEntry = false;
  }
}
