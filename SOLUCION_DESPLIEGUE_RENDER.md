# Solución: Error de despliegue Blazor WebAssembly en Render.com

## Problema identificado

El error `MONO_WASM: mono_wasm_load_runtime () failed` indica que los archivos WebAssembly no se están generando o cargando correctamente.

## Causas principales

1. **Falta la carga de trabajo `wasm-tools`** en el build de Docker
2. **Dependencias externas** (CRM.Dtos) no están disponibles en el repositorio
3. **Puerto incorrecto** configurado para Render.com

## Soluciones aplicadas

### 1. Actualización del Dockerfile

✅ Agregada la instalación de `wasm-tools`:
```dockerfile
RUN dotnet workload install wasm-tools --skip-manifest-update
```

✅ Actualizada la imagen base de `10.0-preview` a `10.0` (estable)

✅ Script de inicio para manejar dinámicamente el puerto de Render.com

### 2. Archivos creados

- ✅ `start.sh`: Script para configurar el puerto dinámicamente
- ✅ `.dockerignore`: Optimiza el build de Docker
- ✅ `render.yaml`: Configuración alternativa para Render.com

## Pasos para desplegar correctamente

### Paso 1: Asegurar que CRM.Dtos esté en el repositorio

**Opción A: Incluir CRM.Dtos como submódulo**
```bash
cd C:\Users\oscar\source\repos\CRM.V3
git submodule add https://github.com/tu-usuario/CRM CRM_REPO
git commit -m "Agregar CRM como submódulo"
```

**Opción B: Copiar CRM.Dtos al repositorio**
```bash
# Crear carpeta en el repositorio
mkdir CRM_REPO
cp -r C:\Users\oscar\source\repos\CRM\CRM.Dtos CRM_REPO\

# Agregar al repositorio
git add CRM_REPO
git commit -m "Agregar CRM.Dtos al repositorio"
```

### Paso 2: Reiniciar Visual Studio

Después de instalar las cargas de trabajo con `dotnet workload restore`, **debes reiniciar Visual Studio** para que reconozca las cargas instaladas.

### Paso 3: Compilar localmente

```powershell
# Limpiar
dotnet clean

# Restaurar
dotnet restore

# Compilar
dotnet build

# Publicar (para probar)
dotnet publish CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj -c Release -o ./publish
```

### Paso 4: Configurar Render.com

En la configuración de tu servicio en Render.com:

#### 4.1 Build Command
```bash
docker build -t crm-v3 .
```

#### 4.2 Start Command
```bash
docker run -p $PORT:$PORT crm-v3
```

#### 4.3 Variables de entorno
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
GITHUB_ACTIONS=true
```

### Paso 5: Verificar configuración de MIME types

Agrega en `CRM.V3.Web/Program.cs` después de `app.UseStaticFiles()`:

```csharp
// Configuración para archivos WASM
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".json"] = "application/json";
provider.Mappings[".js"] = "application/javascript";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
```

## Problemas comunes y soluciones

### Error: "No se encontró el archivo de metadatos CRM.Dtos.dll"

**Solución**: Asegúrate de que `CRM_REPO/CRM.Dtos` esté en el repositorio antes de hacer push.

### Error: "wasm-tools workload not found"

**Solución**: El Dockerfile actualizado ahora instala automáticamente esta carga.

### Error: "Port binding failed"

**Solución**: Render.com usa `$PORT` como variable de entorno. El script `start.sh` lo maneja automáticamente.

### La aplicación se carga pero muestra pantalla en blanco

**Solución**: Verifica la consola del navegador (F12). Probablemente hay errores de carga de archivos .wasm. Asegúrate de que:
1. Los archivos se publiquen correctamente en `wwwroot/_framework`
2. Los MIME types estén configurados correctamente
3. No haya errores de CORS

## Verificación del despliegue

Después de desplegar, verifica:

1. **Logs de Render.com**: Busca mensajes como "Now listening on: http://[::]:10000"
2. **Consola del navegador** (F12): No debe haber errores de carga de archivos .wasm
3. **Network tab**: Los archivos .wasm deben cargar con status 200

## Próximos pasos

1. ✅ Reinicia Visual Studio
2. ✅ Copia o agrega CRM.Dtos como submódulo
3. ✅ Haz commit y push de los cambios
4. ✅ Configura las variables de entorno en Render.com
5. ✅ Activa el despliegue en Render.com

## Contacto y soporte

Si el problema persiste, proporciona:
- Logs completos de Render.com
- Errores de la consola del navegador (F12)
- Resultado de `dotnet workload list`
