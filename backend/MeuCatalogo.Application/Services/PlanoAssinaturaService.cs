using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Application.Services;

public sealed class PlanoAssinaturaService : IPlanoAssinaturaService
{
    private readonly ILogger<PlanoAssinaturaService> _logger;
    private readonly ApplicationDbContext _context;

    public PlanoAssinaturaService(ApplicationDbContext context, ILogger<PlanoAssinaturaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<PlanoAssinaturaDto>>> ObterTodosAsync()
    {
        var planos = await _context.PlanosAssinatura
            .OrderBy(p => p.Preco)
            .ToListAsync();

        return ApiResponse<IEnumerable<PlanoAssinaturaDto>>.Success(planos.Select(MapToDto));
    }

    public async Task<ApiResponse<PlanoAssinaturaDto>> ObterPorIdAsync(Guid id)
    {
        var plano = await _context.PlanosAssinatura.FindAsync(id);
        if (plano == null)
        {
            _logger.LogInformation("Plano de assinatura com ID {Id} não encontrado.", id);
            return ApiResponse<PlanoAssinaturaDto>.Error(ResponseType.NotFound, $"Plano de assinatura com ID {id} não encontrado.");
        }

        return ApiResponse<PlanoAssinaturaDto>.Success(MapToDto(plano));
    }

    public async Task<ApiResponse<PlanoAssinaturaDto>> CriarAsync(PlanoAssinaturaCreateDto planoDto)
    {
        var plano = new PlanoAssinatura
        {
            Nome = planoDto.Nome,
            Descricao = planoDto.Descricao,
            Preco = planoDto.Preco,
            DuracaoEmMeses = planoDto.DuracaoEmMeses,
            LimiteProdutos = planoDto.LimiteProdutos,
            LimiteCatalogos = planoDto.LimiteCatalogos,
            PermiteVariacoes = planoDto.PermiteVariacoes,
            PermiteEstoque = planoDto.PermiteEstoque,
            PermiteRelatorios = planoDto.PermiteRelatorios,
            PermiteExportacao = planoDto.PermiteExportacao,
            PermiteImportacao = planoDto.PermiteImportacao,
            PermitePersonalizacao = planoDto.PermitePersonalizacao,
            EhGratuito = planoDto.EhGratuito
        };

        _context.PlanosAssinatura.Add(plano);
        await _context.SaveChangesAsync();

        return ApiResponse<PlanoAssinaturaDto>.Success(MapToDto(plano));
    }

    public async Task<ApiResponse<PlanoAssinaturaDto>> AtualizarAsync(Guid id, PlanoAssinaturaUpdateDto planoDto)
    {
        var plano = await _context.PlanosAssinatura.FindAsync(id);
        if (plano == null)
        {
            _logger.LogWarning($"Plano de assinatura com ID {id} não encontrado.");
            return ApiResponse<PlanoAssinaturaDto>.Error(ResponseType.NotFound, $"Plano de assinatura '{id}' não encontrado.");
        }

        plano.Nome = planoDto.Nome;
        plano.Descricao = planoDto.Descricao;
        plano.Preco = planoDto.Preco;
        plano.DuracaoEmMeses = planoDto.DuracaoEmMeses;
        plano.LimiteProdutos = planoDto.LimiteProdutos;
        plano.LimiteCatalogos = planoDto.LimiteCatalogos;
        plano.PermiteVariacoes = planoDto.PermiteVariacoes;
        plano.PermiteEstoque = planoDto.PermiteEstoque;
        plano.PermiteRelatorios = planoDto.PermiteRelatorios;
        plano.PermiteExportacao = planoDto.PermiteExportacao;
        plano.PermiteImportacao = planoDto.PermiteImportacao;
        plano.PermitePersonalizacao = planoDto.PermitePersonalizacao;
        plano.EhGratuito = planoDto.EhGratuito;
        plano.DataAtualizacao = DateTime.UtcNow;

        _context.PlanosAssinatura.Update(plano);
        await _context.SaveChangesAsync();

        return ApiResponse<PlanoAssinaturaDto>.Success(MapToDto(plano));
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(Guid id)
    {
        var plano = await _context.PlanosAssinatura.FindAsync(id);
        if (plano == null)
        {
            _logger.LogWarning("Plano de assinatura com ID {Id} não encontrado para exclusão.", id);;
            return ApiResponse<bool>.Error(ResponseType.NotFound, $"Plano de assinatura com ID {id} não encontrado.");
        }

        var temAssinaturasAtivas = await _context.AssinaturasUsuarios
            .AnyAsync(a => a.PlanoAssinaturaId == id && a.Ativa && a.DataFim > DateTime.UtcNow);

        if (temAssinaturasAtivas)
        {
            _logger.LogError("Não é possível excluir o plano de assinatura com ID {Id} pois existem assinaturas ativas associadas a ele.", id);
            return ApiResponse<bool>.Error(ResponseType.Validation, "Não é possível excluir um plano com assinaturas ativas.");
        }

        _context.PlanosAssinatura.Remove(plano);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Success(ResponseType.Deleted, true);
    }

    public async Task<ApiResponse<AssinaturaUsuarioDto>> AtribuirPlanoGratuitoAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário com ID {UserId} não encontrado ao tentar atribuir plano gratuito.", userId);
            return ApiResponse<AssinaturaUsuarioDto>.Error(ResponseType.NotFound, $"Usuário com ID {userId} não encontrado.");
        }

        var assinaturaAtiva = await _context.AssinaturasUsuarios
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Ativa && a.DataFim > DateTime.UtcNow);

        if (assinaturaAtiva != null)
        {
            _logger.LogInformation("Usuário {UserId} já possui uma assinatura ativa.", userId);
            return ApiResponse<AssinaturaUsuarioDto>.Success(await MapToAssinaturaDto(assinaturaAtiva));
        }

        var planoGratuito = await _context.PlanosAssinatura
            .FirstOrDefaultAsync(p => p.EhGratuito);

        if (planoGratuito == null)
        {
            _logger.LogError("Nenhum plano gratuito encontrado ao tentar atribuir plano gratuito para o usuário {UserId}.", userId);
            return ApiResponse<AssinaturaUsuarioDto>.Error(ResponseType.Validation, "Nenhum plano gratuito encontrado.");
        }

        var assinatura = new AssinaturaUsuario
        {
            UserId = userId,
            PlanoAssinaturaId = planoGratuito.Id,
            DataInicio = DateTime.UtcNow,
            DataFim = DateTime.UtcNow.AddYears(100),
            Ativa = true,
            RenovacaoAutomatica = true,
            ValorPago = 0
        };

        _context.AssinaturasUsuarios.Add(assinatura);
        await _context.SaveChangesAsync();

        return ApiResponse<AssinaturaUsuarioDto>.Success(await MapToAssinaturaDto(assinatura));
    }

    public async Task<ApiResponse<AssinaturaUsuarioDto>> AssinarPlanoAsync(string userId, Guid planoId, bool renovacaoAutomatica, string? transacaoId = null, string? metodoPagamento = null, decimal valorPago = 0)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogError("Usuário com ID {UserId} não encontrado ao tentar assinar plano.", userId);
            return ApiResponse<AssinaturaUsuarioDto>.Error(ResponseType.NotFound, $"Usuário com ID {userId} não encontrado.");
        }

        var plano = await _context.PlanosAssinatura.FindAsync(planoId);
        if (plano == null)
        {
            _logger.LogError("Plano de assinatura com ID {PlanoId} não encontrado ao tentar assinar plano.", planoId);
            return ApiResponse<AssinaturaUsuarioDto>.Error(ResponseType.NotFound, $"Plano de assinatura com ID {planoId} não encontrado.");
        }

        // Cancelar assinaturas ativas existentes
        var assinaturasAtivas = await _context.AssinaturasUsuarios
            .Where(a => a.UserId == userId && a.Ativa && a.DataFim > DateTime.UtcNow)
            .ToListAsync();

        foreach (var assinaturaAtiva in assinaturasAtivas)
        {
            assinaturaAtiva.Cancelar("Upgrade para novo plano");
            _context.AssinaturasUsuarios.Update(assinaturaAtiva);
        }

        var dataFim = DateTime.UtcNow.AddMonths(plano.DuracaoEmMeses);
        var assinatura = new AssinaturaUsuario
        {
            UserId = userId,
            PlanoAssinaturaId = planoId,
            DataInicio = DateTime.UtcNow,
            DataFim = dataFim,
            Ativa = true,
            TransacaoId = transacaoId,
            MetodoPagamento = metodoPagamento,
            ValorPago = valorPago,
            RenovacaoAutomatica = renovacaoAutomatica
        };

        _context.AssinaturasUsuarios.Add(assinatura);
        await _context.SaveChangesAsync();

        return ApiResponse<AssinaturaUsuarioDto>.Success(await MapToAssinaturaDto(assinatura));
    }

    public async Task<ApiResponse<bool>> CancelarAssinaturaAsync(string userId, string motivo)
    {
        var assinaturaAtiva = await _context.AssinaturasUsuarios
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Ativa && a.DataFim > DateTime.UtcNow);

        if (assinaturaAtiva == null)
        {
            _logger.LogError("Usuário {UserId} não possui uma assinatura ativa para cancelar.", userId);
            return ApiResponse<bool>.Error(ResponseType.Validation, "Você não possui uma assinatura ativa para cancelar.");
        }

        assinaturaAtiva.Cancelar(motivo);
        _context.AssinaturasUsuarios.Update(assinaturaAtiva);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<AssinaturaUsuarioDto?>> ObterAssinaturaAtivaAsync(string userId)
    {
        var assinaturaAtiva = await _context.AssinaturasUsuarios
            .Include(a => a.PlanoAssinatura)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Ativa && a.DataFim > DateTime.UtcNow);

        if (assinaturaAtiva != null)
        {
            return ApiResponse<AssinaturaUsuarioDto?>.Success(await MapToAssinaturaDto(assinaturaAtiva));
        }

        _logger.LogInformation("Usuário {UserId} não possui uma assinatura ativa.", userId);
        return ApiResponse<AssinaturaUsuarioDto?>.Error(ResponseType.NotFound, "Você não possui uma assinatura ativa.");

    }

    public async Task<ApiResponse<PlanoAssinaturaDto?>> ObterPlanoAtivoAsync(string userId)
    {
        var assinaturaAtiva = await _context.AssinaturasUsuarios
            .Include(a => a.PlanoAssinatura)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Ativa && a.DataFim > DateTime.UtcNow);

        if (assinaturaAtiva != null)
        {
            return ApiResponse<PlanoAssinaturaDto?>.Success(MapToDto(assinaturaAtiva.PlanoAssinatura));
        }

        _logger.LogInformation("Usuário {UserId} não possui um plano ativo.", userId);
        return ApiResponse<PlanoAssinaturaDto?>.Error(ResponseType.NotFound, "Você não possui um plano ativo.");
    }

    private PlanoAssinaturaDto MapToDto(PlanoAssinatura plano)
    {
        return new PlanoAssinaturaDto
        {
            Id = plano.Id,
            Nome = plano.Nome,
            Descricao = plano.Descricao,
            Preco = plano.Preco,
            DuracaoEmMeses = plano.DuracaoEmMeses,
            LimiteProdutos = plano.LimiteProdutos,
            LimiteCatalogos = plano.LimiteCatalogos,
            PermiteVariacoes = plano.PermiteVariacoes,
            PermiteEstoque = plano.PermiteEstoque,
            PermiteRelatorios = plano.PermiteRelatorios,
            PermiteExportacao = plano.PermiteExportacao,
            PermiteImportacao = plano.PermiteImportacao,
            PermitePersonalizacao = plano.PermitePersonalizacao,
            EhGratuito = plano.EhGratuito,
            DataCriacao = plano.DataCriacao,
            DataAtualizacao = plano.DataAtualizacao
        };
    }

    private async Task<AssinaturaUsuarioDto> MapToAssinaturaDto(AssinaturaUsuario assinatura)
    {
        var plano = await _context.PlanosAssinatura.FindAsync(assinatura.PlanoAssinaturaId);
        var user = await _context.Users.FindAsync(assinatura.UserId);

        return new AssinaturaUsuarioDto
        {
            Id = assinatura.Id,
            UserId = assinatura.UserId,
            UserName = user?.UserName,
            PlanoAssinaturaId = assinatura.PlanoAssinaturaId,
            PlanoAssinaturaNome = plano?.Nome,
            DataInicio = assinatura.DataInicio,
            DataFim = assinatura.DataFim,
            Ativa = assinatura.Ativa,
            TransacaoId = assinatura.TransacaoId,
            MetodoPagamento = assinatura.MetodoPagamento,
            ValorPago = assinatura.ValorPago,
            RenovacaoAutomatica = assinatura.RenovacaoAutomatica,
            DataCancelamento = assinatura.DataCancelamento,
            MotivoCancelamento = assinatura.MotivoCancelamento,
            PlanoAssinatura = plano != null ? MapToDto(plano) : null,
            DataCriacao = assinatura.DataCriacao,
            DataAtualizacao = assinatura.DataAtualizacao
        };
    }
}
