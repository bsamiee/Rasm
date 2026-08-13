# [ELEMENT_PROJECTION]

`Rasm.Element` projection is the cross-stratum alignment seam: TWO instance-interface floors the AEC peers depend up on and implement — aligning by contract without referencing each other — under the `Assemble` composition CAPABILITY the apps wire over them. `IElementProjection` lowers a projector's INTERNALLY-captured foreign source onto a `Graph/delta#GRAPH_DELTA` `GraphDelta`, so the seam folds deltas and never learns a provider surface; `IGraphConstraint` carries the IFC-semantic legality Bim implements, disjoint from the seam's own structural edge law [M3].

Model checking is GRADED, never binary: a constraint registers under its `ConstraintSeverity`, and a reviewed deviation is a `ConstraintWaiver` the context carries pinned to the violation's content key, so an accepted issue is typed run policy rather than a hand-stripped rerun. `Assemble` returns the `AssemblyReceipt` — the assembled `ElementGraph` a consumer bakes, the merged `GraphDelta` event body Persistence appends [DELTA_EVENT_RULING], and the graded `ConstraintFinding` set a review workflow and a compliance dashboard read.

Wiring — projector registration, tessellation adapter, `Graph/element#NODE_MODEL` `GeometrySource` port, DI — stays an APP composition-root concern, and IFC egress (`Emit`) is Bim-INTERNAL, never a seam member.

## [01]-[INDEX]

- [02]-[PROJECTION_CONTRACT]: `IElementProjection` the projector floor and its one `Project`, `ProjectionContext` the element-identity/header/primitives carrier with its `Owns` vouch predicate and `ConstraintWaiver` reviewed-deviation set, `TypeCandidate` the reverse-type row the Bim export and the Materials admission both compose, `TextureRoster`/`TextureCandidate` the texture hand-off rows, `ProjectionSuite` the graded-registration mint, and `ProjectionAssembly` the composition capability the apps wire, returning the `AssemblyReceipt`.
- [03]-[GRAPH_CONSTRAINT]: `IGraphConstraint` the IFC-semantic legality floor Bim implements, composed in `Assemble` after the structural admission, accumulating every violation applicatively — each violation graded by its `ConstraintRegistration` row's `ConstraintSeverity`, waived by content key, and the non-blocking findings landed typed on the receipt.
- [04]-[INTERCHANGE_CARRIER]: `ImportedGeometry` the one decoded interchange mesh-pool carrier the Bim import rail produces and the Compute tile/residency lane reads — the kernel `EncodedGeometry` lane arena beside the `Indices` column — with its `MeshBlock`/`MeshInstance` instancing-and-shading-partition overlay and the seam-owned lane-agnostic `Bake` flatten.

## [02]-[PROJECTION_CONTRACT]

- Boundary: `TypeCandidate` is DECLARED HERE, once, because both its producer (`Rasm.Bim` `Exchange/import#REIMPORT` `BimIo.ExportTypeCandidates`) and its consumer (`Rasm.Materials` `Component/component#CATALOGUE` `ComponentCatalogue.AdmitImported`) reference this seam and never each other — the twin local spellings the contract-alignment idiom produced were two declarations of one row that an edit at either end forks silently, which is exactly the drift a shared owner forecloses; the alignment idiom stays correct for the `IIfcTypeReconciler` PORT (a behavioural seam neither end can host), and wrong for a pure data row both ends can reach. `TextureRoster`/`TextureCandidate` seat here under the SAME law — producer `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` `AppearanceProjection.RosterOf`, consumer `Rasm.Materials` `Raster/set#SET_INGEST` `SetIngest.Roster` — and the row carries only neutral columns: the IFC transform decode lowers at the producer's mint, so no host or IFC type reaches this seam.
- Owner: `IElementProjection` the projector strategy floor with one `Project`; `ProjectionContext` the projection input (element `NodeId` set + target `Header` + the kernel `CorrelationId`/`TenantContext` causal pair, carrying the `Owns` vouch predicate and the `ConstraintWaiver` reviewed-deviation set); `ConstraintSeverity` the `[SmartEnum<string>]` verdict grade whose `Blocks` column is the discard policy; `ConstraintRegistration` the graded constraint row the suite registers; `ConstraintFinding` the typed per-violation QA finding (severity + violation + content key + waived flag); `AssemblyReceipt` the assembly result carrier; `ProjectionSuite` the minting seam — the one typed registration value the app root builds from each owning package's factory product; `ProjectionAssembly` the static composition capability the seam owns and the apps wire.
- Entry: `IElementProjection.Project(ProjectionContext ctx)` lowers a concrete projector's captured foreign source onto a `GraphDelta` over the context's element identities, `Fin<T>` carrying the projector's own faults; `ProjectionSuite.Of(projectors, constraints)` mints the registration value — each `IElementProjection` arrives as an owning package's OWN factory product (`ComponentProjector.Of(source)` the Materials mint, the Bim `SemanticProjector` mint likewise package-owned) and each constraint as a `ConstraintRegistration.Of(constraint, severity)` graded row defaulting `Blocking`, the concrete internal and swappable behind its floor; `ProjectionAssembly.Assemble(suite, seed, ctx)` runs the capture→merge→establish→admit→constrain→fold pipeline over the suite returning the `AssemblyReceipt` (`Graph` + `Delta` + `Findings`); the railed `ProjectionContext.For(elementIds, header, key, at, correlation, tenant, waivers)` narrows its guard to correlation alone — tenancy arrives as the kernel `TenantContext`, whose absent case IS the root row, so the mint admits the supplied value unchanged and polices no blank string — `elementIds` carrying the owner-minted Type and Occurrence identities an aspect projector vouches against, the trailing `waivers` the reviewed-deviation set a model review authored.
- Auto: `Assemble` (1) captures each `Project` through `Try.lift` — a thrown foreign exception becomes `ProjectorFaulted`, a projector's own returned typed fault (a `BimFault`, a `ProjectionFault`) is preserved unchanged — and accumulates every projector fault applicatively (`.Traverse(...).As().ToValidation()`, `Error.Combine` unioning the faults so a run where BOTH Bim and Materials fail reports both); (2) on full success seeds the monoid fold with `GraphDelta.Empty.Reheader(ctx.Header)` and merges via `Merge`'s `next.Header`-wins rule; (3) structurally admits the merged delta through `GraphDelta.AdmitOnto` — the validating sibling of the raw persistence `ReplayOnto`, routing through `WorkingGraph.Apply` so `LegalLink` runs per `Link`; (4) validates the ADMITTED `applied.Delta` — the same body the receipt carries and Persistence appends, never the pre-admission merge — against every registered `IGraphConstraint`, grading each accumulated violation by its registration row's severity, marking waived findings by content key, and discarding the candidate ONLY on an unwaived blocking finding; (5) folds onto the seed graph, the surviving findings riding the receipt.
- Receipt: the `Assemble` result is the `AssemblyReceipt` — the assembled `ElementGraph` a consumer bakes, the merged `GraphDelta` event body the `Rasm.Persistence` `Version/ledger` appends to the Marten stream (the one model-creating event, never a whole-graph snapshot) [DELTA_EVENT_RULING], and the graded `ConstraintFinding` set (warnings and waived deviations) the QA report persists beside the model instead of vanishing at the boundary.
- Packages: LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Try`/`ManyErrors` + the `TraverseM`/`Traverse` accumulation split + the `Fold` monoid + `ToValidation`/`ToFin` cross-rail bridges), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` the `ConstraintSeverity` grade), `Rasm` (the kernel `Op` op-key + the `FaultExtensions.Category` projection the finding key folds), NodaTime (`Instant`), System.Collections.Frozen (`FrozenSet`), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter`/`ContentAddress` the `ConstraintFinding.KeyOf` identity mints through).
- Growth: a new aspect projector is one `IElementProjection` implementation in its owning package with one registration row at the app root; a new causal or runtime ingredient is one column on `ProjectionContext`; the unified Material/Component/Element paradigm adds NO new interface and NO new mint method, both mints being owner-side compositions of the kernel `Graph/element#NODE_MODEL` `NodeId` floor.
- Boundary: `ConstraintFinding.Key` and `ConstraintWaiver.Finding` are `Projection/address#CONTENT_ADDRESS` `ContentAddress`, never a bare `UInt128` — the waiver crosses the wire to a review workflow and a raw 128-bit JSON number loses precision past 2^53 in a JS parse, so the pinned key rides the address's own X32 hex face like every other content key on the seam; `IElementProjection` is the ONE projector floor with ONE polymorphic `Project` — a per-provider seam method (`ProjectBim`/`ProjectMaterials`) is the deleted form, the concrete held internal and swappable in its owning package behind that package's minting factory, and `ProjectionSuite.Of` the one registration mint `Assemble` dispatches over (loose per-call projector/constraint collections are the deleted form — selection resolves through the declared minting seam, never provider-specific constructor exposure at a consumer); the projector-capture fold is APPLICATIVE and the admit-then-constrain tail MONADIC — the carrier, never a flag, selects the algebra ([APPLICATIVE_CAPTURE]); `Owns` is a PREDICATE, not an authoring helper — the skip-vs-rail policy is the projector's [H12] ([OWNER_MINTS_IDENTITY]); `ctx.Header` is the floor-seeded model header a projector-authored `Reheader` overrides ([HEADER_ESTABLISHMENT]); the runtime primitives are NEUTRAL (kernel `Op`, instant, correlation, tenant) — the seam references no AppHost `ClockPolicy`/`CorrelationId`/`TenantContext` type; the capture funnel is `Try.lift`, never the kernel `Op.Catch`; the projection path admits through `AdmitOnto`, never the raw `ReplayOnto`; the seam owns the `Assemble` CAPABILITY and the app the WIRING — no APP-PLATFORM package hosts the live assembly ([ASSEMBLE_CAPABILITY]); severity is REGISTRATION policy — the `ConstraintSeverity` column rides the `ConstraintRegistration` row the app root grades, never a floor member a foreign implementor must carry, so a rule family needing two grades registers as two `IGraphConstraint` rows ([GRADED_VERDICT]); IFC egress (`Emit`) is Bim-INTERNAL because the seam never authors IFC; an instance default-interface-method on either floor is the named defect — defaults derive from a minimal core, never an interface body.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Numerics;                               // Matrix4x4, Vector3 — the interchange-carrier placement currency
using System.Runtime.InteropServices;                // ImmutableCollectionsMarshal — the zero-copy index handover
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// ConstraintSeverity is the verdict grade a constraint registers under — its Blocks column IS the discard policy
// Assemble reads: an unwaived violation from a Blocking row discards the candidate, a Warning row's violation lands
// as a receipt finding (assemble-with-warnings), so the grade is row data, never a mode flag or a message probe.
[SmartEnum<string>]
public sealed partial class ConstraintSeverity {
    public static readonly ConstraintSeverity Blocking = new("blocking", blocks: true);
    public static readonly ConstraintSeverity Warning = new("warning", blocks: false);

    public bool Blocks { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
// ConstraintWaiver records the reviewed deviation a model review authors: the pinned finding content key, the
// accepting authority, and the acceptance instant — run policy the app root supplies on the context, so an accepted
// issue is typed and auditable rather than a hand-stripped rerun or a silenced rule.
public readonly record struct ConstraintWaiver(ContentAddress Finding, string Authority, Instant At);

// TypeCandidate is the ingested-type row the reverse type loop carries: Rasm.Bim's BimIo.ExportTypeCandidates mints
// one per IFC type object its reconciler left unresolved, and Rasm.Materials' ComponentCatalogue.AdmitImported lowers
// it onto railed Component.Of construction. The two packages never reference each other, so the row homes HERE at
// this shared seam rather than once at each end, where identical fields stand as two declarations one edit forks
// silently. Absent signature axes are ABSENT KEYS in the source bag, so Option IS
// what a read yields and no presence sentinel travels; Properties carries the type's own IFC property-set rows.
public readonly record struct TypeCandidate(
    string SourceLibrary,
    string GlobalId,
    string IfcEntity,
    string PredefinedToken,
    string Name,
    Map<PropertyName, PropertyValue> Properties,
    Option<string> MaterialName,
    Option<string> ProfileDesignation,
    Option<string> ProfileStandard);

// TextureRoster and TextureCandidate are the hand-off rows the styling producer and the texture-set classifier both
// compose — the same shared-owner law as TypeCandidate: Rasm.Bim's AppearanceProjection.RosterOf mints one
// TextureRoster per styled appearance and Rasm.Materials' SetIngest.Roster classifies it, the two packages never
// referencing each other. Channel is the canonical channel token the producer's mode roster resolved; Inverted the
// gloss/transparency polarity that mode declares and the token does not; RepeatU/RepeatV and CoordinateSet are
// binding facts riding to the consumer's sampler policy; the five frame columns are the NEUTRAL uv frame the
// producer's own transform lowered AT MINT, so no IFC operator decode crosses this row and the consumer lifts them
// straight to its binding policy. Reference carries the texture location as declared — an app root resolves it to
// bytes, this row never does.
public readonly record struct TextureCandidate(
    string Channel,
    string Reference,
    bool Inverted,
    bool RepeatU,
    bool RepeatV,
    int CoordinateSet,
    double OffsetU,
    double OffsetV,
    double ScaleU,
    double ScaleV,
    double Rotation);

public readonly record struct TextureRoster(NodeId Appearance, Seq<TextureCandidate> Textures);

// ProjectionContext carries the projection input: the element identity set an aspect projector authors WITHIN (Owns
// gates it), the target Header the fold seeds (LOAD-BEARING, never a dead field), the kernel Op key, the kernel
// causal pair — S0 vocabulary this seam composes directly, so no app-platform ClockPolicy or Principal type crosses
// — and the waiver set the constrain step reads. Tenancy is `TenantContext`, so absence is the kernel root row
// rather than a blank string the mint must police, and the admission guard narrows to correlation alone.
public sealed record ProjectionContext {
    public FrozenSet<NodeId> ElementIds { get; }
    public Header Header { get; }
    public Op Key { get; }
    public Instant At { get; }
    public CorrelationId Correlation { get; }
    public TenantContext Tenant { get; }
    public Seq<ConstraintWaiver> Waivers { get; }

    private ProjectionContext(FrozenSet<NodeId> elementIds, Header header, Op key, Instant at, CorrelationId correlation, TenantContext tenant, Seq<ConstraintWaiver> waivers) =>
        (ElementIds, Header, Key, At, Correlation, Tenant, Waivers) = (elementIds, header, key, at, correlation, tenant, waivers);

    public static Fin<ProjectionContext> For(Seq<NodeId> elementIds, Header header, Op key, Instant at, CorrelationId correlation, TenantContext tenant, Seq<ConstraintWaiver> waivers = default) =>
        correlation == CorrelationId.None
            ? ElementFault.ValueRejected(key, $"<projection-context-correlation-unset:{correlation}>")
            : Fin.Succ(new ProjectionContext(elementIds.ToFrozenSet(), header, key, at, correlation, tenant, waivers));

    // Owns is the aspect-projector vouch — a PREDICATE, never an authoring helper; skip-vs-rail stays the
    // projector's policy [H12]. Vouching is SEMANTIC, complementary to the structural law AdmitOnto runs.
    public bool Owns(NodeId element) => ElementIds.Contains(element);
}

// One graded QA finding: the registration's severity, the typed violation Error (the BimFault/ElementFault arm
// preserved so error.IsType/Filter recovery survives), the violation's content key, and the waived flag. KeyOf
// mints the identity a review workflow pins a waiver to — band code + Category + the producer-owned Detail
// discriminant through the seam CanonicalWriter and the kernel seed-zero hash — replayable across runs, never a
// positional issue ordinal a re-run renumbers.
public sealed record ConstraintFinding(ConstraintSeverity Severity, Error Violation, ContentAddress Key, bool Waived) {
    // UnquantizedGrid marks the finding preimage as Ordinal + String alone — never Measure — so the writer's
    // quantization grid is never read; the named constant states that irrelevance where a bare 0.0 argument reads as
    // a tolerance decision.
    private const double UnquantizedGrid = 0.0;

    public static ConstraintFinding Of(ConstraintSeverity severity, Error violation, Seq<ConstraintWaiver> waivers) {
        ContentAddress key = KeyOf(violation);
        return new(severity, violation, key, waivers.Exists(waiver => waiver.Finding == key));
    }

    // Message IS the fault band's frozen `<kind:colon-args>` Detail token (Projection/fault#FAULT_BAND
    // [DETAIL_GRAMMAR] — ElementFault.Message => Detail, and every peer band derives Expected the same way), so this
    // preimage hashes three FROZEN axes and a stored waiver survives every run. That dependency is load-bearing in
    // one direction: the grammar is append-only, and a re-worded token is a deliberate re-key of its own waivers.
    // ProjectorFaulted is the one arm whose Detail is raw provider text, so a finding never carries it — the
    // capture funnel rails before Constrain runs.
    public static ContentAddress KeyOf(Error violation) {
        CanonicalWriter w = new(UnquantizedGrid);
        w.Ordinal(violation.Code).String(violation.Category).String(violation.Message);
        return ContentAddress.Of(w.ToBytes().Span);
    }
}

// AssemblyReceipt carries the assembled graph a consumer bakes, the merged event body Persistence appends, and the
// graded findings (warnings and waived deviations) that survive a successful assembly — the model-quality evidence
// stream a review workflow and a compliance dashboard read off the same value.
public sealed record AssemblyReceipt(ElementGraph Graph, GraphDelta Delta, Seq<ConstraintFinding> Findings);

// --- [SERVICES] ---------------------------------------------------------------------------
// IElementProjection is the projector strategy each AEC peer implements over its captured foreign source — one
// polymorphic Project lowering onto a GraphDelta, the concrete (Bim SemanticProjector over GeometryGym, Materials
// ComponentProjector over VividOrange) held internal and swappable in its owning package, reached ONLY through that
// package's own minting factory returning THIS floor (the ComponentProjector.Of shape), so the seam folds deltas and
// never learns a provider.
public interface IElementProjection {
    Fin<GraphDelta> Project(ProjectionContext ctx);
}

// ConstraintRegistration is the graded constraint row the suite registers: the floor instance with its verdict grade
// — severity is REGISTRATION policy (the app root grades each rule family it registers), never a floor member a
// foreign implementor must carry; a rule family needing two grades registers as two IGraphConstraint rows.
public sealed record ConstraintRegistration(IGraphConstraint Constraint, ConstraintSeverity Severity) {
    public static ConstraintRegistration Of(IGraphConstraint constraint, Option<ConstraintSeverity> severity = default) =>
        new(constraint, severity.IfNone(ConstraintSeverity.Blocking));
}

// ProjectionSuite is the MINTING SEAM the open floors pair with (OPEN_FLOOR_DISPATCH): the app composition root
// folds each owning package's factory product into ONE typed registration value, and Assemble dispatches over the
// suite — a consumer names the floors and the mint, never a concrete, and never threads loose per-call collections.
// PRIVATE ctor:
// Of is the one mint, so registration is a value the root builds once and every assembly run reuses.
public sealed record ProjectionSuite {
    public Seq<IElementProjection> Projectors { get; }
    public Seq<ConstraintRegistration> Constraints { get; }

    private ProjectionSuite(Seq<IElementProjection> projectors, Seq<ConstraintRegistration> constraints) =>
        (Projectors, Constraints) = (projectors, constraints);

    public static ProjectionSuite Of(Seq<IElementProjection> projectors, Seq<ConstraintRegistration> constraints = default) =>
        new(projectors, constraints);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// ProjectionAssembly is the seam-owned composition CAPABILITY (the apps own the wiring): capture applicatively,
// floor-seed the fold with ctx.Header (next.Header-wins), admit structurally, constrain graded, fold — the
// AssemblyReceipt carrying the graph, the event-body delta, and the surviving QA findings.
public static class ProjectionAssembly {
    public static Fin<AssemblyReceipt> Assemble(ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
        suite.Projectors
            .Traverse(projector => Capture(projector, ctx).ToValidation()).As()
            .Map(deltas => deltas.Fold(GraphDelta.Empty.Reheader(ctx.Header), static (acc, delta) => acc.Merge(delta)))
            .ToFin()
            // Constrain rules on applied.Delta, NEVER the pre-admission merged delta: AdmitOnto RE-DERIVES the
            // event body through WorkingGraph.Apply (coalescing and cancellation land it in normal form), so the
            // receipt and the verdict read ONE delta or a waiver pins a finding the receipt cannot reproduce.
            .Bind(merged => merged.AdmitOnto(seed, ctx.Key)
                .Bind(applied => Constrain(suite.Constraints, applied.Delta, applied.Graph, ctx.Waivers)
                    .Map(findings => new AssemblyReceipt(applied.Graph, applied.Delta, findings))));

    // Try.lift turns a THROWN foreign call into ProjectorFaulted with the raw provider message — a separate arm from
    // its own ProjectionFailed structural verdict precisely because that message is OUTSIDE the frozen Detail
    // grammar a finding key hashes (Projection/fault#FAULT_BAND). MapFail's lambda closes over ctx.Key, never
    // static; a projector's OWN typed fault passes untouched — Run yields Succ(Fail(x)), MapFail no-ops on the outer
    // Succ, Bind surfaces x.
    private static Fin<GraphDelta> Capture(IElementProjection projector, ProjectionContext ctx) =>
        Try.lift<Fin<GraphDelta>>(() => projector.Project(ctx)).Run()
            .MapFail(error => ElementFault.ProjectorFaulted(ctx.Key, error.Message))
            .Bind(static fin => fin);

    // Composed AFTER the structural admission: every registration's accumulated violations become findings under
    // its row's severity, a finding whose content key a context waiver pins is Waived, and ONLY an unwaived
    // Blocks-true finding discards the candidate — the failure re-carries those violations as ManyErrors (the typed
    // arms survive Error.Combine, so error.Filter<BimFault.ModelRejected>() still reads the full report), while
    // warnings and waived deviations ride the receipt as typed findings instead of vanishing at the boundary.
    // Constrain admits an empty registration set vacuously with an empty finding set.
    private static Fin<Seq<ConstraintFinding>> Constrain(
        Seq<ConstraintRegistration> constraints, GraphDelta delta, ElementGraph graph, Seq<ConstraintWaiver> waivers) {
        Seq<ConstraintFinding> findings = constraints.Bind(row =>
            row.Constraint.Validate(delta, graph).Match(
                Succ: static _ => Seq<ConstraintFinding>(),
                Fail: failure => Violations(failure).Map(violation => ConstraintFinding.Of(row.Severity, violation, waivers))));
        Seq<Error> blocking = findings.Filter(static f => f.Severity.Blocks && !f.Waived).Map(static f => f.Violation);
        return blocking.IsEmpty ? Fin.Succ(findings) : Fin.Fail<Seq<ConstraintFinding>>(new ManyErrors(blocking));
    }

    // Violations splits a constraint's accumulated Validation failure — ONE Error that may be ManyErrors — into one
    // finding per inner violation, so severity grades and waivers pin at the violation grain a review workflow needs.
    private static Seq<Error> Violations(Error failure) => failure is ManyErrors many ? many.Errors : Seq(failure);
}
```

## [03]-[GRAPH_CONSTRAINT]

- Owner: `IGraphConstraint` the IFC-semantic legality floor with one `Validate`, Bim-implemented, composed in `Assemble` after the seam's structural admission.
- Entry: `IGraphConstraint.Validate(GraphDelta delta, ElementGraph graph)` returns `Validation<Error,Unit>` accumulating every legality violation a delta introduces against the assembled graph — a containment edge whose relating node is not a spatial structure, a `Void` whose host is not an element or whose feature is not an opening, a `Compose` whose whole is a `Type` object — each a typed `Error` the applicative `Validation` collects so a malformed projection surfaces all violations at once.
- Auto: the structural edge law admits the merged delta's edges by endpoint-kind FIRST, then every registered `Validate` runs the IFC-semantic legality against the assembled graph — each registration's accumulated violations become `ConstraintFinding` rows under its `ConstraintSeverity`, a finding whose content key a context `ConstraintWaiver` pins marks `Waived`, and the candidate is discarded ONLY on an unwaived blocking finding (the failure re-carrying those violations as `ManyErrors`); the two layers are disjoint by design, the seam carrying no IFC vocabulary and the Bim constraint no structural mechanics; a constraint is a strategy the app registers alongside its projectors, so a non-IFC consumer registers none and the structural law alone admits.
- Receipt: the `Validation<Error,Unit>` is the legality verdict the graded constrain fold reads — a blocking abort carries the accumulated `ManyErrors` intact, so a recovery reads `error.Filter<BimFault.ModelRejected>()` against the full report (the `Rasm.Bim/Projection/semantic#GRAPH_LEGALITY` `IfcLegality` case is the unsuffixed nested record, so the `Filter<E> where E : Error` probe names it directly); the structural admission produces a CANDIDATE graph the constraints validate against, an unwaived-blocking projection's candidate discarded — a warning-graded or waived violation lands on `AssemblyReceipt.Findings` instead, so the QA report persists beside the model.
- Packages: LanguageExt.Core (`Validation`/`Error`/`ManyErrors` + the applicative `Traverse`/`.As()`/`ToFin`).
- Growth: a new IFC-semantic rule is one arm in a Bim `IGraphConstraint` implementation; a new constraint family is one `IGraphConstraint` the app registers under its `ConstraintRegistration` grade — a family whose rules split by grade registers as two rows; a new verdict grade is one `ConstraintSeverity` row carrying its `Blocks` column; never a structural-law rule on the constraint and never an IFC rule on the seam's `LegalLink`.
- Boundary: `IGraphConstraint` is the second seam interface [M3] — the IFC-semantic legality lives HERE and the structural edge law in `Graph/delta#GRAPH_DELTA` `LegalLink`, conflating the two the named defect; the return is `Validation<Error,Unit>` (accumulating), not `Fin` (fail-fast), because a projection's legality report is complete — the doctrinal accumulating-constraint floor foreign code supplies and the seam folds applicatively at the edge, never a case the owner closes; severity and waiver stay SEAM-fold policy over the verdict — the grade a `ConstraintRegistration` column, the waiver a `ProjectionContext` value pinned to the violation's `ConstraintFinding.KeyOf` content key — so the floor a foreign assembly implements never widens for grading; the app registers it alongside the projectors, so the seam composes it without referencing Bim; a default-interface-method carrying a rule is the named defect.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// IGraphConstraint carries the IFC-semantic legality the consumer implements, distinct from the seam's structural
// law — the open accumulating constraint floor (Validation<Error,Unit>, all violations at once), Bim-implemented and
// app-registered, folded applicatively in Assemble after the structural admission. A Fin (fail-fast) return is the
// deleted form.
public interface IGraphConstraint {
    Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph);
}
```

## [04]-[INTERCHANGE_CARRIER]

- Owner: `ImportedGeometry` the decoded interchange mesh-POOL carrier — one kernel `Rasm.Drawing` `EncodedGeometry` arena holding every per-vertex lane (position, normal, UV, colour, and whatever the roster grows next) as descriptor-addressed slices of ONE payload, `Indices` the single non-channel column, each decoded source mesh occupying a `MeshBlock` range, `MeshInstance` rows placing blocks by rigid transform, `Bake()` flattening on demand; `MeshBlock` the pool range with its `Declared` channel-evidence set and its `Material` shading-partition key; `MeshInstance` the rigid placement.
- Entry: `Rasm.Bim` `Exchange/import#IMPORT_RAIL` decode arms construct it (one `Encode.Of` mint per decode over the lanes the source declares, one identity instance per block on a non-instanced source, one block per shading partition where the source splits a mesh by material) and `Rasm.Compute` `Runtime/codecs#TILE_PARTITION` slices baked leaves from it; `Bake()` is the ONE flatten — positions `Transform`, normals `TransformNormal`, every other lane copied rigid-invariant, each placed block inheriting its source `Material` — so a consumer needing world-space geometry calls the one owner and a consumer preserving instancing or shading partitions reads the overlay.
- Law: this seat is the `STRATA_TWIN` resolution — the carrier crosses `Rasm.Bim` (S2 producer) and `Rasm.Compute` (S3 consumer), two packages that never reference each other, so the shape homes at the lowest stratum both reach; the prior same-named twins (a Bim pool form without UVs, a Compute soup form without `Blocks`/`Instances`) merged onto this superset and both packages compose it. `EncodingChannel` supplies the lane set and the kernel `EncodedGeometry` the arena, so the interchange carrier and the kernel's own packed geometry speak ONE channel vocabulary, ONE dtype roster, and ONE payload layout — the per-channel `ReadOnlyMemory<float>` columns this record once held were a second encoding arena one stratum up, and the descriptor set is what makes a new lane cost zero columns here.
- Receipt: the arena's own `RoundTripWitness` — every mint measures per-lane quantization error against its dtype tolerance and carries the payload-rooted `GeometryHash` (`RoundTripWitness.Root` is `DigestRoot.Payload` on every arena this carrier holds, because a foreign decode carries no kernel `EncodeForm` source; a consumer keying dedup against source-rooted `Encode.Apply` digests must read `Root` before comparing), so a decode that silently lost precision fails `IsValid` rather than reaching a consumer; decode evidence still rides the Bim `ModelLoad` receipt and partition evidence the Compute `StreamSegment` receipt.
- Packages: LanguageExt.Core (`Seq`), NodaTime (`Instant`), Generator.Equals (`[Equatable]`/`[OrderedEquality]`/`[StringEquality]` — the carrier's structural equality, without which a content-keyed consumer compares two identical decodes unequal), `Rasm` (the kernel `Drawing.EncodedGeometry` arena with `Encode.Of`/`Channel`/`View<T>`, the `EncodingChannel` lane roster, and `ChannelDtype.Unpack`), BCL inbox (`System.Numerics` `Matrix4x4`/`Vector3` the rigid-placement currency, `ImmutableArray<long>` the index column, `ImmutableCollectionsMarshal.AsImmutableArray` the zero-copy handover).
- Growth: a new vertex attribute is one kernel `EncodingChannel` row — the arena addresses it by descriptor, `Bake` copies it by arity with no edit, and the tile partition and meshlet arm read it through `View<T>`, so the seam record gains NO column and no producing arm re-threads a buffer; a new placement semantic is one `MeshInstance` column and a new per-range fact one `MeshBlock` column; a source that begins splitting by material fills `Material` in its own decode arm with no seam edit; a per-format decode arm stays `Rasm.Bim`'s and a partition/encoding arm `Rasm.Compute`'s — the seam owns the SHAPE alone.
- Law: TWO vector types coexist by DELIBERATE CARVE, named here so neither drifts into the other's territory — the seam-owned `Graph/element#NODE_MODEL` `Vector3` is the ANALYTICAL/domain coordinate (the `AxisCurve`/`FootprintPolygon` shapes, the orientation classifier, the load-vector folds; it owns the domain algebra), while the BCL `System.Numerics` `Matrix4x4`/`Vector3` are INTERCHANGE-CARRIER LAYOUT types confined to `MeshInstance` and the `Bake` flatten, where the placement currency every decoder already speaks and the intrinsics the flatten runs on are the whole reason they are here. This carve forecloses a BCL vector crossing into analytical math and a seam `Vector3` appearing in an instance placement, standing as the declared exemption to the branch's one-coordinate law.
- Boundary: an absent lane is a MISSING DESCRIPTOR at both altitudes — the arena declares no descriptor for a channel the source never carried, and `MeshBlock.Declared` records the per-block set so the pools write only declared ranges; a zero-filled range standing in for an undeclared lane is the deleted form, because a consumer cannot tell fabricated zeros from measured ones.
- Boundary: `FormatKey` carries the `Rasm.Bim` `Exchange/format#FORMAT_AXIS` row KEY — the format VOCABULARY is S2 host-local row data the strata forbid below its stratum, so the seam records the canonical key string and the Bim end alone re-hydrates the row (`InterchangeFormat.Get`); the UV lane is `EncodingChannel.Uv` (TEXCOORD_0, arity 2, `Float32` by the kernel's own law so an unbounded surface parameter never clamps) and the colour lane `EncodingChannel.ColorRgba` (arity 4, `Unorm8`), each ABSENT from the descriptor set when the source declares none — an absent lane is a missing descriptor, never an empty buffer a consumer must length-probe — filled at the ONE decode, sliced by the tile partition with the same triangle gather, and encoded by the residency meshlet arm as its own stream, so a streamed cluster resolves a REAL unwrap and a REAL vertex colour with no second decode of the same bytes; `MeshBlock.Material` follows the `FormatKey` law — the source's OWN material address as an open string the producing `Rasm.Bim` end alone re-hydrates against the appearance projection, never a graph `NodeId` or an `AppearanceSummary` a decode does not hold and cannot seat below its stratum — and it keys a BLOCK rather than the carrier, because a per-material split IS a block partition; an absent key is a source that declared no partition, so an unsplit block never carries a fabricated material and a consumer grouping by key reads the split the source authored; geometry lanes here are the kernel's host-neutral packed arena, never a host geometry type, and the GRAPH keeps geometry by content hash — this carrier is the interchange DECODE product beside the graph, not a node payload.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// Mesh-POOL carrier: the lane arena and Indices hold each decoded source mesh ONCE as a Blocks range, and
// Instances places blocks by rigid transform, so an instanced source (glTF node reuse + EXT_mesh_gpu_instancing,
// dotbim Mesh pool, Assimp node tree, USD prim placement) round-trips its sharing instead of N baked copies.
// Non-instanced decode is one identity instance per block, so its pool IS its world-space scene and Bake() returns
// it unchanged; a consumer needing flat world-space geometry from an instanced carrier calls Bake() once.
// Material is the block's own SHADING PARTITION key — the source's own address for the material this face range
// binds (a USD UsdGeomSubset's bound material path, a glTF primitive's material name, an Assimp material name) —
// so a source that splits ONE mesh per material lands one block per split and a consumer regroups by shared key
// with no re-decode. It is the FormatKey precedent: an open source-declared string the producing end alone
// re-hydrates, never a graph NodeId a decode does not hold. Absence means the source declared no partition, so a
// per-material split is expressible without every unsplit block carrying a fabricated key.
// Declared is the block's OWN channel evidence: the pools write only the ranges a source actually declared, so an
// absent lane on a block is a MISSING DESCRIPTOR, never a zero-filled range a consumer reads as real data. A source
// that carries UVs on its detail geometry and none on its shell lands two blocks whose Declared sets differ, and a
// tile partition or meshlet encoder reads the block's own set rather than assuming the arena's union covers it.
public readonly record struct MeshBlock(
    int VertexOffset, int VertexCount, int IndexOffset, int IndexCount,
    Seq<EncodingChannel> Declared = default, Option<string> Material = default);

public readonly record struct MeshInstance(int Block, Matrix4x4 Transform);

// Lanes is the kernel EncodedGeometry arena: one payload, one descriptor per declared EncodingChannel. Position and
// Normal are the two lanes every source carries; Uv (TEXCOORD_0, arity 2, Float32) and ColorRgba (arity 4, Unorm8)
// are present exactly when the source declares them, so absence is a MISSING DESCRIPTOR rather than a zero-length
// buffer a consumer must length-probe. The Bim decode arms fill the lanes they read, the Compute tile partition
// slices them with the same triangle gather, and the residency meshlet arm encodes each as its own stream — so a hit
// on a streamed cluster resolves a REAL unwrap and a REAL vertex colour instead of a bounding-proxy stand-in. Indices
// stays a column because topology is not a per-vertex lane and no descriptor can address it.
// [Equatable] is LOAD-BEARING here, not ceremony: the synthesized record equality this carrier once inherited
// compared Indices by REFERENCE-AND-RANGE (ReadOnlyMemory's own struct equality) and the Seq columns by their
// carrier identity, so two byte-identical decodes of one source compared UNEQUAL — every content-keyed memo, tile
// residency probe, and re-decode guard keyed on this carrier missed. Indices is ImmutableArray<long> for the same
// reason MaterialComposition's impact matrix is: the immutable owner forbids the post-admission aliasing a memory
// over a caller-held array admits AND is IEnumerable<long>, which is what [OrderedEquality] requires — a
// ReadOnlyMemory member cannot carry the attribute at all, so the sequence semantics were unreachable on it.
// Lanes needs no policy attribute: the kernel owner Rasm/Drawing/pack#ENCODING makes EncodedGeometry [Equatable]
// with Payload excluded and keyed by Witness.ContentHash — DigestRoot.Payload on every arena here, since the
// decode and the Bake re-mint both go through Encode.Of — so the default member comparison here is structural.
[Equatable]
public sealed partial record ImportedGeometry(
    [property: StringEquality(StringComparison.Ordinal)] string FormatKey,
    EncodedGeometry Lanes,
    [property: OrderedEquality] ImmutableArray<long> Indices,
    int VertexCount,
    int TriangleCount,
    [property: OrderedEquality] Seq<MeshBlock> Blocks,
    [property: OrderedEquality] Seq<MeshInstance> Instances,
    Instant At) {

    public bool IsBaked => Instances.ForAll(static i => i.Transform.IsIdentity);

    // Flatten the pool through the instance placements into ONE re-minted arena. The flatten is LANE-AGNOSTIC by
    // construction: it dispatches on the descriptor's own channel — Position rides Transform, Normal rides
    // TransformNormal, and EVERY other lane copies by its declared arity because a parameterization, a vertex colour,
    // and every future attribute are rigid-invariant. That is why a new EncodingChannel row costs this body nothing,
    // where the per-column `if (!uvs.IsEmpty)` ladder the parallel-buffer form grew per attribute is the deleted shape.
    // Unpack lifts each lane back to floats through its OWN dtype (unorm8 colour and float32 position alike), so the
    // re-mint hands Encode.Of the same float raws the original decode did and the witness is measured, never carried.
    // Each placed block INHERITS its source block's Material, so a block placed N times flattens to N blocks under
    // one shading key and the partition survives the flatten a per-material consumer would otherwise have to rebuild.
    public Fin<ImportedGeometry> Bake(Op key) {
        if (IsBaked) { return Fin.Succ(this); }
        ReadOnlySpan<long> x = Indices.AsSpan();
        // ONE fold carries both totals: v5 Seq exposes no selector Sum, and two passes over the placement run to
        // read two columns of the same block is the shape a single tuple-state fold states once.
        var (vertexTotal, indexTotal) = Instances.Fold(
            (Vertices: 0, Indices: 0),
            (total, instance) => Blocks[instance.Block] switch {
                var block => (total.Vertices + block.VertexCount, total.Indices + block.IndexCount),
            });
        long[] indices = new long[indexTotal];
        (EncodingChannel Channel, float[] Source, float[] Placed)[] lanes = [.. Lanes.Descriptors.Map(d => {
            float[] source = new float[d.Floats];
            d.Dtype.Unpack(Lanes.Channel(d.Channel).Span, source);
            return (d.Channel, source, new float[vertexTotal * d.Channel.Arity]);
        })];
        // Instances walks as a FOLD threading (blocks, placed, vSlot, iSlot) — the span copies inside keep their
        // named kernel exemption (a measured write into one pre-sized arena), but the Seq accumulators do not: a
        // mutable `blocks = blocks.Add(...)` rebind is a domain accumulator the fold state carries honestly.
        var (blocks, placed, vSlot, iSlot) = Instances.Fold(
            (Blocks: Seq<MeshBlock>(), Placed: Seq<MeshInstance>(), VSlot: 0, ISlot: 0),
            (state, instance) => {
                MeshBlock block = Blocks[instance.Block];
                foreach (var lane in lanes) {                                  // Exemption: a measured span kernel over one pre-sized arena
                    int arity = lane.Channel.Arity;
                    for (int k = 0; k < block.VertexCount; k++) {
                        int src = (block.VertexOffset + k) * arity, dst = (state.VSlot + k) * arity;
                        if (lane.Channel == EncodingChannel.Position || lane.Channel == EncodingChannel.Normal) {
                            Vector3 read = new(lane.Source[src], lane.Source[src + 1], lane.Source[src + 2]);
                            Vector3 moved = lane.Channel == EncodingChannel.Position
                                ? Vector3.Transform(read, instance.Transform)
                                : Vector3.TransformNormal(read, instance.Transform);
                            (lane.Placed[dst], lane.Placed[dst + 1], lane.Placed[dst + 2]) = (moved.X, moved.Y, moved.Z);
                        }
                        else {
                            lane.Source.AsSpan(src, arity).CopyTo(lane.Placed.AsSpan(dst, arity));
                        }
                    }
                }
                for (int k = 0; k < block.IndexCount; k++) {
                    indices[state.ISlot + k] = state.VSlot + (x[block.IndexOffset + k] - block.VertexOffset);
                }
                return (
                    state.Blocks.Add(new MeshBlock(state.VSlot, block.VertexCount, state.ISlot, block.IndexCount, block.Declared, block.Material)),
                    state.Placed.Add(new MeshInstance(state.Blocks.Count, Matrix4x4.Identity)),
                    state.VSlot + block.VertexCount,
                    state.ISlot + block.IndexCount);
            });
        return Encode.Of(vertexTotal, toSeq(lanes).Map(static lane => (lane.Channel, lane.Placed)), key)
            // AsImmutableArray hands the locally-minted array over WITHOUT a copy — the array never escapes this
            // body, so the immutable owner's no-aliasing invariant holds by construction, and a defensive [..indices]
            // would double the index buffer of every baked import for nothing.
            .Map(arena => this with {
                Lanes = arena, Indices = ImmutableCollectionsMarshal.AsImmutableArray(indices),
                VertexCount = vertexTotal, TriangleCount = indexTotal / 3, Blocks = blocks, Instances = placed,
            });
    }
}
```

## [05]-[IMPLEMENTATION_LAW]

- [TWO_INTERFACE_SPLIT]: `Rasm.Element` declares exactly two instance-interface floors [M3] — `IElementProjection` (the projector floor each AEC peer implements over its captured foreign source, lowering onto a `GraphDelta`) and `IGraphConstraint` (the IFC-semantic legality Bim implements, validating a delta against the graph) — so the seam's total `Switch` enforces ONLY the structural edge law and schema legality lives in the consumer's constraint. Cross-stratum alignment seats both floors at the lowest stratum the closed-vocabulary siblings depend up on and implement, aligning by contract without sibling references, each package usable in isolation. Both floors are open points foreign code plugs into (the `OPEN_FLOOR_DISPATCH` form) — the projector returning `Fin` for one dependent lowering, the constraint returning `Validation` for independent legality rules accumulating — never a `[Union]` the foreign assembly extends and never an instance default-interface-member.
- [OWNER_MINTS_IDENTITY]: each owner mints its concept's `Object` under the ONE rooted-identity regime (`Graph/element#NODE_MODEL` `ObjectKind ∈ {Type, Occurrence}`), so `Rasm.Materials` owning Component Types mints the DETERMINISTIC-rooted Type id through the kernel Type-seed `NodeId` derivation — rooted yet a pure function of the `Object` canonical content with the volatile `Representations` EXCLUDED from the seed, so identical Components dedup to one Type and a later geometry attach never re-keys it. Determinism is LOAD-BEARING: a pure-function id is known BEFORE the projection runs, so the owner seeds it into `ElementIds` with no minting race. Occurrence-authoring projectors mint the Guid-v7 Occurrence id through `NodeId.Rooted()`, the IFC GlobalId riding a Bim-stored `Object.ExternalId` [H6].
- [ASPECT_VOUCH]: aspect projectors mint NOTHING and author edges only INTO a context-vouched id through `ProjectionContext.Owns`, composing `ctx.Owns(element) ? Link(...) : ProjectionFault.Unvouched(...)` — skip-vs-rail stays the projector's policy, because a pure-isolation run authoring no edge is no fault while a binding to an unvouched element MUST rail, never a silent drop [H12]. Mint-vs-vouch splits per CONCEPT, never per projector: the `ComponentProjector` mints the Type it OWNS and vouches the occurrence it BINDS through `Assign.TypeDefinition` in ONE `Project`, both mints owner-side compositions of the kernel `NodeId` floor.
- [APPLICATIVE_CAPTURE]: projector capture folds APPLICATIVELY, never monadically — projectors are INDEPENDENT, so the `INDEPENDENT_JOIN` law accumulates their faults: `projectors.Traverse(p => Capture(p, ctx).ToValidation()).As()` runs the `Validation` `Apply` over every captured projector and `Error.Combine` unions every foreign fault, where the rejected `.TraverseM`/`.Bind` capture silently discards every fault after the first. Admit-then-constrain tails MONADICALLY (`.ToFin().Bind(...)`) because the admission depends on the merged delta and the constraint on the admitted candidate — dependence licenses sequence, independence licenses accumulation, the carrier selecting the algebra.
- [CAPTURE_FUNNEL]: per-projector `Capture` lowers a THROWN foreign exception through the `Try.lift(() => projector.Project(ctx)).Run().MapFail(...).Bind(identity)` funnel — `Try.lift` preserving the raw `error.Message` a bare kernel `Op.Catch` re-wraps as `Fault.InvalidResult` — while a projector's OWN returned typed fault passes unchanged, and `.ToValidation()` carries the typed `Expected`-derived case onto the accumulating carrier so the combine keeps each fault recoverable (`error.IsType`/`HasCode`/`Filter` recurse over `ManyErrors`).
- [GRADED_VERDICT]: model checking grades in three owners with the `IGraphConstraint` floor untouched — a hard schema violation blocks, a best-practice miss warns, and a reviewed deviation is WAIVED and recorded — because a binary discard-on-any-violation floor either over-blocks or silences rules.
- [SEVERITY_GRAIN]: severity is REGISTRATION grain — the `ConstraintSeverity` `Blocks` column rides the `ConstraintRegistration` row the app root grades, because a registered constraint IS a rule family and a family whose rules split by grade registers as two rows, where a per-violation severity demands the floor return findings and forces every foreign implementor to re-shape.
- [WAIVER_GRAIN]: waivers pin at VIOLATION grain — a `ConstraintWaiver` pins `ConstraintFinding.KeyOf(violation)`, the `ContentAddress` over the violation's band `Code`, kernel `Category`, and producer-owned `Detail` discriminant through the seam `CanonicalWriter` and the kernel seed-zero hash, so an accepted deviation is replayable across runs and peers, matches exactly one issue, and never rides a positional ordinal or a message-substring probe.
- [WAIVER_STABILITY]: waiver keys REST on the `Projection/fault#FAULT_BAND` `[DETAIL_GRAMMAR]` law — `Message` is a frozen `<kind:colon-args>` token, so that grammar's append-only rule is what makes a stored waiver outlive the run authoring it, and a case whose detail is unfrozen foreign text (`ProjectorFaulted`) rails at the capture funnel before a finding keys on it.
- [FINDING_EVIDENCE]: receipts carry the evidence stream — an unwaived blocking finding aborts with the violations re-carried as `ManyErrors` (the `error.Filter<E>` recovery unchanged), while warnings and waived deviations ride `AssemblyReceipt.Findings` typed, so "assemble with warnings" is a first-class outcome and a compliance dashboard folds findings off receipts, never a log join.
- [HEADER_ESTABLISHMENT]: `ProjectionContext.Header` is LOAD-BEARING — `Assemble` SEEDS the monoid fold with it as the model-creating header FLOOR (`deltas.Fold(GraphDelta.Empty.Reheader(ctx.Header), Merge)`), so an assembly onto `Graph/element#ELEMENT_GRAPH` `Genesis` freezes under the intended schema/model-view/georeference/tolerance rather than the seed's default (`AdmitOnto` resolves `delta.Header.IfNone(graph.Header)` over the merged delta's resolved header). `Merge`'s `next.Header`-wins rule over a floor-seeded accumulator sets the precedence: a header-less projector's `None` leaves the floor intact, while a PRIMARY projector's authored `Reheader` (a Bim lowering reading `FILE_SCHEMA`/`FILE_NAME`) overrides it — so the ingested schema and `StepHeader` provenance reach the frozen snapshot (the H8 schema-span validation and the H9 owner-history re-emit both read that surviving header). `Header` rides the `GraphDelta` event and the frozen snapshot, never the `WorkingGraph`, so establishment is one floor-seed `Reheader` call, not a header field threaded through the working form.
- [HEADER_CLOBBER]: any trailing UNCONDITIONAL `Reheader(ctx.Header)` AFTER the fold clobbers every projector-authored header and defeats H8/H9; a context-carried header no fold reads is the prior dead-field form — both deleted.
- [ASSEMBLE_CAPABILITY]: `Rasm.Element` owns the `Assemble` fold — the composition CAPABILITY — and the `ProjectionSuite` minting seam its registration crosses, never the wiring: minting the suite from each package's factory product, binding the imported-IFC tessellation adapter (the `IfcConvert`/`ifcopenshell` companion [M5]), wiring the `Graph/element#NODE_MODEL` `GeometrySource` content-key port over the `Rasm.Persistence` object store, wiring the tabular→element map, and the DI are per-app composition-root concerns (`Rasm.Rhino`/`Rasm.Grasshopper` today), no APP-PLATFORM package hosting them.
- [PROJECTOR_ADMISSION]: every projector builds its delta through the `Put`/`Link`/`Reheader` builders with the structural edge law not yet run on it, so `Assemble` MUST admit through `GraphDelta.AdmitOnto`, never the raw persistence `ReplayOnto`; the projector owning both endpoints authors the `Associate` material edge gated on the `Owns` vouch under `[ASPECT_VOUCH]`, and an app-authored wire-side material edge is the deleted form.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
