# Infrastructure - Persistencia MongoDB

La capa `Infrastructure` implementa los contratos de `Application` usando MongoDB Driver.

Aqui vive la logica de conexion, mapeo BSON y acceso a colecciones.

## 1. Archivos principales

- `Configuration/MongoOptions.cs`
- `Services/MongoCollectionResolver.cs`
- `Services/MongoUserService.cs`
- `Mongo/Mappings/BsonMappings.cs`
- `Mongo/Mappings/UserBsonMapping.cs`

## 2. MongoOptions

Archivo: `Infrastructure/Configuration/MongoOptions.cs`.

Define configuracion de Mongo:

- `ConnectionString` (default `mongodb://localhost:27017`)
- `DatabaseName` (default `cocoa_sensory_lab_db`)
- `Collections` (diccionario case-insensitive)

Colecciones por defecto:

- `Users` -> `users`
- `Tasks` -> `tasks`

Metodo clave:

- `GetCollectionName(string collectionKey)` valida existencia de clave y retorna nombre real.

## 3. MongoCollectionResolver

Archivo: `Infrastructure/Services/MongoCollectionResolver.cs`.

Responsabilidad:

- Crear `MongoClient` y `IMongoDatabase` desde `MongoOptions`.
- Resolver colecciones tipadas por clave logica.

Metodo:

- `GetCollection<TDocument>(collectionKey)`.

## 4. Mapeo BSON

### 4.1 BsonMappings

Archivo: `Infrastructure/Mongo/Mappings/BsonMappings.cs`.

Punto unico de registro de class maps.

Estado actual:

- Registra solo `UserBsonMapping`.

### 4.2 UserBsonMapping

Archivo: `Infrastructure/Mongo/Mappings/UserBsonMapping.cs`.

Que configura:

- `AutoMap()` de propiedades.
- `SetIgnoreExtraElements(true)`.
- Mapeo de `Id` string como `ObjectId` BSON.

Resultado:

- Interoperabilidad limpia entre entidad C# y documento Mongo.

## 5. MongoUserService (implementacion IUserService)

Archivo: `Infrastructure/Services/MongoUserService.cs`.

### 5.1 Constructores

Hay dos rutas de inicializacion:

1. Con `MongoCollectionResolver` y `collectionKey`.
2. Con `connectionString`, `databaseName`, `collectionName`.

Ambos:

- Validan parametros.
- Registran mappings BSON.
- Obtienen `IMongoCollection<UserEntity>`.

### 5.2 NormalizeEmail

Politica aplicada:

- `Trim()`
- `ToLowerInvariant()`

Sirve para mantener consistencia en busquedas y unicidad.

### 5.3 UserRegister

Flujo:

1. Valida `user` no nulo.
2. Normaliza email.
3. Verifica duplicado por email.
4. Verifica duplicado por cedula.
5. Genera `Id` si esta vacio.
6. Inserta documento.

### 5.4 UserAuthenticate

Flujo:

1. Si email/password vacios -> `false`.
2. Normaliza email.
3. Busca usuario.
4. Si existe, valida password con `VerifyPassword`.

### 5.5 UserGetAll

- `Find(FilterDefinition.Empty)` y `ToListAsync()`.

### 5.6 UserUpdateByCedulaOrEmail

Flujo:

1. Valida `updatedUser`.
2. Exige criterio (cedula o email).
3. Busca usuario existente.
4. Normaliza email nuevo.
5. Verifica duplicado de cedula en otro `Id`.
6. Verifica duplicado de email en otro `Id`.
7. Preserva `Id` original.
8. `ReplaceOneAsync`.

### 5.7 UserFindByCedulaOrEmail

- Exige criterio.
- Busca por cedula o email normalizado.
- Retorna usuario o `null`.

### 5.8 UserDeleteByCedulaOrEmail

- Exige criterio.
- `DeleteOneAsync`.
- Si borra 0, lanza `User not found`.

## 6. Diseno de errores en Infrastructure

Tipos usados:

- `ArgumentException`
- `ArgumentNullException`
- `InvalidOperationException`

La API traduce estos errores a codigos HTTP adecuados.

## 7. Consideraciones de produccion

Recomendaciones practicas:

1. Crear indices unicos en Mongo (`Email`, `Cedula`).
2. Configurar timeouts, retries y observabilidad.
3. Externalizar secretos y URI por ambiente.
4. Agregar health checks de Mongo.

## 8. Sobre Tasks en esta capa

Aunque `MongoOptions` trae clave `Tasks`, en la version actual:

- No hay `MongoTaskService` activo.
- No hay `TaskBsonMapping` en codigo.
- No hay contrato `ITaskService` en `Application`.

Esto sugiere funcionalidad pendiente o removida.
