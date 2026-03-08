using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.API.Infrastructure.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Autenticação e autorização de usuários")]
public class AuthController : BaseApiController
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly SymmetricSecurityKey _jwtSigningKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly IPlanoAssinaturaService _planoAssinaturaService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly EmailSender _emailSender;
    private readonly ILogger<AuthController> _logger;
    private readonly IMemoryCache _cache;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IMemoryCache cache,
        IPlanoAssinaturaService planoAssinaturaService,
        IRefreshTokenService refreshTokenService,
        EmailSender emailSender,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));
        _jwtIssuer = configuration["JwtSettings:Issuer"]!;
        _jwtAudience = configuration["JwtSettings:Audience"]!;
        _cache = cache;
        _planoAssinaturaService = planoAssinaturaService;
        _refreshTokenService = refreshTokenService;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <param name="registerDto">Dados de registro do usuário</param>
    /// <returns>Dados do usuário criado</returns>
    /// <response code="200">Usuário registrado com sucesso</response>
    /// <response code="400">Dados inválidos ou email já em uso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Registrar novo usuário",
        Description = "Cria uma nova conta de usuário no sistema e atribui automaticamente um plano gratuito."
    )]
    [SwaggerResponse(200, "Usuário registrado com sucesso", typeof(UserDto))]
    [SwaggerResponse(400, "Dados inválidos ou email já em uso", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Autentica um usuário no sistema
    /// </summary>
    /// <param name="loginDto">Dados de login do usuário</param>
    /// <returns>Token de acesso e dados do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="401">Credenciais inválidas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Fazer login",
        Description = "Autentica um usuário e retorna um token JWT de acesso junto com um refresh token."
    )]
    [SwaggerResponse(200, "Login realizado com sucesso", typeof(SigninResponse))]
    [SwaggerResponse(401, "Credenciais inválidas", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return UnauthorizedResponse("Email ou senha incorretos");
        }

        var loginCacheKey = BuildLoginCacheKey(loginDto.Email, loginDto.Password);
        if (_cache.TryGetValue(loginCacheKey, out SigninResponse? cachedSigninResponse) && cachedSigninResponse != null)
        {
            return HandleApiResponse(ApiResponse<SigninResponse>.Success(cachedSigninResponse));
        }

        _logger.LogDebug("Tentativa de login: {Email}", loginDto.Email);
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user is not { EmailConfirmed: true })
        {
            _logger.LogWarning("Login falhou para {Email}: usuário inexistente ou e-mail não confirmado", loginDto.Email);
            return UnauthorizedResponse("Email ou senha incorretos ou e-mail não confirmado");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, true);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Senha incorreta para {Email}", loginDto.Email);
            return UnauthorizedResponse("Email ou senha incorretos");
        }

        string token = GenerateJwtToken(user);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

        var response = ApiResponse<SigninResponse>.Success(new SigninResponse(token, refreshToken.Token,
            new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Nome = user.Nome,
                DataCriacao = user.DataCriacao
            }));
        _cache.Set(loginCacheKey, response.Data, TimeSpan.FromSeconds(45));

        _logger.LogDebug("Login bem-sucedido para {UserId}", user.Id);

        return HandleApiResponse(response);
    }

    /// <summary>
    /// Renova o token de acesso usando refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Dados do refresh token</param>
    /// <returns>Novo token de acesso e refresh token</returns>
    /// <response code="200">Token renovado com sucesso</response>
    /// <response code="400">Refresh token inválido ou expirado</response>
    /// <response code="401">Refresh token não autorizado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("refresh-token")]
    [SwaggerOperation(
        Summary = "Renovar token de acesso",
        Description = "Gera um novo token de acesso usando um refresh token válido."
    )]
    [SwaggerResponse(200, "Token renovado com sucesso", typeof(RefreshTokenResponse))]
    [SwaggerResponse(400, "Refresh token inválido ou expirado", typeof(ProblemDetails))]
    [SwaggerResponse(401, "Refresh token não autorizado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        _logger.LogInformation("Tentativa de refresh token");

        try
        {
            var signinResponse = await _refreshTokenService.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken);
            var response = ApiResponse<RefreshTokenResponse>.Success(new RefreshTokenResponse
            {
                Token = signinResponse.Token,
                RefreshToken = signinResponse.RefreshToken
            });

            _logger.LogInformation("Refresh token realizado com sucesso");
            return HandleApiResponse(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Refresh token falhou: {Message}", ex.Message);
            return UnauthorizedResponse(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante refresh token");
            return BadRequestResponse("Erro interno do servidor");
        }
    }

    /// <summary>
    /// Faz logout do usuário e invalida tokens
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token opcional para invalidar</param>
    /// <returns>Confirmação do logout</returns>
    /// <response code="200">Logout realizado com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Fazer logout",
        Description = "Invalida os tokens do usuário autenticado, fazendo logout do sistema."
    )]
    [SwaggerResponse(200, "Logout realizado com sucesso")]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
    public async Task<IActionResult> Logout(RefreshTokenDto? refreshTokenDto = null)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Logout solicitado para usuário: {UserId}", userId);

        if (!string.IsNullOrEmpty(userId))
        {
            if (refreshTokenDto != null && !string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
            }
            else
            {
                await _refreshTokenService.RevokeAllRefreshTokensAsync(userId);
            }
        }

        var response = ApiResponse<object>.Success(null, "Logout realizado com sucesso");
        return HandleApiResponse(response);
    }

    /// <summary>
    /// Confirma o email do usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="token">Token de confirmação</param>
    /// <returns>Confirmação da validação do email</returns>
    /// <response code="204">Email confirmado com sucesso</response>
    /// <response code="400">Token inválido ou usuário não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("confirmar-email")]
    [SwaggerOperation(
        Summary = "Confirmar email do usuário",
        Description = "Confirma o endereço de email do usuário usando o token de confirmação."
    )]
    [SwaggerResponse(204, "Email confirmado com sucesso")]
    [SwaggerResponse(400, "Token inválido ou usuário não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Obtém dados do usuário autenticado
    /// </summary>
    /// <returns>Dados do usuário autenticado</returns>
    /// <response code="200">Dados do usuário retornados com sucesso</response>
    /// <response code="401">Usuário não autenticado</response>
    /// <response code="404">Usuário não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [Authorize]
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Obter dados do usuário autenticado",
        Description = "Retorna os dados do usuário atualmente autenticado."
    )]
    [SwaggerResponse(200, "Dados do usuário retornados com sucesso", typeof(UserDto))]
    [SwaggerResponse(401, "Usuário não autenticado", typeof(ProblemDetails))]
    [SwaggerResponse(404, "Usuário não encontrado", typeof(ProblemDetails))]
    [SwaggerResponse(500, "Erro interno do servidor", typeof(ProblemDetails))]
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

    /// <summary>
    /// Solicita recuperação de senha e envia um código para o email cadastrado
    /// </summary>
    /// <param name="forgotPasswordDto">Email ou telefone do usuário</param>
    /// <returns>Status da solicitação</returns>
    [HttpPost("forgot-password")]
    [SwaggerOperation(Summary = "Solicitar recuperação de senha", Description = "Envia um email com instruções para redefinir a senha.")]
    [SwaggerResponse(200, "Email de recuperação enviado")]
    [SwaggerResponse(400, "Usuário não encontrado")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        _logger.LogInformation("Solicitação de recuperação de senha para: {Identifier}", forgotPasswordDto.Identifier);

        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Identifier);

        if (user == null && forgotPasswordDto.Identifier.Any(char.IsDigit))
        {
             user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == forgotPasswordDto.Identifier);
        }

        if (user == null)
        {
            return BadRequestResponse("Usuário não encontrado com este e-mail ou telefone.");
        }

        string? token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var message = new EmailMessage(
            "Recuperação de Senha - Meu Catálogo",
            $@"
                <h2>Recuperação de Senha</h2>
                <p>Olá {user.Nome.Split(' ')[0]},</p>
                <p>Recebemos uma solicitação para redefinir sua senha.</p>

                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <p style='margin: 0; font-weight: bold;'>Seu código de recuperação é:</p>
                    <h1 style='margin: 10px 0; letter-spacing: 5px; color: #333;'>{token}</h1>
                </div>

                <p>Se você não solicitou isso, ignore este email.</p>
            ",
            user.Email
        );

        bool emailSent = await _emailSender.EnviarEmailAsync(message);

        if (!emailSent)
        {
            return BadRequestResponse("Erro ao enviar email de recuperação. Tente novamente mais tarde.");
        }

        return HandleApiResponse(ApiResponse<string>.Success(token, "Email de recuperação enviado."));
    }

    /// <summary>
    /// Redefine a senha do usuário
    /// </summary>
    /// <param name="resetPasswordDto">Dados para redefinição</param>
    /// <returns>Status da operação</returns>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Redefinir senha", Description = "Define uma nova senha usando o token de recuperação.")]
    [SwaggerResponse(200, "Senha redefinida com sucesso")]
    [SwaggerResponse(400, "Erro na redefinição")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            return BadRequestResponse("Usuário não encontrado.");
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

        if (result.Succeeded)
        {
            return HandleApiResponse(ApiResponse<string>.Success(null, "Senha redefinida com sucesso. Você já pode fazer login."));
        }

        var errors = result.Errors.Select(e => e.Description);
        return BadRequestResponse($"Erro ao redefinir senha: {string.Join(", ", errors)}");
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.UserName),
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

    private static string BuildLoginCacheKey(string email, string password)
    {
        var payload = $"{email.Trim().ToLowerInvariant()}:{password}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return $"auth:login:{Convert.ToHexString(hash)}";
    }
}
