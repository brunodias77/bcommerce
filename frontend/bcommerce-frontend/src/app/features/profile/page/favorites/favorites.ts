import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface WishlistProduct {
  id: number;
  name: string;
  image: string;
  price: number;
  stockStatus: 'in-stock' | 'out-of-stock';
  stockLabel: string;
}

@Component({
  selector: 'app-favorites',
  imports: [CommonModule],
  templateUrl: './favorites.html',
  styleUrl: './favorites.css'
})
export class Favorites {
  wishlistProducts: WishlistProduct[] = [
    {
      id: 1,
      name: 'Field Roast Chao Cheese Creamy Original',
      image: 'https://via.placeholder.com/80x80/f0f0f0/666?text=Product',
      price: 2.51,
      stockStatus: 'in-stock',
      stockLabel: 'In Stock'
    },
    {
      id: 2,
      name: 'Blue Diamond Almonds Lightly Salted',
      image: 'https://via.placeholder.com/80x80/f0f0f0/666?text=Product',
      price: 3.2,
      stockStatus: 'in-stock',
      stockLabel: 'In Stock'
    },
    {
      id: 3,
      name: 'Fresh Organic Mustard Leaves Red Pepper',
      image: 'https://via.placeholder.com/80x80/f0f0f0/666?text=Product',
      price: 2.43,
      stockStatus: 'in-stock',
      stockLabel: 'In Stock'
    },
    {
      id: 4,
      name: "Angie's Boomchickapop Sweet & Salty",
      image: 'https://via.placeholder.com/80x80/f0f0f0/666?text=Product',
      price: 3.21,
      stockStatus: 'out-of-stock',
      stockLabel: 'Out Stock'
    },
    {
      id: 5,
      name: 'Foster Farms Takeout Crispy Classic',
      image: 'https://via.placeholder.com/80x80/f0f0f0/666?text=Product',
      price: 3.17,
      stockStatus: 'in-stock',
      stockLabel: 'In Stock'
    }
  ];

  get productCount(): number {
    return this.wishlistProducts.length;
  }

  addToCart(product: WishlistProduct): void {
    if (product.stockStatus === 'in-stock') {
      console.log('Adding to cart:', product.name);
      // Implementar lógica de adicionar ao carrinho
    }
  }

  removeFromWishlist(productId: number): void {
    this.wishlistProducts = this.wishlistProducts.filter(p => p.id !== productId);
    console.log('Removed product from wishlist:', productId);
  }
}
