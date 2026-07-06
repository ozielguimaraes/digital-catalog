using FluentAssertions;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ApiRefreshTokenService = MeuCatalogo.API.Services.RefreshTokenService;

namespace MeuCatalogo.Application.Tests.Services;

public class RefreshTokenServiceTests
{
    private static readonly ApplicationUser User = new()
    {
        Id = "user-1",
        UserName = "user@test.com",
        Email = "user@test.com",
        Nome = "Usuário Teste",
        DataCriacao = DateTime.UtcNow
    };

    private static ApiRefreshTokenService NewService(TestDbContext test)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Key"] = "unit-test-signing-key-with-at-least-32-characters!",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:AccessTokenMinutes"] = "15",
                ["JwtSettings:RefreshTokenDays"] = "30"
            })
            .Build();

        var userManager = new UserManager<ApplicationUser>(
            new SingleUserStore(User), null, null, null, null, null, null, null, null);

        return new ApiRefreshTokenService(
            test.Db, configuration, userManager, NullLogger<ApiRefreshTokenService>.Instance);
    }

    private static RefreshToken NovoToken(string token, DateTime expiresAt, bool revoked = false, DateTime? revokedAt = null) => new()
    {
        Token = token,
        UserId = User.Id,
        ExpiresAt = expiresAt,
        IsRevoked = revoked,
        RevokedAt = revokedAt
    };

    [Fact]
    public async Task GenerateRefreshTokenAsync_CriaTokenNovo_MesmoComTokenValidoExistente()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);

        var primeiro = await service.GenerateRefreshTokenAsync(User.Id);
        var segundo = await service.GenerateRefreshTokenAsync(User.Id);

        segundo.Token.Should().NotBe(primeiro.Token);
        test.Db.RefreshTokens.Count().Should().Be(2);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_RotacionaToken_PreservandoExpiracaoAbsoluta()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        var expiraEm = DateTime.UtcNow.AddDays(10);
        var atual = NovoToken("token-atual", expiraEm);
        test.Db.RefreshTokens.Add(atual);
        await test.Db.SaveChangesAsync();

        var response = await service.RefreshAccessTokenAsync("token-atual");

        atual.IsRevoked.Should().BeTrue();
        response.RefreshToken.Should().NotBe("token-atual");
        response.Token.Should().NotBeNullOrEmpty();

        var novo = test.Db.RefreshTokens.Single(rt => rt.Token == response.RefreshToken);
        novo.IsRevoked.Should().BeFalse();
        novo.ExpiresAt.Should().Be(expiraEm);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ReusoForaDaJanelaDeGraca_RevogaTodasAsSessoes()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        test.Db.RefreshTokens.Add(NovoToken("token-vazado", DateTime.UtcNow.AddDays(10),
            revoked: true, revokedAt: DateTime.UtcNow.AddMinutes(-5)));
        var sessaoLegitima = NovoToken("token-outra-sessao", DateTime.UtcNow.AddDays(10));
        test.Db.RefreshTokens.Add(sessaoLegitima);
        await test.Db.SaveChangesAsync();

        var act = () => service.RefreshAccessTokenAsync("token-vazado");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        sessaoLegitima.IsRevoked.Should().BeTrue("reuso malicioso deve derrubar todas as sessões do usuário");
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ReusoDentroDaJanelaDeGraca_NaoRevogaOutrasSessoes()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        test.Db.RefreshTokens.Add(NovoToken("token-race", DateTime.UtcNow.AddDays(10),
            revoked: true, revokedAt: DateTime.UtcNow.AddSeconds(-2)));
        var sessaoLegitima = NovoToken("token-outra-sessao", DateTime.UtcNow.AddDays(10));
        test.Db.RefreshTokens.Add(sessaoLegitima);
        await test.Db.SaveChangesAsync();

        var act = () => service.RefreshAccessTokenAsync("token-race");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        sessaoLegitima.IsRevoked.Should().BeFalse("retry de rede logo após a rotação não é reuso malicioso");
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_TokenExpirado_LancaUnauthorized()
    {
        await using var test = new TestDbContext();
        var service = NewService(test);
        test.Db.RefreshTokens.Add(NovoToken("token-expirado", DateTime.UtcNow.AddDays(-1)));
        await test.Db.SaveChangesAsync();

        var act = () => service.RefreshAccessTokenAsync("token-expirado");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class SingleUserStore : IUserStore<ApplicationUser>
    {
        private readonly ApplicationUser _user;

        public SingleUserStore(ApplicationUser user) => _user = user;

        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult(userId == _user.Id ? _user : null!);

        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
            => Task.FromResult<ApplicationUser>(null!);

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.Id);

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.UserName.ToUpperInvariant());

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public void Dispose()
        {
        }
    }
}
