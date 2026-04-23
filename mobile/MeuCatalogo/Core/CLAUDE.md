# Core

Cross-cutting abstractions and base types. Small, stable. Mostly interfaces; depended on by every feature.

## Files

- **`Abstractions/IRepository.cs`**: Generic CRUD contract — `GetAsync(id)`, `GetAllAsync()`, `AddAsync(entity)`, `UpdateAsync(entity)`, `DeleteAsync(entity)`. Implemented by `Infrastructure/Database/BaseRepository<T>`.
- **`Abstractions/IUseCase.cs`**: UseCase contract variants — `IUseCase<TIn, TOut>` (input + output), `IUseCase<TIn>` (input only), `IUseCaseOut<TOut>` (output only), and a no-arg variant. Pick the variant that matches your DI registration.
- **`Abstractions/Imaging/IImageProcessor.cs`**: Image compression contract — `CompressAsync(Stream input, int quality, int maxDim)` → compressed `Stream`. Implemented by `Infrastructure/Imaging/MauiImageProcessor`.
- **`Base/`**: Shared base types.
  - (ApiResponse/BaseApiService/BasePageViewModel are referenced across Core and Infrastructure; see `Infrastructure/CLAUDE.md` for wiring and `Features/*/CLAUDE.md` for usage patterns.)

## Patterns

- **Interfaces only, thin contracts**: Core is intentionally small. Anything stateful or I/O-bound belongs in `Infrastructure/`.
- **UseCase variants matter for DI**: Registering against the wrong variant breaks resolution silently. Match the interface arity to the implementation.

## Integration

- Implemented by: `Infrastructure/Database/BaseRepository<T>`, `Infrastructure/Imaging/MauiImageProcessor`, every `*UseCase.cs` across `Features/`.
- Consumed by: every feature's Data and UseCases layers.

## Gotchas

- **Don't add concrete types here** — implementations go in `Infrastructure/`. Keeping Core abstract avoids circular dependencies on MAUI-specific APIs.
- **`IUseCase` has several arities** — pick carefully. A no-arg use case registered as `IUseCase<Request, Response>` will fail to resolve with a cryptic DI error.
- **`IRepository<T>` is generic SQLite CRUD** — features that need richer operations (e.g., query by foreign key) should declare a feature-local `IXxxLocalRepository` interface, not expand this one.
