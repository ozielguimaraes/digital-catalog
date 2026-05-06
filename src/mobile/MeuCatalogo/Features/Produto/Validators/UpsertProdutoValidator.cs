using Flunt.Validations;
using MeuCatalogo.Features.Produto.UseCases;

namespace MeuCatalogo.Features.Produto.Validators;

public sealed class UpsertProdutoValidator : Contract<UpsertProdutoOfflineFirstRequest>
{
    public UpsertProdutoValidator(UpsertProdutoOfflineFirstRequest request)
    {
        Requires()
            .IsNotNullOrEmpty(request.Nome, nameof(request.Nome), "Nome é obrigatório")
            .IsLowerOrEqualsThan(request.Nome?.Length ?? 0, 50, nameof(request.Nome), "Nome deve ter no máximo 50 caracteres");

        Requires()
            .IsFalse(request.CategoriaId == Guid.Empty, nameof(request.CategoriaId), "Categoria é obrigatória");

        Requires()
            .IsNotNullOrEmpty(request.CategoriaNome, nameof(request.CategoriaNome), "Categoria é obrigatória");

        Requires()
            .IsGreaterThan(request.Preco, 0m, nameof(request.Preco), "Preço deve ser maior que 0");

        if (request.PrecoComDesconto is not null)
        {
            Requires()
                .IsGreaterOrEqualsThan(request.PrecoComDesconto.Value, 0m, nameof(request.PrecoComDesconto), "Preço com desconto deve ser maior ou igual a 0")
                .IsLowerOrEqualsThan(request.PrecoComDesconto.Value, request.Preco, nameof(request.PrecoComDesconto), "Preço com desconto deve ser menor ou igual ao preço");
        }
    }
}

