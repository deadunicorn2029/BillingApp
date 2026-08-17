import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { ErrorResponse, Receipt, SubmitOrderRequest } from '../models/order.models';

interface ValidationProblemDetails {
  errors?: Record<string, string[]>;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly ordersUrl = `${environment.apiUrl}/orders`;

  constructor(private readonly http: HttpClient) {}

  submitOrder(request: SubmitOrderRequest): Observable<Receipt> {
    return this.http
      .post<Receipt>(this.ordersUrl, request)
      .pipe(catchError((error: HttpErrorResponse) => throwError(() => this.toErrorMessage(error))));
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    const body = error.error as ErrorResponse | ValidationProblemDetails | null;

    if (body && 'message' in body && body.message) {
      return body.message;
    }

    if (body && 'errors' in body && body.errors) {
      return Object.values(body.errors).flat().join(' ');
    }

    return error.message || 'Unexpected error while submitting the order.';
  }
}
