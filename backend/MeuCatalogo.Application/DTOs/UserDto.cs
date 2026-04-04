using System;

namespace MeuCatalogo.Application.DTOs;

public class UserDto
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public DateTime DataCriacao { get; set; }
    public CatalogoFavoritoDto? CatalogoFavorito { get; set; }
}

public class CatalogoFavoritoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
}

public class UserRegisterDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class UserLoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserUpdateDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class UserChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmNewPassword { get; set; }
}
