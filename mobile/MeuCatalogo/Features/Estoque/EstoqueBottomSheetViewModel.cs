using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Estoque;

public sealed partial class EstoqueBottomSheetViewModel(IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    [ObservableProperty] private string _titulo = "Estoque";
    [ObservableProperty] private bool _disponivelEmEstoque = true;
    [ObservableProperty] private bool _estoqueIlimitado = true;
    [ObservableProperty] private int? _quantidadeEmEstoque;
    [ObservableProperty] private string? _quantidadeEmEstoqueErrorMessage;

    partial void OnDisponivelEmEstoqueChanged(bool value)
    {
        Console.WriteLine($"DisponivelEmEstoque alterado para {value}");
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {

    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {

    }

    [RelayCommand]
    private async Task Salvar()
    {
        await bottomSheetNavigationService.GoBackAsync(new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.DisponivelEmEstoqueSelecionado, DisponivelEmEstoque },
            { BottomSheetParameters.EstoqueIlimitadoSelecionado, EstoqueIlimitado },
            { BottomSheetParameters.QuantidadeEmEstoqueSelecionada, QuantidadeEmEstoque }
        });
    }
}
