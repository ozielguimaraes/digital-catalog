using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeuCatalogo.Application.Entities;

public class ApplicationUser : IdentityUser
{
    public string Nome { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
    public ICollection<Catalogo> Catalogos { get; set; }
    public ICollection<AssinaturaUsuario> Assinaturas { get; set; }

    public ApplicationUser()
    {
        DataCriacao = DateTime.UtcNow;
        Ativo = true;
        Catalogos = new List<Catalogo>();
        Assinaturas = new List<AssinaturaUsuario>();
    }

    public AssinaturaUsuario? ObterAssinaturaAtiva()
    {
        return Assinaturas?.FirstOrDefault(a => a.EstaAtiva());
    }

    public bool TemPlanoAtivo()
    {
        return ObterAssinaturaAtiva() != null;
    }

    public PlanoAssinatura? ObterPlanoAtivo()
    {
        return ObterAssinaturaAtiva()?.PlanoAssinatura;
    }

    public bool PodeAdicionarProduto(int quantidadeAtual)
    {
        var plano = ObterPlanoAtivo();
        return plano == null || plano.LimiteProdutos <= 0 || quantidadeAtual < plano.LimiteProdutos;
    }

    public bool PodeAdicionarCatalogo(int quantidadeAtual)
    {
        var plano = ObterPlanoAtivo();
        return plano == null || plano.LimiteCatalogos <= 0 || quantidadeAtual < plano.LimiteCatalogos;
    }
}