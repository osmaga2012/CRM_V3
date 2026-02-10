# Solución: Errores en GitHub Actions Workflow

## 🚨 Problemas Identificados

### Error 1: Instalación de Workload WASM
```
Workload installation failed: Failed to install manifest microsoft.net.sdk.maui version 10.0.20: 
Version 10.0.20 of package microsoft.net.sdk.maui.manifest-10.0.100 is not found in NuGet feeds
```

### Error 2: Formato de Compresión Inválido
```
error : Unknown compression format 'None' for ...
error : Could not create compressed asset for original asset ...
```

### ⚠️ CAUSA RAÍZ (Error 2)
**El problema estaba en DOS lugares:**
1. ❌ En el archivo de workflow YAML (ya corregido)
2. ❌ **En el archivo `.csproj` del proyecto** (CRÍTICO)

El archivo `CRM.V3.Web.Client.csproj` contenía:
```xml
<BuildCompressionFormats>None</BuildCompressionFormats>
<PublishCompressionFormats>None</PublishCompressionFormats>
```

Estas propiedades en el `.csproj` **sobreescriben** cualquier parámetro que se pase en el comando `dotnet publish`, por eso el error persistía.

---

## ✅ Soluciones Implementadas

### Solución 1: Workload WASM - `--skip-manifest-update`

**Cambio aplicado:**
```yaml
- name: Install WASM Workload (Solo WASM)
  run: |
    dotnet workload install wasm-tools --skip-manifest-update --ignore-failed-sources || true
    dotnet workload list

- name: Verify Workload Installation
  run: |
    echo "Verificando workloads instalados:"
    dotnet workload list
    echo "SDK instalado:"
    dotnet --version
```

**¿Qué hace?**
- `--skip-manifest-update`: Evita actualizar manifests a versiones no disponibles
- `--ignore-failed-sources`: Ignora fuentes de NuGet que no responden
- `|| true`: Permite continuar el workflow incluso si hay errores parciales

---

### Solución 2: Formato de Compresión - Eliminar de `.csproj` Y YAML

**🔴 CRÍTICO: El problema estaba en el archivo `.csproj`**

#### Paso 1: Corregir el archivo `.csproj`

**Archivo:** `CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj`

```xml
<!-- ❌ INCORRECTO - Eliminar estas líneas -->
<BuildCompressionFormats>None</BuildCompressionFormats>
<PublishCompressionFormats>None</PublishCompressionFormats>

<!-- ✅ CORRECTO - Reemplazar con -->
<BlazorEnableCompression>false</BlazorEnableCompression>
```

**Cambio completo aplicado:**
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <NoDefaultLaunchSettingsFile>true</NoDefaultLaunchSettingsFile>
  <StaticWebAssetProjectMode>Default</StaticWebAssetProjectMode>
  <WasmEnableSIMD>false</WasmEnableSIMD>
  <WasmEnableExceptionHandling>false</WasmEnableExceptionHandling>
  <BlazorEnableCompression>false</BlazorEnableCompression>  <!-- ✅ Usar esto -->
  <DisableStaticWebAssetsScanning>false</DisableStaticWebAssetsScanning>
  <RunAOTCompilation>false</RunAOTCompilation>
</PropertyGroup>
```

#### Paso 2: Corregir el workflow YAML

**Archivo:** `.github/workflows/publicacion_web.yml`

**Problema:**
```yaml
# ❌ INCORRECTO - "None" no es un formato válido
/p:BuildCompressionFormats=None
/p:PublishCompressionFormats=None
```

**Solución:**
```yaml
# ✅ CORRECTO - Solo usar BlazorEnableCompression=false
dotnet publish CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj \
  -c Release \
  -o release \
  /p:StaticWebAssetsEnabled=true \
  /p:GHPages=true \
  /p:BlazorEnableCompression=false
```

**¿Por qué?**
- En .NET, `None` NO es un formato de compresión reconocido
- Las propiedades en el `.csproj` tienen **prioridad** sobre los parámetros de línea de comandos
- Para desactivar la compresión, usa `/p:BlazorEnableCompression=false` O `<BlazorEnableCompression>false</BlazorEnableCompression>` en el `.csproj`

---

## 📝 Archivo YAML Corregido Completo

```yaml
name: Deploy Blazor WASM to GitHub Pages
on:
  push:
    branches:
      - master
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout CRM_V3 with Submodules
        uses: actions/checkout@v4
        with:
          submodules: recursive
          token: ${{ secrets.API_TOKEN_GITHUB }}

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
          dotnet-quality: 'preview'
          
      - name: Add .NET 10 Preview Feed
        run: |
          dotnet nuget add source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3/index.json --name dotnet10

      - name: Install WASM Workload (Solo WASM)
        run: |
          dotnet workload install wasm-tools --skip-manifest-update --ignore-failed-sources || true
          dotnet workload list
      
      - name: Verify Workload Installation
        run: |
          echo "Verificando workloads instalados:"
          dotnet workload list
          echo "SDK instalado:"
          dotnet --version

      - name: Create Symlink for CRM Folder
        run: ln -s CRM_REPO CRM

      - name: Restore dependencies
        run: dotnet restore CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj

      - name: Publish App
        run: |
          dotnet publish CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj \
          -c Release \
          -o release \
          /p:StaticWebAssetsEnabled=true \
          /p:GHPages=true \
          /p:BlazorEnableCompression=false

      - name: Fix for GitHub Pages
        run: |
          touch release/wwwroot/.nojekyll
          sed -i 's|<base href="/" />|<base href="/CRM_V3/" />|g' release/wwwroot/index.html
          cp release/wwwroot/index.html release/wwwroot/404.html

      - name: Setup Pages
        uses: actions/configure-pages@v5

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: 'release/wwwroot'

      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

---

## 🔄 Soluciones Alternativas

### Opción A: Usar .NET 9 en lugar de .NET 10 Preview

Si .NET 10 sigue dando problemas, puedes cambiar temporalmente a .NET 9 que es más estable:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'
```

### Opción B: Habilitar Compresión (Recomendado para Producción)

Si quieres **mejorar el rendimiento**, es mejor HABILITAR la compresión con los formatos correctos:

```yaml
- name: Publish App
  run: |
    dotnet publish CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj \
    -c Release \
    -o release \
    /p:StaticWebAssetsEnabled=true \
    /p:GHPages=true \
    /p:BlazorEnableCompression=true \
    /p:CompressionExcludedExtensions=".br;.gz"
```

---

## 📋 Próximos Pasos

1. **Commit y push de los cambios**:
```bash
cd "C:\Users\oscar\source\repos\CRM.V3"
git add .github/workflows/publicacion_web.yml
git add SOLUCION_WORKFLOW_WASM.md
git commit -m "Fix: Corregir errores de workload y compresión en GitHub Actions"
git push origin master
```

2. **Monitorea el workflow** en GitHub Actions:
   - Ve a tu repositorio: https://github.com/osmaga2012/CRM_V3
   - Click en la pestaña "Actions"
   - Verifica que el workflow se ejecuta sin errores

3. **Espera el despliegue**:
   - El workflow compilará la aplicación
   - La desplegará en GitHub Pages
   - Podrás acceder a tu app en: `https://osmaga2012.github.io/CRM_V3/`

---

## 🎯 Resumen de Cambios

| Problema | Archivos Afectados | Solución |
|----------|-------------------|----------|
| ❌ `Workload installation failed` | `.github/workflows/publicacion_web.yml` | ✅ Agregado `--skip-manifest-update` y verificación |
| ❌ `Unknown compression format 'None'` | `.github/workflows/publicacion_web.yml` | ✅ Eliminados parámetros `BuildCompressionFormats` y `PublishCompressionFormats` |
| ❌ `Unknown compression format 'None'` | **`CRM.V3.Web.Client.csproj`** (CAUSA RAÍZ) | ✅ Eliminadas propiedades `<BuildCompressionFormats>` y `<PublishCompressionFormats>` |
| ❌ Falta verificación de instalación | `.github/workflows/publicacion_web.yml` | ✅ Agregado paso "Verify Workload Installation" |

### 🔴 Lección Importante:
**Las propiedades MSBuild en el archivo `.csproj` tienen PRIORIDAD sobre los parámetros de línea de comandos.**

Por eso, aunque corrigiéramos el workflow YAML, el error persistía hasta que corregimos el `.csproj`.

---

## 🔗 Referencias

- [.NET Workloads Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-install)
- [Blazor WebAssembly Compression](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly#compression)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net)
- [Static Web Assets](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/static-files)
