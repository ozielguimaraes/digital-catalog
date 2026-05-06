# Auth Feature

Login, signup, and session management. Offline-aware for reads (cached user) but online-only for writes (signin/signup).

## Files

- **`LoginPage.xaml` / `LoginPageViewModel.cs`**: Login screen.
- **`SignupPage.xaml` / `SignupPageViewModel.cs`**: Registration screen.
- **`Data/AuthRepository.cs` (`IAuthRepository`)**: Orchestrates remote auth + local token persistence.
- **`Data/AuthLocalDataSource.cs` (`IAuthLocalDataSource`)**: `SecureStorage`-backed token store.
- **`Data/UserRepository.cs` (`IUserRepository`)**: Local SQLite cache of the current user.
- **`Data/Remote/AuthRemoteDataSource.cs` (`IAuthRemoteDataSource`)**: Calls `IAuthApi`.
- **`Data/Remote/IAuthApi.cs`**: Refit interface — signin, signup, refresh-token, forgot-password, reset-password.
- **`Data/Remote/Contracts/Requests/`**: `SigninRequest`, `SignupRequest`, `RefreshTokenRequest`.
- **`Data/Remote/Contracts/Responses/`**: `SigninResponse`, `UserResponse`, `RefreshTokenResponse`.
- **`Domain/UserEntity.cs`**: SQLite row for the cached current user.
- **`UseCases/SigninUseCase.cs`**: Validates input → calls repository → sets tokens + user cache → sets `CatalogoFavorito` if returned.
- **`UseCases/SignupUseCase.cs`**: Mirror of Signin for new accounts.
- **`UseCases/GetCurrentUserUseCase.cs`**: Reads from `IUserRepository` (local cache).
- **`UseCases/GetStartupRouteUseCase.cs`**: Decides startup page based on auth state + `CatalogoFavorito`.
- **`UseCases/LogoutUseCase.cs`**: Clears tokens, user cache, `CatalogoFavorito`, and the sync queue.
- **`Validators/SigninValidator.cs`**: Flunt `Contract<SigninRequest>` — email required, email format, password required. `SignupValidator` is more lenient (some rules commented out).

## Patterns

- **Validator in UseCase (fail-fast)**: `SigninUseCase` runs `SigninValidator` before touching the repository; invalid requests return `ApiResponse<T>.Error(notifications)` without a network call.
- **Token flow**: `AuthRepository.SigninAsync` → `IAuthApi.Signin` → on success, `AuthLocalDataSource.SetTokensAsync` + `UserRepository.SaveAsync`.
- **AppShell swap on login**: `LoginPageViewModel` calls `INavigationService` directly to swap the shell (not via a use case) because the root chrome changes.
- **Refresh token** is handled by `Infrastructure/Auth/AuthenticationHandler` — the Refit client auto-refreshes 401s.

## Integration

- Depends on: `Infrastructure/Auth/*`, `Infrastructure/Database/AppDbContext`, `Core/Base/*`, `Features/Settings/ISettingsService`.
- Consumed by: `AppShell` / `App.xaml.cs` at startup via `GetStartupRouteUseCase`.
- Remote endpoint: `ApiConstants.BaseUrl + /auth/*`.

## Gotchas

- **Debug-only hardcoded credentials** in `LoginPageViewModel` (`#if DEBUG`). Do not promote these into release builds.
- **`/auth/login`, `/auth/refresh-token`, `/auth/register` are excluded** from `AuthenticationHandler` token injection — do not add `[Authorize]` header manually to them.
- **`SigninUseCase` sets `CatalogoFavorito`** when the signin response carries one — downstream features assume a favorite exists. Skipping this breaks product creation.
- **`LogoutUseCase` must clear the sync queue**; otherwise pending-sync items for the previous user will fire against the new user's token.
- **Validator placement**: Runs inside the UseCase, not the ViewModel. If you see validation logic in a ViewModel here, it's a mistake — move it to the validator.
- **`SignupValidator` has commented-out password rules** — confirm backend enforces these (Identity password policy) since client-side does not.
- **Tokens are in `SecureStorage`** on device, not raw `Preferences`. Don't switch to `Preferences` for "simplicity" — credentials must be secured.

## When Adding Code

- New auth endpoint → add Refit method to `IAuthApi`, data source wrapper, repository method, use case, validator (Flunt).
- Exclude new auth endpoints from token injection in `AuthenticationHandler` if they should not carry a Bearer token.
- Keep Flunt validators pure — no DI, just `Contract<T>` with `Requires()` rules.
