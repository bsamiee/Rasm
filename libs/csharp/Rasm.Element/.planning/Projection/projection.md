# [ELEMENT_PROJECTION]

The cross-stratum alignment contract: TWO instance interfaces the AEC peers depend up on and implement, aligning by contract without referencing each other. `IElementProjection` has ONE polymorphic operation `Fin<GraphDelta> Project(ProjectionContext ctx)` — each concrete projector (the `Rasm.Bim` `SemanticProjector` over GeometryGym, the `Rasm.Materials` `MaterialProjector`, a future `Rasm.Fabrication` projector) captures its foreign source INTERNALLY and lowers it onto a `Graph/delta#GRAPH_DELTA` `GraphDelta` through the `Put`/`Link`/`Reheader` builders, so the seam never learns GeometryGym, VividOrange, or any provider surface — it folds deltas. `IGraphConstraint` has ONE operation `Validation<Error,Unit> Validate(GraphDelta, ElementGraph)` Bim implements with the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, a `Type` may not aggregate an `Occurrence`) the seam's structural `Graph/delta#GRAPH_DELTA` law does NOT carry — the two-interface split, the seam owning the structural floor and the constraint the schema legality. The seam owns the composition CAPABILITY — the `Assemble` fold that captures each `Seq<IElementProjection>` projector's delta (a thrown foreign exception railed `ElementFault.ProjectionFailed` at its boundary), merges them into the ONE model-creating `GraphDelta` via the cancellation-correct monoid, STRUCTURALLY admits the merged delta through the seam's edge law, validates it against a `Seq<IGraphConstraint>`, and folds it onto a seed graph — returning the assembled `ElementGraph` a consumer bakes PLUS the merged `GraphDelta` event body Persistence appends to the Marten stream; but NOT the wiring: registering the projectors, binding the tessellation adapter, and running against a live source is an APP / HOST-BOUNDARY composition-root concern, owned per-app (`Rasm.Rhino`/`Rasm.Grasshopper` today, future standalone/web/sidecar apps). `ProjectionContext` carries the element `NodeId` set so an aspect projector never invents element identities, plus the neutral runtime primitives (instant, correlation, tenant) the app supplies; a PRIMARY projector mints rooted ids through the kernel `Graph/element#NODE_MODEL` `NodeId.Rooted()` floor before assembly. IFC EGRESS (`Emit`) is Bim-INTERNAL, NOT a seam member. The page composes `Graph/element#ELEMENT_GRAPH`, `Graph/delta#GRAPH_DELTA` (the `WorkingGraph.Apply` structural law and the `GraphDelta` monoid), and captures a thrown projector call through the doctrinal `Try.lift` funnel; a projector fault rails `Projection/fault#FAULT_BAND` `ElementFault.ProjectionFailed`.

## [01]-[INDEX]

- [01]-[PROJECTION_CONTRACT]: the `IElementProjection` projector floor and its one `Project`, the `ProjectionContext` element-identity-plus-primitives carrier (the `Owns` aspect-gate; rooted ids minted through `Graph/element#NODE_MODEL` `NodeId.Rooted()` before assembly), and the `Assemble` composition capability the apps wire — collecting the projector deltas, merging them, structurally admitting and constraining the merged delta, and returning the assembled `ElementGraph` plus its `GraphDelta` event body.
- [02]-[GRAPH_CONSTRAINT]: the `IGraphConstraint` IFC-semantic legality floor Bim implements, distinct from the seam's structural edge law, composed in `Assemble` after the structural admission.

## [02]-[PROJECTION_CONTRACT]

- Owner: `IElementProjection` the projector strategy floor with one `Project`; `ProjectionContext` the projection input (element `NodeId` set + target `Header` + neutral runtime primitives); `ProjectionAssembly` the static composition capability the seam owns and the apps wire.
- Entry: `IElementProjection.Project(ProjectionContext ctx)` lowers a concrete projector's captured foreign source onto a `GraphDelta` over the context's element identities, `Fin<T>` carrying the projector's own faults; `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx)` captures and merges every projector's delta, structurally admits the merged delta, validates it against the constraints, and folds it onto the seed — returning `(ElementGraph Graph, GraphDelta Delta)`; `ProjectionContext.For(elementIds, header, key, at, correlation, tenant)` builds the context the app fills from its kernel `Op`, clock, correlation, and tenant primitives.
- Auto: `Assemble` captures each projector's `Project` through the doctrinal `Try.lift` funnel — a thrown foreign exception (a GeometryGym parse fault, a VividOrange miss) becomes `ProjectionFailed` rather than crossing the boundary, while a projector's own returned typed fault (a Bim `BimFault`, a Materials `ProjectionFault`) is preserved unchanged — then merges the captured deltas into the one model-creating `GraphDelta` via the cancellation-correct monoid, STRUCTURALLY admits the merged delta through `Graph/delta#GRAPH_DELTA` `GraphDelta.AdmitOnto` (the structural-validating sibling of the raw `ReplayOnto` — routing the changes through `WorkingGraph.Apply` so the seam's `LegalLink` generated total `Switch` runs per `Link`, the projection path the persistence `ReplayOnto` raw-replay does not validate), validates the merged delta against every `IGraphConstraint` (the `Validation<Error,Unit>` accumulating all legality violations applicatively), and folds it onto the seed graph; the `ProjectionContext` carries the element `NodeId` set so an aspect projector (Materials projecting only the material subgraph) authors edges into the existing element identities rather than minting element ids — the projector owning BOTH endpoints authors the `Associate` material edge, so a Materials-given-a-non-empty-element-set run authors element→material edges and a pure-Materials run authors only the material subgraph.
- Receipt: the `Assemble` result is the assembled `ElementGraph` a consumer bakes PLUS the merged `GraphDelta` event body the `Rasm.Persistence` `Version/ledger` appends to the Marten stream (the one model-creating event, never a whole-graph snapshot) — the seam owns the fold (the composition capability), the app owns the registration (`Seq<IElementProjection>` + the tessellation adapter + the tabular map + DI); adding a projector is one registration row at the app composition root, not a seam edit; IFC egress (`Emit`) is the Bim projector's INTERNAL operation reading the assembled graph, never a seam interface member.
- Packages: LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Try` + the `TraverseM`/`Traverse` accumulation split), `Rasm` (the kernel `Op` op-key), NodaTime (`Instant`), System.Collections.Frozen (`FrozenSet`).
- Growth: a new aspect projector is one `IElementProjection` implementation in its owning package plus one registration row at the app root; a new runtime primitive is one column on `ProjectionContext`; net new Rasm interfaces for the whole rebuild = 2 (`IElementProjection` + `IGraphConstraint`), the `IObjectFactory` floor reused; never an instance default-interface-member and never a per-provider seam method.
- Boundary: `IElementProjection` is the ONE projector floor with ONE polymorphic `Project` — a per-provider seam method (`ProjectBim`/`ProjectMaterials`) is the deleted form, each concrete projector capturing its foreign source internally and the concrete held internal/swappable in its owning package; the projector lowers onto a `GraphDelta`, so the seam folds deltas and never learns GeometryGym/VividOrange/any provider surface; `ProjectionContext` carries the element `NodeId` set so an aspect projector authors into existing identities rather than inventing them (the both-endpoints projector authors the `Associate` edge, `Owns` the gate), while a PRIMARY projector mints new rooted identities through the kernel `Graph/element#NODE_MODEL` `NodeId.Rooted()` floor BEFORE assembly and seeds them into `ElementIds` (the IFC GlobalId rides a Bim-stored `Object.ExternalId`), so the seam threads no ad-hoc mint and no instance mint method; the runtime primitives are NEUTRAL (kernel `Op`, instant, correlation, tenant) the app supplies — the seam references no AppHost `ClockPolicy`/`CorrelationId`/`TenantContext` type (those are app-platform, above the seam); the seam captures a thrown projector exception through the doctrinal `Try.lift` funnel into `ElementFault.ProjectionFailed` so a foreign `Exception` never crosses a seam signature, NOT the kernel `Op.Catch` (which re-wraps a thrown exception as the kernel `Fault.InvalidResult`, erasing the seam's typed arm); the structural edge law runs on the merged projector delta through `Graph/delta#GRAPH_DELTA` `GraphDelta.AdmitOnto` (the structural-validating sibling of the raw `ReplayOnto`, routing through `WorkingGraph.Apply` — the projection path the persistence `ReplayOnto` raw-replay does NOT validate, because a projector builds its delta through the `Put`/`Link` builders, never `WorkingGraph.Apply`), the IFC-semantic legality layered after as the `IGraphConstraint`; the seam owns the `Assemble` CAPABILITY, the app owns the WIRING — no APP-PLATFORM package hosts the live assembly; IFC egress (`Emit`) is Bim-INTERNAL, NOT a seam member, because the seam never authors IFC; an instance default-interface-method on either floor is the named defect — defaults derive from a minimal core, never an interface body.

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
// The projection input: the element identity set an aspect projector authors WITHIN (never minting element ids —
// Owns gates it), the target model Header a projector stamps its nodes against, the kernel Op fault-correlation key
// a projector rails its typed faults on, and the NEUTRAL runtime primitives the app supplies — the seam references
// no app-platform ClockPolicy/CorrelationId/TenantContext type, only the kernel Op and BCL primitives. A PRIMARY
// projector (Bim lowering an IfcRoot, an app authoring from scratch) mints the rooted element ids through the kernel
// `NodeId.Rooted()` floor (Graph/element#NODE_MODEL) BEFORE assembly and seeds them here; an ASPECT projector mints
// nothing, the IFC GlobalId riding a Bim-stored Object.ExternalId re-emitted at Emit.
public sealed record ProjectionContext(
    FrozenSet<NodeId> ElementIds,
    Header Header,
    Op Key,
    Instant At,
    Guid CorrelationId,
    string TenantId) {
    public static ProjectionContext For(Seq<NodeId> elementIds, Header header, Op key, Instant at, Guid correlation, string tenant) =>
        new(elementIds.ToFrozenSet(), header, key, at, correlation, tenant);

    // The aspect-projector gate: an aspect authors edges only INTO an element the context already declares, so it
    // never invents element identities — the projector owning BOTH endpoints (Materials given a non-empty element
    // set) authors the Associate material edge, a pure-Materials run (empty set) authors only the material subgraph.
    public bool Owns(NodeId element) => ElementIds.Contains(element);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The projector strategy each AEC peer implements over its captured foreign source — one polymorphic Project lowering
// onto a GraphDelta, the concrete (Bim SemanticProjector over GeometryGym, Materials MaterialProjector) held internal
// and swappable in its owning package, so the seam folds deltas and never learns a provider surface.
public interface IElementProjection {
    Fin<GraphDelta> Project(ProjectionContext ctx);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The seam-owned composition CAPABILITY — the apps own the registration of the projector sequence. Capture each
// projector's delta (a foreign exception railed ProjectionFailed at its boundary), merge them into the ONE
// model-creating GraphDelta via the cancellation-correct monoid, structurally admit the merged delta through the
// seam's edge law (LegalLink runs), validate it against every IGraphConstraint, and fold it onto the seed — returning
// the assembled ElementGraph a consumer bakes PLUS the merged GraphDelta event body Persistence appends to the stream.
public static class ProjectionAssembly {
    public static Fin<(ElementGraph Graph, GraphDelta Delta)> Assemble(
        Seq<IElementProjection> projectors, Seq<IGraphConstraint> constraints,
        ElementGraph seed, ProjectionContext ctx) =>
        projectors
            .TraverseM(projector => Capture(projector, ctx)).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)))
            .Bind(merged => merged.AdmitOnto(seed, ctx.Key)
                .Bind(applied => Constrain(constraints, merged, applied.Graph).Map(_ => applied)));

    // Capture the projector's foreign call (a GeometryGym/VividOrange call into host code): a THROWN exception becomes
    // ProjectionFailed through the doctrinal Try.lift funnel (the lambda closes over ctx.Key, NOT static; Try.lift keeps
    // the RAW message a kernel Op.Catch would re-wrap as Fault.InvalidResult), while a projector's OWN returned
    // typed fault (a Bim BimFault, a Materials ProjectionFault) is preserved unchanged — Run yields Succ(Fail(x)),
    // MapFail no-ops on the outer Succ, Bind surfaces x.
    static Fin<GraphDelta> Capture(IElementProjection projector, ProjectionContext ctx) =>
        Try.lift<Fin<GraphDelta>>(() => projector.Project(ctx)).Run()
            .MapFail(error => ElementFault.ProjectionFailed(ctx.Key, error.Message))
            .Bind(static fin => fin);

    // The IFC-semantic legality composed AFTER the structural admission: every constraint validates the merged delta
    // against the assembled graph, the applicative Traverse accumulating ALL violations via Error.Combine (not the
    // first), so a malformed projection's legality report is complete; an empty constraint set (a pure-geometry
    // assembly) admits on the structural law alone.
    static Fin<Unit> Constrain(Seq<IGraphConstraint> constraints, GraphDelta delta, ElementGraph graph) =>
        constraints.Traverse(constraint => constraint.Validate(delta, graph)).As().Map(static _ => unit).ToFin();
}
```

## [03]-[GRAPH_CONSTRAINT]

- Owner: `IGraphConstraint` the IFC-semantic legality floor with one `Validate`, Bim-implemented, composed in `Assemble` after the seam's structural admission.
- Entry: `IGraphConstraint.Validate(GraphDelta delta, ElementGraph graph)` returns `Validation<Error,Unit>` accumulating every legality violation a delta introduces against the assembled graph — a containment edge whose relating node is not a spatial structure, a `Void` whose host is not an element or whose feature is not an opening, a `Compose` whose whole is a `Type` object — each a typed `Error` the applicative `Validation` collects so a malformed projection surfaces all violations at once, not the first.
- Auto: the seam composes the constraints in `ProjectionAssembly.Assemble` — the structural edge law admits the merged delta's edges by endpoint-kind FIRST (`Assemble` routes the merged projector delta through `Graph/delta#GRAPH_DELTA` `GraphDelta.AdmitOnto` (the structural-validating sibling of the raw `ReplayOnto`), where the `LegalLink` generated total `Switch` runs per `Link` — the projection path the persistence `ReplayOnto` raw-replay does not validate), then every `IGraphConstraint.Validate` runs the IFC-semantic legality against the assembled graph, the two disjoint by design so the seam carries no IFC vocabulary and the Bim constraint carries no structural mechanics; a constraint is a strategy the app registers alongside its projectors, so a non-IFC consumer (a pure-geometry assembly) registers no constraints and the structural law alone admits.
- Receipt: the `Validation<Error,Unit>` is the legality verdict the `Assemble` fold converts to `Fin`; the structural admission produces a CANDIDATE graph and the constraints validate the merged delta against it, an illegal projection's candidate discarded (the snapshot is immutable, never mutated in place) so `Assemble` returns the assembled graph only when every constraint passes; the constraint is Bim-implemented and app-registered, the seam owning only the floor and the composition point.
- Packages: LanguageExt.Core (`Validation`/`Error`).
- Growth: a new IFC-semantic rule is one arm in a Bim `IGraphConstraint` implementation; a new constraint family is one `IGraphConstraint` the app registers; never a structural-law rule on the constraint and never an IFC rule on the seam's `LegalLink`.
- Boundary: `IGraphConstraint` is the second interface (net new Rasm interfaces = 2) — the IFC-semantic legality lives HERE (Bim-implemented), the structural edge law lives in the seam's `Graph/delta#GRAPH_DELTA` `LegalLink`, and conflating the two is the named defect; the constraint returns `Validation<Error,Unit>` (accumulating, all violations at once), not `Fin` (fail-fast), because a projection's legality report should be complete; the constraint is registered by the app alongside the projectors, so the seam composes it without referencing Bim, and a default-interface-method carrying a rule is the named defect.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// The IFC-semantic legality the consumer implements, distinct from the seam's structural law.
public interface IGraphConstraint {
    Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph);
}
```

## [04]-[RESEARCH]

- [TWO_INTERFACE_SPLIT]: the net-new Rasm interface count is 2 — `IElementProjection` (the projector floor each AEC peer implements over its captured foreign source, lowering onto a `GraphDelta`) and `IGraphConstraint` (the IFC-semantic legality Bim implements, validating a delta against the graph) — so the seam's total `Switch` enforces ONLY the structural edge law and the schema legality lives in the consumer's constraint; this is the cross-stratum alignment pattern: a lowest-stratum seam hosts the instance-interface floors the closed-vocabulary siblings depend up on and implement, aligning by contract without sibling references, each package usable in isolation (Materials projects a material subgraph, Bim the IFC projection, Persistence persists any `ElementGraph`).
- [ASSEMBLE_CAPABILITY]: the seam owns the `Assemble` fold (the composition CAPABILITY — capture each projector's delta, merge them into the one model-creating `GraphDelta` via the cancellation-correct monoid, structurally admit the merged delta through the seam's edge law, validate it against every `IGraphConstraint`, and fold it onto the seed) but NOT the wiring — registering the `Seq<IElementProjection>`, binding the imported-IFC tessellation adapter (the IfcConvert/ifcopenshell companion), wiring the tabular→element map, and the DI is an APP / HOST-BOUNDARY composition-root concern owned per-app (`Rasm.Rhino`/`Rasm.Grasshopper` today, future standalone/web/sidecar apps later), no APP-PLATFORM package hosting it; a projector builds its delta through the `Graph/delta#GRAPH_DELTA` `Put`/`Link`/`Reheader` builders, NEVER through `WorkingGraph.Apply`, so `Assemble` runs the seam's structural edge law on the merged delta through `Graph/delta#GRAPH_DELTA` `GraphDelta.AdmitOnto` (the structural-validating sibling of the persistence `ReplayOnto` raw replay, routing through `WorkingGraph.Apply`), and a thrown projector exception is captured at its boundary into `ElementFault.ProjectionFailed` through the doctrinal `Try.lift` funnel (the kernel `Op.Catch` is rejected because it re-wraps the thrown exception as the kernel `Fault.InvalidResult`, erasing the seam's typed arm); the `Assemble` result is the assembled `ElementGraph` a consumer bakes PLUS the merged `GraphDelta` the `Rasm.Persistence` `Version/ledger` appends to the Marten stream (the one model-creating event); the projector owning both endpoints authors the `Associate` material edge, so the `ProjectionContext` element-id set lets a Materials-given-a-non-empty-element-set run author element→material edges and a pure-Materials run author only the material subgraph — "the app authors it at the wire" is removed.
