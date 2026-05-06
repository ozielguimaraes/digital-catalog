using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MeuCatalogo.Features.Produto.Presentation;

public sealed class ProdutoUpsertedMessage(string produtoId) : ValueChangedMessage<string>(produtoId);

