# Vista de Detalle de Barco

## Descripción General

La vista `DetalleBarco.razor` es una página completa para la visualización y gestión de la información de un barco, incluyendo sus trámites y usuarios asociados. Esta página es accesible desde el listado de empresas (ListaBarcos.razor) haciendo clic en el botón de "Ver Trámites".

## Características Principales

### 🚢 Panel de Información de Barco

- **Cabecera Visual**: Card destacado con gradiente que muestra:
  - Icono de barco
  - Nombre del barco
  - Matrícula
  - Código del barco
  - Nombre del capitán (si existe)
  - Información del armador (empresa asociada)

### 📊 Estadísticas en Tiempo Real

Cuatro tarjetas con estadísticas de trámites clasificados por fechas:

1. **Total Trámites**: Contador general de todos los trámites del barco
2. **Vigentes**: Trámites con `FechaFin > Hoy` (color azul)
3. **Por Vencer**: Trámites que vencen en los próximos 30 días (color amarillo)
4. **Vencidos**: Trámites con `FechaFin <= Hoy` (color rojo)

### 📑 Sistema de Pestañas

#### Pestaña "Trámites"

- **Listado de Trámites**: Cards visuales con información completa de cada trámite
  - Fecha de vencimiento prominente con indicador visual de color
  - Certificado/nombre del trámite
  - Fechas de inicio y fin
  - Estado visual (Vigente, Por Vencer, Vencido)
  - Días restantes hasta vencimiento
  - Información de avisos por email programados
  - Observaciones
  - Botones de **Editar** y **Eliminar**

- **Formulario de Creación/Edición**:
  - Modal fullscreen responsive
  - Campos incluidos:
    - Certificado/Trámite (obligatorio)
    - Fecha Inicio (obligatorio)
    - Fecha Fin (obligatorio)
    - Fecha Aviso (opcional)
    - Días de Aviso (default: 30)
    - Lista de Emails para Avisos (separados por coma)
    - Observaciones (opcional)

#### Pestaña "Usuarios"

- **Listado de Usuarios**: Cards con información de cada usuario
  - Avatar con iniciales
  - Nombre completo
  - Email y teléfono
  - Rol (badge: Cliente, Administrador, Asistente)
  - Estado (Activo/Inactivo)
  - Fecha de registro
  - Botones de **Editar** y **Eliminar**

- **Formulario de Creación/Edición**:
  - Modal fullscreen responsive
  - Campos incluidos:
    - Nombre (obligatorio)
    - Apellidos (obligatorio)
    - Email (obligatorio)
    - Teléfono (opcional)
    - NIF (obligatorio, único al crear)
    - Rol (selector: Cliente, Administrador, Asistente)
    - Contraseña (obligatorio solo al crear)
    - Email para Avisos (opcional, por defecto usa el email principal)
    - Estado Activo (checkbox)

## Navegación

### Desde el Listado de Barcos (ListaBarcos.razor)
- Click en el botón "Ver Trámites" (icono de documento) de cualquier barco
- Ruta: `/barco/empresa/{CodigoEmpresa}/tramites/{CodigoBarco}`

## Funcionalidades CRUD

### Trámites
- ✅ **Crear**: Click en "Nuevo Trámite" abre modal de formulario
- ✅ **Leer**: Visualización de todos los trámites en lista con filtros visuales por estado
- ✅ **Actualizar**: Click en botón "Editar" en cada trámite abre modal con datos precargados
- ✅ **Eliminar**: Click en botón "Eliminar" en cada trámite (eliminación directa)

### Usuarios
- ✅ **Crear**: Click en "Nuevo Usuario" abre modal de formulario
- ✅ **Leer**: Visualización de todos los usuarios en grid responsivo
- ✅ **Actualizar**: Click en botón "Editar" en cada usuario abre modal con datos precargados
- ✅ **Eliminar**: Click en botón "Eliminar" en cada usuario (eliminación directa)

## Filosofía de Diseño

### Consistencia Visual
- Sistema de gradientes y colores consistente con el resto de la aplicación
- Tarjetas con bordes redondeados (`rounded-2xl`)
- Efectos hover con `shadow-lg` y `scale-105`
- Sistema de colores semántico:
  - 🔵 Azul (Sky): Información/Vigente
  - 🟡 Amarillo (Amber): Advertencia/Por Vencer
  - 🔴 Rojo: Error/Vencido
  - 🟢 Verde: Éxito/Usuarios

### Responsive Design
- Grid adaptativo: 1 columna (móvil) → 2 columnas (tablet) → 4 columnas (desktop)
- Breakpoints de Tailwind: `sm:` (640px), `md:` (768px), `lg:` (1024px)
- Texto escalable con clases responsivas
- Modales fullscreen en móvil

### Accesibilidad
- Labels semánticos en formularios
- Placeholders descriptivos
- Estados de deshabilitación claros
- Indicadores visuales de validación
- Íconos de Material Symbols para claridad visual

## Modelo de Datos

### BarcosDto
```csharp
- CodigoBarco: string (identificador)
- NombreB: string (nombre del barco)
- Matricula: string (matrícula del barco)
- CapitanNombre: string (nombre del capitán)
- Censo: int (censo del barco)
- BarcosTramites: ICollection<BarcosTramitesDto>
```

### BarcosTramitesDto
```csharp
- Id: Guid
- Certificado: string (nombre del trámite)
- FechaInicio: DateOnly
- FechaFin: DateOnly (usado para clasificación)
- FechaAviso: DateOnly (para notificaciones)
- DiasAvisoTramite: int
- ListaEmailsEnvioAviso: string (CSV)
- Observaciones: string
- CodigoBarco: string
- CodigoEmpresa: string
- CensoBarco: int
- FechaCreacion: DateOnly
```

### UsuarioDto
```csharp
- Id: Guid
- Nombre: string
- Apellidos: string
- EMail: string
- Telefono: string
- NIFAcceso: string (único)
- Rol: string (Cliente, Administrador, Asistente)
- PasswordHash: string
- EMailAvisos: string
- Activo: bool
- FechaRegistro: DateTime
- CodigoEmpresa: string
- EmpresaId: Guid
- NombreUsuario: string
```

### EmpresasDto
```csharp
- CodigoEmpresa: string
- NombreArmador: string
- NIFE: string
- Barco: BarcosDto
```

## Servicios Utilizados

- `IApiClient<EmpresasDto>`: Obtención de datos de la empresa
- `IApiClient<BarcosDto>`: Obtención de datos del barco y sus trámites
- `IApiClient<BarcosTramitesDto>`: CRUD de trámites
  - `GetAllAsync()`: Obtener todos los trámites
  - `CreateAsync()`: Crear nuevo trámite
  - `UpdateAsync()`: Actualizar trámite existente
  - `DeleteAsync()`: Eliminar trámite
- `IApiClient<UsuarioDto>`: CRUD de usuarios
  - `GetAllAsync()`: Obtener todos los usuarios
  - `CreateAsync()`: Crear nuevo usuario
  - `UpdateAsync()`: Actualizar usuario existente
  - `DeleteAsync()`: Eliminar usuario

## Estados de Carga

- **isLoading**: Spinner central mientras se cargan los datos
- **barco == null**: Mensaje de error si el barco no existe
- **Botón volver**: Siempre disponible para regresar al listado

## Validaciones

### Trámites
- Certificado obligatorio
- Si no hay fecha de inicio, se usa la fecha actual
- Si no hay fecha de fin, se usa fecha actual + 1 año
- Si no hay fecha de aviso, se calcula: `FechaFin - DiasAvisoTramite`
- Al actualizar, se preserva el ID del trámite

### Usuarios
- Nombre y NIF obligatorios
- Validación de NIF único en el sistema (solo al crear)
- Si no hay EMailAvisos, se usa EMail
- Si no hay NombreUsuario, se genera desde el Email
- Al actualizar, se preserva el ID del usuario
- Contraseña obligatoria solo al crear nuevo usuario

## Comparación con DetalleEmpresa

Esta página es similar a `DetalleEmpresa.razor` pero con enfoque en:
- **Barco como entidad principal** (en lugar de empresa)
- **Navegación desde ListaBarcos** con parámetros de empresa y barco
- **Mismo sistema de CRUD** para trámites y usuarios
- **Diseño consistente** con esquema de colores específico para barcos (sky/blue)

## Rutas y Parámetros

- **Ruta**: `/barco/empresa/{CodigoEmpresa}/tramites/{CodigoBarco}`
- **Parámetros**:
  - `CodigoEmpresa`: Código de la empresa propietaria del barco
  - `CodigoBarco`: Código único del barco

## Mejoras Futuras Sugeridas

1. **Confirmación de Eliminación**: Agregar diálogo de confirmación antes de eliminar trámites y usuarios
2. **Búsqueda y filtros**: Búsqueda de trámites por certificado o fechas
3. **Exportación**: Exportar listado de trámites a PDF/Excel
4. **Historial**: Ver historial de cambios en trámites y usuarios
5. **Notificaciones en tiempo real**: Sistema de alertas para trámites próximos a vencer
6. **Adjuntos**: Permitir subir documentos PDF relacionados con los trámites
7. **Calendario**: Vista de calendario con fechas de vencimiento de trámites
8. **Validación mejorada**: Validación de formato de emails en el formulario
9. **Mensajes de éxito/error**: Toast notifications para operaciones CRUD
10. **Paginación**: Agregar paginación si hay muchos trámites o usuarios

## Arquitectura del Código

```
DetalleBarco.razor (Vista)
├── Componentes visuales (HTML + Tailwind CSS)
├── Lógica de renderizado condicional
└── Referencias a métodos del code-behind

DetalleBarco.razor.cs (Lógica)
├── Propiedades de estado
├── Métodos de carga de datos
├── Gestión de Trámites
│   ├── MostrarFormularioTramite()
│   ├── EditarTramite()
│   ├── CerrarFormularioTramite()
│   ├── GuardarTramite()
│   └── EliminarTramite()
├── Gestión de Usuarios
│   ├── MostrarFormularioUsuario()
│   ├── EditarUsuario()
│   ├── CerrarFormularioUsuario()
│   ├── GuardarUsuario()
│   └── EliminarUsuario()
└── Métodos auxiliares
    └── GetInicialesUsuario()
```

## Dependencias

- **Blazor**: Framework de UI
- **CRM.Dtos**: Librería externa con los DTOs (BarcosDto, EmpresasDto, BarcosTramitesDto, UsuarioDto)
- **CRM.V3.Shared.Interfaces**: Interfaces de servicios (IApiClient)
- **Tailwind CSS**: Framework de estilos
- **Material Symbols**: Iconografía

## Notas de Implementación

1. La página utiliza el patrón code-behind para separar lógica y presentación
2. Todos los servicios son inyectados mediante `@inject` en el archivo .razor
3. Los formularios usan modales que se pueden cerrar clickeando fuera de ellos
4. El código maneja correctamente ResponseDto retornado por los métodos del API
5. Se utiliza `StateHasChanged()` para forzar re-renderizado después de operaciones CRUD
