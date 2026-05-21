# Registro de Cambios y Decisiones Técnicas (CHANGELOG)

Este archivo registra las modificaciones importantes, correcciones de errores y nuevas funcionalidades implementadas en el proyecto, junto con su justificación técnica.

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
