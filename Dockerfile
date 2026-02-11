# 1. SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Instalar cargas de trabajo necesarias para Blazor WebAssembly
RUN dotnet workload install wasm-tools --skip-manifest-update

# Copiamos los proyectos para restaurar dependencias
COPY ["CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj", "CRM.V3/CRM.V3.Web/"]
COPY ["CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj", "CRM.V3/CRM.V3.Web.Client/"]
COPY ["CRM.V3/CRM.V3.Shared/CRM.V3.Shared.csproj", "CRM.V3/CRM.V3.Shared/"]

# OJO: Si tus DTOs están en otro repo, Render NO podrá verlos mediante Docker 
# a menos que los copies dentro de este repo primero.
# Supongamos que los has movido a una carpeta llamada 'CRM_REPO' en la raíz:
COPY ["CRM_REPO/CRM.Dtos/CRM.Dtos.csproj", "CRM_REPO/CRM.Dtos/"]

RUN dotnet restore "CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj"

# Copiamos el resto del código y publicamos
COPY . .
WORKDIR "/src/CRM.V3/CRM.V3.Web"
RUN dotnet publish "CRM.V3.Web.csproj" -c Release -o /app/publish

# 2. Runtime para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copiar script de inicio
COPY start.sh /app/start.sh
RUN chmod +x /app/start.sh

# Configuración para Render.com
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/start.sh"]
