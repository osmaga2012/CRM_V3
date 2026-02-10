# 🔍 Diagnóstico: OnInitializedAsync no se ejecuta

## ✅ Cambios Aplicados

### 1. **Propiedades de Inyección de Dependencias Corregidas**
```csharp
// ANTES (podían ser null y causar NullReferenceException)
[Inject] private IApiClient<BarcosDto> servicioBarco { get; set; }

// DESPUÉS (con default! para indicar que serán inyectadas)
[Inject] private IApiClient<BarcosDto> servicioBarco { get; set; } = default!;
```

### 2. **Manejo Completo de Excepciones**
```csharp
// ANTES - Solo capturaba OperationCanceledException
catch (OperationCanceledException)
{
    // Manejar cancelación si es necesario
}

// DESPUÉS - Captura TODAS las excepciones y las muestra
catch (Exception ex)
{
    Console.WriteLine($"OnInitializedAsync: Error - {ex.Message}");
    errorMessage = $"Error al cargar datos: {ex.Message}";
    isLoading = false;
    StateHasChanged();
}
```

### 3. **Logging y Debugging**
Se agregaron mensajes de consola para rastrear la ejecución:
```csharp
Console.WriteLine("OnInitializedAsync: Iniciando carga de barcos...");
Console.WriteLine($"OnInitializedAsync: Carga completada. Total empresas: {empresas?.Count ?? 0}");
```

### 4. **StateHasChanged() Agregado**
```csharp
isLoading = false;
StateHasChanged(); // Forzar actualización de UI
```

### 5. **Indicadores Visuales en la UI**
- ✅ **Estado de carga**: Spinner animado
- ✅ **Estado de error**: Mensaje con botón de reintentar
- ✅ **Estado vacío**: Mensaje cuando no hay datos
- ✅ **Estado normal**: Tabla con datos

---

## 🔎 Cómo Verificar que Funciona

### **Opción 1: Consola del Navegador (Web)**
1. Abre tu aplicación Blazor en el navegador
2. Presiona `F12` para abrir DevTools
3. Ve a la pestaña **Console**
4. Busca estos mensajes:
```
OnInitializedAsync: Iniciando carga de barcos...
CargarBarcos: Iniciando llamada a API...
CargarBarcos: Resultado recibido. Items: X
OnInitializedAsync: Carga completada. Total empresas: X
```

### **Opción 2: Breakpoints (Visual Studio)**
1. Abre `ListaBarcos.razor.cs`
2. Coloca un breakpoint en la línea:
   ```csharp
   Console.WriteLine("OnInitializedAsync: Iniciando carga de barcos...");
   ```
3. Ejecuta la aplicación en modo Debug (F5)
4. Navega a la página `/barcos`
5. El debugger debería detenerse en ese punto

### **Opción 3: Verificación Visual**
1. Navega a `/barcos`
2. Deberías ver uno de estos estados:
   - 🔄 **Spinner** si está cargando
   - ❌ **Mensaje de error** si falló la API
   - 📄 **Lista vacía** si no hay datos
   - ✅ **Tabla con barcos** si todo funcionó

---

## 🚨 Posibles Problemas y Soluciones

### **Problema 1: El método NO se ejecuta**
**Síntomas**: No ves ningún mensaje en la consola

**Posibles causas**:
1. ❌ La ruta de navegación no coincide con `@page "/barcos"`
2. ❌ La autenticación está bloqueando el acceso (`@attribute [Authorize]`)
3. ❌ El componente no se está instanciando correctamente

**Solución**:
- Verifica la URL en el navegador: debe ser `https://localhost:XXXX/barcos`
- Asegúrate de estar autenticado
- Intenta quitar temporalmente `@attribute [Authorize]`

---

### **Problema 2: El método se ejecuta pero falla**
**Síntomas**: Ves "OnInitializedAsync: Iniciando..." pero luego un error

**Posibles causas**:
1. ❌ El servicio `servicioEmpresas` no está registrado en DI
2. ❌ La API no está disponible o devuelve un error
3. ❌ Problema de autenticación/autorización en la API

**Solución**:
- Revisa el mensaje de error completo en la consola
- Verifica que el endpoint `api/Empresas` existe y funciona
- Comprueba la configuración de HttpClient

---

### **Problema 3: Se ejecuta pero no muestra datos**
**Síntomas**: No hay error pero la tabla está vacía

**Posibles causas**:
1. ❌ La API devuelve un array vacío
2. ❌ Los datos no están siendo filtrados correctamente
3. ❌ El campo `Barcos` no se está incluyendo correctamente

**Solución**:
```csharp
// Verifica en la consola:
Console.WriteLine($"CargarBarcos: Resultado recibido. Items: {result?.Count() ?? 0}");
Console.WriteLine($"CargarBarcos: Lista empresas asignada. Total: {empresas.Count}");
```

Si ves `Items: 0` → La API no devuelve datos
Si ves `Items: X` pero `Total: 0` → Problema al convertir a List

---

## 🎯 Código de Prueba Adicional

Si aún no funciona, agrega este código temporal al inicio de `OnInitializedAsync`:

```csharp
override protected async Task OnInitializedAsync()
{
    // TEST: Verificar que el método se ejecuta
    await js.InvokeVoidAsync("alert", "OnInitializedAsync se está ejecutando!");
    
    // Resto del código...
}
```

Si ves el alert → El método SÍ se ejecuta, el problema está en otro lado
Si NO ves el alert → El método NO se está ejecutando

---

## 📋 Checklist de Verificación

- [ ] El archivo `.razor` tiene `@page "/barcos"`
- [ ] Estás navegando a la URL correcta
- [ ] Estás autenticado (si hay `[Authorize]`)
- [ ] El servicio `IApiClient<EmpresasDto>` está registrado en DI
- [ ] El endpoint `api/Empresas` existe y funciona
- [ ] Ves mensajes de consola al navegar a la página
- [ ] La UI muestra el spinner de carga inicialmente
- [ ] Los datos se muestran después de cargar

---

## 🔧 Registro de Servicios

Verifica que en tu archivo de configuración (Program.cs o Startup.cs) tengas:

```csharp
// Ejemplo para Blazor WebAssembly
builder.Services.AddScoped(typeof(IApiClient<>), typeof(ApiClient<>));
builder.Services.AddScoped<IApiClient<EmpresasDto>, ApiClient<EmpresasDto>>();
builder.Services.AddScoped<IApiClient<BarcosDto>, ApiClient<BarcosDto>>();
```

---

## 📞 Si Nada Funciona

1. **Crea un componente de prueba simple**:
```razor
@page "/test-init"
@code {
    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("TEST: OnInitializedAsync funciona!");
        await Task.CompletedTask;
    }
}
```

2. **Navega a `/test-init`**
3. Si ves el mensaje en consola → El problema es específico de `ListaBarcos`
4. Si NO ves el mensaje → Hay un problema más profundo con Blazor

---

## ✅ Resultado Esperado

Después de aplicar estos cambios, al navegar a `/barcos` deberías ver:

1. **Spinner de carga** (isLoading = true)
2. **Mensajes en consola**:
   ```
   OnInitializedAsync: Iniciando carga de barcos...
   CargarBarcos: Iniciando llamada a API...
   CargarBarcos: Resultado recibido. Items: 5
   CargarBarcos: Lista empresas asignada. Total: 5
   OnInitializedAsync: Carga completada. Total empresas: 5
   ```
3. **Tabla con datos** (isLoading = false, empresas.Count > 0)

---

## 📝 Notas Adicionales

- Los mensajes `Console.WriteLine` solo aparecen en la **consola del navegador** para Blazor WebAssembly
- Para MAUI, necesitarías usar `Debug.WriteLine` en su lugar
- Los cambios son **seguros para producción** (puedes quitar los Console.WriteLine después)
- El `StateHasChanged()` es **crítico** para actualizar la UI después de operaciones async

