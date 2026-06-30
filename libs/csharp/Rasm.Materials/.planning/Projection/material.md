# [MATERIALS_PROJECTION]

THE MATERIAL PROJECTOR and THE MATERIAL-SUBGRAPH AUTHOR. `Rasm.Materials` is a PROJECTOR onto the shared `Rasm.Element` seam: one `MaterialProjector` implements the seam's single `IElementProjection` contract with one polymorphic `Fin<GraphDelta> Project(ProjectionContext ctx)` op, capturing every Materials catalogue internally (the `Appearance/graph#MATERIAL_LIBRARY` appearance rows, the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` typed engineering rows, the `Construction/assembly#MATERIAL_COMPOSITION` layer/profile/constituent buildup, the `Profiles/profile#PROFILE_RESOLUTION` section data) and lowering it into ONE additive `GraphDelta` of seam `Node`s and seam `Relationship`s the seam's generic `Assemble` fold merges with every sibling projector's delta. The projector authors the MATERIAL SUBGRAPH only — a content-addressed `Material` node per material (carrying the seam `MaterialComposition` and the `Discipline`-keyed seam `MaterialPropertySet` the Materials engineering rows lower into), a content-keyed `Appearance` node per resolved appearance (the neutral `AppearanceSummary` — PBR scalars + the content key of the full OpenPBR/MaterialX payload `Appearance/interchange#MATERIAL_WIRE` mints), and (for a profiled member) the M7 neutral `SectionProperties` BAKED onto the `ProfileSet` composition (resolved ONE-HOP from the `ProfileRef`). It authors NO assessment-input node: `Rasm.Compute` reads the projected `Material` node plies DIRECTLY (above the seam, no `IElementProjection`) and writes its `Assessment` `Result` node back, so the material's own `Discipline`-keyed `MaterialPropertySet` set IS the input — never a parallel input node. It NEVER mints an element identity: a `Material`/`Appearance` node is keyed by the seam `ContentAddress` over the node's own `ToCanonicalBytes` (the kernel seed-zero `XxHash128` the seam composes, never a second hasher), so the same material projected twice is one node, and the element→material (and element→appearance) `Associate` edge is authored ONLY when the `ProjectionContext` vouches for the element NodeId (a non-empty `ctx.ElementIds` per [H12]), the occurrence binding riding the seam `MaterialUsage` (`LayerSet`/`ProfileSet`/`None` per [C7]) the `Construction/assembly#MATERIAL_COMPOSITION` author produces — a binding to an element the context does not vouch for rails `ProjectionFault.Unvouched`, never an invented node. A pure-Materials composition (empty `ctx.ElementIds`) thus emits a complete material subgraph with NO dangling element edge; a Materials+app or Materials+Bim composition (non-empty `ctx.ElementIds`) additionally names the edge owner. The page composes the `Rasm.Element` seam contracts (`IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`AppearanceSummary`), the Materials catalogues, and the `MaterialFault`/`ProjectionFault` rails; it re-mints NO seam type and re-implements NO host or IFC author — the seam graph IS the wire, and `Rasm.Bim` reads the projected nodes one-hop, never a parallel Materials→IFC carrier.

## [01]-[INDEX]

- [01]-[MATERIAL_PROJECTOR]: the `MaterialProjector` `IElementProjection` owner, the `MaterialProjectionSource`/`MaterialSpec`/`MaterialBinding` captured-source records (with the M7 `ProfileRef`→section table), the `Project` fold producing the seam `GraphDelta`, the `ProjectionFault` band-2470 rail, and the content-addressed `NodeId.Content` minting that composes the seam `ContentAddress`.
- [02]-[MATERIAL_SUBGRAPH]: the `Material`/`Appearance` node authoring (the engineering-property lowering into the seam `Discipline`-keyed `MaterialPropertySet`, the appearance content-keying into the seam `AppearanceSummary`, the M7 `SectionProperties` bake onto a `ProfileSet` via `WithSection`), and the [H12]/[C7] element→material/appearance `Associate` edge with its seam `MaterialUsage` occurrence payload.

## [02]-[MATERIAL_PROJECTOR]

- Owner: `MaterialProjector` the sealed `IElementProjection`; `MaterialProjectionSource` the captured-source aggregate (specs + the M7 `ProfileRef`→section table); `MaterialSpec` the one-material projection unit; `MaterialBinding` the element-occurrence binding (seam `MaterialUsage`); `ProjectionFault` `[Union]` band 2470; the `Mint` content-id helper composing the seam `NodeId.Content`.
- Cases: one `MaterialProjector` shape over a `Seq<MaterialSpec>` — each `MaterialSpec` is a `MaterialId` + a `Construction/assembly#MATERIAL_COMPOSITION` seam `MaterialComposition` + the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` `Discipline`-keyed seam `MaterialPropertySet` set + an optional `Appearance/graph#MATERIAL_LIBRARY` appearance row + a `Seq<Discipline>` assessment-request set + a `Seq<MaterialBinding>` element-occurrence set; the projector is NEVER a per-discipline or per-family projector type, never a `SteelProjector`/`AppearanceProjector` sibling — one projector folds the whole material subgraph.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam contract op, an immutable `Fold` over `source.Specs` accumulating each spec's nodes-and-edges into the running `GraphDelta` (seeded `GraphDelta.Empty`), `Fin<T>` aborting on a content-key collision under divergent content (`ProjectionFault.Collision`), an unresolvable `ProfileRef` (`ProjectionFault.Unresolved` lifting the `Profiles/profile#PROFILE_RESOLUTION` fault), or an `Associate` binding to an element the context does not vouch for (`ProjectionFault.Unvouched`); `MaterialProjector.Of(MaterialProjectionSource source)` captures the source once, and the seam's `Assemble(projectors, constraints, seed, ctx)` folds this projector's delta with `Rasm.Bim`'s `SemanticProjector` delta (and any sibling) into one `ElementGraph` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`Relationship`/`MaterialComposition`/`MaterialPropertySet`/`Discipline`/`ContentAddress`, the seam this folder projects onto), Rasm (project — `Op`/`Context`/the seed-zero `XxHash128` content seed the seam `ContentAddress` composes), Thinktecture.Runtime.Extensions (`[Union]` for `ProjectionFault`), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Bind`/`Option` for the projection rail).
- Growth: a new projected node kind is one arm on `ProjectMaterial` authoring its seam `Node` case (the seam owns the `Node` union; a kind the seam does not carry is a seam growth, never a Materials parallel node); a new assessment discipline is one `Discipline` row the `Assessment` input fold already dispatches over (no new arm); a new occurrence-usage shape is one seam `MaterialUsage` case the `[C7]` author already produces — the projector reads it through the seam's total `Switch`; a new projection fault is one `ProjectionFault` case. There is NO second projector surface, NO per-family projector, and NO Materials→IFC carrier beside the graph — growth is a spec row or a seam case the one `Project` fold absorbs.
- Boundary: `MaterialProjector` captures its foreign source INTERNALLY (§4C inversion) — the four Materials catalogues are private fields the projector reads, never `ProjectionContext` parameters, so a consumer constructs the projector from a `MaterialProjectionSource` and calls the seam op with zero knowledge of the Materials catalogue shapes; the projector authors the MATERIAL SUBGRAPH only and emits NO `Object` node and NO element-level `PropertySet`/`QuantitySet` bag (those are `Rasm.Bim`'s at IFC ingress) — it lowers the engineering rows into the `Material` node's own `Discipline`-keyed `MaterialPropertySet`, never an element Pset; every authored node is CONTENT-ADDRESSED through the projector's `Mint` helper which composes the seam `NodeId.Content(node.ToCanonicalBytes(tolerance))` ([H7] canonical bytes, the kernel seed-zero content hash, NO second hasher in this folder), so a material/appearance projected by two specs collapses to one node and the delta is idempotent under re-projection; the element→material (and element→appearance) `Associate` edge is authored ONLY for a binding whose `Element` is in `ctx.ElementIds` ([H12] — the projector never invents an element identity), the occurrence payload the seam `MaterialUsage` (`LayerSet`/`ProfileSet`/`None`) the `Construction/assembly#MATERIAL_COMPOSITION` author produced, so a pure-Materials run (`ctx.ElementIds` empty) emits the subgraph with no dangling edge and a Materials+app/Materials+Bim run names the edge owner; a binding to an unvouched element rails `ProjectionFault.Unvouched`, never a fabricated `Object`; the `Project` rail is `Fin<GraphDelta>` so the seam `Assemble` fold short-circuits a failed projection without a partial graph, and `ProjectionFault` (band 2470) is disjoint from `ProfileFault` 2300 / `ConstructionFault` 2350 / `ConnectionFault` 2360 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 yet lifts each owner's fault unchanged through `Fin` so a `Profiles/profile#PROFILE_RESOLUTION` or `MaterialFault.Parameter` propagates without re-wrapping.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element;                  // NodeId, Node, MaterialId, MaterialComposition, MaterialPropertySet, SectionProperties,
                                     // AppearanceSummary, Relationship, MaterialUsage, GraphDelta, ProfileRef, ResolvedProfile,
                                     // IElementProjection, ProjectionContext, ProjectionAssembly
using Thinktecture;
using static LanguageExt.Prelude;

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ProjectionFault : Expected, IValidationError<ProjectionFault> {
    private ProjectionFault(Op key, string detail) : base(detail, 2470, None) => Key = key;
    public Op Key { get; }
    public static ProjectionFault Create(string message) => new Source(default, message);
    public sealed record Source(Op Key, string Detail)     : ProjectionFault(Key, Detail) { public override string Category => "Source"; }
    public sealed record Unvouched(Op Key, string Detail)  : ProjectionFault(Key, Detail) { public override string Category => "Unvouched"; }
    public sealed record Unresolved(Op Key, string Detail) : ProjectionFault(Key, Detail) { public override string Category => "Unresolved"; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The element-occurrence binding the app or Rasm.Bim supplies: which VOUCHED seam element NodeId this material
// binds, and the seam MaterialUsage (C7) occurrence usage (LayerSet direction/sense/offset, or ProfileSet
// cardinal-point/extent) the Construction/assembly#MATERIAL_COMPOSITION author produced. The Element MUST be
// vouched by ctx.ElementIds — the projector never mints an element identity (§4C/H12).
public readonly record struct MaterialBinding(NodeId Element, MaterialUsage Usage);

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

    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        source.Specs.Fold(
            Fin.Succ(GraphDelta.Empty),
            (acc, spec) => acc.Bind(delta => ProjectMaterial(spec, ctx, delta)));

    // --- [SUBGRAPH_FOLD]
    // One spec → a content-addressed Material node (with the M7 section baked onto a ProfileSet), an optional
    // content-keyed Appearance node, and the vouched element→material / element→appearance Associate edges. Each
    // node id is minted through the seam content address (NodeId.Content over the id-excluded canonical bytes),
    // so this folder owns NO hasher and a material/appearance projected twice collapses to one node (idempotent delta).
    Fin<GraphDelta> ProjectMaterial(MaterialSpec spec, ProjectionContext ctx, GraphDelta delta) {
        double tolerance = ctx.Header.Tolerance;
        Node.Material material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, BakeSection(spec.Composition), spec.Properties), tolerance);
        Option<Node.Appearance> appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance));
        GraphDelta withNodes = appearance.Match(Some: a => delta.Put(material).Put(a), None: () => delta.Put(material));
        return AuthorBindings(spec, material.Id, appearance.Map(static a => a.Id), ctx, withNodes);
    }

    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured Profiles resolution table and BAKE the
    // neutral seam SectionProperties onto the composition (WithSection), so the structural runner reads
    // graph.SectionOf(member) without re-resolving or admitting VividOrange; a non-ProfileSet or unresolved ref bakes nothing.
    MaterialComposition BakeSection(MaterialComposition composition) =>
        composition is MaterialComposition.ProfileSet ps && source.Sections.TryGetValue(ps.Profile, out ResolvedProfile resolved)
            ? composition.WithSection(SeamSection(resolved.Section))
            : composition;

    // The neutral seam SectionProperties lifted from the Profiles ComputedSection — the full 16-column structural set in
    // the SEAM's declared order, each a typed MeasureValue converted from the SI-millimetre ComputedSection magnitude to
    // SI base (mm->m, mm2->m2, mm3->m3, mm4->m4). The section moduli stamp a "SectionModulus" QuantityType over VolumeDim
    // and the second moments / torsion a "SecondMomentOfArea"/"TorsionConstant" type over the L^4 Dimension, so neither
    // reads as a Volume; the areas stamp the Area row, the lengths the Length row. The seam graph carries the baked
    // scalars and Rasm.Compute reads graph.SectionOf(member) without re-resolving or admitting VividOrange.
    static SectionProperties SeamSection(ComputedSection c) {
        static MeasureValue Len(double mm)      => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, mm * 1e-3);
        static MeasureValue Area(double mm2)    => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, mm2 * 1e-6);
        static MeasureValue Modulus(double mm3) => MeasureValue.OfSi(QuantityType.Create("SectionModulus"), Dimension.VolumeDim, mm3 * 1e-9);
        static MeasureValue Inertia(double mm4) => MeasureValue.OfSi(QuantityType.Create("SecondMomentOfArea"), Dimension.Create(4, 0, 0, 0, 0, 0, 0), mm4 * 1e-12);
        static MeasureValue Torsion(double mm4) => MeasureValue.OfSi(QuantityType.Create("TorsionConstant"), Dimension.Create(4, 0, 0, 0, 0, 0, 0), mm4 * 1e-12);
        return new(
            Area(c.AreaMm2.Value), Inertia(c.IxMm4.Value), Inertia(c.IyMm4.Value), Torsion(c.JMm4.Value),
            Modulus(c.SxMm3.Value), Modulus(c.SyMm3.Value), Modulus(c.ZxMm3.Value), Modulus(c.ZyMm3.Value),
            Area(c.AvyMm2.Value), Area(c.AvzMm2.Value), Len(c.RxMm.Value), Len(c.RyMm.Value),
            Len(c.DepthMm.Value), Len(c.WidthMm.Value), Len(c.HeatedPerimeterMm.Value), Len(c.AxisDistanceMm));
    }

    // Content-id mint — composes the seam NodeId.Content over the node's OWN canonical bytes (H7, the kernel
    // seed-zero XxHash128 the seam ContentAddress wraps; this folder owns no hasher). ToCanonicalBytes EXCLUDES
    // the id, so the placeholder id the draft carries never affects the minted identity.
    static Node.Material Mint(Node.Material draft, double tolerance) => draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };
    static Node.Appearance Mint(Node.Appearance draft, double tolerance) => draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };

    // --- [OCCURRENCE_EDGES]
    // H12: author element→material (and element→appearance) Associate edges ONLY for bindings whose Element is
    // vouched by ctx.ElementIds. An empty ctx.ElementIds (pure-Materials run) authors NO edge and is not a fault —
    // the material subgraph stands alone (usable in isolation); a binding to an unvouched element rails Unvouched,
    // never an invented Object node (§4C). The appearance binds the element directly (the seam attaches Appearance
    // to the Object with MaterialUsage.None); the material→appearance default pairing lives in the catalogue, not an edge.
    static Fin<GraphDelta> AuthorBindings(MaterialSpec spec, NodeId materialId, Option<NodeId> appearanceId, ProjectionContext ctx, GraphDelta delta) =>
        ctx.ElementIds.IsEmpty
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
- Entry: `Fin<GraphDelta> ProjectMaterial(MaterialSpec spec, ProjectionContext ctx, GraphDelta delta)` mints the content-addressed `Material` node (with the M7 section baked), the optional `Appearance` node, then folds the vouched bindings over the running delta; `MaterialWire.Summary(MaterialParameters parameters)` (on `Appearance/interchange`) lowers the resolved appearance to the content-keyed seam `AppearanceSummary`, composed here, never re-derived; `Rasm.Compute` reads the projected `Material` node plies directly, so there is no in-folder assessment-input marshaller.
- Packages: Rasm.Element (project — `Node` cases, `MaterialPropertySet`, `Relationship.Compose`/`Assign`/`Associate`, `MaterialUsage`, `Discipline`, `ContentAddress`), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Option` for the node folds), Thinktecture.Runtime.Extensions (the seam unions' generated `Switch` the usage dispatch reads).
- Growth: a new engineering discipline routed to the material is one `Discipline` row the seam `MaterialPropertySet` carries (Compute's route dispatch reads it off the `Material` node) — no projector arm; a new appearance payload channel is one column on the seam `AppearanceSummary` the `MaterialWire` lowering fills; a new occurrence-usage shape is one seam `MaterialUsage` case the `Construction/assembly#MATERIAL_COMPOSITION` author produces and the projector passes through unread (the usage is opaque to the projector — only the assembly author and `Rasm.Bim`'s emitter interpret it). The subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: the `Material` node carries its OWN `MaterialPropertySet` (the material physics keyed by `Discipline`) and NEVER an element-level `PropertySet`/`QuantitySet` bag — those are `Rasm.Bim`'s at IFC ingress, so the projector authors no element Pset and the engineering rows lower into the material node's intrinsic property set exactly as the task routes the `MaterialProperty` unions; the seam `MaterialPropertySet.Acoustic` case carries the intrinsic pure folds (`Nrc`/`Saa`/`StcWeighted`/`StcContourFit`, [Rasm.Element]`Composition/acoustic`) so a Materials consumer reading an acoustic rating reads the SEAM fold, never a re-authored one here; the `Appearance` node is content-keyed on the `AppearanceSummary` (PBR scalars + the content key of the OpenPBR/MaterialX payload) so two materials with identical appearance share one node and the GLB/render seam keys appearance by the same content hash the geometry seam uses; the projector authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the `Material` node IS the input `Rasm.Compute` reads DIRECTLY (above the seam) to run the closed-form/solver route and write its seam `Assessment` `Result` node content-keyed on `(input key, route)` — the multi-ply `AssemblyAggregator` is `Rasm.Compute`'s (the seam carries the per-material property set, never the assembly fold); the `Associate` edge's `Subject` is always a context-vouched element NodeId and its `Resource` the content-keyed material (or appearance) node, so the edge is directional element→resource and the occurrence usage rides the edge not the type-level composition ([C7] — the `MaterialComposition` Set is the material's type-level buildup, the seam `MaterialUsage` the per-occurrence binding); the whole subgraph is one additive `GraphDelta` the seam's `IGraphConstraint.Validate` ([M3], `Rasm.Bim`-implemented) gates for IFC-semantic legality before the seam folds it, so the projector enforces only the structural invariants (content-key idempotence, context-vouched edges) and defers IFC legality to the seam constraint.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// A worked composition root: capture the catalogues into specs, build the M7 ProfileRef→section resolution table
// once (Profiles/profile#PROFILE_RESOLUTION), then project against a context. The element NodeIds in ctx.ElementIds
// come from Rasm.Bim (IFC ingress) or the app (from-scratch authoring, H6 kernel-minted); Materials binds to them
// via the seam MaterialUsage the assembly author built, never invents them. Rasm.Compute reads the projected
// Material node plies DIRECTLY and writes the Assessment result back — Materials authors no assessment-input node
// (the material's own Discipline-keyed MaterialPropertySet set IS the input Compute consumes).
public static class MaterialSubgraph {
    public static Fin<MaterialProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Option<NodeId>> elementOf,
        FrozenDictionary<ProfileRef, ResolvedProfile> sections,
        Context context, Op key) =>
        materials.Fold(
            Fin.Succ(MaterialProjectionSource.Empty with { Sections = sections }),
            (acc, id) => acc.Bind(source =>
                from engineering in MaterialPropertyCatalogue.Lookup(id, key)                        // Properties/properties#MATERIAL_PROPERTY_CATALOGUE → seam Mechanical/Thermal/Acoustic/Fire cases
                from lifecycle in SustainabilityCatalogue.Lookup(id, key)                            // Properties/sustainability#SUSTAINABILITY_PROPERTY → seam Environmental/Cost cases
                let properties = engineering + lifecycle                                             // the full Seq<MaterialPropertySet> the Material node carries
                let composition = CompositionAuthor.Single(id)                                       // Construction/assembly#MATERIAL_COMPOSITION (a homogeneous library material; layered/profiled compositions come from the app element design)
                let appearance = MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary).ToOption()  // Appearance/graph#MATERIAL_LIBRARY → Appearance/interchange#MATERIAL_WIRE content-keyed AppearanceSummary
                let bindings = elementOf(id).Match(
                    Some: element => Seq1(new MaterialBinding(element, CompositionAuthor.UsageOf(composition))),  // C7 occurrence usage (seam MaterialUsage) from the composition shape
                    None: () => Seq<MaterialBinding>())
                select source.With(new MaterialSpec(id, composition, properties, appearance, bindings))));
}
```

## [04]-[RESEARCH]

- [SEAM_PROJECTION_CONTRACT]: the seam owns ONE instance interface `IElementProjection` with `Fin<GraphDelta> Project(ProjectionContext ctx)`; `MaterialProjector` is one concrete implementation capturing its foreign source internally, `Rasm.Bim`'s `SemanticProjector` the other, a future `Rasm.Fabrication` projector the third — each a one-row registration the seam's `Assemble(projectors, constraints, seed, ctx)` folds, no seam edit per projector. The `ProjectionContext` carries `Op Key`, the target `Header`, `ElementIds: FrozenSet<NodeId>`, and the neutral runtime primitives (instant/correlation/tenant) — the element identities the run vouches for, so an aspect projector binds to them but never invents them (§4C). IFC egress (`Emit`) is a `Rasm.Bim`-INTERNAL op, never a seam member, and `Rasm.Materials` authors NO `Emit` — the projected `Material`/`Appearance` nodes ARE the wire `Rasm.Bim` reads. SEAM CONTRACT (Rasm.Element side; this folder consumes, does not author): `ProjectionContext` (`ElementIds`/`Header`/`Key`), `GraphDelta.Empty`/`Put`/`Link`, `Node.Material`/`Appearance` constructors + `Node.ToCanonicalBytes(tolerance)`, `Relationship.Associate`, `MaterialUsage` (`LayerSet`/`ProfileSet`/`None`), `NodeId.Content`, `SectionProperties` + `MaterialComposition.WithSection`.
- [H12_OCCURRENCE_EDGE_OWNERSHIP]: the projector owning BOTH endpoints authors the `Associate` (material) edge — when `Rasm.Materials` is given a non-empty `ctx.ElementIds` it authors element→material edges, so a Materials+app composition has a NAMED edge owner and "the app authors it at the wire" is removed. The `MaterialBinding` carries the element NodeId (vouched by the context) and the `[C7]` `MaterialUsage` the `Construction/assembly#MATERIAL_COMPOSITION` author produced; a binding to an element not in `ctx.ElementIds` rails `ProjectionFault.Unvouched` rather than minting an `Object`. When `ctx.ElementIds` is empty the projector emits only the material subgraph (no `Associate` edge), so `Rasm.Materials` stays fully usable in ISOLATION (a material subgraph with content-keyed nodes) yet ALIGNED-not-coupled (the same `MaterialComposition`/`MaterialPropertySet`/`MaterialUsage` seam vocabulary `Rasm.Bim` reads).
- [C7_OCCURRENCE_USAGE]: the type-level `MaterialComposition` Set (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`) is the material's buildup; the per-occurrence binding is the `Associate` edge's `MaterialUsage` — `LayerSetUsage` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`) for a layered element and `ProfileSetUsage` (`CardinalPoint`/`ReferenceExtent`) for a profiled member — authored by `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.UsageOf` and passed through the projector unread (the usage is opaque to the projector; only the assembly author mints it and `Rasm.Bim`'s emitter interprets it onto `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`). The `ProfileSet(ProfileRef)` case carries the `[M7]` one-hop section handle: a structural consumer resolves the `ProfileRef` once through `Profiles/profile#PROFILE_RESOLUTION` to the `ComputedSection`, never re-resolving per call.
- [H7_CONTENT_ADDRESSING]: every authored node is minted via the seam `NodeId.Content` over the node's own `ToCanonicalBytes(tolerance)` — the canonical value codec (fixed IEEE-754 LE bits, measure quantization to `Header.Tolerance`, explicit attribute order) the seam exposes as an instance member on the `Node` union, composing the kernel seed-zero content hash (§4E — the seam composes the one hasher; this folder mints NO second hash). The `Material`/`Appearance` nodes are non-rooted content-addressed identities (§4B — content-hash for `IfcMaterial`/representation-class nodes), so the same material projected by two specs collapses to one node and the `GraphDelta` is idempotent under re-projection (minting is total — the id derives from the id-excluded canonical bytes, no collision rail). The `Appearance` node shares the content-key discipline with the GLB/representation seam so appearance and geometry both key by one content-hash seed across the C#/Python/TypeScript wire.
- [ASSESSMENT_OWNERSHIP]: `Rasm.Materials` authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the assessment input. `Rasm.Compute` reads the `Material` node plies DIRECTLY (above the seam, `id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route (the hand-rolled closed-form ISO 6946 U / ISO 12354 STC / EN 15978 LCA / the VividOrange or FE structural solvers + the relocated multi-ply `AssemblyAggregator`), and writes the seam `Assessment` `Result` node (`AssessmentPayload`, `AssessmentOutcome.Computed`) back content-keyed on `(input key, route)`. The multi-ply `AssemblyAggregator` (series-resistance U-value, layered-STC, rule-of-mixtures, GWP/cost folds) is `Rasm.Compute`'s — the seam carries the per-material `MaterialPropertySet`, never the assembly aggregation, so Materials hands the aggregator to Compute and keeps only the single-material property source and the projection. Ripple counterpart: `Rasm.Compute` `Analysis/aggregator` (the relocated multi-ply folds reading the seam `MaterialComposition` plies) + `Analysis/assessment` (the discipline solvers writing `Assessment` `Result` nodes).
- [IGRAPH_CONSTRAINT_GATE]: the projected `GraphDelta` is gated by the seam's second interface `IGraphConstraint.Validate(GraphDelta, ElementGraph) → Validation<Error,Unit>` ([M3], `Rasm.Bim`-implemented with IFC-semantic legality) before the seam folds it into the `ElementGraph`; the projector enforces only the STRUCTURAL invariants it owns (content-key idempotence, context-vouched `Associate` endpoints) and defers IFC legality (an `AssociatesMaterial` must target a real element, a layer-set direction must match the element axis) to the constraint, so the projector never re-implements IFC validation and the two interfaces stay orthogonal.
