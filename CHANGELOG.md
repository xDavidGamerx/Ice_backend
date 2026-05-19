# Registro de Cambios y Decisiones Técnicas (CHANGELOG)

Este archivo registra las modificaciones importantes, correcciones de errores y nuevas funcionalidades implementadas en el proyecto, junto con su justificación técnica.

## [2026-05-18] - Fase de Estabilidad y Sesiones

### Añadido
- **Infraestructura de Pruebas**: Creado proyecto `IceBackend.UnitTests` con xUnit para certificación de lógica core.
- **Integración con Redis**: Implementado `IDistributedCache` mediante `StackExchangeRedisCache`.
- **Abstracción de Caché**: Interfaz `ISessionCache` en Application e implementación en Infrastructure para mantener la pureza de capas.
- **Seguridad de Sesiones**: Generación de tokens criptográficamente seguros en el servidor y persistencia en Redis con TTL configurable.

### Corregido
- **Desincronización de Contratos**: Sincronizada la firma de `LoginIceAccountAsync` entre `IAuthService` y `AuthService`, restaurando la compilación del proyecto.
- **Validación de Identidad**: Asegurado que los UUIDs de cuentas ICE se generen exclusivamente en el servidor, mitigando riesgos de inyección de identidad desde el cliente.

### Técnico
- Aplicada migración inicial de PostgreSQL.
- Verificadas 9/9 pruebas unitarias exitosas.
