import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CartService } from '../../../services/cart.service';
import { CartItem } from '../../../models/cart-item.model';
import { ImageUrlService } from '../../../core/services/image-url.service';

@Component({
  selector: 'app-cart-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cart-modal.component.html',
  styleUrls: ['./cart-modal.component.scss']
})
export class CartModalComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  cartTotal = 0;
  cartCount = 0;
  isOpen = false;
  private destroy$ = new Subject<void>();

  constructor(
    private cartService: CartService,
    private imageUrlService: ImageUrlService,
    private router: Router
  ) {}

  ngOnInit() {
    this.cartService.getCartItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe(items => this.cartItems = items);

    this.cartService.getCartTotal()
      .pipe(takeUntil(this.destroy$))
      .subscribe(total => this.cartTotal = total);

    this.cartService.getCartCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => this.cartCount = count);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  open() {
    this.isOpen = true;
  }

  close() {
    this.isOpen = false;
  }

  updateQuantity(item: CartItem, quantity: number) {
    if (quantity < 1) {
      this.cartService.removeFromCart(item.id);
    } else {
      this.cartService.updateQuantity(item.id, quantity);
    }
  }

  removeItem(item: CartItem) {
    this.cartService.removeFromCart(item.id);
  }

  clearCart() {
    this.cartService.clearCart();
  }

  goToCheckout() {
    this.close();
    this.router.navigate(['/checkout']);
  }

  getProductImage(item: CartItem): string {
    if (item.produto.imagens && item.produto.imagens.length > 0) {
      const firstImage: any = item.produto.imagens[0];
      let imageUrl = '';

      if (typeof firstImage === 'string') {
        imageUrl = firstImage;
      } else if (firstImage && typeof firstImage === 'object') {
        imageUrl = (firstImage as any).url || '';
      }

      return this.imageUrlService.getImageUrl(imageUrl);
    }
    return 'assets/images/placeholder-product.jpg';
  }

  getProductPrice(item: CartItem): number {
    return item.produto.precoComDesconto && item.produto.precoComDesconto > 0 
      ? item.produto.precoComDesconto 
      : item.produto.preco;
  }

  hasDiscount(item: CartItem): boolean {
    return !!(item.produto.precoComDesconto && item.produto.precoComDesconto > 0 && item.produto.precoComDesconto < item.produto.preco);
  }

  getDiscountPercentage(item: CartItem): number {
    if (!this.hasDiscount(item)) return 0;
    return Math.round(((item.produto.preco - item.produto.precoComDesconto!) / item.produto.preco) * 100);
  }
}
