# [RASM_AGENTS]

Scope: `libs/csharp/Rasm/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds kernel and analysis deltas.

## [1]-[SCOPE]

`Rasm` is the foundational geometry kernel and higher-order concern library. It is not a thin Rhino API boundary, wrapper extraction area, or dumping ground for unrelated utilities.

Build reusable category logic for analysis, vectors, detection, orientation, transformation, topology, measurement, spatial search, and future concern categories. Downstream consumers receive `Analyze.Run`, `Operation<TGeometry,TOut>`, `VectorIntent`, typed receipts, and `Domain`-backed projections; they do not receive host-shaped APIs or per-file consumer entrypoints.

## [2]-[READ_ORDER]

- When adding shared validation, context, ownership, statistics, acceptance, projection, or geometry-identity behavior, read `Domain/` to find the reusable owner.
- When changing `Analysis/`, read `Analysis/Analyze.cs`; preserve `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run` as the singular execution rail.
- When changing Vectors projection, sampling, extraction, receipt, or algorithm entrypoints, read `Vectors/Intent.cs`; preserve `VectorIntent.Project<TOut>(Context, Op?)` as the consumer projection rail and `ExtractionDomain` plus `SampleKind` as the sampling/extraction rail unless architecture has moved the owner.
- When changing BCL, `System.*`, or package policy, read `docs/stacks/csharp/platform/`.
- When adding numerical or symbolic behavior, read `docs/stacks/csharp/numeric-algorithms.md` and `docs/stacks/csharp/sparse-factorization.md`.
- When native runtime behavior is required for mesh, plane, unwrap, remesh, SDF, validity, or host predicates, read `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, and `tools/rhino-bridge/AGENTS.md`; route native success to source-owned bridge scenarios instead of static xUnit.

## [3]-[EXTENSION_GRAMMAR]

- Shared kernel concept: extend `Domain` when the behavior is reused across concern categories or required by acceptance, validation, statistics, context, ownership, or geometry identity.
- Analysis behavior: extend `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run` in `Analysis/Analyze.cs`; import `Domain` validation, statistics, coercion, and kind logic instead of creating analysis-local copies.
- Vector behavior: extend `VectorIntent`, `FieldNabla`, `SampleKind`, `CloudKernel.MassOf`, `LaplacianCache`, `SpectralFilter`, `AtomProjection`, or the architecture-named owner before adding public projection, sampling, solver, mesh, spectral, or receipt surfaces.
- New numerical or symbolic behavior: use approved MathNet, CSparse, or native BCL numeric surfaces directly inside the owning algorithm rail; do not expose library knobs or wrapper-only numeric APIs.
- Future concern category: build one category, one consumer surface, typed intent/state, and algorithms over `Domain`; current missing consumers do not justify host-shaped APIs or narrow wrappers.

## [4]-[EXECUTION_RULES]

- Expose one access path per folder; do not give every file its own consumer API.
- Model category intent as typed data when primitive parameters create ceremony or hide semantics.
- Convert native nullable, bool, disposable, and ownership semantics into typed rails at boundary adapters.
- Preserve Rhino predicate semantics; verify substitutions against local API evidence and runtime behavior before replacing native calls with algebraic approximations.

## [5]-[REJECTIONS]

- No wrapper-only abstractions around RhinoCommon or existing domain code.
- No local copies of reusable `Domain` validation, statistics, coercion, geometry-kind, or acceptance logic.
- No new public rail beside an existing owner without removing the obsolete path.
- No Vectors UI, preview, command, GH2 parameter, bake, product workflow, or app receipt surface; host and product packages consume vector receipts and projections through their own rails.
- No hardcoded invisible policy values; use named policies, native defaults, typed receipts, or caller input.
- No exposure of fixed kernel/native choices as public knobs unless they execute and change behavior.
