# AGENTS — Entrada Principal para Agentes (ICE Launcher / ice_backend)

## Propósito

Este archivo es la **puerta de entrada** y orquestador documental para cualquier agente (humano o automatizado) que trabaje en este repositorio. Su objetivo es mapear la estructura documental, la jerarquía de autoridad y los accesos rápidos antes de modificar documentación o código.

## Quick Start (para agentes)

1. Lee `AGENTS.md` (este archivo) para entender el mapa del proyecto.
2. Lee [agentsRules.md](file:///d:/archivos/ice_backend/agentsRules.md) para conocer las reglas operativas y normas de comportamiento obligatorias.
3. Lee [.skills/ContextSkill.md](file:///d:/archivos/ice_backend/.skills/ContextSkill.md) para comprender el contexto de negocio.
4. Si vas a trabajar con código o infraestructura, lee [.skills/TechnicalContextSkill.md](file:///d:/archivos/ice_backend/.skills/TechnicalContextSkill.md).

---

## Qué es este repo (Confirmado)

- **Producto general**: ICE Launcher.
- **Repositorio**: `ice_backend` (solo contiene la API/Backend que consume el launcher).
- **Alcance**: El launcher/cliente (ej. aplicación de escritorio en Electron) **NO está** en este repositorio.

---

## Jerarquía de autoridad documental

Si existe algún conflicto o contradicción entre documentos del repositorio, rige el siguiente orden de prioridad (de mayor a menor autoridad):

1. [agentsRules.md](file:///d:/archivos/ice_backend/agentsRules.md) (Reglas de comportamiento y flujos de decisión).
2. `AGENTS.md` (Este mapa de orquestación y estructura).
3. [.skills/ContextSkill.md](file:///d:/archivos/ice_backend/.skills/ContextSkill.md) (Contexto de negocio y producto).
4. [.skills/TechnicalContextSkill.md](file:///d:/archivos/ice_backend/.skills/TechnicalContextSkill.md) (Contexto técnico e infraestructura).
5. [README.md](file:///d:/archivos/ice_backend/README.md) (Introducción general al proyecto).

*Nota: Cualquier dato o requerimiento no documentado explícitamente en estas fuentes debe ser tratado como "Pendiente de confirmación humana" y no debe asumirse.*

---

## Estructura documental del proyecto

```
raíz/
├── AGENTS.md                       ← (ESTE ARCHIVO) Mapa de orquestación principal
├── agentsRules.md                  ← Reglas detalladas de comportamiento para agentes
├── README.md                       ← Introducción del repositorio
├── TASKS.md                        ← Backlog técnico y hoja de ruta del proyecto (Fuente de tareas)
├── CHANGELOG.md                    ← Registro histórico de cambios y decisiones tomadas
├── .skills/                        ← Única carpeta oficial de skills
│   ├── ContextSkill.md             ← Contexto de negocio y producto (no técnico)
│   ├── TechnicalContextSkill.md    ← Arquitectura y estado técnico consolidado
│   └── SkillTemplate.md            ← Plantilla para nuevas skills
├── docs/
│   ├── database.sql                ← Esquema de base de datos PostgreSQL
│   └── Basedatos.png               ← Diagrama de base de datos
└── src/                            ← Código fuente del proyecto (capas Clean Arch)
```

---

## Skills disponibles

Las skills oficiales se ubican exclusivamente en la carpeta `.skills/`.

*   **Contexto de Negocio ([ContextSkill.md](file:///d:/archivos/ice_backend/.skills/ContextSkill.md))**: Contiene la visión del producto, lógica de negocio y experiencia del usuario esperada. *No debe contener detalles de código, base de datos ni endpoints.*
*   **Contexto Técnico ([TechnicalContextSkill.md](file:///d:/archivos/ice_backend/.skills/TechnicalContextSkill.md))**: Documenta la arquitectura, stack tecnológico, guías de desarrollo y estado consolidado de los componentes. *No debe usarse para debates de negocio o listas de tareas dinámicas.*

---

## Normas operativas y flujos de decisión

Todas las reglas detalladas sobre:
*   Flujo obligatorio de lectura antes de trabajar.
*   Pautas para no inventar información.
*   Normas para la modificación de código y bases de datos.
*   Procedimiento ante ambigüedades e información faltante.
*   Validaciones finales previas a la entrega.

Se encuentran consolidadas y detalladas en **[agentsRules.md](file:///d:/archivos/ice_backend/agentsRules.md)**. Su cumplimiento es obligatorio y complementario a este archivo.
