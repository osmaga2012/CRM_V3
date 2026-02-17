using CRM.V3.Shared.Interfaces;
using CRM.V3.Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace CRM.App.Web.Client.Services
{
    public class WebNavigationService : IPlatformNavigationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<WebNavigationService> _logger;

        public WebNavigationService(NavigationManager navigationManager, ILogger<WebNavigationService> logger)
        {
            _navigationManager = navigationManager;
            _logger = logger;
        }

        public async Task NavigateTo(string route)
        {
            try
            {
                // La llamada original a la navegación
                _navigationManager.NavigateTo(route);
            }
            //catch (NavigationException ex)
            //{
            //    // 🔑 CAPTURA LA EXCEPCIÓN DE NAVEGACIÓN Y NO HAGAS NADA MÁS.
            //    // Blazor lanza esta excepción para detener la renderización en curso e iniciar la nueva página.
            //    // Si no la capturamos, detiene el flujo de la aplicación.
            //}
            catch (Exception ex)
            {
                // Opcional: Loguear cualquier otra excepción grave
                Console.WriteLine($"Error de navegación inesperado: {ex.Message}");
                throw; // Relanzar otras excepciones si es necesario.
            }
        }

        public async Task NavigateToAsync(string route)
        {
            // Asegúrate de que el método asíncrono también use el método sync con el manejo de excepciones.
            await NavigateTo(route);
            //return Task.CompletedTask;
        }

        public void NavigateToNativePage(string route)
        {
            
        }
    }
}
