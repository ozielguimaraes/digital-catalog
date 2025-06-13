using MeuCatalogo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeuCatalogo.Application.Interfaces;

public interface ICatalogoService
{
    Task<IEnumerable<CatalogoDto>> GetCatalogosByUserIdAsync(string userId);
    Task<CatalogoDto?> ObterCatalogoPorIdAsync(Guid id, string userId);
    Task<CatalogoDto> CreateCatalogoAsync(CatalogoCreateDto catalogoDto, string userId);
    Task<CatalogoDto?> UpdateCatalogoAsync(Guid id, CatalogoUpdateDto catalogoDto, string userId);
    Task DeleteCatalogoAsync(Guid id, string userId);
}
