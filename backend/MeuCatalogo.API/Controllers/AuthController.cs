using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IPlanoAssinaturaService _planoAssinaturaService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IPlanoAssinaturaService planoAssinaturaService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _planoAssinaturaService = planoAssinaturaService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    public async Task<IActionResult> Register(UserRegisterDto registerDto)
    {
        _logger.LogInformation("Iniciando registro de novo usuário: {Email}", registerDto.Email);

        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            _logger.LogWarning("Tentativa de registro com email já em uso: {Email}", registerDto.Email);
            return BadRequestResponse("Email já está em uso");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName, Email = registerDto.Email, Nome = registerDto.Nome, EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogError("Falha ao registrar usuário {Email}: {@Errors}", registerDto.Email, errors);
            return BadRequestResponse(string.Join(", ", errors));
        }

        await _userManager.AddToRoleAsync(user, "User");

        try
        {
            await _planoAssinaturaService.AtribuirPlanoGratuitoAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atribuir plano gratuito ao usuário {UserId}", user.Id);
            await _userManager.DeleteAsync(user);
            throw;
        }

        var response = ApiResponse<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nome = user.Nome,
            DataCriacao = user.DataCriacao
        });

        _logger.LogInformation("Usuário registrado com sucesso: {UserId}", user.Id);
        return HandleApiResponse(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SigninResponse))]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        _logger.LogInformation("Tentativa de login: {Email}", loginDto.Email);
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user is not { EmailConfirmed: true })
        {
            _logger.LogWarning("Login falhou para {Email}: usuário inexistente ou e-mail não confirmado", loginDto.Email);
            return UnauthorizedResponse("Email ou senha incorretos ou e-mail não confirmado");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Senha incorreta para {Email}", loginDto.Email);
            return UnauthorizedResponse("Email ou senha incorretos");
        }

        string token = GenerateJwtToken(user);
        var response = ApiResponse<SigninResponse>.Success(new SigninResponse(token,
            new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Nome = user.Nome,
                DataCriacao = user.DataCriacao
            }));

        _logger.LogInformation("Login bem-sucedido para {UserId}", user.Id);

        return HandleApiResponse(response);
    }

    [HttpGet("confirmar-email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SigninResponse))]
    public async Task<ActionResult> ConfirmEmail(string userId, string token)
    {
        _logger.LogInformation("Confirmação de e-mail solicitada para {UserId}", userId);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário inválido na confirmação de e-mail: {UserId}", userId);
            return BadRequestResponse("Usuário inválido");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("E-mail confirmado com sucesso para {UserId}", userId);
            return NoContentResponse();
        }

        _logger.LogWarning("Falha na confirmação de e-mail para {UserId}", userId);
        return BadRequestResponse("Erro ao confirmar e-mail");
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    public async Task<IActionResult> GetCurrentUser()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Obtendo dados do usuário autenticado: {UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Usuário autenticado não encontrado: {UserId}", userId);
            return NotFoundResponse("Usuário não encontrado");
        }

        var response = ApiResponse<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nome = user.Nome,
            DataCriacao = user.DataCriacao
        });

        return HandleApiResponse(response);
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
