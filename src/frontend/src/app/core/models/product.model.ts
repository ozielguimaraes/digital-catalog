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
  variacoes: ProdutoVariacaoDto[];
  imagens?: ProdutoImagem[];
  dataCriacao: string;
  dataAtualizacao?: string;
}

export interface ProdutoImagem {
  id: string;
  url: string;
  isPrincipal: boolean;
  ordem: number;
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
  produtoId: string;
  tipoVariacaoId: string;
  tipoNome: string;
  opcaoVariacaoId: string;
  valor: string;
  dataCriacao: string;
}

export interface OpcaoVariacao {
  id: string;
  valor: string;
}

// Modelo SKU atual: combinação cor × tamanho com quantidade e preço opcional.
export interface ProdutoVariacaoDto {
  id: string;
  cor?: string;
  tamanho?: string;
  preco?: number;
  quantidade: number;
}

export interface ProdutoVariacaoCreateDto {
  cor?: string;
  tamanho?: string;
  preco?: number;
  quantidade: number;
}

export interface VariacaoSugestoes {
  cores: string[];
  tamanhos: string[];
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
  variacoes?: ProdutoVariacaoCreateDto[];
}

export interface ProductUpdateRequest {
  nome: string;
  categoriaId: string;
  preco: number;
  precoComDesconto?: number;
  informacoesAdicionais?: string;
  estoque?: EstoqueUpdateRequest;
  variacoes?: ProdutoVariacaoCreateDto[];
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
