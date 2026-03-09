using CommunityToolkit.Mvvm.ComponentModel;

namespace MeuCatalogo.Features.Produto.Models;

public partial class CategoriaModel : ObservableObject
{
    public CategoriaModel() { }

    public CategoriaModel(string nome, string descricao, Guid catalogoId)
    {
        Nome = nome;
        Descricao = descricao;
        CatalogoId = catalogoId;
    }

    public Guid Id { get; set; }
    
    [ObservableProperty] private string _nome = string.Empty;
    
    public string? Descricao { get; set; }
    public Guid CatalogoId { get; set; }

    [ObservableProperty] private bool _isSelected;
}
