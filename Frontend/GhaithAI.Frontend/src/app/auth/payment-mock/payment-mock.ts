import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-payment-mock',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-mock.html',
  styleUrls: ['./payment-mock.css']
})
export class PaymentMockComponent implements OnInit {

  paymentId = '';
  amount = '';
  loading = false;
  confirmed = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.paymentId = this.route.snapshot.queryParamMap.get('paymentId') ?? '';
    this.amount = this.route.snapshot.queryParamMap.get('amount') ?? '100';
  }

  confirmPayment(): void {
    this.loading = true;
    this.error = '';

    this.http.post(
      `${environment.apiUrl}/payments/${this.paymentId}/mock-confirm`,
      {}
    ).subscribe({
      next: () => {
        this.loading = false;
        this.confirmed = true;
        setTimeout(() => {
          this.router.navigate(['/payment/success']);
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || 'Something went wrong.';
      }
    });
  }

  cancelPayment(): void {
    this.router.navigate(['/dashboard']);
  }
}