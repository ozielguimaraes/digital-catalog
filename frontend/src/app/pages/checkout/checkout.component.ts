import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CartService } from '../../services/cart.service';
import { CartItem } from '../../models/cart-item.model';
import { ImageUrlService } from '../../core/services/image-url.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit, OnDestroy {
  cartItems: CartItem[] = [];
  cartTotal = 0;
  cartCount = 0;
  private destroy$ = new Subject<void>();

  // Customer information
  customerInfo = {
    nome: '',
    telefone: '',
    email: '',
    endereco: '',
    cidade: '',
    cep: '',
    observacoes: ''
  };

  // WhatsApp configuration
  private readonly WHATSAPP_NUMBER = '5534984133739';

  constructor(
    private cartService: CartService,
    private imageUrlService: ImageUrlService,
    private router: Router
  ) {}

  ngOnInit() {
    this.cartService.getCartItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe(items => {
        this.cartItems = items;
        if (items.length === 0) {
          this.router.navigate(['/']);
        }
      });

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

  getProductImage(item: CartItem): string {
    if (item.produto.imagens && item.produto.imagens.length > 0) {
      return this.imageUrlService.getImageUrl(item.produto.imagens[0]);
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

  goBack() {
    this.router.navigate(['/']);
  }

  sendToWhatsApp() {
    if (this.cartItems.length === 0) {
      alert('Seu carrinho está vazio!');
      return;
    }

    // Validate required fields
    if (!this.customerInfo.nome.trim()) {
      alert('Por favor, informe seu nome.');
      return;
    }

    if (!this.customerInfo.telefone.trim()) {
      alert('Por favor, informe seu telefone.');
      return;
    }

    // Format phone number (remove non-numeric characters)
    const phoneNumber = this.customerInfo.telefone.replace(/\D/g, '');
    if (phoneNumber.length < 10) {
      alert('Por favor, informe um telefone válido.');
      return;
    }

    // Create order message
    const orderMessage = this.createOrderMessage();
    
    // Create WhatsApp URL
    const whatsappUrl = `https://wa.me/${this.WHATSAPP_NUMBER}?text=${encodeURIComponent(orderMessage)}`;
    
    // Open WhatsApp
    window.open(whatsappUrl, '_blank');
  }

  private createOrderMessage(): string {
    let message = `🛍️ *NOVO PEDIDO - Sany & Z*\n\n`;
    
    // Customer information
    message += `👤 *DADOS DO CLIENTE:*\n`;
    message += `Nome: ${this.customerInfo.nome}\n`;
    message += `Telefone: ${this.customerInfo.telefone}\n`;
    
    if (this.customerInfo.email.trim()) {
      message += `Email: ${this.customerInfo.email}\n`;
    }
    
    if (this.customerInfo.endereco.trim()) {
      message += `Endereço: ${this.customerInfo.endereco}\n`;
    }
    
    if (this.customerInfo.cidade.trim()) {
      message += `Cidade: ${this.customerInfo.cidade}\n`;
    }
    
    if (this.customerInfo.cep.trim()) {
      message += `CEP: ${this.customerInfo.cep}\n`;
    }
    
    if (this.customerInfo.observacoes.trim()) {
      message += `Observações: ${this.customerInfo.observacoes}\n`;
    }
    
    message += `\n🛒 *ITENS DO PEDIDO:*\n`;
    
    // Order items
    this.cartItems.forEach((item, index) => {
      const price = this.getProductPrice(item);
      const totalPrice = item.precoTotal;
      
      message += `${index + 1}. *${item.produto.nome}*\n`;
      message += `   Quantidade: ${item.quantidade}\n`;
      message += `   Preço unitário: R$ ${price.toFixed(2).replace('.', ',')}\n`;
      
      if (this.hasDiscount(item)) {
        const discount = this.getDiscountPercentage(item);
        message += `   Desconto: ${discount}%\n`;
      }
      
      message += `   Subtotal: R$ ${totalPrice.toFixed(2).replace('.', ',')}\n\n`;
    });
    
    // Order total
    message += `💰 *TOTAL DO PEDIDO: R$ ${this.cartTotal.toFixed(2).replace('.', ',')}*\n\n`;
    
    message += `📱 *Este pedido foi feito através do catálogo digital.*\n`;
    message += `Por favor, confirme os dados e informe a forma de pagamento e entrega.`;
    
    return message;
  }
}
