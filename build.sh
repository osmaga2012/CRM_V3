#!/bin/bash
set -e

echo "=========================================="
echo "Building Blazor WebAssembly Static Site"
echo "=========================================="

# Instalar .NET SDK 10
echo "Installing .NET SDK 10..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 10.0.103
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT=$HOME/.dotnet

# Verificar instalaci?n
echo "Verifying .NET installation..."
dotnet --version

# Restaurar cargas de trabajo
echo "Restoring workloads..."
dotnet workload restore --skip-manifest-update

# Restaurar paquetes NuGet
echo "Restoring NuGet packages..."
dotnet restore CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj

# Publicar como est?ticos
echo "Publishing Blazor WebAssembly..."
dotnet publish CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj -c Release -o publish /p:BlazorEnableCompression=false

# Copiar archivo de redirects
echo "Copying _redirects file..."
cp _redirects publish/wwwroot/_redirects

echo "=========================================="
echo "Build completed successfully!"
echo "Static files are in: publish/wwwroot"
echo "=========================================="