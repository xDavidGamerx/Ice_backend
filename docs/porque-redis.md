# Justificación Técnica: Uso de Redis en IceBackend

Este documento detalla los motivos arquitectónicos y de rendimiento por los cuales **Redis** es una pieza fundamental en el backend de **ICE Launcher**, respondiendo a dudas comunes sobre por qué no delegar toda la lógica a una base de datos relacional (PostgreSQL).

---

## 1. Validación O(1) de Inventario (Cache-Aside)
**El caso de uso:** Cuando un cliente del launcher solicita un cosmético o un activo virtual, el endpoint de entrega debe verificar de forma inmediata si el usuario es propietario del mismo.

*   **El problema relacional (PostgreSQL):** Si el servidor tuviera que ejecutar una consulta SQL (`SELECT`, `JOIN`) contra la base de datos persistente en cada petición de descarga de activos de miles de jugadores concurrentes, la base de datos saturaría sus conexiones y latencia de disco, degradando la experiencia en el juego.
*   **La solución con Redis:** Redis funciona en memoria RAM. Guardamos el inventario de cosméticos del jugador usando un **Set nativo** (`inventory:player:{id}`). Mediante el comando atómico `SISMEMBER`, determinamos en tiempo constante $O(1)$ si el usuario posee el cosmético con una latencia de microsegundos, aislando a PostgreSQL de esta carga.

---

## 2. Barrera de Idempotencia para Webhooks de Stripe
**El caso de uso:** Stripe nos notifica de eventos de facturación y compras (como `invoice.paid`) asíncronamente. Por diseño de red, Stripe puede enviar el mismo evento múltiples veces si hay micro-cortes o demoras en la respuesta HTTP.

*   **El problema relacional (PostgreSQL):** Intentar insertar directamente el evento duplicado en PostgreSQL puede generar excepciones de llave única (provocando HTTP 500 y forzando a Stripe a seguir reintentando) o, peor aún, duplicar el otorgamiento del cosmético o rango en la lógica de negocio.
*   **La solución con Redis:** Redis se utiliza como una **barrera de primera línea**. Al recibir el webhook, intentamos registrar de forma atómica el ID del evento usando operaciones de tipo *set-if-not-exists* (`SETNX`) con un tiempo de expiración (TTL). Si el ID ya existe en Redis, el webhook duplicado se descarta inmediatamente antes de realizar transacciones pesadas en la base de datos.

---

## 3. Revocación Instantánea de Sesiones y Permisos (TTL)
**El caso de uso:** Las sesiones de los usuarios y sus permisos de descarga deben ser altamente dinámicos. Si a un usuario se le cancela la suscripción (`customer.subscription.deleted`) o es suspendido, debe perder el acceso de inmediato.

*   **El problema con JWT estándar:** Los tokens JWT tradicionales son autónomos y no se pueden revocar de forma inmediata antes de su fecha de expiración, a menos que se mantenga una base de datos de tokens inválidos (lista negra), lo que anula la ventaja de que JWT sea "stateless".
*   **La solución con Redis:** Administramos los tokens de sesión en Redis con un TTL (Time-To-Live) controlado. Si ocurre un evento de revocación o baneo, el backend ejecuta un comando atómico `DEL` sobre la clave de la sesión y el inventario del usuario en Redis. El acceso queda revocado inmediatamente en la siguiente solicitud del cliente.

---

## ¿Por qué elegir Redis frente a otras alternativas?

1.  **Estructuras de Datos Nativas:** A diferencia de sistemas de caché simples como *Memcached* (que solo admiten cadenas clave-valor planos), Redis soporta **Sets** (`SISMEMBER`), lo que nos permite manipular el inventario directamente en memoria sin serializar/deserializar objetos JSON completos.
2.  **Operaciones Atómicas:** Garantiza que las comprobaciones de seguridad y lógica de exclusión mutua se realicen en una sola instrucción de CPU.
3.  **Persistencia Configurable (AOF/RDB):** Al tener configurada la persistencia AOF (Append-Only File), si el servidor Redis se reinicia por mantenimiento, el estado de las sesiones y la caché de inventario se recuperan del disco en segundos, evitando sobrecargar a PostgreSQL con re-hidrataciones masivas de datos.
