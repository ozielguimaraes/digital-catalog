using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Infrastructure.Data.Repository;

namespace MeuCatalogo.Application.Services;

public sealed class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(ApplicationDbContext dbContext, ILogger<ClienteService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ApiResponse<ClienteResponse>> CreateAsync(ClienteRequest request)
    {
        try
        {
            if (!IsValidEmail(request.Email))
                return ApiResponse<ClienteResponse>.Error("Email inválido");

            if (!IsValidTelefone(request.Telefone))
                return ApiResponse<ClienteResponse>.Error("Telefone inválido. Deve conter DDD e número com 10 ou 11 dígitos");

            var emailJaCadastrado = await _dbContext.Clientes
                .AnyAsync(c => c.Email == request.Email);

            if (emailJaCadastrado)
                return ApiResponse<ClienteResponse>.Error("Já existe um cliente cadastrado com este email");

            var cliente = new Cliente
            {
                Nome = request.Nome,
                Email = request.Email,
                Telefone = request.Telefone,
                InformacoesAdicionais = request.InformacoesAdicionais
            };

            await _dbContext.Clientes.AddAsync(cliente);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<ClienteResponse>.Success(ResponseType.Created, MapToResponse(cliente));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Falha ao persistir cliente {Email}.", request.Email);
            return ApiResponse<ClienteResponse>.Error("Não foi possível salvar o cliente. Tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar cliente {Email}.", request.Email);
            return ApiResponse<ClienteResponse>.Error("Erro inesperado ao criar cliente.");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error(ResponseType.NotFound, "Cliente não encontrado");

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente {ClienteId}.", id);
            return ApiResponse<ClienteResponse>.Error("Erro ao buscar cliente.");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> GetByEmailAsync(string email)
    {
        try
        {
            var cliente = await _dbContext.Clientes
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error(ResponseType.NotFound, "Cliente não encontrado");

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por email {Email}.", email);
            return ApiResponse<ClienteResponse>.Error("Erro ao buscar cliente por email.");
        }
    }

    public async Task<ApiResponse<List<ClienteResponse>>> GetAllAsync()
    {
        try
        {
            var clientes = await _dbContext.Clientes.ToListAsync();
            var response = clientes.Select(c => MapToResponse(c)).ToList();

            return ApiResponse<List<ClienteResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes.");
            return ApiResponse<List<ClienteResponse>>.Error("Erro ao buscar clientes.");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> UpdateAsync(Guid id, ClienteRequest request)
    {
        try
        {
            if (!IsValidEmail(request.Email))
                return ApiResponse<ClienteResponse>.Error("Email inválido");

            if (!IsValidTelefone(request.Telefone))
                return ApiResponse<ClienteResponse>.Error("Telefone inválido. Deve conter DDD e número com 10 ou 11 dígitos");

            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error(ResponseType.NotFound, "Cliente não encontrado");

            if (cliente.Email != request.Email)
            {
                var emailEmUso = await _dbContext.Clientes
                    .AnyAsync(c => c.Email == request.Email && c.Id != id);

                if (emailEmUso)
                    return ApiResponse<ClienteResponse>.Error("O email informado já está em uso por outro cliente");
            }

            cliente.Nome = request.Nome;
            cliente.Email = request.Email;
            cliente.Telefone = request.Telefone;
            cliente.InformacoesAdicionais = request.InformacoesAdicionais;
            cliente.DataAtualizacao = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Falha ao atualizar cliente {ClienteId}.", id);
            return ApiResponse<ClienteResponse>.Error("Não foi possível atualizar o cliente. Tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao atualizar cliente {ClienteId}.", id);
            return ApiResponse<ClienteResponse>.Error("Erro inesperado ao atualizar cliente.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<bool>.Error(ResponseType.NotFound, "Cliente não encontrado");

            var clienteComPedidos = await _dbContext.Pedidos
                .AnyAsync(p => p.ClienteId == id);

            if (clienteComPedidos)
                return ApiResponse<bool>.Error("Não é possível excluir o cliente pois ele possui pedidos");

            _dbContext.Clientes.Remove(cliente);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.Success(ResponseType.Deleted, true, "Cliente removido com sucesso");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Falha ao remover cliente {ClienteId}.", id);
            return ApiResponse<bool>.Error("Não foi possível remover o cliente. Tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao remover cliente {ClienteId}.", id);
            return ApiResponse<bool>.Error("Erro inesperado ao remover cliente.");
        }
    }

    private ClienteResponse MapToResponse(Cliente cliente)
    {
        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            InformacoesAdicionais = cliente.InformacoesAdicionais
        };
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        return emailRegex.IsMatch(email);
    }

    private bool IsValidTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return false;

        var telefoneLimpo = Regex.Replace(telefone, @"[^0-9]", "");
        
        return telefoneLimpo.Length >= 10 && telefoneLimpo.Length <= 11;
    }
}
