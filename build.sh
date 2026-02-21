#!/bin/bash
set -e

echo "=========================================="
echo "Building Blazor WebAssembly Static Site"
echo "=========================================="

# 1. Instalar .NET SDK 10
echo "Installing .NET SDK 10..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 10.0.103

# 2. CONFIGURACIÓN CRÍTICA DEL PATH
# Usamos rutas absolutas para evitar el "command not found"
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools

# Verificar instalación (con ruta completa por si acaso)
echo "Verifying .NET installation..."
$HOME/.dotnet/dotnet --version

# 3. CONFIGURACIÓN DE NUGET (Lo que arreglamos ayer)
# Render necesita autenticarse igual que GitHub Actions
echo "Configuring NuGet for GitHub Packages..."
CONFIG_PATH=$(find . -name "nuget.config" | head -n 1)

# Si tienes el token en las variables de entorno de Render:
if [ ! -z "$API_TOKEN_GITHUB" ]; then
    cat <<EOF > "$CONFIG_PATH"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/osmaga2012/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="osmaga2012" />
      <add key="ClearTextPassword" value="$API_TOKEN_PACKAGE" />
    </github>
  </packageSourceCredentials>
</configuration>
EOF
    echo "NuGet config updated with API_TOKEN_PACKAGE"
fi

# 4. Restaurar y Publicar
echo "Restoring NuGet packages..."
$HOME/.dotnet/dotnet restore CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj --configfile "$CONFIG_PATH"

echo "Publishing Blazor WebAssembly..."
$HOME/.dotnet/dotnet publish CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj -c Release -o publish /p:BlazorEnableCompression=false

# 5. Post-procesamiento
echo "Copying _redirects file..."
# Asegúrate de que el archivo existe antes de copiarlo para evitar errores
if [ -f "_redirects" ]; then
    cp _redirects publish/wwwroot/_redirects
fi

echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="
