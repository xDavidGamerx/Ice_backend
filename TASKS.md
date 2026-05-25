# Backlog Técnico y Hoja de Ruta

Este documento contiene la lista de tareas del proyecto, estructurada para optimizar el desarrollo y el consumo de recursos de los agentes de IA.

---

## 1. Historial de Tareas Completadas

*   **Fase 1: Estabilidad Base e Integridad**
    *   [x] Corregir Contrato de Autenticación (`AuthService` sincronizado con `IAuthService`).
    *   [x] Certificación Matemática (5/5 tests unitarios de aislamiento aprobados).
    *   [x] Inicialización y migración local de la base de datos PostgreSQL.
*   **Fase 2: Autenticación, Seguridad y Webhooks**
    *   [x] Configuración de Redis (`AddStackExchangeRedisCache`).
    *   [x] Abstracción y persistencia de sesiones en Redis (`ISessionCache`).
    *   [x] Integración de Webhooks idempotentes de Stripe (validación de firma y purga de caché).
    *   [x] Flujo OAuth2 PKCE para Microsoft y Google.
*   **Fase 3: Optimización y Auditoría del Core**
    *   [x] Redirección segura CDN a través de URLs prefirmadas firmadas por el servidor.
    *   [x] Endpoints identificados por hash SHA256 (`/assets/{sha256}`).
    *   [x] Extensión de metadatos inmutables de motor y versión en `CosmeticAsset`.
    *   [x] Seguridad de Borde (`AssetTokenService` con firma HMAC-SHA256 y salt por jugador).
    *   [x] Autenticación declarativa unificada con `SessionTokenAuthenticationHandler` en Redis.
    *   [x] Resiliencia de webhooks mediante tabla de auditoría `unresolved_payment_events` en Postgres.
    *   [x] Hardening de Configuración (Options Pattern & Fail-Fast) para credenciales externas.
    *   [x] Encapsulamiento del Dominio (Eliminar setters públicos, usar private set y métodos expresivos).
    *   [x] Desacople de DbContext en Controladores (Implementar QueryServices como `ICosmeticAssetQueryService`).

---

## 2. Backlog de Tareas Activas (Priorizado)

Aquí se consolidan y detallan las tareas pendientes del proyecto (incluyendo negocio, calidad y deuda técnica del Informe Kevin) ordenadas por prioridad de ejecución recomendada:

0. - [x] **Refactorizar dominio de Jugador e infraestructura de Wearables**
   - **Prioridad**: Crítica (Bloqueante de Producción)
   - **Archivos afectados**: `src/IceBackend.Domain/Entities/Player.cs`, `src/IceBackend.Domain/Entities/PlayerId.cs`, `src/IceBackend.Infrastructure/Data/Configurations/PlayerConfiguration.cs`, `src/IceBackend.Application/UseCases/Inventory/EquipCosmeticUseCase.cs`, `docs/database.sql`
   - **Descripción**: Rediseñar la identidad del jugador con `PlayerId` posicional inmutable, remover `SessionHash` para aislar las sesiones en Redis volátil, y mover ranuras de cosméticos equipados a columnas directas `int?` (Nullable Integers) en la tabla `players`, validando en el dominio la compatibilidad del cliente (`UuidType` y `AssetArchitecture`).

0.5. - [x] **Implementar Webhook Idempotente de Stripe (`invoice.paid`) con Activación de Membresía ICE+ y Purga de Sesiones**
     - **Prioridad**: Alta (Bloqueante de Integración)
     - **Archivos afectados**: `src/IceBackend.Infrastructure/Services/StripeWebhookService.cs`, `src/IceBackend.Domain/Entities/Player.cs`, `src/IceBackend.Application/Interfaces/ISessionCache.cs`, `src/IceBackend.Infrastructure/Services/RedisSessionCache.cs`
     - **Descripción**: Procesar el evento `invoice.paid` de Stripe para extraer la metadata del rango, persistir el rango activo en PostgreSQL, y purgar de manera atómica las sesiones activas del jugador en Redis usando transacciones (`MULTI/EXEC`) para forzar la recarga inmediata de privilegios en el Launcher.

1. - [x] **Extraer secretos y configurar variables de entorno**
   - **Prioridad**: Alta
   - **Archivos afectados**: `appsettings.json`, `.gitignore`, `docs/local-runbook.md`, [NEW] `.env.example`
   - **Descripción**: Mover connection strings, webhook secrets y llaves criptográficas API a variables de entorno y crear una plantilla de variables de entorno `.env.example`.

2. - [x] **Crear runbook de ejecución local**
   - **Prioridad**: Alta
   - **Archivos afectados**: [NEW] `docs/local-runbook.md`, `README.md`
   - **Descripción**: Redactar una guía técnica paso a paso con prerequisitos de .NET SDK, PostgreSQL, Redis, comandos del ciclo de vida y depuración local.

3. - [x] **Implementar flujo de login local para desarrollo**
   - **Prioridad**: Alta
   - **Archivos afectados**: `IceBackend.Api`, `IceBackend.Application`, `IceBackend.Infrastructure`
   - **Descripción**: Creado endpoint `POST /api/v1/dev/login` protegido por guardia de entorno (`IsDevelopment`), token maestro (`DEV_MASTER_TOKEN`) y oculto de Swagger. Sesiones aisladas con prefijo `dev_session:` en Redis. `GetPlayerIdBySessionAsync` resuelve ambos prefijos.

4. - [x] **Sincerar y documentar estrategia de sesión en Redis**
   - **Prioridad**: Media
   - **Archivos afectados**: `docs/local-runbook.md`, `.skills/TechnicalContextSkill.md`
   - **Descripción**: Documentar la arquitectura de sesiones Redis (`SessionToken`) descartando formalmente la migración a JWT propuesta por Kevin, ya que la sesión en Redis está plenamente decidida y operativa.

5. - [x] **Implementar Casos de Uso para Equipar y Desequipar Cosméticos**
   - **Prioridad**: Alta
   - **Archivos afectados**: [NEW] `src/IceBackend.Application/UseCases/Inventory/EquipCosmeticUseCase.cs`, [NEW] `src/IceBackend.Application/UseCases/Inventory/UnequipCosmeticUseCase.cs`, `src/IceBackend.Api/Controllers/InventoryController.cs`
   - **Descripción**: Desarrollar la lógica de negocio para gestionar el equipamiento activo de cosméticos asociados a la cuenta del jugador en Redis (`SADD` / `SREM` sobre sets) y persistir el estado en PostgreSQL.

6. - [x] **Eliminar el acceso directo a la base de datos desde los controladores**
   - **Prioridad**: Alta
   - **Archivos afectados**: `src/IceBackend.Api/Controllers/AssetDeliveryController.cs`, `src/IceBackend.Application/UseCases/*`
   - **Descripción**: Se implementó `ICosmeticAssetQueryService` para aislar `ApplicationDbContext` de `AssetDeliveryController`.

7. - [x] **Fortalecer seguridad del Hashing de Contraseñas y Desacoplar Hasher**
   - **Prioridad**: Alta
   - **Archivos afectados**: `src/IceBackend.Infrastructure/Services/AuthService.cs`, `src/IceBackend.Application/Interfaces/IPasswordHasher.cs`, `src/IceBackend.Infrastructure/Services/BcryptPasswordHasher.cs`
   - **Descripción**: Sustituir el hashing SHA256 con salt fijo por un algoritmo robusto (BCrypt). Se desacopló mediante `IPasswordHasher`, parametrizando `BcryptWorkFactor` y `LegacySalt` en variables de entorno, y se implementó una transacción atómica para el flujo de auto-rehash transparente en login.

8. - [x] **Sincronizar esquema de Base de Datos SQL**
   - **Prioridad**: Alta
   - **Archivos afectados**: `docs/database.sql`
   - **Descripción**: Sincronizar el script SQL manual con las migraciones reales de EF Core para asegurar que represente fielmente el estado actual del modelo de datos (UUIDs, TIMESTAMPTZ, JSONB, indices GIN).

9. - [x] **Implementar Middleware Global de Manejo de Errores**
   - **Prioridad**: Media-Alta
   - **Archivos afectados**: `src/IceBackend.Api/Middleware/ExceptionHandlingMiddleware.cs`, `src/IceBackend.Api/Program.cs`
   - **Descripción**: Capturar excepciones de forma centralizada y retornar respuestas uniformes tipo `ProblemDetails` (RFC 7807) censurando información sensible en desarrollo y ocultando detalles en producción.

10. - [x] **Configurar sistema de Logs Estructurados (Serilog)**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Api/Program.cs`, `appsettings.json`
    - **Descripción**: Integrar y configurar Serilog para escribir logs estructurados en formato JSON (consola y archivo) que faciliten la auditoría de excepciones y webhooks.

11. - [x] **Escribir suite de Tests de Integración**
    - **Prioridad**: Media
    - **Archivos afectados**: `tests/IceBackend.IntegrationTests/*`
    - **Descripción**: Desarrollar pruebas integradas de extremo a extremo (usando Testcontainers de Postgres y Redis si es viable) para certificar webhooks de Stripe y flujos de sesión. *(Status: 11/11 Passing)*.

12. - [x] **Evitar anemia del dominio con Value Objects / Encapsulamiento**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Domain/Entities/*`
    - **Descripción**: Se encapsuló todo el estado mutable (`private set`) y se requiere uso de métodos expresivos o constructores para proteger las invariantes del dominio.

13. - [x] **Suscripción ICE+: Definir modelo de datos técnico y beneficios**
     - **Prioridad**: Media-Baja
     - **Archivos afectados**: `src/IceBackend.Domain/Entities/PlayerSubscription.cs`, `src/IceBackend.Infrastructure/Data/ApplicationDbContext.cs`, `docs/database.sql`
     - **Descripción**: Diseñar e implementar el modelo de datos para la suscripción ICE+ (estilo Lunar+), registrando el estado activo, expiración, renovación y un contador acumulativo de meses para los iconos evolutivos, persistiendo los beneficios y cargándolos en caché (Redis) mediante Cache-Aside.

14. - [x] **Configurar CORS (Cross-Origin Resource Sharing)**
     - **Prioridad**: Media
     - **Archivos afectados**: `src/IceBackend.Api/Program.cs`, `appsettings.json`
     - **Descripción**: Registrar y configurar políticas de CORS en el pipeline de ASP.NET Core que permitan al launcher (origen del cliente Electron) consumir la API, y a `localhost` para desarrollo. Debe ser restrictivo: solo orígenes explícitamente listados, sin permitir `AllowAnyOrigin()` en producción.

15. - [x] **Mitigación de Cache Penetration (Short-lived Null Cache Items)**
     - **Prioridad**: Alta
     - **Archivos afectados**: `src/IceBackend.Infrastructure/Services/RedisSessionCache.cs`
     - **Descripción**: Modificar la estrategia de Cache-Aside de suscripciones ICE+ para almacenar explícitamente valores nulos con expiración corta cuando la consulta a PostgreSQL no arroje resultados, evitando que consultas a IDs inexistentes degraden la base de datos.

---

## 3. Fase 4: Demo y Portabilidad (Opcional — Mejora de Presentación)

Tareas orientadas a hacer el proyecto portable (Docker) y demostrable (datos de prueba, endpoints visibles). No son deuda técnica ni bloquean producción.

16. - [x] **Dockerizar la API e integrarla en docker-compose**
     - **Prioridad**: Alta (para demo)
     - **Archivos afectados**: [NEW] `src/IceBackend.Api/Dockerfile`, `docker-compose.yml`
     - **Descripción**: Crear `Dockerfile` multi-stage en `src/IceBackend.Api/` y agregar el servicio `api` al `docker-compose.yml` con healthchecks en postgres/redis. Objetivo: `docker-compose up --build` levanta API + Postgres + Redis automáticamente.

17. - [x] **Endpoint `/api/v1/dev/seed` para datos de prueba**
     - **Prioridad**: Alta (para demo)
     - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/DevSeedController.cs`
     - **Descripción**: Crear endpoint idempotente que siembra 2 jugadores (1 con ICE+ activo y 3 meses acumulados, 1 sin suscripción), 3 cosméticos demo y 2 ownerships. Protegido con guardia de entorno `Development`.

18. - [x] **Endpoint `GET /api/v1/subscription/me`**
     - **Prioridad**: Media (para demo)
     - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/SubscriptionController.cs`
     - **Descripción**: Endpoint autenticado que retorna `{ isActive, accumulatedMonths, benefits: [...], stripeSubscriptionId, expiresAt }`. Usa `GetIcePlusBenefitsAsync` + consulta a DB. Visible desde Swagger.

19. - [x] **Corregir mock JSON de Stripe en tests de integración**
      - **Prioridad**: Alta (bloquea validación de webhooks)
      - **Archivos afectados**: `tests/IceBackend.IntegrationTests/Fixtures/StripeSignatureHelper.cs`
      - **Descripción**: `BuildStripePayload()` genera JSON simplificado incompatible con la versión actual de `Stripe.net`. El `EventConverter` interno espera campos adicionales del estándar (discriminador `"object"`, estructura anidada completa), y al no encontrarlos lanza `NullReferenceException` al deserializar. Reestructurar el payload mock para que `EventUtility.ParseEvent()`/`ConstructEvent()` no falle, manteniendo los datos mínimos que necesita la lógica de negocio (`customer`, `subscription`, `billing_reason`).

20. - [x] **README final con documentacion de despliegue**
      - **Prioridad**: Alta (para demo)
      - **Archivos afectados**: `README.md`
      - **Descripción**: Reestructurar README.md como carta de presentación del proyecto, incluyendo: descripción, características, stack tecnológico, quick start local, despliegue en servidor real con Docker Compose + nginx, tabla de endpoints, variables de entorno, estructura del proyecto, tests, guía de contribución y licencia. Basado en mejores prácticas de README para APIs REST.

---

## 4. Fase 5: Módulo de Usuarios, Roles y Gestión de Cosméticos (Fundacional)

Tareas críticas para cerrar el ciclo de vida completo del usuario y los cosméticos. Sin estas piezas el sistema es una demo sin capacidad de registrar usuarios ni crear contenido. **Prioridad máxima sobre las fases siguientes.**

**Contexto actual:**
- `IAuthService` tiene `RegisterIceAccountAsync` y `LoginIceAccountAsync` implementados, pero **no hay endpoint público** — solo existe en DevAuthController.
- `ExternalAuthService` ya hace OAuth Microsoft/Google y auto-crea jugadores. Pero no distingue si la cuenta Microsoft tiene Minecraft Java Edition.
- **No existe sistema de roles** — ni admin, ni moderador.
- **No existe CRUD de cosméticos** — solo endpoints de consulta y entrega.
- `PlayerCosmeticOwnership` existe como entidad, pero no hay forma de crearlo salvo por DevSeed.

### Registro y Autenticación

21. - [x] **Endpoint público de registro para cuentas ICE (username + password)**
      - **Prioridad**: Crítica
      - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/AuthController.cs`, `src/IceBackend.Application/UseCases/Auth/RegisterIceAccountUseCase.cs`
      - **Descripción**: Exponer `RegisterIceAccountAsync` de forma pública (`POST /api/v1/auth/register`). Recibe `{ username, password }`, valida unicidad, bcrypt hashing, crea `Player` con `UuidType.ICE`, inicia sesión automáticamente y devuelve `sessionToken`. Misma sesión en Redis que el resto de auth.

22. - [x] **Endpoint público de login para cuentas ICE**
      - **Prioridad**: Crítica
      - **Archivos afectados**: `AuthController.cs`
      - **Descripción**: `POST /api/v1/auth/login` con `{ username, password }`. Validación bcrypt (con soporte de re-hash transparente). Devuelve `sessionToken`. Si ya existe sesión activa para el jugador, invalida la anterior (rotación de token).

23. - [ ] **Detección de Minecraft Premium en registro Microsoft**
      - **Prioridad**: Alta
      - **Archivos afectados**: `ExternalAuthService.cs`, `Player.cs`, `UuidType.cs`
      - **Descripción**: Al registrar/login con Microsoft, llamar a la API de Mojang (`GET https://api.minecraftservices.com/entitlements/mcstore`) con el token de Microsoft para verificar si el usuario posee Minecraft Java Edition. Agregar campo `IsMcPremium` a `Player`. Si es `true`, el launcher puede habilitar funcionalidades exclusivas. Usuarios Google/ICE siempre `false`.

24. - [ ] **Endpoint de logout (invalidación de sesión)**
      - **Prioridad**: Alta
      - **Archivos afectados**: `AuthController.cs`, `ISessionCache.cs`, `RedisSessionCache.cs`
      - **Descripción**: `POST /api/v1/auth/logout` — elimina la sesión actual de Redis. Requiere `[Authorize]` (Bearer token). Previene reuso del token.

### Sistema de Roles (Admin)

25. - [ ] **Agregar flag `IsAdmin` a la entidad Player y migración**
      - **Prioridad**: Crítica
      - **Archivos afectados**: `Player.cs`, `PlayerConfiguration.cs`, migración EF Core
      - **Descripción**: Agregar `IsAdmin` (bool, default false) a `Player`. Crear migración. Configurar `IAuthorizationService` con política `"AdminOnly"` que verifique `player.IsAdmin`. Crear guardia de desarrollo para el primer admin vía `appsettings.Development.json` o variable de entorno.

26. - [ ] **Middleware / filtro de autorización para rutas admin**
      - **Prioridad**: Alta
      - **Archivos afectados**: [NEW] `src/IceBackend.Infrastructure/Authorization/AdminRequirement.cs`, `src/IceBackend.Api/Program.cs`
      - **Descripción**: Implementar `AuthorizationHandler<AdminRequirement>` que cargue `IsAdmin` desde Redis o BD y verifique contra el claim. Registrar política global. Usar `[Authorize(Policy = "AdminOnly")]` en controladores de administración.

### Perfil de Usuario

27. - [ ] **Endpoint de edición de perfil del jugador**
      - **Prioridad**: Alta
      - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/PlayerController.cs`, [NEW] `src/IceBackend.Application/UseCases/Player/UpdatePlayerProfileUseCase.cs`
      - **Descripción**: `PUT /api/v1/player/profile` — permite cambiar `Username` (con validación de unicidad), `DisplayName` (nuevo campo opcional si no existe, agregarlo a Player). Para cuentas ICE, permitir cambio de password enviando `{ oldPassword, newPassword }`. Purga caché de sesión tras cambios críticos.

28. - [ ] **Endpoint de consulta de perfil propio**
      - **Prioridad**: Media
      - **Archivos afectados**: `PlayerController.cs`
      - **Descripción**: `GET /api/v1/player/profile` — retorna datos del jugador autenticado: `{ id, username, uuidType, isMcPremium, isAdmin, createdAt }`. Sin exponer `PasswordHash`.

### CRUD Completo de Cosméticos (Admin)

29. - [ ] **Endpoint admin: Crear cosmético con subida de archivo**
      - **Prioridad**: Crítica
      - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/AdminCosmeticsController.cs`, [NEW] `src/IceBackend.Application/UseCases/AdminCosmetics/CreateCosmeticUseCase.cs`
      - **Descripción**: `POST /api/v1/admin/cosmetics` — recibe `multipart/form-data` con archivo binario, `CosmeticType`, `DisplayName`. Backend: calcula SHA256, guarda `CosmeticAsset` + `CosmeticAssetVersion`, opcionalmente sube a CDN. Protegido con `[Authorize(Policy = "AdminOnly")]`.

30. - [ ] **Endpoint admin: Editar cosmético**
      - **Prioridad**: Alta
      - **Archivos afectados**: `AdminCosmeticsController.cs`, [NEW] `src/IceBackend.Application/UseCases/AdminCosmetics/UpdateCosmeticUseCase.cs`
      - **Descripción**: `PUT /api/v1/admin/cosmetics/{id}` — modificar `DisplayName`, `CosmeticType`. No permite cambiar el UUID ni el InternalId. Validar que el cosmético existe.

31. - [ ] **Endpoint admin: Eliminar cosmético**
      - **Prioridad**: Alta
      - **Archivos afectados**: `AdminCosmeticsController.cs`, [NEW] `src/IceBackend.Application/UseCases/AdminCosmetics/DeleteCosmeticUseCase.cs`
      - **Descripción**: `DELETE /api/v1/admin/cosmetics/{id}` — elimina lógicamente o físicamente el cosmético. Verificar que ningún jugador tenga ownership activo. Si tiene ownerships activos, rechazar o forzar reasignación.

32. - [ ] **Endpoint público: Listar catálogo de cosméticos**
      - **Prioridad**: Media
      - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/CosmeticsController.cs`
      - **Descripción**: `GET /api/v1/cosmetics` — lista todos los cosméticos disponibles con metadatos básicos (id, type, displayName, thumbnail url). Filtrable por tipo. Sin autenticación requerida (catálogo público).

### Asignación de Cosméticos a Usuarios

33. - [ ] **Endpoint admin: Asignar cosmético a jugador**
      - **Prioridad**: Alta
      - **Archivos afectados**: `AdminCosmeticsController.cs` o [NEW] `src/IceBackend.Api/Controllers/AdminOwnershipController.cs`, [NEW] `src/IceBackend.Application/UseCases/AdminCosmetics/GrantCosmeticUseCase.cs`
      - **Descripción**: `POST /api/v1/admin/ownerships` — recibe `{ playerId, cosmeticId, reason }`. Crea `PlayerCosmeticOwnership` en BD. Purga caché de inventario del jugador. Protegido con `[Authorize(Policy = "AdminOnly")]`.

34. - [ ] **Webhook `checkout.session.completed` de Stripe (compra de cosméticos)**
      - **Prioridad**: Alta
      - **Archivos afectados**: `StripeWebhookService.cs`
      - **Descripción**: Procesar `checkout.session.completed` para crear `PlayerCosmeticOwnership` cuando un jugador compra un cosmético individual. Metadata: `{ playerId, cosmeticId }`. Misma lógica de idempotencia que `invoice.paid`.

35. - [ ] **Endpoint de canje / asignación gratuita**
      - **Prioridad**: Media
      - **Archivos afectados**: [NEW] `src/IceBackend.Api/Controllers/RedeemController.cs`
      - **Descripción**: `POST /api/v1/redeem` — el jugador envía un código promocional y recibe un cosmético. Crea `PlayerCosmeticOwnership` y purga caché. Sin autenticación si es código de invitación, con auth si es canje de recompensa.

---

## 5. Fase 6: Correcciones, Deuda Técnica y Pulido

Tareas enfocadas en cerrar el ciclo de vida de los cosméticos existentes y limpiar inconsistencias acumuladas. Depende de la Fase 5 para tener sentido de negocio completo.

### Bugs Activos
36. - [ ] **Agregar versiones cosméticas con hash al DevSeed**
      - **Prioridad**: Alta (desbloquea request-delivery)
      - **Archivos afectados**: `DevSeedController.cs`
      - **Descripción**: Crear 3 `CosmeticAssetVersion` con SHA256 deterministas para cada cosmético del seed. Esto permite probar el flujo completo: `seed → login → request-delivery → deliver`. Sin versiones, `GetCosmeticIdByHashAsync` siempre retorna null.

37. - [ ] **Endpoint GET callback para OAuth desde navegador**
      - **Prioridad**: Media (testing)
      - **Archivos afectados**: `ExternalAuthController.cs`, `ExternalAuthService.cs`
      - **Descripción**: Agregar `GET /api/auth/external/google/callback` que reciba el redirect de Google directamente (sin launcher), muestre una página HTML con el sessionToken, o renderice un JSON. Permite probar OAuth local sin Electron.

### Deuda Técnica e Inconsistencias

38. - [ ] **Unificar versionado de rutas en todos los controladores**
      - **Prioridad**: Media
      - **Archivos afectados**: `WebhooksController.cs`, `ExternalAuthController.cs`
      - **Descripción**: `WebhooksController` usa `api/[controller]` (token replacement). `ExternalAuthController` usa `api/auth/external` (sin versión). Estandarizar a `api/v1/webhooks` y `api/v1/auth/external/...` para mantener consistencia con el resto.

39. - [ ] **Agregar [Required] y validaciones a DTOs de entrada**
      - **Prioridad**: Media
      - **Archivos afectados**: `AssetDeliveryController.cs` (`RequestDeliveryBody`), `InventoryController.cs` (`EquipRequest`, `UnequipRequest`), `ExternalAuthController.cs` (`ExternalCallbackRequest`)
      - **Descripción**: Varios DTOs carecen de `[Required]` o `[StringLength]`, lo que permite requests inválidas con valores por defecto (Guid.Empty, 0). Agregar data annotations para mejorar Swagger docs y validación temprana.

40. - [ ] **Corregir logging de Serilog en Docker (stdout no visible)**
      - **Prioridad**: Media
      - **Archivos afectados**: `Program.cs`, `appsettings.json`
      - **Descripción**: Las trazas de Serilog (`ILogger<T>`) no aparecen en `docker logs ice_api`. Solo se ve el bootstrap logger. Revisar configuración de sinks y flushing para que los logs estructurados sean visibles en stdout del contenedor.

41. - [ ] **Agregar `[JsonConverter(typeof(JsonStringEnumConverter))]` a enums de DTOs**
      - **Prioridad**: Baja
      - **Archivos afectados**: `InventoryController.cs`, enums compartidos
      - **Descripción**: `CosmeticType` en `EquipRequest` se serializa como entero por defecto. Configurar `JsonStringEnumConverter` global o por propiedad para que Swagger muestre nombres legibles y el cliente pueda enviar `"hat"` en vez de `0`.
