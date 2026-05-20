# Backlog Técnico y Hoja de Ruta

Este documento contiene la lista de tareas pendientes del proyecto, organizadas por prioridad según los mandatos de la Arquitecta (Decisión: Cambios Requeridos).

## 1. Estabilidad Base e Integridad
- [x] **Corregir Contrato de Autenticación**: Sincronizar `AuthService` con `IAuthService` para resolver el error de compilación. (Completado).
- [x] **Certificación Matemática (Tests)**: Crear proyecto de pruebas y certificar el aislamiento de UUIDs y lógica de sesión antes de integrar Redis. (Completado: 5/5 tests pasados).
- [x] **Sincronización de Base de Datos**: Ejecutar `dotnet ef database update` SOLO cuando el proyecto compile a cero errores. (Completado).

## 2. Autenticación, Seguridad e Infraestructura Asíncrona
- [x] **Configuración de Redis**: Añadir conexión en `appsettings.json` y configurar `AddStackExchangeRedisCache`. (Completado).
- [x] **Gestión de Sesiones**: Implementar persistencia de `SessionToken` en Redis con abstracción `ISessionCache`. (Completado).
- [x] **Infraestructura de Webhooks Idempotentes (Stripe)**:
    - [x] **Middleware de Validación**: Implementar verificación de firmas criptográficas (Stripe-Signature).
    - [x] **Barrera de Idempotencia Híbrida**: Implementar check rápido en Redis y persistencia única en la tabla `PaymentEvent`.
    - [x] **Event Handlers Transaccionales**: Lógica para `invoice.paid`, `subscription.deleted` con purga automática de caché de usuario.
- [x] **Proveedores Externos**: Implementar flujo OAuth2 PKCE para Microsoft y Google con doble barrera anti-CSRF y anti-replay. (Completado).

## 3. Sistema de Cosméticos y Negocio
- [x] **Modelo de Almacenamiento CAS**:
    - [x] **Redirección CDN**: Implementar `Asset Delivery API` que retorne redirecciones (HTTP 302) o URLs prefirmadas (ej. S3), nunca archivos binarios directamente. (Completado).
    - [x] **Rutas por SHA256**: Los endpoints deben identificar y servir archivos exclusivamente por su hash SHA256 (ej. `/assets/{sha256}`). (Completado).
- [x] **Gestión de Metadatos (Legacy vs Modern)**: 
    - [x] Extender el modelo de `CosmeticAsset` para incluir metadatos inmutables de versión y motor (Legacy 1.8.9 vs Modern Fabric/Forge). (Completado).
- [x] **Auditoría e Integridad de Asset Delivery API (Fase 3)**:
    - [x] **Seguridad de Borde (Asset Tokens)**: Crear `AssetTokenService` con tokens efímeros firmados por `HMACSHA256` y salt/pepper por jugador para la descarga de recursos.
    - [x] **Autenticación Unificada**: Implementar `SessionTokenAuthenticationHandler` personalizado para integrar `[Authorize]` con Redis de forma nativa.
    - [x] **Resiliencia de Webhooks**: Crear tabla `unresolved_payment_events` en PostgreSQL y refactorizar `StripeWebhookService` para webhooks huérfanos sin violar la clave foránea.
- [ ] **Gestión de Inventario**: Implementar Use Cases para equipar y desequipar cosméticos.

## 4. Calidad Continua
- [ ] **Implementar Logging**: Configurar sistema de logs estructurado.
- [ ] **Sistema de Rangos**: Definir modelo de datos técnico y beneficios.
