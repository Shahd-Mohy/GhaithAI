import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ScheduleSlot {
  id: number;
  timeRange: string;
  type: string; // 'Therapy' | 'Initial' | 'Follow-up' | 'Emergency'
  status: string; // 'Active' | 'Booked' | 'Blocked'
  duration: number;
  patientName: string;
  notes: string;
}

@Component({
  selector: 'app-doctor-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './doctor-schedule.component.html',
  styleUrls: ['./doctor-schedule.component.css']
})
export class DoctorScheduleComponent {
  // Navigation & view toggles (replicates Self Help layout mechanism)
  showForm = false;
  isEditMode = false;
  loading = false;
  actionLoading: number | null = null;

  // Pagination parameters
  currentPage = 1;
  totalPages = 2;
  hasPrevious = false;
  hasNext = true;

  // Mock list items representing Doctor's schedule slots
  items: ScheduleSlot[] = [
    {
      id: 1,
      timeRange: '09:00 AM - 10:00 AM',
      type: 'Therapy',
      status: 'Booked',
      duration: 60,
      patientName: 'Ahmed Hassan',
      notes: 'Focus on breathing control and relaxation techniques.'
    },
    {
      id: 2,
      timeRange: '10:30 AM - 11:30 AM',
      type: 'Initial',
      status: 'Booked',
      duration: 60,
      patientName: 'Fatima Khaled',
      notes: 'Intake assessment and distress level screening.'
    },
    {
      id: 3,
      timeRange: '01:00 PM - 02:00 PM',
      type: 'Follow-up',
      status: 'Active',
      duration: 60,
      patientName: '— (Open Slot)',
      notes: 'Open for routine follow-up check-ins.'
    },
    {
      id: 4,
      timeRange: '02:30 PM - 03:00 PM',
      type: 'Follow-up',
      status: 'Active',
      duration: 30,
      patientName: '— (Open Slot)',
      notes: 'Brief medication check or follow-up.'
    },
    {
      id: 5,
      timeRange: '04:00 PM - 05:00 PM',
      type: 'Emergency',
      status: 'Blocked',
      duration: 60,
      patientName: 'Crisis Override',
      notes: 'Reserved slot for high-risk alerts and urgent clinical needs.'
    }
  ];

  // Forms state matching Self Help model bindings
  formData = {
    id: 0,
    timeRange: '',
    type: 'Therapy',
    status: 'Active',
    duration: 60,
    patientName: '',
    notes: '',
    isActive: true
  };

  // Nested form rules list (replicates Self Help 'tips-section' structure)
  formRules = [
    { text: 'Verify patient consent form prior to starting.' },
    { text: 'Keep clinical notes ready in the Clinical Notes panel.' }
  ];

  // Form options
  slotTypes = [
    { label: 'Therapy Session', value: 'Therapy' },
    { label: 'Initial Assessment', value: 'Initial' },
    { label: 'Follow-up Consultation', value: 'Follow-up' },
    { label: 'Emergency Reserve', value: 'Emergency' }
  ];

  statusLevels = ['Active', 'Booked', 'Blocked'];

  // Form Action handlers (UI only)
  openAddMode(): void {
    this.isEditMode = false;
    this.formData = {
      id: 0,
      timeRange: '',
      type: 'Therapy',
      status: 'Active',
      duration: 60,
      patientName: '',
      notes: '',
      isActive: true
    };
    this.formRules = [
      { text: 'Verify patient consent form prior to starting.' },
      { text: 'Keep clinical notes ready in the Clinical Notes panel.' }
    ];
    this.showForm = true;
  }

  openEditMode(id: number): void {
    this.isEditMode = true;
    const item = this.items.find(i => i.id === id);
    if (item) {
      this.formData = {
        id: item.id,
        timeRange: item.timeRange,
        type: item.type,
        status: item.status,
        duration: item.duration,
        patientName: item.patientName === '— (Open Slot)' ? '' : item.patientName,
        notes: item.notes,
        isActive: item.status !== 'Blocked'
      };
      this.formRules = [
        { text: 'Ensure patient history is reviewed.' },
        { text: 'Check for any recent risk alerts.' }
      ];
      this.showForm = true;
    }
  }

  closeForm(): void {
    this.showForm = false;
  }

  addRuleInput(): void {
    this.formRules.push({ text: '' });
  }

  removeRuleInput(index: number): void {
    this.formRules.splice(index, 1);
  }

  save(): void {
    // UI layout template has no active backend operations, just toggle form back
    this.showForm = false;
  }
}
