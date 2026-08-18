import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from './services/order.service';
import { Receipt } from './models/order.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  readonly gateways = [
    { id: 'mock-gateway-a', label: 'Mock Gateway A (always succeeds)' },
    { id: 'mock-gateway-b', label: 'Mock Gateway B (~20% decline)' }
  ];

  readonly form = this.fb.group({
    orderNumber: ['', Validators.required],
    userId: ['', Validators.required],
    payableAmount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    paymentGatewayId: ['mock-gateway-a', Validators.required],
    description: ['']
  });

  submitting = false;
  receipt: Receipt | null = null;
  errorMessage: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly orderService: OrderService
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.receipt = null;
    this.errorMessage = null;

    const value = this.form.getRawValue();

    this.orderService
      .submitOrder({
        orderNumber: value.orderNumber!,
        userId: value.userId!,
        payableAmount: value.payableAmount!,
        paymentGatewayId: value.paymentGatewayId!,
        description: value.description || undefined
      })
      .subscribe({
        next: (receipt) => {
          this.receipt = receipt;
          this.submitting = false;
        },
        error: (message: string) => {
          this.errorMessage = message;
          this.submitting = false;
        }
      });
  }
}
