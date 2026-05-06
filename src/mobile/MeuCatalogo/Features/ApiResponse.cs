using Refit;

namespace MeuCatalogo.Features;

public class ApiResponse<T>
{
    public T? Dados { get; set; }
    public string? MensagemDeErro { get; set; }
    public ProblemDetails? ProblemDetails { get; set; }

    public bool RetornouComSucesso { get; private set; }
    public bool RetornouComErro => !RetornouComSucesso;

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Dados = data,
            RetornouComSucesso = true
        };
    }

    public static ApiResponse<T> Error(string mensagemDeErro, ProblemDetails? problemDetails = null)
    {
        return new ApiResponse<T>
        {
            MensagemDeErro = mensagemDeErro,
            ProblemDetails = problemDetails,
            RetornouComSucesso = false
        };
    }
}
