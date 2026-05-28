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

- RhinoCommon or GH2 APIs that only work inside RhinoWIP must be proven by `*.verify.csx` scenarios under the relevant `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/` folder.
- Static specs may classify bridge-owned behavior, but must not pretend to execute native runtime paths that fail outside Rhino.
- Pair each bridge scenario with an owning source file for `uv run python -m tools.quality bridge check <source> <scenario>`.
- Keep bridge scenarios source-only. Do not add `#r`, `#load`, or absolute build-output paths; bridge check owns reference projection and fresh artifact refs.
- Treat host dependency collisions as product/packaging evidence. Do not rewrite bridge scenarios into weaker static assertions just because Rhino has preloaded a conflicting assembly.
- For Rasm.Vectors on macOS, classify by actual call behavior rather than broad type names. Static specs may own managed factories, guards, smart-enum dispatch, matrix/math laws, and failure categories. Bridge scenarios own successful native sampling/projection/materialization and RhinoCommon calls that the current static run proves require RhinoWIP host state, including point-cloud topology, mesh topology/Laplacian/remesh/SDF/heat, curve/surface projections, Rhino validity/unitization, and mass properties.
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

## [5][POLYMORPHIC_DENSITY]

The following patterns convert O(N) per-case Facts into O(1) generated laws. Reach for them before adding a second Fact that shares setup with an existing one.

- **SmartEnum case sweep** — `Spec.Cases(MySmartEnum.Items, key: m => m.Key, law: m => ...)` enumerates every case, asserts key uniqueness, and runs a per-case invariant in one body. Auto-extends when cases are added.
- **Union case sweep** — `Gen.OneOf(/* factory-routed per-case generators */)` plus a single `Spec.ForAll` body that pattern-matches on case. The body either dispatches via pattern (when oracle differs) or calls a single oracle (when invariant is uniform).
- **Reflection-driven `TheoryData(T.Items)`** — `MemberData(nameof(CasesOf))` where `CasesOf` returns `TheoryData<T>` populated from `SmartEnum.Items`. Maintenance-free row count.
- **Product generator over (case × input × output)** — `Gen.OneOfConst([..Items]).Select(InputGen, Gen.OneOfConst(OutputFamily), ...)` and the body's signature carries all three axes. One Fact, three coverage dimensions.
- **Single fixture / N invariants** — one generated SPD matrix (or one generated mesh, etc.) asserts 5+ invariants per draw. Reuses the fixture cost across the oracle set.
- **Variable-driven dispatch with dispatch-free body** — oracle table mapping case → closed-form (e.g., `Spec.Cases(SdfPrimitives, p => p.ClosedFormDistance)`); body never contains a switch.

When NOT to polymorphize:
- The *oracle statement* changes per case (distinct closed-form per primitive, different operator algebra, disjoint state machines).
- The number of cases is 2 and likely to stay at 2.
- A failure on one case must be visible as a separately-tracked test ID for CI triage.

## [6][TORTURE_TACTICS]

- **Distinct-value product generators** — `[7.0, 13.0, 3.0]` over `[1.0, 1.0, 1.0]` catches transport/dispatch swaps that placeholder fixtures hide.
- **Stratified edge-bias `Gen.Frequency`** — 5-7 tier weights mix tail-of-distribution values (zero, infinity, NaN, tolerance-adjacent) with bulk-random while preserving statistical power.
- **Conditioning-aware tolerance** — for numeric algorithms, tolerance is `κ(A) × base`, not a hardcoded `1e-9`. The conditioning factor comes from the input generator, not from the test author guessing.
- **Composite metamorphic chain** — `f(g(h(x))) ≡ permuted_chain(x)` over generated `(x, perm)` pairs. Catches order-of-operation regressions in algorithmic pipelines.
- **Generative oracle ladder** — when the closed-form is unknown, climb: algebraic identity (Grade A) → metamorphic relation (Grade A) → smaller reference model (Grade B) → conditioning fuzz (Grade B+). Start at the top of the ladder achievable for the algorithm.
- **Pre/post triple for stateful APIs** — generated `(precondition, action, postcondition)` triples for `Atom` / `Validation` accumulation / `Fin` chaining. Single property body covers state-machine contract.

## [7][DEFAULT_STRUCT_LAWS]

Public `record struct` types with private constructors expose a `default` that bypasses the factory and produces an invalid value. Every such type needs a default-invalid raw-payload law per `cs-testing/SKILL.md [2][RAILS]`. Pattern:

```csharp
[Fact]
public void DefaultStructIsRejectedByValidation() =>
    Spec.Fail(Op.Of().AcceptValue(default(MyRecordStruct)));
```

Skip this only when the factory is internalized (no public construction path bypasses validation — see plan A.9 for the Intent positional-ctor lockdown case).
