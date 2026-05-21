# Agents Rules — Reglas Operativas para Agentes (ICE Launcher / ice_backend)

## Propósito

Este archivo define las reglas detalladas de comportamiento, lectura y decisión que todo agente (humano o automatizado) debe seguir al trabajar en el proyecto ICE Launcher. Su objetivo es garantizar que cualquier intervención se realice con la información correcta, en el momento correcto y dentro de los límites del contexto documentado.

---

## Reglas generales para agentes

1. **Lee antes de actuar**: Ningún agente debe proponer cambios, escribir documentación, diseñar flujos, tomar decisiones o modificar código sin haber leído primero los archivos obligatorios indicados en este documento.

2. **Respeta la separación de dominios**: El contexto de negocio y el contexto técnico son dominios distintos. No los mezcles. Usa la skill adecuada para cada tipo de tarea.

3. **No inventes información**: Todo lo que propongas o documentes debe estar respaldado por la documentación existente del proyecto. Si una funcionalidad, regla o comportamiento no está documentado, no existe como hecho del proyecto.

4. **Señala ambigüedades y vacíos**: Si encuentras información ambigua o faltante, documéntala explícitamente. No la ocultes ni la inventes.

5. **Valida antes de modificar**: Antes de cambiar cualquier archivo, verifica que entiendes su propósito, su relación con otros archivos y el impacto del cambio.

6. **Sigue las convenciones establecidas**: Respeta la estructura documental, los formatos y las convenciones de nomenclatura definidas en el proyecto.

---

## Lecturas obligatorias antes de trabajar

### Lectura siempre obligatoria (sin excepción)

| Archivo | Propósito |
|---|---|
| `AGENTS.md` | Mapa de orquestación general. Explica la estructura documental y el orden de lectura. |
| `agentsRules.md` | (Este archivo) Reglas operativas detalladas. |
| `.skills/ContextSkill.md` | Contexto de negocio, visión del producto y lógica funcional. |

### Lectura condicional según la tarea

| Si tu tarea involucra... | Lee también... |
|---|---|
| Modificar código, arquitectura o infraestructura | `.skills/TechnicalContextSkill.md` |
| Entender o modificar la base de datos | `docs/database.sql` |
| Proponer nuevas funcionalidades técnicas | `AGENTS.md` + `.skills/ContextSkill.md` + `.skills/TechnicalContextSkill.md` |
| Documentar flujos de usuario o experiencia | `AGENTS.md` + `.skills/ContextSkill.md` |

---

## Cuándo leer cada skill

### `.skills/ContextSkill.md`

- **Siempre**: Es la primera skill que debe leerse al comenzar a trabajar en el proyecto.
- **Antes de**: Proponer cambios en el producto, documentar funcionalidades, diseñar lógica de negocio, tomar decisiones sobre la dirección del producto, interpretar requerimientos.
- **No es necesaria para**: Tareas puramente técnicas que no afectan la lógica de negocio (ejemplo: refactorizar código sin cambiar comportamiento, actualizar dependencias).

### `.skills/TechnicalContextSkill.md`

- **Antes de**: Modificar código, cambiar la arquitectura, agregar dependencias, trabajar con la base de datos, entender el estado de implementación.
- **Después de**: Haber leído `.skills/ContextSkill.md` si no se comprende el propósito del sistema.
- **No es necesaria para**: Tareas de documentación de negocio, diseño de experiencia de usuario o análisis funcional.

---

## Cómo usar las skills

1. Cada skill está diseñada para un propósito específico. No uses una skill para un propósito que no le corresponde.
2. Si una skill no contiene la información que necesitas, no asumas que la información no existe. Verifica en otras skills o en documentación existente del repo (por ejemplo `docs/`).
3. Las skills son documentos vivos. Si encuentras información desactualizada, señálalo. No la corrijas sin validación humana si cambia significado.
4. **Fuente oficial de contexto de negocio**: `.skills/ContextSkill.md`.

---

## Reglas para modificar documentación

1. No elimines información útil sin reubicarla primero.
2. Si consolidas información repetida, asegúrate de no perder significado.
3. Señala claramente cualquier cambio que introduzcas (notas, secciones de ambigüedades, pendientes).
4. Mantén la separación entre documentación de negocio y documentación técnica.
5. Usa lenguaje claro, profesional y en español (salvo nombres técnicos de archivos o conceptos que ya existan).
6. Si una sección requiere revisión humana, déjalo explícitamente indicado.
7. **Organización del CHANGELOG**: Al actualizar `CHANGELOG.md`, se debe seguir estrictamente un orden cronológico inverso (lo más nuevo siempre arriba). Todos los cambios realizados en el mismo día deben consolidarse bajo una única cabecera de fecha (ej. `## [AAAA-MM-DD]`), agrupados en las subsecciones estándar (`Añadido`, `Cambiado`, `Corregido`, `Mejorado`, `Técnico`). Si la IA realiza múltiples tareas en un día, debe consolidar los cambios en el bloque de esa fecha y, en caso de dudas sobre cómo agruparlos, preguntar al usuario, si la IA no sabe que dia es pregunte al usuario o investigar.


---

## Reglas para modificar código

1. Lee primero `.skills/ContextSkill.md` (para entender el propósito) y `.skills/TechnicalContextSkill.md` (para entender la arquitectura).
2. Sigue las guías de desarrollo documentadas en `.skills/TechnicalContextSkill.md` (pureza de Domain, consistencia de nomenclatura, seguridad).
3. Si modificas la base de datos, refleja los cambios en `docs/database.sql`.
4. No agregues funcionalidades que no estén respaldadas por el contexto de negocio en `.skills/ContextSkill.md`.
5. Verifica que los cambios no rompan la coherencia con la visión del producto.

---

## Reglas para no inventar información

- No agregues funcionalidades, módulos, pantallas, endpoints, integraciones, tecnologías o reglas de negocio que no estén documentadas.
- No asumas relaciones, flujos o comportamientos que no estén explícitamente descritos.
- No inventes beneficios de rangos, tipos de cosméticos, mecánicas comerciales o cualquier otro detalle del producto.
- Si algo no está documentado, documéntalo como información pendiente. No lo des por sentado.

---

## Convención y ciclo de vida de skills

Reglas para crear/actualizar skills (sin excepción):

1. **Ubicación**: toda skill vive en `.skills/`.
2. **Nombre**: `*Skill.md` con nombre descriptivo y único (ejemplo: `ContextSkill.md`).
3. **Plantilla**: si creas una nueva skill, parte de `.skills/SkillTemplate.md`.
4. **No duplicación**: si el contenido encaja en una skill existente, actualiza esa skill en vez de crear otra.
5. **Dividir**: divide una skill cuando mezcle dominios (negocio vs técnica vs reglas) o cuando sea difícil de escanear.
6. **Fusionar**: fusiona skills cuando repitan la misma información. Conserva la versión más clara y actualizada.
7. **Deprecar**: si una skill se reemplaza, deja nota explícita en la skill vieja y elimina referencias; idealmente elimina el archivo si ya no se usa.

Etiquetas obligatorias en texto cuando aplique:

- `Confirmado`: respaldado por documentación vigente del repo.
- `Pendiente`: se sabe que falta definición.
- `Ejemplo`: ilustración, no un requerimiento.
- `No documentado como contrato`: no asumirlo como interfaz/compromiso estable.
- `Pendiente de confirmación humana`: requiere validación por una persona responsable.

---

## Manejo de ambigüedades

1. Si encuentras una ambigüedad (información que puede interpretarse de más de una manera), documéntala en la sección de ambigüedades de la skill correspondiente.
2. No tomes decisiones basadas en una interpretación ambigua sin consultar al responsable del producto.
3. Si debes avanzar, elige la interpretación más conservadora (la que añada menos suposiciones) y documéntalo.

---

## Manejo de información faltante

1. Si falta información relevante para tomar una decisión, créala como información pendiente en la skill correspondiente.
2. No inventes la información faltante para poder avanzar.
3. Si la información faltante bloquea tu trabajo, detente y señálalo explícitamente.

---

## Validación final antes de entregar cambios

Antes de finalizar cualquier intervención, verifica que:

1. No hayas agregado funcionalidades, reglas o comportamientos no documentados.
2. No hayas mezclado contexto de negocio con contenido técnico en el mismo archivo.
3. No hayas eliminado información útil sin reubicarla.
4. No existan contradicciones entre los archivos modificados y el resto de la documentación.
5. La documentación modificada o creada sea clara, profesional y entendible para un humano y útil para un agente.
6. Las ambigüedades y vacíos estén explícitamente señalados si los hay.
7. Los cambios en código sigan las guías de desarrollo del proyecto.

---

## Sugerencia de Commit y Control de Versiones (Opcional)

Al finalizar con éxito cualquier tarea (corrección de bug, nueva funcionalidad, refactorización o cambio de documentación), el agente debe ofrecer al usuario de manera sugerida los comandos de Git necesarios para preparar y subir los cambios a su rama. Esta acción es siempre una sugerencia opcional y queda a la entera decisión del usuario si desea ejecutarla o no.

Se debe proponer un mensaje de commit estructurado bajo la convención de *Conventional Commits*:
- `feat(assets): ...` para nuevas funcionalidades del sistema.
- `fix(auth): ...` para corrección de bugs.
- `docs(readme): ...` para actualizaciones de documentación.
- `refactor(webhooks): ...` para cambios que no modifican comportamiento público.
- `test(delivery): ...` para añadir o modificar pruebas unitarias.

Ejemplo:
*   `git commit -m "feat(assets): implementar autenticación declarativa y tokens efímeros para entrega de activos"`
