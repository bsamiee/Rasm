# [MATERIALS_PROJECTION]

THE COMPONENT PROJECTOR, THE SEAM COMPOSITION AUTHOR, and THE COMPONENT-SUBGRAPH CAPTURE. `ComponentProjector.Project` folds payload-complete `Substance` and `Type` cases onto one `Fin<GraphDelta>`. Every appearance is required by the captured spec, and every `OccurrenceBinding` carries its explicit `MaterialUsage`; layer direction, offset, extent, and profile cardinal placement never derive from type-level composition. `CompositionAuthor` builds `MaterialComposition`, and `ComponentSubgraph` selects `ProfileSet`, `LayerSet`, or `Single` from the component row.

## [01]-[INDEX]

- [02]-[COMPONENT_PROJECTOR]: the `ComponentProjector` `IElementProjection` owner, the `ComponentProjectionSource` aggregate, the payload-complete `ComponentProjectionSpec` union, `MaterialBinding`/`OccurrenceBinding`, the `Project` fold, the `ProjectionFault` rail, the content mints, the M7 section bake, and the binding vouches.
- [03]-[COMPOSITION_AUTHOR]: the seam-`MaterialComposition` `Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet` builders.
- [04]-[COMPONENT_SUBGRAPH]: the `ComponentSubgraph` capture composition root — the homogeneous-substance `Capture` and the `CaptureComponent` Type capture whose composition selection reads the `ComponentRow` `Sectioned` pin and the `SectionProfile.Layered` arm.

## [02]-[COMPONENT_PROJECTOR]

- Owner: `ComponentProjector` is the sealed `IElementProjection`; `ComponentProjectionSource` carries the spec stream and section table; `ComponentProjectionSpec` carries each modality's complete payload; `MaterialBinding` carries substance-path usage and classification; `OccurrenceBinding` carries a vouched Type occurrence and its required usage.
- Cases: `Substance(MaterialId, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Seq<MaterialBinding>)` and `Type(Component, MaterialComposition, Seq<MaterialPropertySet>, AppearanceSummary, Option<Classification>, Seq<OccurrenceBinding>)`.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam op: `source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()` — specs are INDEPENDENT, so the fold is APPLICATIVE (the EXACT seam `Assemble` shape, `Traverse`→`ToValidation`→`Merge`-fold per the seam `[APPLICATIVE_CAPTURE]` law — never a hand-threaded accumulator, never a first-fault-only `TraverseM`) — every failing spec reports (an unvouched binding/occurrence `ProjectionFault.Unvouched`, a malformed Type `Classification` the seam `ElementFault` lifts unchanged through `Classification.Of`), the accumulated `ManyErrors` lowering onto ONE `Fin<GraphDelta>` whose success folds the per-spec deltas through the cancellation-correct `GraphDelta.Merge` MONOID; `ProjectSpec` discriminates via the generated total `Switch` — `Substance`→`ProjectSubstance`, `Type`→`ProjectType`; `ComponentProjector.Of(source)` captures the source once, and the seam `Assemble(ProjectionSuite.Of(…), seed, ctx)` re-merges this delta with `Rasm.Bim`'s `SemanticProjector` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — the seam: `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`RepresentationContentHash`/`SchemaSpan`/`OwnerHistory`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`MaterialId`, plus `FaultBand` the band registry), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`SectionProfile`/`ComputedSection`/`ResolvedComponent`/`QuantityRow`, the standardized-type owner whose `IfcBinding` forwarders and typed-mint rows this projector reads), Rasm.Domain (project — `Op`; the seed-zero `XxHash128` content seed is the seam `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` + generated total `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`ToValidation`/`ToFin`/`Fold`/`Option`); cite `libs/csharp/.api/api-thinktecture-runtime-extensions.md` — the `Rasm.Materials/.api` VividOrange catalogues are the `component#COMPONENT_OWNER`'s, not composed here (the projector reads an already-resolved `ComputedSection`, never the section solver).
- Growth: a new projected node kind is one seam `Node` case, a new spec modality one `ComponentProjectionSpec` case, and a new occurrence-usage shape one seam `MaterialUsage` case carried by `OccurrenceBinding`.
- Boundary: each Type occurrence is vouched independently and binds through `Assign.TypeDefinition` plus its explicit occurrence-to-material usage. `MaterialLibrary.Lookup(...).Map(MaterialWire.Summary)` remains required on `Fin`; no optional appearance state survives inside the spec.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation failure slot the applicative spec/vouch folds accumulate
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element.Classification;   // Classification — the seam standard-reference value
using Rasm.Element.Composition;      // MaterialComposition, MaterialPropertySet, SectionProperties, ProfileRef, MaterialId
using Rasm.Element.Graph;            // GraphDelta, Node, NodeId, ObjectKind, PredefinedType, RepresentationContentHash, SchemaSpan, OwnerHistory, AppearanceSummary
using Rasm.Element.Projection;       // IElementProjection, ProjectionContext, FaultBand
using Rasm.Element.Properties;       // PropertyBag, PropertyName, PropertyValue
using Rasm.Element.Relations;        // Relationship, AssignKind, MaterialUsage
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Interchange;
using Rasm.Materials.Component;      // Component, ComponentRow, SectionProfile, ComputedSection, ResolvedComponent, QuantityRow
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
        Seq<MaterialBinding> Bindings) : ComponentProjectionSpec;

    public sealed record Type(
        Component Component,
        MaterialComposition Composition,
        Seq<MaterialPropertySet> Properties,
        AppearanceSummary Appearance,
        Option<Classification> StandardClassification,   // the facts-resolved standard reference the Type Object's Classifications set carries — Type-seed-excluded, so the stamp never re-keys NodeId.RootedType
        Seq<OccurrenceBinding> Occurrences) : ComponentProjectionSpec;
}

// The captured projection source carries the closed spec stream and the resolved section table.
public sealed record ComponentProjectionSource(Seq<ComponentProjectionSpec> Specs, FrozenDictionary<ProfileRef, ResolvedComponent> Sections) {
    public static readonly ComponentProjectionSource Empty = new(Seq<ComponentProjectionSpec>(), FrozenDictionary<ProfileRef, ResolvedComponent>.Empty);
    public ComponentProjectionSource Add(ComponentProjectionSpec spec) => this with { Specs = Specs.Add(spec) };
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Materials folder publishes. Captures the source internally (the source-capture
// inversion) so the seam op carries only the ProjectionContext; the seam Assemble merges this delta with every sibling.
public sealed class ComponentProjector : IElementProjection {
    readonly ComponentProjectionSource source;
    ComponentProjector(ComponentProjectionSource source) => this.source = source;
    public static ComponentProjector Of(ComponentProjectionSource source) => new(source);

    // Traverse each spec to its OWN delta APPLICATIVELY — specs are independent, so every failing spec reports
    // and ONE Fin carries the accumulated ManyErrors (the EXACT seam Assemble shape: Traverse -> ToValidation ->
    // Merge-fold, the seam [APPLICATIVE_CAPTURE] law; TraverseM's first-fault abort is the rejected disposition).
    // Each spec builds on GraphDelta.Empty so per-spec projection is decoupled from the running delta.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)))
            .ToFin();

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
        from composition in BakeSection(spec.Composition, ctx.Key)
        let tolerance = ctx.Header.Tolerance
        let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, composition, spec.Properties), tolerance)
        let appearance = Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Appearance), tolerance)
        let withNodes = GraphDelta.Empty.Put(material).Put(appearance)
        from bound in AuthorBindings(spec, material.Id, appearance.Id, ctx, withNodes)
        select bound;

    // --- [TYPE_FOLD]
    // The standardized-Component subgraph: MINT the deterministic-rooted Type Object, lower the structural
    // Material node (the SubstanceId material with the M7 section baked on), the required Appearance, and the
    // SEED-BUILT detail bag read STRAIGHT off c.Detail (the deleted Detail(Component) switch's totality is now the
    // Component.Of lane/detail type law — a None-lane family carries no bag, a Realization/Product family always
    // does), wire the Type→resource edges (both endpoints owned — no vouch), then bind every VOUCHED occurrence via
    // Assign.TypeDefinition PLUS its own occurrence→material Associate carrying the binding's OWN explicit
    // MaterialUsage ([C7] — usage is input data on each OccurrenceBinding, never derived from the type-level
    // composition). Classification.Of is the seam Fin admission; AuthorOccurrences the fallible tail.
    Fin<GraphDelta> ProjectType(ComponentProjectionSpec.Type spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Component c = spec.Component;
        return
            from classification in Classification.Of("ifc", c.IfcEntity, ctx.Key)
            from baked in BakeSection(spec.Composition, ctx.Key)
            let type = MintType(c, classification, spec.StandardClassification, ctx)
            let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), c.SubstanceId, baked, spec.Properties), tolerance)
            let appearance = Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Appearance), tolerance)
            let detail = c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))
            let seeded = SeedType(type, material, appearance, detail)
            from bound in AuthorOccurrences(spec.Occurrences, type.Id, material.Id, ctx, seeded)
            select bound;
    }

    // MINT the deterministic-rooted Type Object: a ROOTED identity DERIVED from the Component's canonical content
    // through NodeId.RootedType over Node.Object.ToTypeSeedBytes (which EXCLUDES the volatile Representations AND the
    // secondary Classifications set, so a later geometry attach or a standard-classification stamp never re-keys and
    // identical Components dedup to one Type). Kind is the REUSED ObjectKind.Type static; the Classification/
    // PredefinedType stamp reads the IfcBinding forwarders (the seed-computed row data; AdmitPredefined validity is
    // Rasm.Bim's); the facts-resolved standard reference rides the Classifications set directly (the prior deliberate
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
    // the required Appearance, and the optional seed-built detail bag, plus the Type→Material / Type→Appearance
    // Associate edges (MaterialUsage.None — the TYPE-level association carries no per-occurrence usage; occurrence
    // usage rides the occurrence's own binding [C7]) and the Type→detail Assign.PropertyDefinition (occurrences
    // inherit through the Bake type-bag merge). Both endpoints are owned here, so no vouch gates these edges.
    static GraphDelta SeedType(Node type, Node material, Node appearance, Option<Node> detail) {
        GraphDelta withAppearance = GraphDelta.Empty.Put(type).Put(material).Put(appearance)
            .Link(new Relationship.Associate(type.Id, material.Id, new MaterialUsage.None()));
        withAppearance = withAppearance.Link(new Relationship.Associate(type.Id, appearance.Id, new MaterialUsage.None()));
        return detail.Match(
            Some: d => withAppearance.Put(d).Link(new Relationship.Assign(type.Id, d.Id, AssignKind.PropertyDefinition)),
            None: () => withAppearance);
    }

    // --- [SECTION_BAKE]
    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured component#COMPONENT_RESOLUTION table and
    // BAKE the neutral seam SectionProperties onto the composition (WithSection), so the structural runner reads
    // graph.SectionOf(member) without re-resolving or admitting VividOrange. A non-ProfileSet bakes nothing,
    // total; a ProfileSet ref present with a None section bakes nothing; a ProfileSet ref ABSENT from the table
    // rails ProjectionFault.Unresolved — the M7 cache is total over every catalogued component, so an absent ref
    // is a caller-supplied incomplete-table bug surfaced, never a silently-dropped section. Shared by both folds.
    Fin<MaterialComposition> BakeSection(MaterialComposition composition, Op key) =>
        composition is MaterialComposition.ProfileSet ps
            ? source.Sections.TryGetValue(ps.Profile, out ResolvedComponent resolved)
                ? resolved.Section.Match(
                    Some: section => SeamSection(section).Map(composition.WithSection),
                    None: () => Fin.Succ(composition))
                : ProjectionFault.Unresolved(key, $"<profile-ref-unresolved:{ps.Profile.Designation}>")
            : Fin.Succ(composition);

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
    // under re-projection.
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

- Owner: the `ComponentSubgraph` capture composition root — `Capture` (the homogeneous-substance fold building `Substance` specs — the prior `MaterialSubgraph.Capture` per-material construction with the binding fan widened to the real one-material-many-elements arity, the traversal applicative) and `CaptureComponent` (the applicative catalogue-row fold building the `Type` specs) — plus `CompositionOf`, the RELOCATED composition selection: the per-family `ToLayerSet`/shape-kind methods are the deleted forms, replaced by ONE read of the campaign currency (`ComponentRow.Sectioned` + the `SectionProfile.Layered` arm).
- Cases: the `Substance` case projects a content-keyed material and appearance; the `Type` case projects a rooted type object, structural material, required appearance, seed-built detail bag, and explicit occurrence bindings. `Sectioned` rows select `ProfileSet`, unsectioned `Layered` rows select `LayerSet`, and every other row selects `Single`.
- Entry: `ComponentSubgraph.Capture` builds independent homogeneous `Substance` specs with `MaterialUsage.None`, and `CaptureComponent` builds Type specs whose caller supplies each occurrence usage. Both required appearance lookups remain on `Fin`.
- Packages: Rasm.Element (project — `Node` cases, `MaterialComposition`, `MaterialLayer`/`MaterialConstituent`, `MaterialUsage`/`LayerSetDirection`/`DirectionSense`/`CardinalPoint`, `MeasureValue`, `MaterialPropertySet`, `Relationship`, `Classification`, `AppearanceSummary`, `ProfileRef`), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentId`/`SectionProfile`/`Ply`/`ComponentResolution`/`ResolvedComponent`), Rasm.Materials.Properties (project — `MaterialPropertyCatalogue`/`SustainabilityCatalogue`), Rasm.Materials.Appearance.Graph + Interchange (project — `MaterialLibrary`/`MaterialWire`), LanguageExt.Core; the `Rasm.Materials.Construction` reference is RETIRED — `CompositionAuthor` is `[03]`, this namespace.
- Growth: a new engineering discipline routed to a material is one seam `Discipline` row the `MaterialPropertySet` carries — no capture arm; a new family's Type capture is ALREADY total (a new `ComponentFamily` row's components flow through the same `Sectioned`/`Layered`/`Single` law with zero edits here); a new composition shape is one `CompositionOf` arm over the new seam case — the subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: `CompositionOf` reads only `Sectioned` and `SectionProfile`; a solved section always selects `ProfileSet`, while a `Layered` profile maps its bounded role currency to the seam's string `Name` only at `CompositionAuthor.LayerSet`. Required material facts rail missing keys, lifecycle facts remain optional, component detail stays seed-built, and the additive `GraphDelta` passes through `IGraphConstraint.Validate` before folding.

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
    public static Fin<ComponentProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Seq<NodeId>> elementsOf,
        FrozenDictionary<ProfileRef, ResolvedComponent> sections,
        Op key) =>
        materials
            .Traverse(id =>
                (from facts in MaterialFactsCatalogue.Lookup(id, key)
                 let composition = CompositionAuthor.Single(id)                                       // a homogeneous library material; layered/profiled compositions ride a Type spec's row derivation
                 let elements = elementsOf(id)
                 let bindings = elements.Map(element => new MaterialBinding(element, new MaterialUsage.None(), facts.Classification))
                 from appearance in MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary)
                 select new ComponentProjectionSpec.Substance(id, composition, facts.Properties, appearance, bindings)).ToValidation())
            .As()
            .Map(specs => specs.Fold(ComponentProjectionSource.Empty with { Sections = sections }, static (source, spec) => source.Add(spec)))
            .ToFin();

    // The Type capture pairs catalogue rows with vouched occurrence bindings and requires both material facts and appearance.
    public static Fin<ComponentProjectionSource> CaptureComponent(
        ComponentProjectionSource source, Seq<(ComponentRow Row, Seq<OccurrenceBinding> Occurrences)> rows, Op key) =>
        rows.Traverse(entry =>
                (from composition in CompositionOf(entry.Row, key)
                 from facts in MaterialFactsCatalogue.Lookup(entry.Row.Item.SubstanceId, key)
                 from igu in IguProperties(entry.Row.Item, key)
                 from appearance in MaterialLibrary.Lookup(entry.Row.Item.AppearanceId, key).Map(MaterialWire.Summary)
                 select new ComponentProjectionSpec.Type(entry.Row.Item, composition, facts.Properties + igu, appearance, facts.Classification, entry.Occurrences)).ToValidation())
            .As()
            .Map(specs => specs.Fold(source, static (acc, spec) => acc.Add(spec)))
            .ToFin();

    // The glazing arm's IGU evidence: a ComponentFamily.Glazing row restores its typed build through
    // GlazingSeed.Resolve and lowers the EN 673/EN 410/mass-law receipts through GlazingDetail.Properties onto the
    // projected Material node's property set — the capture stays ONE family-blind fold, the IGU physics joining the
    // substance facts as DATA (every other family contributes the empty set, never a per-family capture branch).
    static Fin<Seq<MaterialPropertySet>> IguProperties(Component item, Op key) =>
        item.Family == ComponentFamily.Glazing
            ? GlazingSeed.Resolve(item, key).Bind(build => GlazingDetail.Properties(build.Panes, build.Cavities, build.FireResistanceEiMinutes, key))
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

- [PROJECTOR_MERGE]: `ComponentProjector.Project` folds the payload-complete `ComponentProjectionSpec.Substance` and `ComponentProjectionSpec.Type` cases. `ComponentProjectionSource.Add` is the single source mutation, and each Type occurrence carries `OccurrenceBinding(Element, Usage)` so the occurrence-to-material association is explicit.
- [OWNER_MINTS_TYPE_IDENTITY]: `ProjectType` MINTS the deterministic-rooted Type `Object` — `NodeId.RootedType` over `Node.Object.ToTypeSeedBytes` (representation- and secondary-classification-excluded, so a later geometry attach or standard-classification stamp never re-keys and identical `Component`s dedup to one Type), the REUSED `ObjectKind.Type` static, `Assign.TypeDefinition` for occurrences (never an IFC `DefinesByType` spelling). The stamp reads the `IfcBinding` forwarders (`c.IfcEntity`/`c.PredefinedToken`) — seed-computed ROW DATA under the brief IFC-binding seed law, so the projector body is family-blind; roster validity is `Rasm.Bim`'s composition-time `IfcLegality` arm and per-token `AdmitPredefined` egress gate.
- [REGISTRY_BAND]: `ProjectionFault.Code => FaultBand.Projection` consumes the `Rasm.Element` `FaultBand` registry through the generated `SmartEnum`-to-`int` conversion — one line, never a `.Value` spelling; a duplicate band integer fails at type initialization, so the 2470 disjointness the prior prose recited is type-enforced. The unmarked-union (no `[GenerateUnionOps]`)/`…Case`/bare-lift shape is unchanged.
- [QUANTITY_ROW_LIFT]: `SeamSection : ComputedSection -> Fin<SectionProperties>` sequences every `QuantityRow.OfSi` mint before constructing the neutral section. The named arguments preserve all twenty columns and prevent a failed finite or dimensional admission from being hidden inside a successful projection.
- [SEED_TIME_DETAIL]: the `Detail(Component)` switch and its `Joint`/`Token`/`Measured`/`RealizationRows`/`ProductRows` constructors are DELETED from this page — each family's bag is built AT SEED TIME through the relocated `component#COMPONENT_DETAIL` `ComponentDetail` owner (same bodies, same dimension-only `MeasureValue.OfSi(dim, si)` mints, same `DetailSchema.Realization`/`DetailSchema.Product` seeds), so the bag rows, `PropertyBag` content, and projected `Node.PropertySet` bytes are identical while the projector reads `c.Detail` with zero per-family knowledge; the `Component.Of` lane/detail law (a `None`-lane family carries no bag, a `Realization`/`Product` family always does) carries the deleted switch's totality as a type law. The IMPORT round-trip still splits by element genus: the four realizing-element families through the `Rasm.Bim` connection-detail reader targeting `DetailSchema.Realization`, panel product detail through the general Bim object/property fold under `DetailSchema.Product`.
- [COMPOSITION_SELECTION]: `CompositionOf` derives `ProfileSet`, `LayerSet`, or `Single` from row currency. A layered ply retains its bounded `PlyRole`; the IFC-facing name is a deterministic projection of role, ordinal, and material rather than a second role vocabulary.
- [CLASS_ROOT_MINT]: `Mint` composes the seam `Node.Relabel` re-stamp — a class-root `[Union]` `Node` case has NO compiler-generated `with`, so the prior `draft with { Id = … }` is the deleted form; the content-derived id re-stamps through `Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span))`, the kernel seed-zero `XxHash128` the seam `ContentAddress` composes — the ONE hasher, no second in this folder. The cross-runtime C#/Python/TypeScript parity corpus pins byte-for-byte agreement on the canonical bytes.
- [H12_OCCURRENCE_VOUCH]: `ProjectionContext.Owns` vouches each substance binding and Type occurrence applicatively. The Type path authors `Assign.TypeDefinition` and the occurrence-to-material `Associate` edge with the binding's explicit `MaterialUsage`; no composition-derived placement default exists.
- [ASSESSMENT_OWNERSHIP]: `Rasm.Materials` authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the assessment input `Rasm.Compute` reads DIRECTLY (above the seam), runs the discipline route, and writes the seam `Assessment` `Result` node content-keyed on `(input key, route)`; the multi-ply `AssemblyAggregator` is `Rasm.Compute`'s.
- [IGRAPH_CONSTRAINT_GATE]: the projected `GraphDelta` is gated by the seam's second interface `IGraphConstraint.Validate` (`Rasm.Bim`-implemented — IFC-semantic legality plus the new vocabulary arms: a `Type` may not aggregate an `Occurrence`, a classified node must resolve a roster row, a predefined token must be in the entity's valid set) before the seam folds it; the projector enforces only the STRUCTURAL invariants it owns, so the two interfaces stay orthogonal.
