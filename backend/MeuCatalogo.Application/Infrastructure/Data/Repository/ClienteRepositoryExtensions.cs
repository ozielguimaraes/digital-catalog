using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data.Repository;

public static class ClienteRepositoryExtensions
{
    public static async Task<Cliente?> GetClienteByIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Clientes.FindAsync(id);
    }

    public static async Task<Cliente?> GetClienteByEmailAsync(this ApplicationDbContext context, string email)
    {
        return await context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
    }

    public static async Task<bool> EmailExistsAsync(this ApplicationDbContext context, string email, Guid? excludeId = null)
    {
        var query = context.Clientes.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(c => c.Email == email);
    }

    public static async Task<List<Cliente>> GetAllClientesAsync(this ApplicationDbContext context)
    {
        return await context.Clientes.ToListAsync();
    }

    public static async Task<bool> HasPedidosAsync(this ApplicationDbContext context, Guid clienteId)
    {
        return await context.Pedidos.AnyAsync(p => p.ClienteId == clienteId);
    }

    public static async Task AddClienteAsync(this ApplicationDbContext context, Cliente cliente)
    {
        await context.Clientes.AddAsync(cliente);
        await context.SaveChangesAsync();
    }

    public static async Task UpdateClienteAsync(this ApplicationDbContext context, Cliente cliente)
    {
        context.Clientes.Update(cliente);
        await context.SaveChangesAsync();
    }

    public static async Task DeleteClienteAsync(this ApplicationDbContext context, Guid id)
    {
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            context.Clientes.Remove(cliente);
            await context.SaveChangesAsync();
        }
    }
}
