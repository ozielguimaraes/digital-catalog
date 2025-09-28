export interface Product {
  id: string;
  nome: string;
  descricao: string;
  preco: number;
  quantidade: number;
  categoriaId: string;
  categoria?: Category;
  imagemUrl?: string;
  ativo: boolean;
  dataCriacao: string;
  dataAtualizacao: string;
}

export interface Category {
  id: string;
  nome: string;
  descricao?: string;
  ativo: boolean;
  dataCriacao: string;
}

export interface ProductRequest {
  nome: string;
  descricao: string;
  preco: number;
  quantidade: number;
  categoriaId: string;
  imagemUrl?: string;
  ativo: boolean;
}

export interface ProductResponse {
  success: boolean;
  data: Product;
  message: string;
}

export interface ProductListResponse {
  success: boolean;
  data: Product[];
  pagination?: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
  message: string;
}

export interface CategoryResponse {
  success: boolean;
  data: Category[];
  message: string;
}
