# AGENTS — Orquestación de Skills del Proyecto ICE Launcher

## Propósito

Este archivo define cómo los agentes (humanos o automatizados) deben utilizar las skills del proyecto para trabajar de forma coherente, informada y alineada con el contexto real del negocio. Su objetivo es garantizar que cualquier intervención sobre el producto se realice con una comprensión profunda de lo que se está construyendo y por qué.

---

## Skills disponibles

### Skill principal de contexto

**`.skills/ContextSkill.md`**

Es la fuente principal de comprensión del negocio, la intención del producto y la visión funcional humana. Contiene todo el contexto real extraído de `Context.md`, organizado, profundizado y libre de contenido técnico.

Esta skill debe ser leída y respetada antes de cualquier acción que involucre:

- Proponer cambios en el producto o sistema.
- Documentar funcionalidades, flujos o decisiones.
- Diseñar lógica de negocio o experiencia de usuario.
- Tomar decisiones que afecten la dirección del producto.
- Interpretar requerimientos o necesidades del negocio.
- Evaluar si una propuesta es coherente con la visión del producto.

---

## Reglas para los agentes

1. **Leer ContextSkill.md primero**: Antes de cualquier intervención, lee completamente `.skills/ContextSkill.md`. No asumas que conoces el contexto.

2. **No inventar funcionalidades**: Todo lo que propongas o documentes debe estar respaldado por el contenido de `Context.md` y `.skills/ContextSkill.md`. Si una funcionalidad no está en esos documentos, no existe como hecho del negocio.

3. **No mezclar contexto con tecnología**: Las discusiones sobre el negocio deben mantenerse en el plano funcional y humano. No introduzcas términos técnicos, arquitectura, código, endpoints o implementaciones en el análisis del contexto del negocio.

4. **Respetar las ambigüedades**: Si una pregunta o duda sobre el negocio no tiene respuesta en `Context.md`, no la inventes. Documenta la ambigüedad o consulta antes de asumir.

5. **Señalar información faltante**: Si durante tu trabajo identificas que falta información relevante para tomar una decisión, señálalo explícitamente. No tomes decisiones basadas en suposiciones no documentadas.

6. **No sobrepasar los límites del contexto**: El alcance de lo que se sabe sobre el negocio está delimitado por `Context.md`. Cualquier expansión debe ser validada con el cliente o responsable del producto.

---

## Flujo recomendado antes de trabajar

1. Lee `.skills/ContextSkill.md` completamente.
2. Identifica qué parte del contexto es relevante para tu tarea específica.
3. Si encuentras ambigüedades o vacíos de información, documéntalos antes de continuar.
4. Propón o ejecuta tu trabajo manteniéndote dentro de los límites del contexto documentado.
5. Al finalizar, verifica que no hayas introducido funcionalidades, conceptos o suposiciones que no estén respaldados por `Context.md`.

---

## Límites

- Este archivo no reemplaza a `Context.md` ni a `.skills/ContextSkill.md`. Es un orquestador, no una fuente de contexto.
- No define requisitos técnicos, arquitectura, tecnologías ni implementaciones.
- No es un reemplazo de la comunicación con el cliente o responsable del producto. Si el contexto es insuficiente, la respuesta no es inventar, sino preguntar.
- Las skills pueden crecer con el tiempo, pero siempre deben estar basadas en información real documentada y validada.

---

## Criterios de calidad

- Toda decisión o propuesta debe poder rastrearse hasta una fuente en `Context.md` o en `.skills/ContextSkill.md`.
- El lenguaje utilizado debe ser claro, profesional y libre de tecnicismos innecesarios cuando se habla del negocio.
- No debe existir ninguna funcionalidad inventada ni capacidad asumida sin respaldo documental.
- Si hay ambigüedades o vacíos, deben ser explícitamente reconocidos y no ocultados.
- El trabajo debe sentirse alineado con la visión del producto descrita en `Context.md`: identidad, personalización, monetización y crecimiento.
