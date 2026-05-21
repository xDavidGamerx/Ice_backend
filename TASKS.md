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

---

## 2. Backlog de Tareas Activas (Priorizado)

Aquí se consolidan y detallan las tareas pendientes del proyecto (incluyendo negocio, calidad y deuda técnica del Informe Kevin) ordenadas por prioridad de ejecución recomendada:

1. - [ ] **Extraer secretos y configurar variables de entorno**
   - **Prioridad**: Alta
   - **Archivos afectados**: `appsettings.json`, `.gitignore`, `docs/local-runbook.md`, [NEW] `.env.example`
   - **Descripción**: Mover connection strings, webhook secrets y llaves criptográficas API a variables de entorno y crear una plantilla de variables de entorno `.env.example`.

2. - [ ] **Crear runbook de ejecución local**
   - **Prioridad**: Alta
   - **Archivos afectados**: [NEW] `docs/local-runbook.md`, `README.md`
   - **Descripción**: Redactar una guía técnica paso a paso con prerequisitos de .NET SDK, PostgreSQL, Redis, comandos del ciclo de vida y depuración local.

3. - [ ] **Implementar flujo de login local para desarrollo**
   - **Prioridad**: Alta
   - **Archivos afectados**: `IceBackend.Api`, `IceBackend.Application`, `IceBackend.Infrastructure`
   - **Descripción**: Crear un endpoint de pruebas o login local (con seeders de prueba) que devuelva un `SessionToken` válido para poder testear Swagger/Postman sin depender del cliente externo.

4. - [ ] **Sincerar y documentar estrategia de sesión en Redis**
   - **Prioridad**: Media
   - **Archivos afectados**: `docs/local-runbook.md`, `.skills/TechnicalContextSkill.md`
   - **Descripción**: Documentar la arquitectura de sesiones Redis (`SessionToken`) descartando formalmente la migración a JWT propuesta por Kevin, ya que la sesión en Redis está plenamente decidida y operativa.

5. - [ ] **Implementar Casos de Uso para Equipar y Desequipar Cosméticos**
   - **Prioridad**: Alta
   - **Archivos afectados**: [NEW] `src/IceBackend.Application/UseCases/Inventory/EquipCosmeticUseCase.cs`, [NEW] `src/IceBackend.Application/UseCases/Inventory/UnequipCosmeticUseCase.cs`, `src/IceBackend.Api/Controllers/InventoryController.cs`
   - **Descripción**: Desarrollar la lógica de negocio para gestionar el equipamiento activo de cosméticos asociados a la cuenta del jugador en Redis (`SADD` / `SREM` sobre sets) y persistir el estado en PostgreSQL.

6. - [ ] **Eliminar el acceso directo a la base de datos desde los controladores**
   - **Prioridad**: Alta
   - **Archivos afectados**: `src/IceBackend.Api/Controllers/AssetDeliveryController.cs`, `src/IceBackend.Application/UseCases/*`
   - **Descripción**: Implementar Casos de Uso (Use Cases) en la capa Application para orquestar la lógica y evitar inyectar o consultar `ApplicationDbContext` directamente en los API Controllers.

7. - [ ] **Fortalecer seguridad del Hashing de Contraseñas**
   - **Prioridad**: Alta
   - **Archivos afectados**: `src/IceBackend.Infrastructure/Services/AuthService.cs`
   - **Descripción**: Sustituir el hashing SHA256 con salt fijo por un algoritmo criptográfico robusto de passwords (como el `PasswordHasher` nativo de ASP.NET Core Identity o BCrypt).

8. - [ ] **Sincronizar esquema de Base de Datos SQL**
   - **Prioridad**: Alta
   - **Archivos afectados**: `docs/database.sql`
   - **Descripción**: Sincronizar el script SQL manual con las migraciones reales de EF Core para asegurar que represente fielmente el estado actual del modelo de datos.

9. - [ ] **Implementar Middleware Global de Manejo de Errores**
   - **Prioridad**: Media-Alta
   - **Archivos afectados**: `src/IceBackend.Api/Middlewares/ExceptionHandlingMiddleware.cs`, `src/IceBackend.Api/Program.cs`
   - **Descripción**: Capturar excepciones de forma centralizada y retornar respuestas uniformes tipo `ProblemDetails` (RFC 7807) en lugar de llenar los controladores con bloques try/catch repetitivos.

10. - [ ] **Configurar sistema de Logs Estructurados (Serilog)**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Api/Program.cs`, `appsettings.json`
    - **Descripción**: Integrar y configurar Serilog para escribir logs estructurados en formato JSON (consola y archivo) que faciliten la auditoría de excepciones y webhooks.

11. - [ ] **Escribir suite de Tests de Integración**
    - **Prioridad**: Media
    - **Archivos afectados**: `tests/IceBackend.IntegrationTests/*`
    - **Descripción**: Desarrollar pruebas integradas de extremo a extremo (usando Testcontainers de Postgres y Redis si es viable) para certificar webhooks de Stripe y flujos de sesión.

12. - [ ] **Evitar anemia del dominio con Value Objects**
    - **Prioridad**: Media
    - **Archivos afectados**: `src/IceBackend.Domain/Entities/*`
    - **Descripción**: Migrar tipos primitivos de negocio hacia Value Objects (como `AssetHash` o `SessionToken`) para validar invariantes de dominio directamente en las entidades.

13. - [ ] **Sistema de Rangos: Definir modelo de datos técnico y beneficios**
    - **Prioridad**: Media-Baja
    - **Archivos afectados**: `src/IceBackend.Domain/Entities/PlayerRange.cs`, `src/IceBackend.Infrastructure/Data/ApplicationDbContext.cs`, `docs/database.sql`
    - **Descripción**: Diseñar e implementar el modelo de datos de rangos, la relación relacional con los jugadores, y la lógica en memoria en Redis para cachear los beneficios activos del rango del jugador.
