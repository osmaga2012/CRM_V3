using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CRM.V3.Shared.Pages
{
    public partial class DetalleUsuario : ComponentBase
    {
        [Parameter]
        public long UsuarioId { get; set; }

        private bool isLoading = true;
        private bool guardando = false;
        private bool esNuevo => UsuarioId == 0;
        private string errorMessage = string.Empty;
        
        private UsuarioDto usuario = new();
        private List<EmpresasDto> empresas = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                isLoading = true;

                // Cargar empresas para el dropdown
                var empresasResult = await servicioEmpresas.GetAllAsync("api/Empresa", null, null);
                empresas = empresasResult?.ToList() ?? new List<EmpresasDto>();

                if (esNuevo)
                {
                    // Inicializar nuevo usuario
                    usuario = new UsuarioDto
                    {
                        Activo = true,
                        Rol = "Cliente",
                        FechaRegistro = DateTime.UtcNow
                    };
                }
                else
                {
                    // Cargar usuario existente
                    usuario = await servicioUsuarios.GetByIdAsync($"api/Usuario/{UsuarioId}", UsuarioId);
                    
                    if (usuario == null)
                    {
                        errorMessage = "Usuario no encontrado";
                        usuario = new UsuarioDto();
                    }
                }

                isLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
                errorMessage = $"Error al cargar datos: {ex.Message}";
                isLoading = false;
            }
        }

        private async Task GuardarUsuario()
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                errorMessage = "El nombre es obligatorio";
                return;
            }

            if (string.IsNullOrWhiteSpace(usuario.NIFAcceso))
            {
                errorMessage = "El NIF es obligatorio";
                return;
            }

            if (string.IsNullOrWhiteSpace(usuario.EMail))
            {
                errorMessage = "El email es obligatorio";
                return;
            }

            if (esNuevo && string.IsNullOrWhiteSpace(usuario.PasswordHash))
            {
                errorMessage = "La contraseña es obligatoria para nuevos usuarios";
                return;
            }

            if (esNuevo && usuario.PasswordHash?.Length < 6)
            {
                errorMessage = "La contraseña debe tener al menos 6 caracteres";
                return;
            }

            try
            {
                guardando = true;
                errorMessage = string.Empty;

                // Si no hay email de avisos, usar el email principal
                if (string.IsNullOrWhiteSpace(usuario.EMailAvisos))
                {
                    usuario.EMailAvisos = usuario.EMail;
                }

                // Si no hay nombre de usuario, generar uno basado en el email
                if (string.IsNullOrWhiteSpace(usuario.NombreUsuario) && !string.IsNullOrWhiteSpace(usuario.EMail))
                {
                    usuario.NombreUsuario = usuario.EMail.Split('@')[0];
                }

                if (esNuevo)
                {
                    // Validar que el NIF no esté duplicado
                    var usuariosExistentes = await servicioUsuarios.GetAllAsync("api/Usuarios", null, null);
                    if (usuariosExistentes?.Any(u => u.NIFAcceso == usuario.NIFAcceso) == true)
                    {
                        errorMessage = "El NIF ya está registrado en el sistema";
                        guardando = false;
                        return;
                    }

                    // Crear nuevo usuario
                    var resultado = await servicioUsuarios.CreateAsync("api/Usuarios", usuario);
                    if (resultado != null && resultado.Success)
                    {
                        NavigationManager.NavigateTo("/usuarios");
                    }
                    else
                    {
                        errorMessage = resultado?.Message ?? "Error al crear el usuario";
                    }
                }
                else
                {
                    // Actualizar usuario existente
                    var resultado = await servicioUsuarios.UpdateAsync($"api/Usuario/{usuario.Id}", usuario);
                    if (resultado != null && resultado.Success)
                    {
                        NavigationManager.NavigateTo("/usuarios");
                    }
                    else
                    {
                        errorMessage = resultado?.Message ?? "Error al actualizar el usuario";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar usuario: {ex.Message}");
                errorMessage = $"Error al guardar: {ex.Message}";
            }
            finally
            {
                guardando = false;
            }
        }

        private void Volver()
        {
            NavigationManager.NavigateTo("/usuarios");
        }
    }
}
