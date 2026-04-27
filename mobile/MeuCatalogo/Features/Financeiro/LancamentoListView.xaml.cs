using System.Collections.ObjectModel;
using MeuCatalogo.Features.Financeiro.Domain;

namespace MeuCatalogo.Features.Financeiro;

public partial class LancamentoListView : ContentView
{
    public LancamentoListView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty LancamentosProperty = BindableProperty.Create(
        nameof(Lancamentos),
        typeof(ObservableCollection<LancamentoInfo>),
        typeof(LancamentoListView));

    public ObservableCollection<LancamentoInfo>? Lancamentos
    {
        get => (ObservableCollection<LancamentoInfo>?)GetValue(LancamentosProperty);
        set => SetValue(LancamentosProperty, value);
    }
}
