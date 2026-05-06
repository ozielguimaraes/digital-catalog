using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Catalogo.UseCases;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalogoEmUsoBottomSheetViewModel(
    GetCatalogosLocalUseCase getCatalogosLocalUseCase,
    SetCatalogoEmUsoUseCase setCatalogoEmUsoUseCase,
    IBottomSheetNavigationService bottomSheetNavigationService) : BasePageViewModel, INavigationAware
{
    [ObservableProperty] private ObservableCollection<CatalogoInfo> _catalogos = [];

    public async void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        var locais = await getCatalogosLocalUseCase.ExecuteAsync();
        Catalogos = new ObservableCollection<CatalogoInfo>(locais);
    }

    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters) { }

    [RelayCommand]
    private async Task Selecionar(CatalogoInfo catalogo)
    {
        await setCatalogoEmUsoUseCase.ExecuteAsync(catalogo);

        var parametros = new BottomSheetNavigationParameters
        {
            { BottomSheetParameters.CatalogoSelecionado, catalogo }
        };

        await bottomSheetNavigationService.GoBackAsync(parametros);
    }
}
