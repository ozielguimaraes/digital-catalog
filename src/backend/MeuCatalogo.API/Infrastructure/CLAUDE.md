# API Infrastructure

API-layer infrastructure helpers. Keep anything that does not need ASP.NET Core types in `MeuCatalogo.Application/Infrastructure` instead.

## Submodules

- **`Messages/`**: SMTP email sending.
  - `EmailSender.cs` — wraps `SmtpClient`; `EnviarEmailAsync(EmailMessage)` returns `bool` (catches and swallows transport errors, returns false on failure).
  - `EmailMessage.cs` — `Assunto`, `Corpo` (HTML), `Destinatarios`, `ReplyTo`, `Attachments`.

## Patterns

- **Transient registration** for `EmailSender` in `Program.cs`. Reads credentials from the `EmailSettings` config section; if the section is missing the DI registration falls back to a dummy sender so dev environments don't crash.

## Integration

- `EmailSender` is consumed by `AuthController` flows (email confirmation, password reset).
- Credentials come from `appsettings.*.json` → `EmailSettings` binding.

## Gotchas

- **Failures are silent** — `EnviarEmailAsync` returns `false` rather than throwing. Callers must check the return value or log explicitly; do not assume success.
- **Dummy sender fallback** masks misconfigured email settings in development. In production, confirm the real sender is registered.
- **HTML body only** — `Corpo` is interpreted as HTML. Plain-text emails still need HTML-escaping.
