# Agent Orchestration (Skill System)

Este archivo coordina el flujo de trabajo y la evolución del conocimiento técnico para el proyecto **Ice**. Todos los agentes deben seguir estas directivas para mantener la integridad del código y la documentación.

## Knowledge Sources (Skills)
- **[.skill/ice.md](.skill/ice.md)**: El "Cerebro" del proyecto. Contiene la arquitectura, el stack técnico, las reglas de dominio y el progreso actual.

## Agent Workflow
Cuando un agente comienza una tarea, debe:
1.  **Analizar el Contexto**: Leer `.skill/ice.md` para entender las reglas vigentes.
2.  **Ejecutar la Tarea**: Implementar cambios siguiendo la Clean Architecture.
3.  **Actualizar el Conocimiento**: Si la tarea cambia la arquitectura o añade una funcionalidad clave, debe actualizarse `.skill/ice.md`.
4.  **Checklist de Salida**:
    - [ ] ¿El código sigue los principios de Clean Architecture?
    - [ ] ¿Se actualizaron las entidades en la capa de Domain si fue necesario?
    - [ ] ¿Se reflejaron los cambios de DB en `docs/database.sql`?
    - [ ] ¿Se actualizó `.skill/ice.md` con el nuevo estado?

## Protocolo de Mejora Continua
Si detectas una ineficiencia o un patrón repetitivo, documenta una nueva "Skill" o mejora la existente en la carpeta `.skill/`.
