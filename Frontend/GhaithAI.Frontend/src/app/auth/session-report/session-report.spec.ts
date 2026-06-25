import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SessionReport } from './session-report';

describe('SessionReport', () => {
  let component: SessionReport;
  let fixture: ComponentFixture<SessionReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionReport],
    }).compileComponents();

    fixture = TestBed.createComponent(SessionReport);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
