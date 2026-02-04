
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace CRM.V3.Shared.Helpers
{
    public class AuthorizedHttpClientHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        // Inyectamos solo IServiceProvider para resolver servicios bajo demanda
        public AuthorizedHttpClientHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Resolver servicios bajo demanda para evitar dependencias de tiempo de construcción
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var storageService = _serviceProvider.GetRequiredService<ISecureStorageService>();

            // Obtener token
            var accessToken = await storageService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Limpiar sesión y notificar logout
                try
                {
                    await authService.LogoutAsync();
                }
                catch
                {
                    // No bloquear por errores en LogoutAsync
                }

                // Intentar navegar usando el servicio de plataforma (MAUI-safe) si existe
                try
                {
                    var platformNav = _serviceProvider.GetService<IPlatformNavigationService>();
                    if (platformNav != null)
                    {
                        await platformNav.NavigateToAsync("login");
                        return response;
                    }

                    // Fallback: intentar NavigationManager si está disponible (capturando errores)
                    var nav = _serviceProvider.GetService<NavigationManager>();
                    if (nav != null)
                    {
                        try
                        {
                            nav.NavigateTo("login");
                        }
                        catch
                        {
                            // Ignorar si NavigationManager no está inicializado (p. ej. WebView no listo)
                        }
                    }
                }
                catch
                {
                    // Evitar propagar excepciones de navegación desde el handler
                }

                // Retornamos la respuesta para que el llamador vea el 401 si es necesario
                return response;
            }

            return response;
        }
    }
}
