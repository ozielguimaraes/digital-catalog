using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MeuCatalogo.Features.Produto.Presentation;

public sealed class ProdutoUpsertedMessage : ValueChangedMessage<string>
{
    public ProdutoUpsertedMessage(string produtoId) : base(produtoId)
    {
    }
}

