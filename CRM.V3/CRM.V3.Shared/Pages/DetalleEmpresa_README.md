# Vista de Detalle de Empresa

## Descripción General

La vista `DetalleEmpresa.razor` es una página completa para la visualización y gestión de la información de una empresa, incluyendo sus trámites y usuarios asociados.

## Características Principales

### ?? Panel de Información de Empresa

- **Cabecera Visual**: Card destacado con gradiente que muestra:
  - Iniciales de la empresa en un avatar grande
  - Nombre del armador
  - Código de empresa
  - NIF
  - Información del barco asociado (si existe)
  - Observaciones

### ?? Estadísticas en Tiempo Real

Cuatro tarjetas con estadísticas de trámites clasificados por fechas:

1. **Total Trámites**: Contador general de todos los trámites del barco
2. **Vigentes**: Trámites con `FechaFin > Hoy` (color azul)
3. **Por Vencer**: Trámites que vencen en los próximos 30 días (color amarillo)
4. **Vencidos**: Trámites con `FechaFin <= Hoy` (color rojo)

### ?? Sistema de Pestañas

#### Pestaña "Trámites"

- **Listado de Trámites**: Cards visuales con información completa de cada trámite
  - Fecha de vencimiento prominente con indicador visual de color
  - Certificado/nombre del trámite
  - Fechas de inicio y fin
  - Estado visual (Vigente, Por Vencer, Vencido)
  - Información de avisos por email programados
  - Observaciones

- **Formulario de Creación**:
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
  - Rol (badge)
  - Estado (Activo/Inactivo)
  - Fecha de registro

- **Formulario de Creación**:
  - Modal fullscreen responsive
  - Campos incluidos:
    - Nombre (obligatorio)
    - Apellidos (obligatorio)
    - Email (obligatorio)
    - Teléfono (opcional)
    - NIF (obligatorio, único)
    - Rol (selector: Cliente, Administrador, Asistente)
    - Contraseña (obligatorio)
    - Email para Avisos (opcional)
    - Estado Activo (checkbox)

## Navegación

### Desde el Panel de Control (PanelCofradia.razor)
- Click en cualquier empresa de la sección "Empresas Recientes"
- Ruta: `/empresa/{CodigoEmpresa}`

### Desde el Listado de Barcos (ListaBarcos.razor)
- Click en el botón "Ver Detalle" (icono de ojo) de cualquier empresa
- Ruta: `/empresa/{CodigoEmpresa}`

## Filosofía de Diseño

### Consistencia Visual
- Mismo sistema de gradientes y colores que PanelCofradia
- Tarjetas con bordes redondeados (`rounded-2xl`)
- Efectos hover con `shadow-xl` y `scale-105`
- Sistema de colores semántico:
  - ?? Azul: Información/Vigente
  - ?? Amarillo: Advertencia/Por Vencer
  - ?? Rojo: Error/Vencido
  - ?? Verde: Éxito/Usuarios

### Responsive Design
- Grid adaptativo: 1 columna (móvil) ? 2 columnas (tablet) ? 4 columnas (desktop)
- Breakpoints de Tailwind: `md:` (768px), `lg:` (1024px)
- Texto escalable con clases responsivas
- Modales fullscreen en móvil

### Accesibilidad
- Labels semánticos en formularios
- Placeholders descriptivos
- Estados de deshabilitación claros
- Indicadores visuales de validación

## Modelo de Datos

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
```

## Servicios Utilizados

- `IApiClient<EmpresasDto>`: Obtención de datos de la empresa
- `IApiClient<BarcosDto>`: Obtención de datos del barco y sus trámites
- `IApiClient<BarcosTramitesDto>`: CRUD de trámites
- `IApiClient<UsuarioDto>`: CRUD de usuarios

## Estados de Carga

- **isLoading**: Spinner central mientras se cargan los datos
- **empresa == null**: Mensaje de error si la empresa no existe
- **Botón volver**: Siempre disponible para regresar al listado

## Validaciones

### Trámites
- Certificado obligatorio
- Si no hay fecha de inicio, se usa la fecha actual
- Si no hay fecha de fin, se usa fecha actual + 1 año
- Si no hay fecha de aviso, se calcula: `FechaFin - DiasAvisoTramite`

### Usuarios
- Nombre y NIF obligatorios
- Validación de NIF único en el sistema
- Si no hay EMailAvisos, se usa EMail
- Si no hay NombreUsuario, se genera desde el Email

## Mejoras Futuras Sugeridas

1. **Edición en línea**: Permitir editar trámites y usuarios directamente desde el listado
2. **Eliminación**: Agregar funcionalidad de eliminar con confirmación
3. **Búsqueda y filtros**: Búsqueda de trámites por certificado o fechas
4. **Exportación**: Exportar listado de trámites a PDF/Excel
5. **Historial**: Ver historial de cambios en trámites y usuarios
6. **Notificaciones**: Sistema de alertas para trámites próximos a vencer
7. **Adjuntos**: Permitir subir documentos PDF relacionados con los trámites
8. **Calendario**: Vista de calendario con fechas de vencimiento de trámites
