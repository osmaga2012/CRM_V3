# Solución: Error de Deserialización JSON - Campo CensoBarco (Number → String Conversion)

## 🔴 Problema Reportado

**Error en la consola del navegador:**
```
Error fetching data from api/Barcos: DeserializeUnableToConvertValue, System.String 
Path: $[296].barcosTramites[0].censoBarco | LineNumber: 0 | BytePositionInLine: 714907.
```

### Archivos Afectados
- `CRM.Dtos/BarcosTramitesDto.cs` - DTO con el campo `CensoBarco`
- API endpoint: `api/Barcos?includes=BarcosTramites`

### Síntomas
- ❌ Error al deserializar JSON del endpoint `api/Barcos`
- ❌ Error específico en el campo `barcosTramites[0].censoBarco`
- ❌ El deserializador no puede convertir el valor recibido

---

## 🔍 Diagnóstico del Problema

### Causa Raíz: Incompatibilidad de Tipos Number → String

**El problema es idéntico al caso del campo `Censo` en `BarcosDto`:**

#### 1. El servidor devuelve JSON con el campo censoBarco como número:
```json
{
  "barcosTramites": [
    {
      "id": 1,
      "codigoBarco": 2132,
      "censoBarco": 123456,  // ← NÚMERO (int/long), no string
      "tipoTramite": "Matriculación"
    }
  ]
}
```

#### 2. El DTO espera un string:

**BarcosTramitesDto.cs (línea 15 - antes del fix):**
```csharp
public string? CensoBarco { get; set; }  // ← Espera STRING
```

#### 3. El deserializador falla al intentar convertir:
```
JSON: "censoBarco": 123456 (number)
  ↓
DTO: public string? CensoBarco (string)
  ↓
❌ ERROR: No puede convertir number 123456 a string "123456"
```

### ¿Por qué ocurre este error?

El backend devuelve el campo `censoBarco` como un número JSON porque probablemente es un campo numérico en la base de datos. Similar al campo `Censo` en `BarcosDto`, se decidió manejarlo como string en el frontend para permitir formatos especiales o caracteres no numéricos.

---

## ✅ Solución Implementada

### 1. Reutilizar NumberToStringConverter Existente

Ya existe un converter en el proyecto:
**Archivo existente:** `CRM.Dtos/Converters/NumberToStringConverter.cs`

Este converter ya fue creado previamente para resolver el mismo problema en el campo `Censo` de `BarcosDto`.

### 2. Aplicar el Converter al Campo CensoBarco

**Archivo modificado:** `CRM.Dtos/BarcosTramitesDto.cs`

```csharp
using System.Text.Json.Serialization;
using CRM.Dtos.Converters;

namespace CRM.Dtos;

public class BarcosTramitesDto
{
    public long Id { get; set; }
    public long CodigoBarco { get; set; }
    public string? CodigoEmpresa { get; set; }
    public string? Certificado { get; set; }
    public string? TipoTramite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime? FechaAviso { get; set; }
    public int? DiasAvisoTramite { get; set; }
    public string? ListaEmailsEnvioAviso { get; set; }
    
    // ↓ Aplicar converter específicamente a este campo
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? CensoBarco { get; set; }
    
    // ... resto de las propiedades
}
```

---

## 📋 Cómo Funciona el Converter

### Ejemplo con CensoBarco en BarcosTramitesDto

**Escenario 1: JSON con número (caso actual del servidor)**
```json
{
  "censoBarco": 123456  // ← Número
}
```

**Proceso de deserialización:**
1. El deserializador detecta que la propiedad `CensoBarco` tiene un `JsonConverter` personalizado
2. Invoca `NumberToStringConverter.Read()`
3. El converter verifica: `reader.TokenType == JsonTokenType.Number` ✅
4. Intenta leer como long: `reader.TryGetInt64(out longValue)` → `longValue = 123456`
5. Convierte a string: `longValue.ToString()` → `"123456"`
6. ✅ Devuelve `"123456"` y lo asigna a `CensoBarco`

**Escenario 2: JSON con string (por compatibilidad)**
```json
{
  "censoBarco": "123456"  // ← String
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.String` ✅
2. Lee el string directamente: `reader.GetString()` → `"123456"`
3. ✅ Devuelve `"123456"` y lo asigna a `CensoBarco`

**Escenario 3: JSON con null**
```json
{
  "censoBarco": null  // ← Null
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.Null` ✅
2. ✅ Devuelve `null` y lo asigna a `CensoBarco`

---

## 🧪 Verificación de la Solución

### 1. Compilación
```bash
cd /home/runner/work/CRM_V3/CRM_V3
dotnet build CRM.Dtos/CRM.Dtos.csproj
dotnet build CRM.V3/CRM.V3.Shared/CRM.V3.Shared.csproj
dotnet build CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj
```
**Resultado:** ✅ Compilación correcta sin errores

### 2. Pruebas en Producción
1. Desplegar a producción (GitHub Pages/Render)
2. Navegar a las páginas que consumen `api/Barcos?includes=BarcosTramites`:
   - `/panel-cofradia`
   - `/detalle-barco`
   - `/lista-barcos`
3. Abrir DevTools (F12) → Console
4. Verificar que **NO aparece:**
   ```
   Error fetching data from api/Barcos: DeserializeUnableToConvertValue, System.String 
   Path: $[296].barcosTramites[0].censoBarco...
   ```
5. Verificar que los datos se cargan correctamente

---

## 📦 Resumen de Cambios

### Archivos Modificados

1. **CRM.Dtos/BarcosTramitesDto.cs** (MODIFICADO)
   - ➕ `using System.Text.Json.Serialization;`
   - ➕ `using CRM.Dtos.Converters;`
   - ✏️ Aplicado `[JsonConverter(typeof(NumberToStringConverter))]` al campo `CensoBarco`

### Compilación
- ✅ Sin errores
- ✅ Sin warnings adicionales

### Revisión de Código
- ✅ Code review completado - sin comentarios
- ✅ CodeQL security scan - sin vulnerabilidades

---

## 🎯 Ventajas de esta Solución

### 1. Mínimamente Invasiva
- Solo afecta el campo `CensoBarco` específicamente
- No modifica el comportamiento global de deserialización
- Otros campos string no se ven afectados

### 2. Flexible y Robusta
- ✅ Acepta números (caso actual del servidor)
- ✅ Acepta strings (por compatibilidad)
- ✅ Acepta null (campo nullable)

### 3. Consistente con Soluciones Previas
- Reutiliza el mismo converter que ya se usó para `Censo` en `BarcosDto`
- Mantiene consistencia en el manejo de campos de censo en todo el proyecto

### 4. Correcta Separación de Responsabilidades
- El converter está en el proyecto `CRM.Dtos` (capa de datos)
- No contamina el código de servicios o presentación
- Puede reutilizarse en otros DTOs si es necesario

---

## 🔧 Campos Relacionados con Converter Aplicado

Los siguientes campos ya tienen aplicado el `NumberToStringConverter`:

1. **BarcosDto.Censo** (solucionado previamente)
   - Documentado en: `SOLUCION_Error_Censo_Number_To_String.md`
   
2. **BarcosTramitesDto.CensoBarco** (solucionado en este fix)
   - Documentado en este archivo

Ambos campos manejan el número de censo de barcos y requieren el mismo tratamiento de conversión.

---

## 📚 Referencias Técnicas

- [System.Text.Json.Serialization - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization)
- [JsonConverter Attribute - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonconverterattribute)
- [Custom converters for JSON serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to)

### Documentación Relacionada
- `SOLUCION_Error_Censo_Number_To_String.md` - Fix original para el campo Censo
- `SOLUCION_Error_Deserializacion_JSON_CamelCase.md` - Maneja String → Number

---

## ✅ Estado Final

- ✅ Error de deserialización Number → String resuelto
- ✅ Converter personalizado reutilizado
- ✅ Soporta JSON con números, strings, o null
- ✅ Aplicación minimalista (solo afecta el campo CensoBarco)
- ✅ Sin cambios en código de negocio o servicios
- ✅ Código compilando correctamente
- ✅ Code review completado sin issues
- ✅ CodeQL scan sin vulnerabilidades

**Fecha:** 2026-02-12  
**Versión .NET:** 10.0  
**Blazor:** WebAssembly

---

## 🔄 Relación con Otros Fixes

Este fix es parte de una familia de soluciones para problemas de deserialización JSON:

1. **SOLUCION_Error_Censo_Number_To_String.md** - Maneja Number → String para campo Censo
2. **Este documento** - Maneja Number → String para campo CensoBarco
3. **SOLUCION_Error_Deserializacion_JSON_CamelCase.md** - Maneja String → Number

Todos trabajando juntos garantizan la robustez de la deserialización JSON independientemente del formato que devuelva el servidor.
