using Blazored.LocalStorage;
using CRM.App.Web.Client.Services;
using CRM.V3.Shared.Helpers;
using CRM.V3.Shared.Interfaces;
using CRM.V3.Shared.Services;
using CRM.V3.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text;
using Microsoft.AspNetCore.Components.Authorization;
using CRM.V3.Shared.Providers; // Añade esta directiva using

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddMediaQueryService();

//await builder.Build().RunAsync();

#if !DEBUG
    builder.RootComponents.Add<Routes>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
#endif

//builder.Services.AddMudServices();
// Add device-specific services used by the CRM.App.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Servicios de autorización y autenticación
//builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
//builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBlazoredLocalStorage(); // Asegúrate de registrar Blazored.LocalStorage

// Asegúrate de tener esta línea, ajustando 'WebNavigationService' al nombre real de tu clase:
// 🔑 AÑADIR ESTA LÍNEA 🔑
builder.Services.AddScoped<IPlatformNavigationService, WebNavigationService>();
builder.Services.AddTransient<AuthorizedHttpClientHandler>();
builder.Services.AddScoped<ISecureStorageService, WebStorageService>();

builder.Services.AddHttpClient();

// 8. Registra tu ApiClient genérico. Usará IHttpClientFactory para obtener el "ApiClient" con autenticación.
builder.Services.AddScoped(typeof(IApiClient<>), typeof(ApiClient<>));

// 1. Obtenemos la URL desde donde se carga la web
var currentBaseAddress = builder.HostEnvironment.BaseAddress;
string baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://crm-api-myhv.onrender.com/";

// Solo si es el emulador, cambiamos la IP
if (currentBaseAddress.Contains("10.0.2.2"))
{
    baseUrl = "https://10.0.2.2:7254/";
}
else if (currentBaseAddress.Contains("localhost"))
{
    baseUrl = "https://localhost:7254/";
}


// 6. Configura el HttpClient principal para tu API con el AuthorizedHttpClientHandler
// Este cliente con nombre "ApiClient" será usado por servicios que necesiten autenticación (ej. AuthService y ApiClient<T>).
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Asegúrate de que esta URL sea correcta y apunte a tu API de backend
    // Lee la BaseUrl desde la configuración (ej. appsettings.json)
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<AuthorizedHttpClientHandler>(); // Añade tu handler autorizado

// 7. Configura un HttpClient adicional *sin* el handler de autorización
//    Este cliente con nombre "NoAuthClient" será usado internamente por AuthService para refrescar tokens
//    sin entrar en un bucle de intentar refrescar el token para la propia solicitud de refresco.
builder.Services.AddHttpClient("NoAuthClient", client =>
{
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<CofradiaState>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped(sp => (CustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

await builder.Build().RunAsync();
