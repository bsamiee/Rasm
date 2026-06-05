# [RASM_AGENTS]

Scope: `libs/csharp/Rasm/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds kernel and analysis deltas.

## [1][SCOPE]

`Rasm` is the foundational geometry kernel and higher-order concern library. It is not a thin Rhino API boundary, wrapper extraction area, or dumping ground for unrelated utilities.

Build reusable category logic for analysis, vectors, detection, orientation, transformation, topology, measurement, spatial search, and future concern categories. Downstream consumers should get powerful operations with minimal ceremony and no repeated sequencing.

## [2][READ_ORDER]

- When adding shared validation, context, ownership, statistics, acceptance, projection, or geometry-identity behavior, read `Domain/` to find the reusable owner.
- When changing Vectors capability or public projection behavior, read `Vectors/_ARCHITECTURE.md`.
- When changing BCL, `System.*`, or package policy, read `docs/system-api-map`.
- When writing numerical algorithms by hand, read `docs/external-libs/mathnet`.
- When native runtime behavior is required for mesh, plane, unwrap, remesh, SDF, validity, or host predicates, route to bridge scenario guidance instead of static xUnit.

## [3][EXTENSION_GRAMMAR]

- Shared kernel concept: extend `Domain` when the behavior is reused across concern categories or required by acceptance, validation, statistics, context, ownership, or geometry identity.
- Analysis behavior: extend the analysis owner, operation rail, and aspects; import `Domain` rather than duplicating validation, statistics, coercion, or kind logic.
- Vector behavior: extend vector intent, support-space projection, fields, clouds, meshes, matrices, sampling, flow, alignment, spectral substrate, typed receipts, or intent projection.
- Future category: create one concern category with one consumer surface, compact intent or state records, and algorithms that reuse `Domain`.

## [4][EXECUTION_RULES]

- Expose one access path per folder; do not give every file its own consumer API.
- Model category intent as typed data when primitive parameters create ceremony or hide semantics.
- Convert native nullable, bool, disposable, and ownership semantics into typed rails at boundary adapters.
- Preserve Rhino predicate semantics; verify substitutions against local API evidence and runtime behavior before replacing native calls with algebraic approximations.

## [5][REJECTIONS]

- No wrapper-only abstractions around RhinoCommon or existing domain code.
- No local copies of reusable `Domain` validation, statistics, coercion, geometry-kind, or acceptance logic.
- No new public rail beside an existing owner without removing the obsolete path.
- No hardcoded invisible policy values; use named policies, native defaults, typed receipts, or caller input.
- No exposure of fixed kernel/native choices as public knobs unless they execute and change behavior.
