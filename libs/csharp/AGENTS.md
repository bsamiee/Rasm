# [CSHARP_LIBS_AGENTS]

Scope: `libs/csharp/` only. Root `AGENTS.md` and `CLAUDE.md` own universal C# policy, skills, and quality rails; this file adds library-family behavior for every `Rasm.*` project.

## [1][READ_ORDER]

- When editing a library project, read this file and then the nearest project `AGENTS.md`.
- When adding a folder, public rail, operation algebra, state record, receipt, or proof pattern, read existing sibling owners first to find the extension rail.
- When changing `System.*`, package policy, global usings, host references, or `global.json` effects, read `docs/system-api-map`.
- When adding product-library or host SDK assumptions, read `docs/external-libs`.
- When moving host-composition packages into graph, read `docs/host-libraries.md`.

## [2][LIBRARY_CONTRACT]

Library projects set capability ceilings for downstream agents, plugins, apps, and tools. Missing callers do not justify weak abstractions, wrapper-only APIs, or partial domain functionality.

Capture native or domain capability deeply, expose one small OOP boundary per durable concern, and keep intelligence internal through typed FP/ROP rails. Downstream code passes intent and context, then receives typed results, receipts, projections, or operations without native call choreography.

## [3][EXTENSION_GRAMMAR]

- New project capability: extend the owning project overlay and source owner before adding a sibling rail.
- New folder: add it only for a durable sub-concern with multiple consumers or a distinct native boundary.
- Repeated slot families, mutation buckets, receipt construction, option cases, or overload families: collapse into one typed rail.
- Data-only catalogue behavior: keep it host-free and geometry-free unless a downstream composition owner exists.
- Package or solution adoption: update central manifests and project files; keep version truth out of instruction prose.

## [4][PROJECT_ROUTING]

| [INDEX] | [PROJECT]          | [LOCAL_ROUTE]                | [ROLE]                               |
| :-----: | :----------------- | :--------------------------- | :----------------------------------- |
|   [1]   | `Rasm`             | `Rasm/AGENTS.md`             | geometry kernel and analysis algebra |
|   [2]   | `Rasm.Rhino`       | `Rasm.Rhino/AGENTS.md`       | RhinoWIP boundary                    |
|   [3]   | `Rasm.Grasshopper` | `Rasm.Grasshopper/AGENTS.md` | Grasshopper 2 boundary               |
|   [4]   | `Rasm.Materials`   | `Rasm.Materials/AGENTS.md`   | host-free material catalogue         |
|   [5]   | `Rasm.AppUi`       | `Rasm.AppUi/AGENTS.md`       | product UI rail                      |
|   [6]   | `Rasm.AppHost`     | `Rasm.AppHost/AGENTS.md`     | runtime platform                     |
|   [7]   | `Rasm.Compute`     | `Rasm.Compute/AGENTS.md`     | measured execution platform          |
|   [8]   | `Rasm.Persistence` | `Rasm.Persistence/AGENTS.md` | local durable state                  |

Use co-located `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` files where present for project state, package adoption, file architecture, and implementation sequence. This parent overlay carries only family-level invariants.

## [5][BOUNDARY_RULES]

- Keep the project graph acyclic and rooted in the kernel. A new sibling-to-sibling edge needs a local owner route and proof that composition belongs there.
- Keep host isolation by host. Rhino and Grasshopper host types stay inside their owning boundary projects unless an explicit multi-host consumer owns composition.
- Keep scaffold facts in project architecture or roadmap files. Do not infer public surfaces, references, or package adoption from a planned project.
- Treat solution, central-package, and directory-prop changes as broad build-trigger changes; route command syntax to `CLAUDE.md` and `tools/quality/README.md`.

## [6][REJECTIONS]

- No `Helpers`, `Utils`, `Manager`, `Service`, `Common`, `Misc`, grab-bag `Options`, or generic parameter-bag sprawl.
- No compatibility aliases or transitional wrappers after the canonical owner exists.
- No package versions in documentation or project references when central manifests own version truth.
- No data-only library references to geometry or host assemblies.
- No public API that merely renames a native call without adding policy, proof, safety, batching, or typed failure value.
