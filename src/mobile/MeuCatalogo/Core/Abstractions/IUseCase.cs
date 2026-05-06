namespace MeuCatalogo.Core.Abstractions;

public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request);
}

public interface IUseCase<in TRequest>
{
    Task ExecuteAsync(TRequest request);
}

public interface IUseCaseOut<TResponse>
{
    Task<TResponse> ExecuteAsync();
}

public interface IUseCaseActivity
{
    Task ExecuteAsync();
}
