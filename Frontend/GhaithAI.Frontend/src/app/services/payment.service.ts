import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PaymentInitiatedResponse {
  paymentId: string;
  stripeSessionId: string;
  checkoutUrl: string;
}

export interface PaymentStatusResponse {
  paymentId: string;
  bookingId: string;
  status: 'Pending' | 'Paid' | 'Failed' | 'Cancelled';
  amount: number;
  currency: string;
  paidAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {

  private readonly baseUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  initiatePayment(bookingId: string): Observable<PaymentInitiatedResponse> {
    return this.http.post<PaymentInitiatedResponse>(`${this.baseUrl}/initiate`, { bookingId });
  }

  confirmMockPayment(sessionId: string): Observable<PaymentStatusResponse> {
    return this.http.post<PaymentStatusResponse>(
      `${this.baseUrl}/mock-confirm`,
      { sessionId }
    );
  }

  getPaymentStatus(bookingId: string): Observable<PaymentStatusResponse> {
    return this.http.get<PaymentStatusResponse>(`${this.baseUrl}/${bookingId}/status`);
  }

  redirectToCheckout(url: string): void {
    window.location.href = url;
  }
}