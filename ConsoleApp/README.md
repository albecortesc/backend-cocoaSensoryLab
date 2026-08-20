# ConsoleApp - Guia completa de la CLI

`ConsoleApp` es un cliente de consola para gestionar usuarios directamente contra `IUserService` sin pasar por HTTP.

Es ideal para aprendizaje, pruebas manuales y validacion rapida de la capa de negocio.

## 1. Objetivo

La CLI permite:

1. Registrar usuarios.
2. Probar autenticacion de credenciales.
3. Listar usuarios.
4. Editar usuarios de forma interactiva.
5. Buscar usuarios.
6. Borrar usuarios.

## 2. Configuracion de conexion

Archivo: `ConsoleApp/Program.cs`.

Instancia actual:

```csharp
IUserService userService = new MongoUserService(
    "mongodb://localhost:27017",
    "users_jwt",
    "users");
```

Importante:

- Esta base (`users_jwt`) no coincide con la base por defecto de la API (`cocoa_sensory_lab_db`).

## 3. Menu principal

Opciones disponibles:

- `1) Registrar usuario`
- `2) Login`
- `3) Listar usuarios`
- `4) Editar usuario`
- `5) Buscar usuario`
- `6) Borrar usuario`
- `0) Salir`

## 4. Registro de usuario

Metodo: `Register(IUserService svc)`.

Secuencia:

1. Validar nombres.
2. Validar apellidos.
3. Validar cedula (numero y minimo 7 digitos).
4. Validar telefono (numero y minimo 10 digitos).
5. Validar email (regex).
6. Validar password (minimo 5).
7. Validar role.
8. Crear entidad con `User.CreateWithPassword(...)`.
9. Guardar via `svc.UserRegister(...)`.

## 5. Validaciones interactivas

Metodos dedicados:

- `ValidateNombres`
- `ValidateApellidos`
- `ValidateCedula`
- `ValidateTelefono`
- `ValidateEmail`
- `ValidatePassword`
- `ValidateRole`

Detalle de roles soportados en la UI:

- 0=Root, 1=Admin, 2=User, 3=Invited, 4=Farmer, 5=Taster.
- Enter vacio = `User`.

## 6. Login

Metodo: `Authenticate(IUserService svc)`.

Comportamiento:

- Solicita email y password.
- Llama `UserAuthenticate`.
- Muestra exito o credenciales invalidas.

No genera JWT, porque es un cliente directo al servicio de aplicacion.

## 7. Listado de usuarios

Metodo: `ListUsers(IUserService svc)`.

Muestra por cada usuario:

- `Id`
- `Email`
- `Nombres + Apellidos`
- `Role`

## 8. Edicion de usuarios

Metodo: `UpdateUser(IUserService svc)`.

Flujo completo:

1. Elige criterio de busqueda (`Cedula` o `Email`).
2. Busca usuario existente.
3. Muestra datos actuales.
4. Pide nuevos valores (Enter para mantener).
5. Valida cada campo editable.
6. Muestra resumen de cambios.
7. Pide confirmacion explicita (`s`).
8. Si no cambia password, preserva hash existente.
9. Si cambia password, rehasea con `CreateWithPassword`.
10. Guarda via `UserUpdateByCedulaOrEmail`.

## 9. Busqueda de usuario

Metodo: `FindUser(IUserService svc)`.

Permite:

- Buscar por cedula.
- Buscar por email.

Muestra detalles del usuario o mensaje de no encontrado.

## 10. Borrado de usuario

Metodo: `DeleteUser(IUserService svc)`.

Pasos:

1. Elegir criterio (`Cedula` o `Email`).
2. Validar entrada.
3. Ejecutar borrado.
4. Mostrar resultado.

## 11. Captura segura de password

Metodo: `ReadPassword()`.

Caracteristicas:

- No muestra caracteres reales.
- Imprime `*` por cada tecla.
- Permite `Backspace`.
- Termina con `Enter`.

## 12. Ejecucion

Desde la raiz del repo:

```bash
dotnet run --project ConsoleApp/ConsoleApp.csproj
```

## 13. Casos de uso recomendados

Esta app es util para:

- Probar reglas de validacion de dominio.
- Verificar comportamiento de `MongoUserService`.
- Cargar datos iniciales de prueba.
- Depurar logica sin depender de HTTP/JWT.

## 14. Limitaciones actuales

- Gestiona solo usuarios.
- No consume endpoints de la API.
- No comparte base por defecto con API.

## 15. Mejoras sugeridas

1. Unificar DB con API mediante configuracion compartida.
2. Agregar confirmacion previa en borrado.
3. Separar validaciones en clase utilitaria para testing.
4. Agregar modo no interactivo con argumentos CLI.
