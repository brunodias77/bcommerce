import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Button } from '../../../../shared/components/button';
import { CartItem, OrderSummary, CouponData, CartStep } from '../../interfaces/cart.interfaces';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CommonModule, FormsModule, Button],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css'
})
export class CartPage {
  // Stepper data
  steps = signal<CartStep[]>([
    { id: 1, name: 'Carrinho', isActive: true, isCompleted: false },
    { id: 2, name: 'Endereços', isActive: false, isCompleted: false },
    { id: 3, name: 'Pagamento', isActive: false, isCompleted: false },
    { id: 4, name: 'Confirmação', isActive: false, isCompleted: false }
  ]);

  // Cart items
  cartItems = signal<CartItem[]>([
    {
      id: '1',
      name: 'Galaxy X 2025',
      price: 5426.22,
      quantity: 1,
      image: 'https://trae-api-us.mchost.guru/api/ide/v1/text_to_image?prompt=modern%20smartphone%20galaxy%20x%202025%20sleek%20design%20black%20color%20premium%20device&image_size=square',
      category: 'Smartphone'
    }
  ]);

  // Order summary
  orderSummary = signal<OrderSummary>({
    subtotal: 2000.00,
    discounts: 0.00,
    shipping: 0.00,
    totalInstallment: 2000.00,
    installmentFee: 200.00,
    totalCash: 1800.00,
    savings: 200.00
  });

  // Coupon
  couponCode = signal<string>('');
  couponData = signal<CouponData | null>(null);

  // Methods
  removeItem(itemId: string): void {
    const currentItems = this.cartItems();
    const updatedItems = currentItems.filter(item => item.id !== itemId);
    this.cartItems.set(updatedItems);
  }

  applyCoupon(): void {
    const code = this.couponCode();
    if (code.trim()) {
      // Simulate coupon validation
      console.log('Applying coupon:', code);
      // Here you would typically call an API to validate the coupon
    }
  }

  updateQuantity(itemId: string, newQuantity: number): void {
    if (newQuantity < 1) return;
    
    const currentItems = this.cartItems();
    const updatedItems = currentItems.map(item => 
      item.id === itemId ? { ...item, quantity: newQuantity } : item
    );
    this.cartItems.set(updatedItems);
  }

  goBack(): void {
    console.log('Going back');
    // Navigate to previous page
  }

  continue(): void {
    console.log('Continuing to next step');
    // Navigate to addresses page
  }
}
