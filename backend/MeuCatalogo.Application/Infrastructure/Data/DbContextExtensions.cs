using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data;

public static class DbContextExtensions
{
    public static async Task AdicionarAsync<TEntity>(this ApplicationDbContext context, TEntity entity) where TEntity : class
    {
        await context.Set<TEntity>().AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public static async Task AtualizarAsync<TEntity>(this ApplicationDbContext context, TEntity entity) where TEntity : class
    {
        context.Update(entity);
        await context.SaveChangesAsync();
    }

    #region Catalogo Extensions

    public static async Task<IEnumerable<Catalogo>> ObterCatalogosPorUuarioIdAsync(this ApplicationDbContext context, string usuarioId)
    {
        return await context.Catalogos
            .Where(c => c.UserId == usuarioId)
            .ToListAsync();
    }

    public static async Task<Catalogo?> ObterCatalogoComProdutosAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Catalogos
            .Include(c => c.Produtos)
            .ThenInclude(p => p.Categoria)
            .Include(c => c.Produtos)
            .ThenInclude(p => p.Estoque)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    #endregion

    #region Produto Extensions

    public static async Task<Produto?> GetProdutoWithDetailsAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.Estoque)
            .Include(p => p.Variacoes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public static async Task<Produto?> GetProdutoByIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Produtos.FindAsync(id);
    }

    public static async Task UpdateProdutoAsync(this ApplicationDbContext context, Produto produto)
    {
        context.Produtos.Update(produto);
        await context.SaveChangesAsync();
    }

    public static async Task DeleteProdutoAsync(this ApplicationDbContext context, Guid id)
    {
        var produto = await context.Produtos.FindAsync(id);
        if (produto != null)
        {
            context.Produtos.Remove(produto);
            await context.SaveChangesAsync();
        }
    }

    #endregion
}
