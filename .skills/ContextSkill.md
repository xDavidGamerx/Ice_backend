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
- No hay un modelo comercial estructurado que permita vender cosméticos y rangos de forma coherente.
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

1. **Launcher de escritorio**: Es el canal principal. Está construido con Electron y consume información del backend para mostrar el estado del usuario, su perfil, su tipo de cuenta, sus cosméticos disponibles, sus rangos activos, su información comercial y cualquier otra funcionalidad asociada a su cuenta. El launcher actúa como una extensión viva de la cuenta del usuario.

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

1. **Cosméticos**: Son elementos visuales o de personalización. Incluyen capas, animaciones y posiblemente otros elementos visuales que se definan más adelante. Su propósito es aportar diferenciación visual y valor estético. No están pensados como ventajas funcionales directas, sino como elementos de apariencia, identidad o presentación.

2. **Rangos**: Son una categoría distinta. Su valor no está únicamente en lo visual, sino en los beneficios que otorgan. Los rangos dan beneficios y tienen relación con el launcher y con otros elementos del ecosistema. Un rango puede representar estatus, privilegios, acceso especial, ventajas funcionales o desbloqueos específicos. Los rangos forman parte del modelo comercial pero deben entenderse como una categoría con comportamiento propio.

La diferencia fundamental es: los cosméticos son personalización visual, los rangos son otorgamiento de beneficios.

---

## Experiencia esperada para el usuario

La experiencia que se busca construir puede describirse así:

Un usuario entra al ecosistema ICE Launcher. Puede hacerlo con su cuenta ICE. Si aplica, también puede vincular o usar su cuenta Microsoft/Minecraft premium.

Una vez dentro, el sistema reconoce quién es el usuario, qué tipo de cuenta tiene, qué información asociada posee y qué contenido le corresponde.

Desde el launcher, el usuario no solo ve una interfaz para abrir el juego, sino una experiencia más rica: ve su perfil, su identidad, sus opciones de personalización, acceso a elementos visuales, su estado comercial y sus beneficios activos.

Además, el usuario puede descubrir productos dentro del ecosistema, comprarlos y ver reflejadas esas compras de forma consistente en su cuenta. Esto incluye cosméticos visibles, contenido desbloqueado y rangos con beneficios aplicables a su experiencia.

La experiencia debe sentirse como un solo producto. El usuario tiene una identidad persistente. Sus compras están ligadas a esa identidad. Los productos adquiridos forman parte de su experiencia. Todo está conectado.

---

## Lógica de negocio identificada

Del contexto se desprenden las siguientes reglas y principios de negocio:

1. **La cuenta ICE es la identidad raíz**. Todo usuario del ecosistema tiene una cuenta ICE. La cuenta Microsoft es un complemento opcional.

2. **La diferenciación entre tipos de cuenta es fundamental**. El sistema no debe tratar a todos los usuarios de la misma forma. Debe entender y manejar las diferencias entre un usuario solo-ICE y un usuario con cuenta Microsoft vinculada.

3. **Los cosméticos y los rangos son categorías de producto distintas**. No deben mezclarse. Tienen propósitos, comportamientos y lógicas diferentes. Los cosméticos son visuales. Los rangos otorgan beneficios.

4. **Los productos adquiridos están ligados a la identidad del usuario**. Una compra no es una transacción aislada. Pasa a formar parte de la experiencia del usuario y debe reflejarse de forma consistente en todos los canales.

5. **Varios canales comparten una misma fuente de verdad**. El launcher, la tienda web y cualquier canal futuro deben leer y escribir sobre la misma base de información central.

6. **El sistema debe soportar expansión futura**. No está pensado para un conjunto fijo de funcionalidades, sino como una base preparada para crecer.

7. **La monetización es parte central del producto**. El sistema no solo resuelve autenticación o acceso, sino que habilita un modelo de negocio basado en venta de contenido virtual.

---

## Principios del producto

- **Identidad primero**: El usuario existe en el sistema a través de su cuenta ICE. Todo lo demás se construye sobre esa base.
- **Diferenciación consciente**: El sistema reconoce y respeta las diferencias entre tipos de usuario sin forzar uniformidad.
- **Experiencia unificada**: Aunque haya múltiples canales, la experiencia debe sentirse como un solo producto.
- **Persistencia**: La identidad, las compras, los productos y los beneficios del usuario son persistentes y están ligados a su cuenta.
- **Coherencia**: Lo que un usuario ve en el launcher debe ser consistente con lo que ve en la tienda web y en cualquier otro canal.
- **Preparación para el crecimiento**: El sistema debe estar diseñado para ampliarse sin romper lo existente.
- **Claridad conceptual**: Cosméticos y rangos no se mezclan. Cada categoría tiene su propia lógica y propósito.
- **El launcher es la cara principal**: Es el canal principal del ecosistema, pero no el único. La tienda web y un futuro panel de gestión también forman parte del todo.

---

## Límites de interpretación

- Este documento no define cómo se implementa técnicamente nada. No habla de endpoints, APIs, bases de datos, frameworks, servicios externos, código, variables de entorno ni infraestructura.
- No se debe asumir que el sistema tiene funcionalidades que no están descritas aquí. Si no está documentado en esta skill, no es parte del contexto de negocio vigente de este repo.
- No se debe inventar tipos de productos, categorías, beneficios, mecánicas comerciales, flujos de usuario o comportamientos del sistema que no estén explícitamente mencionados o que no se deriven directamente de lo mencionado.
- La existencia de un backend es conceptual. Este documento describe qué debe hacer, no cómo debe construirse.

---

## Lo que no debe asumirse

- No se debe asumir que todos los usuarios tienen cuenta premium de Minecraft.
- No se debe asumir que todos los usuarios tienen cuenta Microsoft.
- No se debe asumir que los cosméticos solo incluyen capas y animaciones. Es posible que hayan más tipos, pero no están definidos.
- No se debe asumir que los rangos tienen una lista completa de beneficios definida. Esa información está pendiente.
- No se debe asumir que el sistema ya tiene una tienda web operativa. Es una posibilidad futura.
- No se debe asumir que el panel de gestión existe o está planificado a corto plazo. Es una visualización natural, no un requisito confirmado.
- No se debe asumir que los productos tienen una duración o vigencia específica. Eso no está definido.
- No se debe asumir que el launcher actual ya está conectado a un backend. Precisamente ese es el vacío que se busca llenar.

---

## Ambigüedades detectadas

1. **Naturaleza exacta de los "beneficios" de los rangos**: El contexto dice que los rangos "dan beneficios" y tienen relación con el launcher y otros elementos del ecosistema, pero no especifica qué tipo de beneficios, cómo se aplican, cómo se gestionan ni cómo se reflejan en la experiencia del usuario.

2. **Relación entre el rango y su dimensión visual**: Se menciona que un rango "puede tener además una dimensión visual o simbólica", pero no queda claro si esa dimensión visual es parte inherente del rango, un cosmético aparte, o algo que se configura independientemente.

3. **El concepto de "estado comercial" del usuario**: Se menciona en varios lugares que el sistema debe mostrar el "estado comercial" del usuario, pero no se define qué información compone ese estado ni cómo se determina.

4. **Nivel de integración entre la cuenta Microsoft y el ecosistema ICE**: Se menciona que aporta "contexto adicional" e "información visual o de personalización", pero no se especifica qué información concreta se obtiene ni cómo se utiliza dentro de la plataforma.

---

## Información pendiente por definir

1. **Lista final de tipos de cosméticos**: Solo se mencionan capas y animaciones como ejemplos. El resto está por definir.

2. **Definición exacta de los beneficios de cada rango**: No se sabe qué beneficios otorga cada rango, cómo se estructuran ni cómo se entregan al usuario.

3. **Duración o vigencia de los productos**: No está definido si los productos (cosméticos o rangos) son permanentes, tienen fecha de expiración, requieren renovación o tienen algún otro modelo de vigencia.

4. **Comportamiento de las mecánicas comerciales**: No se han detallado los flujos de compra, los métodos de pago, las políticas de reembolso, las promociones o cualquier otra mecánica relacionada con la venta de productos.

5. **Detalles del panel de gestión**: Es una visualización natural pero no hay requisitos, prioridades ni alcance definido.

6. **Alcance completo de la tienda web**: Se menciona como canal futuro pero sin especificar funcionalidades, catálogo completo, integraciones ni límites.

7. **Modelo de relación entre rangos y otros elementos del ecosistema**: Se menciona que los rangos tienen "relación con el launcher y con otros elementos del ecosistema", pero no se explica cuál es esa relación.

---

## Resumen operativo para futuros agentes

ICE Launcher es un ecosistema digital centrado en un launcher de Minecraft que busca evolucionar hacia una plataforma completa de identidad, personalización y monetización.

El sistema debe permitir que un usuario entre al ecosistema con su cuenta ICE (y opcionalmente vincule una cuenta Microsoft premium), sea reconocido según su tipo de cuenta, acceda a su información y contenido, compre productos virtuales (cosméticos visuales y rangos con beneficios) y reciba una experiencia coherente y conectada a través del launcher de escritorio, una futura tienda web y posiblemente un panel de gestión.

La cuenta ICE es la identidad principal. Los cosméticos son personalización visual. Los rangos otorgan beneficios. Estas dos categorías no deben mezclarse. El sistema debe ser la fuente principal de verdad de todo el ecosistema.

Antes de proponer cambios, documentar, diseñar flujos o tomar decisiones, lee y respeta el contenido completo de esta skill. No agregues funcionalidades que no estén aquí. No inventes capacidades. No incluyas contenido técnico. Trabaja siempre dentro de los límites de lo que el contexto del negocio realmente dice.
