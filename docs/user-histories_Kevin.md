# Historias de Usuario — ICE Launcher Backend

> **Nota**: Este documento contiene las **HU faltantes** identificadas tras analizar el repositorio al 100%. Las HU ya implementadas no se listan aquí; están implícitas en los endpoints y lógica existente (ver `README.md` y `TASKS.md`).

---

## Convenciones

| Prioridad | Significado |
|-----------|-------------|
| **Crítica** | Bloqueante para producción. Sin esto el sistema no es usable en serio |
| **Alta** | Necesaria para completar un flujo de negocio |
| **Media** | Mejora significativa de experiencia o funcionalidad |
| **Baja** | Pulido, deuda técnica menor |

---

## Módulo 1: Autenticación y Cuentas

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F1 | Usuario | **recuperar mi contraseña** (forgot/reset password) | poder acceder a mi cuenta si olvido mis credenciales | **Alta** |
| HU-F2 | Usuario | **ver y editar mi perfil** (username, display name, email) | mantener mi información personal actualizada | **Alta** |
| HU-F3 | Usuario | **cambiar mi contraseña** desde mi perfil | mejorar la seguridad de mi cuenta | **Alta** |
| HU-F4 | Usuario con cuenta Microsoft | que el sistema **detecte automáticamente si tengo Minecraft Premium** al vincular mi cuenta | acceder a funciones exclusivas para usuarios premium | **Media** |
| HU-F5 | Usuario | **vincular/desvincular cuentas externas** (Microsoft, Google) después del registro inicial | gestionar mis identidades vinculadas | **Media** |
| HU-F6 | Usuario | **eliminar mi cuenta** permanentemente del sistema | darme de baja del ecosistema | **Media** |
| HU-F7 | Usuario | **ver mi historial de sesiones activas y dispositivos** | saber dónde estoy logueado y cerrar sesiones remotas | **Baja** |

---

## Módulo 2: Roles y Administración

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F8 | **Admin** | que exista un **flag/rol de administrador** en mi cuenta | gestionar el sistema con privilegios especiales | **Crítica** |
| HU-F9 | **Admin** | que los endpoints de gestión **requieran autorización explícita de admin** | evitar que usuarios regulares accedan a funciones sensibles | **Crítica** |

---

## Módulo 3: Catálogo y CRUD de Cosméticos

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F10 | **Admin** | **crear cosméticos nuevos** (con archivo, tipo, nombre y metadatos) | poblar el catálogo de la tienda | **Crítica** |
| HU-F11 | **Admin** | **editar cosméticos existentes** (nombre, tipo, metadatos) | mantener el catálogo actualizado | **Alta** |
| HU-F12 | **Admin** | **eliminar cosméticos** del catálogo (con validación de ownerships activos) | retirar productos obsoletos | **Alta** |
| HU-F13 | Usuario | **ver el catálogo público de cosméticos** con filtros por tipo | saber qué cosméticos existen y cuáles puedo obtener | **Alta** |

---

## Módulo 4: Inventario y Cosméticos del Jugador

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F14 | Usuario | **ver mi inventario completo de cosméticos** (qué tengo equipado y qué no) | saber qué artículos poseo | **Alta** |
| HU-F15 | **Admin** | **asignar un cosmético a un jugador** (grant) especificando motivo | regalar/recompensar a usuarios | **Alta** |
| HU-F16 | Usuario | **comprar un cosmético individual** mediante Stripe (checkout) | adquirir artículos que me gustan | **Alta** |
| HU-F17 | Usuario | **canjear un código promocional** para recibir un cosmético gratis | obtener recompensas promocionales | **Media** |

---

## Módulo 5: Suscripción ICE+

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F18 | Usuario | **iniciar la compra de ICE+** desde el backend/launcher (crear sesión de checkout) | suscribirme al servicio premium | **Alta** |
| HU-F19 | Usuario | **cancelar mi suscripción ICE+** (desactivar renovación automática) | no seguir pagando si ya no me interesa | **Alta** |
| HU-F20 | Usuario | **cambiar de plan de suscripción** (si hay múltiples niveles) | ajustar mi suscripción a mis necesidades | **Media** |
| HU-F21 | Usuario | **ver mi historial de pagos** de la suscripción ICE+ | llevar un control de mis gastos | **Media** |
| HU-F22 | Usuario con ICE+ activo | **recibir mi emote mensual automáticamente** al inicio de cada ciclo de facturación | obtener el beneficio recurrente de mi membresía | **Media** |

---

## Módulo 6: Entrega de Activos (CDN)

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F23 | **Desarrollador** | que el **DevSeed cree versiones cosméticas con hash SHA256** | poder probar el flujo completo seed → login → request-delivery → deliver | **Alta** |

---

## Módulo 7: OAuth (Testing y Mantenimiento)

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F24 | **Desarrollador** | un **callback GET de OAuth** que muestre el sessionToken en HTML/JSON | probar OAuth localmente sin necesidad del launcher Electron | **Media** |

---

## Módulo 8: Calidad, Deuda Técnica y Operaciones

| ID | Como... | Quiero... | Para... | Prioridad |
|----|---------|-----------|---------|-----------|
| HU-F25 | **Desarrollador/Operador** | que los **logs de Serilog sean visibles en Docker stdout** (`docker logs`) | diagnosticar la API en producción/contenedor | **Media** |
| HU-F26 | **Desarrollador/API Consumer** | que los **enums en las respuestas JSON se serialicen como strings** | mejorar la legibilidad de la API (evitar números mágicos) | **Baja** |
| HU-F27 | **Desarrollador/API Consumer** | que **todas las rutas usen versionado consistente** (`/api/v1/...`) | mantener una convención REST uniforme en toda la API | **Media** |
| HU-F28 | **Desarrollador/API Consumer** | que los **DTOs de entrada tengan validaciones `[Required]` y `[StringLength]`** | recibir errores tempranos y claros al enviar datos inválidos | **Media** |

---

## Resumen por Módulo

| Módulo | Implementadas | Faltantes | % Completitud |
|--------|:------------:|:---------:|:-------------:|
| 1. Autenticación y Cuentas | 6 HU | 7 HU | ~46% |
| 2. Roles y Administración | 0 HU | 2 HU | **0%** |
| 3. Catálogo y CRUD de Cosméticos | 0 HU | 4 HU | **0%** |
| 4. Inventario y Cosméticos del Jugador | 2 HU | 4 HU | ~33% |
| 5. Suscripción ICE+ | 4 HU | 5 HU | ~44% |
| 6. Entrega de Activos (CDN) | 2 HU | 1 HU | ~67% |
| 7. OAuth (Testing) | 0 HU | 1 HU | **0%** |
| 8. Calidad y Deuda Técnica | 0 HU | 4 HU | **0%** |
| **Total** | **14 HU** | **28 HU** | **~33%** |

---

## Prioridad de Implementación Recomendada

| Orden | HU | Módulo | Depende de |
|:-----:|:--:|--------|------------|
| 1 | HU-F8, HU-F9 | Roles Admin | Ninguna |
| 2 | HU-F10, HU-F11, HU-F12 | CRUD Cosméticos | HU-F8, HU-F9 |
| 3 | HU-F13 | Catálogo público | HU-F10 |
| 4 | HU-F15 | Grant de cosméticos | HU-F8, HU-F9, HU-F10 |
| 5 | HU-F16, HU-F17 | Compra/Canje | HU-F10, HU-F13 |
| 6 | HU-F2, HU-F3 | Perfil de usuario | Ninguna |
| 7 | HU-F18, HU-F19 | Compra/Cancelación ICE+ | Ninguna |
| 8 | HU-F14 | Inventario propio | Ninguna |
| 9 | HU-F1 | Recuperar contraseña | Ninguna |
| 10 | HU-F23 | DevSeed hashes | Ninguna |
| 11 | HU-F4 | Minecraft Premium detection | HU-F2 |
| 12 | HU-F5, HU-F6 | Vincular/Eliminar cuenta | HU-F2 |
| 13 | HU-F20, HU-F21, HU-F22 | Mejoras ICE+ | HU-F18 |
| 14 | HU-F24, HU-F25, HU-F26, HU-F27, HU-F28 | Calidad/Deuda técnica | Ninguna |
| 15 | HU-F7 | Sesiones activas | Ninguna |
