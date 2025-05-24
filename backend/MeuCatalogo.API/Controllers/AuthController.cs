using MeuCatalogo.Application.DTOs;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeuCatalogo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IPlanoAssinaturaService _planoAssinaturaService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IPlanoAssinaturaService planoAssinaturaService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _planoAssinaturaService = planoAssinaturaService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(UserRegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return BadRequest("Email already exists");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            Nome = registerDto.Nome
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "User");
        try
        {
            await _planoAssinaturaService.AtribuirPlanoGratuitoAsync(user.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atribuir plano gratuito: {ex.Message}");
        }

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nome = user.Nome,
            DataCriacao = user.DataCriacao
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(UserLoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized("Invalid password");
        }

        return new
        {
            token = GenerateJwtToken(user),
            user = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Nome = user.Nome,
                DataCriacao = user.DataCriacao
            }
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Nome = user.Nome,
            DataCriacao = user.DataCriacao
        };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpiresInDays"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
