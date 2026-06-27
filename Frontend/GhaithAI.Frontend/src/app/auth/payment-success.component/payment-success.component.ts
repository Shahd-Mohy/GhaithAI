import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PaymentService, PaymentStatusResponse } from '../../services/payment.service';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment-success.component.html',
  styleUrls: ['./payment-success.component.css']
})
export class PaymentSuccessComponent implements OnInit {

  loading = true;
  error   = '';
  payment: PaymentStatusResponse | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    const sessionId = this.route.snapshot.queryParamMap.get('session_id');

    if (!sessionId) {
      this.error   = 'Invalid payment session.';
      this.loading = false;
      return;
    }

    this.paymentService.confirmPayment(sessionId).subscribe({
      next: (res) => {
        this.payment = res;
        this.loading = false;
      },
      error: (err) => {
        this.error   = err.error?.message || 'Failed to confirm payment.';
        this.loading = false;
      }
    });
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  goToBookings(): void {
    this.router.navigate(['/bookings']);
  }
}