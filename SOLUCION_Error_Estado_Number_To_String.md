# Solución: Error de Deserialización JSON - Campo Estado en BarcosTramites (Number → String Conversion)

## 🔴 Problema Reportado

**Error en la consola del navegador:**
```
blazor.webassembly.js:1 Error fetching data from api/Barcos: DeserializeUnableToConvertValue, System.String 
Path: $[296].barcosTramites[0].estado | LineNumber: 0 | BytePositionInLine: 714954.
```

### Archivos Afectados
- `CRM.Dtos/BarcosTramitesDto.cs` - DTO con el campo `Estado`
- API endpoint: `api/Barcos?includes=BarcosTramites`

### Síntomas
- ❌ Error al deserializar JSON del endpoint `api/Barcos`
- ❌ Error específico en el campo `barcosTramites[0].estado`
- ❌ El deserializador no puede convertir el valor recibido

---

## 🔍 Diagnóstico del Problema

### Causa Raíz: Incompatibilidad de Tipos Number → String

**El problema es idéntico al caso del campo `CensoBarco` en `BarcosTramitesDto`:**

#### 1. El servidor devuelve JSON con el campo estado como número:
```json
{
  "barcosTramites": [
    {
      "id": 1,
      "codigoBarco": 2132,
      "estado": 1,  // ← NÚMERO (int/long/boolean), no string
      "tipoTramite": "Matriculación"
    }
  ]
}
```

#### 2. El DTO espera un string:

**BarcosTramitesDto.cs (línea 23 - antes del fix):**
```csharp
public string? Estado { get; set; }  // ← Espera STRING
```

#### 3. El deserializador falla al intentar convertir:
```
JSON: "estado": 1 (number)
  ↓
DTO: public string? Estado (string)
  ↓
❌ ERROR: No puede convertir number 1 a string "1"
```

### ¿Por qué ocurre este error?

El backend devuelve el campo `estado` como un número JSON porque probablemente:
- Es un campo numérico en la base de datos (bit, tinyint, int)
- Representa un código de estado (0=inactivo, 1=activo, etc.)
- Se decidió manejarlo como string en el frontend para permitir valores descriptivos

Similar a los campos `CensoBarco` y `Censo`, el problema surge por la discrepancia entre el tipo de dato en el servidor (número) y el tipo esperado en el cliente (string).

---

## ✅ Solución Implementada

### 1. Reutilizar NumberToStringConverter Existente

Ya existe un converter en el proyecto:
**Archivo existente:** `CRM.Dtos/Converters/NumberToStringConverter.cs`

Este converter ya fue creado previamente para resolver el mismo problema en los campos:
- `Censo` en `BarcosDto`
- `CensoBarco` en `BarcosTramitesDto`

### 2. Aplicar el Converter al Campo Estado

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
    
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? CensoBarco { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // ↓ Aplicar converter específicamente a este campo
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? Estado { get; set; }
    
    public string? Observaciones { get; set; }
    public string? DocumentoPath { get; set; }
    public bool? Activo { get; set; }
    
    // ... resto de las propiedades
}
```

---

## 📋 Cómo Funciona el Converter

### Ejemplo con Estado en BarcosTramitesDto

**Escenario 1: JSON con número (caso actual del servidor)**
```json
{
  "estado": 1  // ← Número
}
```

**Proceso de deserialización:**
1. El deserializador detecta que la propiedad `Estado` tiene un `JsonConverter` personalizado
2. Invoca `NumberToStringConverter.Read()`
3. El converter verifica: `reader.TokenType == JsonTokenType.Number` ✅
4. Intenta leer como long: `reader.TryGetInt64(out longValue)` → `longValue = 1`
5. Convierte a string: `longValue.ToString()` → `"1"`
6. ✅ Devuelve `"1"` y lo asigna a `Estado`

**Escenario 2: JSON con string (por compatibilidad)**
```json
{
  "estado": "activo"  // ← String
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.String` ✅
2. Lee el string directamente: `reader.GetString()` → `"activo"`
3. ✅ Devuelve `"activo"` y lo asigna a `Estado`

**Escenario 3: JSON con null**
```json
{
  "estado": null  // ← Null
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.Null` ✅
2. ✅ Devuelve `null` y lo asigna a `Estado`

---

## 🧪 Verificación de la Solución

### 1. Compilación
```bash
cd /home/runner/work/CRM_V3/CRM_V3
dotnet build CRM.Dtos/CRM.Dtos.csproj
dotnet build CRM.V3/CRM.V3.Shared/CRM.V3.Shared.csproj
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
   Path: $[296].barcosTramites[0].estado...
   ```
5. Verificar que los datos se cargan correctamente

---

## 📦 Resumen de Cambios

### Archivos Modificados

1. **CRM.Dtos/BarcosTramitesDto.cs** (MODIFICADO)
   - ➕ Añadida línea en blanco antes de `Estado` para legibilidad
   - ➕ Aplicado `[JsonConverter(typeof(NumberToStringConverter))]` al campo `Estado` (línea 24)

### Compilación
- ✅ Sin errores
- ✅ Sin warnings adicionales

### Revisión de Código
- ✅ Code review completado - sin comentarios
- ✅ CodeQL security scan - sin vulnerabilidades

---

## 🎯 Ventajas de esta Solución

### 1. Mínimamente Invasiva
- Solo afecta el campo `Estado` específicamente
- No modifica el comportamiento global de deserialización
- Otros campos string no se ven afectados

### 2. Flexible y Robusta
- ✅ Acepta números (caso actual del servidor)
- ✅ Acepta strings (por compatibilidad futura)
- ✅ Acepta null (campo nullable)

### 3. Consistente con Soluciones Previas
- Reutiliza el mismo converter que ya se usó para:
  - `Censo` en `BarcosDto`
  - `CensoBarco` en `BarcosTramitesDto`
- Mantiene consistencia en el manejo de campos con este tipo de problema

### 4. Correcta Separación de Responsabilidades
- El converter está en el proyecto `CRM.Dtos` (capa de datos)
- No contamina el código de servicios o presentación
- Puede reutilizarse en otros DTOs si es necesario

---

## 🔧 Campos Relacionados con Converter Aplicado

Los siguientes campos ya tienen aplicado el `NumberToStringConverter`:

1. **BarcosDto.Censo** (solucionado previamente)
   - Documentado en: `SOLUCION_Error_Censo_Number_To_String.md`
   
2. **BarcosTramitesDto.CensoBarco** (solucionado previamente)
   - Documentado en: `SOLUCION_Error_CensoBarco_Deserializacion.md`
   
3. **BarcosTramitesDto.Estado** (solucionado en este fix)
   - Documentado en este archivo

Todos estos campos requieren el mismo tratamiento de conversión number-to-string.

---

## 📚 Referencias Técnicas

- [System.Text.Json.Serialization - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization)
- [JsonConverter Attribute - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonconverterattribute)
- [Custom converters for JSON serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to)

### Documentación Relacionada
- `SOLUCION_Error_Censo_Number_To_String.md` - Fix original para el campo Censo
- `SOLUCION_Error_CensoBarco_Deserializacion.md` - Fix para el campo CensoBarco
- `SOLUCION_Error_Deserializacion_JSON_CamelCase.md` - Maneja String → Number

---

## ✅ Estado Final

- ✅ Error de deserialización Number → String resuelto
- ✅ Converter personalizado reutilizado
- ✅ Soporta JSON con números, strings, o null
- ✅ Aplicación minimalista (solo afecta el campo Estado)
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
2. **SOLUCION_Error_CensoBarco_Deserializacion.md** - Maneja Number → String para campo CensoBarco
3. **Este documento** - Maneja Number → String para campo Estado
4. **SOLUCION_Error_Deserializacion_JSON_CamelCase.md** - Maneja String → Number

Todos trabajando juntos garantizan la robustez de la deserialización JSON independientemente del formato que devuelva el servidor.

---

## 🎓 Lecciones Aprendidas

### 1. Los Converters Personalizados Son Reutilizables
```csharp
// ✅ BIEN - Un converter, múltiples usos
[JsonConverter(typeof(NumberToStringConverter))]
public string? CensoBarco { get; set; }

[JsonConverter(typeof(NumberToStringConverter))]
public string? Estado { get; set; }
```

### 2. Aplicar Converters Solo Donde Se Necesitan
```csharp
// ✅ MEJOR: Aplicar solo a campos específicos
[JsonConverter(typeof(NumberToStringConverter))]
public string? Estado { get; set; }

// ❌ PEOR: Configurar globalmente afectaría todos los strings
```

### 3. Los Converters Deben Ser Flexibles
El `NumberToStringConverter` acepta:
- Números (int64, double, decimal)
- Strings (para compatibilidad)
- Null (para campos opcionales)

Esta flexibilidad evita futuras incompatibilidades.

### 4. Documentar Cada Fix
Cada vez que se aplica un fix similar, documentarlo ayuda a:
- Entender el patrón de problemas
- Identificar rápidamente soluciones futuras
- Mantener consistencia en el código

---

## ✅ Checklist de Validación

- [x] **Converter aplicado** al campo `Estado`
- [x] **Compilación exitosa** sin errores
- [x] **Code review completado** sin issues
- [x] **CodeQL scan ejecutado** sin vulnerabilidades
- [x] **Documentación creada** (este archivo)
- [x] **Commit realizado** con mensaje descriptivo
- [x] **Listo para merge** a la rama principal

---

## 🚀 Siguiente Paso

**Ejecuta la aplicación** en producción y verifica:

1. Navega a páginas que cargan `api/Barcos` con includes de `BarcosTramites`
2. Abre la consola del navegador (F12)
3. Verifica que **NO** aparece el error de deserialización
4. Confirma que los trámites se muestran correctamente

Si todo funciona correctamente, el error ha sido resuelto definitivamente.
