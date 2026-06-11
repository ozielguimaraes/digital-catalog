using CommunityToolkit.Mvvm.ComponentModel;
using MeuCatalogo.Core.Abstractions;

namespace MeuCatalogo.Features;

// Base para telas de formulário. Rastreia se o usuário alterou algum campo
// (dirty) para que o AppShell possa confirmar o descarte ao sair.
public abstract partial class FormPageViewModel : BasePageViewModel, IDirtyAware
{
    private bool _inicializando;

    [ObservableProperty] private bool _temAlteracoes;

    public bool TemAlteracoesNaoSalvas => TemAlteracoes;

    // Envolve o preenchimento inicial dos campos para que os OnXxxChanged
    // disparados durante o load não sejam contados como alteração do usuário.
    protected void CarregarSemMarcar(Action carregar)
    {
        _inicializando = true;
        try
        {
            carregar();
        }
        finally
        {
            _inicializando = false;
            TemAlteracoes = false;
        }
    }

    protected void MarcarAlterado()
    {
        if (!_inicializando)
            TemAlteracoes = true;
    }

    protected void LimparAlteracoes() => TemAlteracoes = false;
}
