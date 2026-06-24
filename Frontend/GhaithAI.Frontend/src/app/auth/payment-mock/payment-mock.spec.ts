import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentMock } from './payment-mock';

describe('PaymentMock', () => {
  let component: PaymentMock;
  let fixture: ComponentFixture<PaymentMock>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentMock],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentMock);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
