using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace MeuCatalogo.Application.Services;

public sealed class ProdutoService : IProdutoService
{
    private readonly ILogger<ProdutoService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const string UploadRoot = "uploads";
    private readonly IStorageService _storage;

    public ProdutoService(ApplicationDbContext dbContext, ILogger<ProdutoService> logger, IStorageService storageService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _storage = storageService;
        _logger.LogInformation("Serviço de Produto inicializado");
    }

    #region Consultas

    public async Task<ApiResponse<IEnumerable<ProdutoDto>>> ObterPorCatalogoIdPublicoAsync(Guid catalogoId)
    {
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(catalogoId);
        if (catalogo == null)
            return ApiResponse<IEnumerable<ProdutoDto>>.Error(ResponseType.NotFound, "Catálogo não encontrado.");

        var produtos = await _dbContext.GetProdutosByCatalogoIdAsync(catalogoId);

        var resultado = produtos.Select(MapProdutoToDto);
        return ApiResponse<IEnumerable<ProdutoDto>>.Success(resultado);
    }

    public async Task<ApiResponse<IEnumerable<ProdutoDto>>> ObterPorCatalogoIdAsync(Guid catalogoId, string userId)
    {
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(catalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<IEnumerable<ProdutoDto>>.Error(ResponseType.Forbidden, "Acesso negado.");

        var produtos = await _dbContext.GetProdutosByCatalogoIdAsync(catalogoId);
        var resultado = produtos.Select(MapProdutoToDto);
        return ApiResponse<IEnumerable<ProdutoDto>>.Success(resultado);
    }

    public async Task<ApiResponse<ProdutoDto?>> ObterPorIdAsync(Guid id, string userId)
    {
        var produto = await _dbContext.ObterProdutoComDetalhesAsync(id);
        if (produto == null)
            return ApiResponse<ProdutoDto?>.Error(ResponseType.NotFound, "Produto não encontrado.");

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<ProdutoDto?>.Error(ResponseType.Forbidden, "Acesso negado.");

        return ApiResponse<ProdutoDto?>.Success(MapProdutoToDto(produto));
    }

    public async Task<ApiResponse<bool>> ExisteAsync(Guid id, string userId)
    {
        bool existe = await _dbContext.Produtos.AnyAsync(x => x.Id == id && x.Catalogo.UserId == userId);
        return existe
            ? ApiResponse<bool>.Success(true)
            : ApiResponse<bool>.Error(ResponseType.NotFound, "Produto não encontrado.");
    }

    #endregion

    #region CRUD

    public async Task<ApiResponse<ProdutoDto>> AdicionarAsync(ProdutoCreateDto dto, string userId)
    {
        _logger.LogInformation("Iniciando {Metodo} para catalogoId: {CatalogoId}, usuarioId: {UsuarioId}",
            nameof(AdicionarAsync), produtoDto.CatalogoId, userId);
        
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(dto.CatalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<ProdutoDto>.Error(ResponseType.Forbidden, "Acesso negado.");

        if (!await _dbContext.ExisteCategoriaAsync(dto.CategoriaId))
            return ApiResponse<ProdutoDto>.Error(ResponseType.Validation, "CategoriaId não pode ser vazio.");

        var produto = new Produto
        {
            Nome = dto.Nome,
            CategoriaId = dto.CategoriaId,
            CatalogoId = dto.CatalogoId,
            Preco = dto.Preco,
            PrecoComDesconto = dto.PrecoComDesconto,
            InformacoesAdicionais = dto.InformacoesAdicionais,
            Estoque = dto.Estoque != null ? new Estoque
            {
                Quantidade = dto.Estoque.Quantidade,
                QuantidadeMinima = dto.Estoque.QuantidadeMinima,
                QuantidadeMaxima = dto.Estoque.QuantidadeMaxima,
                Disponivel = dto.Estoque.Disponivel
            } : null
        };
         var user = await _dbContext.Users
            .Include(u => u.Assinaturas)
            .ThenInclude(a => a.PlanoAssinatura)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ApiResponse<ProdutoDto>.Error(ResponseType.NotFound, "Usuário não encontrado.");

        int quantidadeAtual = await _dbContext.Produtos.CountAsync(p => p.Catalogo.UserId == userId);

        if (!user.PodeAdicionarProduto(quantidadeAtual))
            return ApiResponse<ProdutoDto>.Error(ResponseType.Validation, "Limite de produtos do seu plano atingido. Faça um upgrade para continuar.");


        await _dbContext.AdicionarAsync(produto);
        _logger.LogInformation("Produto criado com sucesso. Id: {ProdutoId}", produto.Id);

        return ApiResponse<ProdutoDto>.Success(MapProdutoToDto(produto));
    }

    public async Task<ApiResponse<ProdutoDto>> AtualizarAsync(Guid id, ProdutoUpdateDto dto, string userId)
    {
        var produto = await _dbContext.ObterProdutoPorIdAsync(id)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo?.UserId != userId)
            throw new UnauthorizedAccessException("Acesso negado.");

        produto.Nome = dto.Nome;
        produto.CategoriaId = dto.CategoriaId;
        produto.Preco = dto.Preco;
        produto.PrecoComDesconto = dto.PrecoComDesconto;
        produto.InformacoesAdicionais = dto.InformacoesAdicionais;

        await _dbContext.AtualizarProdutoAsync(produto);
        return ApiResponse<ProdutoDto>.Success(MapProdutoToDto(produto));
    }

    public async Task<ApiResponse<bool>> RemoverAsync(Guid id, string userId)
    {
        var produto = await _dbContext.GetProdutoByIdAsync(id);
        if (produto == null)
            return ApiResponse<bool>.Error(ResponseType.NotFound, "Produto não encontrado.");

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<bool>.Error(ResponseType.Forbidden, "Acesso negado.");

        await _dbContext.RemoverProdutoAsync(id);
        await RemoverImagensDoProdutoAsync(produto.CatalogoId, id);
        return ApiResponse<bool>.Success(true);
    }

    #endregion

    #region Upload e Arquivos

    public async Task<ApiResponse<ImageDto>> UploadImagemAsync(Guid produtoId, IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<ImageDto>.Error(ResponseType.Validation, "Arquivo não pode ser vazio");

        if (!AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            return ApiResponse<ImageDto>.Error(ResponseType.Validation, "Tipo de arquivo não permitido");

        if (file.Length > MaxFileSize)
            return ApiResponse<ImageDto>.Error(ResponseType.Validation, "Tamanho máximo: 10MB");

        var produto = await _dbContext.ObterProdutoComDetalhesAsync(produtoId)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<ImageDto>.Error(ResponseType.Forbidden, "Acesso negado.");

        var isPrincipal = !produto.Imagens.Any();
        var shortName = catalogo.NomeCurto?.Trim() ?? catalogo.Id.ToString();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = isPrincipal ? "main" + extension : $"img-{produto.Imagens.Count + 1}-{Guid.NewGuid()}{extension}";
        var blobPath = $"catalog/{shortName}/products/{produtoId}/{fileName}";

        await using (var stream = file.OpenReadStream())
        {
            var _ = await _storage.UploadAsync(blobPath, stream, file.ContentType);
        }

        var imageUrl = _storage.GetBlobUrl(blobPath);

        var produtoImagem = new ProdutoImagem
        {
            ProdutoId = produtoId,
            FileName = fileName,
            Url = imageUrl,
            ContentType = file.ContentType,
            Size = file.Length,
            IsPrincipal = isPrincipal,
            Ordem = produto.Imagens.Count + 1
        };

        _dbContext.ProdutoImagens.Add(produtoImagem);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<ImageDto>.Success(new ImageDto
        {
            Url = imageUrl,
            FileName = fileName,
            Size = file.Length,
            ContentType = file.ContentType,
            UploadDate = DateTime.UtcNow,
            IsPrincipal = isPrincipal,
            Ordem = produtoImagem.Ordem
        });
    }

    private async Task RemoverImagensDoProdutoAsync(Guid catalogoId, Guid produtoId)
    {
        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(catalogoId);
        if (catalogo == null)
            return;

        var shortName = catalogo.NomeCurto?.Trim() ?? catalogoId.ToString();
        var prefix = $"catalog/{shortName}/products/{produtoId}/";
        await _storage.DeletePrefixAsync(prefix);
        _logger.LogInformation("Imagens do produto {ProdutoId} removidas do storage.", produtoId);
    }

    #endregion

    #region Estoque

    public async Task<ApiResponse<EstoqueDto>> AtualizarEstoqueAsync(Guid produtoId, EstoqueUpdateDto dto, string userId)
    {
        var produto = await _dbContext.GetProdutoWithDetailsAsync(produtoId);
        if (produto == null)
            return ApiResponse<EstoqueDto>.Error(ResponseType.NotFound, "Produto não encontrado.");

        var catalogo = await _dbContext.ObterCatalogoPorIdAsync(produto.CatalogoId);
        if (catalogo?.UserId != userId)
            return ApiResponse<EstoqueDto>.Error(ResponseType.Forbidden, "Acesso negado.");

        if (produto.Estoque == null)
        {
            produto.Estoque = new Estoque
            {
                ProdutoId = produtoId,
                Quantidade = dto.Quantidade,
                QuantidadeMinima = dto.QuantidadeMinima,
                QuantidadeMaxima = dto.QuantidadeMaxima
            };
        }
        else
        {
            produto.Estoque.Quantidade = dto.Quantidade;
            produto.Estoque.QuantidadeMinima = dto.QuantidadeMinima;
            produto.Estoque.QuantidadeMaxima = dto.QuantidadeMaxima;
        }

        await _dbContext.AtualizarProdutoAsync(produto);

        return ApiResponse<EstoqueDto>.Success(new EstoqueDto
        {
            Id = produto.Estoque.Id,
            ProdutoId = produto.Estoque.ProdutoId,
            Quantidade = produto.Estoque.Quantidade,
            QuantidadeMinima = produto.Estoque.QuantidadeMinima,
            QuantidadeMaxima = produto.Estoque.QuantidadeMaxima,
            DataCriacao = produto.Estoque.DataCriacao,
            DataAtualizacao = produto.Estoque.DataAtualizacao
        });
    }

    #endregion


    private static ProdutoDto MapProdutoToDto(Produto p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        CategoriaId = p.CategoriaId,
        CategoriaNome = p.Categoria?.Nome ?? "Categoria não encontrada",
        CatalogoId = p.CatalogoId,
        Preco = p.Preco,
        PrecoComDesconto = p.PrecoComDesconto,
        InformacoesAdicionais = p.InformacoesAdicionais,
        DataCriacao = p.DataCriacao,
        DataAtualizacao = p.DataAtualizacao,
        Estoque = p.Estoque != null ? new EstoqueDto
        {
            Id = p.Estoque.Id,
            ProdutoId = p.Estoque.ProdutoId,
            Quantidade = p.Estoque.Quantidade,
            QuantidadeMinima = p.Estoque.QuantidadeMinima,
            QuantidadeMaxima = p.Estoque.QuantidadeMaxima,
            Disponivel = p.Estoque.Disponivel,
            EhIlimitado = p.Estoque.EhIlimitado(),
            DataCriacao = p.Estoque.DataCriacao,
            DataAtualizacao = p.Estoque.DataAtualizacao
        } : null,
        Imagens = p.Imagens?.Select(i => i.Url).ToList() ?? new()
    };

}
