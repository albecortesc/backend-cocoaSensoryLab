# API - Guia tecnica completa

Este proyecto expone la capa HTTP del sistema de usuarios, aplica autenticacion JWT, autorizacion por roles y delega la logica de datos al servicio de aplicacion `IUserService`.

## 1. Responsabilidad de la capa API

La capa API hace cinco cosas clave:

1. Recibir solicitudes HTTP.
2. Validar entrada y reglas de autorizacion.
3. Invocar casos de uso via `IUserService`.
4. Transformar entidades de dominio en DTOs de respuesta.
5. Emitir tokens JWT para usuarios autenticados.

## 2. Program.cs explicado seccion por seccion

Archivo: `Api/Program.cs`.

### 2.1 Host y URL de desarrollo

- Fuerza URL estable local: `http://localhost:5000`.

### 2.2 Lectura de JWT settings

Lee desde configuracion:

- `JwtSettings:Key`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`

Si faltan, usa defaults de desarrollo.

### 2.3 Registro de servicios base

- `AddControllers()`
- `AddEndpointsApiExplorer()`
- `AddSwaggerGen(...)`

Swagger agrega esquema `Bearer` para enviar el token en `Authorization`.

### 2.4 Autenticacion JWT

Configura `AddAuthentication().AddJwtBearer(...)` con:

- `ValidateIssuer = true`
- `ValidateAudience = true`
- `ValidateLifetime = true`
- `ValidateIssuerSigningKey = true`
- `ClockSkew = TimeSpan.Zero`

### 2.5 Autorizacion

- `AddAuthorization()`

Luego los endpoints usan `[Authorize]` y `[Authorize(Roles = ...)]`.

### 2.6 Configuracion Mongo + DI

Lee seccion `Mongo` en `appsettings.json`.

Obtiene:

- Connection string
- Database name
- Collection `Users`
- Collection `Tasks` (solo leida; no usada por servicios activos)

Registra implementacion:

- `IUserService -> MongoUserService`

### 2.7 Bootstrap de Root

Antes de atender requests:

- `await EnsureRootUserAsync(app.Services)`

Comportamiento:

1. Consulta usuarios.
2. Si existe un `Root`, termina.
3. Si no existe, pide datos por consola.
4. Crea y registra un Root.

### 2.8 Pipeline HTTP

Orden real:

1. Swagger (solo development)
2. `UseHttpsRedirection()`
3. `UseAuthentication()`
4. `UseAuthorization()`
5. `MapGet("/")` con redirect a swagger en dev
6. `MapControllers()`

Nota: actualmente no hay `UseStaticFiles()` en pipeline.

## 3. Configuracion de appsettings

Archivo: `Api/appsettings.json`.

```json
{
  "JwtSettings": {
    "Key": "DevOnly_ChangeThisJwtKey_AtLeast32Characters!",
    "Issuer": "ApiUsers",
    "Audience": "ApiUsersClient",
    "ExpireMinutes": 60
  },
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "cocoa_sensory_lab_db",
    "Collections": {
      "Users": "users",
      "Tasks": "tasks"
    }
  }
}
```

## 4. Controlador principal: UsersController

Archivo: `Api/Controllers/UsersController.cs`.

### 4.1 Atributos de clase

- `[ApiController]`
- `[Route("api/users")]`
- `[Authorize]` (por defecto todo protegido)

### 4.2 Endpoints

#### POST /api/users/register

- Auth: `Root,Admin`
- Request: `RegisterUserRequest`
- Respuesta: `201 Created` + mensaje
- Bloquea escalamiento: un `Admin` no puede crear `Root`.

Errores tipicos:

- `403` rol no permitido
- `409` email/cedula duplicado
- `400` validaciones de entrada

#### POST /api/users/login

- Auth: anonimo
- Request: `LoginRequest`
- Respuesta: `LoginResponse` con token JWT
- Si credenciales invalidas: `401`

Claims del token:

- `sub`: `user.Id` o cedula
- `email`
- `role`
- `cedula`

#### GET /api/users

- Auth: `Root,Admin`
- Devuelve lista completa de usuarios

#### GET /api/users/search?cedula=&email=

- Auth: `Root,Admin`
- Exige al menos uno de los parametros
- Devuelve un usuario o `404`

#### PUT /api/users?cedula=&email=

- Auth: `Root,Admin`
- Actualiza por criterio de busqueda
- Mantiene hash de password actual
- `Admin` no puede asignar rol `Root`

#### DELETE /api/users?cedula=&email=

- Auth: `Root,Admin`
- Borra por criterio

#### PUT /api/users/password

- Auth: cualquier usuario autenticado
- Cambia su propia password
- Valida password actual antes de actualizar

#### PUT /api/users/password/admin

- Auth: `Root,Admin`
- Cambia password de otra cuenta por cedula/email
- Si actor es `Admin`, no puede operar sobre `Root/Admin`

## 5. DTOs y contratos HTTP

Archivo: `Api/DTOs/UserDtos.cs`.

DTOs de entrada:

- `RegisterUserRequest`
- `LoginRequest`
- `UpdateUserRequest`
- `ChangePasswordRequest`
- `AdminChangeUserPasswordRequest`
- `UploadUserPhotoRequest` (definido, aun no usado por endpoints)

DTOs de salida:

- `UserResponse`
- `ApiMessageResponse`
- `LoginResponse`

## 6. Codigos HTTP usados

Mapa de respuestas del controlador:

- `200 OK`
- `201 Created`
- `400 BadRequest`
- `401 Unauthorized`
- `403 Forbidden`
- `404 NotFound`
- `409 Conflict`

## 7. Ejemplos de uso

### 7.1 Login

```bash
curl -X POST "http://localhost:5000/api/users/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@demo.com",
    "password": "12345"
  }'
```

### 7.2 Listar usuarios con token

```bash
curl -X GET "http://localhost:5000/api/users" \
  -H "Authorization: Bearer TU_TOKEN"
```

### 7.3 Registrar usuario

```bash
curl -X POST "http://localhost:5000/api/users/register" \
  -H "Authorization: Bearer TU_TOKEN_ROOT_O_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "nombres": "Ana",
    "apellidos": "Lopez",
    "cedula": 1234567,
    "telefono": 1234567890,
    "email": "ana@demo.com",
    "password": "12345",
    "role": "User"
  }'
```

## 8. Limitaciones y pendientes

- No existe TaskController ni endpoints de tareas activos en esta version.
- Existe `UploadUserPhotoRequest`, pero aun no hay endpoint para subir foto.
- Clave JWT de ejemplo no debe usarse en produccion.

## 9. Ejecucion y pruebas manuales

Desde raiz:

```bash
dotnet run --project Api/Api.csproj
```

Luego abre:

- `http://localhost:5000/swagger`
