#!/bin/bash
set -e

echo "=========================================="
echo "Building Blazor WebAssembly Static Site"
echo "=========================================="

# 1. Instalar .NET SDK 10
echo "Installing .NET SDK 10..."
INSTALL_DIR="/opt/render/project/dotnet"
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 10.0.103 --install-dir "$INSTALL_DIR"

# 2. Configuración de rutas
export DOTNET_ROOT="$INSTALL_DIR"
export PATH="$PATH:$INSTALL_DIR:$INSTALL_DIR/tools"

echo "Verifying .NET installation..."
"$INSTALL_DIR/dotnet" --version

# 3. Configuración de NuGet para DESCARGA de paquetes
echo "Configuring NuGet for Package Download..."
LOCAL_CONFIG=$(find . -name "nuget.config" | head -n 1)

if [ -z "$LOCAL_CONFIG" ]; then
    LOCAL_CONFIG="./nuget.config"
fi

# Aquí inyectamos tu token de descarga
if [ ! -z "$API_GITHUB_PACKAGE" ]; then
cat <<EOF > "$LOCAL_CONFIG"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/osmaga2012/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="osmaga2012" />
      <add key="ClearTextPassword" value="$API_GITHUB_PACKAGE" />
    </github>
  </packageSourceCredentials>
</configuration>
EOF
    echo "NuGet config updated with API_GITHUB_PACKAGE."
else
    echo "ERROR: API_TOKEN_PACKAGE not found in environment variables."
    exit 1
fi

# 4. Herramientas de compilación
echo "Installing WASM Workloads..."
"$INSTALL_DIR/dotnet" workload install wasm-tools --no-cache

# 5. Restaurar (Usando el token) y Publicar
PROJECT_PATH="CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj"

echo "Restoring NuGet packages..."
"$INSTALL_DIR/dotnet" restore "$PROJECT_PATH" --configfile "$LOCAL_CONFIG"

echo "Publishing Blazor WebAssembly..."
"$INSTALL_DIR/dotnet" publish "$PROJECT_PATH" -c Release -o publish /p:BlazorEnableCompression=false --no-restore

# 6. Finalización
if [ -f "_redirects" ]; then
    cp _redirects publish/wwwroot/_redirects
fi

echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="
