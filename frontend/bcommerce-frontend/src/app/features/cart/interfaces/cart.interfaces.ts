export interface CartItem {
  id: string;
  name: string;
  price: number;
  quantity: number;
  image: string;
  category?: string;
}

export interface OrderSummary {
  subtotal: number;
  discounts: number;
  shipping: number;
  totalInstallment: number;
  installmentFee: number;
  totalCash: number;
  savings: number;
}

export interface CouponData {
  code: string;
  discount: number;
  isValid: boolean;
}

export interface CartStep {
  id: number;
  name: string;
  isActive: boolean;
  isCompleted: boolean;
}