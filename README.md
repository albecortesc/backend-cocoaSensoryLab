# Backend Cocoa Sensory Lab

Backend en .NET 10 orientado a gestion de usuarios con autenticacion JWT y persistencia en MongoDB.

Este documento explica el proyecto completo de forma practica, desde arquitectura y ejecucion hasta seguridad, endpoints y troubleshooting.

## 1. Estado real del proyecto

Este es el estado funcional actual, verificado en codigo:

- Implementado: CRUD de usuarios.
- Implementado: login con emision de JWT.
- Implementado: control de acceso por roles en endpoints HTTP.
- Implementado: bootstrap de usuario Root al arrancar la API.
- Implementado: cliente de consola para gestionar usuarios.
- No implementado aun: modulo de tareas (aunque existe clave `Tasks` en configuracion Mongo).

## 2. Arquitectura por capas

La solucion sigue una separacion clara por responsabilidades:

- `Domain`: entidades y reglas de negocio puras.
- `Application`: contratos (interfaces) de casos de uso.
- `Infrastructure`: implementaciones concretas (MongoDB).
- `Api`: capa HTTP (controladores, JWT, Swagger, DI).
- `ConsoleApp`: cliente CLI para pruebas manuales de usuarios.

### Flujo de dependencias

- `Domain` no depende de otras capas.
- `Application` depende de `Domain`.
- `Infrastructure` depende de `Application` y `Domain`.
- `Api` depende de `Application`, `Infrastructure` y `Domain`.
- `ConsoleApp` depende de `Application`, `Infrastructure` y `Domain`.

## 3. Estructura del workspace

Resumen de carpetas principales:

- `src-jwt.sln`: solucion de Visual Studio/.NET.
- `Api/`: proyecto web ASP.NET Core.
- `Application/`: contratos de aplicacion.
- `Domain/`: modelos y reglas de dominio.
- `Infrastructure/`: persistencia Mongo.
- `ConsoleApp/`: cliente de consola.

## 4. Requisitos

Para ejecutar localmente:

- .NET SDK 10.x
- MongoDB disponible (local o remoto)

Valores por defecto esperados por la API:

- Mongo URI: `mongodb://localhost:27017`
- DB API: `cocoa_sensory_lab_db`
- Coleccion usuarios: `users`

## 5. Configuracion

Archivo principal: `Api/appsettings.json`.

### JwtSettings

- `Key`: clave de firma HMAC SHA256.
- `Issuer`: emisor valido del token.
- `Audience`: audiencia valida.
- `ExpireMinutes`: duracion del token.

### Mongo

- `ConnectionString`
- `DatabaseName`
- `Collections.Users`
- `Collections.Tasks`

Nota: `Collections.Tasks` existe en configuracion, pero no hay implementacion activa del modulo de tareas en esta version.

## 6. Ejecucion rapida

Desde la raiz del repo:

```bash
dotnet build
dotnet run --project Api/Api.csproj
```

API disponible en:

- `http://localhost:5000`

Swagger (modo desarrollo):

- `http://localhost:5000/swagger`

Para el cliente de consola:

```bash
dotnet run --project ConsoleApp/ConsoleApp.csproj
```

## 7. Primer arranque: creacion de Root

Cuando la API inicia, ejecuta una verificacion:

1. Consulta usuarios existentes.
2. Si no existe rol `Root`, solicita datos por consola.
3. Crea el usuario Root con password hasheado.

Campos solicitados:

- Nombres
- Apellidos
- Cedula (>= 1_000_000)
- Telefono (>= 1_000_000_000)
- Email
- Password (>= 5 caracteres)

## 8. Roles soportados

Roles definidos en dominio:

- `Root`
- `Admin`
- `User`
- `Invited`
- `Farmer`
- `Taster`

Reglas de autorizacion importantes:

- Solo `Root` y `Admin` pueden registrar usuarios.
- Un `Admin` no puede crear usuarios `Root`.
- Un `Admin` no puede cambiar password de cuentas `Root` o `Admin`.

## 9. Endpoints HTTP (usuarios)

Base route: `api/users`.

- `POST /api/users/login` (anonimo)
- `POST /api/users/register` (Root,Admin)
- `GET /api/users` (Root,Admin)
- `GET /api/users/search?cedula=&email=` (Root,Admin)
- `PUT /api/users?cedula=&email=` (Root,Admin)
- `DELETE /api/users?cedula=&email=` (Root,Admin)
- `PUT /api/users/password` (usuario autenticado)
- `PUT /api/users/password/admin` (Root,Admin)

## 10. Seguridad actual

Lo que si esta implementado:

- JWT Bearer con validacion estricta de issuer, audience, lifetime y firma.
- `ClockSkew = 0` (expiracion sin margen).
- Password almacenado como hash SHA256 en hexadecimal.
- Normalizacion de email para evitar duplicados por mayusculas/minusculas.

Lo que debes mejorar si vas a produccion:

- Reemplazar SHA256 por Argon2 o BCrypt con salt.
- Mover `JwtSettings:Key` a secretos de entorno.
- Agregar indices unicos en Mongo para `Email` y `Cedula`.

## 11. Diferencia importante entre API y ConsoleApp

La API y la consola no apuntan a la misma base de datos por defecto:

- API usa: `cocoa_sensory_lab_db`
- ConsoleApp usa: `users_jwt`

Consecuencia:

- Un usuario creado en consola puede no verse en la API, y viceversa.

## 12. Troubleshooting

### La API no arranca por static web assets

En este repo ya se aplico mitigacion para evitar fallo por manifiestos viejos:

- `StaticWebAssetsEnabled=false` en `Api/Api.csproj`
- sin `UseStaticFiles()` en pipeline

### Build bloqueado por DLL en uso

Si aparece error `MSB3021/MSB3027`, normalmente hay un proceso `Api` corriendo.

En PowerShell:

```powershell
Get-Process Api | Stop-Process -Force
```

### Comando de ejecucion desde la raiz

Usa proyecto explicito:

```bash
dotnet run --project Api/Api.csproj
```

## 13. Documentacion por proyecto

Lee estos README para detalle por capa:

- `Api/README.md`
- `Application/README.md`
- `Domain/README.md`
- `Infrastructure/README.md`
- `ConsoleApp/README.md`

## 14. Roadmap sugerido

Siguientes mejoras naturales:

1. Implementar modulo de tareas o retirar totalmente su configuracion residual.
2. Endpoints de foto de usuario (ya hay DTO, faltan rutas y servicio).
3. Logging estructurado y auditoria de cambios.
4. Pruebas unitarias por capa y pruebas de integracion API + Mongo.
