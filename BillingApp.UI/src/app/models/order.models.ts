export interface SubmitOrderRequest {
  orderNumber: string;
  userId: string;
  payableAmount: number;
  paymentGatewayId: string;
  description?: string;
}

export interface Receipt {
  orderNumber: string;
  amount: number;
  timestamp: string;
  confirmationCode: string;
}

export interface ErrorResponse {
  message: string;
}
