using Flunt.Validations;
using MeuCatalogo.Features.Produto.UseCases;

namespace MeuCatalogo.Features.Produto;

public sealed partial class ProdutoAdicionarPageViewModel
{
    private bool ValidateAll()
    {
        return ValidateFields();
    }

    private bool ValidateFields(params string[]? fields)
    {
        var request = new UpsertProdutoOfflineFirstRequest(
            ProdutoId: Produto?.Id,
            Nome: Nome,
            CategoriaId: Categoria?.Id ?? Guid.Empty,
            CategoriaNome: Categoria?.Nome ?? string.Empty,
            Preco: Preco,
            PrecoComDesconto: _precoComDesconto,
            InformacoesAdicionais: InformacoesAdicionais,
            Imagens: Imagens.ToList(),
            CurrentSyncStatus: Produto?.SyncStatus);

        var validator = new Validators.UpsertProdutoValidator(request);

        ApplyValidationMessages(validator, fields);
        return validator.IsValid;
    }

    private void ApplyValidationMessages(Contract<UpsertProdutoOfflineFirstRequest> validator, params string[]? fields)
    {
        bool validateAll = fields == null || fields.Length == 0;

        if (validateAll || fields.Contains(nameof(UpsertProdutoOfflineFirstRequest.Nome)))
            NomeErrorMessage = string.Empty;
        if (validateAll || fields.Contains(nameof(UpsertProdutoOfflineFirstRequest.Preco)))
            PrecoErrorMessage = string.Empty;
        if (validateAll || fields.Contains(nameof(UpsertProdutoOfflineFirstRequest.PrecoComDesconto)))
            PrecoComDescontoErrorMessage = string.Empty;
        if (validateAll || fields.Contains(nameof(UpsertProdutoOfflineFirstRequest.CategoriaId)) || fields.Contains(nameof(UpsertProdutoOfflineFirstRequest.CategoriaNome)))
            CategoriaErrorMessage = string.Empty;

        foreach (var n in validator.Notifications)
        {
            var key = n.Key;
            var message = n.Message;

            if (key == nameof(UpsertProdutoOfflineFirstRequest.Nome) && (validateAll || fields.Contains(key)))
                NomeErrorMessage = message;

            if (key == nameof(UpsertProdutoOfflineFirstRequest.Preco) && (validateAll || fields.Contains(key)))
                PrecoErrorMessage = message;

            if (key == nameof(UpsertProdutoOfflineFirstRequest.PrecoComDesconto) && (validateAll || fields.Contains(key)))
                PrecoComDescontoErrorMessage = message;

            if ((key == nameof(UpsertProdutoOfflineFirstRequest.CategoriaId) || key == nameof(UpsertProdutoOfflineFirstRequest.CategoriaNome)) &&
                (validateAll || fields.Contains(key)))
                CategoriaErrorMessage = message;
        }
    }
}
