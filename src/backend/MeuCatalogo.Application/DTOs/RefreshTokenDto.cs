namespace MeuCatalogo.Application.DTOs;

public class RefreshTokenDto
{
    public string RefreshToken { get; set; }
}

public class RefreshTokenResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}
