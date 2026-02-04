

using Blazored.LocalStorage;
using CRM.V3.Shared.Interfaces;

namespace CRM.App.Web.Client.Services
{
    public class WebStorageService : ISecureStorageService
    {
        private readonly ILocalStorageService _localStorage;
        public WebStorageService(ILocalStorageService localStorage) => _localStorage = localStorage;

        public async Task SaveTokenAsync(string token) =>
            await _localStorage.SetItemAsync("auth_token", token);

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("auth_token");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico leyendo LocalStorage: {ex.Message}");
                return null;
            }
            
        }

        public void RemoveTokenAsync()
        {
           _ = _localStorage.RemoveItemAsync("auth_token");
        }
    }
}
