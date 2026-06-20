import { TestBed } from '@angular/core/testing';

import { DoctorPatient } from './doctor-patient';

describe('DoctorPatient', () => {
  let service: DoctorPatient;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DoctorPatient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
