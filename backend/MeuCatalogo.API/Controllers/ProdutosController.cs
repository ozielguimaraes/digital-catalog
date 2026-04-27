using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[SwaggerTag("Gerenciamento de produtos")]
public class ProdutosController : BaseApiController
{
    private readonly IProdutoService _produtoService;
    private readonly ICatalogoService _catalogoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger, ICatalogoService catalogoService)
    {
        _produtoService = produtoService;
        _logger = logger;
        _catalogoService = catalogoService;
    }

    /// <summary>
    /// Obtém todos os produtos de um catálogo específico
    /// </summary>
    /// <param name="catalogoId">ID do catálogo</param>
    /// <returns>Lista de produtos do catálogo</returns>
    /// <response code="200">Lista de produtos retornada com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("catalogo/{catalogoId}")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Listar produtos por catálogo (público)",
        Description = "Retorna todos os produtos de um catálogo específico (acesso público)."
    )]
    [SwaggerResponse(200, "Lista de produtos retornada com sucesso", typeof(IEnumerable<ProdutoDto>))]
    [SwaggerResponse(404, "Catálogo não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> ObterPorCatalogo(Guid catalogoId)
    {
        _logger.LogInformation("Obtendo produtos do catálogo {CatalogoId} (acesso público)", catalogoId);
        var response = await _produtoService.ObterPorCatalogoIdPublicoAsync(catalogoId);
        EnriquecerImagens(response);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Obtém um produto específico por ID
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Dados do produto</returns>
    /// <response code="200">Produto encontrado e retornado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao produto</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obter produto por ID",
        Description = "Retorna os dados de um produto específico do usuário autenticado."
    )]
    [SwaggerResponse(200, "Produto encontrado e retornado com sucesso", typeof(ProdutoDto))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao produto", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Produto não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Obter(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.ObterPorIdAsync(id, userId);
        EnriquecerImagemNullable(response);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    /// <param name="produtoDto">Dados do produto a ser criado</param>
    /// <returns>Dados do produto criado</returns>
    /// <response code="201">Produto criado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao catálogo</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Criar novo produto",
        Description = "Cria um novo produto no catálogo do usuário autenticado."
    )]
    [SwaggerResponse(201, "Produto criado com sucesso", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao catálogo", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Adicionar([FromBody] ProdutoCreateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao adicionar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Adicionando novo produto para o usuário {UserId}", userId);
        var response = await _produtoService.AdicionarAsync(produtoDto, userId);
        EnriquecerImagem(response);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <param name="produtoDto">Dados atualizados do produto</param>
    /// <returns>Dados do produto atualizado</returns>
    /// <response code="200">Produto atualizado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao produto</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualizar produto",
        Description = "Atualiza os dados de um produto existente do usuário autenticado."
    )]
    [SwaggerResponse(200, "Produto atualizado com sucesso", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao produto", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Produto não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProdutoUpdateDto produtoDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar produto: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando produto {ProdutoId} para o usuário {UserId}", id, userId);
        var produto = await _produtoService.AtualizarAsync(id, produtoDto, userId);
        EnriquecerImagem(produto);

        return HandleApiResponse(produto);
    }

    /// <summary>
    /// Remove um produto
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Confirmação da remoção</returns>
    /// <response code="204">Produto removido com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao produto</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remover produto",
        Description = "Remove um produto do catálogo do usuário autenticado."
    )]
    [SwaggerResponse(204, "Produto removido com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao produto", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Produto não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Remover(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Removendo produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.RemoverAsync(id, userId);
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Atualiza o estoque de um produto
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <param name="estoqueDto">Dados do estoque</param>
    /// <returns>Dados do estoque atualizado</returns>
    /// <response code="200">Estoque atualizado com sucesso</response>
    /// <response code="400">Dados inválidos fornecidos</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao produto</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}/estoque")]
    [SwaggerOperation(
        Summary = "Atualizar estoque do produto",
        Description = "Atualiza as informações de estoque de um produto específico."
    )]
    [SwaggerResponse(200, "Estoque atualizado com sucesso", typeof(EstoqueDto))]
    [SwaggerResponse(400, "Dados inválidos fornecidos", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao produto", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Produto não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> AtualizarEstoque(Guid id, [FromBody] EstoqueUpdateDto estoqueDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido ao atualizar estoque: {@ModelState}", ModelState);
            return ValidationProblemResponse(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Atualizando estoque do produto {ProdutoId} para o usuário {UserId}", id, userId);
        var response = await _produtoService.AtualizarEstoqueAsync(id, estoqueDto, userId);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Faz upload de uma imagem para um produto
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <param name="file">Arquivo de imagem</param>
    /// <returns>Dados da imagem enviada</returns>
    /// <response code="200">Imagem enviada com sucesso</response>
    /// <response code="400">Arquivo inválido</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="403">Usuário não tem acesso ao produto</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("{produtoId}/upload-image")]
    [SwaggerOperation(
        Summary = "Upload de imagem para produto",
        Description = "Faz upload de uma imagem para um produto específico."
    )]
    [SwaggerResponse(200, "Imagem enviada com sucesso", typeof(ImageDto))]
    [SwaggerResponse(400, "Arquivo inválido", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(403, "Usuário não tem acesso ao produto", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Produto não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> UploadImage(Guid produtoId, IFormFile? file, [FromQuery] bool isPrincipal = false, [FromQuery] int ordem = 0)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Upload de imagem para produto {ProdutoId} pelo usuário {UserId}", produtoId, userId);

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Arquivo não pode ser vazio" });
        }

        var response = await _produtoService.UploadImagemAsync(produtoId, file, userId, isPrincipal, ordem);
        EnriquecerImagemUpload(response);
        return HandleApiResponse(response);
    }

    private void EnriquecerImagens(ApiResponse<IEnumerable<ProdutoDto>> response)
    {
        if (!response.IsSuccess || response.Data == null)
            return;

        foreach (var produto in response.Data)
        {
            EnriquecerProduto(produto);
        }
    }

    private void EnriquecerImagemNullable(ApiResponse<ProdutoDto?> response)
    {
        if (!response.IsSuccess || response.Data == null)
            return;

        EnriquecerProduto(response.Data);
    }

    private void EnriquecerImagem(ApiResponse<ProdutoDto> response)
    {
        if (!response.IsSuccess || response.Data == null)
            return;

        EnriquecerProduto(response.Data);
    }

    private void EnriquecerImagemUpload(ApiResponse<ImageDto> response)
    {
        if (!response.IsSuccess || response.Data == null)
            return;

        var absoluteUrl = ObterUrlAbsoluta(response.Data.Url);
        response.Data.Url = absoluteUrl;
        response.Data.Images = CriarLinksImagem(absoluteUrl);
    }

    private void EnriquecerProduto(ProdutoDto produto)
    {
        foreach (var imagem in produto.Imagens)
        {
            var absoluteUrl = ObterUrlAbsoluta(imagem.Url);
            imagem.Url = absoluteUrl;
            imagem.Images = CriarLinksImagem(absoluteUrl);
        }
    }

    private string ObterUrlAbsoluta(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.ToString();

        var host = $"{Request.Scheme}://{Request.Host}";
        if (!string.IsNullOrWhiteSpace(Request.PathBase))
        {
            host += Request.PathBase.ToString();
        }

        var path = url.StartsWith('/') ? url : $"/{url}";
        return $"{host}{path}";
    }

    private static ImageLinksDto CriarLinksImagem(string absoluteUrl)
    {
        return new ImageLinksDto
        {
            Thumbnail = absoluteUrl,
            Medium = absoluteUrl,
            Full = absoluteUrl
        };
    }
}
