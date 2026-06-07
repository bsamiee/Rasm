# [CSHARP_LIBS_AGENTS]

Scope: `libs/csharp/` only. Root `AGENTS.md` and `CLAUDE.md` own universal C# policy, skills, and quality rails; this file adds library-family behavior for every `Rasm.*` project.

## [1][READ_ORDER]

- When editing a library project, read this file and then the nearest project `AGENTS.md`.
- When adding a folder, public rail, operation algebra, state record, receipt, or proof pattern, name the existing operation, intent, receipt, policy, runtime, query, or boundary rail that grows; if no rail can be named, stop and read the nearest project overlay, source owner, or architecture before editing.
- When changing C# stack posture (packages, BCL, product libraries, host SDKs, composition), read `docs/stacks/csharp/`, `docs/hosts/`, and `docs/usage/` for the concern that changed.

## [2][LIBRARY_CONTRACT]

Library projects set capability ceilings for downstream agents, plugins, apps, and tools. Missing callers do not justify weak abstractions, wrapper-only APIs, or partial domain functionality.

Capture native or domain capability deeply, expose one small OOP boundary per durable concern, and keep intelligence internal through typed FP/ROP rails. Downstream code passes intent and context, then receives typed results, receipts, projections, or operations without native call choreography.

Approved external libraries, host SDKs, and package-backed capabilities are implementation surfaces that disappear into the owning Rasm rail: operation algebra, runtime record, typed receipt, projection, capability record, source-owned table, query rail, or boundary capsule. Public APIs expose Rasm concepts, not package facades, provider selectors, option bags, toolkit settings, backend modes, or renamed native calls.

For C# variation, prefer generated union or smart-enum dispatch, value-object smart constructors, static-abstract generic math, `params ReadOnlySpan<T>` folds, and LanguageExt rails before adding overload ladders, option bags, single-implementation interfaces, service callbacks, or caller-side switches.

## [3][EXTENSION_GRAMMAR]

- New project capability: extend the owning project overlay, architecture/source owner, and existing operation, intent, receipt, policy, or runtime rail before adding a sibling rail.
- New folder: add it only for a durable sub-concern with multiple consumers or a distinct native boundary.
- Repeated slot families, mutation buckets, receipt construction, option cases, or overload families: extend the shared typed owner named by the project overlay, such as a receipt fold, option or policy table, operation algebra, or overload projection; do not add a parallel family.
- Large owner folder: when a folder has multiple durable operation families, the nearest project overlay must name the exact rail to extend before new public types, files, folders, or entrypoints are added; if the rail cannot be named, stop and read source or architecture before editing.
- Package or solution adoption: bind the package to the local operation algebra, runtime record, projection, or receipt before exposing a public surface; route exact version and package proof to central manifests, project files, and architecture.

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

Use co-located `README.md`, `ARCHITECTURE.md`, and `ROADMAP.md` files where present for project state, package adoption, file architecture, and implementation sequence. This parent overlay carries only family-level invariants.

## [5][BOUNDARY_RULES]

- Keep the project graph acyclic and rooted in the kernel. A new sibling-to-sibling edge needs a local owner route and proof that composition belongs there.
- Keep host isolation by host. Rhino and Grasshopper host types stay inside their owning boundary projects unless an explicit multi-host consumer owns composition.
- Keep scaffold facts in project architecture or roadmap files. Do not infer public surfaces, references, or package adoption from a planned project.
- Treat solution, central-package, and directory-prop changes as broad build-trigger changes; route command syntax to `CLAUDE.md` and `tools/quality/README.md`.

## [6][REJECTIONS]

- No `Helpers`, `Utils`, `Manager`, `Common`, `Misc`, grab-bag `Options`, generic parameter-bag sprawl, or local `*Service` names that rename an owner rail; framework service-provider terms stay boundary facts only where AppHost or architecture owns them.
- No compatibility aliases or transitional wrappers after the canonical owner exists.
- No public package-forwarding facade, provider selector, backend mode, toolkit settings bag, option bag, or wrapper API when a typed owner rail can internalize the dependency.
- No package versions in documentation or project references when central manifests own version truth.
- No data-only library references to geometry or host assemblies.
- No public API that merely renames a native call without adding policy, proof, safety, batching, or typed failure value.
