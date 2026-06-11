import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterClinician } from './register-clinician';

describe('RegisterClinician', () => {
  let component: RegisterClinician;
  let fixture: ComponentFixture<RegisterClinician>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterClinician],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterClinician);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
