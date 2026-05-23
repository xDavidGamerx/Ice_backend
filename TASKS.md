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

4. - [ ] **Sincerar y documentar estrategia de sesión en Redis**
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

10. - [ ] **Configurar sistema de Logs Estructurados (Serilog)**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Api/Program.cs`, `appsettings.json`
    - **Descripción**: Integrar y configurar Serilog para escribir logs estructurados en formato JSON (consola y archivo) que faciliten la auditoría de excepciones y webhooks.

11. - [ ] **Escribir suite de Tests de Integración**
    - **Prioridad**: Media
    - **Archivos afectados**: `tests/IceBackend.IntegrationTests/*`
    - **Descripción**: Desarrollar pruebas integradas de extremo a extremo (usando Testcontainers de Postgres y Redis si es viable) para certificar webhooks de Stripe y flujos de sesión.

12. - [x] **Evitar anemia del dominio con Value Objects / Encapsulamiento**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Domain/Entities/*`
    - **Descripción**: Se encapsuló todo el estado mutable (`private set`) y se requiere uso de métodos expresivos o constructores para proteger las invariantes del dominio.

13. - [x] **Suscripción ICE+: Definir modelo de datos técnico y beneficios**
     - **Prioridad**: Media-Baja
     - **Archivos afectados**: `src/IceBackend.Domain/Entities/PlayerSubscription.cs`, `src/IceBackend.Infrastructure/Data/ApplicationDbContext.cs`, `docs/database.sql`
     - **Descripción**: Diseñar e implementar el modelo de datos para la suscripción ICE+ (estilo Lunar+), registrando el estado activo, expiración, renovación y un contador acumulativo de meses para los iconos evolutivos, persistiendo los beneficios y cargándolos en caché (Redis) mediante Cache-Aside.

14. - [ ] **Configurar CORS (Cross-Origin Resource Sharing)**
     - **Prioridad**: Media
     - **Archivos afectados**: `src/IceBackend.Api/Program.cs`, `appsettings.json`
     - **Descripción**: Registrar y configurar políticas de CORS en el pipeline de ASP.NET Core que permitan al launcher (origen del cliente Electron) consumir la API, y a `localhost` para desarrollo. Debe ser restrictivo: solo orígenes explícitamente listados, sin permitir `AllowAnyOrigin()` en producción.
