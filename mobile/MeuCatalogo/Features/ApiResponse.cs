namespace MeuCatalogo.Features;

public class ApiResponse<T>
{
    public T? Dados { get; set; }
    public bool RetornouComSucesso => string.IsNullOrEmpty(MensageDeErro);
    public bool RetornouComErro => !RetornouComSucesso;
    public string? MensageDeErro { get; set; }

    public static ApiResponse<T> Success(T data, string mensagem = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            MensageDeErro = mensagem,
            Dados = data
        };
    }

    public static ApiResponse<T> Error(string mensagem, List<string>? erros = null)
    {
        return new ApiResponse<T>
        {
            MensageDeErro = mensagem
        };
    }
}
