#!/bin/bash
set -e

echo "=========================================="
echo "Building Blazor WebAssembly Static Site"
echo "=========================================="

# 1. Instalar .NET SDK 10 y capturar la ruta de instalación
echo "Installing .NET SDK 10..."
# El script de instalación suele imprimir dónde se instaló
INSTALL_DIR="/opt/render/project/dotnet"
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 10.0.103 --install-dir "$INSTALL_DIR"

# 2. Configuración de rutas
export DOTNET_ROOT="$INSTALL_DIR"
export PATH="$PATH:$INSTALL_DIR:$INSTALL_DIR/tools"

# Verificar instalación
echo "Verifying .NET installation..."
"$INSTALL_DIR/dotnet" --version

# 3. Configuración de NuGet (Usando la lógica que nos funcionó)
echo "Configuring NuGet for GitHub Packages..."
CONFIG_PATH=$(find . -name "nuget.config" | head -n 1)

if [ -z "$CONFIG_PATH" ]; then
    CONFIG_PATH="./nuget.config"
fi

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
      <add key="ClearTextPassword" value="$API_TOKEN_GITHUB" />
    </github>
  </packageSourceCredentials>
</configuration>
EOF
    echo "NuGet config updated."
fi

# 4. Instalación de Workloads (Necesario para WASM)
echo "Installing WASM Workloads..."
# Instalamos específicamente wasm-tools para evitar que el restore falle por falta de herramientas
"$INSTALL_DIR/dotnet" workload install wasm-tools --no-cache

# 5. Restaurar y Publicar
PROJECT_PATH="CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj"

echo "Restoring NuGet packages..."
"$INSTALL_DIR/dotnet" restore "$PROJECT_PATH" --configfile "$CONFIG_PATH"

echo "Publishing Blazor WebAssembly..."
"$INSTALL_DIR/dotnet" publish "$PROJECT_PATH" -c Release -o publish /p:BlazorEnableCompression=false --no-restore


# 5. Restaurar y Publicar
# IMPORTANTE: Verifica si es CRM.V3.Web o CRM.V3.Web.Client
PROJECT_PATH="CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj"

echo "Restoring NuGet packages..."
"$INSTALL_DIR/dotnet" restore "$PROJECT_PATH" --configfile "$CONFIG_PATH"

echo "Publishing Blazor WebAssembly..."
"$INSTALL_DIR/dotnet" publish "$PROJECT_PATH" -c Release -o publish /p:BlazorEnableCompression=false

# 6. Post-procesamiento
if [ -f "_redirects" ]; then
    cp _redirects publish/wwwroot/_redirects
fi

echo "=========================================="
echo "Build completed successfully!"
echo "=========================================="
