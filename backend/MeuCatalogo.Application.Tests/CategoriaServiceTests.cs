using Xunit;

namespace MeuCatalogo.Application.Tests;

public class CategoriaServiceTests
{
    [Fact]
    public void Teste_Basico_De_Exemplo()
    {
        // Arrange
        var valor = 10;
        var esperado = 10;

        // Act
        var resultado = valor;

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void Teste_De_String_Nao_Nula()
    {
        // Arrange
        var texto = "MeuCatalogo";

        // Act & Assert
        Assert.NotNull(texto);
        Assert.NotEmpty(texto);
    }

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(10, 20, 30)]
    [InlineData(-5, 5, 0)]
    public void Teste_De_Soma(int a, int b, int esperado)
    {
        // Act
        var resultado = a + b;

        // Assert
        Assert.Equal(esperado, resultado);
    }
}