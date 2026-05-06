namespace MeuCatalogo.Application.DTOs.Responses;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ResponseType Type { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse()
    {
        Errors = new List<string>();
    }

    public static ApiResponse<T> Success(T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Type = ResponseType.Success,
            Data = data
        };
    }

    public static ApiResponse<T> Success(ResponseType responseType, T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Type = responseType,
            Data = data
        };
    }

    public static ApiResponse<T> Error(string message, params string[] errors)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Type = ResponseType.Validation,
            Errors = errors.ToList(),
        };
    }

    public static ApiResponse<T> Error(ResponseType responseType, string message, params string[] errors)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Type = responseType,
            Errors = errors.ToList(),
        };
    }

    public static ApiResponse<T> Error(ResponseType responseType, string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Type = responseType,
            Errors = errors ?? new List<string>()
        };
    }
}

public enum ResponseType
{
    Success,
    Forbidden,
    Validation,
    NotFound,
    Created,
    Deleted
}
