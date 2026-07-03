# [ELEMENT_PROJECTION]

The cross-stratum alignment seam: TWO instance-interface floors the AEC peers depend up on and implement — aligning by contract without referencing each other — plus the `Assemble` composition CAPABILITY the apps wire over them. `IElementProjection` has ONE polymorphic `Fin<GraphDelta> Project(ProjectionContext ctx)`: each concrete projector (the `Rasm.Bim` `SemanticProjector` over GeometryGym, the `Rasm.Materials` `ComponentProjector` over VividOrange, a future `Rasm.Fabrication` projector) captures its foreign source INTERNALLY and lowers it onto a `Graph/delta#GRAPH_DELTA` `GraphDelta` through the `Put`/`Link`/`Reheader` builders, so the seam folds deltas and never learns a provider surface. `IGraphConstraint` has ONE `Validation<Error,Unit> Validate(GraphDelta, ElementGraph)` — the IFC-semantic legality Bim implements, disjoint from the seam's structural edge law the two-interface split [M3] fixes. `Assemble` runs the capture→merge→establish→admit→constrain→fold pipeline — projector faults accumulated applicatively through the `Try.lift` funnel, the monoid fold seeded with `ctx.Header`, the merged delta admitted through `Graph/delta#GRAPH_DELTA` `GraphDelta.AdmitOnto` — returning the assembled `ElementGraph` a consumer bakes PLUS the merged `GraphDelta` event body Persistence appends [C1]. The WIRING — projector registration, tessellation adapter, `Graph/element#NODE_MODEL` `GeometrySource` port, DI — is an APP / HOST-BOUNDARY composition-root concern (`Rasm.Rhino`/`Rasm.Grasshopper` today, future standalone/web/sidecar apps). `ProjectionContext` carries the element `NodeId` set (the `Owns` vouch an aspect projector authors within — the owner of each concept mints its `Object` BEFORE assembly under the owner-mints-its-identity law), the target `Header`, the kernel `Op` key, and the NEUTRAL runtime primitives (instant, correlation, tenant) the app supplies. IFC egress (`Emit`) is Bim-INTERNAL, never a seam member.

## [01]-[INDEX]

- [02]-[PROJECTION_CONTRACT]: the `IElementProjection` projector floor and its one `Project`, the `ProjectionContext` element-identity/header/primitives carrier with the `Owns` vouch predicate, and the `Assemble` composition capability the apps wire.
- [03]-[GRAPH_CONSTRAINT]: the `IGraphConstraint` IFC-semantic legality floor Bim implements, composed in `Assemble` after the structural admission, accumulating every violation applicatively.

## [02]-[PROJECTION_CONTRACT]

- Owner: `IElementProjection` the projector strategy floor with one `Project`; `ProjectionContext` the projection input (element `NodeId` set + target `Header` + neutral runtime primitives, carrying the `Owns` vouch predicate); `ProjectionAssembly` the static composition capability the seam owns and the apps wire.
- Entry: `IElementProjection.Project(ProjectionContext ctx)` lowers a concrete projector's captured foreign source onto a `GraphDelta` over the context's element identities, `Fin<T>` carrying the projector's own faults; `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx)` runs the capture→merge→establish→admit→constrain→fold pipeline returning `(ElementGraph Graph, GraphDelta Delta)`; `ProjectionContext.For(elementIds, header, key, at, correlation, tenant)` builds the context from the app's kernel `Op`, clock, correlation, and tenant primitives, `elementIds` carrying the owner-minted Type and Occurrence identities an aspect projector vouches against.
- Auto: `Assemble` (1) captures each `Project` through `Try.lift` — a thrown foreign exception becomes `ProjectionFailed`, a projector's own returned typed fault (a `BimFault`, a `ProjectionFault`) is preserved unchanged — and accumulates every projector fault applicatively (`.Traverse(...).ToValidation()`, `Error.Combine` unioning the faults so a run where BOTH Bim and Materials fail reports both); (2) on full success seeds the monoid fold with `GraphDelta.Empty.Reheader(ctx.Header)` and merges via `Merge`'s `next.Header`-wins rule; (3) structurally admits the merged delta through `GraphDelta.AdmitOnto` — the validating sibling of the raw persistence `ReplayOnto`, routing through `WorkingGraph.Apply` so `LegalLink` runs per `Link`; (4) validates against every `IGraphConstraint`, accumulating all legality violations; (5) folds onto the seed graph.
- Receipt: the `Assemble` result is the assembled `ElementGraph` a consumer bakes PLUS the merged `GraphDelta` event body the `Rasm.Persistence` `Version/ledger` appends to the Marten stream (the one model-creating event, never a whole-graph snapshot) [C1].
- Packages: LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Try` + the `TraverseM`/`Traverse` accumulation split + the `Fold` monoid + `ToValidation`/`ToFin` cross-rail bridges), `Rasm` (the kernel `Op` op-key), NodaTime (`Instant`), System.Collections.Frozen (`FrozenSet`).
- Growth: a new aspect projector is one `IElementProjection` implementation in its owning package plus one registration row at the app root; a new runtime primitive is one column on `ProjectionContext`; net new Rasm interfaces for the whole rebuild = 2 (`IElementProjection` + `IGraphConstraint`) — the unified Material/Component/Element paradigm adds NO new interface and NO new mint method, both mints being owner-side compositions of the kernel `Graph/element#NODE_MODEL` `NodeId` floor.
- Boundary: `IElementProjection` is the ONE projector floor with ONE polymorphic `Project` — a per-provider seam method (`ProjectBim`/`ProjectMaterials`) is the deleted form, the concrete held internal and swappable in its owning package; the projector-capture fold is APPLICATIVE and the admit-then-constrain tail MONADIC — the carrier, never a flag, selects the algebra ([APPLICATIVE_CAPTURE]); `Owns` is a PREDICATE, not an authoring helper — the skip-vs-rail policy is the projector's [H12] ([OWNER_MINTS_IDENTITY]); `ctx.Header` is the floor-seeded model header a projector-authored `Reheader` overrides ([HEADER_ESTABLISHMENT]); the runtime primitives are NEUTRAL (kernel `Op`, instant, correlation, tenant) — the seam references no AppHost `ClockPolicy`/`CorrelationId`/`TenantContext` type; the capture funnel is `Try.lift`, never the kernel `Op.Catch`; the projection path admits through `AdmitOnto`, never the raw `ReplayOnto`; the seam owns the `Assemble` CAPABILITY and the app the WIRING — no APP-PLATFORM package hosts the live assembly ([ASSEMBLE_CAPABILITY]); IFC egress (`Emit`) is Bim-INTERNAL because the seam never authors IFC; an instance default-interface-method on either floor is the named defect — defaults derive from a minimal core, never an interface body.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [MODELS] -----------------------------------------------------------------------------
// The projection input: the element identity set an aspect projector authors WITHIN (Owns gates it), the target
// Header the fold seeds (LOAD-BEARING, never a dead field), the kernel Op key, and NEUTRAL runtime primitives —
// no app-platform ClockPolicy/CorrelationId/TenantContext type crosses the seam.
public sealed record ProjectionContext(
    FrozenSet<NodeId> ElementIds,
    Header Header,
    Op Key,
    Instant At,
    Guid CorrelationId,
    string TenantId) {
    public static ProjectionContext For(Seq<NodeId> elementIds, Header header, Op key, Instant at, Guid correlation, string tenant) =>
        new(elementIds.ToFrozenSet(), header, key, at, correlation, tenant);

    // The aspect-projector vouch — a PREDICATE, never an authoring helper; the skip-vs-rail policy is the
    // projector's [H12]. The SEMANTIC vouch, complementary to the structural law AdmitOnto runs.
    public bool Owns(NodeId element) => ElementIds.Contains(element);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The projector strategy each AEC peer implements over its captured foreign source — one polymorphic Project lowering
// onto a GraphDelta, the concrete (Bim SemanticProjector over GeometryGym, Materials ComponentProjector over
// VividOrange) held internal and swappable in its owning package, so the seam folds deltas and never learns a provider.
public interface IElementProjection {
    Fin<GraphDelta> Project(ProjectionContext ctx);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The seam-owned composition CAPABILITY (the apps own the wiring): capture applicatively, floor-seed the fold
// with ctx.Header (next.Header-wins), admit structurally, constrain, fold — the graph PLUS the event-body delta.
public static class ProjectionAssembly {
    public static Fin<(ElementGraph Graph, GraphDelta Delta)> Assemble(
        Seq<IElementProjection> projectors, Seq<IGraphConstraint> constraints,
        ElementGraph seed, ProjectionContext ctx) =>
        projectors
            .Traverse(projector => Capture(projector, ctx).ToValidation()).As()
            .Map(deltas => deltas.Fold(GraphDelta.Empty.Reheader(ctx.Header), static (acc, delta) => acc.Merge(delta)))
            .ToFin()
            .Bind(merged => merged.AdmitOnto(seed, ctx.Key)
                .Bind(applied => Constrain(constraints, merged, applied.Graph).Map(_ => applied)));

    // A THROWN foreign call becomes ProjectionFailed with the raw message (the MapFail lambda closes over ctx.Key,
    // never static); a projector's OWN typed fault passes untouched — Run yields Succ(Fail(x)), MapFail no-ops on
    // the outer Succ, Bind surfaces x.
    static Fin<GraphDelta> Capture(IElementProjection projector, ProjectionContext ctx) =>
        Try.lift<Fin<GraphDelta>>(() => projector.Project(ctx)).Run()
            .MapFail(error => ElementFault.ProjectionFailed(ctx.Key, error.Message))
            .Bind(static fin => fin);

    // Composed AFTER the structural admission: the applicative Traverse accumulates ALL violations, .ToFin() carrying
    // the ManyErrors intact so a failure aborts with the FULL report; an empty constraint set admits vacuously.
    static Fin<Unit> Constrain(Seq<IGraphConstraint> constraints, GraphDelta delta, ElementGraph graph) =>
        constraints.Traverse(constraint => constraint.Validate(delta, graph)).As().Map(static _ => unit).ToFin();
}
```

## [03]-[GRAPH_CONSTRAINT]

- Owner: `IGraphConstraint` the IFC-semantic legality floor with one `Validate`, Bim-implemented, composed in `Assemble` after the seam's structural admission.
- Entry: `IGraphConstraint.Validate(GraphDelta delta, ElementGraph graph)` returns `Validation<Error,Unit>` accumulating every legality violation a delta introduces against the assembled graph — a containment edge whose relating node is not a spatial structure, a `Void` whose host is not an element or whose feature is not an opening, a `Compose` whose whole is a `Type` object — each a typed `Error` the applicative `Validation` collects so a malformed projection surfaces all violations at once.
- Auto: the structural edge law admits the merged delta's edges by endpoint-kind FIRST, then every `Validate` runs the IFC-semantic legality against the assembled graph — the constraint set folded through the applicative `.Traverse` so `Error.Combine` unions every fault before the boundary reports; the two layers are disjoint by design, the seam carrying no IFC vocabulary and the Bim constraint no structural mechanics; a constraint is a strategy the app registers alongside its projectors, so a non-IFC consumer registers none and the structural law alone admits.
- Receipt: the `Validation<Error,Unit>` is the legality verdict `.ToFin()` carries onto the rail with the accumulated `ManyErrors` intact, so a recovery reads `error.Filter<BimFault.ModelRejected>()` against the full report (the `Rasm.Bim/Projection/semantic#GRAPH_LEGALITY` `IfcLegality` case is the unsuffixed nested record, so the `Filter<E> where E : Error` probe names it directly); the structural admission produces a CANDIDATE graph the constraints validate against, an illegal projection's candidate discarded — `Assemble` returns the graph only when every constraint passes.
- Packages: LanguageExt.Core (`Validation`/`Error` + the applicative `Traverse`/`.As()`/`ToFin`).
- Growth: a new IFC-semantic rule is one arm in a Bim `IGraphConstraint` implementation; a new constraint family is one `IGraphConstraint` the app registers; never a structural-law rule on the constraint and never an IFC rule on the seam's `LegalLink`.
- Boundary: `IGraphConstraint` is the second seam interface [M3] — the IFC-semantic legality lives HERE and the structural edge law in `Graph/delta#GRAPH_DELTA` `LegalLink`, conflating the two the named defect; the return is `Validation<Error,Unit>` (accumulating), not `Fin` (fail-fast), because a projection's legality report is complete — the doctrinal accumulating-constraint floor foreign code supplies and the seam folds applicatively at the edge, never a case the owner closes; the app registers it alongside the projectors, so the seam composes it without referencing Bim; a default-interface-method carrying a rule is the named defect.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// The IFC-semantic legality the consumer implements, distinct from the seam's structural law — the open accumulating
// constraint floor (Validation<Error,Unit>, all violations at once), Bim-implemented and app-registered, folded
// applicatively in Assemble after the structural admission. A Fin (fail-fast) return is the deleted form.
public interface IGraphConstraint {
    Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph);
}
```

## [04]-[RESEARCH]

- [TWO_INTERFACE_SPLIT]: the net-new Rasm interface count is 2 [M3] — `IElementProjection` (the projector floor each AEC peer implements over its captured foreign source, lowering onto a `GraphDelta`) and `IGraphConstraint` (the IFC-semantic legality Bim implements, validating a delta against the graph) — so the seam's total `Switch` enforces ONLY the structural edge law and the schema legality lives in the consumer's constraint. This is the cross-stratum alignment pattern: a lowest-stratum seam hosts the instance-interface floors the closed-vocabulary siblings depend up on and implement, aligning by contract without sibling references, each package usable in isolation (Materials projects a material subgraph, Bim the IFC projection, Persistence persists any `ElementGraph`). Both floors are open points foreign code plugs into (the `OPEN_FLOOR_DISPATCH` form) — the projector returning `Fin` (one dependent lowering) and the constraint returning `Validation` (independent legality rules accumulating) — never a `[Union]` the foreign assembly extends and never an instance default-interface-member.
- [OWNER_MINTS_IDENTITY]: the OWNER of a concept mints its `Object` under the ONE rooted-identity regime (`Graph/element#NODE_MODEL` `ObjectKind ∈ {Type, Occurrence}`, no content-keyed-vs-rooted bifurcation for an `Object`). `Rasm.Materials` owns Component Types, so the `ComponentProjector` mints the DETERMINISTIC-rooted Type id through the kernel Type-seed `NodeId` derivation — rooted, yet a pure function of the `Object` canonical content with the volatile `Representations` EXCLUDED from the seed, so identical Components dedup to one Type and a later geometry attach (the heavy `Body` content hash) never re-keys it; the determinism is LOAD-BEARING, because a pure-function id is known BEFORE the projection runs and the owner seeds it into `ElementIds` with no minting race. An occurrence-authoring projector (a `SemanticProjector` ingesting an `IfcElement`, an app authoring from scratch) mints the Guid-v7 Occurrence id through `NodeId.Rooted()`, the IFC GlobalId riding a Bim-stored `Object.ExternalId` [H6]. An ASPECT projector mints NOTHING: it authors edges only INTO a context-vouched id through `ProjectionContext.Owns`, composing `ctx.Owns(element) ? Link(...) : ProjectionFault.Unvouched(...)` — the skip-vs-rail policy the projector's, because a pure-isolation run authoring no edge is no fault while a binding to an unvouched element MUST rail, never a silent drop [H12]. The mint-vs-vouch split is per-CONCEPT, not per-projector — the `ComponentProjector` mints the Type it OWNS and vouches the occurrence it BINDS through `Assign.TypeDefinition` in ONE `Project` — replacing the fragile minter-stamps-the-aspect-egress convention, the seam adding no interface and no mint method: both mints are owner-side compositions of the kernel `NodeId` floor.
- [APPLICATIVE_CAPTURE]: the projector-capture fold is APPLICATIVE, not monadic — projectors are INDEPENDENT, so the `INDEPENDENT_JOIN` law accumulates their faults: `projectors.Traverse(p => Capture(p, ctx).ToValidation()).As()` runs the `Validation` `Apply` over every captured projector and `Error.Combine` unions every foreign fault, where the rejected `.TraverseM`/`.Bind` capture silently discards every fault after the first. The per-projector `Capture` lowers a THROWN foreign exception through the `Try.lift(() => projector.Project(ctx)).Run().MapFail(...).Bind(identity)` funnel — `Try.lift` preserving the raw `error.Message` a bare kernel `Op.Catch` re-wraps as `Fault.InvalidResult` — while a projector's OWN returned typed fault is preserved unchanged, and `.ToValidation()` carries the typed `Expected`-derived case onto the accumulating carrier so the combine keeps each fault recoverable (`error.IsType`/`HasCode`/`Filter` recurse over `ManyErrors`). The SUBSEQUENT admit-then-constrain tail is MONADIC (`.ToFin().Bind(...)`) because the admission depends on the merged delta and the constraint on the admitted candidate — dependence licenses sequence, independence licenses accumulation, the carrier selecting the algebra.
- [HEADER_ESTABLISHMENT]: `ProjectionContext.Header` is LOAD-BEARING — `Assemble` SEEDS the monoid fold with it as the model-creating header FLOOR (`deltas.Fold(GraphDelta.Empty.Reheader(ctx.Header), Merge)`), so an assembly onto `Graph/element#ELEMENT_GRAPH` `Genesis` freezes under the intended schema/model-view/georeference/tolerance rather than the seed's default (`AdmitOnto` resolves `delta.Header.IfNone(graph.Header)` over the merged delta's resolved header). The precedence is the monoid's `next.Header`-wins rule over a floor-seeded accumulator: a header-less projector's `None` leaves the floor intact, while a PRIMARY projector's authored `Reheader` (a Bim lowering reading `FILE_SCHEMA`/`FILE_NAME`) overrides it — so the ingested schema and `StepHeader` provenance reach the frozen snapshot (the H8 schema-span validation and the H9 owner-history re-emit both read that surviving header). A trailing UNCONDITIONAL `Reheader(ctx.Header)` AFTER the fold clobbers every projector-authored header and defeats H8/H9; a context-carried header no fold reads is the prior dead-field form — both deleted. The `Header` rides the `GraphDelta` event and the frozen snapshot, never the `WorkingGraph`, so the establishment is one floor-seed `Reheader` call, not a header field threaded through the working form.
- [ASSEMBLE_CAPABILITY]: the seam owns the `Assemble` fold — the composition CAPABILITY — but NOT the wiring: registering the `Seq<IElementProjection>`, binding the imported-IFC tessellation adapter (the `IfcConvert`/`ifcopenshell` companion [M5]), wiring the `Graph/element#NODE_MODEL` `GeometrySource` content-key port over the `Rasm.Persistence` object store, wiring the tabular→element map, and the DI are per-app composition-root concerns (`Rasm.Rhino`/`Rasm.Grasshopper` today), no APP-PLATFORM package hosting them. A projector builds its delta through the `Put`/`Link`/`Reheader` builders — the structural edge law has not yet run on it — so `Assemble` MUST admit through `GraphDelta.AdmitOnto`, never the raw persistence `ReplayOnto`; the projector owning both endpoints authors the `Associate` material edge gated on the `Owns` vouch, the skip-vs-rail law `[OWNER_MINTS_IDENTITY]` fixes — an app-authored wire-side material edge is the deleted form.
