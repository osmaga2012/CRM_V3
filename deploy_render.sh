#!/usr/bin/env bash

# Salir inmediatamente si un comando falla
set -e

# 1. Instalar el SDK de .NET (Usaremos el canal 9.0 para estabilidad, o 10.0 si prefieres)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --quality preview --install-dir .dotnet

# 2. Configurar el PATH para usar el dotnet recién instalado
export PATH="$PWD/.dotnet:$PATH"
export DOTNET_ROOT="$PWD/.dotnet"

# 2. Forzar actualización de submódulos (por si acaso Render no lo hizo bien)
git submodule update --init --recursive

# 3. Diagnóstico: Esto nos dirá en el log si la carpeta existe y cómo se llama
echo "--- Verificando carpetas presentes ---"
ls -d */

# 3. INSTALAR WORKLOADS (Esto soluciona tu error NETSDK1147)
dotnet workload install wasm-tools

# 4. Publicar el proyecto
# Asegúrate de que la ruta al .csproj sea correcta según tu estructura
dotnet publish "CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj" -c Release -o output
