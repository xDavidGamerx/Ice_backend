# Contexto General del Proyecto — ICE Launcher Platform

## Visión general

ICE Launcher es un ecosistema digital centrado en un launcher de Minecraft llamado **ICE Launcher**, cuyo objetivo es ofrecer una experiencia más completa, personalizada y comercialmente escalable alrededor del acceso al launcher, la identidad del usuario, la vinculación con cuentas premium de Minecraft y la compra/gestión de productos virtuales.

Actualmente, el launcher ya existe a nivel de producto y está construido con tecnologías web (**HTML, CSS y JavaScript**) empaquetadas como aplicación de escritorio mediante **Electron**. Esto significa que, aunque visualmente es una aplicación de escritorio, conceptualmente funciona como un cliente moderno que consume servicios desde un backend centralizado.

La idea del proyecto no es únicamente “tener login y tienda”, sino construir una plataforma que sirva como base para la identidad, la personalización, la monetización y la futura expansión de ICE Launcher.

---

## Qué se quiere construir

Se quiere desarrollar un **backend central** que sirva como núcleo del ecosistema ICE Launcher. Ese backend será el responsable de sostener la lógica principal del negocio y de conectar diferentes partes del producto, especialmente:

- el launcher de escritorio hecho en Electron,
- una posible tienda web,
- y más adelante, potencialmente, un panel administrativo o de gestión.

La intención es que el backend permita manejar de forma clara y consistente todo lo relacionado con usuarios, autenticación, cuentas vinculadas, perfiles, productos virtuales, compras y beneficios asociados a ciertos productos.

En otras palabras, el backend debe convertirse en la **fuente principal de verdad del ecosistema ICE Launcher**.

---

## Propósito del sistema

El propósito principal del sistema es permitir que un usuario pueda entrar al ecosistema ICE Launcher, identificarse correctamente según su tipo de cuenta, acceder a su información, ver su contenido disponible, comprar productos digitales y recibir beneficios dentro del entorno del launcher.

Este sistema debe soportar tanto usuarios que usan una cuenta propia de ICE como usuarios que además vinculan una cuenta Microsoft/Minecraft premium. La plataforma debe entender esas diferencias, sin tratarlas como si fueran lo mismo, y debe ser capaz de ofrecer una experiencia coherente para ambos casos.

Al mismo tiempo, el sistema debe abrir la puerta a la monetización del producto mediante la venta de contenido virtual, especialmente **cosméticos** y **rangos**.

---

## Naturaleza del producto

ICE Launcher no debe pensarse solo como un launcher técnico para abrir Minecraft. Debe pensarse como una **plataforma de experiencia** alrededor del usuario. El launcher no solo inicia sesión o ejecuta un juego: también representa identidad, progreso, apariencia, acceso a contenido, beneficios y posibilidad de compra.

Por eso, el proyecto necesita una base de backend que no esté limitada a tareas simples, sino que entienda el producto como una plataforma viva, conectada a identidad, catálogo, inventario y beneficios.

---

## Canales del ecosistema

### 1. Launcher de escritorio
El canal principal es el launcher de escritorio, construido con Electron. Este launcher será el cliente que consume la información del backend para mostrar:

- el estado del usuario,
- su perfil,
- su tipo de cuenta,
- sus cosméticos disponibles,
- sus rangos activos,
- su información comercial,
- y cualquier otra funcionalidad asociada a su cuenta dentro del ecosistema ICE.

### 2. Tienda web
Además del launcher, se visualiza una tienda web como otro canal importante. Esta web tendría el objetivo de mostrar productos, permitir compras y ofrecer acceso a información comercial del ecosistema sin obligar al usuario a pasar exclusivamente por el launcher.

### 3. Posible panel de gestión
Aunque no sea el foco inmediato, es natural pensar que el sistema terminará necesitando un espacio de administración para gestionar productos, usuarios, beneficios, estados de compra y configuración general del ecosistema.

---

## Identidad del usuario dentro de la plataforma

Uno de los puntos más importantes del proyecto es entender que no todos los usuarios entran de la misma forma ni tienen el mismo contexto.

### Cuenta ICE
Debe existir una cuenta propia de ICE, que funcione como identidad principal dentro del ecosistema. Esta cuenta sirve para reconocer al usuario dentro de la plataforma, darle persistencia, asociarle productos, guardar su estado y construir una relación comercial con él.

La cuenta ICE debe existir incluso para usuarios que no necesariamente tengan una cuenta premium de Minecraft. Esto es importante porque permite que también exista un modelo para usuarios no premium dentro del ecosistema, especialmente si igualmente pueden interactuar con la plataforma, personalizar su experiencia o comprar productos.

### Cuenta Microsoft / Minecraft
Por otro lado, debe existir la posibilidad de vincular una cuenta Microsoft para usuarios premium. Esa vinculación tiene sentido porque aporta contexto adicional del usuario dentro de Minecraft, como su identidad premium, su perfil relacionado y potencialmente información visual o de personalización que permita enriquecer la experiencia en el launcher.

La cuenta Microsoft no debe reemplazar a la cuenta ICE, sino complementarla. La cuenta ICE sigue siendo la identidad principal dentro del ecosistema propio del launcher, mientras que la cuenta Microsoft representa una identidad externa vinculada que agrega valor y contexto premium.

---

## Tipos de usuario contemplados

### Usuario ICE no premium
Es un usuario que entra al ecosistema mediante su cuenta ICE y puede usar funcionalidades propias de la plataforma sin depender necesariamente de una cuenta premium de Minecraft. Este usuario igualmente debe poder tener perfil, historial, compras, inventario y acceso a productos dentro del ecosistema.

### Usuario ICE con cuenta Microsoft vinculada
Es un usuario que, además de su cuenta ICE, vincula su cuenta Microsoft/Minecraft premium. Esto permite enriquecer su perfil y ofrecer una experiencia más integrada con el universo premium de Minecraft.

La plataforma debe soportar ambos escenarios sin romper la lógica de negocio ni forzar que todos los usuarios sean tratados de la misma forma.

---

## Enfoque comercial del proyecto

El proyecto tiene un componente comercial claro. ICE Launcher no solo busca resolver autenticación o acceso, sino construir una experiencia monetizable mediante la venta de productos virtuales.

Actualmente, lo que el cliente ha definido con mayor claridad es que se venderán dos grandes grupos de productos:

### 1. Cosméticos
Los cosméticos representan elementos visuales o de personalización. Dentro de lo ya mencionado se encuentran, por ejemplo:

- capas,
- animaciones,
- y posiblemente otros elementos visuales que se definan más adelante.

La idea de los cosméticos es que aporten diferenciación visual y valor estético al usuario dentro del ecosistema ICE Launcher. No están pensados como ventajas funcionales directas, sino como elementos de apariencia, identidad o presentación.

### 2. Rangos
Los rangos representan una categoría distinta de producto. No deben verse como simples cosméticos, porque su valor no está únicamente en lo visual, sino en los **beneficios** que otorgan.

Según lo expresado por el cliente, los rangos “dan beneficios” y tienen relación con el launcher y con otros elementos del ecosistema. Aunque todavía no se haya detallado por completo cada beneficio, ya es claro que un rango implica una lógica más amplia que la de un cosmético.

Un rango puede representar estatus, privilegios, acceso especial, ventajas funcionales o desbloqueos específicos dentro del entorno ICE Launcher. Por eso, conceptualmente, los rangos forman parte del modelo comercial del sistema, pero deben entenderse como una categoría con comportamiento propio.

---

## Diferencia conceptual entre cosméticos y rangos

Una parte fundamental de la idea es no mezclar categorías de producto que cumplen funciones distintas.

### Cosméticos
Su propósito principal es la personalización visual. Su valor está en cómo el usuario se ve, se representa o diferencia dentro del ecosistema.

### Rangos
Su propósito principal es otorgar beneficios. Un rango puede tener además una dimensión visual o simbólica, pero su esencia es que modifica la experiencia del usuario al darle acceso a algo, mejorar su estatus o activar determinadas ventajas.

Esta diferencia es importante porque define la forma en que el sistema debe ser pensado a nivel de negocio, incluso antes de entrar a detalles técnicos.

---

## Qué papel juega el backend en esta visión

El backend no es un simple acompañante del launcher. Es el sistema que organiza toda la lógica del ecosistema. Debe centralizar el contexto del usuario y convertirlo en información útil para el launcher y para cualquier otro canal.

El backend será responsable de sostener conceptualmente cosas como:

- quién es el usuario,
- cómo inicia sesión,
- si tiene cuenta ICE, Microsoft o ambas,
- qué productos existen,
- qué productos ha comprado,
- qué cosméticos posee,
- qué rangos tiene activos,
- qué beneficios aplican a su cuenta,
- y cómo se refleja todo eso en la experiencia dentro del launcher y la web.

En resumen, el backend es la capa que convierte la idea comercial y funcional de ICE Launcher en una plataforma coherente.

---

## Experiencia esperada del usuario

La experiencia que se busca construir puede entenderse así:

Un usuario entra al ecosistema ICE Launcher. Puede hacerlo con su cuenta ICE y, si aplica, también vincular o usar su cuenta Microsoft/Minecraft premium. Una vez dentro, el sistema reconoce quién es, qué tipo de cuenta tiene, qué información asociada posee y qué contenido le corresponde.

Desde el launcher, el usuario no solo ve una interfaz para abrir el juego, sino una experiencia más rica: perfil, identidad, personalización, acceso a elementos visuales, estado comercial y beneficios activos.

Además, el usuario puede descubrir productos dentro del ecosistema, comprarlos y ver reflejadas esas compras de forma consistente en su cuenta. Esto puede incluir cosméticos visibles, contenido desbloqueado o rangos con beneficios aplicables a su experiencia.

---

## Relación entre launcher, cuenta y tienda

La plataforma debe sentirse como un solo producto, aunque tenga varios puntos de acceso.

El launcher, la cuenta del usuario y la tienda no deben existir como piezas aisladas. Deben formar parte de una misma experiencia conectada.

Eso implica una visión donde:

- el usuario tiene una identidad persistente,
- las compras están ligadas a esa identidad,
- los productos adquiridos forman parte de su experiencia,
- y el launcher actúa como una extensión viva de esa cuenta.

La tienda no es algo separado del launcher, sino una dimensión comercial del mismo ecosistema. De igual forma, el launcher no es una herramienta aislada, sino la cara principal de una plataforma más amplia.

---

## Qué se está imaginando como producto final

La idea general del proyecto apunta a que ICE Launcher evolucione de un launcher funcional a una plataforma con identidad propia. No se trata solo de iniciar Minecraft, sino de construir un entorno donde el usuario tenga cuenta, presencia, personalización, historial y pertenencia a un sistema más grande.

En ese sistema, el usuario puede:

- tener una cuenta propia dentro de ICE,
- conectar su identidad premium si corresponde,
- acceder a una experiencia adaptada a su contexto,
- comprar contenido virtual,
- acumular productos,
- activar elementos visuales,
- recibir beneficios por rangos,
- y relacionarse con el ecosistema desde varios canales.

Esto convierte a ICE Launcher en un producto con base técnica, lógica comercial y potencial de crecimiento.

---

## Enfoque de negocio implícito

Aunque todavía no se haya detallado todo el roadmap, la idea ya deja ver varios objetivos de negocio:

- construir una identidad de marca propia alrededor del launcher,
- generar monetización mediante productos virtuales,
- diferenciar la experiencia del usuario según su cuenta y beneficios,
- consolidar una base de usuarios persistente dentro del ecosistema,
- y preparar una estructura que permita ampliar el producto con el tiempo.

En este sentido, el backend no solo resuelve una necesidad técnica, sino que habilita el modelo de negocio del proyecto.

---

## Contexto funcional actual

A partir de lo ya conversado, el contexto actual del proyecto puede resumirse así:

- ya existe un launcher construido con tecnologías web y empaquetado con Electron,
- se quiere desarrollar un backend en .NET 8 que centralice la lógica del ecosistema,
- ese backend debe soportar cuentas ICE y cuentas Microsoft vinculadas,
- se quiere atender tanto a usuarios premium como no premium,
- se quieren vender productos virtuales dentro del ecosistema,
- actualmente los productos definidos con más claridad son cosméticos y rangos,
- los cosméticos incluyen elementos como capas y animaciones,
- los rangos otorgan beneficios y no deben tratarse como simples cosméticos,
- y además existe la intención de tener presencia también en la web para la parte comercial.

---

## Qué NO se está definiendo todavía

Este documento no busca definir detalles técnicos específicos de implementación. Tampoco entra en endpoints, estructuras internas, contratos de API o decisiones profundas de ingeniería.

Lo que busca es dejar absolutamente clara la idea del producto y el contexto del sistema, para que cualquier persona o cualquier asistente de IA entienda correctamente qué se quiere construir antes de pasar a diseño técnico, arquitectura o ejecución.

Todavía pueden existir elementos no completamente cerrados, como:

- la lista final de tipos de cosméticos,
- la definición exacta de los beneficios de cada rango,
- la duración o vigencia de ciertos productos,
- o el comportamiento final de algunas mecánicas comerciales.

Pero incluso con esos detalles pendientes, la visión central del producto ya es suficientemente clara.

---

## Síntesis final

ICE Launcher es un ecosistema digital centrado en un launcher de Minecraft construido con Electron, que busca evolucionar hacia una plataforma completa de identidad, personalización y monetización.

El proyecto necesita un backend central en .NET 8 que permita conectar y gestionar usuarios, cuentas ICE, cuentas Microsoft vinculadas, perfiles asociados, productos virtuales, compras y beneficios.

Dentro del modelo comercial actual, los productos principales son dos: **cosméticos** y **rangos**. Los cosméticos representan elementos visuales como capas y animaciones. Los rangos representan una categoría diferente, enfocada en otorgar beneficios dentro del ecosistema del launcher.

La plataforma debe servir tanto para usuarios no premium como para usuarios premium con cuenta Microsoft vinculada, y debe permitir que el launcher y una futura tienda web compartan una misma lógica central y una misma fuente de verdad.

En esencia, lo que se quiere construir no es solo un backend para un launcher, sino la base de una plataforma propia de ICE Launcher, con identidad, persistencia, monetización y capacidad de crecimiento.