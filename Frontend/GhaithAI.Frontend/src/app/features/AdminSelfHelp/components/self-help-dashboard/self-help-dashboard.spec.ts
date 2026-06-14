import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelfHelpDashboard } from './self-help-dashboard';

describe('SelfHelpDashboard', () => {
  let component: SelfHelpDashboard;
  let fixture: ComponentFixture<SelfHelpDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelfHelpDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(SelfHelpDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
