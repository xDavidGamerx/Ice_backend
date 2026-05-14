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
- **Sistema de identidad**: Identity (Premium/ICE UUIDs) (`Confirmado` como objetivo técnico; `No documentado como contrato`)
- **Autenticación OAuth**: Microsoft, Google (`Pendiente de confirmación humana` para alcance exacto; `No documentado como contrato`)
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

## Estado actual de implementación

### Implementado (`Confirmado`)
- [x] Estructura inicial de Clean Architecture.
- [x] Esquema de base de datos definido para Identity y Cosméticos.
- [x] Configuración de EF Core y PostgreSQL.
- [x] Health Check implementado (`No documentado como contrato`: la ruta exacta no debe asumirse por clientes externos sin confirmación).

### Pendiente (`Pendiente de confirmación humana` para detalles)
- [ ] Implementación de proveedores de autenticación (Auth Providers).
- [ ] Implementación de API de entrega de assets (Asset Delivery API).
- [ ] Ejecución de migraciones de base de datos.

---

## Límites de interpretación

- Esta skill documenta el estado técnico actual y las decisiones de arquitectura ya tomadas. No define nuevas decisiones técnicas.
- Las funcionalidades listadas como pendientes son las que están documentadas en el proyecto. No se deben asumir otras funcionalidades no listadas.
- Para entender el contexto de negocio que impulsa estas decisiones técnicas, consulta `ContextSkill.md`.

---

## Relación con otras skills

- **`ContextSkill.md`**: Proporciona el contexto de negocio y la visión del producto que esta implementación técnica debe satisfacer. Debe leerse antes que esta skill si no se comprende el propósito del sistema.
- **`agentsRules.md`**: Define las reglas de cuándo y cómo usar cada skill.

---

## Información pendiente o ambigua

- No se ha documentado si la implementación de OAuth (Microsoft, Google) sigue algún flujo específico no estándar.
- No se ha detallado el modelo de datos completo para la gestión de rangos y sus beneficios a nivel técnico.
- El sistema de Asset Delivery API no tiene especificación pública más allá de su nombre.
