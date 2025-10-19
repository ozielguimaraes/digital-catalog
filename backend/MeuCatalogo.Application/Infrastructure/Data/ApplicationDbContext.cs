using MeuCatalogo.Application.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<ProdutoImagem> ProdutoImagens { get; set; }
    public DbSet<Variacao> Variacoes { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }
    public DbSet<PlanoAssinatura> PlanosAssinatura { get; set; }
    public DbSet<AssinaturaUsuario> AssinaturasUsuarios { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Ignorar se for IdentityUser, IdentityRole, IdentityUserRole, etc.
            if (typeof(IdentityUser).IsAssignableFrom(clrType) ||
                typeof(IdentityRole).IsAssignableFrom(clrType) ||
                typeof(IdentityUserRole<string>).IsAssignableFrom(clrType) ||
                typeof(IdentityUserClaim<string>).IsAssignableFrom(clrType) ||
                typeof(IdentityUserLogin<string>).IsAssignableFrom(clrType) ||
                typeof(IdentityRoleClaim<string>).IsAssignableFrom(clrType) ||
                typeof(IdentityUserToken<string>).IsAssignableFrom(clrType))
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                {
                    property.SetMaxLength(100);
                }
            }
        }

        // Configurações para Catalogo
        if (!modelBuilder.Model.FindEntityType(typeof(Catalogo)).GetForeignKeys().Any(fk => fk.PrincipalEntityType.ClrType == typeof(ApplicationUser)))
        {
            modelBuilder.Entity<Catalogo>()
                .HasOne(c => c.User)
                .WithMany(u => u.Catalogos)
                .HasForeignKey(c => c.UserId);
        }

        // Configurações para Categoria
        if (!modelBuilder.Model.FindEntityType(typeof(Categoria)).GetForeignKeys().Any(fk => fk.PrincipalEntityType.ClrType == typeof(Catalogo)))
        {
            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.Catalogo)
                .WithMany()
                .HasForeignKey(c => c.CatalogoId);
        }

        // Configurações para Pedido
        if (!modelBuilder.Model.FindEntityType(typeof(Pedido)).GetForeignKeys().Any(fk => fk.PrincipalEntityType.ClrType == typeof(Cliente)))
        {
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId);
        }

    }
}
