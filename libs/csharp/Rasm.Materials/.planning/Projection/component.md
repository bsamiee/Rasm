# [MATERIALS_PROJECTION]

THE COMPONENT PROJECTOR, THE SEAM COMPOSITION AUTHOR, and THE COMPONENT-SUBGRAPH CAPTURE. `ComponentProjector.Project` folds payload-complete `Substance` and `Type` cases onto one `Fin<GraphDelta>` and crosses the `observability#HOOK_RAIL` `ProjectionGate` veto before that delta returns. Every appearance is required by the captured spec, and every `OccurrenceBinding` carries its explicit `MaterialUsage`; layer direction, offset, extent, and profile cardinal placement never derive from type-level composition. Every baked `TextureSet` rides as an ASSOCIATION off the projected appearance node rather than as an `AppearanceSummary` column, so the seam key stays bake-invariant. `CompositionAuthor` builds `MaterialComposition`, and `ComponentSubgraph` selects `ProfileSet`, `LayerSet`, or `Single` from the component row and lowers each family's own physics receipt through one family-keyed table.

## [01]-[INDEX]

- [02]-[COMPONENT_PROJECTOR]: the `ComponentProjector` `IElementProjection` owner, the `ComponentProjectionSource` aggregate, the payload-complete `ComponentProjectionSpec` union, `MaterialBinding`/`OccurrenceBinding`, the `Project` fold with its `ProjectionGate` veto consult, the `ProjectionFault` rail, the content mints, the M7 section bake, the type-level takeoff mint, the baked-set association edge, and the binding vouches.
- [03]-[COMPOSITION_AUTHOR]: the seam-`MaterialComposition` `Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet` builders.
- [04]-[COMPONENT_SUBGRAPH]: the `ComponentSubgraph` capture composition root — the homogeneous-substance `Capture` and the `CaptureComponent` Type capture whose composition selection reads the `ComponentRow` `Sectioned` pin and the `SectionProfile.Layered` arm, beside the family-keyed `Lowerings` physics table.

## [02]-[COMPONENT_PROJECTOR]

- Owner: `ComponentProjector` is the sealed `IElementProjection`; `ComponentProjectionSource` carries the spec stream and section table; `ComponentProjectionSpec` carries each modality's complete payload; `MaterialBinding` carries substance-path usage and classification; `OccurrenceBinding` carries a vouched Type occurrence and its required usage.
- Cases: `Substance(MaterialId, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Option<ContentAddress>, Seq<MaterialBinding>)` and `Type(Component, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Option<ContentAddress>, Option<Classification>, Seq<OccurrenceBinding>)`.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam op: `source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()` — specs are INDEPENDENT, so the fold is APPLICATIVE (the EXACT seam `Assemble` shape, `Traverse`→`ToValidation`→`Merge`-fold per the seam `[APPLICATIVE_CAPTURE]` law — never a hand-threaded accumulator, never a first-fault-only `TraverseM`) — every failing spec reports (an unvouched binding/occurrence `ProjectionFault.Unvouched`, a malformed Type `Classification` the seam `ElementFault` lifts unchanged through `Classification.Of`), the accumulated `ManyErrors` lowering onto ONE `Fin<GraphDelta>` whose success then crosses the `observability#HOOK_RAIL` `ProjectionGate` VETO before it returns; `ProjectSpec` discriminates via the generated total `Switch` — `Substance`→`ProjectSubstance`, `Type`→`ProjectType`; `ComponentProjector.Of(source, hooks)` captures both once, and the seam `Assemble(ProjectionSuite.Of(…), seed, ctx)` re-merges this delta with `Rasm.Bim`'s `SemanticProjector` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — the seam: `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`RepresentationContentHash`/`SchemaSpan`/`OwnerHistory`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`PropertyValue`/`DetailSchema`/`PropertySource`/`ContentAddress`/`MaterialId`, plus `FaultBand` the band registry), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile`/`ComputedSection`/`ResolvedComponent`/`QuantityRow`, the standardized-type owner whose `IfcBinding` forwarders and typed-mint rows this projector reads), Rasm.Materials.Projection (`observability#HOOK_RAIL` — `MaterialsHooks`/`MaterialsFact`, the veto seating this folder's own signal owner declares), Rasm.Domain (project — `Op`; the seed-zero `XxHash128` content seed is the seam `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` + generated total `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`ToValidation`/`ToFin`/`Fold`/`Option`); cite `libs/csharp/.api/api-thinktecture-runtime-extensions.md` — the `Rasm.Materials/.api` VividOrange catalogues are the `component#COMPONENT_OWNER`'s, not composed here (the projector reads an already-resolved `ComputedSection`, never the section solver).
- Growth: a new projected node kind is one seam `Node` case, a new spec modality one `ComponentProjectionSpec` case, a new occurrence-usage shape one seam `MaterialUsage` case carried by `OccurrenceBinding`, a new type-level takeoff quantity one seam `DetailSchema.Takeoff` row with its `TypeTakeoff` mint line, and a new VETO one `MaterialsPoint` row with its seating — never a projector edit, because the consult is over the merged delta this fold already produces.
- Boundary: each Type occurrence is vouched independently and binds through `Assign.TypeDefinition` plus its explicit occurrence-to-material usage. `MaterialLibrary.Lookup(...).Bind(row => MaterialWire.Summary(row, key))` remains required on `Fin` at BOTH hops — the seam factory gates every channel to the unit range and rails on its own key, so the lowering binds rather than maps; no optional appearance state survives inside the spec. `TypeTakeoff` reads the seam-owned row vocabulary and the seam substance-density accessor, deriving no numeric semantics of its own — quantity identity, unit, and dimensional composition stay `Rasm.Element`'s, this projector supplying only the section and substance a running metre is measured from. BAKING NEVER RE-KEYS `AppearanceSummary`: the seam key freezes at the seven neutral PBR values, so the baked set rides as an ASSOCIATION off the projected appearance node — a content-keyed `Node.PropertySet` carrying the set address under the seam-declared row — and re-pressing a material at a higher resolution adds an edge while every node id in the estate stands. Widening the summary instead forks the `Rasm.Bim` dedup key for a field only a texture consumer reads and stops a material deduplicating against its own baked variant. Seed-built detail bags ROUND-TRIP by element genus: a realizing-element family imports through the `Rasm.Bim` connection-detail reader against `DetailSchema.Realization`, panel product detail through the general Bim object/property fold against `DetailSchema.Product` — one bag crosses out and two genus-keyed readers bring it back, never a projector-side import path. `Veto` verdicts enter this fold's OWN rail per the folder ruling, so the gate consult sits after the merge and before the return, where an `Observe` point stays decorator-only and this page names none.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation failure slot the applicative spec/vouch folds accumulate
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element.Classification;   // Classification — the seam standard-reference value
using Rasm.Element.Composition;      // MaterialComposition, MaterialPropertySet, SectionProperties, ProfileRef, MaterialId
using Rasm.Element.Graph;            // GraphDelta, Node, NodeId, ObjectKind, PredefinedType, RepresentationContentHash, SchemaSpan, OwnerHistory, AppearanceSummary
using Rasm.Element.Projection;       // IElementProjection, ProjectionContext, FaultBand, ContentAddress
using Rasm.Element.Properties;       // PropertyBag, PropertyName, PropertyValue, DetailSchema, MeasureValue, QuantityType, PropertySource
using Rasm.Element.Relations;        // Relationship, AssignKind, MaterialUsage
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Interchange;
using Rasm.Materials.Component;      // Component, ComponentRow, ComponentFamily, SectionProfile, ComputedSection,
                                     // ResolvedComponent, QuantityRow, and the per-family lowering owners
                                     // (GlazingSeed/GlazingDetail, MasonrySeed/MasonryRow/MasonryDetail, CmuSeed/CmuRow)
using Rasm.Materials.Properties;     // MaterialPropertyCatalogue, SustainabilityCatalogue
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [ERRORS] ------------------------------------------------------------------------------
// The component-projection band read off the FaultBand registry (a duplicate band integer fails at type init —
// disjointness is type-enforced, never prose). Expected-derived so the band IS the Expected Code and a typed case
// lifts BARE onto Fin<T> (no .ToError() hop); a failed projection assembles NO partial graph — the seam Assemble
// capture accumulates every projector's faults, then the one Fin failure stops the bake. No [GenerateUnionOps] (the
// kernel union-ops generator is strictly opt-in; every case carries an explicit Op, wanting no generated SelfOp);
// [Union] generates Switch/Map, never factories: the nested `…Case` record carries the data, the same-name-less
// static factory returns the base (a same-named nested type + method is CS0102). Create routes the unspecific case
// under a boundary-admission Op.
[Union]
public abstract partial record ProjectionFault : Expected, IValidationError<ProjectionFault> {
    private ProjectionFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Projection;
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
// The element-occurrence material binding the app or Rasm.Bim supplies: which VOUCHED seam element NodeId this
// material binds, the C7 seam MaterialUsage the [03] author derived, and the material's resolved standard
// Classification (an Object-node VALUE — NOT a Node.Material field, NOT an edge payload) the bound element's
// Object node carries. The element MUST be vouched by ctx.Owns — the substance path never mints an element
// identity ([H12]); the Object-node owner (Bim ingest or the from-scratch app) folds the classification into the
// element's Classifications set.
public readonly record struct MaterialBinding(NodeId Element, MaterialUsage Usage, Option<Classification> Classification);

public sealed record MaterialFacts(
    Seq<MaterialPropertySet> Properties,
    Option<Classification> Classification);

public static class MaterialFactsCatalogue {
    public static Fin<MaterialFacts> Lookup(MaterialId id, Op key) =>
        (MaterialPropertyCatalogue.Lookup(id, key), SustainabilityCatalogue.Lookup(id, key), SustainabilityCatalogue.Classification(id, key))
            .Apply(static (engineering, lifecycle, classification) => new MaterialFacts(engineering + lifecycle, classification));
}

public readonly record struct OccurrenceBinding(NodeId Element, MaterialUsage Usage);

// The ONE projection spec the single Project fold discriminates: Substance (the pure-material subgraph, no Object)
// versus Type (a minted Type Object + baked-section material + seed-built detail). One Seq the projector folds —
// the MaterialProjector/ConnectionProjector dual surface collapsed onto one discriminant, never two projectors.
[Union]
public abstract partial record ComponentProjectionSpec {
    private ComponentProjectionSpec() { }

    public sealed record Substance(
        MaterialId Material,
        MaterialComposition Composition,
        Seq<MaterialPropertySet> Properties,
        AppearanceSummary Appearance,
        Option<ContentAddress> TextureSet,               // the press's own set key — an ASSOCIATION off the appearance node, never a summary column, so a bake re-ids nothing
        Seq<MaterialBinding> Bindings) : ComponentProjectionSpec {
        public bool ThinWalled { get; init; }            // the row's double-sided shell fact, bag-bound under DetailSchema.DoubleSided — never a summary column, so sidedness re-ids nothing
    }

    public sealed record Type(
        Component Component,
        MaterialComposition Composition,
        Seq<MaterialPropertySet> Properties,
        AppearanceSummary Appearance,
        Option<ContentAddress> TextureSet,
        Option<Classification> StandardClassification,   // the facts-resolved standard reference the Type Object's Classifications set carries — Type-seed-excluded, so the stamp never re-keys NodeId.RootedType
        Seq<OccurrenceBinding> Occurrences) : ComponentProjectionSpec {
        public bool ThinWalled { get; init; }
    }
}

// The captured projection source carries the closed spec stream and the resolved section table.
public sealed record ComponentProjectionSource(Seq<ComponentProjectionSpec> Specs, FrozenDictionary<ProfileRef, ResolvedComponent> Sections) {
    public static readonly ComponentProjectionSource Empty = new(Seq<ComponentProjectionSpec>(), FrozenDictionary<ProfileRef, ResolvedComponent>.Empty);
    public ComponentProjectionSource Add(ComponentProjectionSpec spec) => this with { Specs = Specs.Add(spec) };
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Materials folder publishes. Captures the source AND the folder's hook roster
// internally (the source-capture inversion) so the seam op carries only the ProjectionContext; the seam Assemble
// merges this delta with every sibling.
public sealed class ComponentProjector : IElementProjection {
    readonly ComponentProjectionSource source;
    readonly MaterialsHooks hooks;
    ComponentProjector(ComponentProjectionSource source, MaterialsHooks hooks) => (this.source, this.hooks) = (source, hooks);
    public static ComponentProjector Of(ComponentProjectionSource source, MaterialsHooks hooks) => new(source, hooks);

    // Traverse each spec to its OWN delta APPLICATIVELY — specs are independent, so every failing spec reports
    // and ONE Fin carries the accumulated ManyErrors (the EXACT seam Assemble shape: Traverse -> ToValidation ->
    // Merge-fold, the seam [APPLICATIVE_CAPTURE] law; TraverseM's first-fault abort is the rejected disposition).
    // Each spec builds on GraphDelta.Empty so per-spec projection is decoupled from the running delta. The merged
    // delta then crosses the VETO: Fire hands back the ADMITTED fact, so a gate may transform or refuse the whole
    // delta pre-merge and the projector returns what the gate admitted rather than what it built. A veto point no
    // fold consults is a modality the roster asserts and the estate cannot honour.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)))
            .ToFin()
            .Bind(delta => hooks.ProjectionGate.Fire(new MaterialsFact.ProjectionGate(delta)))
            .Map(static admitted => admitted.Delta);

    // The ONE discriminator — the substance subgraph (no Object) versus the Type subgraph (a minted Type Object),
    // the generated total Switch over the spec union, never two projector entrypoints.
    Fin<GraphDelta> ProjectSpec(ComponentProjectionSpec spec, ProjectionContext ctx) => spec.Switch(
        substance: s => ProjectSubstance(s, ctx),
        type:      c => ProjectType(c, ctx));

    // --- [SUBSTANCE_FOLD]
    // The pure-material subgraph (the prior MaterialProjector.ProjectMaterial nodes and edges, byte-identical;
    // only the binding vouch below accumulates): a content-addressed
    // Material node (the M7 section baked onto a ProfileSet), an optional content-keyed Appearance node, and the
    // vouched element→material / element→appearance Associate edges — built on GraphDelta.Empty. Each id is minted
    // through the seam content address, so two specs projecting the same material mint ONE id and the duplicate
    // add collapses at the seam WorkingGraph.Set upsert when AdmitOnto folds the merged delta.
    Fin<GraphDelta> ProjectSubstance(ComponentProjectionSpec.Substance spec, ProjectionContext ctx) =>
        from baked in BakeSection(spec.Composition, ctx.Key)
        let tolerance = ctx.Header.Tolerance
        let composition = baked.Composition
        let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, composition, spec.Properties), tolerance)
        let appearance = Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Appearance), tolerance)
        let withNodes = GraphDelta.Empty.Put(material).Put(appearance)
        from addressed in BindTextureSet(withNodes, spec.TextureSet, spec.ThinWalled, appearance.Id, tolerance, ctx.Key)
        from bound in AuthorBindings(spec, material.Id, appearance.Id, ctx, addressed)
        select bound;

    // --- [TYPE_FOLD]
    // The standardized-Component subgraph: MINT the deterministic-rooted Type Object, lower the structural
    // Material node (the SubstanceId material with the M7 section baked on), the required Appearance, and the
    // SEED-BUILT detail bag read STRAIGHT off c.Detail (each family's bag is built at seed time by the relocated
    // component#COMPONENT_DETAIL ComponentDetail owner, and Component.Of's lane/detail type law now carries the
    // deleted Detail(Component) switch's totality — a None-lane family carries no bag, a Realization/Product
    // family always does), wire the Type→resource edges (both endpoints owned — no vouch), then bind every VOUCHED occurrence via
    // Assign.TypeDefinition PLUS its own occurrence→material Associate carrying the binding's OWN explicit
    // MaterialUsage ([OCCURRENCE_USAGE_RULING] — usage is input data on each OccurrenceBinding, never derived from the type-level
    // composition). Classification.Of is the seam Fin admission; AuthorOccurrences the fallible tail.
    Fin<GraphDelta> ProjectType(ComponentProjectionSpec.Type spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Component c = spec.Component;
        return
            from classification in Classification.Of("ifc", c.IfcEntity, ctx.Key)
            from baked in BakeSection(spec.Composition, ctx.Key)
            let type = MintType(c, classification, spec.StandardClassification, ctx)
            let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), c.SubstanceId, baked.Composition, spec.Properties), tolerance)
            let appearance = Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Appearance), tolerance)
            let detail = c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))
            from takeoff in TypeTakeoff(baked.Section, spec.Properties, tolerance)
            let seeded = SeedType(type, material, appearance, detail, takeoff)
            from addressed in BindTextureSet(seeded, spec.TextureSet, spec.ThinWalled, appearance.Id, tolerance, ctx.Key)
            from bound in AuthorOccurrences(spec.Occurrences, type.Id, material.Id, ctx, addressed)
            select bound;
    }

    // --- [TEXTURE_SET_EDGE]
    // Baked sets sit ONE GRAPH HOP off the appearance node, never as a summary column: the seam AppearanceKey
    // freezes at the seven neutral PBR values, so widening it re-ids every Node.Appearance in the estate for a
    // field only a texture consumer reads and stops a material deduplicating against its own baked variant.
    // Addresses ride a content-keyed Node.PropertySet under the seam-declared row, so two materials sharing one
    // baked set share one bag node and a re-press at a higher resolution adds an edge while every id stands.
    // Values cross the PropertyValue.Of structural gate like every other bag row — this projector holds no
    // second admission path. The bag carries TWO rows: the baked-set address under DetailSchema.TextureSet and the
    // double-sided shell flag under DetailSchema.DoubleSided — the Materials PRODUCER of the sidedness fact, written
    // only when the row's ThinWalled is set so absence stays undeclared and the Bim IfcSurfaceSide producer (which
    // answers for appearances IT minted) never contends over one node. A thin-walled UNBAKED material mints the bag
    // for its flag alone; a material with neither row authors nothing.
    static Fin<GraphDelta> BindTextureSet(GraphDelta delta, Option<ContentAddress> set, bool thinWalled, NodeId appearanceId, double tolerance, Op key) =>
        from address in set.Match(
            Some: a => PropertyValue.Of(new PropertyValue.Text(a.ToValue()), key).Map(Optional),
            None: () => Fin.Succ(Option<PropertyValue>.None))
        from sided in thinWalled
            ? PropertyValue.Of(new PropertyValue.Boolean(true), key).Map(Optional)
            : Fin.Succ(Option<PropertyValue>.None)
        select address.IsNone && sided.IsNone
            ? delta
            : Bound(delta, appearanceId, tolerance,
                Seq((Row: DetailSchema.TextureSet, Value: address), (Row: DetailSchema.DoubleSided, Value: sided)));

    // One bag node over the present rows — the fold keeps row order declaration-stable so two materials sharing
    // one baked set and one sidedness still content-key to ONE bag node.
    static GraphDelta Bound(GraphDelta delta, NodeId appearanceId, double tolerance, Seq<(PropertyName Row, Option<PropertyValue> Value)> rows) {
        Node.PropertySet node = Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty),
            rows.Fold(DetailSchema.Appearance.Bag(PropertySource.Derived),
                static (bag, row) => row.Value.Match(Some: value => bag.With(row.Row, value), None: () => bag))), tolerance);
        return delta.Put(node).Link(new Relationship.Associate(appearanceId, node.Id, new MaterialUsage.None()));
    }

    // --- [TYPE_TAKEOFF]
    // TypeTakeoff mints linear mass, surface-area-per-length, and volume-per-length ONCE at projection from the
    // section integral the catalogue already ran and the substance density the seam property set already carries,
    // so tonnage, coating-area, and embodied-carbon reads are graph hops rather than a geometry re-fold at every
    // consumer. Rows spell the seam-owned DetailSchema.Takeoff vocabulary and values compose the seam MeasureValue
    // algebra: mass-per-length IS area.Multiply(density) re-typed through WithType, since AreaDim x DensityDim
    // composes exactly the [-1,1,0,0,0,0,0] LinearDensity signature — never a bare-double kg/m re-derivation —
    // while both geometric rows re-type already-minted QuantityRow measures rather than a second scale. PARTIALITY
    // IS TWO-TIERED and deliberate: a section-free component mints no set at all, while a substance carrying
    // neither stiffness case drops the mass row ALONE and still lands both geometric rows — withholding a takeoff
    // because one input is absent is the deleted all-or-nothing form.
    static Fin<Option<Node>> TypeTakeoff(Option<ComputedSection> section, Seq<MaterialPropertySet> properties, double tolerance) =>
        section.Match(
            None: () => Fin.Succ(Option<Node>.None),
            Some: c =>
                from area in QuantityRow.Area.OfNative(c.AreaMm2.Value)
                from perimeter in QuantityRow.Length.OfNative(c.HeatedPerimeterMm.Value)
                from volumePerLength in area.WithType(QuantityType.Create("VolumePerLength"))
                from areaPerLength in perimeter.WithType(QuantityType.Create("SurfaceAreaPerLength"))
                from massPerLength in properties.Density.Match(
                    Some: density => area.Multiply(density).Bind(static m => m.WithType(QuantityType.Create("LinearDensity"))).Map(Some),
                    None: () => Fin.Succ(Option<MeasureValue>.None))
                let geometric = DetailSchema.Takeoff.Quantities(PropertySource.Derived)
                    .With(DetailSchema.VolumePerLength, volumePerLength)
                    .With(DetailSchema.SurfaceAreaPerLength, areaPerLength)
                select Some(Mint(new Node.QuantitySet(NodeId.Content(ReadOnlySpan<byte>.Empty),
                    massPerLength.Match(
                        Some: mass => geometric.With(DetailSchema.MassPerLength, mass),
                        None: () => geometric)), tolerance)));

    // MINT the deterministic-rooted Type Object: a ROOTED identity DERIVED from the Component's canonical content
    // through NodeId.RootedType over Node.Object.ToTypeSeedBytes (which EXCLUDES the volatile Representations AND the
    // secondary Classifications set, so a later geometry attach or a standard-classification stamp never re-keys and
    // identical Components dedup to one Type). Kind is the REUSED ObjectKind.Type static; the Classification/
    // PredefinedType stamp reads the IfcBinding forwarders (the seed-computed row data, so this body is family-blind;
    // roster validity is Rasm.Bim's composition-time IfcLegality arm and its per-token AdmitPredefined egress gate); the facts-resolved standard reference rides the Classifications set directly (the prior deliberate
    // Type-path drop retired with the seed exclusion); the Designation rides Name+Tag; Representations are Empty
    // (geometry host-materialized and content-key-attached later); the SchemaSpan comes from the model Header. The
    // draft carries a placeholder Rooted id (ToTypeSeedBytes excludes the id), then Relabel re-stamps the derived
    // NodeId.RootedType — a class-root [Union] Node case has NO compiler `with`, so Relabel.
    static Node MintType(Component c, Classification classification, Option<Classification> standard, ProjectionContext ctx) {
        Node.Object draft = new(
            NodeId.Rooted(), ObjectKind.Type, Option<string>.None, classification, PredefinedType.Create(c.PredefinedToken),
            c.Designation.Value, c.Designation.Value, RepresentationContentHash.Empty, Option<OwnerHistory>.None, SchemaSpan.From(ctx.Header.Schema), standard.ToSeq());
        return draft.Relabel(NodeId.RootedType(draft.ToTypeSeedBytes(ctx.Header.Tolerance).Span));
    }

    // Author the Type subgraph: Put the minted Type Object, its content-keyed structural Material (baked section),
    // the required Appearance, and every optional bag — the seed-built detail set and the derived takeoff quantity
    // set — plus the Type→Material / Type→Appearance Associate edges (MaterialUsage.None — the TYPE-level
    // association carries no per-occurrence usage; occurrence usage rides the occurrence's own binding
    // [OCCURRENCE_USAGE_RULING]) and one Type→bag Assign.PropertyDefinition each (occurrences inherit through the
    // Bake type-bag merge). Both bags take the SAME put-and-link step, so they fold over one Seq rather than
    // repeating a Match arm per bag — a third bag kind adds one ToSeq term and no branch. Both endpoints are
    // owned here, so no vouch gates these edges.
    static GraphDelta SeedType(Node type, Node material, Node appearance, Option<Node> detail, Option<Node> takeoff) =>
        (detail.ToSeq() + takeoff.ToSeq()).Fold(
            GraphDelta.Empty.Put(type).Put(material).Put(appearance)
                .Link(new Relationship.Associate(type.Id, material.Id, new MaterialUsage.None()))
                .Link(new Relationship.Associate(type.Id, appearance.Id, new MaterialUsage.None())),
            (delta, bag) => delta.Put(bag).Link(new Relationship.Assign(type.Id, bag.Id, AssignKind.PropertyDefinition)));

    // --- [SECTION_BAKE]
    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured component#COMPONENT_RESOLUTION table and
    // BAKE the neutral seam SectionProperties onto the composition (WithSection), so the structural runner reads
    // graph.SectionOf(member) without re-resolving or admitting VividOrange. A non-ProfileSet bakes nothing,
    // total; a ProfileSet ref present with a None section bakes nothing; a ProfileSet ref ABSENT from the table
    // rails ProjectionFault.Unresolved — the M7 cache is total over every catalogued component, so an absent ref
    // is a caller-supplied incomplete-table bug surfaced, never a silently-dropped section. Shared by both folds.
    // BakeSection rides the mm-basis ComputedSection out beside the baked composition because the takeoff mint
    // needs the raw section integral, not its SI seam projection — resolving one ref twice is the deleted form.
    Fin<(MaterialComposition Composition, Option<ComputedSection> Section)> BakeSection(MaterialComposition composition, Op key) =>
        composition is MaterialComposition.ProfileSet ps
            ? source.Sections.TryGetValue(ps.Profile, out ResolvedComponent resolved)
                ? resolved.Section.Match(
                    Some: section => SeamSection(section).Map(seam => (composition.WithSection(seam), Some(section))),
                    None: () => Fin.Succ((composition, Option<ComputedSection>.None)))
                : ProjectionFault.Unresolved(key, $"<profile-ref-unresolved:{ps.Profile.Designation}>")
            : Fin.Succ((composition, Option<ComputedSection>.None));

    // The neutral seam SectionProperties lifted from the twenty-field ComputedSection — every typed mint now a
    // component#QUANTITY_ROW QuantityRow row (the ONE bounded mint site; the six local Len/Area/Modulus/Inertia/
    // Torsion/Warping statics are DELETED), so every QuantityType/Dimension/SI-scale is byte-identical to the
    // registry row and MeasureValue content keys are unchanged. Iw is the FIFTH field (after J); the shear-area
    // lift preserves the major/minor convention (AvyMm2 MAJOR/web -> AvY); the three asymmetric-section LTB
    // columns lift last (engineering-zero for every doubly-symmetric family, non-zero for the open thin-walled
    // shapes steel fills). Named arguments PIN each lift so a future seam re-order cannot silently re-slot a column.
    static Fin<SectionProperties> SeamSection(ComputedSection c) =>
        from area in QuantityRow.Area.OfNative(c.AreaMm2.Value)
        from iyy in QuantityRow.SecondMomentOfArea.OfNative(c.IxMm4.Value)
        from izz in QuantityRow.SecondMomentOfArea.OfNative(c.IyMm4.Value)
        from j in QuantityRow.TorsionConstant.OfNative(c.JMm4.Value)
        from iw in QuantityRow.WarpingConstant.OfNative(c.IwMm6)
        from wely in QuantityRow.SectionModulus.OfNative(c.SxMm3.Value)
        from welz in QuantityRow.SectionModulus.OfNative(c.SyMm3.Value)
        from wply in QuantityRow.SectionModulus.OfNative(c.ZxMm3.Value)
        from wplz in QuantityRow.SectionModulus.OfNative(c.ZyMm3.Value)
        from avY in QuantityRow.Area.OfNative(c.AvyMm2.Value)
        from avZ in QuantityRow.Area.OfNative(c.AvzMm2.Value)
        from radiusMajor in QuantityRow.Length.OfNative(c.RxMm.Value)
        from radiusMinor in QuantityRow.Length.OfNative(c.RyMm.Value)
        from depth in QuantityRow.Length.OfNative(c.DepthMm.Value)
        from width in QuantityRow.Length.OfNative(c.WidthMm.Value)
        from heatedPerimeter in QuantityRow.Length.OfNative(c.HeatedPerimeterMm.Value)
        from axisDistance in QuantityRow.Length.OfNative(c.AxisDistanceMm)
        from shearCentreY in QuantityRow.Length.OfNative(c.ShearCentreYMm)
        from shearCentreZ in QuantityRow.Length.OfNative(c.ShearCentreZMm)
        select new SectionProperties(
            Area: area,
            Iyy: iyy,
            Izz: izz,
            J: j,
            Iw: iw,
            Wely: wely,
            Welz: welz,
            Wply: wply,
            Wplz: wplz,
            AvY: avY,
            AvZ: avZ,
            RadiusOfGyrationMajor: radiusMajor,
            RadiusOfGyrationMinor: radiusMinor,
            Depth: depth,
            Width: width,
            HeatedPerimeter: heatedPerimeter,
            AxisDistance: axisDistance,
            ShearCentreY: shearCentreY,
            ShearCentreZ: shearCentreZ,
            MonosymmetryFactor: c.MonosymmetryFactor);

    // --- [CONTENT_MINT]
    // Content-id mint — re-stamps a non-rooted node's id from its OWN canonical bytes (id-excluded, the seam
    // ToCanonicalBytes + the kernel seed-zero XxHash128 the seam ContentAddress composes; this folder owns no
    // hasher) through the seam Relabel re-stamp. Two specs minting the same node collapse to one id, idempotent
    // under re-projection, and the cross-runtime C#/Python/TypeScript parity corpus pins the canonical bytes
    // byte-for-byte so the same node keys identically in every runtime.
    static Node Mint(Node draft, double tolerance) =>
        draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));

    // --- [OCCURRENCE_EDGES]
    // H12 (substance): author element→material (and element→appearance) Associate edges ONLY for bindings whose
    // Element ctx.Owns vouches. Vouching is APPLICATIVE — every unvouched element reports, accumulated into one
    // failure (an empty Bindings set traverses to zero edges and zero faults: the pure-Materials subgraph usable
    // in isolation). Gating on ctx.ElementIds emptiness and SILENTLY DROPPING a bound spec's edges is the [H12]
    // violation — an empty context vouches none, so EVERY binding faults, never just the first.
    static Fin<GraphDelta> AuthorBindings(ComponentProjectionSpec.Substance spec, NodeId materialId, NodeId appearanceId, ProjectionContext ctx, GraphDelta delta) =>
        spec.Bindings
            .Traverse(binding => ctx.Owns(binding.Element)
                ? Success<Error, MaterialBinding>(binding)
                : Fail<Error, MaterialBinding>(ProjectionFault.Unvouched(ctx.Key, $"<associate-element-not-in-context:{binding.Element.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(delta, (g, binding) => BindElement(g, binding, materialId, appearanceId)))
            .ToFin();

    static GraphDelta BindElement(GraphDelta delta, MaterialBinding binding, NodeId materialId, NodeId appearanceId) =>
        delta
            .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))
            .Link(new Relationship.Associate(binding.Element, appearanceId, new MaterialUsage.None()));

    // H12 (Type): each vouched occurrence binds to the minted Type and carries its explicit material usage.
    // The occurrence is the sited piece the model author / Bim ingest already rooted (in ctx.ElementIds); the Type
    // is the identity THIS projection minted (NOT vouched — the owner mints its own Type id). Applicative like the
    // substance vouch: EVERY unvouched occurrence reports; an empty set binds nothing.
    static Fin<GraphDelta> AuthorOccurrences(Seq<OccurrenceBinding> occurrences, NodeId typeId, NodeId materialId, ProjectionContext ctx, GraphDelta delta) =>
        occurrences
            .Traverse(binding => ctx.Owns(binding.Element)
                ? Success<Error, OccurrenceBinding>(binding)
                : Fail<Error, OccurrenceBinding>(ProjectionFault.Unvouched(ctx.Key, $"<type-occurrence-not-in-context:{binding.Element.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(delta, (graph, binding) => graph
                .Link(new Relationship.Assign(binding.Element, typeId, AssignKind.TypeDefinition))
                .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))))
            .ToFin();
}
```

## [03]-[COMPOSITION_AUTHOR]

- Owner: `CompositionAuthor` coerces material rows and delegates admission to the seam `MaterialComposition` factories.
- Cases: one `CompositionAuthor` family over the seam trichotomy-plus-single — `Single` (one `MaterialId`, homogeneous — `IfcMaterial`), `LayerSet` (material-plus-thickness rows coerced into `Seq<MaterialLayer>` — `IfcMaterialLayerSet`), `ProfileSet` (one `MaterialId` per extruded member with the `ComponentId` wrapped into a seam `ProfileRef` — `IfcMaterialProfileSet`), `ConstituentSet` (keyword-tagged fraction rows — `IfcMaterialConstituentSet`); the author coerces and DELEGATES to the seam smart-constructor, never a fourth case (`IfcMaterialList` deprecated, never admitted).
- Entry: `LayerSet` and `ConstituentSet` coerce row values and retain the seam admission rail; `ProfileSet` and `Single` are total.
- Growth: a new composition shape extends the seam union and this builder's coercion surface. Occurrence placement remains input data on `OccurrenceBinding`.
- Boundary: the seam owns composition invariants and occurrence-usage admission. This author never invents direction, offset, extent, or cardinal placement from a type-level composition.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// Builds the SEAM MaterialComposition (Rasm.Element/Composition owns the [Union] type, the layer/constituent
// specs, the MeasureValue SI coercion, AND the empty/thickness/fraction admission). This author COERCES Materials'
// raw rows into the seam shapes and DELEGATES every invariant to the seam smart-constructor — it declares NO type
// and owns NO invariant the seam owns; composition admission rails the seam ElementFault.ValueRejected.
public static class CompositionAuthor {
    public static MaterialComposition Single(MaterialId material) => MaterialComposition.OfSingle(material);

    // Coerce each row's thickness through the seam MeasureValue (which rails ElementFault.ValueRejected on a
    // non-finite magnitude), then DELEGATE to the seam OfLayerSet which OWNS the empty-set and non-positive-
    // thickness admission — no duplicated pre-guard, no wrong-band fault. Layers are independent, so the
    // applicative Traverse reports EVERY non-finite thickness, never just the first.
    public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key) =>
        layers.Traverse(l => MeasureValue.Of(l.ThicknessMm, UnitsNet.Units.LengthUnit.Millimeter, key).Map(t => new MaterialLayer(l.Material, t, l.Name))).As()
              .Bind(specs => MaterialComposition.OfLayerSet(specs, key));

    // The component is referenced by its seam ProfileRef ONE-HOP (M7) — the catalogue key (ComponentId.Value)
    // wraps to a ProfileRef the seam ProfileSet case carries. The composition holds the HANDLE; the [02] projector
    // resolves it through component#COMPONENT_RESOLUTION ONCE and BAKES the neutral SectionProperties on
    // (WithSection). TOTAL — the seam OfProfileSet carries no admission invariant, mirroring the total Single.
    public static MaterialComposition ProfileSet(MaterialId material, ComponentId component) =>
        MaterialComposition.OfProfileSet(material, ProfileRef.Of(component.Value));

    // Lift each row into a seam MaterialConstituent, then DELEGATE to the seam OfConstituentSet which OWNS the
    // empty-set, per-fraction unit-range, AND fraction-sum normalization — this author keeps NO constituent invariant.
    public static Fin<MaterialComposition> ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key) =>
        MaterialComposition.OfConstituentSet(constituents.Map(static c => new MaterialConstituent(c.Material, c.Category, c.Fraction)), key);

}
```

## [04]-[COMPONENT_SUBGRAPH]

- Owner: the `ComponentSubgraph` capture composition root — `Capture` (the homogeneous-substance fold building `Substance` specs — the prior `MaterialSubgraph.Capture` per-material construction with the binding fan widened to the real one-material-many-elements arity, the traversal applicative) and `CaptureComponent` (the applicative catalogue-row fold building the `Type` specs) — plus `CompositionOf`, the RELOCATED composition selection (the per-family `ToLayerSet`/shape-kind methods are the deleted forms, replaced by ONE read of the campaign currency: `ComponentRow.Sectioned` + the `SectionProfile.Layered` arm), and `Lowerings`, the family-keyed physics-lowering table.
- Cases: the `Substance` case projects a content-keyed material and appearance; the `Type` case projects a rooted type object, structural material, required appearance, seed-built detail bag, and explicit occurrence bindings. `Sectioned` rows select `ProfileSet`, unsectioned `Layered` rows select `LayerSet`, and every other row selects `Single`. `Lowerings` rows — glazing's IGU receipts, masonry's clay coursing physics, cmu's lattice physics — each restore their own seed axes and lower onto the seam property set.
- Entry: `ComponentSubgraph.Capture` builds independent homogeneous `Substance` specs with `MaterialUsage.None`, and `CaptureComponent` builds Type specs whose caller supplies each occurrence usage; both take the caller's `setOf` baked-set resolver keyed on the appearance material. Both required appearance lookups remain on `Fin`.
- Packages: Rasm.Element (project — `Node` cases, `MaterialComposition`, `MaterialLayer`/`MaterialConstituent`, `MaterialUsage`/`LayerSetDirection`/`DirectionSense`/`CardinalPoint`, `MeasureValue`, `MaterialPropertySet`, `Relationship`, `Classification`, `AppearanceSummary`, `ProfileRef`), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentId`/`SectionProfile`/`Ply`/`ComponentResolution`/`ResolvedComponent`), Rasm.Materials.Properties (project — `MaterialPropertyCatalogue`/`SustainabilityCatalogue`), Rasm.Materials.Appearance.Graph + Interchange (project — `MaterialLibrary`/`MaterialWire`), LanguageExt.Core; the `Rasm.Materials.Construction` reference is RETIRED — `CompositionAuthor` is `[03]`, this namespace.
- Growth: a new engineering discipline routed to a material is one seam `Discipline` row the `MaterialPropertySet` carries — no capture arm; a new family's Type capture is ALREADY total (a new `ComponentFamily` row's components flow through the same `Sectioned`/`Layered`/`Single` law with zero edits here); a new family physics lowering is one `Lowerings` row binding its seed page's own restore-and-lower pair; a new composition shape is one `CompositionOf` arm over the new seam case — the subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: `CompositionOf` reads only `Sectioned` and `SectionProfile`; a solved section always selects `ProfileSet`, while a `Layered` profile maps its bounded role currency to the seam's string `Name` only at `CompositionAuthor.LayerSet`. Required material facts rail missing keys, lifecycle facts remain optional, component detail stays seed-built, and the additive `GraphDelta` passes through the seam's second interface `IGraphConstraint.Validate` — the `Rasm.Bim`-implemented IFC-semantic legality gate — before folding, this capture enforcing only the structural invariants it owns so the two interfaces stay orthogonal. `Rasm.Materials` authors NO `Assessment` node: the `Discipline`-keyed `MaterialPropertySet` set carried on the projected `Material` node IS the assessment input, which `Rasm.Compute` reads directly above the seam, routes by discipline, and answers with a seam `Assessment` `Result` node content-keyed on the input key and route — the multi-ply `AssemblyAggregator` being `Rasm.Compute`'s as well. Every `Lowerings` row DELEGATES: the design-code computation stays its seed page's, so this table restores a typed row and calls one lowering, and a family whose row cannot restore its own axes rails `ProjectionFault.Unresolved` rather than lowering a partial receipt — an incomplete-table bug surfaced, never a silently thinned property set.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The capture composition root: fold the catalogues into the ComponentProjectionSource the ComponentProjector
// folds. The SUBSTANCE capture keeps the prior MaterialSubgraph.Capture per-material construction, its binding
// fan widened to the real one-material-many-elements arity; the TYPE capture takes catalogue ComponentRows and
// DERIVES each composition from the row currency. The M7 ProfileRef→ResolvedComponent table
// (component#COMPONENT_RESOLUTION, supplied once by the caller so the section integral runs once per
// component, never per material) seeds the source's Sections.
public static class ComponentSubgraph {
    // The homogeneous-substance capture authors one applicatively validated Substance case per library material.
    // `setOf` resolves the baked set behind an APPEARANCE material — the same resolver the Type capture reads
    // through each row's AppearanceId — so a caller holding the press's own products supplies one function and an
    // unbaked estate supplies the constant absence.
    public static Fin<ComponentProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Seq<NodeId>> elementsOf,
        Func<MaterialId, Option<ContentAddress>> setOf,
        FrozenDictionary<ProfileRef, ResolvedComponent> sections,
        Op key) =>
        materials
            .Traverse(id =>
                (from facts in MaterialFactsCatalogue.Lookup(id, key)
                 let composition = CompositionAuthor.Single(id)                                       // a homogeneous library material; layered/profiled compositions ride a Type spec's row derivation
                 let elements = elementsOf(id)
                 let bindings = elements.Map(element => new MaterialBinding(element, new MaterialUsage.None(), facts.Classification))
                 from row in MaterialLibrary.Lookup(id, key)
                 from appearance in MaterialWire.Summary(row, key)
                 select new ComponentProjectionSpec.Substance(id, composition, facts.Properties, appearance, setOf(id), bindings) { ThinWalled = row.ThinWalled }).ToValidation())
            .As()
            .Map(specs => specs.Fold(ComponentProjectionSource.Empty with { Sections = sections }, static (source, spec) => source.Add(spec)))
            .ToFin();

    // The Type capture pairs catalogue rows with vouched occurrence bindings and requires both material facts and appearance.
    public static Fin<ComponentProjectionSource> CaptureComponent(
        ComponentProjectionSource source, Seq<(ComponentRow Row, Seq<OccurrenceBinding> Occurrences)> rows,
        Func<MaterialId, Option<ContentAddress>> setOf, Op key) =>
        rows.Traverse(entry =>
                (from composition in CompositionOf(entry.Row, key)
                 from facts in MaterialFactsCatalogue.Lookup(entry.Row.Item.SubstanceId, key)
                 from physics in FamilyProperties(entry.Row.Item, key)
                 from row in MaterialLibrary.Lookup(entry.Row.Item.AppearanceId, key)
                 from appearance in MaterialWire.Summary(row, key)
                 select new ComponentProjectionSpec.Type(entry.Row.Item, composition, facts.Properties + physics, appearance,
                     setOf(entry.Row.Item.AppearanceId), facts.Classification, entry.Occurrences) { ThinWalled = row.ThinWalled }).ToValidation())
            .As()
            .Map(specs => specs.Fold(source, static (acc, spec) => acc.Add(spec)))
            .ToFin();

    // Rows lower per-family physics — ONE per family whose seed page computes a receipt the projected Material
    // node's property set can carry, each composing that page's OWN restore-and-lower pair rather than re-reading
    // its axes here. Glazing lowers the EN 673 / EN 410 / mass-law IGU receipts, masonry the EN 1745 / IBC 722.4
    // clay physics with its WallAcoustics spectrum and its unit fire rating, cmu the ACI 216.1 / NCMA TEK 6-2C
    // lattice physics — the last two the parity owners the coursing families' fire hours, isothermal-planes
    // resistance, self-weight, and eighteen-band spectrum previously reached no seam through. Capture stays ONE
    // family-blind fold: a family absent from the table contributes the empty set, and a fourth lowering lands as
    // one row rather than a branch. Lazy supplies the materialization edge a SmartEnum-keyed index requires,
    // since an eager initializer reading a family static captures null before its type init completes.
    static readonly Lazy<FrozenDictionary<ComponentFamily, Func<Component, Op, Fin<Seq<MaterialPropertySet>>>>> Lowerings =
        new(static () => new Dictionary<ComponentFamily, Func<Component, Op, Fin<Seq<MaterialPropertySet>>>> {
            [ComponentFamily.Glazing] = static (item, key) => GlazingSeed.Resolve(item, key)
                .Bind(build => GlazingDetail.Properties(build.Panes, build.Cavities, build.FireResistanceEiMinutes, key)),
            [ComponentFamily.Masonry] = static (item, key) =>
                MasonrySeed.Table.TryGetValue(item.Designation, out MasonryRow row)
                    ? MasonryDetail.Properties(item.Profile, row.Body, key)
                    : ProjectionFault.Unresolved(key, $"<masonry-row-unresolved:{item.Designation.Value}>"),
            [ComponentFamily.Cmu] = static (item, key) =>
                CmuSeed.Table.TryGetValue(item.Designation, out CmuRow row) && item.Profile is SectionProfile.CellularRectangle cell
                    ? CmuSeed.Properties(row, cell, key)
                    : ProjectionFault.Unresolved(key, $"<cmu-lattice-unresolved:{item.Designation.Value}>"),
        }.ToFrozenDictionary());

    static Fin<Seq<MaterialPropertySet>> FamilyProperties(Component item, Op key) =>
        Lowerings.Value.TryGetValue(item.Family, out Func<Component, Op, Fin<Seq<MaterialPropertySet>>>? lower)
            ? lower(item, key)
            : Fin.Succ(Seq<MaterialPropertySet>());

    // The RELOCATED composition selection — ONE law over the campaign currency, replacing every per-family
    // ToLayerSet/shape-kind method: a Sectioned row is a ProfileSet (its solved section MUST have a ProfileSet to
    // bake onto — steel/timber-member/cmu and the structural deck boards); an unsectioned Layered row becomes a
    // LayerSet whose names derive at the IFC boundary from each ply's material, bounded role, and stable ordinal;
    // every other row is a Single discrete part (fastener, connector, joint, bar, brick).
    static Fin<MaterialComposition> CompositionOf(ComponentRow row, Op key) =>
        row switch {
            { Sectioned: true } => Fin.Succ(CompositionAuthor.ProfileSet(row.Item.SubstanceId, row.Item.Designation)),
            { Item.Profile: SectionProfile.Layered layered } => CompositionAuthor.LayerSet(
                layered.Plies.Map(static (p, ordinal) => (p.Material, p.ThicknessMm.Value, $"{p.Role.Key}:{ordinal}:{p.Material.Value}")), key),
            _ => Fin.Succ(CompositionAuthor.Single(row.Item.SubstanceId)),
        };
}
```

## [05]-[RESEARCH]

(none)
