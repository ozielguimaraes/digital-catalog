namespace MeuCatalogo.Application.DTOs.Responses;

public record SigninResponse(string Token, string RefreshToken, UserDto User);
