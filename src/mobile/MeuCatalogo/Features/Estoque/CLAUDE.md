# Estoque Feature

Stock configuration bottom sheet for the product add/edit flow. Pure UI state — no repository, no remote calls, no persistence.

## Files

- **`EstoqueBottomSheet.xaml` / `EstoqueBottomSheetViewModel.cs`**: Stock settings sheet.

## Patterns

- **Input → flags → output**: Sheet receives current values via `BottomSheetParameters.DisponivelEmEstoqueSelecionado`, `EstoqueIlimitadoSelecionado`, `QuantidadeEmEstoqueSelecionada`. On save, returns the updated values via `GoBackAsync(parameters)`.
- **Observable flags**: `DisponivelEmEstoque`, `EstoqueIlimitado`, `QuantidadeEmEstoque` (nullable int). `partial void OnPropertyChanged(...)` triggers `ValidateEstoque()` per keystroke.
- **Conditional validation**: Quantity is only validated when `DisponivelEmEstoque == true && EstoqueIlimitado == false`.
- **`_hasTypedQuantity` guard**: Prevents the "invalid quantity" error from appearing before the user actually types anything.
- **`CanSalve` RelayCommand guard**: `SalveCommand` is disabled unless the current state is valid — the Save button binds to `CanExecute`.

## Integration

- Opened from `Features/Produto/Presentation/ProdutoAdicionarPageViewModel` via `IBottomSheetNavigationService.NavigateToAsync<EstoqueBottomSheetViewModel>(BottomSheetKeys.Estoque, parameters)`.
- Registered via `AddBottomSheet<EstoqueBottomSheet, EstoqueBottomSheetViewModel>(BottomSheetKeys.Estoque)` in `ServiceCollectionExtension`.
- Results are merged into the `Produto` request before `UpsertProdutoOfflineFirstUseCase` runs — stock persistence happens downstream inside the product save, not here.

## Gotchas

- **No persistence here**: Saving the sheet only returns values; it does not touch the database or the API. Stock is stored when the product is saved.
- **Three flags are independent but related**: `DisponivelEmEstoque = false` makes the quantity fields irrelevant; `EstoqueIlimitado = true` hides the quantity control. Toggling one must not silently clear the others' state.
- **Quantity is `int?`** — null means "not entered yet", 0 is a valid entered value (out of stock). Don't collapse null and 0.
- **The Save button enables via `CanSalve`** — if you replace the button binding, preserve the guard or users can submit invalid state.

## When Adding Code

- New stock field → add `[ObservableProperty]`, wire into `ValidateEstoque()`, extend `BottomSheetParameters`, update caller's `OnNavigatedTo`.
- Keep this sheet persistence-free — if you need to call an API, you are solving the wrong problem; move logic into `UpsertProdutoOfflineFirstUseCase`.
