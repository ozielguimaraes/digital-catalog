using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data.Repository;

public static class CatalogoRepositoryExtensions
{
    public static async Task<ICollection<Catalogo>> ObterCatalogosPorUsuarioIdAsync(this ApplicationDbContext context, string userId)
    {
        return await context.Catalogos
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public static async Task<Catalogo?> ObterCatalogoComProdutoAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Catalogos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Produtos)
            .ThenInclude(p => p.Categoria)
            .Include(c => c.Produtos)
            .ThenInclude(p => p.Estoque)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public static async Task<Catalogo?> ObterCatalogoPorIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Catalogos.FindAsync(id);
    }

    public static async Task AddCatalogoAsync(this ApplicationDbContext context, Catalogo catalogo)
    {
        await context.Catalogos.AddAsync(catalogo);
        await context.SaveChangesAsync();
    }

    public static async Task UpdateCatalogoAsync(this ApplicationDbContext context, Catalogo catalogo)
    {
        context.Catalogos.Update(catalogo);
        await context.SaveChangesAsync();
    }

    public static async Task DeleteCatalogoAsync(this ApplicationDbContext context, Guid id)
    {
        var catalogo = await context.Catalogos.FindAsync(id);
        if (catalogo != null)
        {
            context.Catalogos.Remove(catalogo);
            await context.SaveChangesAsync();
        }
    }
}
