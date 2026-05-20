# TechnicalContextSkill — Contexto Técnico del Proyecto ICE Backend

**Estado**: `Confirmado` (solo en lo que está documentado en este archivo)

**Alcance del repositorio (Confirmado)**: este repositorio (`ice_backend`) contiene el backend/API para el producto **ICE Launcher**. No incluye el launcher/cliente.

**No documentado como contrato**: cualquier mención a endpoints, APIs o nombres de módulos aquí debe tratarse como **referencia interna** o **estado actual documentado**, no como contrato estable para clientes externos.

## Propósito de esta skill

Esta skill contiene la información técnica documentada del proyecto ICE Backend: tecnologías, arquitectura, estructura de carpetas, guías de desarrollo y estado actual de implementación. Está diseñada para que cualquier agente o desarrollador comprenda la base técnica sobre la que se construye el sistema.

No contiene contexto de negocio, visión del producto ni lógica funcional. Para eso, consulta `ContextSkill.md`.

---

## Cuándo debe usarse

- Cuando se necesite entender la arquitectura técnica del proyecto.
- Antes de modificar código, agregar dependencias o cambiar la estructura del proyecto.
- Para conocer las convenciones de desarrollo establecidas.
- Para verificar el estado actual de implementación.
- Cuando se trabaje con la base de datos, entidades o infraestructura.

---

## Qué información contiene

- Stack tecnológico del proyecto.
- Arquitectura y estructura de carpetas.
- Guías de desarrollo y convenciones.
- Estado actual de funcionalidades implementadas y pendientes.

---

## Stack tecnológico

- **Runtime**: .NET 8.0 (`Confirmado`)
- **Base de datos**: PostgreSQL (`Confirmado`)
- **Arquitectura**: Clean Architecture (capas: Api, Application, Domain, Infrastructure) (`Confirmado`)
- **Caché/Sesiones**: Redis (StackExchange.Redis para Sets y SISMEMBER) (`Confirmado`)
- **Sistema de identidad**: Identity (Premium/ICE UUIDs) (`Confirmado` como objetivo técnico)
- **Autenticación OAuth**: Microsoft, Google (`Confirmado` - Flujo Authorization Code con PKCE)
- **Sistema de cosméticos**: Sombreros, Alas, Capas, etc. (`Pendiente de confirmación humana` para lista exacta)
- **Gestión de assets**: Verificación SHA256 (`Pendiente de confirmación humana` para comportamiento exacto)

---

## Estructura del proyecto

```
src/
├── IceBackend.Api          # Punto de entrada y Controllers
├── IceBackend.Application  # Lógica de negocio e interfaces
├── IceBackend.Domain       # Entidades y value objects
├── IceBackend.Infrastructure # Acceso a datos y servicios externos
docs/
├── database.sql            # Esquema actual de base de datos
```

---

## Guías de desarrollo

1. **Pureza**: Mantener la capa Domain libre de dependencias externas.
2. **Persistencia de esquema**: Todos los cambios en base de datos deben reflejarse en `docs/database.sql`.
3. **Seguridad**: Manejar hashes de contraseñas y tokens con cuidado (SHA256/hashing de sesión).
4. **Consistencia**: Usar las convenciones de nomenclatura establecidas para entidades y assets.

---

## Estado de los componentes técnicos

*   **Arquitectura Base**: Clean Architecture estructurada con API, Application, Domain e Infrastructure (`Confirmado`).
*   **Base de Datos**: PostgreSQL para almacenamiento persistente con esquemas e índices migrados en desarrollo (`Confirmado`).
*   **Caché y Sesiones**: Redis operativo para la validación O(1) de inventario mediante Sets (`SISMEMBER`) y almacenamiento temporal de sesiones (`Confirmado`).
*   **Autenticación**:
    *   Sesiones unificadas integradas con `[Authorize]` a través de `SessionTokenAuthenticationHandler` (`Confirmado`).
    *   Flujo OAuth2 PKCE para Microsoft/Google implementado para sincronización de identidades externas (`Confirmado`).
*   **Entrega de Activos**: Asset Delivery API con Just-in-Time delivery, tokens efímeros firmados con HMAC-SHA256 y redirección temporal 307 al CDN (`Confirmado`).
*   **Auditoría**: Sistema de auditoría en la tabla `unresolved_payment_events` para webhooks de pago huérfanos en Stripe (`Confirmado`).
*   **Rangos e Inventario**: Lógica técnica para equipar cosméticos y modelo técnico de beneficios de rangos (`Pendiente de definición técnica y confirmación humana`).

---

## Límites de interpretación

- Este documento documenta el estado técnico actual y las decisiones de arquitectura ya tomadas. No define nuevas decisiones técnicas.
- Las funcionalidades listadas como pendientes son las que están documentadas en el proyecto. No se deben asumir otras funcionalidades no listadas.
- Para entender el contexto de negocio que impulsa estas decisiones técnicas, consulta `ContextSkill.md`.

---

## Relación con otras skills

- **`ContextSkill.md`**: Proporciona el contexto de negocio y la visión del producto que esta implementación técnica debe satisfacer. Debe leerse antes que esta skill si no se comprende el propósito del sistema.
- **`agentsRules.md`**: Define las reglas de cuándo y cómo usar cada skill.

---

## Información pendiente o ambigua

- No se ha detallado el modelo de datos final ni las implicaciones funcionales para la gestión técnica de Rangos y sus beneficios.
