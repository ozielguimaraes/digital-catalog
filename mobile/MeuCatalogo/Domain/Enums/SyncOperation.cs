namespace MeuCatalogo.Domain.Enums;

public enum SyncOperation
{
    Create,
    Update,
    Delete,
    PullCatalogos,
    PullProdutosByCatalogoId,
    PullCategoriasByCatalogoId
}
