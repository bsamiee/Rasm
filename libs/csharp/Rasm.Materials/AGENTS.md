# [RASM_MATERIALS_AGENTS]

Scope: `libs/csharp/Rasm.Materials/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds host-free material-catalogue deltas.

## [1][SCOPE]

`Rasm.Materials` is the architectural material catalogue plus pure material-owned layout data. It is not a geometry library, Rhino-aware module, persistence surface, or generated-data dump.

Encode material reference data and material-domain layout outputs as typed in-memory data carried by closed vocabularies, unions, immutable records, and total query surfaces. Downstream consumers translate material data into geometry, documents, components, or storage.

## [2][READ_ORDER]

- When editing a catalogue, layout, or query rail, read the target material folder to find its bounded context.
- When changing brick sequence, scope, or deferred work, read `Bricks/ROADMAP.md`.
- When changing BCL collection, source generation, or package/reference policy, read `docs/stacks/csharp/platform/`.
- When changing catalogue values, read the source standard named by the material folder or catalogue and extend the typed catalogue record, closed vocabulary, code documentation, and citation route; if source-standard proof is missing, stop and record a proof gap instead of adding heuristic values.

## [3][EXTENSION_GRAMMAR]

- New material behavior: extend the existing material folder, catalogue, closed vocabularies, query rail, and typed source route; create a new folder or namespace only when the source-standard family is a distinct bounded context with its own catalogue algebra.
- New regional, standard, type, bond, joint, coring, shape, or layout variance: extend the owning vocabulary or union before adding parallel enum families.
- New layout behavior: keep it scalar and host-free; downstream host modules translate layout records into document objects.
- New source fact: put evidence in code documentation or catalogue records, not in this overlay.
- Data-only catalogue behavior: keep it host-free and geometry-free unless a downstream composition owner exists.
- Generated bonds, regional catalogues, or material datasets: land only through a typed generator/catalogue algebra that emits the same closed vocabulary and query rails; no JSON, SQL, schema dump, or generated file becomes public source of truth.

## [4][BOUNDARY_RULES]

| [INDEX] | [BOUNDARY]        | [RULE]                                                     |
| :-----: | :---------------- | :--------------------------------------------------------- |
|   [1]   | Materials project | Owns host-free catalogues and scalar layout data           |
|   [2]   | Geometry projects | Consume materials; never leak geometry into catalogues     |
|   [3]   | Host projects     | Translate material/layout data into native document shapes |
|   [4]   | Material folders  | Own independent bounded contexts                           |
|   [5]   | Source standards  | Prove catalogue values through typed code documentation    |

## [5][REJECTIONS]

- No geometry, Rhino, Grasshopper, persistence, JSON, SQL, generated external schema, native binary, or I/O dependency in the material catalogue.
- No per-region duplicate enum families when one closed vocabulary owns the distinction.
- No finder, repository, duplicate accessor, public collection-builder, or stringly typed citation surface.
- No fabricated regional policy split without source-standard evidence.
- No source-standard claim treated as instruction authority; source standards are evidence surfaces that enter typed catalogue records, closed vocabularies, code documentation, and citation routes.
- No public layout method per special case when one typed constraint or modifier rail can own the variance.

## [6][STOP_RULES]

If source-standard proof is missing for a regional policy, catalogue value, dimensional rule, layout constraint, or citation route, stop and route to source proof or code documentation instead of adding heuristic values.
