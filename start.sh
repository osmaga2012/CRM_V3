#!/bin/bash

# Script de inicio para Render.com
# Este script asegura que la aplicación use el puerto correcto

# Render.com proporciona el puerto a través de la variable $PORT
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
export ASPNETCORE_ENVIRONMENT="Production"

echo "Starting application on port ${PORT}..."

# Ejecutar la aplicación
exec dotnet CRM.V3.Web.dll
