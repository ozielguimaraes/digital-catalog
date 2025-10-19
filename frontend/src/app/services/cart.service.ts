import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CartItem } from '../models/cart-item.model';
import { Product } from '../core/models/product.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems: CartItem[] = [];
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  private cartTotalSubject = new BehaviorSubject<number>(0);
  private cartCountSubject = new BehaviorSubject<number>(0);

  constructor() {
    this.loadCartFromStorage();
  }

  getCartItems(): Observable<CartItem[]> {
    return this.cartItemsSubject.asObservable();
  }

  getCartTotal(): Observable<number> {
    return this.cartTotalSubject.asObservable();
  }

  getCartCount(): Observable<number> {
    return this.cartCountSubject.asObservable();
  }

  addToCart(produto: Product, quantidade: number = 1): void {
    const existingItem = this.cartItems.find(item => item.id === produto.id);

    if (existingItem) {
      existingItem.quantidade += quantidade;
    } else {
      const newItem = new CartItem(produto, quantidade);
      this.cartItems.push(newItem);
    }

    this.updateCart();
  }

  removeFromCart(id: string): void {
    this.cartItems = this.cartItems.filter(item => item.id !== id);
    this.updateCart();
  }

  updateQuantity(id: string, quantidade: number): void {
    const item = this.cartItems.find(item => item.id === id);
    if (item) {
      item.quantidade = quantidade;
      if (item.quantidade <= 0) {
        this.removeFromCart(id);
      } else {
        this.updateCart();
      }
    }
  }

  clearCart(): void {
    this.cartItems = [];
    this.updateCart();
  }

  private updateCart(): void {
    this.cartItemsSubject.next([...this.cartItems]);
    
    const total = this.cartItems.reduce((sum, item) => sum + item.precoTotal, 0);
    this.cartTotalSubject.next(total);
    
    const count = this.cartItems.reduce((sum, item) => sum + item.quantidade, 0);
    this.cartCountSubject.next(count);
    
    this.saveCartToStorage();
  }

  private saveCartToStorage(): void {
    localStorage.setItem('cart', JSON.stringify(this.cartItems));
  }

  private loadCartFromStorage(): void {
    const storedCart = localStorage.getItem('cart');
    if (storedCart) {
      try {
        const parsedCart = JSON.parse(storedCart);
        this.cartItems = parsedCart.map((item: any) => {
          return new CartItem(item.produto, item.quantidade);
        });
        this.updateCart();
      } catch (error) {
        console.error('Erro ao carregar carrinho do localStorage:', error);
        this.clearCart();
      }
    }
  }
}