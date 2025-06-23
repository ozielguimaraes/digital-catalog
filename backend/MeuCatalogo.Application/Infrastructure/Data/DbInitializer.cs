using MeuCatalogo.Application.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MeuCatalogo.Application.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Created role: {role}");
            }
        }

        if (!context.PlanosAssinatura.Any())
        {
            var planoStart = new PlanoAssinatura
            {
                Nome = "Start",
                Descricao = "Plano gratuito com recursos básicos",
                Preco = 0,
                DuracaoEmMeses = 0,
                LimiteProdutos = 20,
                LimiteCatalogos = 1,
                PermiteVariacoes = true,
                PermiteEstoque = true,
                PermiteRelatorios = true,
                PermiteExportacao = true,
                PermiteImportacao = true,
                PermitePersonalizacao = true,
                EhGratuito = true
            };

            var planoEssencial = new PlanoAssinatura
            {
                Nome = "Essencial",
                Descricao = "Plano básico com recursos essenciais",
                Preco = 29.85m,
                DuracaoEmMeses = 1,
                LimiteProdutos = 200,
                LimiteCatalogos = 3,
                PermiteVariacoes = true,
                PermiteEstoque = true,
                PermiteRelatorios = true,
                PermiteExportacao = true,
                PermiteImportacao = true,
                PermitePersonalizacao = true,
                EhGratuito = true
            };

            var planoPremium = new PlanoAssinatura
            {
                Nome = "Premium",
                Descricao = "Plano profissional com recursos avançados",
                Preco = 59.90m,
                DuracaoEmMeses = 1,
                LimiteProdutos = 500,
                LimiteCatalogos = 10,
                PermiteVariacoes = true,
                PermiteEstoque = true,
                PermiteRelatorios = true,
                PermiteExportacao = true,
                PermiteImportacao = true,
                PermitePersonalizacao = true,
                EhGratuito = true
            };

            context.PlanosAssinatura.AddRange(planoStart, planoEssencial, planoPremium);
            await context.SaveChangesAsync();
            Console.WriteLine("Added subscription plans");
        }
    }
}