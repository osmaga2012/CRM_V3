using CRM.Dtos.Login;
using System.Net.Http.Json;
using CRM.Dtos.Response;
using CRM.V3.Shared.Interfaces;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using CRM.V3.Shared.Providers;

namespace CRM.V3.Shared.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ISecureStorageService secureStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly HttpClient _httpClient;

        public event Action OnLoginSuccess;

        public string? LastErrorMessage { get; private set; }

        public AuthService(
            //ILocalStorageService localStorage,
            ISecureStorageService secureStorage,
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authStateProvider)
        {
            //_localStorage = localStorage;
            this.secureStorage = secureStorage;
            _authStateProvider = authStateProvider;
            _httpClient = httpClientFactory.CreateClient("NoAuthClient");
        }



        public async Task<LoginResultDto> LoginAsync(string email, string password)
        {
            LastErrorMessage = null;
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { email, password });
                if (!response.IsSuccessStatusCode)
                {
                    var message = "Credenciales incorrectas.";
                    LastErrorMessage = message;
                    return new LoginResultDto { IsSuccess = false, Message = message };
                }

                var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResult is null || string.IsNullOrWhiteSpace(loginResult.access_token))
                {
                    var message = "Respuesta de login inválida o token no recibido.";
                    LastErrorMessage = message;
                    return new LoginResultDto { IsSuccess = false, Message = message };
                }

                //await _localStorage.SetItemAsync("access_token", loginResult.access_token);
                await secureStorage.SaveTokenAsync(loginResult.access_token);
                await ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResult.access_token);
                return new LoginResultDto { IsSuccess = true, access_token = loginResult.access_token };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error en LoginAsync: {ex.Message}");
                var message = "Falla de red. Por favor, intenta nuevamente.";
                LastErrorMessage = message;
                return new LoginResultDto { IsSuccess = false, Message = message };
            }
        }

        public async Task LogoutAsync()
        {
            //await _localStorage.RemoveItemAsync("access_token");
             secureStorage.RemoveTokenAsync();
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            //return await _localStorage.GetItemAsync<string>("access_token");
            return await secureStorage.GetTokenAsync();
        }

        public void LoginOk()
        {
            OnLoginSuccess?.Invoke();
        }
    }
}
