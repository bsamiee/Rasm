# [ELEMENT_PROJECTION]

`Rasm.Element` projection is the cross-stratum alignment seam: TWO instance-interface floors the AEC peers depend up on and implement — aligning by contract without referencing each other — under the `Assemble` composition CAPABILITY the apps wire over them. `IElementProjection` lowers a projector's INTERNALLY-captured foreign source onto a `Graph/delta#GRAPH_DELTA` `GraphDelta`, so the seam folds deltas and never learns a provider surface; `IGraphConstraint` carries the IFC-semantic legality Bim implements, disjoint from the seam's own structural edge law [M3].

Model checking is GRADED, never binary: a constraint registers under its `ConstraintSeverity`, and a reviewed deviation is a `ConstraintWaiver` the context carries pinned to the violation's content key, so an accepted issue is typed run policy rather than a hand-stripped rerun. `Assemble` returns the `AssemblyReceipt` — the assembled `ElementGraph` a consumer bakes, the merged `GraphDelta` event body Persistence appends [DELTA_EVENT_RULING], and the graded `ConstraintFinding` set a review workflow and a compliance dashboard read.

Wiring — projector registration, tessellation adapter, `Graph/element#NODE_MODEL` `GeometrySource` port, DI — stays an APP composition-root concern, and IFC egress (`Emit`) is Bim-INTERNAL, never a seam member.

## [01]-[INDEX]

- [02]-[PROJECTION_CONTRACT]: `IElementProjection` the projector floor and its one `Project`, `ProjectionContext` the element-identity/header/primitives carrier with its `Owns` vouch predicate and `ConstraintWaiver` reviewed-deviation set, `TypeCandidate` the reverse-type row the Bim export and Materials admission compose, `TextureRoster`/`TextureCandidate` the texture hand-off rows, `ProjectionSuite` the graded-registration mint, and `ProjectionAssembly` the composition capability the apps wire, returning the `AssemblyReceipt`.
- [03]-[GRAPH_CONSTRAINT]: `IGraphConstraint` the IFC-semantic legality floor Bim implements, composed in `Assemble` after the structural admission, accumulating every violation applicatively — each violation graded by its `ConstraintRegistration` row's `ConstraintSeverity`, waived by content key, and the non-blocking findings landed typed on the receipt.
- [04]-[INTERCHANGE_CARRIER]: `ImportedGeometry` the one decoded interchange mesh-pool carrier the Bim import rail produces and the Compute tile/residency lane reads — the kernel `EncodedGeometry` lane arena beside the `Indices` column — with its `MeshBlock`/`MeshInstance` instancing-and-shading-partition overlay, the `MeshletBand` cluster row (kernel E2 — one band where Bim/AppUi/Compute held three same-shaped triplets), the railed `Of` admission, and the seam-owned placement-column `Bake` flatten.

## [02]-[PROJECTION_CONTRACT]

- Boundary: `TypeCandidate` is DECLARED HERE, once, because both its producer (`Rasm.Bim` `Projection/foreign#REINGEST` `Reingest.ExportTypeCandidates`) and its consumer (`Rasm.Materials` `Component/component#CATALOGUE` `ComponentCatalogue.AdmitImported`) reference this seam and never each other — the twin local spellings the contract-alignment idiom produced were two declarations of one row that an edit at either end forks silently, which is exactly the drift a shared owner forecloses; the alignment idiom stays correct for the `IIfcTypeReconciler` PORT (a behavioural seam neither end can host), and wrong for a pure data row both ends can reach. `TextureRoster`/`TextureCandidate` seat here under the SAME law — producer `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` `AppearanceProjection.RosterOf`, consumer `Rasm.Materials` `Raster/set#SET_INGEST` `SetIngest.Roster` — and the row carries only neutral columns: the IFC transform decode lowers at the producer's mint, so no host or IFC type reaches this seam.
- Owner: `IElementProjection` the projector strategy floor with one `Project`; `ProjectionContext` the projection input (element `NodeId` set + target `Header` + the kernel `CorrelationId`/`TenantContext` causal pair, carrying the `Owns` vouch predicate and the `ConstraintWaiver` reviewed-deviation set); `ConstraintSeverity` the `[SmartEnum<string>]` verdict grade whose `Blocks` column is the discard policy; `ConstraintRegistration` the graded constraint row the suite registers; `ConstraintFinding` the typed per-violation QA finding (severity + violation + content key + the pinning `Option<ConstraintWaiver>` itself, so the receipt records WHO waived and WHEN); `AssemblyReceipt` the assembly result carrier; `ProjectionSuite` the minting seam — the one typed registration value the app root builds from each owning package's factory product; `ProjectionAssembly` the static composition capability the seam owns and the apps wire.
- Entry: `IElementProjection.Project(ProjectionContext ctx)` lowers a concrete projector's captured foreign source onto a `GraphDelta` over the context's element identities, `Fin<T>` carrying the projector's own faults; `ProjectionSuite.Of(projectors, constraints)` mints the registration value — each `IElementProjection` arrives as an owning package's OWN factory product (`ComponentProjector.Of(source)` the Materials mint, the Bim `SemanticProjector` mint likewise package-owned) and each constraint as a `ConstraintRegistration.Of(constraint, severity)` graded row defaulting `Blocking`, the concrete internal and swappable behind its floor; `ProjectionAssembly.Assemble(suite, seed, ctx)` runs the capture→merge→establish→admit→constrain→fold pipeline over the suite returning the `AssemblyReceipt` (`Graph` + `Delta` + `Findings`); the railed `ProjectionContext.For(elementIds, header, key, at, correlation, tenant, waivers)` narrows its guard to correlation alone — tenancy arrives as the kernel `TenantContext`, whose absent case IS the root row, so the mint admits the supplied value unchanged and polices no blank string — `elementIds` carrying the owner-minted Type and Occurrence identities an aspect projector vouches against, the trailing `waivers` the reviewed-deviation set a model review authored.
- Auto: `Assemble` (1) preserves every projector-returned `Error`, captures an unknown throw as its exact exceptional `Error`, and accumulates every projector failure applicatively (`.Traverse(...).As().ToValidation()`, `Error.Combine` unioning the faults so a run where BOTH Bim and Materials fail reports both); a projector implementation alone classifies a documented provider refusal into its typed owner fault before returning; (2) on full success seeds the monoid fold with `GraphDelta.Empty.Reheader(ctx.Header)` and merges via `Merge`'s `next.Header`-wins rule; (3) structurally admits the merged delta through `GraphDelta.AdmitOnto` — the validating sibling of the raw persistence `ReplayOnto`, routing through `WorkingGraph.Apply` so `LegalLink` runs per `Link`; (4) validates the ADMITTED `applied.Delta` — the same body the receipt carries and Persistence appends, never the pre-admission merge — against every registered `IGraphConstraint`, grading each accumulated violation by its registration row's severity, pinning each waived finding's own `ConstraintWaiver` by content key, and discarding the candidate ONLY on an unwaived blocking finding; (5) folds onto the seed graph, the surviving findings riding the receipt.
- Receipt: the `Assemble` result is the `AssemblyReceipt` — the assembled `ElementGraph` a consumer bakes, the merged `GraphDelta` event body the `Rasm.Persistence` `Version/ledger` appends to the Marten stream (the one model-creating event, never a whole-graph snapshot) [DELTA_EVENT_RULING], and the graded `ConstraintFinding` set (warnings and waived deviations) the QA report persists beside the model instead of vanishing at the boundary.
- Packages: LanguageExt.Core (`Fin`/`Validation`/`Seq`/`ManyErrors` + the `TraverseM`/`Traverse` accumulation split + the `Fold` monoid + `ToValidation`/`ToFin` cross-rail bridges), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` the `ConstraintSeverity` grade), `Rasm` (the kernel `Op` key and numeric `Fault` identity), NodaTime (`Instant`), System.Collections.Frozen (`FrozenSet`), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter` the `ConstraintFinding.KeyOf` preimage writes through), `Projection/address#CONTENT_ADDRESS` (`ContentAddress` the `ConstraintFinding.KeyOf` identity mints through).
- Growth: a new aspect projector is one `IElementProjection` implementation in its owning package with one registration row at the app root; a new causal or runtime ingredient is one column on `ProjectionContext`; the unified Material/Component/Element paradigm adds NO new interface and NO new mint method, both mints being owner-side compositions of the kernel `Graph/element#NODE_MODEL` `NodeId` floor.
- Boundary: `ConstraintFinding.Key` and `ConstraintWaiver.Finding` carry `ContentAddress`; the seam preserves an unknown raised exception unchanged, and constraint identity projects numeric fault code with evidence.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using System.Runtime.InteropServices;
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
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ConstraintSeverity {
    public static readonly ConstraintSeverity Blocking = new("blocking", blocks: true);
    public static readonly ConstraintSeverity Warning = new("warning", blocks: false);

    public bool Blocks { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ConstraintWaiver(ContentAddress Finding, string Authority, Instant At);

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

[SmartEnum<string>]
public sealed partial class ChannelPolarity {
    public static readonly ChannelPolarity Direct = new("direct");
    public static readonly ChannelPolarity Inverted = new("inverted");
}

[SmartEnum<string>]
public sealed partial class TextureWrap {
    public static readonly TextureWrap Repeat = new("repeat");
    public static readonly TextureWrap ClampToEdge = new("clamp-to-edge");
    public static readonly TextureWrap MirroredRepeat = new("mirrored-repeat");
}

public readonly record struct TextureCandidate(
    string Channel,
    string Reference,
    ChannelPolarity Polarity,
    TextureWrap WrapU,
    TextureWrap WrapV,
    int CoordinateSet,
    double OffsetU,
    double OffsetV,
    double ScaleU,
    double ScaleV,
    double Rotation);

public readonly record struct TextureRoster(NodeId Appearance, Seq<TextureCandidate> Textures);

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
            ? new KernelFault.InvalidValue("projection correlation", "a minted correlation identity", Some(key))
            : Fin.Succ(new ProjectionContext(elementIds.ToFrozenSet(), header, key, at, correlation, tenant, waivers));

    public bool Owns(NodeId element) => ElementIds.Contains(element);
}

public sealed record ConstraintFinding(ConstraintSeverity Severity, Error Violation, ContentAddress Key, Option<ConstraintWaiver> Waiver) {
    public bool Waived => Waiver.IsSome;

    private const double UnquantizedGrid = 0.0;

    public static ConstraintFinding Of(ConstraintSeverity severity, Error violation, Seq<ConstraintWaiver> waivers) {
        ContentAddress key = KeyOf(violation);
        return new(severity, violation, key, waivers.Find(waiver => waiver.Finding == key));
    }

    public static ContentAddress KeyOf(Error violation) =>
        ContentAddress.Of(violation, UnquantizedGrid, static (v, w) => {
            if (v is Fault fault) { w.Bool(true).Ordinal(fault.Code); }
            else { w.Bool(false); }
            w.String(v.Message);
        });
}

public sealed record AssemblyReceipt(ElementGraph Graph, GraphDelta Delta, Seq<ConstraintFinding> Findings);

// --- [SERVICES] ------------------------------------------------------------------------
public interface IElementProjection {
    Fin<GraphDelta> Project(ProjectionContext ctx);
}

public sealed record ConstraintRegistration(IGraphConstraint Constraint, ConstraintSeverity Severity) {
    public static ConstraintRegistration Of(IGraphConstraint constraint, Option<ConstraintSeverity> severity = default) =>
        new(constraint, severity.IfNone(ConstraintSeverity.Blocking));
}

public sealed record ProjectionSuite {
    public Seq<IElementProjection> Projectors { get; }
    public Seq<ConstraintRegistration> Constraints { get; }

    private ProjectionSuite(Seq<IElementProjection> projectors, Seq<ConstraintRegistration> constraints) =>
        (Projectors, Constraints) = (projectors, constraints);

    public static ProjectionSuite Of(Seq<IElementProjection> projectors, Seq<ConstraintRegistration> constraints = default) =>
        new(projectors, constraints);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProjectionAssembly {
    public static Fin<AssemblyReceipt> Assemble(ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
        suite.Projectors
            .Traverse(projector => Capture(projector, ctx).ToValidation()).As()
            .Map(deltas => deltas.Fold(GraphDelta.Empty.Reheader(ctx.Header), static (acc, delta) => acc.Merge(delta)))
            .ToFin()
            .Bind(merged => merged.AdmitOnto(seed, ctx.Key)
                .Bind(applied => Constrain(suite.Constraints, applied.Delta, applied.Graph, ctx.Waivers)
                    .Map(findings => new AssemblyReceipt(applied.Graph, applied.Delta, findings))));

    private static Fin<GraphDelta> Capture(IElementProjection projector, ProjectionContext ctx) =>
        ctx.Key.Catch(() => projector.Project(ctx));

    private static Fin<Seq<ConstraintFinding>> Constrain(
        Seq<ConstraintRegistration> constraints, GraphDelta delta, ElementGraph graph, Seq<ConstraintWaiver> waivers) =>
        constraints.Bind(row =>
            row.Constraint.Validate(delta, graph).Match(
                Succ: static _ => Seq<ConstraintFinding>(),
                Fail: failure => Unpack(failure).Map(violation => ConstraintFinding.Of(row.Severity, violation, waivers))))
         is var findings && findings.Filter(static f => f.Severity.Blocks && !f.Waived).Map(static f => f.Violation)
         is var blocking && blocking.IsEmpty
            ? Fin.Succ(findings)
            : Fin.Fail<Seq<ConstraintFinding>>(Error.Many(blocking));
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

```csharp
// --- [SERVICES] ------------------------------------------------------------------------
public interface IGraphConstraint {
    Validation<Error, Unit> Validate(GraphDelta delta, ElementGraph graph);
}
```

## [04]-[INTERCHANGE_CARRIER]

- Owner: `ImportedGeometry` the decoded interchange mesh-POOL carrier — one kernel `Rasm.Drawing` `EncodedGeometry` arena holding every per-vertex lane (position, normal, UV, colour, and whatever the roster grows next) as descriptor-addressed slices of ONE payload, `Indices` the single non-channel column, each decoded source mesh occupying a `MeshBlock` range, `MeshInstance` rows placing blocks by rigid transform, `Bake()` flattening on demand; `MeshBlock` the pool range with its `Declared` channel-evidence set and its `Material` shading-partition key; `MeshInstance` the rigid placement.
- Entry: `Rasm.Bim` `Exchange/import#IMPORT_RAIL` decode arms construct it (one `Encode.Of` mint per decode over the lanes the source declares, one identity instance per block on a non-instanced source, one block per shading partition where the source splits a mesh by material) and `Rasm.Compute` `Runtime/tiles#TILE_PARTITION` slices baked leaves from it; `Bake()` is the ONE flatten — positions `Transform`, normals `TransformNormal`, every other lane copied rigid-invariant, each placed block inheriting its source `Material` — so a consumer needing world-space geometry calls the one owner and a consumer preserving instancing or shading partitions reads the overlay.
- Law: this seat is the `STRATA_TWIN` resolution — the carrier crosses `Rasm.Bim` (S2 producer) and `Rasm.Compute` (S3 consumer), two packages that never reference each other, so the shape homes at the lowest stratum both reach; the prior same-named twins (a Bim pool form without UVs, a Compute soup form without `Blocks`/`Instances`) merged onto this superset and both packages compose it. `EncodingChannel` supplies the lane set and the kernel `EncodedGeometry` the arena, so the interchange carrier and the kernel's own packed geometry speak ONE channel vocabulary, ONE dtype roster, and ONE payload layout — the per-channel `ReadOnlyMemory<float>` columns this record once held were a second encoding arena one stratum up, and the descriptor set is what makes a new lane cost zero columns here.
- Receipt: the arena's own `RoundTripWitness` — every mint measures per-lane quantization error against its dtype tolerance and carries the payload-rooted `GeometryHash` (`RoundTripWitness.Root` is `DigestRoot.Payload` on every arena this carrier holds, because a foreign decode carries no kernel `EncodeForm` source; a consumer keying dedup against source-rooted `Encode.Apply` digests must read `Root` before comparing), so a decode that silently lost precision fails `IsValid` rather than reaching a consumer; decode evidence still rides the Bim `ModelLoad` receipt and partition evidence the Compute `StreamSegment` receipt.
- Packages: LanguageExt.Core (`Seq`), NodaTime (`Instant`), Generator.Equals (`[Equatable]`/`[OrderedEquality]`/`[StringEquality]` — the carrier's structural equality, without which a content-keyed consumer compares two identical decodes unequal), `Rasm` (the kernel `Drawing.EncodedGeometry` arena with `Encode.Of`/`Channel`/`View<T>`, the `EncodingChannel` lane roster, and `ChannelDtype.Unpack`), BCL inbox (`System.Numerics` `Matrix4x4`/`Vector3` the rigid-placement currency, `ImmutableArray<long>` the index column, `ImmutableCollectionsMarshal.AsImmutableArray` the zero-copy handover).
- Growth: a new vertex attribute is one kernel `EncodingChannel` row — the arena addresses it by descriptor, `Bake` copies it by arity with no edit, and the tile partition and meshlet arm read it through `View<T>`, so the seam record gains NO column and no producing arm re-threads a buffer; a new placement semantic is one `MeshInstance` column and a new per-range fact one `MeshBlock` column; a source that begins splitting by material fills `Material` in its own decode arm with no seam edit; a per-format decode arm stays `Rasm.Bim`'s and a partition/encoding arm `Rasm.Compute`'s — the seam owns the SHAPE alone.
- Law: TWO vector types coexist by DELIBERATE CARVE, named here so neither drifts into the other's territory — the seam-owned `Graph/element#NODE_MODEL` `Vector3` is the ANALYTICAL/domain coordinate (the `AxisCurve`/`FootprintPolygon` shapes, the orientation classifier, the load-vector folds; it owns the domain algebra), while the BCL `System.Numerics` `Matrix4x4`/`Vector3` are INTERCHANGE-CARRIER LAYOUT types confined to `MeshInstance` and the `Bake` flatten, where the placement currency every decoder already speaks and the intrinsics the flatten runs on are the whole reason they are here. This carve forecloses a BCL vector crossing into analytical math and a seam `Vector3` appearing in an instance placement, standing as the declared exemption to the branch's one-coordinate law.
- Boundary: an absent lane is a MISSING DESCRIPTOR at both altitudes — the arena declares no descriptor for a channel the source never carried, and `MeshBlock.Declared` records the per-block set so the pools write only declared ranges; a zero-filled range standing in for an undeclared lane is the deleted form, because a consumer cannot tell fabricated zeros from measured ones.
- Boundary: `FormatKey` carries the `Rasm.Bim` `Exchange/format#FORMAT_AXIS` row KEY — the format VOCABULARY is S2 host-local row data the strata forbid below its stratum, so the seam records the canonical key string and the Bim end alone re-hydrates the row (`InterchangeFormat.Get`); the UV lane is `EncodingChannel.Uv` (TEXCOORD_0, arity 2, `Float32` by the kernel's own law so an unbounded surface parameter never clamps) and the colour lane `EncodingChannel.ColorRgba` (arity 4, `Unorm8`), each ABSENT from the descriptor set when the source declares none — an absent lane is a missing descriptor, never an empty buffer a consumer must length-probe — filled at the ONE decode, sliced by the tile partition with the same triangle gather, and encoded by the residency meshlet arm as its own stream, so a streamed cluster resolves a REAL unwrap and a REAL vertex colour with no second decode of the same bytes; `MeshBlock.Material` follows the `FormatKey` law — the source's OWN material address as an open string the producing `Rasm.Bim` end alone re-hydrates against the appearance projection, never a graph `NodeId` or an `AppearanceSummary` a decode does not hold and cannot seat below its stratum — and it keys a BLOCK rather than the carrier, because a per-material split IS a block partition; an absent key is a source that declared no partition, so an unsplit block never carries a fabricated material and a consumer grouping by key reads the split the source authored; geometry lanes here are the kernel's host-neutral packed arena, never a host geometry type, and the GRAPH keeps geometry by content hash — this carrier is the interchange DECODE product beside the graph, not a node payload.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct MeshBlock(
    int VertexOffset, int VertexCount, int IndexOffset, int IndexCount,
    Seq<EncodingChannel> Declared = default, Option<string> Material = default);

public readonly record struct MeshInstance(int Block, Matrix4x4 Transform);

public readonly record struct MeshletBand(
    MeshBlock Block, int VertexRun, int TriangleRun,
    Graph.Vector3 BoundCenter, double BoundRadius, Graph.Vector3 ConeAxis, double ConeCutoff);

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

    public static Fin<ImportedGeometry> Of(
        string formatKey, EncodedGeometry lanes, ImmutableArray<long> indices, int vertexCount, int triangleCount,
        Seq<MeshBlock> blocks, Seq<MeshInstance> instances, Instant at, Op key) =>
        instances.Find(instance => instance.Block < 0 || instance.Block >= blocks.Count).Match(
            Some: rogue => Fin.Fail<ImportedGeometry>(new KernelFault.OutOfRange("mesh-instance-block", rogue.Block, $"fall inside [0,{blocks.Count})", Some(key))),
            None: () => Fin.Succ(new ImportedGeometry(formatKey, lanes, indices, vertexCount, triangleCount, blocks, instances, at)));

    public Fin<ImportedGeometry> Bake(Op key) {
        if (IsBaked) { return Fin.Succ(this); }
        ReadOnlySpan<long> x = Indices.AsSpan();
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
        var (blocks, placed, vSlot, iSlot) = Instances.Fold(
            (Blocks: Seq<MeshBlock>(), Placed: Seq<MeshInstance>(), VSlot: 0, ISlot: 0),
            (state, instance) => {
                MeshBlock block = Blocks[instance.Block];
                foreach (var lane in lanes) {
                    int arity = lane.Channel.Arity;
                    ChannelPlacement placement = lane.Channel.Placement;
                    for (int k = 0; k < block.VertexCount; k++) {
                        int src = (block.VertexOffset + k) * arity, dst = (state.VSlot + k) * arity;
                        if (placement == ChannelPlacement.Invariant) {
                            lane.Source.AsSpan(src, arity).CopyTo(lane.Placed.AsSpan(dst, arity));
                        }
                        else {
                            Vector3 read = new(lane.Source[src], lane.Source[src + 1], lane.Source[src + 2]);
                            Vector3 moved = placement == ChannelPlacement.Positional
                                ? Vector3.Transform(read, instance.Transform)
                                : Vector3.TransformNormal(read, instance.Transform);
                            (lane.Placed[dst], lane.Placed[dst + 1], lane.Placed[dst + 2]) = (moved.X, moved.Y, moved.Z);
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
            .Map(arena => this with {
                Lanes = arena, Indices = ImmutableCollectionsMarshal.AsImmutableArray(indices),
                VertexCount = vertexTotal, TriangleCount = indexTotal / 3, Blocks = blocks, Instances = placed,
            });
    }
}
```

## [05]-[IMPLEMENTATION_LAW]

- [TWO_INTERFACE_SPLIT]: `Rasm.Element` declares exactly two instance-interface floors [M3] — `IElementProjection` (the projector floor each AEC peer implements over its captured foreign source, lowering onto a `GraphDelta`) and `IGraphConstraint` (the IFC-semantic legality Bim implements, validating a delta against the graph) — so the seam's total `Switch` enforces ONLY the structural edge law and schema legality lives in the consumer's constraint. Cross-stratum alignment seats both floors at the lowest stratum the closed-vocabulary siblings depend up on and implement, aligning by contract without sibling references, each package usable in isolation. Both floors are open points foreign code plugs into (the `OPEN_FLOOR_DISPATCH` form) — the projector returning `Fin` for one dependent lowering, the constraint returning `Validation` for independent legality rules accumulating — never a `[Union]` the foreign assembly extends and never an instance default-interface-member.
- [OWNER_MINTS_IDENTITY]: each owner mints its concept's `Object` under the ONE rooted-identity regime (`Graph/element#NODE_MODEL` `ObjectKind ∈ {Type, Occurrence}`), so `Rasm.Materials` owning Component Types mints the DETERMINISTIC-rooted Type id through the kernel Type-seed `NodeId` derivation — rooted yet a pure function of the `Object` canonical content with the volatile `Representations` EXCLUDED from the seed, so identical Components dedup to one Type and a later geometry attach never re-keys it. Determinism is LOAD-BEARING: a pure-function id is known BEFORE the projection runs, so the owner seeds it into `ElementIds` with no minting race. Occurrence-authoring projectors mint the Guid-v7 Occurrence id through `NodeId.Of(new NodeSeed.Placement())`, the IFC GlobalId riding a Bim-stored `Object.ExternalId` [H6].
- [ASPECT_VOUCH]: aspect projectors mint NOTHING and author edges only INTO a context-vouched id through `ProjectionContext.Owns`, composing `ctx.Owns(element) ? Link(...) : new ProjectionFault.Unvouched(...)` — skip-vs-rail stays the projector's policy, because a pure-isolation run authoring no edge is no fault while a binding to an unvouched element MUST rail, never a silent drop [H12]. Mint-vs-vouch splits per CONCEPT, never per projector: the `ComponentProjector` mints the Type it OWNS and vouches the occurrence it BINDS through `Assign.TypeDefinition` in ONE `Project`, both mints owner-side compositions of the kernel `NodeId` floor.
- [APPLICATIVE_CAPTURE]: projector capture folds APPLICATIVELY, never monadically — projectors are INDEPENDENT, so the `INDEPENDENT_JOIN` law accumulates their faults: `projectors.Traverse(p => Capture(p, ctx).ToValidation()).As()` runs the `Validation` `Apply` over every captured projector and `Error.Combine` unions every foreign fault, where the rejected `.TraverseM`/`.Bind` capture silently discards every fault after the first. Admit-then-constrain tails MONADICALLY (`.ToFin().Bind(...)`) because the admission depends on the merged delta and the constraint on the admitted candidate — dependence licenses sequence, independence licenses accumulation, the carrier selecting the algebra.
- [CAPTURE_FUNNEL]: per-projector `Capture` preserves a returned `Error` unchanged and an unknown throw as its exact exceptional `Error`; documented provider refusals classify at the implementing owner before return, and `.ToValidation()` accumulates without message reminting.
- [GRADED_VERDICT]: model checking grades in three owners with the `IGraphConstraint` floor untouched — a hard schema violation blocks, a best-practice miss warns, and a reviewed deviation is WAIVED and recorded — because a binary discard-on-any-violation floor either over-blocks or silences rules.
- [SEVERITY_GRAIN]: severity is REGISTRATION grain — the `ConstraintSeverity` `Blocks` column rides the `ConstraintRegistration` row the app root grades, because a registered constraint IS a rule family and a family whose rules split by grade registers as two rows, where a per-violation severity demands the floor return findings and forces every foreign implementor to re-shape.
- [WAIVER_GRAIN]: `ConstraintWaiver` pins `ConstraintFinding.KeyOf(violation)` from the fault's numeric code and self-contained evidence through `CanonicalWriter`.
- [WAIVER_STABILITY]: producer evidence stays complete at the finding boundary; numeric fault identity supplies routing while the evidence distinguishes occurrences.
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
