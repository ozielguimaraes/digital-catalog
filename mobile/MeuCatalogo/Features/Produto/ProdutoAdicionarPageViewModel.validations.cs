namespace MeuCatalogo.Features.Produto;

public sealed partial class ProdutoAdicionarPageViewModel
{
    #region Validações
    private bool ValidateNome()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            NomeErrorMessage = "Campo Nome é obrigatório";
            return false;
        }
        if (Nome.Length > 50)
        {
            NomeErrorMessage = "Máximo 50 caracteres permitido";
            return false;
        }

        NomeErrorMessage = string.Empty;
        return true;
    }

    private bool ValidatePreco()
    {
        if (Preco < 0)
        {
            PrecoErrorMessage = "Preço deve ser maior que '0'";
            return false;
        }
        PrecoErrorMessage = string.Empty;
        return true;
    }

    private bool ValidatePrecoComDesconto()
    {
        var asds = PrecoString;
        var ghj = PrecoComDescontoString;

        if (_precoComDesconto is null)
        {
            PrecoComDescontoErrorMessage = string.Empty;
            return true;
        }

        if (_precoComDesconto < 0)
        {
            PrecoComDescontoErrorMessage = "Preço com desconto deve ser maior ou igual a '0'";
            return false;
        }
        if (_precoComDesconto > Preco)
        {
            PrecoComDescontoErrorMessage = "Preço com desconto deve ser menor que o preço original";
            return false;
        }
        PrecoComDescontoErrorMessage = string.Empty;
        return true;
    }

    private bool ValidateCategoria()
    {
        if (Categoria is null)
        {
            CategoriaErrorMessage = "Categoria é obrigatória";
            return false;
        }
        CategoriaErrorMessage = string.Empty;
        return true;
    }

    private bool ValidateAll()
    {
        return ValidateNome()
               & ValidatePreco()
               & ValidatePrecoComDesconto()
               & ValidateCategoria();
    }
    #endregion
}
