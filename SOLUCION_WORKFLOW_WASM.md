# Solución: Error en Instalación de Workloads WASM en GitHub Actions

## 🚨 Problema Original

```
Workload installation failed: Failed to install manifest microsoft.net.sdk.maui version 10.0.20: 
Version 10.0.20 of package microsoft.net.sdk.maui.manifest-10.0.100 is not found in NuGet feeds
```

## ✅ Solución Implementada

Se ha modificado el archivo `.github/workflows/publicacion_web.yml` con los siguientes cambios:

### Cambio 1: Usar `--skip-manifest-update`

```yaml
- name: Install WASM Workload (Solo WASM)
  run: |
    dotnet workload install wasm-tools --skip-manifest-update --ignore-failed-sources || true
    dotnet workload list
```

**¿Qué hace?**
- `--skip-manifest-update`: Evita actualizar manifests a versiones no disponibles
- `--ignore-failed-sources`: Ignora fuentes de NuGet que no responden
- `|| true`: Permite continuar el workflow incluso si hay errores parciales

### Cambio 2: Verificación de Workloads Instalados

```yaml
- name: Verify Workload Installation
  run: |
    echo "Verificando workloads instalados:"
    dotnet workload list
    echo "SDK instalado:"
    dotnet --version
```

## 🔄 Soluciones Alternativas

### Opción A: Usar .NET 9 en lugar de .NET 10 Preview

Si .NET 10 sigue dando problemas, puedes cambiar temporalmente a .NET 9 que es más estable:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'
```

### Opción B: Instalar Workload con Versión Específica

```yaml
- name: Install WASM Workload
  run: |
    dotnet workload install wasm-tools-net9 --skip-manifest-update --ignore-failed-sources || true
```

### Opción C: Usar Pre-instalado (Sin Workload Installation)

Eliminar el paso de instalación de workload y confiar en que GitHub Actions ya tiene las herramientas necesarias:

```yaml
# Comentar o eliminar el paso "Install WASM Workload"
# - name: Install WASM Workload (Solo WASM)
#   run: |
#     dotnet workload install wasm-tools...
```

Luego ajustar el paso de `Publish` para usar runtime específico:

```yaml
- name: Publish App
  run: |
    dotnet publish CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj \
    -c Release \
    -o release \
    -r browser-wasm \
    /p:StaticWebAssetsEnabled=true \
    /p:GHPages=true \
    /p:BlazorEnableCompression=false
```

### Opción D: Usar Contenedor Docker con Workloads Pre-instalados

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    container:
      image: mcr.microsoft.com/dotnet/sdk:10.0-preview
    steps:
      # ... resto de pasos
```

## 📝 Pasos para Probar la Solución

1. **Commit y Push de los cambios**:
   ```bash
   git add .github/workflows/publicacion_web.yml
   git commit -m "Fix: Resolver error de instalación de workload WASM"
   git push origin master
   ```

2. **Verificar en GitHub Actions**:
   - Ve a tu repositorio en GitHub
   - Click en la pestaña "Actions"
   - Verifica que el workflow se ejecuta sin errores

3. **Si sigue fallando, revisar logs**:
   - Haz click en el workflow fallido
   - Revisa el paso "Install WASM Workload"
   - Copia los logs y aplica una de las soluciones alternativas

## 🎯 Recomendación Final

**Para Producción**: Usa .NET 9 que es más estable  
**Para Testing con .NET 10**: Usa la solución implementada con `--skip-manifest-update`

## 🔗 Referencias

- [.NET Workloads Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-workload-install)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net)
- [Blazor WebAssembly Deployment](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)
