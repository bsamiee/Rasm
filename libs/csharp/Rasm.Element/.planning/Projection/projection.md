# [ELEMENT_PROJECTION]

The cross-stratum alignment contract: TWO instance interfaces the AEC peers depend up on and implement, aligning by contract without referencing each other. `IElementProjection` has ONE polymorphic operation `Fin<GraphDelta> Project(ProjectionContext ctx)` — each concrete projector (the `Rasm.Bim` `SemanticProjector` over GeometryGym, the `Rasm.Materials` `MaterialProjector`, a future `Rasm.Fabrication` projector) captures its foreign source INTERNALLY and lowers it onto a `Graph/delta#GRAPH_DELTA` `GraphDelta`, so the seam never learns GeometryGym, VividOrange, or any provider surface — it folds deltas. `IGraphConstraint` has ONE operation `Validation<Error,Unit> Validate(GraphDelta, ElementGraph)` Bim implements with the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, a `Type` may not aggregate an `Occurrence`) the seam's structural `Graph/delta#GRAPH_DELTA` law does NOT carry — the two-interface split, the seam owning the structural floor and the constraint the schema legality. The seam owns the composition CAPABILITY — the `Assemble` fold that runs a `Seq<IElementProjection>`, validates each delta against a `Seq<IGraphConstraint>`, and folds the deltas onto a seed graph — but NOT the wiring: registering the projectors, binding the tessellation adapter, and running against a live source is an APP / HOST-BOUNDARY composition-root concern, owned per-app (`Rasm.Rhino`/`Rasm.Grasshopper` today, future standalone/web/sidecar apps). `ProjectionContext` carries the element `NodeId` set so an aspect projector never invents element identities, plus the neutral runtime primitives (instant, correlation, tenant) the app supplies. IFC EGRESS (`Emit`) is Bim-INTERNAL, NOT a seam member. The page composes `Graph/element#ELEMENT_GRAPH`, `Graph/delta#GRAPH_DELTA`, and the kernel `Op.Catch` exception funnel; a projector fault rails `Projection/fault#FAULT_BAND` `ElementFault.ProjectionFailed`.

## [01]-[INDEX]

- [01]-[PROJECTION_CONTRACT]: the `IElementProjection` projector floor and its one `Project`, the `IGraphConstraint` legality floor and its one `Validate`, the `ProjectionContext` element-identity-plus-primitives carrier (with the sanctioned `Rooted()` identity mint), and the `Assemble` composition capability the apps wire.
- [02]-[GRAPH_CONSTRAINT]: the `IGraphConstraint` IFC-semantic legality floor Bim implements, distinct from the seam's structural edge law, composed in `Assemble` after the structural admission.

## [02]-[PROJECTION_CONTRACT]

- Owner: `IElementProjection` the projector strategy floor with one `Project`; `ProjectionContext` the projection input (element `NodeId` set + target `Header` + runtime primitives); `ProjectionAssembly` the static `Assemble` fold the seam owns and the apps wire.
- Entry: `IElementProjection.Project(ProjectionContext ctx)` lowers a concrete projector's captured foreign source onto a `GraphDelta` over the context's element identities, `Fin<T>` carrying the projector's own faults; `ProjectionAssembly.Assemble(projectors, constraints, seed, ctx, key)` folds every projector's delta onto the seed graph, validating each delta against the constraints and capturing a thrown foreign fault into `ElementFault.ProjectionFailed` through `Op.Catch`; `ProjectionContext.For(elementIds, header, key, at, correlation, tenant)` builds the context the app fills from its kernel `Op`, clock, correlation, and tenant primitives.
- Auto: `Assemble` folds the projector sequence — each `Project` runs through the `Op.Catch` funnel so a foreign exception (a GeometryGym parse fault, a VividOrange miss) becomes `ProjectionFailed` rather than crossing the boundary, the resulting `GraphDelta` validated against every `IGraphConstraint` (the `Validation<Error,Unit>` accumulating all legality violations applicatively) and then folded onto the running graph through `GraphDelta.ReplayOnto`; the `ProjectionContext` carries the element `NodeId` set so an aspect projector (Materials projecting only the material subgraph) authors edges into the existing element identities rather than minting element ids — the projector owning BOTH endpoints authors the `Associate` material edge, so a Materials-given-a-non-empty-element-set run authors element→material edges and a pure-Materials run authors only the material subgraph.
- Receipt: the `Assemble` result is the assembled `ElementGraph` a consumer bakes — the seam owns the fold (the composition capability), the app owns the registration (`Seq<IElementProjection>` + the tessellation adapter + the tabular map + DI); adding a projector is one registration row at the app composition root, not a seam edit; IFC egress (`Emit`) is the Bim projector's INTERNAL operation reading the assembled graph, never a seam interface member.
- Packages: LanguageExt.Core (`Fin`/`Validation`/`Seq`), `Rasm` (the kernel `Op.Catch` exception funnel + `IObjectFactory` floor), NodaTime (`Instant`).
- Growth: a new aspect projector is one `IElementProjection` implementation in its owning package plus one registration row at the app root; a new runtime primitive is one column on `ProjectionContext`; net new Rasm interfaces for the whole rebuild = 2 (`IElementProjection` + `IGraphConstraint`), the `IObjectFactory` floor reused; never an instance default-interface-member and never a per-provider seam method.
- Boundary: `IElementProjection` is the ONE projector floor with ONE polymorphic `Project` — a per-provider seam method (`ProjectBim`/`ProjectMaterials`) is the deleted form, each concrete projector capturing its foreign source internally and the concrete held internal/swappable in its owning package; the projector lowers onto a `GraphDelta`, so the seam folds deltas and never learns GeometryGym/VividOrange/any provider surface; `ProjectionContext` carries the element `NodeId` set so an aspect projector authors into existing identities rather than inventing them (the both-endpoints projector authors the `Associate` edge) while a PRIMARY projector mints new rooted identities through the sanctioned `Rooted()` (the kernel `IObjectFactory` floor; the IFC GlobalId rides a Bim-stored `Object.ExternalId`), and the runtime primitives are NEUTRAL (kernel `Op`, instant, correlation, tenant) the app supplies — the seam references no AppHost `ClockPolicy`/`CorrelationId`/`TenantContext` type (those are app-platform, above the seam); the seam owns the `Assemble` CAPABILITY, the app owns the WIRING — no APP-PLATFORM package hosts the live assembly; IFC egress (`Emit`) is Bim-INTERNAL, NOT a seam member, because the seam never authors IFC; an instance default-interface-method on either floor is the named defect — defaults derive from a minimal core, never an interface body.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [MODELS] -----------------------------------------------------------------------------
// The projection input: the element identity set an aspect projector authors within (never minting
// element ids), the target model Header, the kernel Op fault-correlation key a projector rails its
// typed faults on, and the NEUTRAL runtime primitives the app supplies — the seam references no
// app-platform ClockPolicy/CorrelationId/TenantContext type, only the kernel Op and BCL primitives.
public sealed record ProjectionContext(
 FrozenSet<NodeId> ElementIds,
 Header Header,
 Op Key,
 Instant At,
 Guid CorrelationId,
 string TenantId) {
 public static ProjectionContext For(Seq<NodeId> elementIds, Header header, Op key, Instant at, Guid correlation, string tenant) =>
 new(elementIds.ToFrozenSet(), header, key, at, correlation, tenant);

 public bool Owns(NodeId element) => ElementIds.Contains(element);

 // The sanctioned mint point: a PRIMARY projector (Bim lowering an IfcRoot, an app authoring from scratch) mints a
 // rooted element identity HERE so minting is a projection capability threaded through the context, never reached for
 // ad hoc; an ASPECT projector mints nothing and authors within ElementIds (Owns gates it). The neutral Guid-v7 id is
 // the kernel IObjectFactory floor's, the IFC GlobalId a Bim-stored Object.ExternalId re-emitted at Emit (H6).
 public NodeId Rooted() => NodeId.Rooted();
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The projector strategy each AEC peer implements over its captured foreign source.
public interface IElementProjection {
 Fin<GraphDelta> Project(ProjectionContext ctx);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The seam-owned composition CAPABILITY — the apps own the registration of the projector sequence.
public static class ProjectionAssembly {
 public static Fin<ElementGraph> Assemble(
 Seq<IElementProjection> projectors, Seq<IGraphConstraint> constraints,
 ElementGraph seed, ProjectionContext ctx, Op key) =>
 projectors.Fold(
 Fin.Succ(seed),
 (acc, projector) => acc.Bind(graph =>
 key.Catch(() => projector.Project(ctx))
 .Bind(delta => Admit(constraints, delta, graph, key).Map(_ => delta.ReplayOnto(graph)))));

 // The IFC-semantic legality composed AFTER the structural admission: every constraint
 // validates the delta against the running graph, the Validation accumulating all violations.
 static Fin<Unit> Admit(Seq<IGraphConstraint> constraints, GraphDelta delta, ElementGraph graph, Op key) =>
 constraints
 .Map(c => c.Validate(delta, graph))
 .Fold(Success<Error, Unit>(unit), static (acc, v) => (acc, v).Apply(static (_, _) => unit).As())
 .ToFin();
}
```

## [03]-[GRAPH_CONSTRAINT]

- Owner: `IGraphConstraint` the IFC-semantic legality floor with one `Validate`, Bim-implemented, composed in `Assemble` after the seam's structural admission.
- Entry: `IGraphConstraint.Validate(GraphDelta delta, ElementGraph graph)` returns `Validation<Error,Unit>` accumulating every legality violation a delta introduces against the current graph — a containment edge whose relating node is not a spatial structure, a `Void` whose host is not an element or whose feature is not an opening, a `Compose` whose whole is a `Type` object — each a typed `Error` the applicative `Validation` collects so a malformed projection surfaces all violations at once, not the first.
- Auto: the seam composes the constraints in `ProjectionAssembly.Assemble` — the structural edge law (`Graph/delta#GRAPH_DELTA` `LegalLink`) admits the delta's edges by endpoint-kind FIRST (within `ReplayOnto`'s producing path), then every `IGraphConstraint.Validate` runs the IFC-semantic legality, the two disjoint by design so the seam carries no IFC vocabulary and the Bim constraint carries no structural mechanics; a constraint is a strategy the app registers alongside its projectors, so a non-IFC consumer (a pure-geometry assembly) registers no constraints and the structural law alone admits.
- Receipt: the `Validation<Error,Unit>` is the legality verdict the `Assemble` fold converts to `Fin` before applying the delta, so an illegal projection never mutates the graph; the constraint is Bim-implemented and app-registered, the seam owning only the floor and the composition point.
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
- [ASSEMBLE_CAPABILITY]: the seam owns the `Assemble` fold (the composition CAPABILITY — run the projector sequence, validate each delta, fold onto the seed) but NOT the wiring — registering the `Seq<IElementProjection>`, binding the imported-IFC tessellation adapter (the IfcConvert/ifcopenshell companion), wiring the tabular→element map, and the DI is an APP / HOST-BOUNDARY composition-root concern owned per-app (`Rasm.Rhino`/`Rasm.Grasshopper` today, future standalone/web/sidecar apps later), no APP-PLATFORM package hosting it; the projector owning both endpoints authors the `Associate` material edge, so the `ProjectionContext` element-id set lets a Materials-given-a-non-empty-element-set run author element→material edges and a pure-Materials run author only the material subgraph — "the app authors it at the wire" is removed.
