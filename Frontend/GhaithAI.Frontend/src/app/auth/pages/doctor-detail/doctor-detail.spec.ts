import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorDetail } from './doctor-detail';

describe('DoctorDetail', () => {
  let component: DoctorDetail;
  let fixture: ComponentFixture<DoctorDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
