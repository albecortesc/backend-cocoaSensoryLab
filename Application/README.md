# Application - Contratos de casos de uso

La capa `Application` define que puede hacer el sistema, sin decidir como se implementa cada operacion.

En este proyecto, la API y la consola dependen de esta capa para operar usuarios sin acoplarse a MongoDB.

## 1. Objetivo de la capa

`Application` existe para:

1. Declarar contratos claros de negocio.
2. Permitir multiples implementaciones del mismo contrato.
3. Facilitar testing con mocks o fakes.
4. Evitar que API y UI conozcan detalles de persistencia.

## 2. Estructura actual

Contenido principal:

- `Interfaces/IUserService.cs`

Nota importante:

- En el estado actual no hay `ITaskService` en codigo.

## 3. Interfaz IUserService (detallada)

Archivo: `Application/Interfaces/IUserService.cs`.

Firma:

```csharp
public interface IUserService
{
    Task UserRegister(UserEntity user);
    Task<bool> UserAuthenticate(string email, string password);
    Task<IEnumerable<UserEntity>> UserGetAll();
    Task UserUpdateByCedulaOrEmail(long? cedula, string? email, UserEntity updatedUser);
    Task<UserEntity?> UserFindByCedulaOrEmail(long? cedula, string? email);
    Task UserDeleteByCedulaOrEmail(long? cedula, string? email);
}
```

## 4. Semantica metodo por metodo

### 4.1 UserRegister(UserEntity user)

Objetivo:

- Registrar un usuario nuevo.

Comportamiento esperado:

- Valida argumento no nulo.
- Garantiza unicidad de email.
- Garantiza unicidad de cedula.
- Persiste la entidad.

Errores esperables:

- `ArgumentNullException`
- `InvalidOperationException`

### 4.2 UserAuthenticate(string email, string password)

Objetivo:

- Verificar credenciales.

Comportamiento esperado:

- Normalizar email.
- Buscar usuario.
- Verificar password.
- Devolver `true` o `false`.

### 4.3 UserGetAll()

Objetivo:

- Obtener todos los usuarios.

Comportamiento esperado:

- Retornar secuencia de `UserEntity`.

### 4.4 UserUpdateByCedulaOrEmail(...)

Objetivo:

- Reemplazar datos de un usuario identificado por cedula o email.

Comportamiento esperado:

- Exigir criterio de busqueda.
- Cargar usuario existente.
- Validar duplicados en nuevos datos.
- Preservar identidad tecnica (`Id`).
- Persistir reemplazo.

Errores esperables:

- `ArgumentException`
- `ArgumentNullException`
- `InvalidOperationException`

### 4.5 UserFindByCedulaOrEmail(...)

Objetivo:

- Buscar un usuario puntual.

Comportamiento esperado:

- Exigir cedula o email.
- Retornar entidad o `null`.

### 4.6 UserDeleteByCedulaOrEmail(...)

Objetivo:

- Borrar un usuario por criterio.

Comportamiento esperado:

- Exigir cedula o email.
- Ejecutar borrado.
- Si no existe, indicar error de no encontrado.

## 5. Reglas de diseno de esta capa

Principios aplicados:

- Contratos asincronos para IO no bloqueante.
- Criterio flexible de identificacion (cedula o email).
- Dependencia por abstraccion (`IUserService`).
- Entidades de dominio como tipo canonico de intercambio.

## 6. Implementaciones conocidas

La implementacion concreta activa esta en `Infrastructure`:

- `Infrastructure/Services/MongoUserService.cs`

Consumidores directos:

- `Api/Controllers/UsersController.cs`
- `ConsoleApp/Program.cs`

## 7. Como extender esta capa

Si quieres agregar nuevos casos de uso:

1. Define nueva interfaz en `Application/Interfaces`.
2. Implementala en `Infrastructure`.
3. Registrala en DI desde `Api/Program.cs`.
4. Consumela desde controlador o cliente.

## 8. Pendientes recomendados

- Definir interfaz para modulo de tareas cuando se implemente.
- Separar contratos de lectura/escritura si se necesita CQRS.
- Documentar errores de dominio con tipos propios en vez de `InvalidOperationException`.
