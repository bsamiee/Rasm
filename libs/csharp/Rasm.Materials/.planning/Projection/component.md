# [MATERIALS_PROJECTION]

THE COMPONENT PROJECTOR, THE SEAM COMPOSITION AUTHOR, and THE COMPONENT-SUBGRAPH CAPTURE. `ComponentProjector.Project` folds payload-complete `Substance` and `Type` cases onto one `Fin<GraphDelta>` and crosses the `observability#HOOK_RAIL` `ProjectionGate` veto before that delta returns.

Every appearance is required by the captured spec, and every `OccurrenceBinding` carries its explicit `MaterialUsage`; layer direction, offset, extent, and profile cardinal placement never derive from type-level composition. Every baked `TextureSet` rides as a property-set DEFINITION on the owning object rather than as an `AppearanceSummary` column, so the seam key stays bake-invariant.

`CompositionAuthor` builds `MaterialComposition`, and `ComponentSubgraph` selects `ProfileSet`, `LayerSet`, or `Single` from the component row and lowers each family's own physics receipt through one family-keyed table.

## [01]-[INDEX]

- [02]-[COMPONENT_PROJECTOR]: the `ComponentProjector` `IElementProjection` owner, the `ProjectionSource` aggregate, the payload-complete `ProjectionSpec` union, `MaterialBinding`/`OccurrenceBinding`, the `Project` fold with its `ProjectionGate` veto consult, the `ProjectionFault` rail, the content mints, the M7 section bake, the type-level takeoff mint, the baked-set property bag, and the binding vouches.
- [03]-[COMPOSITION_AUTHOR]: the seam-`MaterialComposition` `Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet` builders, and the `ConstituentRecipe`/`Constituents` producer deriving fraction-tagged rows from a declared mix proportion or a product layup.
- [04]-[COMPONENT_SUBGRAPH]: the `ComponentSubgraph` capture composition root — the homogeneous-substance `Capture` and the `CaptureComponent` Type capture whose composition selection reads the `ComponentRow` `Sectioned` pin and the `SectionProfile.Layered` arm, beside the family-keyed `Lowerings` physics table.

## [02]-[COMPONENT_PROJECTOR]

- Owner: `ComponentProjector` is the sealed `IElementProjection`; `ProjectionSource` carries the spec stream and section table; `ProjectionSpec` carries each modality's complete payload; `MaterialBinding` carries substance-path usage and classification; `OccurrenceBinding` carries a vouched Type occurrence and its required usage.
- Cases: `Substance(MaterialId, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Option<ContentAddress>, bool ThinWalled, Seq<MaterialBinding>)` and `Type(Component, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Option<ContentAddress>, Option<Classification>, bool ThinWalled, Seq<OccurrenceBinding>)` — `ThinWalled` is POSITIONAL on both, because an `init` property outside the positional list is a payload a construction site can omit silently, and the whole point of a payload-complete spec is that omission is unspellable.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam op: `source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()` — specs are INDEPENDENT, so the fold is APPLICATIVE (the EXACT seam `Assemble` shape, `Traverse`→`ToValidation`→`Merge`-fold per the seam `[APPLICATIVE_CAPTURE]` law — never a hand-threaded accumulator, never a first-fault-only `TraverseM`) — every failing spec reports (an unvouched binding/occurrence `ProjectionFault.Unvouched`, a malformed Type `Classification` the seam `ElementFault` lifts unchanged through `Classification.Of`), the accumulated `ManyErrors` lowering onto ONE `Fin<GraphDelta>` whose success then crosses the `observability#HOOK_RAIL` `MaterialsPoint.ProjectionGate` VETO — addressed through the fired fact's own `At` column, never a point literal beside it — before it returns; `ProjectSpec` discriminates via the generated total `Switch` — `Substance`→`ProjectSubstance`, `Type`→`ProjectType`; `ComponentProjector.Of(source, rail)` captures both once, and the seam `Assemble(ProjectionSuite.Of(…), seed, ctx)` re-merges this delta with `Rasm.Bim`'s `SemanticProjector` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — the seam: `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`RepresentationContentHash`/`SchemaSpan`/`OwnerHistory`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`PropertyValue`/`DetailSchema`/`EvidenceGrade`/`ContentAddress`/`MaterialId`, with `FaultBand` the allocation ledger), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile`/`ComputedSection`/`ResolvedComponent`/`QuantityRow`, the standardized-type owner whose `IfcBinding` forwarders and typed-mint rows this projector reads), Rasm.Materials.Projection (`observability#HOOK_RAIL` — `MaterialsPoint`/`MaterialsFact` over the kernel `HookRail`, the veto seating this folder's own signal owner declares), Rasm.Domain (project — `Op`/`HookRail`/`TelemetrySource`; the seed-zero `XxHash128` content seed is the seam `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` + generated total `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`ToValidation`/`ToFin`/`Fold`/`Option`); cite `libs/csharp/.api/api-thinktecture-runtime-extensions.md` — the `Rasm.Materials/.api` VividOrange catalogues are the `component#COMPONENT_OWNER`'s, not composed here (the projector reads an already-resolved `ComputedSection`, never the section solver).
- Growth: a new projected node kind is one seam `Node` case, a new spec modality one `ProjectionSpec` case, a new occurrence-usage shape one seam `MaterialUsage` case carried by `OccurrenceBinding`, a new type-level takeoff quantity one seam `DetailSchema.Takeoff` row with its `TypeTakeoff` mint line, and a new VETO one `MaterialsPoint` row with its seating — never a projector edit, because the consult is over the merged delta this fold already produces.
- Law: EDGE KIND FOLLOWS ENDPOINT KIND, which is the seam's own admission and not a local preference. `Associate` carries a RESOURCE and admits `Node.Object` relating a `Node.Material`, `Node.Appearance`, or `Node.Coverage`; `Assign` carries a DEFINITION and admits `Node.Object` relating the bag or type its `AssignKind` names. Every property set — seed-built detail, derived takeoff, texture-and-sidedness alike — therefore reaches its owner as `Assign.PropertyDefinition` from an OBJECT, and an appearance node standing as a relating endpoint is unrepresentable in both directions at once: it is not an `Object`, and a bag is not a resource. The Type fold binds its bags to the minted Type; the substance fold, which mints no Object, binds them to each VOUCHED element.
- Law: each Type occurrence is vouched independently and binds through `Assign.TypeDefinition` with its explicit occurrence-to-material usage.
- Law: `MaterialLibrary.Lookup(...).Bind(row => AppearanceEgress.Summary(row, key))` remains required on `Fin` at BOTH hops — the seam factory gates every channel to the unit range and rails on its own key, so the lowering binds rather than maps; no optional appearance state survives inside the spec.
- Law: `TypeTakeoff` reads the seam-owned row vocabulary and the seam substance-density accessor, deriving no numeric semantics of its own — quantity identity, unit, and dimensional composition stay `Rasm.Element`'s, this projector supplying only the section and substance a running metre is measured from.
- Law: BAKING NEVER RE-KEYS `AppearanceSummary`. The seam key freezes at the seven neutral PBR values, so the baked set rides one graph hop away as a content-keyed `Node.PropertySet` carrying the set address under the seam-declared row, and re-pressing a material at a higher resolution adds an edge while every node id in the estate stands. Widening the summary instead forks the `Rasm.Bim` dedup key for a field only a texture consumer reads and stops a material deduplicating against its own baked variant.
- Law: seed-built detail bags ROUND-TRIP by element genus — a realizing-element family imports through the `Rasm.Bim` connection-detail reader against `DetailSchema.Realization`, panel product detail through the general Bim object/property fold against `DetailSchema.Product`. One bag crosses out and two genus-keyed readers bring it back, never a projector-side import path.
- Boundary: `Veto` verdicts enter this fold's OWN rail per the folder ruling, so the gate consult sits after the merge and before the return, where an `Observe` point stays decorator-only and this page names none.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Interchange;
using Rasm.Materials.Component;
using Rasm.Materials.Properties;
using Thinktecture;
using MaterialsRail = Rasm.Domain.HookRail<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [ERRORS] ------------------------------------------------------------------------------
// ProjectionFault closes the projection recovery vocabulary; generated identity binds each direct leaf's explicit
// ordinal to FaultBand.Projection. A failed projection assembles no partial graph.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectionFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Projection;
    private ProjectionFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    // The Op key rides the PROJECTED message and not the typed property alone: the moment this text leaves the
    // rail, a bare detail strands the refusal from the operation that produced it.
    public override string Message => $"{Key.Value}: {Detail}";
    [FaultCase(0)] public sealed partial record Source(Op Key, string Detail) : ProjectionFault(Key, Detail);
    [FaultCase(1)] public sealed partial record Unvouched(Op Key, string Detail) : ProjectionFault(Key, Detail);
    [FaultCase(2)] public sealed partial record Unresolved(Op Key, string Detail) : ProjectionFault(Key, Detail);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The element-occurrence material binding the app or Rasm.Bim supplies. Classification is an Object-node VALUE —
// never a Node.Material field, never an edge payload — and the element MUST be vouched by ctx.Owns, since the
// substance path never mints an element identity ([H12]).
public readonly record struct MaterialBinding(NodeId Element, MaterialUsage Usage, Option<Classification> Classification);

// Three INDEPENDENT catalogue reads, so the fan-in ACCUMULATES: each Fin lifts through ToValidation, the tuple
// Apply fans them in, and ToFin lowers the ManyErrors back onto the rail. Applying over the Fin tuple directly
// short-circuits at the first miss, so a material absent from two catalogues reported one gap per lookup round.
public sealed record MaterialFacts(
    Seq<MaterialPropertySet> Properties,
    Option<Classification> Classification) {
    public static Fin<MaterialFacts> Lookup(MaterialId id, Op key) =>
        (MaterialPropertyCatalogue.Lookup(id, key).ToValidation(),
         SustainabilityCatalogue.Lookup(id, key).ToValidation(),
         SustainabilityCatalogue.Classification(id, key).ToValidation())
            .Apply(static (engineering, lifecycle, classification) => new MaterialFacts(engineering + lifecycle, classification))
            .As()
            .ToFin();
}

public readonly record struct OccurrenceBinding(NodeId Element, MaterialUsage Usage);

// The ONE projection spec the single Project fold discriminates: Substance (the pure-material subgraph, no Object)
// versus Type (a minted Type Object, baked-section material, and seed-built detail) — the MaterialProjector and
// ConnectionProjector dual surface collapsed onto one discriminant, never two projectors.
[Union]
public abstract partial record ProjectionSpec {
    private ProjectionSpec() { }

    public sealed record Substance(
        MaterialId Material,
        MaterialComposition Composition,
        Seq<MaterialPropertySet> Properties,
        AppearanceSummary Appearance,
        Option<ContentAddress> TextureSet,               // the press's own set key — an ASSOCIATION off the appearance node, never a summary column, so a bake re-ids nothing
        bool ThinWalled,                                 // the row's double-sided shell fact, bag-bound under DetailSchema.DoubleSided — never a summary column, so sidedness re-ids nothing
        Seq<MaterialBinding> Bindings) : ProjectionSpec;

    public sealed record Type(
        Component Component,
        MaterialComposition Composition,
        Seq<MaterialPropertySet> Properties,
        AppearanceSummary Appearance,
        Option<ContentAddress> TextureSet,
        Option<Classification> StandardClassification,   // the facts-resolved standard reference the Type Object's Classifications set carries — Type-seed-excluded, so the stamp never re-keys NodeSeed.TypeSeed
        bool ThinWalled,
        Seq<OccurrenceBinding> Occurrences) : ProjectionSpec {
        // The capacity-published member rows the resolved SectionCapacity egresses for the forward Compute member
        // check — the RC shear-link triple or the aluminium Table 6.6 buckling pair — under ONE carrier because
        // every row is Element StructuralRows-keyed and rides one derived Realization bag. Init-defaulted EMPTY,
        // so a spec whose section published no member rows binds unchanged.
        public Seq<(PropertyName Row, PropertyValue Value)> CapacityRows { get; init; }
    }
}

// The captured projection source carries the closed spec stream and the resolved section table.
public sealed record ProjectionSource(Seq<ProjectionSpec> Specs, FrozenDictionary<ProfileRef, ResolvedComponent> Sections) {
    public static readonly ProjectionSource Empty = new(Seq<ProjectionSpec>(), FrozenDictionary<ProfileRef, ResolvedComponent>.Empty);
    public ProjectionSource Add(ProjectionSpec spec) => this with { Specs = Specs.Add(spec) };
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Materials folder publishes. Captures the source AND the folder's kernel hook rail
// internally (the source-capture inversion) so the seam op carries only the ProjectionContext; the seam Assemble
// merges this delta with every sibling.
public sealed class ComponentProjector : IElementProjection {
    readonly ProjectionSource source;
    readonly MaterialsRail rail;
    ComponentProjector(ProjectionSource source, MaterialsRail rail) => (this.source, this.rail) = (source, rail);
    public static ComponentProjector Of(ProjectionSource source, MaterialsRail rail) => new(source, rail);

    // Traverse each spec to its OWN delta APPLICATIVELY — specs are independent, so every failing spec reports and
    // ONE Fin carries the accumulated ManyErrors under the [APPLICATIVE_CAPTURE] law; TraverseM's first-fault abort
    // is the rejected disposition. Each spec builds on GraphDelta.Empty, so per-spec projection is decoupled from
    // the running delta. The merged delta then crosses the VETO: the rail hands the fold the ADMITTED fact, so a
    // gate may transform or refuse the whole delta pre-merge. The point comes off the fact's own At column, and a
    // gate answering a foreign case rails rather than silently returning the built delta.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        from merged in source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)))
            .ToFin()
        let fired = new MaterialsFact.ProjectionGate(merged)
        from admitted in rail.Fire(fired.At, fired, ctx.Key, fact => fact is MaterialsFact.ProjectionGate settled
            ? Fin.Succ(settled.Delta)
            : new ProjectionFault.Unresolved(ctx.Key, $"<projection-gate-case:{fact.At.Key}>"))
        select admitted;

    // The ONE discriminator — the substance subgraph (no Object) versus the Type subgraph (a minted Type Object),
    // the generated total Switch over the spec union, never two projector entrypoints.
    Fin<GraphDelta> ProjectSpec(ProjectionSpec spec, ProjectionContext ctx) => spec.Switch(
        substance: s => ProjectSubstance(s, ctx),
        type:      c => ProjectType(c, ctx));

    // --- [SUBSTANCE_FOLD]
    // The pure-material subgraph: a content-addressed Material node with the M7 section baked onto a ProfileSet, an
    // optional content-keyed Appearance node, and the vouched element→material / element→appearance Associate
    // edges, built on GraphDelta.Empty. Each id mints through the seam content address, so two specs projecting the
    // same material mint ONE id and the duplicate add collapses at the seam WorkingGraph.Set upsert.
    Fin<GraphDelta> ProjectSubstance(ProjectionSpec.Substance spec, ProjectionContext ctx) =>
        from baked in BakeSection(spec.Composition, ctx.Key)
        let tolerance = ctx.Header.Tolerance
        let material = Mint(new Node.Material(Unkeyed, spec.Material, baked.Composition, spec.Properties), tolerance)
        let appearance = Mint(new Node.Appearance(Unkeyed, spec.Appearance), tolerance)
        from textures in TextureBag(spec.TextureSet, spec.ThinWalled, tolerance, ctx.Key)
        from bound in AuthorBindings(spec, material, appearance, textures, ctx)
        select bound;

    // --- [TYPE_FOLD]
    // The standardized-Component subgraph: MINT the deterministic-rooted Type Object, lower the structural Material
    // node, the required Appearance, and the SEED-BUILT detail bag read STRAIGHT off c.Detail — each family's bag
    // is built at seed time by the component#COMPONENT_DETAIL owner, and Component.Of's lane/detail type law
    // carries the deleted Detail(Component) switch's totality. Wire the Type→resource edges (both endpoints owned,
    // no vouch), then bind every VOUCHED occurrence via Assign.TypeDefinition PLUS its own occurrence→material
    // Associate carrying the binding's OWN explicit MaterialUsage ([OCCURRENCE_USAGE_RULING] — usage is input data
    // on each OccurrenceBinding, never derived from the type-level composition).
    Fin<GraphDelta> ProjectType(ProjectionSpec.Type spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Component c = spec.Component;
        return
            from classification in Classification.Of("ifc", c.IfcEntity, ctx.Key)
            from baked in BakeSection(spec.Composition, ctx.Key)
            let type = MintType(c, classification, spec.StandardClassification, ctx)
            let material = Mint(new Node.Material(Unkeyed, c.SubstanceId, baked.Composition, spec.Properties), tolerance)
            let appearance = Mint(new Node.Appearance(Unkeyed, spec.Appearance), tolerance)
            from detail in DetailBag(c, tolerance, ctx.Key)
            from takeoff in TypeTakeoff(baked.Section, spec.Properties, tolerance)
            from textures in TextureBag(spec.TextureSet, spec.ThinWalled, tolerance, ctx.Key)
            let capacity = CapacityBag(spec.CapacityRows, tolerance)
            let seeded = SeedType(type, material, appearance, Seq(detail, takeoff, textures, capacity))
            from bound in AuthorOccurrences(spec.Occurrences, type.Id, material.Id, ctx, seeded)
            select bound;
    }

    // --- [TEXTURE_BAG]
    // Baked sets ride a BAG rather than a summary column: the seam AppearanceKey freezes at the seven neutral PBR
    // values, so widening it re-ids every Node.Appearance in the estate for a field only a texture consumer reads.
    // Addresses ride a content-keyed Node.PropertySet, so two materials sharing one baked set share one bag node
    // and a re-press at a higher resolution adds an edge while every id stands. The bag carries TWO rows: the
    // baked-set address and the double-sided shell flag — the Materials PRODUCER of the sidedness fact, written
    // only when ThinWalled is set so absence stays undeclared and the Bim IfcSurfaceSide producer never contends.
    // This producer mints the NODE and authors NO EDGE, the third peer of DetailBag and TypeTakeoff.
    static Fin<Option<Node>> TextureBag(Option<ContentAddress> set, bool thinWalled, double tolerance, Op key) =>
        from address in set.Match(
            Some: a => PropertyValue.Of(new PropertyValue.Text(a.ToValue()), key).Map(Optional),
            None: () => Fin.Succ(Option<PropertyValue>.None))
        from sided in thinWalled
            ? PropertyValue.Of(new PropertyValue.Boolean(true), key).Map(Optional)
            : Fin.Succ(Option<PropertyValue>.None)
        select address.IsNone && sided.IsNone
            ? Option<Node>.None
            // Row order stays declaration-stable, so two materials sharing one baked set content-key to ONE node.
            : Some(Mint(new Node.PropertySet(Unkeyed,
                Seq((Row: DetailSchema.TextureSet, Value: address), (Row: DetailSchema.DoubleSided, Value: sided))
                    .Fold(DetailSchema.Appearance.Bag(EvidenceGrade.Derived),
                        static (bag, row) => row.Value.Match(Some: value => bag.With(row.Row, value), None: () => bag))), tolerance));

    // --- [CAPACITY_BAG]
    // The FOURTH bag term on the peer shape TextureBag legislates: it mints the node, authors no edge, and the
    // binding folds carry it through the same Assign/PropertyDefinition shape. The capacity-published rows ride ONE
    // derived Realization bag under the Element StructuralRows names the Compute member check reads, and an empty
    // set mints nothing — the producer's own whole-or-nothing absence.
    static Option<Node> CapacityBag(Seq<(PropertyName Row, PropertyValue Value)> rows, double tolerance) =>
        rows.IsEmpty
            ? Option<Node>.None
            : Some(Mint(new Node.PropertySet(Unkeyed,
                rows.Fold(DetailSchema.Realization.Bag(EvidenceGrade.Derived),
                    static (bag, row) => bag.With(row.Row, row.Value))), tolerance));

    // --- [DETAIL_GENUS]
    // The seed authors each family's bag against its own DetailSchema root, and the genus is what the two READERS
    // key on: Rasm.Bim's connection-detail reader follows DetailSchema.Realization and its general object/property
    // fold follows DetailSchema.Product. So the projector proves the family's DetailLane TYPE LAW in BOTH
    // directions — a None-lane component carrying a bag has no reader at all, and a Realization- or Product-lane
    // component missing one silently drops the wire datum its egress lane depends on. The lane is a family COLUMN,
    // so this fold is family-blind and a new family needs no arm here.
    static Fin<Option<Node>> DetailBag(Component c, double tolerance, Op key) =>
        (c.Family.Lane, c.Detail) switch {
            (DetailLane.None, { IsNone: true }) => Fin.Succ(Option<Node>.None),
            (DetailLane.None, _) => new ProjectionFault.Unresolved(key, $"<detail-bag-on-none-lane:{c.Designation.Value}>"),
            (_, { IsSome: true, Case: PropertyBag bag }) =>
                Fin.Succ(Some(Mint(new Node.PropertySet(Unkeyed, bag), tolerance))),
            (DetailLane lane, _) => new ProjectionFault.Unresolved(key, $"<detail-bag-absent:{c.Designation.Value}:{lane}>"),
        };

    // --- [TYPE_TAKEOFF]
    // Linear mass, surface-area-per-length, and volume-per-length mint ONCE at projection off the section integral
    // the catalogue already ran and the substance density the seam property set already carries, so tonnage,
    // coating-area, and embodied-carbon reads are graph hops rather than a geometry re-fold at every consumer.
    // Values compose the seam MeasureValue algebra: mass-per-length IS area.Multiply(density) re-typed through
    // WithType, since AreaDim x DensityDim composes exactly the [-1,1,0,0,0,0,0] LinearDensity signature.
    // PARTIALITY IS TWO-TIERED: a section-free component mints no set at all, while a substance carrying neither
    // stiffness case drops the mass row ALONE and still lands both geometric rows.
    static Fin<Option<Node>> TypeTakeoff(Option<ComputedSection> section, Seq<MaterialPropertySet> properties, double tolerance) =>
        section.Match(
            None: () => Fin.Succ(Option<Node>.None),
            Some: c =>
                from area in QuantityRow.Area.OfNative(c.AreaMm2.Value)
                from perimeter in QuantityRow.Length.OfNative(c.HeatedPerimeterMm.Value)
                from volumePerLength in area.WithType(QuantityRow.VolumePerLength.Type)
                from areaPerLength in perimeter.WithType(QuantityRow.SurfaceAreaPerLength.Type)
                from massPerLength in properties.Density.Match(
                    Some: density => area.Multiply(density).Bind(static m => m.WithType(QuantityRow.LinearDensity.Type)).Map(Some),
                    None: () => Fin.Succ(Option<MeasureValue>.None))
                let geometric = DetailSchema.Takeoff.Quantities(EvidenceGrade.Derived)
                    .With(DetailSchema.VolumePerLength, volumePerLength)
                    .With(DetailSchema.SurfaceAreaPerLength, areaPerLength)
                select Some(Mint(new Node.QuantitySet(Unkeyed,
                    massPerLength.Match(
                        Some: mass => geometric.With(DetailSchema.MassPerLength, mass),
                        None: () => geometric)), tolerance)));

    // MINT the deterministic-rooted Type Object: a ROOTED identity DERIVED from the Component's canonical content
    // through NodeId.Of over the NodeSeed.TypeSeed case, which EXCLUDES the volatile Representations AND the
    // secondary Classifications set, so a later geometry attach or a standard-classification stamp never re-keys
    // and identical Components dedup to one Type. The Classification/PredefinedType stamp reads the IfcBinding
    // forwarders — seed-computed row data, so this body is family-blind, roster validity staying Rasm.Bim's
    // composition-time IfcLegality arm and its per-token AdmitPredefined egress gate. Representations are Empty
    // because geometry is host-materialized and content-key-attached later. The draft carries a placeholder
    // Placement id the TypeSeed case excludes, then Relabel re-stamps the derived id — a class-root [Union] Node
    // case has NO compiler `with`.
    static Node MintType(Component c, Classification classification, Option<Classification> standard, ProjectionContext ctx) {
        // Every argument NAMED: eleven positional slots carry four interchangeable string-shaped columns and two
        // Option-shaped ones, so a seam re-order re-seats them without a compiler word, and the two Designation
        // reads land in different columns by construction rather than by position.
        Node.Object draft = new(
            Id: NodeId.Of(new NodeSeed.Placement()),
            Kind: ObjectKind.Type,
            Guid: Option<string>.None,
            Classification: classification,
            Predefined: PredefinedType.Create(c.PredefinedToken),
            Name: c.Designation.Value,
            Tag: c.Designation.Value,
            Representations: RepresentationContentHash.Empty,
            History: Option<OwnerHistory>.None,
            Schema: SchemaSpan.From(ctx.Header.Schema),
            Classifications: standard.ToSeq());
        return draft.Relabel(NodeId.Of(new NodeSeed.TypeSeed(draft, ctx.Header.Tolerance)));
    }

    // Author the Type subgraph: Put the minted Type Object, its content-keyed structural Material, the required
    // Appearance, and every optional bag, plus the Type→Material / Type→Appearance Associate edges under
    // MaterialUsage.Unbound — the TYPE-level association carries no per-occurrence usage, which rides the
    // occurrence's own binding [OCCURRENCE_USAGE_RULING] — and one Type→bag Assign.PropertyDefinition each, which
    // occurrences inherit through the Bake type-bag merge. The bags arrive as ONE Seq the fold flattens, so a
    // fourth bag kind is one term at the call site and no branch here. Both endpoints are owned by this
    // projection, so no vouch gates these edges.
    static GraphDelta SeedType(Node type, Node material, Node appearance, Seq<Option<Node>> bags) =>
        bags.Bind(static bag => bag.ToSeq()).Fold(
            GraphDelta.Empty.Put(type).Put(material).Put(appearance)
                .Link(new Relationship.Associate(type.Id, material.Id, new MaterialUsage.Unbound()))
                .Link(new Relationship.Associate(type.Id, appearance.Id, new MaterialUsage.Unbound())),
            (delta, bag) => delta.Put(bag).Link(new Relationship.Assign(type.Id, bag.Id, AssignKind.PropertyDefinition)));

    // --- [SECTION_BAKE]
    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured component#COMPONENT_RESOLUTION table and BAKE
    // the neutral seam SectionProperties onto the composition, so the structural runner reads graph.SectionOf
    // without re-resolving or admitting VividOrange. A non-ProfileSet bakes nothing, total; a ref present with a
    // None section bakes nothing; a ref ABSENT from the table rails ProjectionFault.Unresolved, since the M7 cache
    // is total over every catalogued component. The mm-basis ComputedSection rides out beside the baked
    // composition because the takeoff mint needs the raw section integral, not its SI seam projection.
    Fin<(MaterialComposition Composition, Option<ComputedSection> Section)> BakeSection(MaterialComposition composition, Op key) =>
        composition is MaterialComposition.ProfileSet ps
            ? source.Sections.TryGetValue(ps.Profile, out ResolvedComponent resolved)
                ? resolved.Section.Match(
                    Some: section => SeamSection(section).Map(seam => (composition.WithSection(seam), Some(section))),
                    None: () => Fin.Succ((composition, Option<ComputedSection>.None)))
                : new ProjectionFault.Unresolved(key, $"<profile-ref-unresolved:{ps.Profile.Designation}>")
            : Fin.Succ((composition, Option<ComputedSection>.None));

    // The neutral seam SectionProperties lifted from the twenty-field ComputedSection — every typed mint a
    // component#QUANTITY_ROW row, the ONE bounded mint site, so every QuantityType, Dimension, and SI scale is
    // byte-identical to the registry row and MeasureValue content keys are unchanged. Iw is the FIFTH field after
    // J; the shear-area lift preserves the major/minor convention (AvyMm2 MAJOR/web -> AvY); the three
    // asymmetric-section LTB columns lift last. Named arguments PIN each lift so a seam re-order cannot re-slot.
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
    // Content-id mint — re-stamps a non-rooted node's id from its OWN canonical bytes, id-excluded, through the
    // seam Relabel; this folder owns no hasher. Two specs minting the same node collapse to one id, idempotent
    // under re-projection, and the cross-runtime parity corpus pins the canonical bytes byte-for-byte.
    static Node Mint(Node draft, double tolerance) =>
        draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tolerance)));

    // The DRAFT id every content-minted node carries INTO Mint. The placeholder belongs to the mint protocol rather
    // than to each call site, so a protocol that changed its placeholder moves one line rather than every draft.
    static readonly NodeId Unkeyed = NodeId.Of(new NodeSeed.Placement());

    // --- [OCCURRENCE_EDGES]
    // H12 (substance): author element→material and element→appearance Associate edges ONLY for bindings ctx.Owns
    // vouches. Vouching is APPLICATIVE — every unvouched element reports into one accumulated failure, and an empty
    // Bindings set traverses to zero edges and zero faults, the pure-Materials subgraph usable in isolation. Gating
    // on ctx.ElementIds emptiness and SILENTLY DROPPING a bound spec's edges is the [H12] violation.
    // The substance path mints NO Object, so the vouched ELEMENTS are the only lawful Relating endpoints on it. The
    // bag node is content-keyed and put ONCE, so every element sharing an appearance shares one bag node.
    static Fin<GraphDelta> AuthorBindings(
        ProjectionSpec.Substance spec, Node material, Node appearance, Option<Node> textures, ProjectionContext ctx) =>
        spec.Bindings
            .Traverse(binding => ctx.Owns(binding.Element)
                ? Success<Error, MaterialBinding>(binding)
                : Fail<Error, MaterialBinding>(new ProjectionFault.Unvouched(ctx.Key, $"<associate-element-not-in-context:{binding.Element.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(
                textures.Fold(GraphDelta.Empty.Put(material).Put(appearance), static (delta, bag) => delta.Put(bag)),
                (delta, binding) => BindElement(delta, binding, material.Id, appearance.Id, textures)))
            .ToFin();

    static GraphDelta BindElement(
        GraphDelta delta, MaterialBinding binding, NodeId materialId, NodeId appearanceId, Option<Node> textures) =>
        textures.Fold(
            delta.Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))
                 .Link(new Relationship.Associate(binding.Element, appearanceId, new MaterialUsage.Unbound())),
            (graph, bag) => graph.Link(new Relationship.Assign(binding.Element, bag.Id, AssignKind.PropertyDefinition)));

    // H12 (Type): each vouched occurrence binds to the minted Type and carries its explicit material usage. The
    // occurrence is the sited piece the model author or Bim ingest already rooted; the Type is the identity THIS
    // projection minted and is NOT vouched, the owner minting its own Type id.
    static Fin<GraphDelta> AuthorOccurrences(Seq<OccurrenceBinding> occurrences, NodeId typeId, NodeId materialId, ProjectionContext ctx, GraphDelta delta) =>
        occurrences
            .Traverse(binding => ctx.Owns(binding.Element)
                ? Success<Error, OccurrenceBinding>(binding)
                : Fail<Error, OccurrenceBinding>(new ProjectionFault.Unvouched(ctx.Key, $"<type-occurrence-not-in-context:{binding.Element.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(delta, (graph, binding) => graph
                .Link(new Relationship.Assign(binding.Element, typeId, AssignKind.TypeDefinition))
                .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))))
            .ToFin();
}
```

## [03]-[COMPOSITION_AUTHOR]

- Owner: `CompositionAuthor` coerces material rows and delegates admission to the seam `MaterialComposition` factories; `ConstituentRecipe` the declared weighted-composition axis (`Mix` a standardized proportion spec, `Layup` a product's own layer stack); `Constituents` the ONE producer deriving fraction-tagged constituent rows from either arm.
- Cases: one `CompositionAuthor` family over the seam trichotomy-plus-single — `Single` (one `MaterialId`, homogeneous — `IfcMaterial`), `LayerSet` (material-plus-thickness rows coerced into `Seq<MaterialLayer>` — `IfcMaterialLayerSet`), `ProfileSet` (one `MaterialId` per extruded member with the `ComponentId` wrapped into a seam `ProfileRef` — `IfcMaterialProfileSet`), `ConstituentSet` (keyword-tagged fraction rows — `IfcMaterialConstituentSet`); the author coerces and DELEGATES to the seam smart-constructor, never a fourth case (`IfcMaterialList` is a closed-window IFC2x3 spelling this projector never admits); `ConstituentRecipe` cases `Mix` · `Layup` (2).
- Entry: `LayerSet` and `ConstituentSet` coerce row values and retain the seam admission rail; `ProfileSet` and `Single` are total. `Constituents.Of(ConstituentRecipe, Op)` is the one recipe fold: the `Mix` arm runs `Properties/properties#MIX_PROPORTION` `MixDesign.Proportion` and projects the per-m³ receipt onto mass-fraction rows over the spec's own `MixMaterials` bindings (air carries no mass and enters no row), and the `Layup` arm folds thickness × catalogued density per layer (a ply material with no catalogued density rails by name — a fabricated mass is the refused form); both emit the raw fraction rows `CompositionAuthor.ConstituentSet` coerces, the seam `OfConstituentSet` owning the normalization algebra.
- Growth: a new composition shape extends the seam union and this builder's coercion surface; a new weighted-composition source is one `ConstituentRecipe` case with its `Of` arm. Occurrence placement remains input data on `OccurrenceBinding`.
- Boundary: the seam owns composition invariants and occurrence-usage admission. This author never invents direction, offset, extent, or cardinal placement from a type-level composition; a recipe is CALLER data resolved per material at the capture root — no substance roster column carries a mix and no family page asserts a recipe, the same declaration law the durability catalogue holds.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// Builds the SEAM MaterialComposition — Rasm.Element/Composition owns the [Union] type, the layer/constituent
// specs, the MeasureValue SI coercion, AND the empty/thickness/fraction admission. This author COERCES Materials'
// raw rows into the seam shapes and DELEGATES every invariant to the seam smart-constructor, declaring NO type and
// owning NO invariant the seam owns; composition admission rails the seam ElementFault.ValueRejected.
public static class CompositionAuthor {
    public static MaterialComposition Single(MaterialId material) => MaterialComposition.OfSingle(material);

    // Coerce each thickness through the seam MeasureValue, then DELEGATE to OfLayerSet which OWNS the empty-set and
    // non-positive-thickness admission — no duplicated pre-guard, no wrong-band fault. Layers are independent, so
    // the applicative Traverse reports EVERY non-finite thickness.
    public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key) =>
        layers.Traverse(l => MeasureValue.Of(l.ThicknessMm, UnitsNet.Units.LengthUnit.Millimeter, key).Map(t => new MaterialLayer(l.Material, t, l.Name))).As()
              .Bind(specs => MaterialComposition.OfLayerSet(specs, key));

    // The component is referenced by its seam ProfileRef ONE-HOP (M7): the composition holds the HANDLE and the
    // [02] projector resolves it through component#COMPONENT_RESOLUTION ONCE, baking the neutral SectionProperties
    // on. TOTAL — the seam OfProfileSet carries no admission invariant, mirroring the total Single.
    public static MaterialComposition ProfileSet(MaterialId material, ComponentId component) =>
        MaterialComposition.OfProfileSet(material, ProfileRef.Of(component.Value));

    // DELEGATE to OfConstituentSet, which OWNS the empty-set, per-fraction unit-range, AND fraction-sum
    // normalization — this author keeps NO constituent invariant.
    public static Fin<MaterialComposition> ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction, string PartName)> constituents, Op key) =>
        MaterialComposition.OfConstituentSet(constituents.Map(static c => new MaterialConstituent(c.Material, c.Category, c.Fraction, c.PartName)), key);
}

// A component's material truth is a weighted composition exactly when a RECIPE declares it — the standardized mix
// proportion or the product's own layup. A recipe is caller data resolved per material at the capture root, never
// a roster column.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConstituentRecipe {
    private ConstituentRecipe() { }
    public sealed record Mix(MixSpec Spec) : ConstituentRecipe;
    public sealed record Layup(Seq<(MaterialId Material, double ThicknessMm, string Category)> Layers) : ConstituentRecipe;
}

// The ONE constituent-row producer both recipe arms fold through; it emits raw mass-fraction rows and DELEGATES
// the normalization algebra to the seam through CompositionAuthor.ConstituentSet.
public static class Constituents {
    public static Fin<Seq<(MaterialId Material, string Category, double Fraction, string PartName)>> Of(ConstituentRecipe recipe, Op key) =>
        recipe.Switch(
            state: key,
            mix:   static (k, m) => MixDesign.Proportion(m.Spec, k).Map(receipt => MixRows(m.Spec.Materials, receipt)),
            layup: static (k, l) => LayupRows(l.Layers, k));

    // Air carries no mass and enters no row; the four rows share the receipt's own total, so the seam
    // normalization confirms rather than repairs.
    static Seq<(MaterialId Material, string Category, double Fraction, string PartName)> MixRows(MixMaterials materials, MixProportion p) {
        double total = p.CementKgM3 + p.WaterKgM3 + p.FineKgM3 + p.CoarseKgM3;
        return Seq(
            (materials.Cement, "binder", p.CementKgM3 / total, "cement"),
            (materials.Water, "water", p.WaterKgM3 / total, "water"),
            (materials.FineAggregate, "aggregate", p.FineKgM3 / total, "fine"),
            (materials.CoarseAggregate, "aggregate", p.CoarseKgM3 / total, "coarse"));
    }

    // Thickness × catalogued density per layer, normalized over the stack's own total. A ply material with no
    // density column rails NAMING the material — a fabricated mass is the refused form — and every missing ply
    // reports applicatively.
    static Fin<Seq<(MaterialId Material, string Category, double Fraction, string PartName)>> LayupRows(
        Seq<(MaterialId Material, double ThicknessMm, string Category)> layers, Op key) =>
        layers.Traverse(layer =>
                (from sets in MaterialPropertyCatalogue.Lookup(layer.Material, key)
                 from mass in sets.Density.Match(
                     Some: density => Fin.Succ(layer.ThicknessMm * 1e-3 * density.Si),
                     None: () => new ElementFault.ValueRejected(key, $"<layup-density-missing:{layer.Material.Value}>"))
                 select (layer.Material, layer.Category, MassPerArea: mass)).ToValidation())
            .As()
            .ToFin()
            .Map(rows => {
                double total = rows.Fold(0.0, static (sum, row) => sum + row.MassPerArea);
                return rows.Map((row, ordinal) => (row.Material, row.Category, row.MassPerArea / total, $"{row.Category}:{ordinal}"));
            });
}
```

## [04]-[COMPONENT_SUBGRAPH]

- Owner: the `ComponentSubgraph` capture composition root — `Capture` (the homogeneous-substance fold building `Substance` specs — the prior `MaterialSubgraph.Capture` per-material construction with the binding fan widened to the real one-material-many-elements arity, the traversal applicative) and `CaptureComponent` (the applicative catalogue-row fold building the `Type` specs) — with `CompositionOf`, the RELOCATED composition selection (the per-family `ToLayerSet`/shape-kind methods are the deleted forms, replaced by ONE read of the campaign currency: `ComponentRow.Sectioned` + the `SectionProfile.Layered` arm), and `Lowerings`, the family-keyed physics-lowering table.
- Cases: the `Substance` case projects a content-keyed material and appearance; the `Type` case projects a rooted type object, structural material, required appearance, seed-built detail bag, and explicit occurrence bindings. `Sectioned` rows select `ProfileSet`, unsectioned `Layered` rows select `LayerSet`, and every other row selects `Single`; a recipe-declared SUBSTANCE selects `ConstituentSet` — the fourth seam case's traffic, a concrete mix or a multi-substance product decomposing the EPD way. `Lowerings` rows — glazing's IGU receipts, timber's grade-sourced orthotropic stiffness, masonry's clay coursing physics, cmu's lattice physics — each restore their own seed axes and lower onto the seam property set, and the timber row is the only one whose lowering is DIRECTIONAL rather than a scalar band.
- Entry: `ComponentSubgraph.Capture` builds independent `Substance` specs with `MaterialUsage.Unbound`, and `CaptureComponent` builds Type specs whose caller supplies each occurrence usage; both take the caller's `setOf` baked-set resolver keyed on the appearance material, and `Capture` takes a second resolver, `recipeOf` — the declared weighted-composition resolver mirroring `setOf`, so a project declares which library materials are mixes or layup products and every undeclared material stays homogeneous. Both required appearance lookups remain on `Fin`.
- Packages: Rasm.Element (project — `Node` cases, `MaterialComposition`, `MaterialLayer`/`MaterialConstituent`, `MaterialUsage`/`LayerSetDirection`/`DirectionSense`/`CardinalPoint`, `MeasureValue`, `MaterialPropertySet`, `Relationship`, `Classification`, `AppearanceSummary`, `ProfileRef`), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentId`/`SectionProfile`/`Ply`/`ComponentResolution`/`ResolvedComponent`), Rasm.Materials.Properties (project — `MaterialPropertyCatalogue`/`SustainabilityCatalogue`), Rasm.Materials.Appearance.Graph + Interchange (project — `MaterialLibrary`/`AppearanceEgress`), LanguageExt.Core; the `Rasm.Materials.Construction` reference is RETIRED — `CompositionAuthor` is `[03]`, this namespace.
- Growth: a new engineering discipline routed to a material is one seam `Discipline` row the `MaterialPropertySet` carries — no capture arm; a new family's Type capture is ALREADY total (a new `ComponentFamily` row's components flow through the same `Sectioned`/`Layered`/`Single` law with zero edits here); a new family physics lowering REPLACES that family's declared-None `Lowerings` row with its seed page's own restore-and-lower pair — the table is TOTAL over `ComponentFamily.Items`, so a family with no physics states that rather than reaching a lookup fallback a new family silently inherits; a new composition shape is one `CompositionOf` arm over the new seam case — the subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: `CompositionOf` reads only `Sectioned` and `SectionProfile`; a solved section always selects `ProfileSet`, while a `Layered` profile maps its bounded role currency to the seam's string `Name` only at `CompositionAuthor.LayerSet`. Required material facts rail missing keys, lifecycle facts remain optional, component detail stays seed-built, and the additive `GraphDelta` passes through the seam's second interface `IGraphConstraint.Validate` — the `Rasm.Bim`-implemented IFC-semantic legality gate — before folding, this capture enforcing only the structural invariants it owns so the two interfaces stay orthogonal. `Rasm.Materials` authors NO `Assessment` node: the `Discipline`-keyed `MaterialPropertySet` set carried on the projected `Material` node IS the assessment input, which `Rasm.Compute` reads directly above the seam, routes by discipline, and answers with a seam `Assessment` `Result` node content-keyed on the input key and route — the multi-ply `AssemblyAggregator` being `Rasm.Compute`'s as well. `Lowerings` is TOTAL over the family roster and a family with no seam-carriable physics carries an EXPLICIT None row, so a new family cannot project physics-free by omission — the whole difference between a declared absence and a lookup miss. Every populated row DELEGATES: the design-code computation stays its seed page's, so this table restores a typed row and calls one lowering, and a family whose row cannot restore its own axes rails `ProjectionFault.Unresolved` rather than lowering a partial receipt — an incomplete-table bug surfaced, never a silently thinned property set.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The capture composition root: fold the catalogues into the ProjectionSource the ComponentProjector folds. The
// TYPE capture takes catalogue ComponentRows and DERIVES each composition from the row currency. The M7
// ProfileRef→ResolvedComponent table is supplied once by the caller, so the section integral runs once per
// component rather than per material.
public static class ComponentSubgraph {
    // `setOf` resolves the baked set behind an APPEARANCE material — the same resolver the Type capture reads
    // through each row's AppearanceId — so a caller holding the press's own products supplies one function and an
    // unbaked estate supplies the constant absence.
    public static Fin<ProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Seq<NodeId>> elementsOf,
        Func<MaterialId, Option<ContentAddress>> setOf,
        Func<MaterialId, Option<ConstituentRecipe>> recipeOf,
        FrozenDictionary<ProfileRef, ResolvedComponent> sections,
        Op key) =>
        materials
            .Traverse(id =>
                (from facts in MaterialFacts.Lookup(id, key)
                 // A recipe-declared material is a weighted composition; every other library material stays
                 // homogeneous Single. Layered and profiled compositions ride a Type spec's row derivation.
                 from composition in recipeOf(id).Match(
                     Some: recipe => Constituents.Of(recipe, key).Bind(rows => CompositionAuthor.ConstituentSet(rows, key)),
                     None: () => Fin.Succ(CompositionAuthor.Single(id)))
                 let elements = elementsOf(id)
                 let bindings = elements.Map(element => new MaterialBinding(element, new MaterialUsage.Unbound(), facts.Classification))
                 from row in MaterialLibrary.Lookup(id, key)
                 from appearance in AppearanceEgress.Summary(row, key)
                 select new ProjectionSpec.Substance(id, composition, facts.Properties, appearance, setOf(id), row.ThinWalled, bindings)).ToValidation())
            .As()
            .Map(specs => specs.Fold(ProjectionSource.Empty with { Sections = sections }, static (source, spec) => source.Add(spec)))
            .ToFin();

    // The Type capture pairs catalogue rows with vouched occurrence bindings and requires both material facts and appearance.
    public static Fin<ProjectionSource> CaptureComponent(
        ProjectionSource source, Seq<(ComponentRow Row, Seq<OccurrenceBinding> Occurrences)> rows,
        Func<MaterialId, Option<ContentAddress>> setOf, Op key) =>
        rows.Traverse(entry =>
                (from composition in CompositionOf(entry.Row, key)
                 from facts in MaterialFacts.Lookup(entry.Row.Item.SubstanceId, key)
                 from physics in FamilyProperties(entry.Row.Item, key)
                 from row in MaterialLibrary.Lookup(entry.Row.Item.AppearanceId, key)
                 from appearance in AppearanceEgress.Summary(row, key)
                 select new ProjectionSpec.Type(entry.Row.Item, composition, facts.Properties + physics, appearance,
                     setOf(entry.Row.Item.AppearanceId), facts.Classification, row.ThinWalled, entry.Occurrences)).ToValidation())
            .As()
            .Map(specs => specs.Fold(source, static (acc, spec) => acc.Add(spec)))
            .ToFin();

    // Rows lower per-family physics — ONE per family whose seed page computes a receipt the projected Material
    // node's property set can carry, each composing that page's OWN restore-and-lower pair rather than re-reading
    // its axes here: glazing the EN 673 / EN 410 / mass-law IGU receipts, timber the EN 338 / EN 14080 grade-sourced
    // orthotropic stiffness — the ONE DIRECTIONAL row, carrying the E0,05 fractile the EN 1995-1-1 §6.3.2/§6.3.3
    // stability kernels take and nothing above the seam can reconstruct — masonry the EN 1745 / IBC 722.4 clay
    // physics, cmu the ACI 216.1 / NCMA TEK 6-2C lattice physics. Capture stays ONE family-blind fold. Lazy
    // supplies the materialization edge a SmartEnum-keyed index requires, since an eager initializer reading a
    // family static captures null before its type init completes.
    static readonly Lazy<FrozenDictionary<ComponentFamily, Func<Component, Op, Fin<Seq<MaterialPropertySet>>>>> Lowerings =
        new(static () => new Dictionary<ComponentFamily, Func<Component, Op, Fin<Seq<MaterialPropertySet>>>> {
            [ComponentFamily.Glazing] = static (item, key) => GlazingSeed.Resolve(item, key)
                .Bind(build => GlazingDetail.Properties(build.Panes, build.Cavities, build.FireResistanceEiMinutes, key)),
            [ComponentFamily.Timber] = static (item, key) => TimberSeed.Resolve(item, key)
                .Bind(row => row.Grade.TimberArm.Match(
                    Some: arm => arm.ToProperties(key),
                    None: () => new ProjectionFault.Unresolved(key, $"<timber-grade-arm-unresolved:{item.Designation.Value}>"))),
            [ComponentFamily.Masonry] = static (item, key) => MasonrySeed.Resolve(item, key)
                .Bind(row => MasonryDetail.Properties(item.Profile, row.Body, key)),
            [ComponentFamily.Cmu] = static (item, key) => SeedJoin.Resolve(CmuSeed.Table, item.Designation, key)
                .Bind(row => item.Profile is SectionProfile.CellularRectangle cell
                    ? CmuSeed.Properties(row, cell, key)
                    : new ProjectionFault.Unresolved(key, $"<cmu-lattice-unresolved:{item.Designation.Value}>")),
            // Every remaining family declares NONE explicitly, which is what makes the absence a DECISION: a
            // lookup miss and a family with no physics read identically through a TryGetValue fallback, so a new
            // family landing without its lowering projected silently physics-free. Seeding from
            // ComponentFamily.Items and filling only the gaps means the table cannot be short.
        }.Concat(ComponentFamily.Items.Select(static family =>
                KeyValuePair.Create(family, (Func<Component, Op, Fin<Seq<MaterialPropertySet>>>)Barren)))
            .DistinctBy(static entry => entry.Key)
            .ToFrozenDictionary());

    // The declared-absent lowering: a family whose seed page computes no seam-carriable receipt.
    static Fin<Seq<MaterialPropertySet>> Barren(Component item, Op key) => Fin.Succ(Seq<MaterialPropertySet>());

    // TOTAL by construction over the roster, so the read carries no fallback of its own — a miss here would be
    // a roster that changed under a frozen table, which is a type-init question and not a per-component one.
    static Fin<Seq<MaterialPropertySet>> FamilyProperties(Component item, Op key) =>
        Lowerings.Value[item.Family](item, key);

    // ONE law over the campaign currency, replacing every per-family ToLayerSet/shape-kind method: a Sectioned row
    // is a ProfileSet, since its solved section MUST have one to bake onto; an unsectioned Layered row becomes a
    // LayerSet whose names derive at the IFC boundary from each ply's material, bounded role, and stable ordinal;
    // every other row is a Single discrete part.
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
