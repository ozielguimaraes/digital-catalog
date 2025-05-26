using Flunt.Validations;
using MeuCatalogo.Features.Auth.Requests;

namespace MeuCatalogo.Features.Auth.Validators;

public class SigninValidator : Contract<SigninRequest>
{
    public SigninValidator(SigninRequest login)
    {
        Requires()
            .IsEmail(login.UserName, nameof(login.UserName), "E-mail é inválido")
            .IsNotNullOrEmpty(login.UserName, nameof(login.UserName), "E-mail é inválido");

        Requires()
            .IsNotNullOrEmpty(login.Password, nameof(login.Password), "Senha é inválida");
    }
}
