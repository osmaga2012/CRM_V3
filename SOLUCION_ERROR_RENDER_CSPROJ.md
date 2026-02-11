# Solución: Error al desplegar en Render.com

## 🔍 Errores encontrados

### Error 1: Archivo no encontrado
```
Could not find a part of the path '/opt/render/project/CRM/CRM.Dtos/CRM.Dtos.csproj'
```

**Causa**: La ruta de referencia al proyecto `CRM.Dtos` estaba usando barras diagonales (/) en lugar de barras invertidas (\) para Windows, y la resolución de ruta relativa no funcionaba correctamente en Linux.

### Error 2: BOM en build.sh
```
build.sh: line 1: ﻿#!/bin/bash: No such file or directory
```

**Causa**: El archivo `build.sh` tenía un BOM (Byte Order Mark) al inicio, que causaba que bash no pudiera interpretar el shebang correctamente.

## ✅ Soluciones aplicadas

### 1. Corregir ruta de CRM.Dtos

**Archivo modificado**: `CRM.V3\CRM.V3.Shared\CRM.V3.Shared.csproj`

**Antes**:
```xml
<ProjectReference Include="../../CRM_REPO/CRM.Dtos/CRM.Dtos.csproj"
```

**Después**:
```xml
<ProjectReference Include="..\..\CRM_REPO\CRM.Dtos\CRM.Dtos.csproj"
```

**Explicación**: Cambié las barras diagonales (/) por barras invertidas (\) para que la ruta relativa se resuelva correctamente tanto en Windows como en Linux.

### 2. Recrear build.sh sin BOM

**Archivo recreado**: `build.sh`

- Eliminé el BOM del inicio del archivo
- Usé codificación ASCII sin BOM
- El shebang `#!/bin/bash` ahora se interpreta correctamente

### 3. Configurar .gitattributes

**Archivo modificado**: `.gitattributes`

**Agregado**:
```
# Shell scripts must always use LF (Linux/Mac line endings)
*.sh text eol=lf
```

**Explicación**: Esto asegura que `build.sh` siempre use LF (line endings de Unix) en lugar de CRLF (Windows), incluso cuando se trabaja en Windows.

## 📂 Estructura del repositorio

```
CRM.V3/
├── CRM_REPO/
│   └── CRM.Dtos/
│       └── CRM.Dtos.csproj
├── CRM.V3/
│   ├── CRM.V3.Shared/
│   │   └── CRM.V3.Shared.csproj (referencia a ../../CRM_REPO/CRM.Dtos/)
│   ├── CRM.V3.Web/
│   └── CRM.V3.Web.Client/
├── build.sh (sin BOM, con LF)
├── _redirects
├── render.yaml
└── .gitattributes
```

## 🚀 Próximos pasos

### 1. Verificar que el despliegue funcione

Render.com debería:
1. ✅ Clonar el repositorio
2. ✅ Ejecutar `bash build.sh`
3. ✅ Encontrar `CRM_REPO/CRM.Dtos/CRM.Dtos.csproj`
4. ✅ Restaurar dependencias
5. ✅ Compilar Blazor WebAssembly
6. ✅ Publicar en `publish/wwwroot`

### 2. Monitorear logs de Render.com

Busca estos mensajes de éxito:
```
Installing .NET SDK 10...
✓ dotnet-install: .NET Core SDK with version '10.0.103' is already installed.
Verifying .NET installation...
✓ 10.0.103
Restoring workloads...
✓ Successfully updated workload(s)
Restoring NuGet packages...
✓ Restore succeeded
Publishing Blazor WebAssembly...
✓ Build succeeded
Build completed successfully!
```

### 3. Si aún hay errores

**Error de referencia de proyecto**:
- Verifica que `CRM_REPO/CRM.Dtos` esté en el repositorio
- Ejecuta: `git ls-files CRM_REPO` para verificar

**Error de workload**:
- El script `build.sh` instala automáticamente `wasm-tools`
- Verifica los logs para ver si la instalación fue exitosa

**Error de line endings**:
- Ejecuta: `git add --renormalize .`
- Ejecuta: `git commit -m "Normalize line endings"`
- Ejecuta: `git push`

## 📊 Verificación local

Para verificar que todo funciona localmente antes de desplegar:

```bash
# En Windows (PowerShell)
dotnet restore CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj
dotnet publish CRM.V3/CRM.V3.Web/CRM.V3.Web.csproj -c Release -o publish

# Verificar que los archivos estén en publish/wwwroot
dir publish/wwwroot/_framework/*.wasm
```

## ✅ Commits realizados

1. **Commit 1**: `Fix: Corregir ruta de CRM.Dtos para Render.com y eliminar BOM de build.sh`
   - Corrigió la ruta de referencia
   - Recreó build.sh sin BOM

2. **Commit 2**: `Add .gitattributes to ensure build.sh uses LF line endings`
   - Configuró .gitattributes
   - Normalizó build.sh para usar LF

3. **Push**: Los cambios fueron enviados a GitHub exitosamente

## 🎉 Estado actual

✅ Código compilando localmente
✅ Referencias de proyecto corregidas
✅ build.sh sin BOM y con LF
✅ .gitattributes configurado
✅ Cambios pusheados a GitHub
⏳ Esperando despliegue en Render.com

## 📞 Próxima acción

Ve a tu dashboard de Render.com y:
1. Verifica que el build se esté ejecutando automáticamente
2. Monitorea los logs en tiempo real
3. Si hay errores, copia el mensaje completo para diagnosticar

El despliegue debería completarse exitosamente en aproximadamente 5-8 minutos.
