import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientSessionRoomComponent } from './patient-session-room.component';

describe('PatientSessionRoomComponent', () => {
  let component: PatientSessionRoomComponent;
  let fixture: ComponentFixture<PatientSessionRoomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientSessionRoomComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientSessionRoomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
