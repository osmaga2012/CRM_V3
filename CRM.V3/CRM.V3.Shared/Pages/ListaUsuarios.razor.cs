using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging; // Añadir este using

namespace CRM.V3.Shared.Pages
{
    public partial class ListaUsuarios : ComponentBase
    {
        private bool isLoading = true;
        private List<UsuarioDto> usuarios = new();
        private List<UsuarioDto> usuariosFiltrados = new();
        
        // Estadísticas
        private int totalUsuarios = 0;
        private int usuariosActivos = 0;
        private int usuariosInactivos = 0;
        private int usuariosEmpresa = 0;
        
        // Filtros
        private string searchTerm = string.Empty;
        private string filtroEstado = string.Empty;
        private string filtroRol = string.Empty;

        [Inject]
        private ILogger<ListaUsuarios> _logger { get; set; } = default!; // Inyectar el logger

        protected override async Task OnInitializedAsync()
        {
            await CargarUsuarios();
        }

        private async Task CargarUsuarios()
        {
            try
            {
                isLoading = true;
                
                var result = await servicioUsuarios.GetAllAsync("api/Usuarios", null, null);
                usuarios = result?.ToList() ?? new List<UsuarioDto>();
                
                // Calcular estadísticas
                totalUsuarios = usuarios.Count;
                usuariosActivos = usuarios.Count(u => u.Activo == true);
                usuariosInactivos = usuarios.Count(u => u.Activo != true);
                usuariosEmpresa = usuarios.Count(u => !string.IsNullOrEmpty(u.CodigoEmpresa));
                
                // Inicializar lista filtrada
                usuariosFiltrados = usuarios;
                
                isLoading = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios");
                isLoading = false;
            }
        }

        private void FiltrarUsuarios()
        {
            usuariosFiltrados = usuarios;
            
            // Filtrar por término de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var termLower = searchTerm.ToLower();
                usuariosFiltrados = usuariosFiltrados.Where(u =>
                    (!string.IsNullOrEmpty(u.Nombre) && u.Nombre.ToLower().Contains(termLower)) ||
                    (!string.IsNullOrEmpty(u.Apellidos) && u.Apellidos.ToLower().Contains(termLower)) ||
                    (!string.IsNullOrEmpty(u.EMail) && u.EMail.ToLower().Contains(termLower)) ||
                    (!string.IsNullOrEmpty(u.NIFAcceso) && u.NIFAcceso.ToLower().Contains(termLower)) ||
                    (!string.IsNullOrEmpty(u.NombreUsuario) && u.NombreUsuario.ToLower().Contains(termLower))
                ).ToList();
            }
            
            // Filtrar por estado
            if (!string.IsNullOrWhiteSpace(filtroEstado))
            {
                if (filtroEstado == "activos")
                {
                    usuariosFiltrados = usuariosFiltrados.Where(u => u.Activo == true).ToList();
                }
                else if (filtroEstado == "inactivos")
                {
                    usuariosFiltrados = usuariosFiltrados.Where(u => u.Activo != true).ToList();
                }
            }
            
            // Filtrar por rol
            if (!string.IsNullOrWhiteSpace(filtroRol))
            {
                usuariosFiltrados = usuariosFiltrados.Where(u => 
                    u.Rol != null && u.Rol.Equals(filtroRol, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            StateHasChanged();
        }

        private void CrearNuevoUsuario()
        {
            NavigationManager.NavigateTo("/usuario/nuevo");
        }

        private void VerDetalleUsuario(Guid usuarioId)
        {
            NavigationManager.NavigateTo($"/usuario/{usuarioId}");
        }

        private async Task EliminarUsuario(Guid usuarioId)
        {
            try
            {
                var result = await servicioUsuarios.DeleteAsync("api/Usuario/", usuarioId);
                if (result != null && result.Success)
                {
                    await CargarUsuarios();
                    FiltrarUsuarios();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {UsuarioId}", usuarioId);
            }
        }

        private string GetInicialesUsuario(string? nombre, string? apellidos)
        {
            var iniciales = "";
            if (!string.IsNullOrEmpty(nombre))
                iniciales += nombre[0];
            if (!string.IsNullOrEmpty(apellidos))
                iniciales += apellidos[0];
            return iniciales.ToUpper();
        }
    }
}
