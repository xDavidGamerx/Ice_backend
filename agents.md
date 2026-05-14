# AGENTS — Entrada Principal para Agentes (ICE Launcher / ice_backend)

## Propósito

Este archivo es la **puerta de entrada** para cualquier agente (humano o automatizado) que trabaje en este repositorio.

Objetivo: que cualquier agente sepa **qué leer primero**, **qué documento manda**, **qué skill usar**, y **qué no asumir** antes de modificar documentación o código.

## Quick Start (para agentes)

1. Lee `AGENTS.md` (este archivo).
2. Lee `agentsRules.md` (reglas operativas).
3. Lee `.skills/ContextSkill.md` (contexto de negocio vigente).
4. Si vas a tocar código o decisiones técnicas: lee `.skills/TechnicalContextSkill.md`.

---

## Qué es este repo (Confirmado)

- Producto general: **ICE Launcher**.
- Repositorio: **ice_backend**.
- Alcance de este repo: **solo backend/API** que consume ICE Launcher.
- El launcher/cliente (por ejemplo, una app Electron) **NO está** en este repositorio.

---

## Jerarquía de autoridad documental

Si hay conflicto entre documentos, usa este orden (de mayor a menor autoridad):

1. `agentsRules.md` para reglas operativas de agentes.
2. `AGENTS.md` para orquestación, rutas y orden de lectura.
3. `.skills/ContextSkill.md` para contexto de negocio vigente.
4. `.skills/TechnicalContextSkill.md` para contexto técnico vigente.
5. `README.md` como introducción rápida.

Nota: si un dato no está documentado en estas fuentes, **no se asume**. Se marca como `Pendiente de confirmación humana`.

---

## Estructura documental del proyecto

```
raíz/
├── AGENTS.md                       ← (ESTE ARCHIVO) Mapa de orquestación principal
├── agentsRules.md                  ← Reglas detalladas de comportamiento para agentes
├── README.md                        ← Introducción del repositorio
├── .skills/                        ← Única carpeta oficial de skills
│   ├── ContextSkill.md             ← Skill de contexto de negocio (no técnico)
│   └── TechnicalContextSkill.md    ← Skill de contexto técnico del proyecto
│   └── SkillTemplate.md            ← Plantilla para nuevas skills
├── docs/
│   ├── database.sql                ← Esquema de base de datos
│   └── Basedatos.png               ← Diagrama de base de datos
└── src/                            ← Código fuente del proyecto
```

---

## Skills disponibles

### `.skills/ContextSkill.md` — Contexto de Negocio

**Propósito**: Contiene la comprensión más profunda del negocio, la intención del producto y la visión funcional humana del ecosistema ICE Launcher. No contiene detalles técnicos.

**Úsala para**: Entender el problema que resuelve el sistema, la visión del producto, los tipos de usuario, la lógica de negocio, los principios del producto y la experiencia esperada.

**No la uses para**: Decisiones técnicas, arquitectura, código o implementación.

### `.skills/TechnicalContextSkill.md` — Contexto Técnico

**Propósito**: Contiene la información técnica documentada del proyecto: stack tecnológico, arquitectura, estructura de carpetas, guías de desarrollo y estado de implementación.

**Úsala para**: Entender la base técnica, modificar código, trabajar con la base de datos, seguir convenciones de desarrollo.

**No la uses para**: Análisis de negocio, visión del producto o decisiones funcionales.

---

## Orden recomendado de lectura

Para cualquier tarea nueva, sigue este orden:

1. **`AGENTS.md`** — (este archivo) Para entender la estructura documental.
2. **`agentsRules.md`** — Para conocer las reglas operativas detalladas.
3. **`.skills/ContextSkill.md`** — Para comprender el contexto de negocio.
4. **`.skills/TechnicalContextSkill.md`** — Solo si la tarea involucra aspectos técnicos.

---

## Uso de agentsRules.md

`agentsRules.md` contiene las reglas detalladas de comportamiento, lectura y decisión para agentes. Este archivo es de lectura obligatoria complementaria a `AGENTS.md`.

En `agentsRules.md` encontrarás:
- Qué archivos leer según el tipo de tarea.
- Cuándo y cómo usar cada skill.
- Reglas para modificar documentación y código.
- Cómo manejar ambigüedades e información faltante.
- Validación final antes de entregar cambios.

**No trabajes en el proyecto sin haber leído `agentsRules.md`.**

---

## Flujo obligatorio para agentes

1. Lee `AGENTS.md` (este archivo).
2. Lee `agentsRules.md`.
3. Lee `.skills/ContextSkill.md`.
4. Si tu tarea es técnica, lee también `.skills/TechnicalContextSkill.md`.
5. Identifica qué parte del contexto es relevante para tu tarea.
6. Si encuentras ambigüedades o vacíos, documéntalos. No los inventes.
7. Ejecuta tu trabajo dentro de los límites del contexto documentado.
8. Valida que no hayas introducido funcionalidades, conceptos o suposiciones no respaldados.
9. Entrega los cambios con las validaciones correspondientes.

---

## Criterios de calidad

- Toda decisión o propuesta debe poder rastrearse hasta una fuente documentada en el proyecto.
- No debe existir ninguna funcionalidad inventada ni capacidad asumida sin respaldo documental.
- Las ambigüedades y vacíos deben estar explícitamente reconocidos, no ocultados.
- El trabajo debe sentirse alineado con la visión del producto: identidad, personalización, monetización y crecimiento.
- La documentación de negocio y la documentación técnica no deben mezclarse en un mismo archivo.
- El lenguaje debe ser claro, profesional y en español (salvo nombres técnicos de archivos o conceptos existentes).

---

## Límites y restricciones

- `AGENTS.md` no reemplaza a `agentsRules.md` ni a ninguna skill. Es un orquestador, no una fuente de contenido.
- No define requisitos técnicos, arquitectura, tecnologías ni implementaciones.
- Si el contexto documentado es insuficiente, la respuesta no es inventar, sino preguntar o documentar el vacío.
- Las skills pueden crecer con el tiempo, pero siempre deben basarse en información real documentada y validada.

---

## Carpeta oficial de skills (Regla no negociable)

La **única** carpeta válida para skills en este repositorio es `.skills/`.

No se deben crear carpetas alternas (`skill/`, `skills/`, `.skill/`, `Skills/`, etc.).

---

## Qué hacer ante ambigüedades

1. Documenta la ambigüedad en la sección correspondiente de la skill afectada.
2. No tomes decisiones basadas en interpretaciones no validadas.
3. Si debes avanzar, usa la interpretación más conservadora (la que añada menos suposiciones) y documéntalo.

---

## Qué hacer cuando falta información

1. Crea una entrada de información pendiente en la skill correspondiente.
2. No inventes la información faltante.
3. Si la falta de información bloquea tu trabajo, detente y señálalo explícitamente.
