export interface Product {
  id: string;
  nome: string;
  categoriaId: string;
  categoriaNome: string;
  catalogoId: string;
  preco: number;
  precoComDesconto?: number;
  informacoesAdicionais?: string;
  estoque: Estoque;
  variacoes: Variacao[];
  imagens?: string[];
  dataCriacao: string;
  dataAtualizacao?: string;
}

export interface Estoque {
  id: string;
  produtoId: string;
  quantidade?: number;
  quantidadeMinima?: number;
  quantidadeMaxima?: number;
  disponivel: boolean;
  ehIlimitado: boolean;
  dataCriacao: string;
  dataAtualizacao?: string;
}

export interface Variacao {
  id: string;
  nome: string;
  opcaoVariacao: OpcaoVariacao;
  produtoId: string;
  dataCriacao: string;
  dataAtualizacao?: string;
}

export interface OpcaoVariacao {
  id: string;
  nome: string;
  valor: string;
  precoAdicional?: number;
}

export interface Category {
  id: string;
  nome: string;
  descricao: string;
  catalogoId: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductCreateRequest {
  nome: string;
  categoriaId: string;
  catalogoId: string;
  preco: number;
  precoComDesconto?: number;
  informacoesAdicionais?: string;
  estoque?: EstoqueCreateRequest;
}

export interface ProductUpdateRequest {
  nome: string;
  categoriaId: string;
  preco: number;
  precoComDesconto?: number;
  informacoesAdicionais?: string;
  estoque?: EstoqueUpdateRequest;
}

export interface EstoqueCreateRequest {
  quantidade?: number;
  quantidadeMinima?: number;
  quantidadeMaxima?: number;
  disponivel: boolean;
  ehIlimitado: boolean;
}

export interface EstoqueUpdateRequest {
  quantidade?: number;
  quantidadeMinima?: number;
  quantidadeMaxima?: number;
  disponivel: boolean;
  ehIlimitado: boolean;
}

export interface ProductResponse {
  isSuccess: boolean;
  data: Product;
  message: string;
  type: string;
  errors?: string[];
}

export interface ProductListResponse {
  isSuccess: boolean;
  data: Product[];
  message: string;
  type: string;
  errors?: string[];
}

export interface CategoryResponse {
  isSuccess: boolean;
  data: Category[];
  message: string;
  type: string;
  errors?: string[];
}
