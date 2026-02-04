
using CRM.V3.Shared.Dtos;
using UsuarioDto = CRM.Dtos.UsuarioDto;

namespace CRM.V3.Shared.Interfaces
{
    public interface IUsuarioService
    {
        Task<PerfilDto?> VerificarPerfilAsync();
        Task<CRM.Dtos.UsuarioDto?> ObtenerPerfilUsuarioAsync();
        Task<CRM.Dtos.UsuarioDto> CrearUsuario(UsuarioDto usuario);
    }
}
