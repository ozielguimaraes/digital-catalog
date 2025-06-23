using System.Text.RegularExpressions;
using Flunt.Validations;
using MeuCatalogo.Features.Auth.Requests;

namespace MeuCatalogo.Features.Auth.Validators;

public class SigninValidator : Contract<SigninRequest>
{
    public SigninValidator(SigninRequest request)
    {
        Requires()
            .IsNotNullOrEmpty(request.Email, nameof(request.Email), "E-mail é obrigatório");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            Requires()
                .IsEmail(request.Email, nameof(request.Email), "E-mail é inválido");
        }

        Requires()
            .IsNotNullOrEmpty(request.Password, nameof(request.Password), "Senha é obrigatória");

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return;
        }

        // switch (request.Password.Length)
        // {
        //     case < 6:
        //         AddNotification(nameof(request.Password), "Senha deve ter pelo menos 6 caracteres");
        //         break;
        //     case >= 30:
        //         AddNotification(nameof(request.Password), "Senha deve ter no máximo 30 caracteres");
        //         break;
        // }
        //
        // // Expressão regular: pelo menos uma letra e um número
        // var hasLetterAndNumber = Regex.IsMatch(request.Password, @"^(?=.*[A-Za-z])(?=.*\d)");
        //
        // if (!hasLetterAndNumber)
        // {
        //     AddNotification(nameof(request.Password), "Senha deve conter ao menos uma letra e um número");
        // }
    }
}

public class SignupValidator : Contract<SignupRequest>
{
    public SignupValidator(SignupRequest request)
    {
        Requires()
            .IsEmail(request.UserName, nameof(request.UserName), "Login é inválido")
            .IsNotNullOrEmpty(request.UserName, nameof(request.UserName), "Login é inválido");

        Requires()
            .IsEmail(request.Email, nameof(request.Email), "E-mail é inválido")
            .IsNotNullOrEmpty(request.Email, nameof(request.Email), "E-mail é inválido");

        Requires()
            .IsNotNullOrEmpty(request.Password, nameof(request.Password), "Senha é inválida");
    }
}
