
using CRM.Dtos;
using CRM.Dtos.Response;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using UsuarioDto = CRM.Dtos.UsuarioDto;

namespace CRM.V3.Services
{
    public class EmpresaService : IEmpresaService
    {
        private readonly HttpClient http;
        private readonly AuthenticationStateProvider userState;

        public EmpresaService(HttpClient http, AuthenticationStateProvider userState)
        {
            this.http = http;
            this.userState = userState;
        }

        public async Task<EmpresasDto> GetEmpresaByIdAsync(Guid id)
        {
            var response = await http.GetAsync($"/api/Empresa/{id}");
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<EmpresasDto>();
                return resultado;
            }
            else
            {
                return null;
            }

        }
        public async Task<List<EmpresasDto>> ListadoEmpresaAsync()
        {
            var response = await http.GetAsync("/api/Empresa");

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<List<EmpresasDto>>();
                return resultado;
            }
            else
            {
                return null;
            }
        }
        public async Task<ResponseDto> RegistrarEmpresaAsync(EmpresasDto empresa)
        {
            var usuario = userState.GetAuthenticationStateAsync();

            var response = await http.PostAsJsonAsync("api/empresa", empresa);

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<ResponseDto>();
                return resultado ?? new ResponseDto { Success = false, Message = "Respuesta vacía del servidor." };
            }

            return new ResponseDto { Success = false, Message = "Error en la solicitud al servidor." };
        }
        public async Task<ResponseDto> UpdateEmpresaAsync(EmpresasDto empresa)
        {
            var response = await http.PutAsJsonAsync($"api/Empresa/{empresa.CodigoEmpresa}",empresa);

            var resultado = await response.Content.ReadFromJsonAsync<ResponseDto>();
            return resultado;
        }
        public async Task<List<UsuarioDto>> GetUsuarios(Guid id)
        {
            var response = await http.GetAsync($"api/Empresa/{id.ToString()}/usuarios");

            var resultado = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();
            return resultado;
        }
    }
}
