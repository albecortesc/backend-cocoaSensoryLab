# Domain - Modelo y reglas de negocio

La capa `Domain` contiene los tipos centrales del negocio y sus invariantes.

No depende de HTTP, MongoDB, Swagger ni framework web. Esta capa debe ser estable y reutilizable.

## 1. Componentes actuales

- `Entities/User.cs`
- `Enums/Role.cs`

## 2. Entidad User

Archivo: `Domain/Entities/User.cs`.

### 2.1 Propiedades

- `Id` (string nullable): identificador tecnico compatible con ObjectId.
- `Nombres`: requerido, minimo 1 caracter.
- `Apellidos`: requerido, minimo 1 caracter.
- `Cedula`: requerido, minimo 7 digitos.
- `Telefono`: requerido, minimo 10 digitos.
- `Email`: requerido, formato email.
- `Role`: enum de rol.
- `PasswordHash`: requerido, hash en texto hexadecimal.

### 2.2 Validaciones declarativas

Se usan DataAnnotations:

- `[Required]`
- `[MinLength]`
- `[Range]`
- `[EmailAddress]`

### 2.3 Constructor publico

El constructor recibe `passwordHash` (no password en texto plano).

Validaciones con guard clauses:

- `nombres`, `apellidos`, `email`, `passwordHash` no nulos.
- `cedula >= 1_000_000`.
- `telefono >= 1_000_000_000`.

Observacion tecnica:

- Dentro del constructor hay asignacion duplicada de `Telefono`.
- No rompe funcionamiento, pero se puede limpiar.

### 2.4 CreateWithPassword(...)

Factory method recomendado para crear usuario con password en claro.

Que hace:

1. Valida longitud minima de password (5).
2. Calcula hash SHA256.
3. Construye `User` con `PasswordHash` ya generado.

### 2.5 VerifyPassword(password)

Que hace:

1. Si password vacio, retorna `false`.
2. Hash del input.
3. Compara con `PasswordHash` usando `StringComparison.Ordinal`.

### 2.6 ComputeHash(input)

Implementacion:

- SHA256 sobre bytes UTF-8.
- Salida hex en minusculas (`x2`).

Resultado:

- hash de 64 caracteres.

## 3. Enum Role

Archivo: `Domain/Enums/Role.cs`.

Valores definidos:

- `Root`
- `Admin`
- `User`
- `Invited`
- `Farmer`
- `Taster`

Interpretacion funcional (segun reglas actuales de API):

- `Root`: maximo privilegio.
- `Admin`: administracion operativa con restricciones anti-escalamiento.
- `User`: operacion estandar.
- `Invited`: perfil restringido.
- `Farmer` y `Taster`: roles disponibles en el modelo, listos para reglas futuras.

## 4. Limites de la capa Domain

La capa Domain intencionalmente no valida:

- Unicidad de email.
- Unicidad de cedula.

Esas reglas dependen de almacenamiento y se aplican en `Infrastructure`.

## 5. Implicaciones de seguridad

Estado actual:

- Hash sin sal (SHA256) funcional para demo/desarrollo.

Recomendacion para produccion:

- Migrar a Argon2 o BCrypt con salt por usuario.

## 6. Buenas practicas de uso

Para crear o actualizar contrasenas:

- Preferir `CreateWithPassword(...)`.

Para actualizar otros campos sin cambiar password:

- Reusar `PasswordHash` existente en constructor.

## 7. Evolucion recomendada

1. Encapsular hash en un value object o servicio dedicado.
2. Agregar tipos de error de dominio mas expresivos.
3. Definir entidades de tareas cuando el modulo se implemente.
