using Microsoft.EntityFrameworkCore;
using MeuCatalogo.Application.Entities;

namespace MeuCatalogo.Application.Infrastructure.Data.Repository;

public static class ProdutoRepositoryExtensions
{
    public static async Task AtualizarProdutoAsync(this ApplicationDbContext context, Produto produto)
    {
        await context.AtualizarAsync(produto);
    }

    public static async Task<IList<Produto>> GetProdutosByCatalogoIdAsync(this ApplicationDbContext context, Guid catalogoId)
    {
        return await context.Produtos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Categoria)
            .Include(p => p.Estoque)
            .Include(p => p.Imagens)
            .Where(p => p.CatalogoId == catalogoId)
            .ToListAsync();
    }

    public static async Task<Produto?> ObterProdutoComDetalhesAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Produtos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Categoria)
            .Include(p => p.Estoque)
            .Include(p => p.Variacoes)
            .Include(p => p.Imagens)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public static async Task<Produto?> ObterProdutoPorIdAsync(this ApplicationDbContext context, Guid id)
    {
        return await context.Produtos.FindAsync(id);
    }

    public static async Task UpdateProdutoAsync(this ApplicationDbContext context, Produto produto)
    {
        context.Produtos.Update(produto);
        await context.SaveChangesAsync();
    }

    public static async Task RemoverProdutoAsync(this ApplicationDbContext context, Guid id)
    {
        var produto = await context.Produtos.FindAsync(id);
        if (produto != null)
        {
            context.Produtos.Remove(produto);
            await context.SaveChangesAsync();
        }
    }
}
