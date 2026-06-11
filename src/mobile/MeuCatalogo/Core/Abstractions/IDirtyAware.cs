namespace MeuCatalogo.Core.Abstractions;

// Implementado por ViewModels de formulário. O AppShell intercepta a navegação
// (back, troca de aba) e, se houver alterações não salvas, confirma o descarte.
public interface IDirtyAware
{
    bool TemAlteracoesNaoSalvas { get; }
}
