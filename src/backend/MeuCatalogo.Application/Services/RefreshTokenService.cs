using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        ApplicationDbContext context,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRandomToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Refresh token válido por 30 dias
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token gerado para usuário {UserId}", userId);
        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
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
            _logger.LogInformation("Refresh token revogado: {Token}", token);
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
        _logger.LogInformation("Todos os refresh tokens revogados para usuário {UserId}", userId);
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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Access token válido por 15 minutos
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
