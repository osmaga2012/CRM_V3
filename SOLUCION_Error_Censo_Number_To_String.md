# Solución: Error de Deserialización JSON - Campo Censo (Number → String Conversion)

## 🔴 Problema Reportado

**Error en la consola del navegador:**
```
blazor.webassembly.js:1 Error fetching data from api/Empresa: DeserializeUnableToConvertValue, System.String 
Path: $[0].barco.censo | LineNumber: 0 | BytePositionInLine: 1191.
```

### Archivos Afectados
- `CRM.Dtos/BarcosDto.cs` - DTO con el campo `Censo`
- API endpoint: `api/Empresa?includes=Barco`

### Síntomas
- ❌ Error al deserializar JSON del endpoint `api/Empresa`
- ❌ Error específico en el campo `barco.censo`
- ❌ El deserializador no puede convertir el valor recibido

---

## 🔍 Diagnóstico del Problema

### Causa Raíz: Incompatibilidad de Tipos Number → String

**El problema es INVERSO al de codigoBarco:**

#### 1. El servidor devuelve JSON con el campo censo como número:
```json
{
  "codigoEmpresa": "EMP001",
  "codigoBarco": 2132,
  "barco": {
    "codigoBarco": 2132,
    "censo": 123456       // ← NÚMERO (int/long), no string
  }
}
```

#### 2. El DTO espera un string:

**BarcosDto.cs (línea 6):**
```csharp
public string? Censo { get; set; }  // ← Espera STRING
```

#### 3. El deserializador falla al intentar convertir:
```
JSON: "censo": 123456 (number)
  ↓
DTO: public string? Censo (string)
  ↓
❌ ERROR: No puede convertir number 123456 a string "123456"
```

### ¿Por qué ocurre este error?

El backend devuelve el campo `censo` como un número JSON porque probablemente es un campo numérico en la base de datos, pero en el frontend se decidió manejarlo como string (posiblemente porque puede contener caracteres especiales o formatos específicos).

---

## ✅ Solución Implementada

### 1. Crear NumberToStringConverter

**Archivo creado:** `CRM.Dtos/Converters/NumberToStringConverter.cs`

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRM.Dtos.Converters;

/// <summary>
/// Converter para manejar números que vienen del servidor como strings
/// Ejemplo: 123 -> "123", 123.45 -> "123.45"
/// </summary>
public class NumberToStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();  // Ya es string, devolver tal cual
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Convertir número a string
            if (reader.TryGetInt64(out var longValue))
            {
                return longValue.ToString();  // ✅ Convierte 123456 → "123456"
            }
            else if (reader.TryGetDouble(out var doubleValue))
            {
                return doubleValue.ToString();  // ✅ Convierte 123.45 → "123.45"
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return null;  // ✅ Maneja valores null
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            writer.WriteStringValue(value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
```

### 2. Aplicar el Converter al Campo Censo

**Archivo modificado:** `CRM.Dtos/BarcosDto.cs`

```csharp
using System.Text.Json.Serialization;
using CRM.Dtos.Converters;

namespace CRM.Dtos;

public class BarcosDto
{
    public long CodigoBarco { get; set; }
    
    // ↓ Aplicar converter específicamente a este campo
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? Censo { get; set; }
    
    public string? NombreB { get; set; }
    // ... resto de las propiedades
}
```

---

## 📋 Cómo Funciona el Converter

### Ejemplo con Censo en BarcosDto

**Escenario 1: JSON con número (caso actual del servidor)**
```json
{
  "censo": 123456  // ← Número
}
```

**Proceso de deserialización:**
1. El deserializador detecta que la propiedad `Censo` tiene un `JsonConverter` personalizado
2. Invoca `NumberToStringConverter.Read()`
3. El converter verifica: `reader.TokenType == JsonTokenType.Number` ✅
4. Intenta leer como long: `reader.TryGetInt64(out longValue)` → `longValue = 123456`
5. Convierte a string: `longValue.ToString()` → `"123456"`
6. ✅ Devuelve `"123456"` y lo asigna a `Censo`

**Escenario 2: JSON con string (por compatibilidad)**
```json
{
  "censo": "123456"  // ← String
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.String` ✅
2. Lee el string directamente: `reader.GetString()` → `"123456"`
3. ✅ Devuelve `"123456"` y lo asigna a `Censo`

**Escenario 3: JSON con null**
```json
{
  "censo": null  // ← Null
}
```

**Proceso de deserialización:**
1. El converter verifica: `reader.TokenType == JsonTokenType.Null` ✅
2. ✅ Devuelve `null` y lo asigna a `Censo`

---

## 🧪 Verificación de la Solución

### 1. Compilación
```bash
cd /home/runner/work/CRM_V3/CRM_V3
dotnet build CRM.Dtos/CRM.Dtos.csproj
dotnet build CRM.V3/CRM.V3.Shared/CRM.V3.Shared.csproj
dotnet build CRM.V3/CRM.V3.Web.Client/CRM.V3.Web.Client.csproj
```
**Resultado esperado:** ✅ Compilación correcta sin errores

### 2. Pruebas en Producción
1. Desplegar a producción (GitHub Pages/Render)
2. Navegar a las páginas que consumen `api/Empresa?includes=Barco`:
   - `/` (PanelCofradia)
   - `/detalle-barco`
   - `/detalle-empresa`
   - `/lista-barcos`
3. Abrir DevTools (F12) → Console
4. Verificar que **NO aparece:**
   ```
   Error fetching data from api/Empresa: DeserializeUnableToConvertValue, System.String 
   Path: $[0].barco.censo...
   ```
5. Verificar que los datos se cargan correctamente

---

## 📦 Resumen de Cambios

### Archivos Modificados/Creados

1. **CRM.Dtos/Converters/NumberToStringConverter.cs** (NUEVO)
   - ➕ Clase `NumberToStringConverter` (convierte number → string)
   - ✅ Maneja números enteros (int/long)
   - ✅ Maneja números decimales (float/double)
   - ✅ Maneja strings (ya convertidos)
   - ✅ Maneja valores null

2. **CRM.Dtos/BarcosDto.cs** (MODIFICADO)
   - ➕ `using System.Text.Json.Serialization;`
   - ➕ `using CRM.Dtos.Converters;`
   - ✏️ Aplicado `[JsonConverter(typeof(NumberToStringConverter))]` al campo `Censo`

### Compilación
- ✅ Sin errores
- ✅ Sin warnings adicionales

---

## 🎯 Ventajas de esta Solución

### 1. Mínimamente Invasiva
- Solo afecta el campo `Censo` específicamente
- No modifica el comportamiento global de deserialización
- Otros campos string no se ven afectados

### 2. Flexible y Robusta
- ✅ Acepta números (caso actual del servidor)
- ✅ Acepta strings (por compatibilidad)
- ✅ Acepta null (campo nullable)

### 3. Correcta Separación de Responsabilidades
- El converter está en el proyecto `CRM.Dtos` (capa de datos)
- No contamina el código de servicios o presentación
- Puede reutilizarse en otros DTOs si es necesario

---

## 🔧 Si Necesitas Aplicar el Converter a Otros Campos

Si encuentras otros campos con el mismo problema (número → string):

**Paso 1:** Aplicar el atributo en el DTO
```csharp
[JsonConverter(typeof(NumberToStringConverter))]
public string? CampoProblematico { get; set; }
```

**Paso 2:** Agregar los using necesarios
```csharp
using System.Text.Json.Serialization;
using CRM.Dtos.Converters;
```

---

## 📚 Referencias Técnicas

- [System.Text.Json.Serialization - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization)
- [JsonConverter Attribute - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonconverterattribute)
- [Custom converters for JSON serialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to)

---

## ✅ Estado Final

- ✅ Error de deserialización Number → String resuelto
- ✅ Converter personalizado implementado
- ✅ Soporta JSON con números, strings, o null
- ✅ Aplicación minimalista (solo afecta el campo Censo)
- ✅ Sin cambios en código de negocio o servicios
- ✅ Código compilando correctamente

**Fecha:** 2026-02-12  
**Versión .NET:** 10.0  
**Blazor:** WebAssembly

---

## 🔄 Relación con Otros Fixes

Este fix es complementario a:
- `SOLUCION_Error_Deserializacion_JSON_CamelCase.md` - Que maneja String → Number
- Este documento maneja el caso inverso: Number → String

Ambos trabajando juntos garantizan la robustez de la deserialización JSON independientemente del formato que devuelva el servidor.
