using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data.Repository;

public static class CategoriaRepositoryExtensions
{
    public static async Task<Categoria?> GetCategoriaByIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Categorias.FindAsync(id);
    }

    public static async Task<List<Categoria>> GetAllCategoriasAsync(this ApplicationDbContext context)
    {
        return await context.Categorias.ToListAsync();
    }

    public static async Task<bool> HasProdutosAsync(this ApplicationDbContext context, Guid categoriaId)
    {
        return await context.Produtos.AnyAsync(p => p.CategoriaId == categoriaId);
    }

    public static async Task AddCategoriaAsync(this ApplicationDbContext context, Categoria categoria)
    {
        await context.Categorias.AddAsync(categoria);
        await context.SaveChangesAsync();
    }

    public static async Task UpdateCategoriaAsync(this ApplicationDbContext context, Categoria categoria)
    {
        context.Categorias.Update(categoria);
        await context.SaveChangesAsync();
    }

    public static async Task DeleteCategoriaAsync(this ApplicationDbContext context, Guid id)
    {
        var categoria = await context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            context.Categorias.Remove(categoria);
            await context.SaveChangesAsync();
        }
    }
}
