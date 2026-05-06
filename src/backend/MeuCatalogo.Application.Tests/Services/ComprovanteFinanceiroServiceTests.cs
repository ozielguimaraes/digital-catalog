using System.Text;
using FluentAssertions;
using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Services;
using MeuCatalogo.Application.Tests.Helpers;
using Xunit;

namespace MeuCatalogo.Application.Tests.Services;

public class ComprovanteFinanceiroServiceTests
{
    private const string UserId = "user-1";
    private const string OutroUserId = "user-2";

    private static MemoryStream Stream(int bytes = 100)
    {
        var arr = new byte[bytes];
        new Random(42).NextBytes(arr);
        return new MemoryStream(arr);
    }

    [Fact]
    public async Task UploadAsync_PersisteEntidadeERetornaUrl()
    {
        await using var test = new TestDbContext();
        var service = new ComprovanteFinanceiroService(test.Db, new StubStorageService());

        var result = await service.UploadAsync(UserId, "recibo.pdf", "application/pdf", 100, Stream(), "Aluguel");

        result.IsSuccess.Should().BeTrue();
        result.Type.Should().Be(ResponseType.Created);
        result.Data!.Url.Should().StartWith("/stub/comprovantes-financeiros/user-1/");
        test.Db.ComprovantesFinanceiros.Should().HaveCount(1);
    }

    [Fact]
    public async Task UploadAsync_ContentTypeNaoSuportado_RetornaErro()
    {
        await using var test = new TestDbContext();
        var service = new ComprovanteFinanceiroService(test.Db, new StubStorageService());

        var result = await service.UploadAsync(UserId, "x.exe", "application/x-msdownload", 100, Stream(), null);

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11 * 1024 * 1024)]
    public async Task UploadAsync_TamanhoInvalido_RetornaErro(long size)
    {
        await using var test = new TestDbContext();
        var service = new ComprovanteFinanceiroService(test.Db, new StubStorageService());

        var result = await service.UploadAsync(UserId, "x.png", "image/png", size, Stream(), null);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_DeOutroUser_RetornaNotFound()
    {
        await using var test = new TestDbContext();
        var service = new ComprovanteFinanceiroService(test.Db, new StubStorageService());
        var c = await service.UploadAsync(OutroUserId, "x.png", "image/png", 100, Stream(), null);

        var result = await service.GetByIdAsync(c.Data!.Id, UserId);

        result.IsSuccess.Should().BeFalse();
        result.Type.Should().Be(ResponseType.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_SemUso_Remove()
    {
        await using var test = new TestDbContext();
        var service = new ComprovanteFinanceiroService(test.Db, new StubStorageService());
        var c = await service.UploadAsync(UserId, "x.png", "image/png", 100, Stream(), null);

        var result = await service.DeleteAsync(c.Data!.Id, UserId);

        result.IsSuccess.Should().BeTrue();
        test.Db.ComprovantesFinanceiros.Should().BeEmpty();
    }
}
