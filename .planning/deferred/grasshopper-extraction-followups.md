# [H1][GRASSHOPPER_EXTRACTION_FOLLOWUPS]
>**Dictum:** *Lock follow-on work to the boundary library substrate; never let the component re-grow.*

<br>

[IMPORTANT] Canonical roadmap for the next iteration of the Radyab Grasshopper2 plugin and `libs/csharp/analysis`. Each section names APIs, acceptance criteria, and excluded patterns so a future agent can execute item-by-item without re-deriving the design.

---
## [1][CONTEXT]
>**Dictum:** *Phases 1-4 closed the boilerplate gap; Phase 5+ encodes durable polymorphism.*

<br>

**Completed in this iteration:**

| [INDEX] | [PHASE]      | [DELIVERABLE]                                                                                                                                            |
| :-----: | ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | **Phase 1**  | `Bounds.Center`/`Bounds.Corners` polymorphic over `GeometryBase`/`Line`/`Polyline`/`BoundingBox`/`Box`; `SubD` vertex iteration via direct enumeration.  |
|   [2]   | **Phase 1**  | `GeometryContext.FromKnownUnits` (kept `internal`; cross-assembly access via existing `[InternalsVisibleTo("Analysis")]`); `Analyze.In(absolute, relative, angle, units)` overload added. |
|   [3]   | **Phase 2**  | New `libs/csharp/grasshopper/` library housing `Bridge.ResolveScope`, `Bridge.RunMany`, `Bridge.MissingInput`, `Bridge.WritePoints`, `Bridge.Warn`.      |
|   [4]   | **Phase 3**  | `ExtractPointsComponent` reduced to declarative descriptor table over `object`; ribbon at `Radyab > Extraction`; `Center` replaces `Spatial Midpoint`.   |
|   [5]   | **Phase 4**  | Managed specs for `Bounds.Center`/`Bounds.Corners` polymorphism; `Analyze.In(abs,rel,angle,units)` rail tests; runtime specs for Brep/Mesh `Bounds`.     |
|   [6]   | **Phase 6**  | `GeometryKind` public enum + `Query.Kind<TGeometry, TOut>()` polymorphic detector; `KindOfBrep` private dispatcher; `SemanticFault.PrimitiveNoEdges`/`PrimitiveNoVertices`; per-arm short-circuit in `Query.EdgeMidpoints`/`Query.Vertices` Brep arms for sphere/cylinder/cone/torus. |
|   [7]   | **Phase 6**  | +2 managed specs (`RejectsKindForUnsupportedOutputType`, `RejectsKindForUnsupportedGeometryType`); +6 runtime specs (`DetectsBrepSphereKind`, `DetectsBrepBoxKind`, `DetectsBrepGeneralKindForAsymmetricSolid`, `RejectsEdgeMidpointsForSpherePrimitive`, `RejectsVerticesForCylinderPrimitive`, `PreservesVerticesForBoxBrep`). |
|   [8]   | **Phase 7**  | Optimization audit closed at saturation: Bridge.cs (87 LOC) and Analysis library polymorphism reviewed; no further compression warranted under the no-helper-for-single-call-site rule. |

**Outstanding work** falls into four capability tracks (sections 2, 4, 5) and one quality track (section 8). Section [3][PRIMITIVE_AWARE_DISPATCH] is now CLOSED. Section 7 locks permanent exclusions per `handoff.md`. Section 9 captures non-obvious GH2 truths surfaced during implementation.

---
## [2][SHARED_MESSAGE_ACTIONS]
>**Dictum:** *Diagnostics route the user, not just inform them.*

<br>

**Location.** `libs/csharp/grasshopper/Bridge.cs` (extend) or sibling `libs/csharp/grasshopper/Messages.cs` if growth pressure justifies a second module.

**Motivation.** Components should emit actionable diagnostics with uniform vocabulary: "Connect to <type>", "Increase tolerance to <value>", "Open documentation". The `IDataAccess.AddWarning(text, details, MessageAction[])` overload exists; current components pass empty arrays.

**Primary GH2 APIs.**

| [INDEX] | [API]                                              | [SEMANTIC]                                                                  |
| :-----: | -------------------------------------------------- | --------------------------------------------------------------------------- |
|   [1]   | `Grasshopper2.Doc.MessageAction`                   | Concrete action attached to a remark/warning/error; rendered by canvas UI.  |
|   [2]   | `Grasshopper2.Doc.MessageDelegate`                 | Callback variant — invoked when the user activates the action.              |
|   [3]   | `IDataAccess.AddWarning(string, string, MessageAction[])` | Boundary surface that consumes the action array.                     |
|   [4]   | `IDataAccess.AddWarning(string, string, MessageDelegate[])` | Delegate variant for closure-based handlers.                       |

**Design sketch.** Static factories on `Bridge`:

- `Bridge.ConnectAction(IReadOnlyList<Type> supported)` advertises accepted types for the wire.
- `Bridge.RaiseToleranceAction(double suggestion)` opens the document tolerance dialog with a suggestion.
- `Bridge.DocumentationAction(string url)` opens the handbook entry.

Components consume via `access.AddWarning(name, details, Bridge.ConnectAction(supported: [typeof(Curve), typeof(Brep)]))`. Factories return `MessageAction[]` directly — no registry, no config table, no subclass.

**Acceptance criteria.**

- [ ] `ExtractPointsComponent.MissingInput` path emits a `ConnectAction` listing `Curve`, `Brep`, `Mesh`, `SubD`, `Box`, `BoundingBox`, `Line`, `Polyline`.
- [ ] When a Brep input cannot be converted to mesh for a mesh-only output, the warning carries a `MessageDelegate` that triggers `Brep.CreateFromMesh`-style conversion.
- [ ] Factory functions are pure: same arguments produce equal `MessageAction[]` instances by structural equality.
- [ ] All factories live in `Bridge`; no per-component `MessageAction` construction.

**Anti-patterns to avoid.**

- [ ] Configuration tables mapping component IDs to action sets — the action is a property of the diagnostic, not the component.
- [ ] Wrapping `MessageAction` in a domain record — use the GH2 type directly.
- [ ] Async or fire-and-forget delegates — `MessageDelegate` runs on the UI thread; honor the contract.

---
## [3][PRIMITIVE_AWARE_DISPATCH]
>**Dictum:** *Geometry kind precedes geometry algorithm.*

<br>

[STATUS] **CLOSED** in Phase 6. Implementation files: `libs/csharp/analysis/Query.cs` (`GeometryKind` enum, `KindKey`), `libs/csharp/analysis/Extract.cs` (`Query.Kind<TGeometry, TOut>()`, `KindOfBrep`, `EdgeMidpoints` Brep short-circuit, `Vertices` Brep short-circuit), `libs/csharp/core/Domain/Operation.cs` (`SemanticFault`). Tests: 2 managed in `tests/csharp/analysis/AnalysisSpec.cs`, 6 runtime in `tests/rhino/libs/analysis/AnalysisRuntimeSpec.cs`.

**Original motivation (preserved for archival).** A `Brep` representing a sphere should yield zero edges with the message "Sphere primitive has no real edges", not a misleading seam-edge midpoint. Today `Query.EdgeMidpoints` and `Query.Vertices` walk topology blindly.

**Primary RhinoCommon APIs.**

| [INDEX] | [API]                                | [SEMANTIC]                                                       |
| :-----: | ------------------------------------ | ---------------------------------------------------------------- |
|   [1]   | `Brep.IsSurface`                     | True for single-face Brep; gateway to primitive surface checks.  |
|   [2]   | `Surface.TryGetSphere(out Sphere)`   | Native primitive recognition for sphere.                         |
|   [3]   | `Surface.TryGetCylinder(out Cylinder)` | Native primitive recognition for cylinder.                     |
|   [4]   | `Surface.TryGetCone(out Cone)`       | Native primitive recognition for cone.                           |
|   [5]   | `Surface.TryGetTorus(out Torus)`     | Native primitive recognition for torus.                          |
|   [6]   | `Surface.TryGetPlane(out Plane)`     | Native primitive recognition for plane.                          |
|   [7]   | `Brep.IsBox(double tolerance)`       | Native primitive recognition for box.                            |

**Design sketch.** Add a typed enum and a single descriptor:

- `enum GeometryKind { Curve, Polyline, BrepGeneral, BrepSphere, BrepCylinder, BrepCone, BrepTorus, BrepBox, BrepPlane, Mesh, SubD, Surface, Sphere, Box, BoundingBox, Line }`
- `Query.Kind<TGeometry, GeometryKind>()` returning `Fin<GeometryKind>` via switch-expression dispatch over the existing `Query.Primitive<,>` rails.

Per-arm short-circuit lives in `Extract.cs`:

```text
EdgeMidpoints<Brep>:
    Query.Kind dispatch:
        BrepSphere   -> Fin.Fail(SemanticFault.SpherePrimitiveNoEdges)
        BrepCylinder -> Fin.Fail(SemanticFault.CylinderPrimitiveNoEdges)
        _            -> existing edge-midpoint walk
```

`SemanticFault` is a sibling to `OperationFault` in `Operation.cs:40-49` — typed extension methods on `OperationKey`. Each factory carries the operation key + primitive kind into the message ("Geometry operation 'EdgeMidpoints' rejects 'Sphere' primitive: no edges."); `Bridge.Warn` forwards `error.Message` to `IDataAccess.AddWarning`.

**Acceptance criteria.**

- [x] `Query.Kind<Brep, GeometryKind>()` returns `BrepSphere` for `Sphere.ToBrep()`, `BrepBox` for `Box.ToBrep()`, `BrepGeneral` for an asymmetric solid (capped cone).
- [x] `Query.EdgeMidpoints<Brep, Point3d>()` over a sphere Brep fails with `SemanticFault.PrimitiveNoEdges("Sphere")` on the failure rail.
- [x] `Query.Vertices<Brep, Point3d>()` over a cylinder Brep fails with `SemanticFault.PrimitiveNoVertices("Cylinder")`.
- [x] `Query.Vertices<Brep, Point3d>()` over a box Brep returns 8 corners (existing behavior preserved).
- [x] Semantic faults route through `Bridge.Warn` via the existing `IDataAccess.AddWarning` boundary; no per-component string assembly.

**Anti-patterns to avoid.**

- [ ] Configuration tables keyed by `GeometryKind` mapping to per-output policies — keep the dispatch as direct switch-expression arms.
- [ ] Polymorphism over `object` at this layer — `Query.Kind<TGeometry, GeometryKind>()` is type-parameterized; the discriminant is the input type.
- [ ] Adding a `IsPrimitive` boolean to existing descriptors — kind is structural information, not a flag.

---
## [4][TYPE_ASSISTANT_INPUTS]
>**Dictum:** *Convert at the boundary, dispatch on canonical type internally.*

<br>

**Location.** `apps/grasshopper/Radyab/*/AddInputs` overrides; helper in `libs/csharp/grasshopper/Bridge.cs`.

**Motivation.** `inputs.AddGeneric` accepts anything; users wiring a `Surface` to a `Brep`-expecting input get an opaque cast failure. GH2 supports type-assistant configuration for implicit conversion at the wire boundary.

**Primary GH2 APIs.** Verify exact names at edit time — the GH2 surface for type assistants has drifted across builds.

| [INDEX] | [API_CANDIDATE]                       | [SEMANTIC]                                                            |
| :-----: | ------------------------------------- | --------------------------------------------------------------------- |
|   [1]   | `InputAdder.AddGeneric` (current)     | Untyped input; no conversion; receives `object?`.                     |
|   [2]   | `InputAdder.AddCurve`/`AddBrep`/etc.  | Typed inputs with native conversion; richer error surface.            |
|   [3]   | `IPin.TypeAssistant`                  | Per-pin conversion strategy attached after `AddGeneric`.              |

**Design sketch.** Add `Bridge.AddElement(InputAdder inputs, string name, Type[] accepted)` that inspects the accepted set and chooses between `AddGeneric` (multi-type) and a typed adder when the set is a single type. For multi-type inputs, the helper attaches a type assistant covering `Curve`, `Brep`, `Mesh`, `SubD`, `Box`, `BoundingBox`, `Line`, `Polyline`. Internal handling stays `object`.

**Acceptance criteria.**

- [ ] Wiring a `Surface` to `ExtractPointsComponent.Element` automatically converts to `Brep` and yields valid outputs — no manual `ToBrep` upstream.
- [ ] Wiring an unsupported type (e.g., `Plane`) produces a typed error with a `Bridge.ConnectAction` listing the accepted types.
- [ ] `Bridge.AddElement` is reused by every future Radyab component — no per-component conversion logic.
- [ ] The conversion happens at the GH2 boundary; component `Process` still dispatches on `object`.

---
## [5][LIFECYCLE_HOOKS]
>**Dictum:** *One scope per solve, not one per item.*

<br>

**Location.** `libs/csharp/grasshopper/Bridge.cs` (extend), `Component` lifecycle overrides in client components.

**Motivation.** Twig/tree access components call `Process` once per item. `Bridge.ResolveScope` rebuilds `Analyze.Scope` every call — three `IDataAccess.GetTolerance` round-trips per item. One scope per solve, not one per item.

**Primary GH2 APIs.**

| [INDEX] | [API]                                       | [SEMANTIC]                                                                |
| :-----: | ------------------------------------------- | ------------------------------------------------------------------------- |
|   [1]   | `Component.BeforeProcess(IDataAccess)`      | Invoked once per solve before the first `Process` call.                   |
|   [2]   | `Component.PostProcess(IDataAccess)`        | Invoked once per solve after the last `Process` call.                     |
|   [3]   | `Component.PreProcess(IDataAccess)`         | Invoked before each `Process` (per-item).                                 |
|   [4]   | `Component.PostProcessTree(IDataAccess)`    | Invoked after tree-mode iteration completes.                              |
|   [5]   | `IDataAccess.CustomData` (`FleetingCustomData`) | Per-solve scratch keyed by `object`; lifetime bounded by solve cycle. |
|   [6]   | `Document.CustomValues` (`KeyedValues`)     | Cross-solve persistence on the GH2 document.                              |

**Design sketch.** Add `Bridge.CachedScope(this IDataAccess access)`:

- Checks `access.CustomData` (the `FleetingCustomData` per-solve scratch) for a sentinel key; on miss, calls `Bridge.ResolveScope` and stores; on hit, returns the cached scope.
- Cache key is `typeof(Analyze.Scope)` — single scope per solve regardless of caller.
- Components opt in by calling `access.CachedScope()` instead of `access.ResolveScope()`.

`BeforeProcess`/`PostProcess` are not strictly required if `CustomData` lifetime is solve-scoped; expose them for components needing pre-warmed state (e.g., a once-per-solve `SpatialIndex` build).

**Acceptance criteria.**

- [ ] Two consecutive `access.CachedScope()` calls within the same solve return reference-equal `Analyze.Scope` instances.
- [ ] A new solve cycle (re-trigger via canvas) produces a fresh scope (no leak across solves).
- [ ] `ExtractPointsComponent` and one tree-mode component both consume the same cached scope without coordination.
- [ ] `IDataAccess.CustomData` cleanup is automatic (GH2 `FleetingCustomData` contract); no manual disposal.

**Anti-patterns to avoid.**

- [ ] Static caches on `Component` subclasses — break solve isolation.
- [ ] `WeakReference` or `ConditionalWeakTable` keyed on document — `IDataAccess.CustomData` is the supported channel for per-solve state.
- [ ] Caching `Analyze.Scope` longer than a solve — tolerance/units may change between solves; use `Document.CustomValues` only for genuinely cross-solve persistence.

---
## [6][UNIVERSAL_NAMING_AND_REUSE]
>**Dictum:** *Vocabulary locked once; never re-derived per component.*

<br>

Rules locked from the Phase 1-4 implementation. Future components inherit these without restating.

| [INDEX] | [RULE]                                    | [LOCKED_FORM]                                                                                         |
| :-----: | ----------------------------------------- | ----------------------------------------------------------------------------------------------------- |
|   [1]   | Output naming                             | `Center` (intelligent center), `Edge Midpoints`, `Vertices`. No `Spatial Midpoint`, no per-type variants. |
|   [2]   | Helper class name                         | `Bridge` — never `Component`. The name `Component` collides with `Grasshopper2.Components.Component`.  |
|   [3]   | Output declaration shape                  | `static readonly Seq<PointOutput<TGeometry>>` table; `Process` is a fold over the table.               |
|   [4]   | LanguageExt usings                        | Explicit per-file `using LanguageExt;`/`using static LanguageExt.Prelude;`. `UseWorkspaceLibraries=false` on plugin csproj forbids implicit globals. |
|   [5]   | Nickname parameter at GH2 boundary        | `code:` — not `nickname:`. The parameter name is `code` in `OutputAdder.AddPoint` and `InputAdder.AddGeneric`.|
|   [6]   | Named arguments at GH2 boundaries         | Mandatory. GH2 API parameter order has drifted; positional arguments break across releases.            |
|   [7]   | Boundary translation locality             | Type assistants and conversion live in `Bridge`; component `Process` dispatches on canonical types.   |
|   [8]   | Per-component boilerplate                 | Forbidden. All `IDataAccess` plumbing lives in `Bridge`; component is descriptor table + `Nomen` + override stubs. |

**Verification posture.** Every new component diff must be reviewable as: descriptor table, constructor, three override stubs (`AddInputs`/`AddOutputs`/`Process`), zero local helpers. If a helper appears in a component, it belongs in `Bridge`.

---
## [7][PERMANENTLY_REJECTED]
>**Dictum:** *Locked exclusions are architecture, not backlog.*

<br>

These items are permanently out of scope per `handoff.md` and the conventions established in Phases 1-4. Reopening requires a named native Rhino API or approved standard, not vague future interest.

| [INDEX] | [ITEM]                                                | [REASON]                                                                              |
| :-----: | ----------------------------------------------------- | ------------------------------------------------------------------------------------- |
|   [1]   | Healing, mutation, repair operations                  | `analysis` is read-only; mutation belongs outside this library.                       |
|   [2]   | Aggregate quality/fairness/FEA scores                 | Score semantics are subjective; already decomposed into explicit evidence streams.    |
|   [3]   | Workflow facades, request/result records              | Conflict with descriptor-first typed rails; duplicate `Analyze`/`Query` ownership.    |
|   [4]   | Configuration tables, score bags                      | Reintroduce broad result bags and folder-per-capability APIs.                         |
|   [5]   | Spatial clustering, fields, proximity systems         | Require broad algorithms and knobs outside native `RTree` ownership.                  |
|   [6]   | Broad classifiers, feature/pattern detection          | Reintroduce request/result/config architecture.                                       |
|   [7]   | Auto-detect conformance                               | Conflicts with explicit primitive-pair API; encourages `object?` result bags.         |
|   [8]   | Custom spatial IDs                                    | Source indices are the canonical stable identity.                                     |

---
## [8][COVERAGE_AND_MUTATION_FOLLOWUPS]
>**Dictum:** *Every arm of every dispatch is exercised.*

<br>

**Outstanding.**

- [ ] **`Box` arm in `ExtractBounds<TGeometry>`** — the `Box box => Fin.Succ(box.BoundingBox)` arm in `Measure.cs` still lacks a direct managed spec. Phase 4 cleanup added `Line`/`Polyline` arm coverage but `Box` is exercised only indirectly via `ComputesBoxAnalysis`. Add `Query.Bounds<Box, Point3d>(Bounds.Center)` over `new Box(plane: Plane.WorldXY, ...)` fixture.
- [ ] **`Query.Kind<object, GeometryKind>()` Unknown path** — added in Phase 6, not exercised by any test. Add a managed spec asserting `Unknown` for non-Rhino input cast to `object`.
- [ ] **`BrepPlane` arm in `KindOfBrep`** — added in Phase 6, no test fixture; Stryker would flag this as a surviving mutant. Add a runtime spec via `Brep.CreatePlanarBreps` or equivalent.
- [ ] **Stryker mutation pass** — execute `dotnet stryker` against `ExtractBounds`, `Bounds.Center`/`Corners`, `KindOfBrep`, and the new `EdgeMidpoints`/`Vertices` Brep short-circuits. Project policy: `high: 80`, `low: 60`, `break: 50`. Surviving mutants on the new arms block close-out.
- [ ] **Runtime parity for `SubD.Vertices` enumeration** — `IteratesSubDVerticesInControlNetOrder` now exists; confirm under `RASM_RHINO_TESTS=1` once the `Rhino.Testing` testhost asset for `net10.0` ships.

**Closed since Phase 4 cleanup.**

- [x] `Line`/`Polyline` arms in `ExtractBounds` — covered by `ComputesBoundsCenterAndCornersOverLine` and `ComputesBoundsCenterAndCornersOverPolyline`.
- [x] `BoundingBox` arm — covered by `ComputesBoundsCenterPolymorphicallyOverObject` (passes a `BoundingBox` cast to `object`).

**Acceptance criteria.**

- [ ] All five `ExtractBounds` arms (`GeometryBase`, `Line`, `Polyline`, `BoundingBox`, `Box`) have at least one managed spec; Phase 4 closed three of five, Phase 7 must close `Box` and verify `GeometryBase` via Brep runtime spec.
- [ ] Stryker mutation kill rate `>= 80` on the new arms (Phase 1 `ExtractBounds` arms + Phase 6 `KindOfBrep` + Brep short-circuits); surviving mutants documented or killed.
- [ ] Runtime spec for SubD vertex enumeration runs successfully under `RASM_RHINO_TESTS=1` (currently gated; compiles green).

---
## [9][PLAN_VS_REALITY_LESSONS]
>**Dictum:** *GH2 API surface drifts; verify accessor and parameter names at edit time.*

<br>

Non-obvious truths surfaced during Phase 1-4 implementation. Future agents should treat these as load-bearing.

| [INDEX] | [LESSON]                            | [DETAIL]                                                                                                                |
| :-----: | ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Nomen` parameter order             | `Nomen(name, info, slot, rank, chapter, section)` — `chapter`/`section` positional 5/6. Named arguments mandatory.      |
|   [2]   | Nickname parameter is `code:`       | `OutputAdder.AddPoint(code: ...)` and `InputAdder.AddGeneric(code: ...)`. The historical `nickname:` parameter is gone. |
|   [3]   | `Angle` namespace                   | `Rhino.UnitSystem.Angle` wrapping struct. Member: `Angle.Radians` (property).                                           |
|   [4]   | `UnitSystem` is a wrapping struct   | `IDataAccess.GetUnitSystem(out UnitSystem)` returns the struct; pass directly to `Analyze.In(units:)`.                  |
|   [5]   | `UseWorkspaceLibraries=false`       | Plugin csproj forbids implicit globals. Every plugin file needs explicit `using LanguageExt;` + `using static LanguageExt.Prelude;`. |
|   [6]   | `Bridge` not `Component`            | Static helper class in `libs/csharp/grasshopper/` is `Bridge`. `Component` collides with `Grasshopper2.Components.Component`. |
|   [7]   | `Solution.Document` exists          | `IDataAccess.Solution.Document` returns the active GH2 document; gateway for cross-component `Solution.CustomData`.     |
|   [8]   | No `AddBrep` on `OutputAdder`       | `OutputAdder.AddPoint` exists; `AddBrep` does not. Brep-returning outputs use `AddGeneric`.                             |
|   [9]   | Lifecycle hooks opt-in              | `BeforeProcess`/`PostProcess` are real overrides; they fire only when components explicitly override.                   |
|  [10]   | `GetTolerance` returns `bool`       | A `false` return means the host could not supply the value; fall back to `Analyze.In(units: UnitSystem.Millimeters)` and emit `AddRemark` so the user is informed of the substitution. |
|  [11]   | `Box _` discard pattern             | `Query.Kind` Brep arm at `Extract.cs:243` uses `Box _ => ...` because `Box` (type) collides with the `Measure.Box<,>` private method name when used as a type-only switch pattern. |
|  [12]   | `KindOfBrep` co-located in `Extract.cs` | The internal Brep-kind dispatcher lives at `Extract.cs:533-547` (Analysis assembly), not in `Query.cs`. Three callers: `Query.Kind`, `EdgeMidpoints` Brep arm, `Vertices` Brep arm. |
|  [13]   | `SemanticFault` cross-assembly      | `internal static` in `Core.Domain.Operation.cs:40-49`; consumed from `Analysis` via `[InternalsVisibleTo("Analysis")]` at `libs/csharp/core/Properties/AssemblyInfo.cs:4`. Renaming `Analysis` would silently break access. |
|  [14]   | `Vertices` requires context         | Set `requiresContext: true` so that without a `Scope` the query fails loudly with `MissingContext` — primitive-Brep detection cannot silently degrade to seam vertex extraction. `EdgeMidpoints` already binds context via `Bind`, so its propagation is automatic. |
|  [15]   | Switch-statement in `Spatial.Dispose` | The original `switch (disposed) { case false: ... }` violated the no-statement rule. Replaced with `disposed = disposed switch { false => DisposeTree(), true => true };` at `Spatial.cs:135-142`. |

---
## [10][NEXT_BRANCH_GATING]
>**Dictum:** *Pick one section; pick a native API; ship it.*

<br>

**Recommended next branch.** Section [2][SHARED_MESSAGE_ACTIONS] — small surface, value-add at every component, ~30 LOC of Bridge factories. Section [3][PRIMITIVE_AWARE_DISPATCH] is now CLOSED; Section [5][LIFECYCLE_HOOKS] is the natural fallback when a tree-mode component lands and per-solve scope caching becomes load-bearing.

**Approval criteria before editing Section [2].**

- [ ] Verify `Grasshopper2.Doc.MessageDelegate` runtime contract (UI thread? blocking? cancellable?) against the GH2 binary at edit time.
- [ ] Define the three factories (`Bridge.ConnectAction`, `Bridge.RaiseToleranceAction`, `Bridge.DocumentationAction`) on the existing `Bridge` static class — no new file.
- [ ] Wire `ExtractPointsComponent.MissingInput` to consume `Bridge.ConnectAction(supported: typeof[Curve, Brep, Mesh, SubD, Box, BoundingBox, Line, Polyline])`.
- [ ] No configuration table mapping component IDs to action sets.
- [ ] Factories return `MessageAction[]` directly with structural equality on equivalent argument sets.

**Fallback if not approved.** Hold at the current state. Section [5][LIFECYCLE_HOOKS] becomes the next pick if a tree-mode component is added; Section [4][TYPE_ASSISTANT_INPUTS] remains gated on GH2 type-assistant API stability.
