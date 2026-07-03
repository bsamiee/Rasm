# [MATERIALS_PROJECTION]

THE COMPONENT PROJECTOR, THE SEAM COMPOSITION AUTHOR, and THE COMPONENT-SUBGRAPH CAPTURE. `Rasm.Materials` lowers onto the shared `Rasm.Element` seam through ONE `ComponentProjector` — the merge of the prior `MaterialProjector` and `ConnectionProjector` into a single `IElementProjection` whose one `Fin<GraphDelta> Project(ProjectionContext ctx)` folds a `Seq<ComponentProjectionSpec>` discriminating the pure-substance `MaterialSpec` path from the Type-minting `ComponentSpec` path. Under the owner-mints-its-identity law a `Type` spec MINTS the deterministic-rooted Type `Object` from the `Component`'s canonical content (`NodeId.RootedType` over `Node.Object.ToTypeSeedBytes`, which EXCLUDES the volatile `Representations` so a later geometry attach never re-keys the type), stamps `Classification("ifc", c.IfcEntity)` + `PredefinedType.Create(c.PredefinedToken)` off the `Component`'s `IfcBinding` forwarders (the stored ROW DATA that replaced the deleted `ComponentSection` egress switches; validity stays the `Rasm.Bim` roster + `AdmitPredefined` gate, never a seam invariant), bakes the M7 `ComputedSection` onto the structural material's `ProfileSet` composition through `SeamSection` — whose every typed lift now composes the `component#QUANTITY_ROW` `QuantityRow` rows, the ONE bounded mint site, byte-identical content keys — and reads the SEED-BUILT detail bag straight off `Component.Detail` (the cross-file `Detail(Component)` switch is DELETED: each family's bag is constructed at seed time by `component#COMPONENT_DETAIL` `ComponentDetail`, so the projector carries zero per-family knowledge). `ProjectionFault` consumes its band through the `Rasm.Element` `FaultBand` registry (`Code => FaultBand.Projection` — duplicate band integers fail at type initialization, never prose-enforced disjointness). This page ALSO owns `CompositionAuthor` — the seam-`MaterialComposition` author absorbed whole from the deleted `Construction/assembly.md` (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet` coerce-and-delegate builders plus the `[C7]` `UsageOf` occurrence-usage derivation), re-homed to `Rasm.Materials.Projection` so the capture composes it in-namespace and the `using Rasm.Materials.Construction` retires — and the `ComponentSubgraph` capture whose `CaptureComponent` DERIVES each row's composition from the campaign currency itself: a `Sectioned` row is a `ProfileSet` (the M7 bake target), an unsectioned `SectionProfile.Layered` row is a `LayerSet` built from its role-tagged `Ply` rows (CLT, IGU, board build — the relocated family `ToLayerSet` selection), every other row a `Single` discrete part. Identity is the ONE content rail: `Material`/`Appearance`/detail nodes are `NodeId.Content` over `ToCanonicalBytes` (the kernel seed-zero `XxHash128` the seam `ContentAddress` composes — no second hasher in this folder), the Type `Object` `NodeId.RootedType` over its representation-excluded seed, so identical `Component`s dedup to one Type and the delta is idempotent under re-projection. The page composes the `Rasm.Element` seam contracts, the `component#COMPONENT_OWNER` owner (`Component`/`ComponentRow`/`SectionProfile`/`ComputedSection`/`ResolvedComponent`/`QuantityRow`), and the Properties/Appearance catalogues; it re-mints NO seam type and authors NO IFC entity — the seam graph IS the wire `Rasm.Bim` reads one-hop.

## [01]-[INDEX]

- [02]-[COMPONENT_PROJECTOR]: the `ComponentProjector` `IElementProjection` owner, the `ComponentProjectionSource`/`ComponentProjectionSpec`/`MaterialSpec`/`ComponentSpec`/`MaterialBinding` captured-source records (with the M7 `ProfileRef`→`ResolvedComponent` table), the `Project`/`ProjectSpec`/`ProjectSubstance`/`ProjectType` fold, the `ProjectionFault` registry-banded rail, `MintType`/`SeedType`/`Mint`, the `BakeSection` M7 resolve, the `SeamSection` twenty-field lift over `QuantityRow`, and the `[H12]` `AuthorBindings`/`AuthorOccurrences` vouches.
- [03]-[COMPOSITION_AUTHOR]: the absorbed seam-`MaterialComposition` author — `Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet` coerce-and-delegate builders and the `[C7]` `UsageOf` `MaterialUsage` derivation.
- [04]-[COMPONENT_SUBGRAPH]: the `ComponentSubgraph` capture composition root — the homogeneous-substance `Capture` and the `CaptureComponent` Type capture whose composition selection reads the `ComponentRow` `Sectioned` pin and the `SectionProfile.Layered` arm.

## [02]-[COMPONENT_PROJECTOR]

- Owner: `ComponentProjector` the sealed `IElementProjection` merging the prior `MaterialProjector` and `ConnectionProjector`; `ComponentProjectionSource` the captured-source aggregate (the `Seq<ComponentProjectionSpec>` + the M7 `ProfileRef`→`ResolvedComponent` table); `ComponentProjectionSpec` the `[Union]` discriminating `Substance(MaterialSpec)` from `Type(ComponentSpec)`; `MaterialSpec` the pure-substance unit (KEPT); `ComponentSpec` the Type-minting unit; `MaterialBinding` the element-occurrence binding; `MaterialFactsCatalogue` the engineering+lifecycle+classification join; `ProjectionFault` the `[Union]` reading `FaultBand.Projection`; `Mint`/`MintType` the content-id and deterministic-Type-id mints.
- Cases: one `ComponentProjector` over a `Seq<ComponentProjectionSpec>` — a `Substance` arm carrying a `MaterialSpec` (`MaterialId` + seam `MaterialComposition` + the `Discipline`-keyed `MaterialPropertySet` set + optional `AppearanceSummary` + `Seq<MaterialBinding>`), a `Type` arm carrying a `ComponentSpec` (a `Component` + the row-derived composition + the `SubstanceId` property set + optional `AppearanceSummary` + `Seq<NodeId>` vouched occurrences bound through `Assign.TypeDefinition`); never a per-family or per-discipline projector sibling — one projector folds the whole subgraph, the discriminant the spec union.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam op: `source.Specs.Traverse(spec => ProjectSpec(spec, ctx).ToValidation()).As()` — specs are INDEPENDENT, so the fold is APPLICATIVE (the EXACT seam `Assemble` shape, `Traverse`→`ToValidation`→`Merge`-fold per the seam `[APPLICATIVE_CAPTURE]` law — never a hand-threaded accumulator, never a first-fault-only `TraverseM`) — every failing spec reports (an unvouched binding/occurrence `ProjectionFault.Unvouched`, a malformed Type `Classification` the seam `ElementFault` lifts unchanged through `Classification.Of`), the accumulated `ManyErrors` lowering onto ONE `Fin<GraphDelta>` whose success folds the per-spec deltas through the cancellation-correct `GraphDelta.Merge` MONOID; `ProjectSpec` discriminates via the generated total `Switch` — `Substance`→`ProjectSubstance`, `Type`→`ProjectType`; `ComponentProjector.Of(source)` captures the source once, and the seam `Assemble(projectors, constraints, seed, ctx)` re-merges this delta with `Rasm.Bim`'s `SemanticProjector` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — the seam: `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`RepresentationContentHash`/`SchemaSpan`/`OwnerHistory`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`MaterialId`, plus `FaultBand` the band registry), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`SectionProfile`/`ComputedSection`/`ResolvedComponent`/`QuantityRow`, the standardized-type owner whose `IfcBinding` forwarders and typed-mint rows this projector reads), Rasm.Domain (project — `Op`; the seed-zero `XxHash128` content seed is the seam `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` + generated total `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Traverse`/`ToValidation`/`ToFin`/`Fold`/`Option`); cite `libs/csharp/.api/api-thinktecture-runtime-extensions.md` — the `Rasm.Materials/.api` VividOrange catalogues are the `component#COMPONENT_OWNER`'s, not composed here (the projector reads an already-resolved `ComputedSection`, never the section solver).
- Growth: a new projected node kind is one seam `Node` case the matching arm authors; a new spec modality one `ComponentProjectionSpec` case the `Project` `Switch` dispatches; a new occurrence-usage shape one seam `MaterialUsage` case the `[C7]` author derives, passed through unread; a new seam quantity one `QuantityRow` row on `component#QUANTITY_ROW` this page merely composes; a new projection fault one `ProjectionFault` case — never a second projector, never a per-family arm, never a Materials→IFC carrier beside the graph.
- Boundary: `ComponentProjector` is the ONE Materials `IElementProjection` — the dual `MaterialProjector`/`ConnectionProjector` paradigm is the deleted form. The `Substance` path is the prior `MaterialProjector` UNCHANGED; the `Type` path MINTS the Type `Object` (REUSING `ObjectKind.Type`, deriving `NodeId.RootedType` from the representation-excluded `ToTypeSeedBytes` seed) and binds occurrences via `Assign.TypeDefinition` (REUSED, never an IFC `DefinesByType` spelling). The Type-stamped `Classification("ifc", c.IfcEntity)` and `PredefinedType.Create(c.PredefinedToken)` read the `IfcBinding` ROW DATA every seed computed from its own token vocabulary — the three-mode `ComponentSection` egress switch is the deleted form, and the `IfcClass` roster + per-token `AdmitPredefined` validity stays `Rasm.Bim`'s egress concern. The `[H12]` vouch is per-binding/per-occurrence (`ctx.Owns`): an empty `Bindings`/`Occurrences` set traverses to zero edges and zero faults (genuinely nothing to bind), NEVER a `ctx.ElementIds`-emptiness gate — a spec that DOES carry bindings/occurrences rails `ProjectionFault.Unvouched` for EVERY element the context cannot vouch, the misses ACCUMULATED into one failure, never a silently-dropped edge and never a first-miss-only report; the Type `Object` minted here is NOT context-vouched (the owner mints its own Type identity), and the resources (structural `Material`, `Appearance`, detail bag) are content-keyed and owned by the minting projector so the Type→resource edges need no vouch. Every authored non-rooted node is CONTENT-ADDRESSED through `Mint` (`NodeId.Content(node.ToCanonicalBytes(tolerance))` + `Relabel` — a class-root `[Union]` `Node` case has no compiler `with`), so two specs minting the same node collapse to one and the delta is idempotent. A material's standard `Classification` rides the `MaterialBinding` to the bound element's `Object` node (an Object-node VALUE — NOT a `Node.Material` field, NOT an edge payload). `ProjectionFault` reads `FaultBand.Projection` off the registry (`Expected`-derived, unmarked by the kernel union-ops generator, the `…Case` factory pattern) so a typed case lifts BARE onto `Fin<T>`; disjointness with `ComponentFault`/`GeometryFault`/`MaterialFault`/`ElementFault`/`BimFault` is type-enforced at the registry, and each owner's fault lifts through unchanged. The projected `GraphDelta` is gated by the seam `IGraphConstraint.Validate` (`Rasm.Bim`-implemented) for IFC-semantic legality before the seam folds it — the projector enforces only the STRUCTURAL invariants it owns (content-key idempotence, vouched endpoints, Type-Object dedup).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;            // Error — the Validation failure slot the applicative spec/vouch folds accumulate
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element;                  // the seam: IElementProjection, ProjectionContext, GraphDelta, Node, NodeId, ObjectKind,
                                     // Classification, PredefinedType, RepresentationContentHash, SchemaSpan, OwnerHistory,
                                     // Relationship, AssignKind, MaterialUsage, MaterialComposition, MaterialPropertySet,
                                     // SectionProperties, ProfileRef, AppearanceSummary, PropertyBag, MaterialId, FaultBand
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
        from engineering in MaterialPropertyCatalogue.Lookup(id, key)
        from lifecycle in SustainabilityCatalogue.Lookup(id, key)
        from classification in SustainabilityCatalogue.Classification(id, key)
        select new MaterialFacts(engineering + lifecycle, classification);
}

// One material on the PURE-SUBSTANCE path (the prior MaterialProjector unit, UNCHANGED): the seam MaterialId, the
// [03]-authored MaterialComposition (a ProfileSet carrying the M7 ProfileRef the projector resolves and bakes),
// the Discipline-keyed property set, the optional content-keyed AppearanceSummary, and the element bindings. NO
// assessment input here — Rasm.Compute reads the projected Material node plies DIRECTLY and writes the result back.
public sealed record MaterialSpec(
    MaterialId Material,
    MaterialComposition Composition,
    Seq<MaterialPropertySet> Properties,
    Option<AppearanceSummary> Appearance,
    Seq<MaterialBinding> Bindings) {
    public static MaterialSpec Of(MaterialId material, MaterialComposition composition, Seq<MaterialPropertySet> properties) =>
        new(material, composition, properties, Option<AppearanceSummary>.None, Seq<MaterialBinding>());
}

// One Component to mint as a Type Object on the TYPE path: the component#COMPONENT_OWNER Component (carrying the
// IfcBinding IfcEntity/PredefinedToken forwarders the Type Object stamps, the SubstanceId/AppearanceId slots, and
// the SEED-BUILT Option<PropertyBag> Detail), the ROW-DERIVED structural MaterialComposition ([04] CompositionOf —
// Sectioned→ProfileSet, Layered→LayerSet, else Single), the SubstanceId property set, the optional appearance, and
// the vouched occurrence NodeIds. A pure type-authoring run carries an EMPTY Occurrences set.
public sealed record ComponentSpec(
    Component Component,
    MaterialComposition Composition,
    Seq<MaterialPropertySet> Properties,
    Option<AppearanceSummary> Appearance,
    Seq<NodeId> Occurrences) {
    public static ComponentSpec Of(Component component, MaterialComposition composition, Seq<MaterialPropertySet> properties) =>
        new(component, composition, properties, Option<AppearanceSummary>.None, Seq<NodeId>());
}

// The ONE projection spec the single Project fold discriminates: Substance (the pure-material subgraph, no Object)
// versus Type (a minted Type Object + baked-section material + seed-built detail). One Seq the projector folds —
// the MaterialProjector/ConnectionProjector dual surface collapsed onto one discriminant, never two projectors.
[Union]
public abstract partial record ComponentProjectionSpec {
    public sealed record Substance(MaterialSpec Spec) : ComponentProjectionSpec;
    public sealed record Type(ComponentSpec Spec)     : ComponentProjectionSpec;
}

// The captured projection source: the per-spec stream plus the M7 ProfileRef→ResolvedComponent table
// (component#COMPONENT_RESOLUTION, built once above the seam) the projector bakes onto ProfileSet plies. The two
// With overloads add the matching discriminant arm so a capture names the concept without re-spelling the union.
public sealed record ComponentProjectionSource(Seq<ComponentProjectionSpec> Specs, FrozenDictionary<ProfileRef, ResolvedComponent> Sections) {
    public static readonly ComponentProjectionSource Empty = new(Seq<ComponentProjectionSpec>(), FrozenDictionary<ProfileRef, ResolvedComponent>.Empty);
    public ComponentProjectionSource With(MaterialSpec spec)  => this with { Specs = Specs.Add(new ComponentProjectionSpec.Substance(spec)) };
    public ComponentProjectionSource With(ComponentSpec spec) => this with { Specs = Specs.Add(new ComponentProjectionSpec.Type(spec)) };
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
        substance: s => ProjectSubstance(s.Spec, ctx),
        type:      c => ProjectType(c.Spec, ctx));

    // --- [SUBSTANCE_FOLD]
    // The pure-material subgraph (the prior MaterialProjector.ProjectMaterial nodes and edges, byte-identical;
    // only the binding vouch below accumulates): a content-addressed
    // Material node (the M7 section baked onto a ProfileSet), an optional content-keyed Appearance node, and the
    // vouched element→material / element→appearance Associate edges — built on GraphDelta.Empty. Each id is minted
    // through the seam content address, so two specs projecting the same material mint ONE id and the duplicate
    // add collapses at the seam WorkingGraph.Set upsert when AdmitOnto folds the merged delta.
    Fin<GraphDelta> ProjectSubstance(MaterialSpec spec, ProjectionContext ctx) =>
        BakeSection(spec.Composition, ctx.Key).Bind(composition => {
            double tolerance = ctx.Header.Tolerance;
            Node material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, composition, spec.Properties), tolerance);
            Option<Node> appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance));
            GraphDelta withNodes = appearance.Match(Some: a => GraphDelta.Empty.Put(material).Put(a), None: () => GraphDelta.Empty.Put(material));
            return AuthorBindings(spec, material.Id, appearance.Map(static a => a.Id), ctx, withNodes);
        });

    // --- [TYPE_FOLD]
    // The standardized-Component subgraph: MINT the deterministic-rooted Type Object, lower the structural
    // Material node (the SubstanceId material with the M7 section baked on), the optional Appearance, and the
    // SEED-BUILT detail bag read STRAIGHT off c.Detail (the deleted Detail(Component) switch's totality is now the
    // Component.Of lane/detail type law — a None-lane family carries no bag, a Realization/Product family always
    // does), wire the Type→resource edges (both endpoints owned — no vouch), then bind every VOUCHED occurrence
    // via Assign.TypeDefinition. Classification.Of is the seam Fin admission; AuthorOccurrences the fallible tail.
    Fin<GraphDelta> ProjectType(ComponentSpec spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Component c = spec.Component;
        return
            from classification in Classification.Of("ifc", c.IfcEntity, ctx.Key)
            from baked in BakeSection(spec.Composition, ctx.Key)
            let type = MintType(c, classification, ctx)
            let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), c.SubstanceId, baked, spec.Properties), tolerance)
            let appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance))
            let detail = c.Detail.Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))
            let seeded = SeedType(type, material, appearance, detail)
            from bound in AuthorOccurrences(spec.Occurrences, type.Id, ctx, seeded)
            select bound;
    }

    // MINT the deterministic-rooted Type Object: a ROOTED identity DERIVED from the Component's canonical content
    // through NodeId.RootedType over Node.Object.ToTypeSeedBytes (which EXCLUDES the volatile Representations, so
    // a later geometry attach never re-keys and identical Components dedup to one Type). Kind is the REUSED
    // ObjectKind.Type static; the Classification/PredefinedType stamp reads the IfcBinding forwarders (the seed-
    // computed row data; AdmitPredefined validity is Rasm.Bim's); the Designation rides Name+Tag; Representations
    // are Empty (geometry host-materialized and content-key-attached later); the SchemaSpan comes from the model
    // Header. The draft carries a placeholder Rooted id (ToTypeSeedBytes excludes the id), then Relabel re-stamps
    // the derived NodeId.RootedType — a class-root [Union] Node case has NO compiler `with`, so Relabel.
    static Node MintType(Component c, Classification classification, ProjectionContext ctx) {
        Node.Object draft = new(
            NodeId.Rooted(), ObjectKind.Type, Option<string>.None, classification, PredefinedType.Create(c.PredefinedToken),
            c.Designation.Value, c.Designation.Value, RepresentationContentHash.Empty, Option<OwnerHistory>.None, SchemaSpan.From(ctx.Header.Schema));
        return draft.Relabel(NodeId.RootedType(draft.ToTypeSeedBytes(ctx.Header.Tolerance).Span));
    }

    // Author the Type subgraph: Put the minted Type Object, its content-keyed structural Material (baked section),
    // the optional Appearance, and the optional seed-built detail bag, plus the Type→Material / Type→Appearance
    // Associate edges (MaterialUsage.None — the TYPE-level association carries no per-occurrence usage; occurrence
    // usage rides the occurrence's own binding [C7]) and the Type→detail Assign.PropertyDefinition (occurrences
    // inherit through the Bake type-bag merge). Both endpoints are owned here, so no vouch gates these edges.
    static GraphDelta SeedType(Node type, Node material, Option<Node> appearance, Option<Node> detail) {
        GraphDelta withMaterial = GraphDelta.Empty.Put(type).Put(material)
            .Link(new Relationship.Associate(type.Id, material.Id, new MaterialUsage.None()));
        GraphDelta withAppearance = appearance.Match(
            Some: a => withMaterial.Put(a).Link(new Relationship.Associate(type.Id, a.Id, new MaterialUsage.None())),
            None: () => withMaterial);
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
                ? Fin.Succ(resolved.Section.Match(Some: section => composition.WithSection(SeamSection(section)), None: () => composition))
                : ProjectionFault.Unresolved(key, $"<profile-ref-unresolved:{ps.Profile.Designation}>")
            : Fin.Succ(composition);

    // The neutral seam SectionProperties lifted from the twenty-field ComputedSection — every typed mint now a
    // component#QUANTITY_ROW QuantityRow row (the ONE bounded mint site; the six local Len/Area/Modulus/Inertia/
    // Torsion/Warping statics are DELETED), so every QuantityType/Dimension/SI-scale is byte-identical to the
    // registry row and MeasureValue content keys are unchanged. Iw is the FIFTH field (after J); the shear-area
    // lift preserves the major/minor convention (AvyMm2 MAJOR/web -> AvY); the three asymmetric-section LTB
    // columns lift last (engineering-zero for every doubly-symmetric family, non-zero for the open thin-walled
    // shapes steel fills). Named arguments PIN each lift so a future seam re-order cannot silently re-slot a column.
    static SectionProperties SeamSection(ComputedSection c) => new(
        Area: QuantityRow.Area.OfSi(c.AreaMm2.Value),
        Iyy: QuantityRow.SecondMomentOfArea.OfSi(c.IxMm4.Value),
        Izz: QuantityRow.SecondMomentOfArea.OfSi(c.IyMm4.Value),
        J: QuantityRow.TorsionConstant.OfSi(c.JMm4.Value),
        Iw: QuantityRow.WarpingConstant.OfSi(c.IwMm6),
        Wely: QuantityRow.SectionModulus.OfSi(c.SxMm3.Value),
        Welz: QuantityRow.SectionModulus.OfSi(c.SyMm3.Value),
        Wply: QuantityRow.SectionModulus.OfSi(c.ZxMm3.Value),
        Wplz: QuantityRow.SectionModulus.OfSi(c.ZyMm3.Value),
        AvY: QuantityRow.Area.OfSi(c.AvyMm2.Value),
        AvZ: QuantityRow.Area.OfSi(c.AvzMm2.Value),
        RadiusOfGyrationMajor: QuantityRow.Length.OfSi(c.RxMm.Value),
        RadiusOfGyrationMinor: QuantityRow.Length.OfSi(c.RyMm.Value),
        Depth: QuantityRow.Length.OfSi(c.DepthMm.Value),
        Width: QuantityRow.Length.OfSi(c.WidthMm.Value),
        HeatedPerimeter: QuantityRow.Length.OfSi(c.HeatedPerimeterMm.Value),
        AxisDistance: QuantityRow.Length.OfSi(c.AxisDistanceMm),
        ShearCentreY: QuantityRow.Length.OfSi(c.ShearCentreYMm),
        ShearCentreZ: QuantityRow.Length.OfSi(c.ShearCentreZMm),
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
    static Fin<GraphDelta> AuthorBindings(MaterialSpec spec, NodeId materialId, Option<NodeId> appearanceId, ProjectionContext ctx, GraphDelta delta) =>
        spec.Bindings
            .Traverse(binding => ctx.Owns(binding.Element)
                ? Success<Error, MaterialBinding>(binding)
                : Fail<Error, MaterialBinding>(ProjectionFault.Unvouched(ctx.Key, $"<associate-element-not-in-context:{binding.Element.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(delta, (g, binding) => BindElement(g, binding, materialId, appearanceId)))
            .ToFin();

    static GraphDelta BindElement(GraphDelta delta, MaterialBinding binding, NodeId materialId, Option<NodeId> appearanceId) =>
        appearanceId.Match(
            Some: appearance => delta
                .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))
                .Link(new Relationship.Associate(binding.Element, appearance, new MaterialUsage.None())),
            None: () => delta.Link(new Relationship.Associate(binding.Element, materialId, binding.Usage)));

    // H12 (Type): bind each VOUCHED occurrence to the minted Type via Assign.TypeDefinition (REUSED, never an IFC
    // DefinesByType name). The occurrence is the sited piece the model author / Bim ingest already rooted (in
    // ctx.ElementIds); the Type is the identity THIS projection minted (NOT vouched — the owner mints its own Type
    // id). Applicative like the substance vouch: EVERY unvouched occurrence reports; an empty set binds nothing.
    static Fin<GraphDelta> AuthorOccurrences(Seq<NodeId> occurrences, NodeId typeId, ProjectionContext ctx, GraphDelta delta) =>
        occurrences
            .Traverse(occurrence => ctx.Owns(occurrence)
                ? Success<Error, NodeId>(occurrence)
                : Fail<Error, NodeId>(ProjectionFault.Unvouched(ctx.Key, $"<type-occurrence-not-in-context:{occurrence.Value}>")))
            .As()
            .Map(vouched => vouched.Fold(delta, (g, occurrence) => g.Link(new Relationship.Assign(occurrence, typeId, AssignKind.TypeDefinition))))
            .ToFin();
}
```

## [03]-[COMPOSITION_AUTHOR]

- Owner: `CompositionAuthor` the seam-`MaterialComposition` BUILDER (coerce-and-delegate, owning NO invariant the seam owns) and the `[C7]` `MaterialUsage` occurrence-payload DERIVER — absorbed whole from the deleted `Construction/assembly.md`, re-homed to `Rasm.Materials.Projection` (its host-neutral run geometry landed in the Generation spec; the band-2350 rail retired with the folder).
- Cases: one `CompositionAuthor` family over the seam trichotomy-plus-single — `Single` (one `MaterialId`, homogeneous — `IfcMaterial`), `LayerSet` (material-plus-thickness rows coerced into `Seq<MaterialLayer>` — `IfcMaterialLayerSet`), `ProfileSet` (one `MaterialId` per extruded member with the `ComponentId` wrapped into a seam `ProfileRef` — `IfcMaterialProfileSet`), `ConstituentSet` (keyword-tagged fraction rows — `IfcMaterialConstituentSet`); the author coerces and DELEGATES to the seam smart-constructor, never a fourth case (`IfcMaterialList` deprecated, never admitted).
- Entry: `LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key)` and `ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key)` are `Fin<T>` coerce-and-delegate builders (the seam `OfLayerSet`/`OfConstituentSet` OWN the empty/thickness/fraction admission, railing `ElementFault.ValueRejected`); `ProfileSet(MaterialId material, ComponentId component)` and `Single(MaterialId material)` are TOTAL; `UsageOf(MaterialComposition composition, Op key)` derives the `[C7]` occurrence usage the seam `Associate` edge carries from the composition shape.
- Growth: a new composition shape is a seam `MaterialComposition` case this author gains one coerce arm for (a seam growth, never a parallel Materials assignment); a new occurrence-usage axis one seam `MaterialUsage` case the `UsageOf` derivation maps — never a per-element-type assignment.
- Boundary: `CompositionAuthor` BUILDS the seam value and NEVER re-declares or re-validates it — the seam `Rasm.Element/Composition` owns the `MaterialComposition` `[Union]`, the layer/constituent specs, the `MeasureValue` SI coercion, AND the admission rail, so a duplicated pre-guard here (or any Materials-minted fault for a composition miss) is the named defect. The layer thickness coerces once through the seam `MeasureValue.Of(_, UnitsNet.Units.LengthUnit.Millimeter, key)`; a `ProfileSet` references a component by its catalogue key as a seam `ProfileRef` the `component#COMPONENT_RESOLUTION` resolver dereferences ONE-HOP (`[M7]`); the `[C7]` usage is DISJOINT from the type-level composition — the derived default (`LayerSetDirection.Axis2`/`DirectionSense.Positive`/zero offset/NaN unset extent for a layered element; `CardinalPoint.Mid`/NaN extent for a profiled member) rides the seam `Associate` edge, an app composing a non-default `MaterialUsage` directly against the seam where an occurrence differs.

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

    // C7: the per-occurrence usage the seam Associate edge carries, derived from the composition shape — a
    // LayerSet binds with the IFC-typical vertical-wall Axis2/positive/0 default and an unset NaN ReferenceExtent
    // (a Materials-authored layer usage knows no occurrence extent; the seam treats NaN as the unset sentinel so
    // the canonical bytes stabilize), a ProfileSet with the IFC-default Mid cardinal point + an unset NaN extent,
    // a Single/ConstituentSet with None. Fin<T> because the ProfileSet usage admits its CardinalPoint through the
    // seam MaterialUsage.ProfileSet.Of grid admission (Mid is in-grid, so the default never faults — the rail is
    // the seam's contract); the other three arms are total and lifted into Fin.Succ. An app needing a non-default
    // occurrence binding composes a seam MaterialUsage directly (the seam owns the type).
    public static Fin<MaterialUsage> UsageOf(MaterialComposition composition, Op key) =>
        composition.Switch(
            single:         _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()),
            layerSet:       _ => Fin.Succ<MaterialUsage>(new MaterialUsage.LayerSet(LayerSetDirection.Axis2, DirectionSense.Positive, 0.0, double.NaN)),
            profileSet:     _ => MaterialUsage.ProfileSet.Of(CardinalPoint.Mid.Key, double.NaN, key),
            constituentSet: _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()));
}
```

## [04]-[COMPONENT_SUBGRAPH]

- Owner: the `ComponentSubgraph` capture composition root — `Capture` (the homogeneous-substance fold building `Substance` specs — the prior `MaterialSubgraph.Capture` per-material construction with the binding fan widened to the real one-material-many-elements arity, the traversal applicative) and `CaptureComponent` (the applicative catalogue-row fold building the `Type` specs) — plus `CompositionOf`, the RELOCATED composition selection: the per-family `ToLayerSet`/shape-kind methods are the deleted forms, replaced by ONE read of the campaign currency (`ComponentRow.Sectioned` + the `SectionProfile.Layered` arm).
- Cases: two projected node families over one fold — the SUBSTANCE family (`Material` content-keyed with the M7-baked composition + the `Discipline`-keyed property set; `Appearance` content-keyed) and the TYPE family (the rooted-deterministic Type `Object` + its structural `Material` + optional `Appearance` + the seed-built `PropertySet` detail bag); the projector authors NO `Assessment` node (`Rasm.Compute` reads the `Material` plies directly and writes the `Assessment` `Result`). Composition selection: `Sectioned` row → `ProfileSet` (steel/timber-member/cmu/masonry-course-capable rows — the M7 bake target); unsectioned `Layered` row → `LayerSet` (a CLT panel's role-tagged plies, a glazing IGU's pane/cavity stack, a board's face/core/face build — each `Ply.Role` the `IfcMaterialLayer.Name`); every other row → `Single` (a fastener, connector, joint, bar, or brick is a homogeneous discrete part).
- Entry: `ComponentSubgraph.Capture(materials, elementsOf, sections, key)` builds the `Substance` specs — each the seam `MaterialId` + `CompositionAuthor.Single` + the REQUIRED engineering set + the OPTIONAL lifecycle set + the Object-node `Classification` riding EVERY binding + the `[C7]` usage derived once and fanned across `elementsOf(id)` (one material binds MANY elements — the real-model arity; an empty set is the pure-substance run) + the content-keyed `AppearanceSummary` — the per-material builds INDEPENDENT and traversed APPLICATIVELY onto ONE `Fin` rail, so a from-scratch capture reports EVERY unregistered material in one failure; `ComponentSubgraph.CaptureComponent(source, rows, key)` adds the `Type` specs over catalogue `(ComponentRow, occurrences)` pairs — rows traversed APPLICATIVELY so a catalogue-wide capture reports EVERY unresolvable `SubstanceId`, a single Type a one-element `Seq` — `CompositionOf(row, key)` derives the structural composition from the row itself (the projector never re-selects, and no family page selects either: the currency decides), `MaterialFactsCatalogue.Lookup(row.Item.SubstanceId)` the REQUIRED engineering rows, `MaterialLibrary.Lookup(row.Item.AppearanceId)` the OPTIONAL appearance.
- Packages: Rasm.Element (project — `Node` cases, `MaterialComposition`, `MaterialLayer`/`MaterialConstituent`, `MaterialUsage`/`LayerSetDirection`/`DirectionSense`/`CardinalPoint`, `MeasureValue`, `MaterialPropertySet`, `Relationship`, `Classification`, `AppearanceSummary`, `ProfileRef`), Rasm.Materials.Component (project — `Component`/`ComponentRow`/`ComponentId`/`SectionProfile`/`Ply`/`ComponentResolution`/`ResolvedComponent`), Rasm.Materials.Properties (project — `MaterialPropertyCatalogue`/`SustainabilityCatalogue`), Rasm.Materials.Appearance.Graph + Interchange (project — `MaterialLibrary`/`MaterialWire`), LanguageExt.Core; the `Rasm.Materials.Construction` reference is RETIRED — `CompositionAuthor` is `[03]`, this namespace.
- Growth: a new engineering discipline routed to a material is one seam `Discipline` row the `MaterialPropertySet` carries — no capture arm; a new family's Type capture is ALREADY total (a new `ComponentFamily` row's components flow through the same `Sectioned`/`Layered`/`Single` law with zero edits here); a new composition shape is one `CompositionOf` arm over the new seam case — the subgraph grows by seam case and catalogue row, never a new node author.
- Boundary: `CompositionOf` is the ONE composition-selection law and it reads ONLY row-carried facts — the `Sectioned` membership pin (frozen this campaign, so the ProfileSet set is value-identical to the prior per-family selection) and the profile arm; a `Sectioned` row ALWAYS takes `ProfileSet` even when `Layered`-admitting families widen membership later (a section that solves must have a `ProfileSet` to bake onto — the M7 invariant), and the `Layered` plies coerce STRAIGHT into the `LayerSet` tuple shape (`Ply(Material, ThicknessMm, Role)` ≡ `(MaterialId, double, string)`), so the CLT/IGU/board layer names round-trip the same `Ply.Role` strings the gamma-method and the generators read. The REQUIRED-vs-OPTIONAL `Lookup` asymmetry holds (`MaterialPropertyCatalogue.Lookup` rails the seam `ElementFault.ValueRejected` on an unregistered material; `SustainabilityCatalogue.Lookup` returns `Fin.Succ(empty)` — lifecycle OPTIONAL; `Classification` returns `None` when absent); the resolved standard `Classification` rides the `MaterialBinding` to the bound element's `Object` node; the `Material` node carries its OWN `Discipline`-keyed set and NEVER an element-level `PropertySet`/`QuantitySet` bag (those are `Rasm.Bim`'s at IFC ingress); the detail bag is SEED-BUILT (`component#COMPONENT_DETAIL` — the four realizing-part families and the panel/masonry/glazing lanes construct their rows at seed time; this page reads `Component.Detail`, carrying zero per-family knowledge); the whole subgraph is one additive `GraphDelta` the seam `IGraphConstraint.Validate` gates before the seam folds it.

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// The capture composition root: fold the catalogues into the ComponentProjectionSource the ComponentProjector
// folds. The SUBSTANCE capture keeps the prior MaterialSubgraph.Capture per-material construction, its binding
// fan widened to the real one-material-many-elements arity; the TYPE capture takes catalogue ComponentRows and
// DERIVES each composition from the row currency. The M7 ProfileRef→ResolvedComponent table
// (component#COMPONENT_RESOLUTION, supplied once by the caller so the section integral runs once per
// component, never per material) seeds the source's Sections.
public static class ComponentSubgraph {
    // The HOMOGENEOUS-SUBSTANCE capture: one MaterialSpec per library material — the REQUIRED engineering set
    // + the OPTIONAL lifecycle set, the OPTIONAL Object-node Classification riding EVERY binding, the C7 usage
    // (derived ONCE per material, fanned across its bound elements — one material binds MANY elements in a real
    // model), the appearance. Materials are INDEPENDENT, so the traversal is applicative — a from-scratch
    // capture reports EVERY unregistered material, never just the first. A pure-substance run (elementsOf
    // empty) emits a content-keyed material subgraph with no dangling edge; a bound run names the edge owners,
    // every element vouched by ctx.Owns at Project.
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
                 from bindings in elements.IsEmpty
                     ? Fin.Succ(Seq<MaterialBinding>())                                                // no bound element → a pure-Materials subgraph run
                     : CompositionAuthor.UsageOf(composition, key).Map(usage =>
                         elements.Map(element => new MaterialBinding(element, usage, facts.Classification)))   // C7: ONE usage derivation + the Object-node Classification fan across every bound element
                 let appearance = MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary).ToOption()
                 select new MaterialSpec(id, composition, facts.Properties, appearance, bindings)).ToValidation())
            .As()
            .Map(specs => specs.Fold(ComponentProjectionSource.Empty with { Sections = sections }, static (source, spec) => source.With(spec)))
            .ToFin();

    // The TYPE capture: catalogue ComponentRows + their vouched occurrences → ComponentSpecs the ProjectType
    // fold mints into Type Objects. The composition DERIVES from each row (CompositionOf); the structural-
    // material engineering rows are the SubstanceId row (REQUIRED), the appearance the AppearanceId library row
    // (OPTIONAL). Rows are INDEPENDENT, so the traversal is applicative — a catalogue-wide capture reports
    // EVERY unresolvable SubstanceId, never just the first; a single Type is a one-element Seq, never a
    // sibling singular arity. facts.Classification is DELIBERATELY dropped here: ToTypeSeedBytes hashes the
    // Classifications set, so stamping it re-keys every derived Type NodeId against the frozen id law —
    // Type-borne classification lands only via a brief amendment that excludes Classifications from the seed.
    public static Fin<ComponentProjectionSource> CaptureComponent(
        ComponentProjectionSource source, Seq<(ComponentRow Row, Seq<NodeId> Occurrences)> rows, Op key) =>
        rows.Traverse(entry =>
                (from composition in CompositionOf(entry.Row, key)
                 from facts in MaterialFactsCatalogue.Lookup(entry.Row.Item.SubstanceId, key)
                 let appearance = MaterialLibrary.Lookup(entry.Row.Item.AppearanceId, key).Map(MaterialWire.Summary).ToOption()
                 select new ComponentSpec(entry.Row.Item, composition, facts.Properties, appearance, entry.Occurrences)).ToValidation())
            .As()
            .Map(specs => specs.Fold(source, static (acc, spec) => acc.With(spec)))
            .ToFin();

    // The RELOCATED composition selection — ONE law over the campaign currency, replacing every per-family
    // ToLayerSet/shape-kind method: a Sectioned row is a ProfileSet (its solved section MUST have a ProfileSet to
    // bake onto — steel/timber-member/cmu and the structural deck boards); an unsectioned Layered row is a
    // LayerSet built straight from its role-tagged plies (CLT, IGU, board build — Ply.Role becomes the
    // IfcMaterialLayer.Name); every other row is a Single discrete part (fastener, connector, joint, bar, brick).
    static Fin<MaterialComposition> CompositionOf(ComponentRow row, Op key) =>
        row switch {
            { Sectioned: true } => Fin.Succ(CompositionAuthor.ProfileSet(row.Item.SubstanceId, row.Item.Designation)),
            { Item.Profile: SectionProfile.Layered layered } => CompositionAuthor.LayerSet(
                layered.Plies.Map(static p => (p.Material, p.ThicknessMm.Value, p.Role)), key),
            _ => Fin.Succ(CompositionAuthor.Single(row.Item.SubstanceId)),
        };
}
```

## [05]-[RESEARCH]

- [PROJECTOR_MERGE]: REALIZED — the prior `MaterialProjector` and `ConnectionProjector` collapse into ONE `ComponentProjector` whose single `Project` fold discriminates `Substance(MaterialSpec)` from `Type(ComponentSpec)` over the `ComponentProjectionSpec` `[Union]`. The dual-projector paradigm, the `ConnectionSpec`/`ConnectionSchedule` source pair, and the "authors NO `Object` node" stance are the deleted forms; adding a future `Rasm.Fabrication` projector is one registration row the seam `Assemble` merges.
- [OWNER_MINTS_TYPE_IDENTITY]: `ProjectType` MINTS the deterministic-rooted Type `Object` — `NodeId.RootedType` over `Node.Object.ToTypeSeedBytes` (representation-excluded, so a later geometry attach never re-keys and identical `Component`s dedup to one Type), the REUSED `ObjectKind.Type` static, `Assign.TypeDefinition` for occurrences (never an IFC `DefinesByType` spelling). The stamp reads the `IfcBinding` forwarders (`c.IfcEntity`/`c.PredefinedToken`) — seed-computed ROW DATA under the brief IFC-binding seed law, so the projector body is family-blind; roster validity is `Rasm.Bim`'s composition-time `IfcLegality` arm and per-token `AdmitPredefined` egress gate.
- [REGISTRY_BAND]: `ProjectionFault.Code => FaultBand.Projection` consumes the `Rasm.Element` `FaultBand` registry through the generated `SmartEnum`-to-`int` conversion — one line, never a `.Value` spelling; a duplicate band integer fails at type initialization, so the 2470 disjointness the prior prose recited is type-enforced. The unmarked-union (no `[GenerateUnionOps]`)/`…Case`/bare-lift shape is unchanged.
- [QUANTITY_ROW_LIFT]: the M7 `SeamSection` bake lifts the TWENTY-field `ComputedSection` onto the twenty-field seam `SectionProperties` column-for-column, every typed mint a `component#QUANTITY_ROW` `QuantityRow` row (`Length`/`Area`/`SectionModulus`/`SecondMomentOfArea`/`TorsionConstant`/`WarpingConstant` — the six local statics DELETED), so every `QuantityType` spelling, `Dimension` vector, and SI scale is the registry row's and `MeasureValue` content keys are byte-identical; the named-argument pinning (`Iw` fifth after `J`, the three asymmetry columns last) is preserved character-for-character per the frozen-wire law. The asymmetry columns stay engineering-zero for every doubly-symmetric family and non-zero for the open thin-walled shapes the steel `Forms` supplement fills, so a PFC/tee/angle's LTB inputs cross the seam faithfully.
- [SEED_TIME_DETAIL]: the `Detail(Component)` switch and its `Joint`/`Token`/`Measured`/`RealizationRows`/`ProductRows` constructors are DELETED from this page — each family's bag is built AT SEED TIME through the relocated `component#COMPONENT_DETAIL` `ComponentDetail` owner (same bodies, same dimension-only `MeasureValue.OfSi(dim, si)` mints, same `DetailSchema.Realization`/`DetailSchema.Product` seeds), so the bag rows, `PropertyBag` content, and projected `Node.PropertySet` bytes are identical while the projector reads `c.Detail` with zero per-family knowledge; the `Component.Of` lane/detail law (a `None`-lane family carries no bag, a `Realization`/`Product` family always does) carries the deleted switch's totality as a type law. The IMPORT round-trip still splits by element genus: the four realizing-element families through the `Rasm.Bim` connection-detail reader targeting `DetailSchema.Realization`, panel product detail through the general Bim object/property fold under `DetailSchema.Product`.
- [COMPOSITION_SELECTION]: the family `ToLayerSet`/shape-kind methods are DELETED — `CompositionOf` derives the structural composition from the row currency: `Sectioned` → `ProfileSet` (the M7 bake invariant — a solved section needs a `ProfileSet` to bake onto), unsectioned `Layered` → `LayerSet` (the plies coerce straight into the seam tuple, `Ply.Role` the `IfcMaterialLayer.Name` — CLT `"0"`/`"90"`, IGU pane/cavity names, board face/core/face), else `Single`. The seam trichotomy-plus-single is untouched (`IfcMaterialList` never admitted), the `[C7]` `UsageOf` default rides the `Associate` edge, and a new family's components flow through with ZERO edits here.
- [CLASS_ROOT_MINT]: `Mint` composes the seam `Node.Relabel` re-stamp — a class-root `[Union]` `Node` case has NO compiler-generated `with`, so the prior `draft with { Id = … }` is the deleted form; the content-derived id re-stamps through `Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span))`, the kernel seed-zero `XxHash128` the seam `ContentAddress` composes — the ONE hasher, no second in this folder. The cross-runtime C#/Python/TypeScript parity corpus pins byte-for-byte agreement on the canonical bytes.
- [H12_OCCURRENCE_VOUCH]: the `ProjectionContext.Owns` vouch is per-binding (substance) and per-occurrence (Type), APPLICATIVE end to end — an empty `Bindings`/`Occurrences` set traverses to zero edges and zero faults (genuinely nothing to bind), NEVER a `ctx.ElementIds.IsEmpty` gate: a spec that DOES carry bindings/occurrences rails `ProjectionFault.Unvouched` for EVERY element the context cannot vouch, the misses accumulated into one failure (`Traverse`→`ToValidation`→`ToFin`, matching the `Project` spec fold and the seam `Assemble` capture), never a silently-dropped edge and never a first-miss-only report.
- [ASSESSMENT_OWNERSHIP]: `Rasm.Materials` authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the assessment input `Rasm.Compute` reads DIRECTLY (above the seam), runs the discipline route, and writes the seam `Assessment` `Result` node content-keyed on `(input key, route)`; the multi-ply `AssemblyAggregator` is `Rasm.Compute`'s.
- [IGRAPH_CONSTRAINT_GATE]: the projected `GraphDelta` is gated by the seam's second interface `IGraphConstraint.Validate` (`Rasm.Bim`-implemented — IFC-semantic legality plus the new vocabulary arms: a `Type` may not aggregate an `Occurrence`, a classified node must resolve a roster row, a predefined token must be in the entity's valid set) before the seam folds it; the projector enforces only the STRUCTURAL invariants it owns, so the two interfaces stay orthogonal.
