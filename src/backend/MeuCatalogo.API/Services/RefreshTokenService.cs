using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MeuCatalogo.API.Services;

public class RefreshTokenService : IRefreshTokenService
{
    // Reuso de um token já rotacionado dentro desta janela é tratado como retry
    // de rede (mobile + Polly reenviam o refresh após timeout), não como roubo.
    private static readonly TimeSpan ReuseGracePeriod = TimeSpan.FromSeconds(30);

    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly ApplicationDbContext _context;
    private readonly SymmetricSecurityKey _jwtSigningKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        ApplicationDbContext context,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));
        _jwtIssuer = configuration["JwtSettings:Issuer"]!;
        _jwtAudience = configuration["JwtSettings:Audience"]!;
        _accessTokenMinutes = configuration.GetValue<int?>("JwtSettings:AccessTokenMinutes") ?? 15;
        _refreshTokenDays = configuration.GetValue<int?>("JwtSettings:RefreshTokenDays") ?? 30;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        // Sempre cria um token novo: cada login/sessão (device) tem o seu.
        // Reaproveitar um token existente fazia dois devices compartilharem a
        // mesma sessão — a rotação de um derrubava o outro e inviabilizava
        // detectar reuso malicioso.
        var refreshToken = new RefreshToken
        {
            Token = GenerateRandomToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Refresh token gerado para usuário {UserId}", userId);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogDebug("Refresh token revogado");
        }
    }

    public async Task RevokeAllRefreshTokensAsync(string userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogDebug("Todos os refresh tokens revogados para usuário {UserId}", userId);
    }

    public async Task<bool> IsRefreshTokenValidAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    public async Task<SigninResponse> RefreshAccessTokenAsync(string refreshToken)
    {
        var token = await GetRefreshTokenAsync(refreshToken);

        if (token == null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");
        }

        if (token.IsRevoked)
        {
            // Token rotacionado sendo apresentado de novo: fora da janela de
            // graça isso indica vazamento — revoga todas as sessões do usuário.
            if (token.RevokedAt is { } revokedAt && DateTime.UtcNow - revokedAt > ReuseGracePeriod)
            {
                _logger.LogWarning(
                    "Reuso de refresh token revogado detectado para usuário {UserId}; revogando todas as sessões.",
                    token.UserId);
                await RevokeAllRefreshTokensAsync(token.UserId);
            }

            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");
        }

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");
        }

        var user = await _userManager.FindByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado");
        }

        // Rotação: revoga o token atual e emite um novo que herda o ExpiresAt,
        // impondo um teto absoluto de sessão (renovar não estende o prazo).
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;

        var newAccessToken = GenerateJwtToken(user);

        var newRefreshToken = new RefreshToken
        {
            Token = GenerateRandomToken(),
            UserId = user.Id,
            ExpiresAt = token.ExpiresAt,
            IsRevoked = false
        };
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nome = user.Nome,
            DataCriacao = user.DataCriacao
        };

        _logger.LogInformation("Token renovado para usuário {UserId}", user.Id);

        return new SigninResponse(newAccessToken, newRefreshToken.Token, userDto);
    }

    private string GenerateRandomToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var creds = new SigningCredentials(_jwtSigningKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtIssuer,
            _jwtAudience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: creds);

        return TokenHandler.WriteToken(token);
    }
}
