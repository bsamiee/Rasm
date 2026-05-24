# [LIB_SPEC_MANIFEST]

[REQUIRED]: Follow `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/AGENTS.md` before editing library specs.

## [1][OWNERSHIP]

- Mirror source ownership: one spec file should target one owning source file or one tight source concern.
- Add a new spec only when the source file has no truthful owner. Example: `Vectors/Space.spec.cs` owns `Vectors/Space.cs`.
- Move laws out of broad specs when a more precise owner appears; do not duplicate the same behavior in two specs.
- Keep module-local generators inside the spec until at least two specs need the same generator shape.

## [2][USING_THE_TESTKIT]

- Import `/Users/bardiasamiee/Documents/99.Github/Rasm/tests/csharp/_testkit` through the `Rasm.TestKit` project reference; do not create module-local shared helper files.
- Use `Spec.Valid` and `Spec.Invalid` for `Validation<Error,T>`, `Spec.Succ` and `Spec.Fail` for `Fin<T>`, and `Spec.FailCategory`/`Spec.FailMany` for durable diagnostics.
- Use `Gens.Context`, `Gens.Dimension`, `Gens.PositiveMagnitude`, `Gens.UnitInterval`, and edge-biased tolerances before inventing local numeric generators.
- Use `Numeric` whenever a matrix, vector, eigen, inverse, residual, or reconstruction oracle needs to be independent from production code.
- Use `Spec.Cases` or `Spec.SmartEnumKeysUnique` for bounded catalogs instead of repeating tiny key facts.
- Keep model-based checks spec-local unless two source owners need the same actual-vs-model adapter.
- Add `_testkit` surface only after the second consumer appears, or when the primitive is a cross-rail law/oracle that prevents circular tests across many future specs.
- Promote shared testkit code only after naming the higher-order purpose it owns. Valid promotions are law/oracle/generator capability, not copy-paste extraction or shorter spelling for one owner.
- In vector specs, a declared output type must be paired with an actual projection law for at least one static-safe payload. Metadata-only checks are not enough.
- Do not use production vector operators, projections, or kernels as expected-value engines for the same behavior. Use `Numeric`, a closed form, a smaller model, or a bridge observation.

## [3][BRIDGE_BOUNDARIES]

- RhinoCommon or GH2 APIs that only work inside RhinoWIP must be proven by `*.verify.csx` scenarios under `/Users/bardiasamiee/Documents/99.Github/Rasm/apps/grasshopper/Radyab/Scenarios`.
- Static specs may classify bridge-owned behavior, but must not pretend to execute native runtime paths that fail outside Rhino.
- Pair each bridge scenario with an owning source file for `scripts/rhino.sh bridge check-source <source> --script <scenario>`.
- Treat host dependency collisions as product/packaging evidence. Do not rewrite bridge scenarios into weaker static assertions just because Rhino has preloaded a conflicting assembly.
- For Rasm.Vectors on macOS, assume Rhino native geometry validity/materialization can cross into `rhcommon_c` unless a current static run proves otherwise. Static specs may own managed guards and failure categories for `Curve`, `Surface`, `Mesh`, `PlaneSurface`, `Point3d.IsValid`, `Vector3d.IsTiny`, and `Polyline.IsValid`, but successful native sampling/projection belongs in bridge scenarios.
- Record bridge-owned gaps as executable scenario work, not skipped xUnit tests or shape-only assertions.
- Vector bridge-owned successes include ICP/RTree point-cloud ordering, native contours/materialization, mesh topology/Laplacian/remesh/SDF/heat, curve/surface projections, Rhino validity/unitization, and mass properties.

## [4][DENSITY]

- Normal specs target 175 LOC. A spec may reach 176-200 LOC only when one source owner has multiple real concepts and the result is denser than a split.
- Prefer one law-packed fact over several narrow facts when the same generated sample can attack closure, dispatch, unsupported outputs, and invariants together.
- Avoid circular tests, snapshots of implementation structure, and assertions on mutable incidental ordering unless ordering is the contract.
- Keep every snapshot, benchmark, or fuzz target outside library specs unless the artifact is the behavior under test.
- Prefer one generated law table that varies inputs, output kinds, failure categories, and invariants over many one-assertion facts.
- Specs may reach 225 LOC, and exceptionally 300 LOC, only when every added line buys a real oracle, boundary, native classification, or product-bug guard that cannot be expressed more densely through existing `Spec`, `Gens`, `Numeric`, or local case tables.
- If a spec starts to grow through repeated setup, collapse the repetition into a richer product generator or table inside the same owner before adding helper files.
- Thin tests found during review are bugs in the test design. Replace them with multi-axis laws, delete them when they only mirror implementation, or classify the behavior as bridge-owned/product-direction instead of preserving low-value coverage.
