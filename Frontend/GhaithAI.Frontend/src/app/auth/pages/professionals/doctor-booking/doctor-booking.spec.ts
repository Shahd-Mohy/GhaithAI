import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorBooking } from './doctor-booking';

describe('DoctorBooking', () => {
  let component: DoctorBooking;
  let fixture: ComponentFixture<DoctorBooking>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorBooking],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorBooking);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
