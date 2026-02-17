using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.V3.Shared.Interfaces;
using CRM.V3.Shared.Services;
using System.IdentityModel.Tokens.Jwt;

namespace CRM.V3.Shared.Providers
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        //private readonly ILocalStorageService _localStorage;
        private readonly ISecureStorageService _localStorage;
        private readonly NavigationManager _navigationManager;
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly ILogger<CustomAuthStateProvider> _logger;
        private readonly AuthenticationState _anonymous;
        private readonly Timer _tokenCheckTimer;

        // JSON options with converters to handle string-to-number conversions
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = 
            { 
                new StringToLongConverter(),
                new StringToIntConverter(),
                new StringToDecimalConverter()
            }
        };

        private static readonly string[] _requiredClaims = new[]
        {
            ClaimTypes.NameIdentifier,
            //ClaimTypes.Email,
            ClaimTypes.Name
        };

        private bool HasRequiredClaims(IEnumerable<Claim> claims)
        {
            return _requiredClaims.All(t => claims.Any(c => c.Type == t));
        }

        public CustomAuthStateProvider(
            //ILocalStorageService localStorage,
            ISecureStorageService localStorage,
            NavigationManager navigationManager,
            IHttpClientFactory httpClient, // Recibe HttpClient en el constructor
            IConfiguration configuration,
            ILogger<CustomAuthStateProvider> logger)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
            this.httpClient = httpClient.CreateClient("NoAuthClient");
            this.configuration = configuration;
            _logger = logger;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            // Verificar el token cada 5 minutos
            //_tokenCheckTimer = new Timer(async _ => await CheckTokenValidityAsync(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        public async Task<AuthenticationState> GetAuthenticationStateAsync1()
        {
            try
            {
                string tokenInfo = "";/*await _localStorage.GetItemAsync<string>("access_token");*/

                if (tokenInfo is null)
                    return _anonymous;

                var identity = BuildClaims(tokenInfo);
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthStateProvider] Error recuperando el token");
                return _anonymous;
            }
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                //var tokenInfo = await _localStorage.GetItemAsync<string>("access_token");
                var tokenInfo = await _localStorage.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(tokenInfo))
                {
                    return _anonymous;
                }


                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(tokenInfo);

                if (token.ValidTo < DateTime.UtcNow)
                {
                    //await _localStorage.RemoveItemAsync("access_token");
                    return _anonymous;
                }

                var tokenClaims = token.Claims.ToList();

                var userId = tokenClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
                var email = tokenClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
                var name = tokenClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value;
                var roles = tokenClaims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value).ToList();

                var claims = new List<Claim>();
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                }
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email));
                }
                if (!string.IsNullOrWhiteSpace(name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, name));
                }
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                // Reconstruir los claims únicamente a partir del JWT. Si se necesita información
                // adicional del usuario, otros servicios deben obtenerla después del renderizado.
                //if (!HasRequiredClaims(claims))
                //{
                //    await _localStorage.RemoveItemAsync("access_token");
                //    return _anonymous;
                //}

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CustomAuthStateProvider] Error procesando JWT");
                //_localStorage.RemoveTokenAsync(); //.RemoveItemAsync("access_token");
                //return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                return _anonymous;
            }
        }

        //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        //{
        //    var savedToken = await _localStorage.GetItemAsync<string>("authToken");

        //    if (string.IsNullOrWhiteSpace(savedToken))
        //    {
        //        // No hay token guardado, usuario no autenticado.
        //        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //    }

        //    // --- INICIO DE LA LÓGICA DE RECONSTRUCCIÓN DE CLAIMS ---
        //    var claims = new List<Claim>();
        //    try
        //    {
        //        var handler = new JwtSecurityTokenHandler();
        //        var jwtToken = handler.ReadJwtToken(savedToken);

        //        // Verificar si el token ha expirado. Si sí, deberías refrescarlo o forzar el login.
        //        if (jwtToken.ValidTo < DateTime.UtcNow)
        //        {
        //            // Token expirado. Podrías intentar un refresh token aquí o simplemente limpiar y forzar login.
        //            await _localStorage.RemoveItemAsync("authToken");
        //            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //        }

        //        // Añadir los claims del JWT al ClaimsPrincipal
        //        foreach (var claim in jwtToken.Claims)
        //        {
        //            claims.Add(claim);
        //        }

        //        // También puedes añadir claims adicionales si necesitas consolidar info del perfil
        //        // que NO ESTÁ en el JWT pero sí en tu API de perfil (menos común, mejor JWT completo)
        //        // Ejemplo:
        //        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub"); // "sub" es el claim estándar para el ID de usuario
        //        var userId = userIdClaim?.Value;
        //        //var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //        if (!string.IsNullOrEmpty(userId))
        //        {
        //            // Puedes hacer una llamada a tu API de perfil aquí para obtener datos adicionales
        //            // si esos datos no están en el JWT y los necesitas en los claims.
        //            // Pero idealmente, todos los claims necesarios deberían estar en el JWT.
        //            // 2. Llamar a tu API de backend para obtener el perfil del usuario, incluyendo TipoUsuario.
        //            // ¡IMPORTANTE! Asegúrate de que esta URL esté configurada en tu appsettings.json.
        //            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken); // Añadir el token JWT para autenticar la llamada
        //            var profileEndpoint = "/api/Usuarios/perfil"; // Obtiene el endpoint desde la configuración

        //            if (string.IsNullOrEmpty(profileEndpoint))
        //            {
        //                Console.WriteLine("El endpoint del perfil no está configurado en appsettings.json (ApiSettings:ProfileEndpoint).");
        //                NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        //                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //            }

        //            // Asegúrate de que UserProfileDto o un DTO similar incluya TipoUsuario.
        //            // Aquí, he usado UserLoginResponseDto por simplicidad, pero lo ideal es un DTO de Perfil.
        //            var profileResponse = await httpClient.GetFromJsonAsync<Usuario2Dto>(profileEndpoint);

        //            if (profileResponse != null)
        //            {
        //                // 3. Construir los Claims, añadiendo TipoUsuario como un claim personalizado.
        //                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId)); // Usar el ID de usuario extraído
        //                claims.Add(new Claim(ClaimTypes.Email, profileResponse.EMail)); // Ejemplo: si el email viene en el perfil

        //                // Si tu backend también devuelve roles, añádelos aquí:
        //                // foreach (var role in profileResponse.Roles) { claims.Add(new Claim(ClaimTypes.Role, role)); }
        //                claims.Add(new Claim("tipo_usuario", profileResponse.TipoUsuario.ToString())); // ¡Claim personalizado!
        //                                                                                               //return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //            }
        //        }

        //        var identity = new ClaimsIdentity(claims, "jwt");
        //        var user = new ClaimsPrincipal(identity);

        //        // Configurar el token de autorización en el HttpClient
        //        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

        //        return new AuthenticationState(new ClaimsPrincipal(user));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Si el token no es válido (corrupto, no JWT, etc.), loguear y tratar como no autenticado.
        //        Console.WriteLine($"Error al procesar el JWT del localStorage: {ex.Message}");
        //        await _localStorage.RemoveItemAsync("authToken");
        //        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        //    }
        //    //---FIN DE LA LÓGICA DE RECONSTRUCCIÓN DE CLAIMS-- -


        //    ////Console.WriteLine(_localStorage.GetItemAsync<LoginResponse>("authToken").Result);
        //    //try
        //    //{
        //    //    string tokenInfo = await _localStorage.GetItemAsync<string>("authToken");

        //    //    if (tokenInfo is null)
        //    //        return _anonymous;

        //    //    var identity = BuildClaims(tokenInfo);
        //    //    return new AuthenticationState(new ClaimsPrincipal(identity));
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Console.WriteLine($"[AuthStateProvider] Error recuperando el token: {ex.Message}");
        //    //    return _anonymous;
        //    //}

        //}

        public async Task NotifyUserAuthentication(string token)
        {
            try
            {
                // 1. Decodificar el JWT para obtener información básica del usuario (ej. su ID).
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub"); // "sub" es el claim estándar para el ID de usuario
                var userIdClaim2 = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);


                var userId = userIdClaim2?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No se pudo extraer el ID de usuario del JWT");
                    NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
                    return;
                }

                // 2. Llamar a tu API de backend para obtener el perfil del usuario, incluyendo TipoUsuario.
                // ¡IMPORTANTE! Asegúrate de que esta URL esté configurada en tu appsettings.json.
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Añadir el token JWT para autenticar la llamada
                var profileEndpoint = "/api/Usuarios/perfil"; // Obtiene el endpoint desde la configuración

                if (string.IsNullOrEmpty(profileEndpoint))
                {
                    _logger.LogWarning("El endpoint del perfil no está configurado en appsettings.json (ApiSettings:ProfileEndpoint)");
                    NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
                    return;
                }

                // Asegúrate de que UserProfileDto o un DTO similar incluya TipoUsuario.
                // Aquí, he usado UserLoginResponseDto por simplicidad, pero lo ideal es un DTO de Perfil.
                var response = await httpClient.GetAsync(profileEndpoint);
                response.EnsureSuccessStatusCode();
                var profileResponse = await response.Content.ReadFromJsonAsync<CRM.Dtos.UsuarioDto>(_jsonOptions);

                //var profileResponse = await httpClient.GetAsync(profileEndpoint);

                if (profileResponse != null)
                {
                    // 3. Construir los Claims, añadiendo TipoUsuario como un claim personalizado.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId), // Usar el ID de usuario extraído
                        new Claim(ClaimTypes.Email, string.IsNullOrEmpty(profileResponse.EMail) ? "": profileResponse.EMail  ), // Ejemplo: si el email viene en el perfil

                        // Si tu backend también devuelve roles, añádelos aquí:
                        // foreach (var role in profileResponse.Roles) { claims.Add(new Claim(ClaimTypes.Role, role)); }
                    };
                    
                    // Add role as custom claim if available
                    if (!string.IsNullOrEmpty(profileResponse.Rol))
                    {
                        claims.Add(new Claim("tipo_usuario", profileResponse.Rol));
                    }

                    var identity = new ClaimsIdentity(claims, "jwt");
                    var user = new ClaimsPrincipal(identity);

                    //await _localStorage.SetItemAsync("authToken", token); // Guardar el token en localStorage
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                    //_navigationManager.NavigateTo("/", forceLoad: false); // Redirigir a la página de inicio o dashboard
                }
                else
                {
                    _logger.LogWarning("Falló la obtención del perfil de usuario desde la API");
                    NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Error de red o error de HTTP del servidor (404, 500, etc.)
                _logger.LogError(httpEx, "Error HTTP al obtener el perfil. Código de estado: {StatusCode}", httpEx.StatusCode);
                // Puedes acceder a httpEx.StatusCode si es un error HTTP
                // Si el servidor devuelve un cuerpo de error, podrías intentar leerlo
                // var errorContent = await httpEx.Content.ReadAsStringAsync();
                // Console.WriteLine($"Contenido del error: {errorContent}");
            }
            catch (JsonException jsonEx)
            {
                // Error al deserializar la respuesta JSON
                _logger.LogError(jsonEx, "Error de JSON al obtener el perfil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en NotifyUserAuthentication");
                NotifyAuthenticationStateChanged(Task.FromResult(_anonymous)); // Manejar el error adecuadamente
            }

            //var identity = BuildClaims(token);
            //var user = new ClaimsPrincipal(identity);

            //NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            //_localStorage.RemoveItemAsync("authToken");
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
            //NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
            //_navigationManager.NavigateTo("login", forceLoad: true);
        }

        private ClaimsIdentity BuildClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims;

            return new ClaimsIdentity(claims, "jwt");
        }

        private async Task CheckTokenValidityAsync()
        {
            try
            {
                var tokenInfo = ""; /* await _localStorage.GetItemAsync<LoginResponse>("authToken");*/

                //if (tokenInfo == null || string.IsNullOrWhiteSpace(tokenInfo.access_token))
                //{
                //    NotifyUserLogout();
                //    return;
                //}

                //var jwtHandler = new JwtSecurityTokenHandler();
                //var jwt = jwtHandler.ReadJwtToken(tokenInfo.access_token);

                //if (jwt.ValidTo <= DateTime.UtcNow)
                //{
                //    // Token ha caducado, cerrar sesión
                //    NotifyUserLogout();
                //}
            }
            catch
            {
                NotifyUserLogout();
            }
        }
    }

}
