using MeuCatalogo.Core.Abstractions.Imaging;
using System.Diagnostics;
using System.Globalization;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Catalogo;
using MeuCatalogo.Features.Categoria;
using MeuCatalogo.Features.Estoque;
using MeuCatalogo.Features.Categoria.Models;
using MeuCatalogo.Features.Categoria.UseCases;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;
using MeuCatalogo.Features.Produto.Presentation;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Features.Produto.UseCases;
using MeuCatalogo.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Plugin.Maui.BottomSheet.Navigation;

namespace MeuCatalogo.Features.Produto;

public sealed partial class ProdutoAdicionarPageViewModel(
    ILogger<ProdutoAdicionarPageViewModel> logger,
    UpsertProdutoOfflineFirstUseCase upsertProdutoOfflineFirstUseCase,
    GetCategoriasByCatalogoUseCase getCategoriasByCatalogoUseCase,
    ISettingsService settingsService,
    IBottomSheetNavigationService bottomSheetNavigationService,
    IServiceProvider serviceProvider,
    INavigationService navigationService)
    : BasePageViewModel, INavigationAware, IQueryAttributable
{
    private CancellationTokenSource? _ctsCategorias;
    private Task<ApiResponse<List<CategoriaModel>>>? _taskCarregaCategorias;
    private Guid? _categoriasCatalogoId;

    private UploadProdutoImageUseCase? _uploadProdutoImageUseCase;
    private IImageProcessor? _imageProcessor;

    private UploadProdutoImageUseCase UploadProdutoImageUseCase =>
        _uploadProdutoImageUseCase ??= serviceProvider.GetRequiredService<UploadProdutoImageUseCase>();

    private IImageProcessor ImageProcessor =>
        _imageProcessor ??= serviceProvider.GetRequiredService<IImageProcessor>();

    private INavigationService NavigationService { get; } = navigationService;

    [ObservableProperty] private string _nome;
    [ObservableProperty] private string _nomeErrorMessage;

    [ObservableProperty] private decimal _preco;
    [ObservableProperty] private string _precoString = string.Empty;
    [ObservableProperty] private string _precoErrorMessage;

    private decimal? _precoComDesconto;
    [ObservableProperty] private string _precoComDescontoString = string.Empty;
    [ObservableProperty] private string _precoComDescontoErrorMessage;

    [ObservableProperty] private string? _informacoesAdicionais;
    [ObservableProperty] private string? _informacoesAdicionaisErrorMessage;

    [ObservableProperty] private CategoriaModel? _categoria;
    [ObservableProperty] private string? _categoriaErrorMessage;

    [ObservableProperty] private int? _estoque;
    [ObservableProperty] private string? _estoqueErrorMessage;

    [ObservableProperty] private string _titulo = "Novo produto";

    [ObservableProperty] private ObservableCollection<ProdutoImagemResponse> _imagens = [];
    [ObservableProperty] private bool _isProcessandoImagem;

    private ProdutoImagemResponse? _imagemSendoArrastada;

    [RelayCommand]
    private void ArrastarImagemIniciado(ProdutoImagemResponse imagem)
    {
        _imagemSendoArrastada = imagem;
    }

    [RelayCommand]
    private void SoltarImagem(ProdutoImagemResponse imagemDestino)
    {
        if (_imagemSendoArrastada == null || _imagemSendoArrastada == imagemDestino)
            return;

        int indexOrigem = Imagens.IndexOf(_imagemSendoArrastada);
        int indexDestino = Imagens.IndexOf(imagemDestino);

        if (indexOrigem != -1 && indexDestino != -1)
        {
            var lista = Imagens.ToList();
            var item = lista[indexOrigem];
            lista.RemoveAt(indexOrigem);
            lista.Insert(indexDestino, item);

            // Atualiza ordem e quem é principal
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].Ordem = i + 1;
                lista[i].IsPrincipal = (i == 0);
            }

            Imagens = new ObservableCollection<ProdutoImagemResponse>(lista);

            if (Produto != null)
                Produto.Imagens = lista;
        }

        _imagemSendoArrastada = null;
    }

    [ObservableProperty] private ProdutoResponse? _produto;
    [ObservableProperty] private bool _isSaving;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Produto", out object? produtoObj) && produtoObj is ProdutoResponse produto)
        {
            Produto = produto;
            Nome = produto.Nome;
            InformacoesAdicionais = produto.InformacoesAdicionais;
            Categoria = new CategoriaModel(produto.CategoriaNome, string.Empty, produto.CatalogoId) { Id = produto.CategoriaId };
            Preco = produto.Preco;
            Estoque = produto.Estoque?.Quantidade;
            Imagens = new ObservableCollection<ProdutoImagemResponse>(produto.Imagens);
            PrecoString = produto.Preco.ToString("N2");
            PrecoComDescontoString = produto.PrecoComDesconto?.ToString("N2") ?? string.Empty;
            Titulo = "Editar produto";
        }
        else
        {
            Produto = null;
            Nome = string.Empty;
            PrecoString = string.Empty;
            PrecoComDescontoString = string.Empty;
            InformacoesAdicionais = string.Empty;
            Categoria = null;
            Preco = 0;
            _precoComDesconto = null;
            Estoque = null;
            Imagens = [];
            Titulo = "Novo produto";
        }
    }

    #region Conversão Preços
    partial void OnNomeChanged(string value) => ValidateFields(nameof(UpsertProdutoOfflineFirstRequest.Nome));

    partial void OnPrecoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Preco = 0;
        }
        else if (TentarConverterPreco(value, out decimal preco))
        {
            Preco = preco;
        }
        ValidateFields(nameof(UpsertProdutoOfflineFirstRequest.Preco), nameof(UpsertProdutoOfflineFirstRequest.PrecoComDesconto));
    }

    partial void OnPrecoComDescontoStringChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _precoComDesconto = null;
        }
        else if (TentarConverterPreco(value, out decimal preco))
        {
            _precoComDesconto = preco;
        }
        ValidateFields(nameof(UpsertProdutoOfflineFirstRequest.PrecoComDesconto));
    }

    private static bool TentarConverterPreco(string? value, out decimal preco)
    {
        var culture = CultureInfo.CurrentCulture;
        string? sanitized = value?.Trim();

        return decimal.TryParse(
            sanitized,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowCurrencySymbol,
            culture,
            out preco);
    }
    #endregion

    #region Navegação
    public void OnNavigatedFrom(IBottomSheetNavigationParameters parameters)
    {
    }

    public void OnNavigatedTo(IBottomSheetNavigationParameters parameters)
    {
        try
        {
            if (parameters.TryGetValue(BottomSheetParameters.CategoriaSelectionada, out object? categoriaSelecionada) &&
                categoriaSelecionada is CategoriaModel categoria)
            {
                Categoria = categoria;
                CategoriaErrorMessage = string.Empty;
                ValidateFields(nameof(UpsertProdutoOfflineFirstRequest.CategoriaId), nameof(UpsertProdutoOfflineFirstRequest.CategoriaNome));
            }

            if (!parameters.TryGetValue(BottomSheetParameters.DisponivelEmEstoqueSelecionado, out object? disponivelObj) ||
                disponivelObj is not bool disponivel)
            {
                return;
            }

            if (!disponivel)
            {
                Estoque = 0;
            }
            else
            {
                if (parameters.TryGetValue(BottomSheetParameters.EstoqueIlimitadoSelecionado, out object? ilimitadoObj) && ilimitadoObj is true)
                {
                    Estoque = null;
                }
                else if (parameters.TryGetValue(BottomSheetParameters.QuantidadeEmEstoqueSelecionada, out object? qtdObj))
                {
                    Estoque = qtdObj switch
                    {
                        int quantidade => quantidade,
                        string qtdStr when int.TryParse(qtdStr, out int qtdInt) => qtdInt,
                        _ => Estoque
                    };
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar retorno da navegação");
        }
    }
    #endregion

    #region Categorias
    [RelayCommand]
    private async Task ExibirCategorias()
    {
        try
        {
            if (_taskCarregaCategorias == null)
                await CarregarCategoriasCommand.ExecuteAsync(null);

            if (_taskCarregaCategorias == null)
                return;

            var categoriasResponse = await _taskCarregaCategorias;

            if (categoriasResponse.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(categoriasResponse));
                await Application.Current.MainPage.DisplayAlert("Erro", mensagemErro, "OK");
                return;
            }

            var parametros = new BottomSheetNavigationParameters
            {
                { BottomSheetParameters.Categorias, categoriasResponse.Dados! }
            };

            await bottomSheetNavigationService.NavigateToAsync<CategoriaBottomSheetViewModel>(
                BottomSheetKeys.ListaCategoria, parametros);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Carregamento de categorias cancelado.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao exibir as categorias");
            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível exibir as categorias", "OK");
        }
    }

    [RelayCommand]
    private async Task CarregarCategorias()
    {
        if (settingsService.CatalogoEmUso is null)
        {
            logger.LogWarning("Nenhum catálogo em uso definido.");

            await Application.Current.MainPage.DisplayAlert("Erro",
                "Nenhum catálogo em uso. Por favor, selecione um catálogo.", "OK");

            await NavigationService.NavigateToAsync(nameof(CatalogoListaPage));
            return;
        }

        var catalogoId = settingsService.CatalogoEmUso.Id;
        if (_taskCarregaCategorias != null && _categoriasCatalogoId == catalogoId)
            return;

        CancelarCarregamentoCategorias();
        _categoriasCatalogoId = catalogoId;

        _ctsCategorias = new CancellationTokenSource();
        var ct = _ctsCategorias.Token;

        _taskCarregaCategorias = CarregarCategoriasInternoAsync(catalogoId, ct);
    }

    private async Task<ApiResponse<List<CategoriaModel>>> CarregarCategoriasInternoAsync(Guid catalogoId, CancellationToken ct)
    {
        var response = await getCategoriasByCatalogoUseCase.ExecuteAsync(catalogoId);
        if (response.RetornouComErro)
            return ApiResponse<List<CategoriaModel>>.Error(response.MensagemDeErro ?? "Erro ao carregar categorias", response.ProblemDetails);

        var models = response.Dados!
            .Select(c => new CategoriaModel(c.Nome, c.Descricao ?? string.Empty, c.CatalogoId) { Id = c.Id })
            .ToList();

        return ApiResponse<List<CategoriaModel>>.Success(models);
    }

    private void CancelarCarregamentoCategorias()
    {
        if (_ctsCategorias is { IsCancellationRequested: false })
        {
            _ctsCategorias.Cancel();
            _ctsCategorias.Dispose();
        }
        _ctsCategorias = null;
        _taskCarregaCategorias = null;
        _categoriasCatalogoId = null;
    }
    #endregion

    #region Estoque
    [RelayCommand]
    private async Task ExibirEstoque()
    {
        try
        {
            var parametros = new BottomSheetNavigationParameters();

            await bottomSheetNavigationService.NavigateToAsync<EstoqueBottomSheetViewModel>(
                BottomSheetKeys.Estoque, parametros);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao exibir o estoque");
            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível exibir o estoque", "OK");
        }
    }
    #endregion

    #region Produto
    [RelayCommand]
    private async Task AdicionarImagem()
    {
        try
        {
            string action = await Application.Current.MainPage.DisplayActionSheet("Adicionar Imagem", "Cancelar", null, "Tirar Foto", "Galeria de Fotos", "Arquivos");

            if (action == "Cancelar" || string.IsNullOrEmpty(action))
                return;

            FileResult? result = null;
            IsProcessandoImagem = true;

            switch (action)
            {
                case "Tirar Foto" when MediaPicker.Default.IsCaptureSupported:
                    if (!await GarantirPermissaoCameraAsync())
                        return;

                    result = await MediaPicker.Default.CapturePhotoAsync();
                    break;
                case "Tirar Foto":
                    await Application.Current.MainPage.DisplayAlert("Erro", "Câmera não suportada neste dispositivo.", "OK");
                    return;
                case "Galeria de Fotos":
                    result = await MediaPicker.Default.PickPhotoAsync();
                    break;
                case "Arquivos":
                    result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
                    break;
            }

            if (result != null)
            {
                var imagemLocal = await CriarImagemLocalAsync(result);
                Imagens.Add(imagemLocal);
            }
        }
        catch (FeatureNotSupportedException ex)
        {
            logger.LogError(ex, "Erro ao selecionar imagem");
            await Application.Current.MainPage.DisplayAlert("Erro", "A câmera não está disponível neste dispositivo ou configuração.", "OK");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao selecionar imagem");
            await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível abrir câmera/galeria. Verifique permissões do app.", "OK");
        }
        finally
        {
            IsBusy = false;
            IsProcessandoImagem = false;
        }
    }

    [RelayCommand]
    private async Task Salvar()
    {
        try
        {
            if (IsBusy || IsSaving) return;

            IsBusy = true;
            IsSaving = true;

            if (!ValidateAll()) return;

            var upsertRequest = new UpsertProdutoOfflineFirstRequest(
                ProdutoId: Produto?.Id,
                Nome: Nome,
                CategoriaId: Categoria!.Id,
                CategoriaNome: Categoria.Nome,
                Preco: Preco,
                PrecoComDesconto: _precoComDesconto,
                InformacoesAdicionais: InformacoesAdicionais,
                Imagens: Imagens.ToList(),
                CurrentSyncStatus: Produto?.SyncStatus);

            var response = await upsertProdutoOfflineFirstUseCase.ExecuteAsync(upsertRequest);

            if (response.RetornouComErro)
            {
                string mensagemErro = string.Join("\n", ObterErros(response));
                await Application.Current.MainPage.DisplayAlert(response.ProblemDetails!.Title, mensagemErro, "OK");
                return;
            }

            if (response.Dados != null)
            {
                Produto = response.Dados;

                if (Imagens.Count > 0 && Produto.SyncStatus == SyncStatus.Completed)
                    await SincronizarImagensPendentesAsync(Produto.Id);

                if (Produto.SyncStatus != SyncStatus.Completed || Imagens.Any(i => i.SyncStatus != SyncStatus.Completed))
                    await Application.Current.MainPage.DisplayAlert("Sincronização pendente", "Produto salvo localmente. Algumas imagens serão sincronizadas com a API em seguida.", "OK");
            }

            CancelarCarregamentoCategorias();
            if (Produto is not null)
                WeakReferenceMessenger.Default.Send(new ProdutoUpsertedMessage(Produto.Id.ToString()));
            await NavigationService.NavigateToAsync($"//{nameof(ProdutoListaPage)}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar produto");
        }
        finally
        {
            IsSaving = false;
            IsBusy = false;
        }
    }

    private static async Task<bool> GarantirPermissaoCameraAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
            return true;

        await Application.Current.MainPage.DisplayAlert(
            "Permissão necessária",
            "Para tirar foto, permita acesso à câmera nas configurações do app.",
            "OK");
        return false;
    }

    private async Task<ProdutoImagemResponse> CriarImagemLocalAsync(FileResult file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var nomeArquivo = $"{Guid.NewGuid():N}{extension}";
        var caminhoLocal = Path.Combine(FileSystem.AppDataDirectory, nomeArquivo);

        await using (var origem = await file.OpenReadAsync())
        await using (var fluxoComprimido = await ImageProcessor.CompressAsync(origem))
        await using (var destino = File.Create(caminhoLocal))
        {
            await fluxoComprimido.CopyToAsync(destino);
        }

        return new ProdutoImagemResponse
        {
            Id = Guid.NewGuid(),
            Url = caminhoLocal,
            Images = new ProdutoImagemLinksResponse
            {
                Thumbnail = caminhoLocal,
                Medium = caminhoLocal,
                Full = caminhoLocal
            },
            IsPrincipal = Imagens.Count == 0,
            Ordem = Imagens.Count + 1,
            SyncStatus = SyncStatus.Pending
        };
    }

    private async Task SincronizarImagensPendentesAsync(Guid produtoId)
    {
        var atualizadas = Imagens.ToList();

        for (int i = 0; i < atualizadas.Count; i++)
        {
            var imagem = atualizadas[i];
            if (imagem.SyncStatus == SyncStatus.Completed)
                continue;

            // Se a URL já for remota, não precisamos fazer upload novamente
            if (!string.IsNullOrWhiteSpace(imagem.Url) && imagem.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                imagem.SyncStatus = SyncStatus.Completed;
                continue;
            }

            if (string.IsNullOrWhiteSpace(imagem.Url) || !File.Exists(imagem.Url))
                continue;

            var fileResult = new FileResult(imagem.Url);
            var uploadResponse = await UploadProdutoImageUseCase.ExecuteAsync((produtoId, fileResult, imagem.IsPrincipal, imagem.Ordem));

            if (uploadResponse is { RetornouComSucesso: true, Dados: not null })
            {
                uploadResponse.Dados.IsPrincipal = imagem.IsPrincipal;
                uploadResponse.Dados.Ordem = imagem.Ordem;
                atualizadas[i] = uploadResponse.Dados;
            }
        }

        Imagens = new ObservableCollection<ProdutoImagemResponse>(atualizadas);
        if (Produto != null)
            Produto.Imagens = atualizadas;
    }

    [RelayCommand]
    private async Task RemoverImagem(ProdutoImagemResponse imagem)
    {
        if (imagem == null || Imagens.Count == 0)
            return;

        bool confirmar = await Application.Current.MainPage.DisplayAlert(
            "Remover imagem",
            "Deseja remover esta imagem da lista?",
            "Remover",
            "Cancelar");

        if (!confirmar)
            return;

        var lista = Imagens.ToList();
        var imagemNaLista = lista.FirstOrDefault(i => i.Id == imagem.Id) ?? imagem;

        if (!lista.Remove(imagemNaLista))
            return;

        if (imagemNaLista.IsPrincipal && lista.Count > 0)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].IsPrincipal = i == 0;
            }
        }

        Imagens = new ObservableCollection<ProdutoImagemResponse>(lista);

        if (Produto != null)
            Produto.Imagens = lista;
    }
    #endregion
}
