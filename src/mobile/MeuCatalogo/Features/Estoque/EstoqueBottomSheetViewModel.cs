using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Estoque;

public sealed partial class EstoqueBottomSheetViewModel(IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    private bool _hasTypedQuantity = false;

    [ObservableProperty] private string _titulo = "Estoque";
    [ObservableProperty] private bool _disponivelEmEstoque = true;
    [ObservableProperty] private bool _estoqueIlimitado = true;
    [ObservableProperty] private int? _quantidadeEmEstoque;
    [ObservableProperty] private string? _quantidadeEmEstoqueErrorMessage;

    partial void OnDisponivelEmEstoqueChanged(bool value)
    {
        ValidateEstoque();
    }

    partial void OnEstoqueIlimitadoChanged(bool value)
    {
        ValidateEstoque();
    }

    partial void OnQuantidadeEmEstoqueChanged(int? value)
    {
        _hasTypedQuantity = true;
        ValidateEstoque();
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {

    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        _hasTypedQuantity = false;
        QuantidadeEmEstoqueErrorMessage = string.Empty;
    }

    private void ValidateEstoque()
    {
        bool isInvalid = DisponivelEmEstoque && !EstoqueIlimitado && (!QuantidadeEmEstoque.HasValue || QuantidadeEmEstoque <= 0);

        if (isInvalid && _hasTypedQuantity)
        {
            QuantidadeEmEstoqueErrorMessage = "Informe uma quantidade válida maior que zero.";
        }
        else
        {
            QuantidadeEmEstoqueErrorMessage = string.Empty;
        }
        SalveCommand.NotifyCanExecuteChanged();
    }

    private bool CanSalve()
    {
        if (!DisponivelEmEstoque) return true;
        if (EstoqueIlimitado) return true;

        return QuantidadeEmEstoque is > 0;
    }

    [RelayCommand(CanExecute = nameof(CanSalve))]
    private async Task SalveAsync()
    {
        var parameters = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.DisponivelEmEstoqueSelecionado, DisponivelEmEstoque },
            { BottomSheetParameters.EstoqueIlimitadoSelecionado, EstoqueIlimitado }
        };

        if (QuantidadeEmEstoque.HasValue)
        {
            parameters.Add(BottomSheetParameters.QuantidadeEmEstoqueSelecionada, QuantidadeEmEstoque.Value);
        }

        await bottomSheetNavigationService.GoBackAsync(parameters);
    }
}
