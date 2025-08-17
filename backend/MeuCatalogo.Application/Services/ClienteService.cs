using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

    public ClienteService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<ClienteResponse>> CreateAsync(ClienteRequest request)
    {
        try
        {
            // Verificar se já existe um cliente com o mesmo email
            var clienteExistente = await _dbContext.Clientes
                .FirstOrDefaultAsync(c => c.Email == request.Email);

            if (clienteExistente != null)
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

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteResponse>.Error($"Erro ao criar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error("Cliente não encontrado");

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteResponse>.Error($"Erro ao buscar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> GetByEmailAsync(string email)
    {
        try
        {
            var cliente = await _dbContext.Clientes
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error("Cliente não encontrado");

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteResponse>.Error($"Erro ao buscar cliente por email: {ex.Message}");
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
            return ApiResponse<List<ClienteResponse>>.Error($"Erro ao buscar clientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteResponse>> UpdateAsync(Guid id, ClienteRequest request)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<ClienteResponse>.Error("Cliente não encontrado");

            // Verificar se o novo email já está em uso por outro cliente
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

            _dbContext.Clientes.Update(cliente);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<ClienteResponse>.Success(MapToResponse(cliente));
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteResponse>.Error($"Erro ao atualizar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);

            if (cliente == null)
                return ApiResponse<bool>.Error("Cliente não encontrado");

            // Verificar se o cliente possui pedidos
            var clienteComPedidos = await _dbContext.Pedidos
                .AnyAsync(p => p.ClienteId == id);

            if (clienteComPedidos)
                return ApiResponse<bool>.Error("não u00e9 possu00edvel excluir o cliente pois ele possui pedidos");

            _dbContext.Clientes.Remove(cliente);
            await _dbContext.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Cliente removido com sucesso");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Error($"Erro ao remover cliente: {ex.Message}");
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
}
