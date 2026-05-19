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
- [ ] **API de Assets**: Desarrollar endpoints para entrega de cosméticos (Asset Delivery API).
- [ ] **Validación SHA256**: Implementar verificación de integridad de archivos de assets.
- [ ] **Gestión de Inventario**: Implementar Use Cases para equipar y desequipar cosméticos.

## 4. Calidad Continua
- [ ] **Implementar Logging**: Configurar sistema de logs estructurado.
- [ ] **Sistema de Rangos**: Definir modelo de datos técnico y beneficios.