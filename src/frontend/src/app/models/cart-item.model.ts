import { Product } from '../core/models/product.model';

export class CartItem {
    id: string;
    produto: Product;
    quantidade: number;
    
    constructor(produto: Product, quantidade: number = 1) {
        this.id = produto.id;
        this.produto = produto;
        this.quantidade = quantidade;
    }

    get precoTotal(): number {
        return (this.produto.precoComDesconto && this.produto.precoComDesconto > 0) 
            ? this.produto.precoComDesconto * this.quantidade 
            : this.produto.preco * this.quantidade;
    }
}