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
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly ApplicationDbContext _context;
    private readonly SymmetricSecurityKey _jwtSigningKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
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
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var existingToken = await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.ExpiresAt)
            .FirstOrDefaultAsync();

        if (existingToken != null)
        {
            return existingToken;
        }

        var refreshToken = new RefreshToken
        {
            Token = GenerateRandomToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
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
        
        if (token == null || !await IsRefreshTokenValidAsync(token))
        {
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");
        }

        var user = await _userManager.FindByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado");
        }

        // Revogar o refresh token atual
        await RevokeRefreshTokenAsync(refreshToken);

        // Gerar novo access token
        var newAccessToken = GenerateJwtToken(user);

        // Gerar novo refresh token
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

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
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return TokenHandler.WriteToken(token);
    }
}
