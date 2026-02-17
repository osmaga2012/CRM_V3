



using Blazored.LocalStorage;
using CRM.V3.Shared.Interfaces;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace CRM.App.Web.Client.Services
{
    public class WebStorageService : ISecureStorageService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<WebStorageService> _logger;

        public WebStorageService(ILocalStorageService localStorage, IJSRuntime jsRuntime, ILogger<WebStorageService> logger)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task SaveTokenAsync(string token)
        {
            try
            {
                await _localStorage.SetItemAsync("auth_token", token);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                Console.WriteLine($"[WebStorageService] JavaScript no disponible aún al guardar token: {ex.Message}");
                // El token se guardará cuando el usuario haga login manualmente
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebStorageService] Error guardando token: {ex.Message}");
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                // Verificar si JavaScript está disponible
                if (!await IsJavaScriptAvailableAsync())
                {
                    Console.WriteLine("[WebStorageService] JavaScript no disponible aún - retornando null");
                    return null;
                }

                var token = await _localStorage.GetItemAsync<string>("auth_token");
                return token;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                Console.WriteLine($"[WebStorageService] JavaScript no disponible al leer token: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebStorageService] Error leyendo token: {ex.Message}");
                return null;
            }
        }

        public void RemoveTokenAsync()
        {
            try
            {
                _ = _localStorage.RemoveItemAsync("auth_token");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebStorageService] Error eliminando token: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si JavaScript está disponible para realizar operaciones de interop
        /// </summary>
        private async Task<bool> IsJavaScriptAvailableAsync()
        {
            try
            {
                // Intenta una operación JavaScript simple para verificar disponibilidad
                await _jsRuntime.InvokeVoidAsync("eval", "void(0)");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
