# Despliegue de Blazor WebAssembly en Render.com (Static Site)

## 🎯 Configuración para Static Site

Blazor WebAssembly es una aplicación que se ejecuta completamente en el navegador, por lo que se puede desplegar como **archivos estáticos** sin necesidad de un servidor .NET.

## 📋 Pasos para configurar en Render.com

### **1. Crear un nuevo Static Site**

1. Ve a tu dashboard de Render.com
2. Click en **"New +"** → **"Static Site"**
3. Conecta tu repositorio: `https://github.com/osmaga2012/CRM_V3`

### **2. Configuración básica**

```
Name: crm-v3-web
Branch: master
Root Directory: (déjalo vacío)
Build Command: bash build.sh
Publish Directory: publish/wwwroot
```

### **3. Variables de Entorno**

En la sección **"Environment Variables"**, agrega:

| Key | Value |
|-----|-------|
| `GITHUB_ACTIONS` | `true` |

### **4. Configuración avanzada**

**Auto-Deploy**: Yes (para que se despliegue automáticamente con cada push)

**Pull Request Previews**: Yes (opcional, para previews de PRs)

## 🔧 Archivos de configuración

### **render.yaml**
Configuración automática para Render.com. Define:
- Tipo: `static`
- Build command
- Publish path: `publish/wwwroot`
- Headers para archivos WASM
- Rutas SPA (redirige todo a index.html)

### **build.sh**
Script de build que:
1. Instala .NET SDK 10
2. Restaura `wasm-tools` workload
3. Restaura paquetes NuGet
4. Publica la aplicación
5. Copia el archivo `_redirects`

### **_redirects**
Archivo de redirects de Render.com para SPA:
```
/*    /index.html   200
```
Esto asegura que todas las rutas (como `/test`, `/login`) funcionen correctamente.

## 🚀 Proceso de build

Cuando hagas push a GitHub, Render.com:

1. ✅ Clonará tu repositorio
2. ✅ Ejecutará `bash build.sh`
3. ✅ Instalará .NET SDK 10
4. ✅ Instalará `wasm-tools` workload
5. ✅ Compilará tu aplicación Blazor WebAssembly
6. ✅ Publicará archivos estáticos en `publish/wwwroot`
7. ✅ Servirá los archivos con los headers correctos

## ⏱️ Tiempo de build

- **Primer deploy**: 5-8 minutos (descarga de .NET SDK + build)
- **Deploys siguientes**: 3-5 minutos (usa caché)

## 🎨 Ventajas del Static Site

✅ **Más rápido**: No necesita servidor .NET en ejecución
✅ **Más barato**: Plan gratuito de Render.com incluye 100GB de ancho de banda
✅ **Más simple**: Solo archivos estáticos
✅ **CDN automático**: Render.com distribuye en CDN global
✅ **HTTPS gratis**: Certificado SSL automático

## 🔍 Verificación del despliegue

Después del despliegue:

### 1. **Verifica los logs de build**
Busca estos mensajes:
```
Installing .NET SDK 10...
Restoring workloads...
Publishing Blazor WebAssembly...
Build completed successfully!
```

### 2. **Verifica en el navegador**
1. Abre la URL de tu sitio: `https://crm-v3-web.onrender.com`
2. Abre la consola del navegador (F12)
3. Ve a la pestaña **Network**
4. Recarga la página
5. Verifica que los archivos `.wasm` se carguen con status `200`

### 3. **Verifica los headers**
En la pestaña Network, selecciona un archivo `.wasm` y verifica:
```
Content-Type: application/wasm
Status: 200
```

## 🚨 Troubleshooting

### **Error: "Cannot find CRM.Dtos"**
**Solución**: Asegúrate de que `CRM_REPO/CRM.Dtos` esté en tu repositorio:
```bash
mkdir -p CRM_REPO
cp -r ../CRM/CRM.Dtos CRM_REPO/
git add CRM_REPO
git commit -m "Add CRM.Dtos to repository"
git push
```

### **Error: "wasm-tools workload not found"**
**Solución**: El script `build.sh` instala automáticamente esta carga. Verifica los logs.

### **Error 404 en rutas (ej: /test)**
**Solución**: Asegúrate de que el archivo `_redirects` esté en `publish/wwwroot/`:
```bash
cp _redirects publish/wwwroot/_redirects
```

### **Archivos .wasm no cargan**
**Solución**: Verifica que los headers estén configurados en `render.yaml`:
```yaml
headers:
  - path: /_framework/*.wasm
    name: Content-Type
    value: application/wasm
```

### **La aplicación carga pero está en blanco**
**Soluciones**:
1. Verifica la consola del navegador (F12) para errores
2. Verifica que `base href="/"` esté correcto en `index.html`
3. Verifica que la API esté accesible (CORS configurado)

## 🔗 URLs importantes

Después del despliegue:

**Sitio principal**: `https://crm-v3-web.onrender.com`
**Dashboard**: https://dashboard.render.com

## 📦 Estructura de archivos publicados

```
publish/wwwroot/
├── _framework/
│   ├── blazor.webassembly.js
│   ├── dotnet.*.wasm
│   ├── *.dll
│   └── *.dat
├── css/
├── index.html
└── _redirects
```

## 🔄 Actualizar el despliegue

Simplemente haz push a tu rama master:
```bash
git add .
git commit -m "Update application"
git push origin master
```

Render.com detectará el push y comenzará un nuevo despliegue automáticamente.

## ✅ Checklist antes de desplegar

- [ ] ✅ `CRM_REPO/CRM.Dtos` está en el repositorio
- [ ] ✅ Archivos creados: `render.yaml`, `build.sh`, `_redirects`
- [ ] ✅ `build.sh` tiene permisos de ejecución
- [ ] ✅ Hiciste `git push origin master`
- [ ] ✅ Static Site configurado en Render.com
- [ ] ✅ Publish Directory = `publish/wwwroot`

## 🎉 Resultado final

Tu aplicación Blazor WebAssembly estará disponible globalmente a través de CDN, con HTTPS, sin necesidad de servidor .NET, completamente gratis en el plan Free de Render.com.

## 📞 Soporte

Si tienes problemas:
1. Revisa los logs de build en Render.com
2. Verifica la consola del navegador (F12)
3. Verifica que la API esté accesible y tenga CORS configurado
