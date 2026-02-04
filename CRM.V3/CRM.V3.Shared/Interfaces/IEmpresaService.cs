using CRM.Dtos;
using CRM.Dtos.Response;
using UsuarioDto = CRM.Dtos.UsuarioDto;

namespace CRM.V3.Shared.Interfaces
{
    public interface IEmpresaService
    {
        Task<List<EmpresasDto>> ListadoEmpresaAsync();
        Task<EmpresasDto> GetEmpresaByIdAsync(Guid id);
        Task<ResponseDto> UpdateEmpresaAsync(EmpresasDto empresa);
        Task<List<UsuarioDto>> GetUsuarios(Guid id);

        //Task<ResponseDto> RegistrarEmpresaAsync(EmpresasDto empresa, UsuarioDto usuario);
        Task<ResponseDto> RegistrarEmpresaAsync(EmpresasDto empresa);
    }
}
