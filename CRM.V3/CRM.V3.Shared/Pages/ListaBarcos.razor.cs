using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using CRM.V3.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CRM.V3.Shared.Pages
{
    public partial class ListaBarcos : ComponentBase
    {
        [Inject] private IApiClient<BarcosDto> servicioBarco { get; set; }
        [Inject] private IApiClient<EmpresasDto> servicioEmpresas { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IFormFactor PlatformService { get; set; }
        [Inject] private IPlatformNavigationService NavigationService { get; set; }
        [Inject] private IJSRuntime js { get;set;}
 
        private ICollection<BarcosDto> barcos = new List<BarcosDto>();
        private ICollection<EmpresasDto> empresas = new List<EmpresasDto>();

        private BarcosDto barcoSeleccionado;
        private string _filtroBarco = string.Empty;

        private string _emailInput = string.Empty;
        private string _emailError = string.Empty;
        private List<string> EmailsAviso = new();
        private bool IsMobile = false;

        // Propiedades de paginación
        private int _paginaActual = 1;
        private int _itemsPorPagina = 10;
        private int TotalPaginas => (int)Math.Ceiling((double)FiltroEmpresas.Count() / _itemsPorPagina);
        private int TotalRegistros => FiltroEmpresas.Count();
        private int RegistroInicio => (_paginaActual - 1) * _itemsPorPagina + 1;
        private int RegistroFin => Math.Min(_paginaActual * _itemsPorPagina, TotalRegistros);

        // Propiedad que QuickGrid usará para mostrar los datos filtrados (siempre devuelve IQueryable no nulo)
        protected IQueryable<EmpresasDto> FiltroEmpresas
        {
            get
            {
                var source = empresas ?? Enumerable.Empty<EmpresasDto>();
                if (string.IsNullOrWhiteSpace(_filtroBarco))
                {
                    return source.AsQueryable();
                }
                return source.Where(filtroEmpresa).AsQueryable();
            }
        }

        // Propiedad para obtener los datos paginados
        protected IEnumerable<EmpresasDto> EmpresasPaginadas
        {
            get
            {
                return FiltroEmpresas
                    .Skip((_paginaActual - 1) * _itemsPorPagina)
                    .Take(_itemsPorPagina)
                    .ToList();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await CargarBarcos();


            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //if (firstRender) 
            //{ 
            //    var dimensions = await js.InvokeAsync<WindowDimensions>("getWindowDimensions"); 
            //    IsMobile = dimensions.Width < 768; 
            //    StateHasChanged(); 
            //}
        }
        public class WindowDimensions { 
            public int Width { get; set; } 
        }
        
        private async Task CargarBarcos()
        {
            string[] includesEmpresas = new string[] { "Barcos" };
            var result = await servicioEmpresas.GetAllAsync("api/Empresa", null, includesEmpresas);
            empresas = result?.ToList() ?? new List<EmpresasDto>();
        }

        // quick filter - filter globally across multiple columns with the same input
        private Func<EmpresasDto, bool> filtroEmpresa => x =>
        {
            if (string.IsNullOrWhiteSpace(_filtroBarco))
            {
                return true; // Si no hay texto de búsqueda, mostrar todas las empresas
            }

            var textoBusquedaLower = _filtroBarco.ToLowerInvariant();

            // Revisa nulos para evitar NullReferenceException
            if (x.CodigoBarco.ToString().Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;


            if (!string.IsNullOrEmpty(x.NombreArmador) && x.NombreArmador.Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Barco != null && !string.IsNullOrEmpty(x.Barco!.NombreB) && x.Barco!.NombreB.Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Barco != null && !string.IsNullOrEmpty(x.Barco.NombreA) && x.Barco.NombreA.Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Barco != null && !string.IsNullOrEmpty(x.Barco.CapitanNombre) && x.Barco.CapitanNombre.Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.CodigoEmpresa != null && !string.IsNullOrEmpty(x.CodigoEmpresa) && x.CodigoEmpresa.Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;

            if ($"{x.CodigoBanco} {x.Barco!.NombreB} {x.NombreArmador} {x.Barco?.CapitanNombre} {x.CodigoEmpresa} ".Contains(textoBusquedaLower, StringComparison.OrdinalIgnoreCase))
                return true;



            return false;
        };

        // Navegación: usa NavigationService en MAUI (hilo UI) y NavigationManager en web
        public void OpenTramites(string codigoEmpresa, BarcosDto barco)
        {
            if (PlatformService != null && PlatformService.IsNativeApp && NavigationService != null)
            {
                NavigationService.NavigateToAsync($"barco/empresa/{codigoEmpresa}/tramites/{barco.CodigoBarco}");
            }
            else
            {
                NavigationManager.NavigateTo($"barco/empresa/{codigoEmpresa}/tramites/{barco.CodigoBarco}");
            }
        }

        public async Task OpenTramitesAsync(string codigoEmpresa, BarcosDto barco)
        {
            if (PlatformService != null && PlatformService.IsNativeApp && NavigationService != null)
            {
                await NavigationService.NavigateToAsync($"barco/empresa/{codigoEmpresa}/tramites/{barco.CodigoBarco}");
            }
            else
            {
                NavigationManager.NavigateTo($"barco/empresa/{codigoEmpresa}/tramites/{barco.CodigoBarco}");
            }
        }

        public async Task VerUsuariosBarco(EmpresasDto empresa)
        {
            //var parameters = new DialogParameters { ["Empresa"] = empresa };
            //var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };

            //var dialog = DialogService.ShowAsync<CrearUsuario>("Ver y Crear usuarios", parameters, options);
            //var result = (await dialog);
        }

        #region Métodos de Paginación

        private void IrAPagina(int numeroPagina)
        {
            if (numeroPagina >= 1 && numeroPagina <= TotalPaginas)
            {
                _paginaActual = numeroPagina;
                StateHasChanged();
            }
        }

        private void PaginaAnterior()
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                StateHasChanged();
            }
        }

        private void PaginaSiguiente()
        {
            if (_paginaActual < TotalPaginas)
            {
                _paginaActual++;
                StateHasChanged();
            }
        }

        private void CambiarItemsPorPagina(int cantidad)
        {
            _itemsPorPagina = cantidad;
            _paginaActual = 1; // Resetear a la primera página
            StateHasChanged();
        }

        // Método para obtener los números de página a mostrar
        private List<int> ObtenerNumerosPagina()
        {
            var paginas = new List<int>();
            int maxPaginasMostrar = 5;
            int mitad = maxPaginasMostrar / 2;

            int inicio = Math.Max(1, _paginaActual - mitad);
            int fin = Math.Min(TotalPaginas, inicio + maxPaginasMostrar - 1);

            // Ajustar el inicio si estamos cerca del final
            if (fin - inicio < maxPaginasMostrar - 1)
            {
                inicio = Math.Max(1, fin - maxPaginasMostrar + 1);
            }

            for (int i = inicio; i <= fin; i++)
            {
                paginas.Add(i);
            }

            return paginas;
        }

        #endregion

        // Sobrescribir el cambio de filtro para resetear la paginación
        private void OnFiltroChanged(string nuevoFiltro)
        {
            _filtroBarco = nuevoFiltro;
            _paginaActual = 1; // Resetear a la primera página cuando se filtra
            StateHasChanged();
        }
    }
}

