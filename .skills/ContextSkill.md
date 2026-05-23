# ContextSkill — Guía de Contexto del Negocio ICE Launcher

**Estado**: `Confirmado` (contexto vigente de negocio para este repositorio)

**Alcance del repositorio (Confirmado)**: este repo (`ice_backend`) contiene **solo** el backend/API consumido por ICE Launcher. El launcher/cliente **no** está dentro de este repositorio.

## Propósito de esta skill

Esta skill representa la comprensión más profunda del negocio, la intención del producto y la visión funcional humana del ecosistema ICE Launcher. No contiene detalles técnicos, arquitectura, código, endpoints, bases de datos ni implementaciones. Su propósito es servir como fuente única de verdad del contexto del negocio para cualquier persona o agente que necesite entender, discutir, documentar o tomar decisiones sobre el producto.

Leer esta skill es el primer paso obligatorio antes de proponer cualquier cambio, diseñar flujos, escribir documentación o tomar decisiones relacionadas con el producto.

---

## Resumen general del negocio

ICE Launcher es un ecosistema digital centrado en un launcher de Minecraft. El launcher existe como producto (cliente) y consume servicios desde un backend.

El proyecto busca que ICE Launcher evolucione de ser simplemente un launcher técnico para abrir Minecraft a convertirse en una plataforma de experiencia alrededor del usuario. Una plataforma con identidad propia, donde el usuario tenga cuenta, presencia, personalización, historial y pertenencia a un sistema más grande.

No se trata únicamente de tener autenticación y una tienda. Se trata de construir una base sólida para la identidad del usuario, la personalización, la monetización y la futura expansión del ecosistema.

---

## Problema que busca resolver

El problema central es que actualmente existe un launcher de Minecraft funcional, pero falta una capa central que organice, conecte y dé coherencia a toda la experiencia del usuario dentro del ecosistema. Sin esa capa:

- No hay una identidad de usuario persistente y reconocible.
- No hay una forma clara de asociar productos virtuales a un usuario.
- No hay diferenciación entre tipos de cuenta (ICE vs Microsoft).
- No hay una fuente central de verdad que el launcher, la tienda web y otros canales puedan consultar.
- No hay un modelo comercial estructurado que permita vender cosméticos y membresías ICE+ de forma coherente.
- La experiencia del usuario se siente fragmentada y sin continuidad.

El sistema busca resolver exactamente eso: construir la base que convierta un launcher aislado en una plataforma conectada, con identidad, personalización, monetización y potencial de crecimiento.

---

## Visión del sistema

ICE Launcher debe pensarse como una plataforma de experiencia, no como una herramienta técnica. El launcher no solo inicia sesión o ejecuta un juego: también representa identidad, progreso, apariencia, acceso a contenido, beneficios y posibilidad de compra.

La visión es que el ecosistema se sienta como un solo producto, aunque tenga varios puntos de acceso. El launcher, la cuenta del usuario y la tienda no deben existir como piezas aisladas. Deben formar parte de una misma experiencia conectada.

El backend no es un simple acompañante del launcher. Es el sistema que organiza toda la lógica del ecosistema. Centraliza el contexto del usuario y lo convierte en información útil para el launcher y para cualquier otro canal. Es la capa que convierte la idea comercial y funcional en una plataforma coherente. Es la fuente principal de verdad del ecosistema.

La visión final es que ICE Launcher se convierta en una plataforma con identidad propia, capaz de sostener:

- Una base de usuarios persistente.
- Experiencias diferenciadas según el tipo de cuenta.
- Un modelo de monetización mediante productos virtuales.
- Expansión futura a nuevos canales y funcionalidades.

---

## Cómo se imagina el funcionamiento

### Canales del ecosistema

El sistema está pensado para operar a través de varios canales que comparten una misma lógica central:

1. **Launcher de escritorio**: Es el canal principal. Está construido con Electron y consume información del backend para mostrar el estado del usuario, su perfil, su tipo de cuenta, sus cosméticos disponibles, su membresía ICE+ activa, su información comercial y cualquier otra funcionalidad asociada a su cuenta. El launcher actúa como una extensión viva de la cuenta del usuario.

2. **Tienda web**: Es un canal complementario. Su objetivo es mostrar productos, permitir compras y ofrecer acceso a información comercial del ecosistema sin obligar al usuario a pasar exclusivamente por el launcher. La tienda no es algo separado, sino una dimensión comercial del mismo ecosistema.

3. **Posible panel de gestión**: Aunque no es el foco inmediato, se visualiza que el sistema necesitará eventualmente un espacio de administración para gestionar productos, usuarios, beneficios, estados de compra y configuración general del ecosistema.

### Identidad del usuario

El sistema maneja dos tipos de identidad que conviven:

- **Cuenta ICE**: Es la identidad principal dentro del ecosistema. Sirve para reconocer al usuario, darle persistencia, asociarle productos, guardar su estado y construir una relación comercial con él. La cuenta ICE debe existir incluso para usuarios que no tengan una cuenta premium de Minecraft.

- **Cuenta Microsoft / Minecraft**: Es una identidad externa que se puede vincular a la cuenta ICE. Aporta contexto adicional del usuario dentro del mundo premium de Minecraft, como su identidad premium, su perfil y potencialmente información visual o de personalización. No reemplaza a la cuenta ICE, sino que la complementa.

### Tipos de usuario

El sistema reconoce dos escenarios:

1. **Usuario ICE no premium**: Entra al ecosistema con su cuenta ICE y usa las funcionalidades de la plataforma sin depender de una cuenta premium de Minecraft. Puede tener perfil, historial, compras, inventario y acceso a productos.

2. **Usuario ICE con cuenta Microsoft vinculada**: Además de su cuenta ICE, vincula su cuenta Microsoft/Minecraft premium. Su experiencia se enriquece con el contexto premium y se siente más integrada con el universo de Minecraft.

La plataforma debe soportar ambos escenarios sin romper la lógica de negocio ni forzar que todos los usuarios sean tratados de la misma forma.

### Productos del ecosistema

El modelo comercial contempla dos grandes categorías de productos virtuales:

1. **Cosméticos**: Son elementos visuales o de personalización. Incluyen capas, animaciones y posiblemente otros elementos visuales que se definan más adelante. Su propósito es aportar diferenciación visual y valor estético. No están pensados como ventajas funcionales d2. **Suscripción ICE+ (Premium - Estilo Lunar+)**: ICE+ es una suscripción mensual, periódica o de pago único diseñada para mejorar integralmente la experiencia del usuario dentro del Launcher y el ecosistema. No representa privilegios de servidor tradicionales, sino una membresía estética y funcional global.

La diferencia fundamental es: los cosméticos son elementos de personalización visual individuales que se compran por separado, mientras que ICE+ es una suscripción recurrente que otorga un paquete de beneficios activos y privilegios exclusivos globales.

---

## Experiencia esperada para el usuario

Un usuario entra al ecosistema ICE Launcher con su cuenta ICE. Si tiene una cuenta Microsoft premium, puede vincularla.

Al ingresar, el Launcher detecta si el usuario tiene una suscripción ICE+ activa. Si está activa, el Launcher desbloquea inmediatamente toda la gama de características premium:
- Desbloqueo y renderizado de cosméticos exclusivos de ICE+ en su inventario.
- Icono evolutivo de ICE+ al lado de su nombre, reflejando el color correspondiente a sus meses de suscripción acumulados.
- Activación de la física de movimiento en sus capas (Cloth Cloaks).
- Interfaz del Launcher sin anuncios de publicidad.
- Posibilidad de añadir amigos sin límites.
- Aplicación de un 10% de descuento automático si decide comprar otros cosméticos individuales en la tienda web.

Si la suscripción expira o es cancelada, el Launcher bloquea los accesos, oculta el icono de ICE+ y revoca el uso de cosméticos exclusivos, pero conserva intacto el registro de "meses acumulados" para que, en caso de re-suscripción futura, el usuario retome su nivel de icono evolutivo donde lo dejó.

---

## Lógica y Reglas de Negocio

Del contexto se desprenden las siguientes reglas estrictas de negocio:

1.  **Identidad Raíz**: Todo usuario posee una cuenta ICE como identidad base persistente en el sistema. La cuenta de Microsoft es un complemento opcional.
2.  **Diferenciación de Cuentas**: La plataforma debe identificar y dar soporte tanto a usuarios solo-ICE (no premium) como a usuarios con cuenta Microsoft vinculada.
3.  **Segregación de Catálogo**: Los cosméticos individuales y la suscripción ICE+ son categorías distintas con lógicas de negocio separadas que no deben mezclarse.
4.  **Vigencia e Inventario**: Los beneficios de ICE+ solo están vigentes mientras la suscripción esté activa, con excepción del contador de meses acumulados.
5.  **Acumulación de Tiempo (Icono Evolutivo)**: Cada mes en que el usuario pague e inicie su ciclo de suscripción, se incrementa en uno el contador de meses acumulados. Este contador es persistente, acumulativo e irreversible; no se reinicia a cero si la suscripción se cancela temporalmente.
6.  **Descuento Coherente**: El 10% de descuento en la tienda web requiere verificar en el backend que el usuario tiene la suscripción ICE+ activa en el momento de calcular el precio del checkout.
7.  **Compatibilidad Universal de Versiones (Soporte Multi-versión)**: El Launcher da soporte completo a la ejecución de todas las versiones de Minecraft. El backend debe discriminar entre la arquitectura Legacy (modelos planos/OBJ en 1.8.9) y Modern (BBMODEL/GeckoLib en versiones más recientes) para los cosméticos exclusivos de ICE+.

---

## Principios del Producto

-   **Experiencia Unificada (Omnicanalidad)**: La experiencia debe ser coherente en todos los canales. La tienda web y el launcher leen el estado de la suscripción de la misma fuente de verdad en Redis/PostgreSQL.
-   **El Launcher como Eje Central**: Es la cara principal del ecosistema, operando como una extensión visual interactiva del perfil y pertenencias del usuario.
-   **Diseño Extensible**: La plataforma se concibe como un sistema preparado para la expansión comercial continua sin alterar la infraestructura existente.

---

## Límites de interpretación

- Este documento no define cómo se implementa técnicamente nada. No habla de endpoints, APIs, bases de datos, ni variables de entorno.
- No se debe asumir que el sistema tiene funcionalidades que no están descritas aquí.
- La existencia de un backend es conceptual. Este documento describe qué debe hacer, no cómo debe construirse.

---

## Lo que no debe asumirse

- No se debe asumir que todos los usuarios tienen cuenta premium de Minecraft o de Microsoft.
- No se debe asumir que el sistema ya tiene un portal de Discord completamente automatizado o bots configurados.
- No se debe asumir que el sistema ya tiene la tienda web totalmente integrada. Es una posibilidad futura.
- No se debe asumir que los cosméticos de ICE+ no pueden cambiar de catálogo. Son exclusividades pero pueden variar.

---

## Ambigüedades detectadas

1. **La entrega del Emote Mensual**: Se menciona que al inicio de cada mes se entrega un emote aleatorio. Queda pendiente definir técnicamente si esto se hace mediante un proceso en segundo plano (cron job) en base de datos al inicio de mes cronológico, o bien al momento del cobro del ciclo recurrente de cada usuario en Stripe webhook.
2. **Nivel de integración de Discord**: No se especifica cómo el backend informará al bot de Discord sobre el estado de la suscripción para asignar roles.

---

## Información pendiente por definir

1. **Lista final de cosméticos exclusivos de ICE+**: Se mencionan capas, mochilas, bandannas y el emote "Default Dance", pero los IDs y recursos visuales están por definir.
2. **Definición de los 15 colores del Icono Evolutivo**: Los hitos exactos de meses acumulados para cada una de las 15 variantes de colores del icono rosa con signo más verde.

---

## Resumen operativo para futuros agentes

ICE Launcher es un ecosistema digital centrado en un launcher de Minecraft. El producto estrella de monetización recurrente es la suscripción premium **ICE+** (estilo Lunar+).

El sistema debe permitir que un usuario con cuenta ICE adquiera una suscripción ICE+ (con renovación mensual o pago único). Mientras la suscripción esté activa, el usuario goza de cosméticos exclusivos, físicas en capas, lista de amigos ilimitada, launcher sin anuncios, 10% de descuento en tienda y un icono evolutivo en el Launcher que cambia de color según los meses acumulados que tenga la cuenta (los meses son acumulativos y no se resetean ante cancelaciones).

Antes de proponer cambios, documentar, diseñar flujos o tomar decisiones, lee y respeta el contenido completo de esta skill. Trabaja siempre dentro de los límites de lo que el contexto del negocio realmente dice.eto de esta skill. No agregues funcionalidades que no estén aquí. No inventes capacidades. No incluyas contenido técnico. Trabaja siempre dentro de los límites de lo que el contexto del negocio realmente dice.
