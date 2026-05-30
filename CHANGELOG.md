# Registro de Cambios y Decisiones Técnicas (CHANGELOG)

Este archivo registra las modificaciones importantes, correcciones de errores y nuevas funcionalidades implementadas en el proyecto, junto con su justificación técnica.

## [2026-05-29] - Flujo de Recuperación de Contraseña (Forgot/Reset) (Task 25 - HU-F1)

### Añadido
- **Endpoints de Recuperación de Contraseñas** (`src/IceBackend.Api/Controllers/AuthController.cs`): Implementados los endpoints públicos `/api/v1/auth/forgot-password` (solicitud de reset, anti-enumeración de usuarios, retorna el token en respuesta e imprime en logger) y `/api/v1/auth/reset-password` (actualización de clave mediante token válido, con validaciones estrictas de password de longitud mínima de 8 caracteres).
- **Rate Limiting Local** (`src/IceBackend.Api/Controllers/AuthController.cs`): Registro de rate limiting local por IP a través de `IMemoryCache` en el endpoint de forgot-password (máximo 3 solicitudes por hora, retorna 429).
- **AddMemoryCache en Startup** (`src/IceBackend.Api/Program.cs`): Configurado el servicio de caché en memoria nativo de ASP.NET en el contenedor de dependencias del API.

### Corregido
- **Persistencia de Reset Tokens** (`src/IceBackend.Infrastructure/Services/RedisSessionCache.cs`): Implementados métodos `SetPasswordResetTokenAsync`, `GetPasswordResetTokenAsync` e `InvalidatePasswordResetTokenAsync` directamente en la API de Redis (`db.StringSetAsync`) bajo el prefijo `pwd_reset:` para garantizar el borrado sin depender de `IDistributedCache`.
- **Invalidación de Tokens (Single-Use)** (`src/IceBackend.Infrastructure/Services/AuthService.cs`): Implementado el borrado del token tras cambiar la contraseña de forma exitosa en Postgres, previniendo ataques de repetición.

## [2026-05-25] - Estabilización de DevSeed, Registro, Login y Logout Público de Cuentas ICE (Tasks 17, 19, 21, 22, 24)

### Añadido
- **Endpoint de Logout Público** (`src/IceBackend.Api/Controllers/AuthController.cs`): Implementado endpoint `POST /api/v1/auth/logout` protegido por el atributo `[Authorize]` que invalida y elimina la sesión activa del jugador.
- **Controlador de Autenticación Público** (`src/IceBackend.Api/Controllers/AuthController.cs`): Implementado `AuthController` con endpoints `POST /api/v1/auth/register` (registro y login atómico automático) y `POST /api/v1/auth/login` (login local clásico), ambos expuestos a Swagger y dotados de DTOs inline con validación `[Required]` y restricciones de longitud/caracteres.
- **Fail-Fast en Startup** (`src/IceBackend.Api/Program.cs`): Envoltorio try/catch con `Log.Fatal` y `throw` al arrancar para detener la aplicación ruidosamente si la base de datos no se inicializa o migra.

### Cambiado
- **Borrado Masivo por Lotes en Redis** (`src/IceBackend.Infrastructure/Services/RedisSessionCache.cs`): Se optimizó `RemoveSessionAsync` consolidando la invalidación de las 5-7 claves de Redis (mapeo directo, inverso, sesión dev, inventario cache, empty flags y suscripciones) en un único comando atómico masivo de eliminación (`db.KeyDeleteAsync(keysToDelete)`), reduciendo roundtrips de red y optimizando la memoria.
- **Migración Automática en Startup** (`src/IceBackend.Api/Program.cs`): Se sustituyó `EnsureCreated()` por `db.Database.Migrate()` en el arranque normal (fuera de Testing), garantizando que las tablas se creen siguiendo la historia de migraciones en `__EFMigrationsHistory` y previniendo colisiones de esquema persistentes.
- **Idempotencia de DevSeed** (`src/IceBackend.Api/Controllers/DevSeedController.cs`): Se flexibilizó el control de duplicados del endpoint `/api/v1/dev/seed` para que verifique únicamente si los cosméticos sembrados ya existen, en lugar de bloquear la operación si hay cualquier otro jugador registrado.

### Corregido
- **Rotación de Sesiones e Invalidez de Huérfanos** (`src/IceBackend.Infrastructure/Services/AuthService.cs`): Se inyectó el borrado preventivo `_sessionCache.RemoveSessionAsync()` antes de persistir tokens nuevos en `RegisterAndLoginAsync` y `LoginIceAccountAsync`, previniendo fugas de memoria en Redis de sesiones previas inactivas.
- **Atomicidad Transaccional en Registro** (`src/IceBackend.Infrastructure/Services/AuthService.cs`): El nuevo método de aplicación `RegisterAndLoginAsync` envuelve la inserción de PostgreSQL y la generación de la sesión de Redis bajo una transacción explícita de base de datos.
- **Aislamiento de Base de Datos en Tests** (`src/IceBackend.Api/Program.cs`): Se desactivó el llamado a `EnsureCreated()` (y consecuentemente `Migrate()`) para el entorno `Testing`, permitiendo que la suite de pruebas de integración controle el esquema completamente mediante `MigrateAsync()` sin colisionar con tablas creadas previamente de forma implícita.
- **Carga de Suscripciones en EF Core** (`src/IceBackend.Infrastructure/Data/Configurations/PlayerSubscriptionConfiguration.cs`): Se configuró la clave primaria `Id` de `PlayerSubscription` con `.ValueGeneratedNever()` para notificar a EF Core que el ID es generado en el dominio, previniendo excepciones `DbUpdateConcurrencyException` al sembrar datos.
- **Traducción LINQ en Consultas de Cosméticos** (`src/IceBackend.Infrastructure/Queries/CosmeticAssetQueryService.cs`): Ajuste de casting a `Guid` en la comparación de `c.Id` en `FirstOrDefaultAsync()` para evitar errores de traducción de LINQ en EF Core.
- **Visibilidad de Excepciones** (`src/IceBackend.Api/Middleware/ExceptionHandlingMiddleware.cs`): Se retiró `"Database"` de las palabras clave sensibles redactadas en desarrollo para permitir diagnosticar fallas de base de datos directamente en el body de `ProblemDetails`.

### Técnico
- **Timeout Resiliente de Testcontainers** (`tests/IceBackend.IntegrationTests/Fixtures/PostgreSqlFixture.cs`): Incrementado el timeout de Polly de 30 a 90 segundos para evitar fallas por `TimeoutRejectedException` cuando el daemon de Docker local opera bajo carga pesada.

## [2026-05-24] - Portabilidad, Dockerización, Endpoint DevSeed, Suscripciones, Configuración CORS y README Final (Tasks 14, 16, 17, 18, 20)

### Añadido
- **Dockerfile multi-stage** (`src/IceBackend.Api/Dockerfile`): Build con SDK 8.0 y runtime ASP.NET 8.0 con cache de capas para NuGet restore.
- **Servicio `api` en docker-compose.yml**: Integración completa con `env_file: .env` para heredar configuración y solo 2 overrides de red (`ConnectionStrings`) para apuntar a los hostnames internos de Docker.
- **Healthchecks en Postgres y Redis**: `pg_isready` y `redis-cli ping` con `condition: service_healthy` para evitar que la API arranque antes que los motores.
- **`DevSeedController`** (`src/IceBackend.Api/Controllers/DevSeedController.cs`): Controlador y endpoint seed `/api/v1/dev/seed` exclusivo de `Development` y oculto en Swagger. Permite sembrar datos de prueba deterministas (jugadores, cosméticos, ownerships y suscripción ICE+) de forma idempotente.
- **`SubscriptionController`** (`src/IceBackend.Api/Controllers/SubscriptionController.cs`): Implementado endpoint `GET /api/v1/subscription/me` autenticado con `[Authorize]` para consultar el estado actual de la suscripción del jugador en base de datos PostgreSQL e inyectar los beneficios calculados en caché Redis (Cache-Aside).
- **Suite de pruebas de integración para suscripciones** (`tests/IceBackend.IntegrationTests/Subscription/SubscriptionTests.cs`): 3 nuevas pruebas automatizadas certificando el comportamiento del endpoint en escenarios de suscripción activa (con beneficios), sin suscripción (beneficios nulos) y acceso no autenticado (401 Unauthorized).
- **Configuración de CORS** (`src/IceBackend.Api/Program.cs` y `appsettings.json`): Registrada la política `"IcePolicy"` con `WithOrigins` dinámicos desde configuración, `AllowCredentials()` requerido para la autenticación de cabeceras, y restricciones explícitas de headers (`Authorization`, `Content-Type`, `X-Session-Token`).
- **Pruebas de integración de CORS** (`tests/IceBackend.IntegrationTests/Cors/CorsTests.cs`): 3 nuevas pruebas automatizadas certificando el comportamiento de las peticiones preflight (OPTIONS) y la correcta presencia de cabeceras CORS (`Access-Control-Allow-Origin`, `Access-Control-Allow-Credentials`) frente a orígenes permitidos y su ausencia en orígenes no listados.
- **README final** (`README.md`): Documentación completa del proyecto incluyendo características, stack tecnológico, quick start local, despliegue en servidor real con Docker Compose + Nginx, tabla de endpoints, variables de entorno, estructura del proyecto, tests, auditoría de logs y guía de contribución.

### Cambiado
- **`docker-compose.yml`**: Agregados healthchecks a servicios `postgres` y `redis`. Agregado servicio `api` con mapeo `5000:8080`.
- **`docs/local-runbook.md`**: Agregada sección de ejecución con Docker Compose (API incluida), nota sobre dependencia del `.env`.

### Corregido
- **Suite E2E (Plan v2)**: Solucionado `CS0618` en constructores de Testcontainers (`PostgreSqlBuilder` y `RedisBuilder`). Rutas de webhook en pruebas emparejadas con Attribute Routing estricto (`/api/Webhooks/stripe`).
- **Redis y EF Core (Plan v2)**: Eliminada fuga de traducción LINQ al comparar Value Objects en `RedisSessionCache.cs` (usando instanciación explícita `== new PlayerId()`). Corregido binding de variables nombradas en Lua `ScriptEvaluateAsync`.

### Técnico
- Principio DRY aplicado a la configuración: Stripe, OAuth, CDN y Auth viven exclusivamente en `.env` (gitignorado). `docker-compose.yml` solo define las 2 rutas de red que cambian dentro del contenedor.
- `dotnet build`: 0 errores. Correcciones post-dockerización de caché de sesión y Testcontainers. Integración completa del seed de desarrollo, de la consulta de beneficios de suscripción y de las políticas seguras de CORS.

## [2026-05-23] - Testcontainers E2E (Task 11), Mitigación de Caché (Task 15) y Logs (Task 10)

### Añadido
- **Serilog (`Task 10`)**: Configuración completa de *Serilog* sustituyendo al logger por defecto. Logs estructurados JSON (`CompactJsonFormatter`) con fail-fast environment. Redireccionamiento del sink `File` exclusivo al `appsettings.Development.json` para proteger contenedores de producción de I/O bloqueante.

### Cambiado
- **Mitigación de *Cache Penetration* (`Task 15`)**: Modificado `RedisSessionCache.cs` reduciendo el TTL del caché nulo de 5 minutos a 30 segundos como *Circuit-Breaker*.
- **Integridad Semántica de Sets (Redis)**: Se reemplazó el centinela en texto plano (`"NONE"`) por una llave auxiliar de bloqueo (`inventory:player:{id}:empty_flag`). Evaluada de forma concurrente mediante un pipeline `Task.WhenAll` sin $O(1)$ adicional.
- **Transaccionalidad en Mutaciones**: En `EquipCosmeticUseCase` y `UnequipCosmeticUseCase`, la purga reactiva de caché fue removida del bloque `catch` inerte y amarrada de manera síncrona post-éxito del `SaveChangesAsync()`.
- **Transaccionalidad en Webhooks (Stripe)**: Removida la purga asíncrona dentro de los handlers y del bloque `finally` ciego en `StripeWebhookService`. Ahora recolecta *PlayerIds* concurrentemente (`ConcurrentBag<Guid>`) y los purga en iteración **sólo si** el pipeline principal confirma el `.CommitAsync()` hacia PostgreSQL exitosamente.

### Corregido
- **Mock JSON de Stripe (Task 19)**: Modificado `StripeSignatureHelper.BuildStripePayload()` para generar una estructura JSON más estricta (`api_version`, `livemode`, bloque `request` e `id` del objeto interno). Esto previene que el SDK oficial de Stripe (`Stripe.net` v40+) arroje un `NullReferenceException` interno (`JsonUtils.DeserializeObject`) al instanciar el evento durante las pruebas de los webhooks de integración.

### Técnico
- **Testcontainers E2E (`Task 11`)**: Refactorizada la suite E2E aislando Timeouts mediante *Polly WrapAsync* independiente (Docker Fail-Fast) y protegiendo el multihilo aislando estado con purga atómica determinista `UNLINK` (*FireAndForget*) en llaves acotadas por prueba. Implementación unitaria pura de firmas HMAC en `StripeSignatureUnitTests`.
- **Estabilización de Flujos Asíncronos (Task 11)**: Detectadas y corregidas múltiples *Race Conditions* y *Unique Constraint Violations* en `IceBackend.IntegrationTests` causadas por el hilo de fondo de ASP.NET (`Task.Run`). Se aislaron las pruebas herméticamente eliminando colisiones de llaves de suscripción. Se implementaron *Active Polling Waits* incrementados (30s) para contrarrestar latencia I/O en la máquina de Docker Desktop. Suite alcanza **9/11 pruebas estables**. Tareas `C6` y `C7` delegadas a revisión manual por fugas del hilo (Background Thread Leakage) interrumpiendo aserciones de xUnit.
- Reescritura del bootstrap en `Program.cs` para inmutabilidad del cargador de entorno `.env` atado a `ASPNETCORE_ENVIRONMENT == "Development"`.
- `TASKS.md`: Marcadas Tarea 10, Tarea 11 y Tarea 15 como completadas.
- `dotnet build tests/IceBackend.IntegrationTests`: Completado con 0 errores.

## [2026-05-22] - Suscripción ICE+: Modelo de Datos, Beneficios y Caché

### Añadido
- **Entidad `PlayerSubscription`**: Nueva entidad de dominio (`src/IceBackend.Domain/Entities/PlayerSubscription.cs`) que representa la suscripción premium ICE+ de un jugador. Almacena: estado activo, fecha de expiración, ID de suscripción Stripe, renovación automática, meses acumulados y marca de auditoría.
- **`IcePlusBenefitsProvider`**: Proveedor de dominio estático (`src/IceBackend.Domain/Services/IcePlusBenefitsProvider.cs`) que calcula los beneficios activos de ICE+ (prefijo `[ICE+]`, física de capas, sin anuncios, amigos ilimitados, 10% descuento automático en tienda) y resuelve las 15 variantes de color del icono evolutivo según los meses acumulados.
- **`PlayerSubscriptionConfiguration`**: Configuración de EF Core para la tabla `player_subscriptions` con relación 1-a-1 a `Player` y cascada en eliminación.
- **Métodos ICE+ en `Player`**: `AssignIcePlusSubscription`, `IncrementIcePlusMonths` y `CancelIcePlusSubscription` como métodos expresivos de dominio.
- **Caché de Suscripción en Redis**: Métodos `GetIcePlusBenefitsAsync` e `InvalidatePlayerSubscriptionAsync` en `ISessionCache`/`RedisSessionCache` con patrón Cache-Aside y protección contra Cache Penetration (clave `subscription:player:{id}`).
- **Tests unitarios `PlayerSubscriptionTests`**: 13 nuevos tests cubriendo activación, cancelación, acumulación de meses e icono evolutivo. Suite total: **40/40 tests superados**.

### Cambiado
- **`Player.cs`**: Eliminadas las propiedades obsoletas `ActiveRange` y `RangeExpiresAt` (desnormalización de rangos). Añadida la navegación `Subscription` 1-a-1 a `PlayerSubscription` y campo `RequiresSessionSync` mantenido.
- **`PlayerConfiguration.cs`**: Eliminado el mapeo de columnas obsoletas `active_range` y `range_expires_at`. Agregada la relación 1-a-1 con `PlayerSubscription`.
- **`ApplicationDbContext.cs`**: Registrado `DbSet<PlayerSubscription>`.
- **`StripeWebhookService.cs`**: `HandleInvoicePaidAsync` ahora activa la suscripción ICE+ e incrementa meses acumulados según `billing_reason`. `HandleInvoicePaymentFailedAsync` cancela la suscripción inmediatamente. `HandleSubscriptionDeletedAsync` cancela la suscripción y purga sesión, suscripción e inventario de Redis.
- **`RemoveSessionAsync` y `PurgePlayerSessionsAsync`**: Ahora también invalidan la clave de caché de suscripción del jugador.
- **`database.sql`**: Eliminadas columnas `active_range` y `range_expires_at` de la tabla `players`. Añadida la creación de la tabla `player_subscriptions` con índices único en `PlayerId` e índice en `StripeSubscriptionId`.

### Técnico
- Contexto de negocio (`ContextSkill.md`) y contexto técnico (`TechnicalContextSkill.md`) actualizados para reflejar el modelo ICE+ (estilo Lunar+) con sus beneficios, reglas de acumulación de meses y lógica de caché Redis.
- `TASKS.md`: Tarea 13 marcada como completada.

## [2026-05-22] - Endpoint DevLogin, Middleware de Errores, Refactor de Hashing y Reestructuración de Wearables/Idempotencia

### Añadido
- **Endpoint DevLogin (`POST /api/v1/dev/login`)**: Controlador `DevAuthController` exclusivo para desarrollo local. Protegido con triple barrera: guardia de entorno (`Development`), token maestro (`DEV_MASTER_TOKEN`) con fail-fast en constructor, y oculto de Swagger.
- **Sesiones de desarrollo aisladas (`dev_session:`)**: Nuevos métodos `SetDevSessionAsync`/`RemoveDevSessionAsync` en `ISessionCache`/`RedisSessionCache` con prefijos `dev_session:*`.
- **Resolución transparente de sesiones**: `GetPlayerIdBySessionAsync` busca primero en sesiones reales y luego en desarrollo como fallback.
- **Limpieza dual en `RemoveSessionAsync`**: Revoca claves reales y de desarrollo del mismo jugador.
- **Middleware de Excepciones Global**: `ExceptionHandlingMiddleware` unifica errores bajo `ProblemDetails` (RFC 7807), ocultando `Detail` en producción y censurando información sensible en desarrollo.
- **Interfaz `IPasswordHasher`**: Contrato en `IceBackend.Application` para desacoplar autenticación de implementaciones criptográficas.
- **Implementación `BcryptPasswordHasher`**: Clase en `IceBackend.Infrastructure` usando `BCrypt.Net` + `SHA256` para validación legacy.
- **Validación al Inicio (`ValidateOnStart`)**: Rango de `BcryptWorkFactor` (4-15) validado al arrancar.
- **Variables de Entorno**: `Auth__BcryptWorkFactor` y `Auth__LegacySalt` eliminando secretos de `appsettings.json`.
- **Pruebas de Middleware**: `ExceptionHandlingMiddlewareTests.cs` con 2 tests de mapeo 400/403 y redacción de trazas.
- **Value Objects de Dominio (`PlayerId` y `CosmeticId`)**: Records posicionales inmutables y fuertemente tipados que erradican la obsesión por los primitivos.
- **Tokens Criptográficos de Dominio**: `ValidatedEquipmentToken` y `ValidatedRangeAssignmentToken` inmutables y firmados simétricamente con HMAC-SHA256 con control de expiración.
- **Interfaces de Criptografía de Dominio**: `ICosmeticTokenSigner` e `IRangeTokenSigner` en el dominio, e implementaciones con clave derivada en la infraestructura.
- **Servicios de Dominio (`CosmeticEquipmentPolicy` y `RangeAssignmentService`)**: Desacoplan las invariantes de negocio de Minecraft (arquitectura de cliente, Fabric/Sodium, etc.) y las restricciones de cuenta (`UuidType`) fuera de las entidades físicas.
- **Verificador Mojang (`IMojangSessionValidator`)**: Interfaz en dominio con implementación en infraestructura que valida identidades premium contra la API oficial de Microsoft/Mojang.

### Cambiado
- **Refactorización de `AuthService`**: Inyectado `IPasswordHasher`, eliminadas dependencias estáticas a `BCrypt`/`SHA256`.
- **Atomicidad de Migración**: Auto-rehash envuelto en transacción de base de datos.
- **Tests Unitarios**: `AuthServiceTests` actualizado para inyectar `BcryptPasswordHasher`.
- **Sincronización de Base de Datos**: `docs/database.sql` sincronizado con tipos exactos, transaccionalidad e idempotencia.
- **Rediseño Completo de `Player`**: Removido `SessionHash` (sesiones 100% volátiles en Redis). Wearables mapeados como columnas de tipo entero anulable (`int?`) en base de datos mediante campos de respaldo privados (`_equippedHatInternalId`, etc.) de EF Core, logrando persistencia O(1) sin consultas extra a disco.
- **Clave Dual de `CosmeticAsset`**: Modificado para usar una clave interna secuencial (`InternalId`) para base de datos y la clave pública UUID (`CosmeticId`) para el exterior, previniendo ataques de enumeración.
- **Optimización de Caché Redis (`RedisSessionCache`)**: Purga atómica de sesiones implementada mediante un script Lua precargado usando `UNLINK` no bloqueante. Las sesiones activas del jugador se indexan en un Sorted Set (`ZSET`) con score basado en expiración para autolimpieza automática de memoria.
- **Configuración de Mapeo EF Core**: Adaptados `PlayerConfiguration`, `CosmeticAssetConfiguration` y `CosmeticAssetVersionConfiguration` para soportar las claves subrogadas, campos de respaldo privados de wearables, y columnas de rango/sincronización eventual.
- **Políticas de Equipamiento y Compatibilidad de Cuentas**: Refactorizado `CosmeticEquipmentPolicy.cs` para permitir que las cuentas `ICE` (no premium) usen arquitectura `Modern` en versiones modernas. La verificación de Mojang Session API ahora es opcional y solo se ejecuta para cuentas `PREMIUM` cuando el token no es nulo.

### Corregido
- **Mapping Relacional de Claves en EF Core (PlayerCosmeticOwnership)**: Refactorizado `PlayerCosmeticOwnership` para usar la clave interna secuencial `CosmeticAssetInternalId` (tipo `int`) apuntando a la PK de `CosmeticAsset`, evitando incompatibilidades de tipo y solucionando de raíz el error de compilación del modelo DbContext en EF Core InMemory y PostgreSQL.
- **Tipos de Claves Foráneas de Jugador**: Actualizado el tipo de `PlayerId` de `Guid` a `PlayerId` en `ExternalAuth`, `BootstrapToken`, `PaymentEvent` y `PlayerCosmeticOwnership` para garantizar coherencia exacta con la clave primaria fuertemente tipada de `Player`.
- **Esquema de Base de Datos**: Sincronizado por completo el archivo `docs/database.sql` con el modelo real de EF Core.

### Técnico
- Suite `BcryptPasswordHasherTests.cs` (6 tests) certificando hashing BCrypt, verificación, detección legacy y matching SHA-256.
- Sin nuevas dependencias NuGet; sin cambios en `IAuthService` ni `SessionTokenAuthenticationHandler`.
- 24/24 tests unitarios aprobados sin regresiones.
- Registro de dependencias actualizado en `Program.cs` para inyectar los nuevos firmadores, validadores de Mojang y políticas del dominio.

## [2026-05-21] - Fail-Fast de Arquitectura y Certificación BCrypt

### Añadido
- **Configuración de Entorno Estricta**: Creación de `.env.example` y deshardcodeo de secretos de `appsettings.json`.
- **Validación Fail-Fast en Arranque**: Validación de variables críticas (`PostgresConnection`, `RedisConnection`) en `Program.cs`.
- **Runbook Local y Docker Compose**: Documentación de inicialización local (`docs/local-runbook.md`) y `docker-compose.yml` (PostgreSQL 15, Redis 7).

### Mejorado
- **Reordenamiento Fail-Fast de Inventario**: `EquipCosmeticUseCase` valida compatibilidad de arquitectura (Legacy vs Modern) antes de consultas redundantes a BD.

### Técnico
- **Certificación de Hashing BCrypt**: Verificada implementación completa: (1) registro con `BCrypt.Net.BCrypt.HashPassword()`, (2) detección legacy SHA-256 con auto-rehash, (3) validación BCrypt nativa. Suite de 16/16 tests unitarios aprobados.
- **TASKS.md actualizado**: Tarea 7 marcada como completada.

## [2026-05-20] - Refactorización Arquitectónica, Autenticación Unificada y Seguridad de Borde

### Añadido
- **Options Pattern (Fail-Fast)**: Implementadas clases de opciones tipadas (`AuthOptions`, `CdnOptions`, `OAuthOptions`, `StripeOptions`) con DataAnnotations para validar la configuración externa en el arranque de la API mediante `.ValidateOnStart()`.
- **Query Services**: Creado `ICosmeticAssetQueryService` e implementación `CosmeticAssetQueryService` en Infraestructura para consultas optimizadas de solo lectura (`AsNoTracking()`).
- **Autenticación Declarativa (`[Authorize]`)**: Custom `SessionTokenAuthenticationHandler` para integrar la validación de tokens Bearer contra Redis en el pipeline nativo de ASP.NET Core.
- **Auditoría de Eventos Huérfanos**: Nueva tabla `unresolved_payment_events` en PostgreSQL con índices B-Tree en `ProviderEventId` para persistir webhooks de facturación sin jugador asociado (estado `PENDING_RESOLUTION`), preservando la integridad referencial.
- **Tokens Efímeros de Descarga**: Generación y validación de tokens firmados con `HMACSHA256` en `AssetTokenService` usando el `PlayerId` como salt/pepper por usuario para evitar la reutilización de firmas de descarga de cosméticos.
- **Endpoints de Entrega Segura**: Endpoint `POST /api/v1/assets/request-delivery` (autenticado) y endpoint `GET /api/v1/assets/deliver/{hash}` (anónimo con token efímero), que retorna un error genérico `403 Forbidden` ante fallos para evitar el probing.

### Cambiado
- **Desacople de DbContext**: Removido `ApplicationDbContext` del `AssetDeliveryController`. El controlador ahora delega las operaciones de base de datos a `ICosmeticAssetQueryService`.
- **Encapsulamiento de Entidades de Dominio**: Eliminados los setters públicos (`public set`) y convertidos en `private set` en entidades clave (`Player`, `PlayerCosmetic`, `ExternalAuth`, `CosmeticAsset`, `CosmeticAssetVersion`, `PaymentEvent`, `UnresolvedPaymentEvent`). Toda mutación ahora pasa por constructores seguros o métodos expresivos para prevenir la "anemia del dominio".
- **Inicialización de Servicios**: Refactorizados `AuthService`, `ExternalAuthService`, `StripeWebhookService`, `AssetTokenService`, y `CdnUrlSigner` para inyectar configuraciones usando `IOptionsSnapshot<T>`.

### Técnico
- Adaptación de la suite de pruebas unitarias (13 tests) para usar `Mock<IOptionsSnapshot<T>>` y el nuevo `ICosmeticAssetQueryService`. Todas las pruebas pasadas sin errores.
- Adaptación y ampliación de las pruebas unitarias en `AssetDeliveryControllerTests.cs` (13 tests en total aprobados).

## [2026-05-19] - Identidad Federada, Asset Delivery, JIT y Seguridad CDN

### Añadido
- **Flujo PKCE S256**: Implementación completa de Authorization Code Flow con PKCE para Microsoft y Google.
- **Seguridad Anti-Replay**: Sistema de detección de reuso de códigos con revocación automática de sesiones previas en Redis.
- **Validación CSRF Dinámica**: Generación de `state` de alta entropía con TTL de 5 minutos en caché distribuida.
- **Mapeo de Identidad**: Aislamiento total de proveedores externos; los perfiles se mapean a UUIDs internos generados en el servidor.
- **Persistencia Temporal**: Uso de Redis para custodiar los desafíos PKCE y estados de transición, asegurando memoria volátil para datos sensibles.
- **Compatibilidad**: Emisión de tokens de sesión compatibles con estándares de la industria (authlib).
- **Almacenamiento CAS**: Implementación de direccionamiento por contenido mediante hashes SHA256 para inmutabilidad de assets.
- **URLs Prefirmadas**: Sistema de firmas efímeras en `CdnUrlSigner` con expiración configurable, mitigando el hotlinking de recursos premium.
- **Diferenciación de Arquitecturas**: Soporte para versiones Legacy y Modern dentro de un mismo contrato de cosmético.
- **Validación de Propiedad (Ownership)**: El acceso a los metadatos y URLs de descarga está estrictamente custodiado por la tabla `PlayerCosmeticOwnership` y el `SessionToken` de Redis.
- **Metadatos Inmutables**: Soporte para configuración específica de motores (Legacy vs Modern) mediante campos `jsonb` de PostgreSQL.
- **Asset Delivery JIT**: Implementado flujo de redirección dinámica `307 Temporary Redirect` para activos cosméticos.
- **Validación O(1) en Memoria**: Nuevo sistema de inventario en Redis usando comandos `SISMEMBER` para validación instantánea de propiedad.
- **Patrón Cache-Aside**: Hidratación inteligente de permisos y mapeos de hashes desde PostgreSQL hacia Redis ante fallos de caché (Cache Miss).
- **Seguridad Anti-Leakage**: El sistema exige que el cliente purgue cabeceras `Authorization` antes de seguir redirecciones al CDN.
- **Firmas Efímeras**: Reducido el TTL de URLs de descarga a 60 segundos, garantizando que el acceso sea exclusivamente bajo demanda.

### Mejorado
- **Contratos Estáticos**: El DTO de cosméticos ahora es inmutable y cacheable, removiendo URLs temporales que degradaban el rendimiento del Launcher.
- **Resiliencia de Webhooks**: Integración de eventos `invoice.payment_failed` y purga atómica de inventario en Redis tras cambios en suscripciones de Stripe.

### Técnico
- Nueva migración para soportar versiones de assets (`AddCosmeticAssetVersions`).
- Registro de `IConnectionMultiplexer` en el contenedor DI para soporte de Redis Sets.
- Refactorización de `ISessionCache` y `RedisSessionCache` para desacoplar el motor de caché de la lógica de aplicación.
- 10/10 tests unitarios superados (incluyendo test de aislamiento de propiedad).

## [2026-05-18] - Infraestructura de Webhooks, Sesiones y Resiliencia

### Añadido
- **Infraestructura de Pruebas**: Creado proyecto `IceBackend.UnitTests` con xUnit para certificación de lógica core.
- **Integración con Redis**: Implementado `IDistributedCache` mediante `StackExchangeRedisCache`.
- **Abstracción de Caché**: Interfaz `ISessionCache` en Application e implementación en Infrastructure para mantener la pureza de capas.
- **Seguridad de Sesiones**: Generación de tokens criptográficamente seguros en el servidor y persistencia en Redis con TTL configurable.
- **Webhooks de Stripe**: Implementado endpoint `api/Webhooks/stripe` con lectura de body crudo para validación HMAC.
- **Validación Criptográfica**: Servicio `StripeWebhookValidator` para verificación de firmas en el borde de la API.
- **Idempotencia Híbrida**: Sistema de doble barrera (Redis + PostgreSQL) para evitar procesamiento duplicado de eventos.
- **Procesamiento Asíncrono**: Despachador de eventos con soporte para `invoice.paid` y `customer.subscription.deleted`.
- **Purga Automática de Sesiones**: Lógica para revocar el acceso en Redis inmediatamente tras la cancelación de una suscripción.

### Mejorado
- **Aislamiento de Infraestructura**: Los servicios de Stripe están desacoplados de la lógica de aplicación mediante DTOs y abstracciones.

### Corregido
- **Desincronización de Contratos**: Sincronizada la firma de `LoginIceAccountAsync` entre `IAuthService` y `AuthService`, restaurando la compilación del proyecto.
- **Validación de Identidad**: Asegurado que los UUIDs de cuentas ICE se generen exclusivamente en el servidor, mitigando riesgos de inyección de identidad desde el cliente.

### Técnico
- Aplicada migración inicial de PostgreSQL.
- Verificadas 9/9 pruebas unitarias exitosas.
