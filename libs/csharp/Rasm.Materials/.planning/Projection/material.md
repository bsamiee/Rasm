# [MATERIALS_PROJECTION]

THE MATERIAL PROJECTOR and THE MATERIAL-SUBGRAPH AUTHOR. `Rasm.Materials` is a PROJECTOR onto the shared `Rasm.Element` seam: one `MaterialProjector` implements the seam's single `IElementProjection` contract with one polymorphic `Fin<GraphDelta> Project(ProjectionContext ctx)` op, capturing every Materials catalogue internally (the `Appearance/graph#MATERIAL_LIBRARY` appearance rows, the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` typed engineering rows, the `Construction/assembly#MATERIAL_COMPOSITION` layer/profile/constituent buildup, the `Profiles/profile#PROFILE_RESOLUTION` section data) and lowering it into ONE additive `GraphDelta` of seam `Node`s and seam `Relationship`s the seam's generic `Assemble` fold merges with every sibling projector's delta. The projector authors the MATERIAL SUBGRAPH only — a content-addressed `Material` node per material (carrying the seam `MaterialComposition` and the `Discipline`-keyed seam `MaterialPropertySet` the Materials engineering rows lower into), a content-keyed `Appearance` node per resolved appearance (the neutral `AppearanceSummary` — PBR scalars + the content key of the full OpenPBR/MaterialX payload `Appearance/interchange#MATERIAL_WIRE` mints), and (for a profiled member) the M7 neutral `SectionProperties` BAKED onto the `ProfileSet` composition (resolved ONE-HOP from the `ProfileRef`). It authors NO assessment-input node: `Rasm.Compute` reads the projected `Material` node plies DIRECTLY (above the seam, no `IElementProjection`) and writes its `Assessment` `Result` node back, so the material's own `Discipline`-keyed `MaterialPropertySet` set IS the input — never a parallel input node. It NEVER mints an element identity: a `Material`/`Appearance` node is keyed by the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the node's own `Rasm.Element/Projection/address#CANONICAL_WRITER` `ToCanonicalBytes` projection (the kernel seed-zero `XxHash128` the seam composes, never a second hasher), so the same material projected twice is one node, and the element→material (and element→appearance) `Associate` edge is authored ONLY for a `MaterialBinding` whose element the `ProjectionContext` vouches (`ctx.ElementIds.Contains` per [H12]), the occurrence binding riding the seam `MaterialUsage` (`LayerSet`/`ProfileSet`/`None` per [C7]) the `Construction/assembly#MATERIAL_COMPOSITION` author produces — a binding to an element the context does not vouch for rails `ProjectionFault.Unvouched`, never an invented node. A pure-Materials composition (a spec carrying NO bindings) thus emits a complete material subgraph with NO dangling element edge; a Materials+app or Materials+Bim composition (a spec carrying bindings, every element vouched by `ctx.ElementIds`) additionally names the edge owner — and a spec carrying bindings the context does NOT vouch (including the empty-context case) rails `Unvouched` rather than SILENTLY dropping the bound edges, so the no-edge path is "no bindings authored", never "bindings present but context empty". The page composes the `Rasm.Element` seam contracts (`IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`AppearanceSummary`), the Materials catalogues, and the `MaterialFault`/`ProjectionFault` rails; it re-mints NO seam type and re-implements NO host or IFC author — the seam graph IS the wire, and `Rasm.Bim` reads the projected nodes one-hop, never a parallel Materials→IFC carrier.

## [01]-[INDEX]

- [01]-[MATERIAL_PROJECTOR]: the `MaterialProjector` `IElementProjection` owner, the `MaterialProjectionSource`/`MaterialSpec`/`MaterialBinding` captured-source records (with the M7 `ProfileRef`→section table), the `Project` fold producing the seam `GraphDelta`, the `ProjectionFault` band-2470 rail, and the content-addressed `NodeId.Content` minting that composes the seam `ContentAddress`.
- [02]-[MATERIAL_SUBGRAPH]: the `Material`/`Appearance` node authoring (the engineering-property lowering into the seam `Discipline`-keyed `MaterialPropertySet`, the appearance content-keying into the seam `AppearanceSummary`, the M7 `SectionProperties` bake onto a `ProfileSet` via `WithSection`), and the [H12]/[C7] element→material/appearance `Associate` edge with its seam `MaterialUsage` occurrence payload.

## [02]-[MATERIAL_PROJECTOR]

- Owner: `MaterialProjector` the sealed `IElementProjection`; `MaterialProjectionSource` the captured-source aggregate (specs + the M7 `ProfileRef`→section table); `MaterialSpec` the one-material projection unit; `MaterialBinding` the element-occurrence binding (seam `MaterialUsage`); `ProjectionFault` `[Union]` band 2470; the `Mint` content-id helper composing the seam `NodeId.Content`.
- Cases: one `MaterialProjector` shape over a `Seq<MaterialSpec>` — each `MaterialSpec` is a `MaterialId` + a `Construction/assembly#MATERIAL_COMPOSITION` seam `MaterialComposition` + the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Discipline`-keyed seam `MaterialPropertySet` set + an optional `Appearance/graph#MATERIAL_LIBRARY` appearance row + a `Seq<Discipline>` assessment-request set + a `Seq<MaterialBinding>` element-occurrence set; the projector is NEVER a per-discipline or per-family projector type, never a `SteelProjector`/`AppearanceProjector` sibling — one projector folds the whole material subgraph.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam contract op, `source.Specs.TraverseM(spec => ProjectMaterial(spec, ctx))` accumulating each spec's OWN delta then folding them through the cancellation-correct `Graph/delta#GRAPH_DELTA` `GraphDelta.Merge` MONOID (the EXACT `Projection/projection#PROJECTION_CONTRACT` `Assemble` shape — `TraverseM` → `Merge`-fold — not a hand-threaded accumulator), `Fin<T>` aborting on an unresolvable `ProfileRef` (`ProjectionFault.Unresolved` lifting the `Profiles/profile#PROFILE_RESOLUTION` fault) or an `Associate` binding to an element the context does not vouch for (`ProjectionFault.Unvouched`) — never a content-key collision, the H7 mint being TOTAL and idempotent (the same material projected twice mints one id and the duplicate add collapses to one node at the seam `WorkingGraph.Set` upsert when `AdmitOnto` folds the merged delta, so re-projection has no collision rail); `MaterialProjector.Of(MaterialProjectionSource source)` captures the source once, and the seam's `Assemble(projectors, constraints, seed, ctx)` re-merges this projector's delta with `Rasm.Bim`'s `SemanticProjector` delta (and any sibling) into one `ElementGraph` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`MaterialComposition`/`MaterialPropertySet`/`Discipline`/`ContentAddress`, the seam this folder projects onto), Rasm (project — `Op` the fault-correlation key; the seed-zero `XxHash128` content seed is the SEAM's `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` for `ProjectionFault`), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Bind`/`Option` for the projection rail).
- Growth: a new projected node kind is one arm on `ProjectMaterial` authoring its seam `Node` case (the seam owns the `Node` union; a kind the seam does not carry is a seam growth, never a Materials parallel node); a new assessment discipline is one `Discipline` row the `Assessment` input fold already dispatches over (no new arm); a new occurrence-usage shape is one seam `MaterialUsage` case the `[C7]` author already produces — the projector reads it through the seam's total `Switch`; a new projection fault is one `ProjectionFault` case. There is NO second projector surface, NO per-family projector, and NO Materials→IFC carrier beside the graph — growth is a spec row or a seam case the one `Project` fold absorbs.
- Boundary: `MaterialProjector` captures its foreign source INTERNALLY (§4C inversion) — the four Materials catalogues are private fields the projector reads, never `ProjectionContext` parameters, so a consumer constructs the projector from a `MaterialProjectionSource` and calls the seam op with zero knowledge of the Materials catalogue shapes; the projector authors the MATERIAL SUBGRAPH only and emits NO `Object` node and NO element-level `PropertySet`/`QuantitySet` bag (those are `Rasm.Bim`'s at IFC ingress) — it lowers the engineering rows into the `Material` node's own `Discipline`-keyed `MaterialPropertySet`, never an element Pset; every authored node is CONTENT-ADDRESSED through the projector's `Mint` helper which composes the seam `NodeId.Content(node.ToCanonicalBytes(tolerance))` ([H7] canonical bytes, the kernel seed-zero content hash, NO second hasher in this folder), so a material/appearance projected by two specs collapses to one node and the delta is idempotent under re-projection; the element→material (and element→appearance) `Associate` edge is authored ONLY for a binding whose `Element` is in `ctx.ElementIds` ([H12] — the projector never invents an element identity), the occurrence payload the seam `MaterialUsage` (`LayerSet`/`ProfileSet`/`None`) the `Construction/assembly#MATERIAL_COMPOSITION` author produced, so a pure-Materials run (a spec with NO bindings) emits the subgraph with no dangling edge and a Materials+app/Materials+Bim run (a spec WITH bindings, every element vouched) names the edge owner; a binding to an unvouched element rails `ProjectionFault.Unvouched`, never a fabricated `Object` — the `AuthorBindings` short-circuit is on `spec.Bindings.IsEmpty` (nothing to author), NOT on `ctx.ElementIds` emptiness (which would SILENTLY drop a bound spec's edges against an empty context, the §4C/[H12] violation: the per-binding vouch check rails `Unvouched` for every binding an empty context cannot vouch); the `Project` rail is `Fin<GraphDelta>` so the seam `Assemble` fold short-circuits a failed projection without a partial graph, and `ProjectionFault` (band 2470) is disjoint from `ProfileFault` 2300 / `ConstructionFault` 2350 / `ConnectionFault` 2360 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 yet lifts each owner's fault unchanged through `Fin` so a `Profiles/profile#PROFILE_RESOLUTION` or `MaterialFault.Parameter` propagates without re-wrapping.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element;                  // NodeId, Node, MaterialId, MaterialComposition, MaterialPropertySet, SectionProperties,
                                     // AppearanceSummary, Relationship, MaterialUsage, GraphDelta, ProfileRef, ResolvedProfile,
                                     // Classification (the Object-node (system, code) value the binding threads), IElementProjection,
                                     // ProjectionContext, ProjectionAssembly
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

// --- [ERRORS] ------------------------------------------------------------------------------
// The material-projection fault band (2470): Expected-derived over the kernel Rasm.Domain.Expected so band 2470 IS
// the Expected Code and a typed case lifts BARE onto Fin<T> (no .ToError() hop) — the seam Assemble fold short-circuits
// a failed projection without a partial graph. The kernel base ctor is PARAMETERLESS (Code a virtual Error member,
// Message abstract, Category virtual) — so band 2470 is a `Code => 2470` override and `Message => Detail`, and the
// per-case Category override drives FaultExtensions.Category(error); the legacy `base(detail, 2470, None)` form
// targeted the OTHER LanguageExt.Common.Expected (no Category to override) and was the defect. The band is disjoint
// from ProfileFault 2300 / ConstructionFault 2350 / ConnectionFault 2360 / kernel GeometryFault 2400 / MaterialFault
// 2450, yet lifts each owner's fault unchanged through Fin so a ProfileFault.Family or MaterialFault.Parameter
// propagates without re-wrapping. [SkipUnionOps] skips the generated implicit-conversion ops (every case carries an
// explicit Op) and emits NO per-case factory, so the band declares its own (the production UiFault / seam ElementFault
// shape): a nested `…Case` record carries the data and a same-name-less static factory ProjectionFault.Unvouched(key,
// detail) returns the Expected-derived base so the case lifts BARE onto Fin<T> with no `new` and no .ToError() hop —
// the `…Case` suffix frees the unsuffixed factory name (a same-named nested type + method is CS0102). Create routes the
// unspecific case under a boundary-admission Op, never a default Op.
[SkipUnionOps]
[Union]
public abstract partial record ProjectionFault : Expected, IValidationError<ProjectionFault> {
    private ProjectionFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2470;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record SourceCase(Op Key, string Detail)     : ProjectionFault(Key, Detail) { public override string Category => "Source"; }
    public sealed record UnvouchedCase(Op Key, string Detail)  : ProjectionFault(Key, Detail) { public override string Category => "Unvouched"; }
    public sealed record UnresolvedCase(Op Key, string Detail) : ProjectionFault(Key, Detail) { public override string Category => "Unresolved"; }

    public static ProjectionFault Source(Op key, string detail)     => new SourceCase(key, detail);
    public static ProjectionFault Unvouched(Op key, string detail)  => new UnvouchedCase(key, detail);
    public static ProjectionFault Unresolved(Op key, string detail) => new UnresolvedCase(key, detail);
    public static ProjectionFault Create(string message) => Source(Admission, message);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The element-occurrence binding the app or Rasm.Bim supplies: which VOUCHED seam element NodeId this material
// binds, the seam MaterialUsage (C7) occurrence usage (LayerSet direction/sense/offset, or ProfileSet
// cardinal-point/extent) the Construction/assembly#MATERIAL_COMPOSITION author produced, and the material's
// resolved standard Classification (the Properties/sustainability#SUSTAINABILITY_PROPERTY (system, code) the Capture
// composed) the bound element's Object node carries. The Element MUST be vouched by ctx.ElementIds — the projector
// never mints an element identity (§4C/H12). Classification is an Object-node VALUE (Rasm.Element/Classification/
// classification#CLASSIFICATION_AXIS) — NOT a Node.Material field and NOT an edge: the seam Relations/relation#
// EDGE_ALGEBRA is explicit that "classification is a generic value ON the Object node, NOT an edge", the Associate
// edge carrying a Material/Appearance resource ONLY, and Node.Material carries no Classification column. So a material
// that publishes a Uniclass/OmniClass code threads it onto the SAME Object-node Classifications set Rasm.Bim's
// Semantics/classification ReauthorClassifications authors IfcRelAssociatesClassification from — the binding carries
// the resolved value to the Object-node owner (Bim ingest or the from-scratch app) rather than re-minting the
// material-level classification surface the seam retired (the migration MaterialPropertyWire.Classification half).
public readonly record struct MaterialBinding(NodeId Element, MaterialUsage Usage, Option<Classification> Classification);

// One material to project: the seam MaterialId, the seam MaterialComposition the assembly author built (a
// ProfileSet carrying the M7 ProfileRef the projector resolves and bakes the SectionProperties onto), the
// Discipline-keyed seam MaterialPropertySet set the catalogue lowered, the optional content-keyed AppearanceSummary,
// and the element occurrence bindings. NO assessment-input here — Rasm.Compute reads the projected Material node
// plies DIRECTLY (above the seam) and writes the Assessment result back, so Materials authors no input node.
public sealed record MaterialSpec(
    MaterialId Material,
    MaterialComposition Composition,
    Seq<MaterialPropertySet> Properties,
    Option<AppearanceSummary> Appearance,
    Seq<MaterialBinding> Bindings) {
    public static MaterialSpec Of(MaterialId material, MaterialComposition composition, Seq<MaterialPropertySet> properties) =>
        new(material, composition, properties, Option<AppearanceSummary>.None, Seq<MaterialBinding>());
}

// The captured projection source: the per-material specs plus the M7 ProfileRef→section resolution table
// (Profiles/profile#PROFILE_RESOLUTION, built once above the seam) the projector bakes onto ProfileSet plies.
public sealed record MaterialProjectionSource(Seq<MaterialSpec> Specs, FrozenDictionary<ProfileRef, ResolvedProfile> Sections) {
    public static readonly MaterialProjectionSource Empty = new(Seq<MaterialSpec>(), FrozenDictionary<ProfileRef, ResolvedProfile>.Empty);
    public MaterialProjectionSource With(MaterialSpec spec) => this with { Specs = Specs.Add(spec) };
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Materials folder publishes. Captures the source internally (§4C) so the seam
// op carries only the ProjectionContext; the seam ProjectionAssembly.Assemble merges this delta with every sibling.
public sealed class MaterialProjector : IElementProjection {
    readonly MaterialProjectionSource source;
    MaterialProjector(MaterialProjectionSource source) => this.source = source;
    public static MaterialProjector Of(MaterialProjectionSource source) => new(source);

    // TraverseM each spec to its OWN delta (the monadic accumulation short-circuiting on the first Unvouched/Unresolved
    // spec), then fold the per-spec deltas through the cancellation-correct GraphDelta.Merge MONOID — the EXACT seam
    // ProjectionAssembly.Assemble shape (TraverseM -> Merge-fold), not a hand-threaded accumulator. Each spec builds on
    // GraphDelta.Empty so per-spec projection is decoupled from the running delta, and Merge (not naive .Put concatenation
    // across specs) keeps the projector's contribution a faithful single delta the seam re-merges with every sibling.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        source.Specs.TraverseM(spec => ProjectMaterial(spec, ctx)).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));

    // --- [SUBGRAPH_FOLD]
    // One spec → its OWN content-addressed Material node (with the M7 section baked onto a ProfileSet), an optional
    // content-keyed Appearance node, and the vouched element→material / element→appearance Associate edges — built on
    // GraphDelta.Empty (the seam Merge composes the per-spec deltas), never a threaded accumulator. Each node id is
    // minted through the seam content address (NodeId.Content over the id-excluded canonical bytes), so this folder owns
    // NO hasher and two specs projecting the same material mint ONE id — the duplicate add collapses to one node at the
    // seam WorkingGraph.Set upsert (last-write-wins on the HAMT) when AdmitOnto/ReplayOnto folds the merged delta.
    Fin<GraphDelta> ProjectMaterial(MaterialSpec spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Node.Material material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, BakeSection(spec.Composition), spec.Properties), tolerance);
        Option<Node.Appearance> appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance));
        GraphDelta withNodes = appearance.Match(Some: a => GraphDelta.Empty.Put(material).Put(a), None: () => GraphDelta.Empty.Put(material));
        return AuthorBindings(spec, material.Id, appearance.Map(static a => a.Id), ctx, withNodes);
    }

    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured Profiles resolution table and BAKE the
    // neutral seam SectionProperties onto the composition (WithSection), so the structural runner reads
    // graph.SectionOf(member) without re-resolving or admitting VividOrange; a non-ProfileSet or unresolved ref bakes nothing.
    MaterialComposition BakeSection(MaterialComposition composition) =>
        composition is MaterialComposition.ProfileSet ps && source.Sections.TryGetValue(ps.Profile, out ResolvedProfile resolved)
            ? resolved.Section.Match(Some: section => composition.WithSection(SeamSection(section)), None: () => composition)
            : composition;

    // The neutral seam SectionProperties lifted from the Profiles ComputedSection — the full SEVENTEEN-column structural
    // set in the SEAM's declared order (Area, Iyy, Izz, J, Iw, Wely, Welz, Wply, Wplz, AvY, AvZ, RadiusOfGyrationMajor,
    // RadiusOfGyrationMinor, Depth, Width, HeatedPerimeter, AxisDistance — Iw FIFTH), each a typed MeasureValue converted
    // from the SI-millimetre ComputedSection magnitude to SI base (mm->m, mm2->m2, mm3->m3, mm4->m4, mm6->m6). The section
    // moduli stamp a "SectionModulus" QuantityType over VolumeDim, the second moments / torsion a
    // "SecondMomentOfArea"/"TorsionConstant" type over the L^4 Dimension, and the warping constant a "WarpingConstant"
    // type over the L^6 Dimension, so none reads as a Volume; the areas stamp the Area row, the lengths the Length row.
    // The shear-area lift preserves the ComputedSection major/minor convention: AvyMm2 (MAJOR-axis/web) -> AvY, AvzMm2
    // (MINOR-axis/flange) -> AvZ, so a downstream EN 1993 §G web-shear consumer reads AvY as the major-axis shear; the
    // RadiusOfGyrationMajor/Minor lift mirrors it (RxMm major -> Major, RyMm minor -> Minor). Named arguments PIN each lift
    // to its seam field so a future seam re-order cannot silently re-slot a column. The seam graph carries the baked scalars
    // and Rasm.Compute reads graph.SectionOf(member) without re-resolving or admitting VividOrange.
    static SectionProperties SeamSection(ComputedSection c) {
        static MeasureValue Len(double mm)      => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, mm * 1e-3);
        static MeasureValue Area(double mm2)    => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, mm2 * 1e-6);
        static MeasureValue Modulus(double mm3) => MeasureValue.OfSi(QuantityType.Create("SectionModulus"), Dimension.VolumeDim, mm3 * 1e-9);
        static MeasureValue Inertia(double mm4) => MeasureValue.OfSi(QuantityType.Create("SecondMomentOfArea"), Dimension.Create(4, 0, 0, 0, 0, 0, 0), mm4 * 1e-12);
        static MeasureValue Torsion(double mm4) => MeasureValue.OfSi(QuantityType.Create("TorsionConstant"), Dimension.Create(4, 0, 0, 0, 0, 0, 0), mm4 * 1e-12);
        static MeasureValue Warping(double mm6) => MeasureValue.OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6, 0, 0, 0, 0, 0, 0), mm6 * 1e-18);
        return new(
            Area: Area(c.AreaMm2.Value), Iyy: Inertia(c.IxMm4.Value), Izz: Inertia(c.IyMm4.Value), J: Torsion(c.JMm4.Value), Iw: Warping(c.IwMm6),
            Wely: Modulus(c.SxMm3.Value), Welz: Modulus(c.SyMm3.Value), Wply: Modulus(c.ZxMm3.Value), Wplz: Modulus(c.ZyMm3.Value),
            AvY: Area(c.AvyMm2.Value), AvZ: Area(c.AvzMm2.Value), RadiusOfGyrationMajor: Len(c.RxMm.Value), RadiusOfGyrationMinor: Len(c.RyMm.Value),
            Depth: Len(c.DepthMm.Value), Width: Len(c.WidthMm.Value), HeatedPerimeter: Len(c.HeatedPerimeterMm.Value), AxisDistance: Len(c.AxisDistanceMm));
    }

    // Content-id mint — composes the seam Graph/element#NODE_MODEL NodeId.Content over the node's OWN
    // Projection/address#CANONICAL_WRITER canonical bytes (H7, the kernel seed-zero XxHash128 the seam
    // Projection/address#CONTENT_ADDRESS ContentAddress wraps; this folder owns no hasher). ToCanonicalBytes
    // EXCLUDES the id, so the placeholder id the draft carries never affects the minted identity.
    static Node.Material Mint(Node.Material draft, double tolerance) => draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };
    static Node.Appearance Mint(Node.Appearance draft, double tolerance) => draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };

    // --- [OCCURRENCE_EDGES]
    // H12: author element→material (and element→appearance) Associate edges ONLY for bindings whose Element is
    // vouched by ctx.ElementIds. The short-circuit is on spec.Bindings.IsEmpty (genuinely no edge to author — the
    // pure-Materials/material-subgraph run usable in isolation), NOT ctx.ElementIds.IsEmpty: a spec that DOES carry
    // bindings must rail ProjectionFault.Unvouched for any binding the context does not vouch (an empty context vouches
    // none, so every binding faults) — gating on the context emptiness would SILENTLY DROP a bound spec's edges, the
    // §4C/H12 violation. The per-binding ctx.ElementIds.Contains check thus owns both cases: a bound element vouched →
    // BindElement, an unvouched (or empty-context) element → Unvouched, never an invented Object node. The appearance
    // binds the element directly (the seam attaches Appearance to the Object with MaterialUsage.None); the
    // material→appearance default pairing lives in the catalogue, not an edge.
    static Fin<GraphDelta> AuthorBindings(MaterialSpec spec, NodeId materialId, Option<NodeId> appearanceId, ProjectionContext ctx, GraphDelta delta) =>
        spec.Bindings.IsEmpty
            ? Fin.Succ(delta)
            : spec.Bindings.Fold(
                Fin.Succ(delta),
                (acc, binding) => acc.Bind(g => ctx.ElementIds.Contains(binding.Element)
                    ? Fin.Succ(BindElement(g, binding, materialId, appearanceId))
                    : ProjectionFault.Unvouched(ctx.Key, $"<associate-element-not-in-context:{binding.Element.Value}>")));

    static GraphDelta BindElement(GraphDelta delta, MaterialBinding binding, NodeId materialId, Option<NodeId> appearanceId) =>
        appearanceId.Match(
            Some: appearance => delta
                .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))
                .Link(new Relationship.Associate(binding.Element, appearance, new MaterialUsage.None())),
            None: () => delta.Link(new Relationship.Associate(binding.Element, materialId, binding.Usage)));
}
```

## [03]-[MATERIAL_SUBGRAPH]

- Owner: the `ProjectMaterial` node-authoring fold (the `Material`/`Appearance` seam `Node` cases the projector emits), the engineering-row lowering into the seam `Discipline`-keyed `MaterialPropertySet`, the appearance content-keying into the seam `AppearanceSummary`, the M7 `BakeSection` (`ProfileRef`→neutral `SectionProperties` via `WithSection`), and the `[H12]`/`[C7]` element→material/appearance `Associate` edge over the seam `MaterialUsage`.
- Cases: two projected node kinds over one fold — `Material` (the `MaterialId` + seam `MaterialComposition` with the M7 section baked + the `Discipline`-keyed `MaterialPropertySet` set, content-keyed) · `Appearance` (the `Appearance/interchange#MATERIAL_WIRE` content-keyed seam `AppearanceSummary`) — and the `Associate` element→material / element→appearance occurrence bindings; the projector authors NO `Assessment` node (Compute reads the `Material` plies directly and writes the `Assessment` `Result`), and a node kind the seam union does not carry is a seam growth, never a fourth Materials node.
- Entry: `Fin<GraphDelta> ProjectMaterial(MaterialSpec spec, ProjectionContext ctx)` mints the content-addressed `Material` node (with the M7 section baked) and the optional `Appearance` node onto a fresh `GraphDelta.Empty`, then authors the vouched bindings — the per-spec delta the `Project` `TraverseM` collects and the `GraphDelta.Merge` monoid composes, never a threaded accumulator; `MaterialWire.Summary(MaterialParameters parameters)` (on `Appearance/interchange`) lowers the resolved appearance to the content-keyed seam `AppearanceSummary`, composed here, never re-derived; `Rasm.Compute` reads the projected `Material` node plies directly, so there is no in-folder assessment-input marshaller. The `MaterialSubgraph` `[PROFILE_FAMILY_CAPTURE]` ops lower a resolved profile-family section into a `MaterialSpec`: `CaptureTimber(TimberSection, ProfileId, …)` ROUTES the composition by `TimberForm.Shape` (a `MaterialShapeKind.LayeredPanel` CLT through `TimberSection.ToLayerSet`, a `ProfiledMember` through `CompositionAuthor.ProfileSet`'s M7 `ProfileRef` the `BakeSection` path resolves) and composes the seam `Orthotropic` stiffness (`TimberSection.ToProperties`) with the OPTIONAL catalogue physical rows, and `CaptureGlazing(GlazingSection, ProfileId, …)` lowers the always-`LayerSet` IGU (`GlazingSection.ToLayerSet`) with the COMPUTED `GlazingSection.ToProperties` set — the family→spec composition selection and the computed-property lowering the homogeneous `Capture` could not carry, both threading the C7 usage and the `SustainabilityCatalogue` Object-node `Classification` onto the `MaterialBinding` exactly as `Capture` does.
- Packages: Rasm.Element (project — `Node` cases, `MaterialPropertySet`, `Relationship.Compose`/`Assign`/`Associate`, `MaterialUsage`, `Discipline`, `ContentAddress`), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Option` for the node folds), Thinktecture.Runtime.Extensions (the seam unions' generated `Switch` the usage dispatch reads).
- Growth: a new engineering discipline routed to the material is one `Discipline` row the seam `MaterialPropertySet` carries (Compute's route dispatch reads it off the `Material` node) — no projector arm; a new appearance payload channel is one column on the seam `AppearanceSummary` the `MaterialWire` lowering fills; a new occurrence-usage shape is one seam `MaterialUsage` case the `Construction/assembly#MATERIAL_COMPOSITION` author produces and the projector passes through unread (the usage is opaque to the projector — only the assembly author and `Rasm.Bim`'s emitter interpret it). The subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: the `Material` node carries its OWN `MaterialPropertySet` (the material physics keyed by `Discipline`) and NEVER an element-level `PropertySet`/`QuantitySet` bag — those are `Rasm.Bim`'s at IFC ingress, so the projector authors no element Pset and the engineering rows lower into the material node's intrinsic property set exactly as the task routes the `MaterialProperty` unions; the seam `MaterialPropertySet.Acoustic` case carries the intrinsic pure folds (`Nrc`/`Saa`/`StcWeighted` over the shared `RatingContour.Stc.Fit` contour kernel, [Rasm.Element]`Composition/acoustic`) so a Materials consumer reading an acoustic rating reads the SEAM fold, never a re-authored one here; the `Appearance` node is content-keyed on the `AppearanceSummary` (PBR scalars + the content key of the OpenPBR/MaterialX payload) so two materials with identical appearance share one node and the GLB/render seam keys appearance by the same content hash the geometry seam uses; the projector authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the `Material` node IS the input `Rasm.Compute` reads DIRECTLY (above the seam) to run the closed-form/solver route and write its seam `Assessment` `Result` node content-keyed on `(input key, route)` — the multi-ply `AssemblyAggregator` is `Rasm.Compute`'s (the seam carries the per-material property set, never the assembly fold); the `Associate` edge's `Subject` is always a context-vouched element NodeId and its `Resource` the content-keyed material (or appearance) node, so the edge is directional element→resource and the occurrence usage rides the edge not the type-level composition ([C7] — the `MaterialComposition` Set is the material's type-level buildup, the seam `MaterialUsage` the per-occurrence binding); the whole subgraph is one additive `GraphDelta` the seam's `IGraphConstraint.Validate` ([M3], `Rasm.Bim`-implemented) gates for IFC-semantic legality before the seam folds it, so the projector enforces only the structural invariants (content-key idempotence, context-vouched edges) and defers IFC legality to the seam constraint; a material's standard `Classification` (the `Properties/sustainability#SUSTAINABILITY_PROPERTY` `(system, code)` the `Capture` composes) is an Object-node VALUE — NOT a `Node.Material` field (the seam `Material` node carries `MaterialId`/`MaterialComposition`/`MaterialPropertySet` only) and NOT an `Associate` edge payload (the seam `Relations/relation#EDGE_ALGEBRA` carries no classification relationship, the edge carrying a `Material`/`Appearance` resource ONLY) — so it rides the `MaterialBinding` to the Object-node owner (`Rasm.Bim` at IFC ingest, or the from-scratch app), which folds it into the bound element's Object-node `Classifications` set the `Rasm.Bim` `Semantics/classification#CLASSIFICATION_AXIS` `Author` re-emits onto `IfcRelAssociatesClassification`, the projector never authoring the foreign Object node ([H12]) nor re-minting the retired material-level classification wire.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;                              // Op
using Rasm.Element;                             // MaterialId, NodeId, MaterialUsage, MaterialComposition, ProfileRef, ResolvedProfile, MaterialPropertySet, Classification
using Rasm.Materials.Properties;                // MaterialPropertyCatalogue, SustainabilityCatalogue
using Rasm.Materials.Construction;              // CompositionAuthor
using Rasm.Materials.Profiles;                  // ProfileId, ResolvedProfile, ComputedSection (the parent PROFILE_OWNER section receipt + M7 resolution carrier)
using Rasm.Materials.Profiles.Timber;           // TimberSection, MaterialShapeKind (the timber#TIMBER_FAMILY lowerings, in its own sub-namespace)
using Rasm.Materials.Profiles.Glazing;          // GlazingSection (the glazing#GLAZING_FAMILY lowering, in its own sub-namespace)
using Rasm.Materials.Appearance.Graph;          // MaterialLibrary
using Rasm.Materials.Appearance.Interchange;    // MaterialWire (the AppearanceSummary lowering)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] --------------------------------------------------------------------------
// A worked composition root: capture the catalogues into specs against a PRE-BUILT M7 ProfileRef→section resolution
// table (Profiles/profile#PROFILE_RESOLUTION ProfileResolution.Build, supplied once by the caller so the section
// integral runs once per profile, never per material), then project against a context. The element NodeIds in
// ctx.ElementIds come from Rasm.Bim (IFC ingress) or the app (from-scratch authoring, H6 kernel-minted); Materials
// binds to them via the seam MaterialUsage CompositionAuthor.UsageOf derives, never invents them. Rasm.Compute reads
// the projected Material node plies DIRECTLY and writes the Assessment result back — Materials authors no
// assessment-input node (the material's own Discipline-keyed MaterialPropertySet set IS the input Compute consumes).
// Every catalogue resolution rides the ONE Fin rail: the REQUIRED engineering set rails MaterialFault.Parameter for an
// unregistered material, the OPTIONAL lifecycle set returns Fin.Succ(empty), the OPTIONAL Object-node Classification
// (the SustainabilityCatalogue.Classification (system, code) EGRESS lifting through the seam Classification.Of, None
// when the row or material is absent) is the CONSUMER the catalogue's classification column otherwise lacked, and the
// C7 usage threads CompositionAuthor.UsageOf's Fin<MaterialUsage> (the seam MaterialUsage.ProfileSet.Of grid admission)
// rather than discarding its key — so a from-scratch capture short-circuits on the first unregistered material,
// malformed classification pair, or out-of-grid usage. The resolved classification rides the MaterialBinding (NOT the
// Material node, NOT an edge — classification is an Object-node VALUE per Rasm.Element/Relations/relation): the
// Object-node owner (Rasm.Bim at IFC ingest, or the from-scratch app authoring the Object) folds binding.Classification
// into the Object node's Classifications set, the SAME set Rasm.Bim's Semantics/classification ReauthorClassifications
// authors IfcRelAssociatesClassification from — so a material's Uniclass/OmniClass code reaches IFC through the bound
// element's Object node, the seam home classification.md names, never through a retired material-level wire.
public static class MaterialSubgraph {
    public static Fin<MaterialProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Option<NodeId>> elementOf,
        FrozenDictionary<ProfileRef, ResolvedProfile> sections,
        Op key) =>
        materials.Fold(
            Fin.Succ(MaterialProjectionSource.Empty with { Sections = sections }),
            (acc, id) => acc.Bind(source =>
                from engineering in MaterialPropertyCatalogue.Lookup(id, key)                        // Properties/properties#MATERIAL_PROPERTY_CATALOGUE → seam Mechanical/Thermal/Acoustic/Fire cases (REQUIRED)
                from lifecycle in SustainabilityCatalogue.Lookup(id, key)                            // Properties/sustainability#SUSTAINABILITY_PROPERTY → seam Environmental/Cost cases (OPTIONAL, empty when absent)
                from classification in SustainabilityCatalogue.Classification(id, key)               // Properties/sustainability#SUSTAINABILITY_PROPERTY → the material's Object-node Classification (the (system, code) EGRESS, OPTIONAL — None when the row or material is absent), the consumer the catalogue's classification column lacked
                let properties = engineering + lifecycle                                             // the full Seq<MaterialPropertySet> the Material node carries (classification is NOT a property case — it rides the Object node, not the Material node)
                let composition = CompositionAuthor.Single(id)                                       // Construction/assembly#MATERIAL_COMPOSITION (a homogeneous library material; layered/profiled compositions come from the app element design)
                let appearance = MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary).ToOption()  // Appearance/graph#MATERIAL_LIBRARY → Appearance/interchange#MATERIAL_WIRE content-keyed AppearanceSummary
                from bindings in elementOf(id).Match(
                    Some: element => CompositionAuthor.UsageOf(composition, key).Map(usage => Seq1(new MaterialBinding(element, usage, classification))),  // C7: thread the seam MaterialUsage Fin (UsageOf is Fin<MaterialUsage>, not a bare value) AND the resolved Object-node Classification onto the binding the Object-node owner folds into its Classifications set, key-correlated
                    None: () => Fin.Succ(Seq<MaterialBinding>()))                                     // no bound element → the resolved classification has no Object node to carry it (a pure-Materials subgraph run); the egress lands only when the material binds a vouched element
                select source.With(new MaterialSpec(id, composition, properties, appearance, bindings))));

    // --- [PROFILE_FAMILY_CAPTURE]
    // The Capture above is the HOMOGENEOUS-LIBRARY path (a Single composition + the catalogue's published rows). A
    // PROFILE-FAMILY material (a timber member/panel, a glazing IGU) carries a STRUCTURED composition AND properties
    // COMPUTED from its build, not a flat catalogue lookup — so these capture ops lower a resolved family section into a
    // MaterialSpec the SAME Project fold projects. The COMPOSITION SELECTION is the load-bearing wiring the prior page
    // left to "the app": a timber product is a LayerSet (CLT panel) OR a ProfileSet (sawn/glulam/LVL member) by its
    // TimberForm.Shape (MaterialShapeKind), a glazing IGU is ALWAYS a LayerSet — so the projector READS Form.Shape and
    // routes the composition, never forcing one shape. The families OWN the lowering (TimberSection.ToLayerSet/
    // ToProperties, GlazingSection.ToLayerSet/ToProperties); these ops thread it into a spec + the C7 usage + the
    // SustainabilityCatalogue Object-node Classification (the SAME egress the homogeneous Capture threads onto the
    // MaterialBinding, NOT a Material-node field). A profiled timber member's ProfileSet carries the M7 ProfileRef the
    // SAME Project BakeSection path resolves from source.Sections, so the structural runner reads graph.SectionOf(member)
    // without re-resolving. These are the ONLY two family captures: the seam carries exactly two structured composition
    // shapes (LayerSet/ProfileSet) and a glazing IGU is the LayerSet shape, so the SELECTION is the dispatch, never a
    // per-family projector — a cmu/masonry unit stays a Single (the homogeneous Capture).

    // A timber section → its MaterialSpec, the composition ROUTED by Form.Shape: a LayeredPanel (CLT) lowers through
    // TimberSection.ToLayerSet (the seam LayerSet / IfcMaterialLayerSet), a ProfiledMember (sawn/glulam/LVL) through
    // CompositionAuthor.ProfileSet keyed by the catalogue ProfileId (the M7 ProfileRef handle Project bakes the section
    // onto). The properties COMPOSE the seam Orthotropic stiffness (timber#TIMBER_FAMILY ToProperties — the independent-G
    // directional law the seam Orthotropic case carries) with the OPTIONAL Properties/properties#MATERIAL_PROPERTY_CATALOGUE
    // physical rows (λ/SRI/reaction); the catalogue miss folds to the orthotropic set alone (IfFail) — the timber grade
    // DATA is the authority, the catalogue the physical supplement a bespoke grade may lack, never a block on a gap.
    public static Fin<MaterialSpec> CaptureTimber(TimberSection section, ProfileId profile, Func<MaterialId, Option<NodeId>> elementOf, Op key) {
        MaterialId material = MaterialId.Of($"wood.{section.Grade.Key}");
        return (section.Form.Shape == MaterialShapeKind.LayeredPanel
                ? section.ToLayerSet(key)                                                          // CLT panel → seam LayerSet
                : Fin.Succ(CompositionAuthor.ProfileSet(material, profile)))                        // sawn/glulam/LVL member → seam ProfileSet (M7 ProfileRef, baked at Project)
            .Bind(composition =>
                from structural in section.ToProperties(key)                                       // the seam Orthotropic stiffness case (timber grade DATA)
                let physical = MaterialPropertyCatalogue.Lookup(material, key).IfFail(Seq<MaterialPropertySet>())  // OPTIONAL catalogue λ/SRI/reaction supplement
                from classification in SustainabilityCatalogue.Classification(material, key)        // the Object-node Classification egress riding the binding (the SAME shape Capture threads)
                from bindings in elementOf(material).Match(
                    Some: e => CompositionAuthor.UsageOf(composition, key).Map(usage => Seq1(new MaterialBinding(e, usage, classification))),
                    None: () => Fin.Succ(Seq<MaterialBinding>()))
                select new MaterialSpec(material, composition, structural + physical, Option<AppearanceSummary>.None, bindings));
    }

    // A glazing IGU → its MaterialSpec: ALWAYS a LayerSet (pane-cavity-pane, never a profile) via GlazingSection.ToLayerSet,
    // its properties the FULL computed GlazingPerformance set (Thermal Ug / Acoustic Rw / Environmental GWP / Fire) the
    // GlazingSection.ToProperties lowers — an IGU's properties are COMPUTED from its build, NOT a static catalogue row, so
    // this composes ToProperties DIRECTLY and never routes the property catalogue (the prior page omitted both wirings).
    // The designation the glazing catalogue keys IS the IGU MaterialId; the Classification egress rides the binding.
    public static Fin<MaterialSpec> CaptureGlazing(GlazingSection section, ProfileId designation, Func<MaterialId, Option<NodeId>> elementOf, Op key) {
        MaterialId material = MaterialId.Of(designation.Value);
        return from composition in section.ToLayerSet(key)                                          // IGU → seam LayerSet
               from properties in section.ToProperties(key)                                         // the COMPUTED Thermal/Acoustic/Environmental/Fire set
               from classification in SustainabilityCatalogue.Classification(material, key)         // the Object-node Classification egress riding the binding
               from bindings in elementOf(material).Match(
                   Some: e => CompositionAuthor.UsageOf(composition, key).Map(usage => Seq1(new MaterialBinding(e, usage, classification))),
                   None: () => Fin.Succ(Seq<MaterialBinding>()))
               select new MaterialSpec(material, composition, properties, Option<AppearanceSummary>.None, bindings);
    }
}
```

## [04]-[RESEARCH]

- [SEAM_PROJECTION_CONTRACT]: the seam owns ONE instance interface `IElementProjection` with `Fin<GraphDelta> Project(ProjectionContext ctx)`; `MaterialProjector` is one concrete implementation capturing its foreign source internally, `Rasm.Bim`'s `SemanticProjector` the other, a future `Rasm.Fabrication` projector the third — each a one-row registration the seam's `Assemble(projectors, constraints, seed, ctx)` folds, no seam edit per projector. The `ProjectionContext` carries `Op Key`, the target `Header`, `ElementIds: FrozenSet<NodeId>`, and the neutral runtime primitives (instant/correlation/tenant) — the element identities the run vouches for, so an aspect projector binds to them but never invents them (§4C). IFC egress (`Emit`) is a `Rasm.Bim`-INTERNAL op, never a seam member, and `Rasm.Materials` authors NO `Emit` — the projected `Material`/`Appearance` nodes ARE the wire `Rasm.Bim` reads. SEAM CONTRACT (Rasm.Element side; this folder consumes, does not author): `ProjectionContext` (`ElementIds`/`Header`/`Key`), `GraphDelta.Empty`/`Put`/`Link`, `Node.Material`/`Appearance` constructors + the `Projection/address#CANONICAL_WRITER` `Node.ToCanonicalBytes(tolerance)`, `Relationship.Associate`, `MaterialUsage` (`LayerSet`/`ProfileSet`/`None`), the `Graph/element#NODE_MODEL` `NodeId.Content`, `SectionProperties` + `MaterialComposition.WithSection`.
- [H12_OCCURRENCE_EDGE_OWNERSHIP]: the projector owning BOTH endpoints authors the `Associate` (material) edge — when `Rasm.Materials` is given a non-empty `ctx.ElementIds` it authors element→material edges, so a Materials+app composition has a NAMED edge owner and "the app authors it at the wire" is removed. The `MaterialBinding` carries the element NodeId (vouched by the context) and the `[C7]` `MaterialUsage` the `Construction/assembly#MATERIAL_COMPOSITION` author produced; a binding to an element not in `ctx.ElementIds` rails `ProjectionFault.Unvouched` rather than minting an `Object`. A spec carrying NO bindings emits only the material subgraph (no `Associate` edge), so `Rasm.Materials` stays fully usable in ISOLATION (a material subgraph with content-keyed nodes) yet ALIGNED-not-coupled (the same `MaterialComposition`/`MaterialPropertySet`/`MaterialUsage` seam vocabulary `Rasm.Bim` reads); a spec carrying bindings is vouched per-binding, so an empty context with bound specs rails `Unvouched` — the isolation path is "author no bindings", never "drop the bound edges silently".
- [C7_OCCURRENCE_USAGE]: the type-level `MaterialComposition` Set (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`) is the material's buildup; the per-occurrence binding is the `Associate` edge's `MaterialUsage` — `LayerSetUsage` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`) for a layered element and `ProfileSetUsage` (`CardinalPoint`/`ReferenceExtent`) for a profiled member — authored by `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.UsageOf` and passed through the projector unread (the usage is opaque to the projector; only the assembly author mints it and `Rasm.Bim`'s emitter interprets it onto `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`). The `ProfileSet(ProfileRef)` case carries the `[M7]` one-hop section handle: a structural consumer resolves the `ProfileRef` once through `Profiles/profile#PROFILE_RESOLUTION` to the `ComputedSection`, never re-resolving per call.
- [MATERIAL_CLASSIFICATION_EGRESS]: a material's standard `Classification` (Uniclass/OmniClass) reaches IFC through the bound element's `Object` node, NOT through the `Material` node and NOT through an edge — the seam home `Rasm.Element/Classification/classification#CLASSIFICATION_AXIS` is explicit ("an `Object` node carries" the primary `Classification` plus the `Classifications` `Seq<Classification>`), `Rasm.Element/Relations/relation#EDGE_ALGEBRA` is explicit ("classification is a generic value ON the `Object` node, NOT an edge … the seam carries no classification-association relationship"), and the seam `Node.Material` carries `MaterialKey`/`Composition`/`Properties` only (a Classification field on it would fork the material content key on a non-physics annotation and re-open the retired material-level classification wire). So `MaterialSubgraph.Capture` composes `Properties/sustainability#SUSTAINABILITY_PROPERTY` `SustainabilityCatalogue.Classification(id, key)` — the `(system, code)` EGRESS lifting through the seam `Classification.Of`, the CONSUMER the catalogue's classification column otherwise lacked (the column was minted but never read, dying on the row) — and threads the resolved `Option<Classification>` onto the `MaterialBinding`; the Object-node owner (`Rasm.Bim` at IFC ingest accumulating onto the Object node's `Classifications` set via `Semantics/classification#CLASSIFICATION_AXIS` `Ingest`, or the from-scratch app authoring the Object) folds `binding.Classification` into that set, the SAME set `Rasm.Bim`'s `ReauthorClassifications` re-emits onto `IfcRelAssociatesClassification`/`IfcClassificationReference` at `Emit`. The `MaterialProjector` never authors the foreign `Object` node ([H12]) — it carries the resolved value on the binding for the node's owner, so the material's classification round-trips to IFC through the element-Object the seam designates, the migration `Rasm.Materials` `MaterialPropertyWire.Classification` material-level half retired into this element-Object egress. A pure-Materials run (no bound element) carries no Object node, so the resolved classification simply has nowhere to land — the egress is occurrence-scoped, never a dangling material-level association.
- [H7_CONTENT_ADDRESSING]: every authored node is minted via the seam `Graph/element#NODE_MODEL` `NodeId.Content` over the node's own `ToCanonicalBytes(tolerance)` — the `Rasm.Element/Projection/address#CANONICAL_WRITER` canonical value codec (fixed IEEE-754 LE bits, measure quantization to `Header.Tolerance`, explicit attribute order) the seam exposes as an instance member on the `Node` union, composing the `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress` over the kernel seed-zero content hash (§4E — the seam composes the one hasher; this folder mints NO second hash). The `Material`/`Appearance` nodes are non-rooted content-addressed identities (§4B — content-hash for `IfcMaterial`/representation-class nodes), so the same material projected by two specs collapses to one node and the `GraphDelta` is idempotent under re-projection (minting is total — the id derives from the id-excluded canonical bytes, no collision rail). The `Appearance` node shares the content-key discipline with the GLB/representation seam so appearance and geometry both key by one content-hash seed across the C#/Python/TypeScript wire.
- [ASSESSMENT_OWNERSHIP]: `Rasm.Materials` authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the assessment input. `Rasm.Compute` reads the `Material` node plies DIRECTLY (above the seam, `id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the hand-rolled closed-form ISO 6946 U / ISO 12354 STC / EN 15978 LCA / the VividOrange or FE structural solvers + the relocated multi-ply `AssemblyAggregator`), and writes the seam `Assessment` `Result` node (`AssessmentPayload`, `AssessmentOutcome.Computed`) back content-keyed on `(input key, route)`. The multi-ply `AssemblyAggregator` (series-resistance U-value, layered-STC, rule-of-mixtures, GWP/cost folds) is `Rasm.Compute`'s — the seam carries the per-material `MaterialPropertySet`, never the assembly aggregation, so Materials hands the aggregator to Compute and keeps only the single-material property source and the projection. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` (the relocated multi-ply folds reading the seam `MaterialComposition` plies) + `Analysis/assessment` (the discipline solvers writing `Assessment` `Result` nodes).
- [IGRAPH_CONSTRAINT_GATE]: the projected `GraphDelta` is gated by the seam's second interface `IGraphConstraint.Validate(GraphDelta, ElementGraph) → Validation<Error,Unit>` ([M3], `Rasm.Bim`-implemented with IFC-semantic legality) before the seam folds it into the `ElementGraph`; the projector enforces only the STRUCTURAL invariants it owns (content-key idempotence, context-vouched `Associate` endpoints) and defers IFC legality (an `AssociatesMaterial` must target a real element, a layer-set direction must match the element axis) to the constraint, so the projector never re-implements IFC validation and the two interfaces stay orthogonal.
