# [MATERIALS_PROJECTION]

THE COMPONENT PROJECTOR and THE COMPONENT-SUBGRAPH AUTHOR. `Rasm.Materials` lowers onto the shared `Rasm.Element` seam through ONE `ComponentProjector` — the merge of the prior `MaterialProjector` and `ConnectionProjector` into a single `IElementProjection` whose one `Fin<GraphDelta> Project(ProjectionContext ctx)` op folds a `Seq<ComponentProjectionSpec>` discriminating the pure-substance `MaterialSpec` path from the Type-minting `ComponentSpec` path. The unified projection INVERTS the prior "authors NO `Object` node, mints NO element identity" stance under the owner-mints-its-identity law: `Rasm.Materials` owns Component TYPES, so a `Type` spec MINTS the deterministic-rooted Type `Object` from the `Component`'s canonical content (the seam `Graph/element#NODE_MODEL` `NodeId.RootedType` derivation over `Node.Object.ToTypeSeedBytes`, which EXCLUDES the volatile `Representations` so a later geometry attach never re-keys the type), stamps its `Classification("ifc", IfcEntity)` + `PredefinedType.Create(PredefinedToken)` off the `Rasm.Materials.Component/component#COMPONENT_OWNER` `ComponentSection` egress projections (validity staying the `Rasm.Bim` `AdmitPredefined` egress gate, never a seam invariant), bakes its `ComputedSection` onto the structural material's `ProfileSet` composition (the M7 `SeamSection` lift via `Composition/material#MATERIAL_COMPOSITION` `WithSection`), authors the NEUTRAL realization-detail bag over the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema`, and binds every vouched occurrence to the Type via `Relations/relation#EDGE_ALGEBRA` `Assign.TypeDefinition` — all in ONE `Project` fold the seam `Projection/projection#PROJECTION_CONTRACT` `Assemble` merges with every sibling projector's delta. The pure-substance path is UNCHANGED: a `MaterialSpec` lowers a content-keyed `Material` node (its `Classification/classification#DISCIPLINE_AXIS` `Discipline`-keyed `MaterialPropertySet` set, the M7 section baked onto a `ProfileSet`) plus an optional content-keyed `Appearance` node, and authors the element→material `Associate` edge ONLY for a `MaterialBinding` whose element `ctx.Owns` vouches (`[H12]`), the occurrence binding riding the seam `MaterialUsage` the `Construction/assembly#MATERIAL_COMPOSITION` author produced. Identity is the ONE content rail: a `Material`/`Appearance`/detail node is `NodeId.Content` over its own `Projection/address#CANONICAL_WRITER` `ToCanonicalBytes` (the kernel seed-zero `XxHash128` the seam `Projection/address#CONTENT_ADDRESS` `ContentAddress` composes — no second hasher in this folder), the Type `Object` `NodeId.RootedType` over its representation-excluded seed, so identical `Component`s dedup to one Type, identical materials to one node, idempotent under re-projection. The page composes the `Rasm.Element` seam contracts (`IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`DetailSchema`), the `Rasm.Materials.Component/component#COMPONENT_OWNER` owner (`Component`/`ComponentSection`/`ComputedSection`/`ResolvedComponent`), the `Properties`/`Appearance`/`Construction` catalogues the `ComponentSubgraph` capture reads, and the `ProjectionFault` band-2470 rail; it re-mints NO seam type and authors NO IFC entity — the seam graph IS the wire `Rasm.Bim` reads one-hop, the IFC `Pset_*`/`Rasm_ConnectionRealization` names + the `AdmitPredefined` validity gate staying `Rasm.Bim`-only.

## [01]-[INDEX]

- [02]-[COMPONENT_PROJECTOR]: the `ComponentProjector` `IElementProjection` owner (the `MaterialProjector` + `ConnectionProjector` merge), the `ComponentProjectionSource`/`ComponentProjectionSpec`/`MaterialSpec`/`ComponentSpec`/`MaterialBinding` captured-source records (with the M7 `ProfileRef`→`ResolvedComponent` section table), the `Project` fold discriminating `Substance` from `Type`, the `ProjectionFault` band-2470 rail, the `Mint` content-id helper, and the `MintType` deterministic-rooted Type identity.
- [03]-[COMPONENT_SUBGRAPH]: the `ComponentSubgraph` capture composition root — the homogeneous-substance `Capture` (the `Material`/`Appearance` content-keyed authoring + the `[H12]`/`[C7]` `Associate` binding + the Object-node `Classification` egress) and the `CaptureComponent` Type capture — composing the re-homed `Component/{timber,glazing}` family lowerings, the M7 `SeamSection` bake onto the twenty-field seam `SectionProperties`, and the neutral `DetailSchema` realization bag.

## [02]-[COMPONENT_PROJECTOR]

- Owner: `ComponentProjector` the sealed `IElementProjection` merging the prior `MaterialProjector` and `ConnectionProjector`; `ComponentProjectionSource` the captured-source aggregate (the `Seq<ComponentProjectionSpec>` + the M7 `ProfileRef`→`ResolvedComponent` table); `ComponentProjectionSpec` the `[Union]` discriminating `Substance(MaterialSpec)` from `Type(ComponentSpec)`; `MaterialSpec` the pure-substance projection unit (KEPT); `ComponentSpec` the Type-minting unit; `MaterialBinding` the element-occurrence material binding (seam `MaterialUsage` + the Object-node `Classification` egress, the trio name RESERVED for the Materials projection); `ProjectionFault` `[Union]` band 2470; the `Mint` content-id and `MintType` deterministic-Type-id helpers composing the seam `NodeId`.
- Cases: one `ComponentProjector` over a `Seq<ComponentProjectionSpec>` — a `Substance` arm carrying a `MaterialSpec` (a `MaterialId` + a `MATERIAL_COMPOSITION` seam `MaterialComposition` + the `Discipline`-keyed `MaterialPropertySet` set + an optional `AppearanceSummary` + a `Seq<MaterialBinding>` element-occurrence set), a `Type` arm carrying a `ComponentSpec` (a `Component` + its structural `MaterialComposition` + the `Component.CapacityKey` `MaterialPropertySet` set + an optional `AppearanceSummary` + a `Seq<NodeId>` vouched-occurrence set bound through `Assign.TypeDefinition`); the projector is NEVER a per-family or per-discipline projector type, never a `SteelProjector`/`ConnectionProjector` sibling — one projector folds the whole subgraph, the substance and the Type discriminated by the spec union rather than two projector surfaces.
- Entry: `public Fin<GraphDelta> Project(ProjectionContext ctx)` — the ONE seam contract op, `source.Specs.TraverseM(spec => ProjectSpec(spec, ctx))` accumulating each spec's OWN delta then folding through the cancellation-correct `Graph/delta#GRAPH_DELTA` `GraphDelta.Merge` MONOID (the EXACT `Projection/projection#PROJECTION_CONTRACT` `Assemble` shape — `TraverseM`→`Merge`-fold, not a hand-threaded accumulator), `Fin<T>` aborting on an occurrence/binding the context does not vouch (`ProjectionFault.Unvouched`) or a malformed Type `Classification` (lifting the seam `ElementFault` through `Classification.Of`) — never a content-key collision, the mint TOTAL and idempotent (the same `Component`/material projected twice mints one id and the duplicate add collapses at the seam `WorkingGraph.Set` upsert when `AdmitOnto` folds the merged delta); `ProjectSpec` discriminates the union via the generated `Switch` — `Substance`→`ProjectSubstance` (content-keyed `Material`/`Appearance` nodes + the vouched `Associate` edges), `Type`→`ProjectType` (the minted Type `Object` + the baked-section structural `Material` + the optional `Appearance` + the optional neutral detail bag + the vouched `Assign.TypeDefinition` occurrence edges); `ComponentProjector.Of(ComponentProjectionSource source)` captures the source once (the source-capture inversion), and the seam `Assemble(projectors, constraints, seed, ctx)` re-merges this projector's delta with `Rasm.Bim`'s `SemanticProjector` (and any sibling) into one `ElementGraph` — adding a projector is one registration row at the app composition root, never a seam edit.
- Packages: Rasm.Element (project — the seam: `IElementProjection`/`ProjectionContext`/`GraphDelta`/`Node`/`NodeId`/`ObjectKind`/`Classification`/`PredefinedType`/`RepresentationContentHash`/`SchemaSpan`/`OwnerHistory`/`Relationship`/`AssignKind`/`MaterialUsage`/`MaterialComposition`/`MaterialPropertySet`/`SectionProperties`/`ProfileRef`/`AppearanceSummary`/`PropertyBag`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension`/`QuantityType`/`DetailSchema`/`MaterialId`, the seam this folder projects onto), Rasm.Materials.Component (project — `Component`/`ComponentSection`/`ComputedSection`/`ResolvedComponent`, the standardized-type owner whose egress projections the Type mint reads), Rasm.Domain (project — `Op` the fault-correlation key; the seed-zero `XxHash128` content seed is the seam `ContentAddress` composition, not re-reached here), Thinktecture.Runtime.Extensions (`[Union]` for `ProjectionFault`/`ComponentProjectionSpec` + the `ComponentSection`/`JointSection` generated total `Switch` the `Detail` fold reads), LanguageExt.Core (`Fin`/`Seq`/`TraverseM`/`Fold`/`Bind`/`Map`/`Option` for the projection rail); cite `libs/csharp/.api` (the `thinktecture-runtime-extensions` substrate catalogue the union generators verify) — the `Rasm.Materials/.api` VividOrange family catalogues are the `component#COMPONENT_OWNER`'s, not composed here (the projector reads an already-resolved `ComputedSection`, never the section solver).
- Growth: a new projected node kind is one seam `Node` case the matching project arm authors (the seam owns the `Node` union; a kind the seam does not carry is a seam growth, never a Materials parallel node); a new spec modality is one `ComponentProjectionSpec` case the `Project` `Switch` dispatches; a new occurrence-usage shape is one seam `MaterialUsage` case the `[C7]` `MATERIAL_COMPOSITION` author produces, passed through unread; a new realization-detail row is one `DetailSchema` `PropertyName` the `Detail` fold reads; a new projection fault is one `ProjectionFault` case — never a second projector surface, never a per-family projector, never a Materials→IFC carrier beside the graph.
- Boundary: `ComponentProjector` is the ONE Materials `IElementProjection` — the prior dual `MaterialProjector`/`ConnectionProjector` paradigm and the `ConnectionProjector` "authors NO `Object` node" stance are the deleted forms; the `Substance` path is the prior `MaterialProjector` UNCHANGED (content-keyed `Material`/`Appearance` nodes, the `Associate` material edge authored only for a `ctx.Owns`-vouched `MaterialBinding`, the source-capture inversion capturing the four Materials catalogues as private projector fields), while the `Type` path is the owner-mints-its-identity inversion — `Rasm.Materials` owns Component TYPES, so `ProjectType` MINTS the Type `Object` (REUSING the `Graph/element#NODE_MODEL` `ObjectKind.Type` static, deriving its rooted `NodeId.RootedType` from the `Component`'s representation-excluded `ToTypeSeedBytes` seed so identical Components dedup and a later geometry attach never re-keys) rather than authoring no Object, and binds occurrences via `Assign.TypeDefinition` (REUSED, never an IFC `DefinesByType` spelling — the seam carries the neutral `AssignKind`); the Type-stamped `Classification("ifc", component.IfcEntity)` is the IFC entity-class pair the `ComponentSection.IfcEntity` projects across THREE modes (the four discrete-part families a fixed concrete leaf; the five profiled-but-supertype families the `ComponentClass` supertype; the panel family the KIND-DETERMINED leaf `p.Board.Kind.IfcEntity` — a gypsum/sheathing board `IfcCovering`, a wood structural panel `IfcPlate`, a composite floor deck `IfcSlab`, the leaf varying by `PanelKind` row rather than fixed per family) and `PredefinedType.Create(component.PredefinedToken)` the neutral token (the panel `p.Board.Kind.IfcPredefinedType` likewise kind-determined — `CLADDING`/`CEILING`/`INSULATION`/`SHEET`/`FLOOR`; the `IfcClass` roster + `AdmitPredefined` validity is `Rasm.Bim`'s egress concern); the `[H12]` vouch is per-binding/per-occurrence (`ctx.Owns`) — the substance arm short-circuits on `spec.Bindings.IsEmpty` and the Type arm on `spec.Occurrences.IsEmpty` (genuinely nothing to bind — the pure-isolation subgraph), NEVER on `ctx.ElementIds` emptiness (an empty context SILENTLY DROPPING a bound spec's edges is the [H12] violation: a spec that DOES carry bindings/occurrences rails `ProjectionFault.Unvouched` for every element an empty context cannot vouch); the Type `Object` minted here is NOT a context-vouched id (the owner mints its own Type identity) — only the substance bindings' elements and the Type's occurrences are vouched, the resources (the structural `Material`, the `Appearance`, the detail bag) content-keyed and owned by the minting projector so the Type→resource `Associate`/`Assign.PropertyDefinition` edges need no vouch; every authored non-rooted node is CONTENT-ADDRESSED through `Mint` composing the seam `NodeId.Content(node.ToCanonicalBytes(tolerance))` (the kernel seed-zero content hash, NO second hasher in this folder) so two specs minting the same node collapse to one and the delta is idempotent under re-projection; a material's standard `Classification` (the `Properties/sustainability#SUSTAINABILITY_PROPERTY` `(system, code)` egress) rides the `MaterialBinding` to the bound element's `Object` node (an Object-node VALUE per `Relations/relation#EDGE_ALGEBRA` — NOT a `Node.Material` field and NOT an `Associate` edge payload), the migration material-level classification wire retired into this element-Object egress; `ProjectionFault` band 2470 is `Expected`-derived (`[SkipUnionOps]`, the `…Case`-suffixed factory pattern) so a typed case lifts BARE onto `Fin<T>` with no `.ToError()` hop, disjoint from `ComponentFault` 2300 / `ConstructionFault` 2350 / kernel `GeometryFault` 2400 / `MaterialFault` 2450 / seam `ElementFault` 2500 yet lifting each owner's fault unchanged; the projected `GraphDelta` is gated by the seam's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` (`[M3]`, `Rasm.Bim`-implemented) for IFC-semantic legality before the seam folds it, so the projector enforces only the STRUCTURAL invariants it owns (content-key idempotence, context-vouched binding/occurrence endpoints, the Type-Object dedup) and defers IFC legality (a `Type` may not aggregate an `Occurrence`, a predefined token must be in the entity's valid set) to the constraint.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;                   // Op (the fault-correlation key the ProjectionContext carries)
using Rasm.Element;                  // the seam: IElementProjection, ProjectionContext, GraphDelta, Node, NodeId, ObjectKind,
                                     // Classification, PredefinedType, RepresentationContentHash, SchemaSpan, OwnerHistory,
                                     // Relationship, AssignKind, MaterialUsage, MaterialComposition, MaterialPropertySet,
                                     // SectionProperties, ProfileRef, AppearanceSummary, PropertyBag, PropertyName,
                                     // PropertyValue, MeasureValue, Dimension, QuantityType, DetailSchema, MaterialId
using Rasm.Materials.Component;      // Component, ComponentSection, ComputedSection, ResolvedComponent (the standardized-type owner)
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [ERRORS] ------------------------------------------------------------------------------
// The component-projection fault band (2470): Expected-derived over the kernel Rasm.Domain.Expected so band 2470 IS the
// Expected Code and a typed case lifts BARE onto Fin<T> (no .ToError() hop) — the seam Assemble fold short-circuits a
// failed projection without a partial graph. The kernel base ctor is PARAMETERLESS (Code a virtual Error member, Message
// abstract, Category virtual) — so band 2470 is a `Code => 2470` override and `Message => Detail`, the per-case Category
// override driving FaultExtensions.Category(error). The band is disjoint from ComponentFault 2300 / ConstructionFault 2350
// / kernel GeometryFault 2400 / MaterialFault 2450 / seam ElementFault 2500, yet lifts each owner's fault unchanged through
// Fin so a ComponentFault.Family or the seam ElementFault.ValueRejected (a malformed Type Classification) propagates without
// re-wrapping. [SkipUnionOps] skips the generated implicit-conversion ops (every case carries an explicit Op) and emits NO
// per-case factory: a nested `…Case` record carries the data and a same-name-less static factory returns the Expected-derived
// base so the case lifts BARE with no `new` (a same-named nested type + method is CS0102). Create routes the unspecific case
// under a boundary-admission Op.
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
// The element-occurrence material binding the app or Rasm.Bim supplies (the trio name RESERVED for the Materials
// projection — distinct from the seam Bake-folded BakedMaterial and the type→occurrence TypeBinding): which VOUCHED seam
// element NodeId this material binds, the C7 seam MaterialUsage (LayerSet direction/sense/offset, ProfileSet cardinal-
// point/extent) the MATERIAL_COMPOSITION author produced, and the material's resolved standard Classification (an
// Object-node VALUE per Relations/relation#EDGE_ALGEBRA — NOT a Node.Material field and NOT an edge payload) the bound
// element's Object node carries. The element MUST be vouched by ctx.Owns — the substance path never mints an element
// identity ([H12]); the Classification rides the binding to the Object-node owner (Bim ingest or the from-scratch app),
// which folds it into the bound element's Classifications set the Bim Semantics/classification re-emits.
public readonly record struct MaterialBinding(NodeId Element, MaterialUsage Usage, Option<Classification> Classification);

// One material to project on the PURE-SUBSTANCE path (the prior MaterialProjector unit, UNCHANGED): the seam MaterialId,
// the MATERIAL_COMPOSITION the assembly author built (a ProfileSet carrying the M7 ProfileRef the projector resolves and
// bakes the SectionProperties onto), the Discipline-keyed MaterialPropertySet set the catalogue lowered, the optional
// content-keyed AppearanceSummary, and the element occurrence bindings. NO assessment-input here — Rasm.Compute reads the
// projected Material node plies DIRECTLY (above the seam) and writes the Assessment result back.
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
// ComponentSection egress IfcEntity/PredefinedToken the Type Object stamps, the CapacityKey structural-material id, the
// AppearanceId render-row id), the structural material's TYPE-LEVEL MaterialComposition the family lowering selected (a
// ProfileSet for steel/timber/cmu the SeamSection bakes the section onto, a LayerSet for a glazing IGU, a Single for a
// discrete part), the CapacityKey MaterialPropertySet set the structural Material node carries, the optional content-keyed
// AppearanceSummary the AppearanceId resolved to, and the vouched sited-piece NodeIds bound to the minted Type via
// Assign.TypeDefinition. A pure type-authoring run carries an EMPTY Occurrences set (a Type subgraph usable in isolation).
public sealed record ComponentSpec(
    Component Component,
    MaterialComposition Composition,
    Seq<MaterialPropertySet> Properties,
    Option<AppearanceSummary> Appearance,
    Seq<NodeId> Occurrences) {
    public static ComponentSpec Of(Component component, MaterialComposition composition, Seq<MaterialPropertySet> properties) =>
        new(component, composition, properties, Option<AppearanceSummary>.None, Seq<NodeId>());
}

// The ONE projection spec the single Project fold discriminates: a Substance arm (the pure-material subgraph — content-
// keyed Material/Appearance nodes, no Object) versus a Type arm (the standardized Component — a minted Type Object + the
// baked-section structural material + the realization detail). One Seq<ComponentProjectionSpec> the projector folds, the
// MaterialProjector/ConnectionProjector dual surface collapsed to one fold over the discriminant, never two projectors.
[Union]
public abstract partial record ComponentProjectionSpec {
    public sealed record Substance(MaterialSpec Spec) : ComponentProjectionSpec;
    public sealed record Type(ComponentSpec Spec)     : ComponentProjectionSpec;
}

// The captured projection source: the per-spec stream plus the M7 ProfileRef→ResolvedComponent resolution table
// (component#COMPONENT_RESOLUTION, built once above the seam) the projector bakes onto ProfileSet plies. The two With
// overloads add the matching discriminant arm so a capture names the concept (substance vs Type) without re-spelling the union.
public sealed record ComponentProjectionSource(Seq<ComponentProjectionSpec> Specs, FrozenDictionary<ProfileRef, ResolvedComponent> Sections) {
    public static readonly ComponentProjectionSource Empty = new(Seq<ComponentProjectionSpec>(), FrozenDictionary<ProfileRef, ResolvedComponent>.Empty);
    public ComponentProjectionSource With(MaterialSpec spec)  => this with { Specs = Specs.Add(new ComponentProjectionSpec.Substance(spec)) };
    public ComponentProjectionSource With(ComponentSpec spec) => this with { Specs = Specs.Add(new ComponentProjectionSpec.Type(spec)) };
}

// --- [SERVICES] ----------------------------------------------------------------------------
// The one IElementProjection the Materials folder publishes (the MaterialProjector + ConnectionProjector merge). Captures
// the source internally (the source-capture inversion) so the seam op carries only the ProjectionContext; the seam ProjectionAssembly.Assemble
// merges this delta with every sibling.
public sealed class ComponentProjector : IElementProjection {
    readonly ComponentProjectionSource source;
    ComponentProjector(ComponentProjectionSource source) => this.source = source;
    public static ComponentProjector Of(ComponentProjectionSource source) => new(source);

    // TraverseM each spec to its OWN delta (the monadic accumulation short-circuiting on the first Unvouched spec), then
    // fold the per-spec deltas through the cancellation-correct GraphDelta.Merge MONOID — the EXACT seam Assemble shape
    // (TraverseM -> Merge-fold), not a hand-threaded accumulator. Each spec builds on GraphDelta.Empty so per-spec projection
    // is decoupled from the running delta, and Merge keeps the projector's contribution a faithful single delta the seam re-merges.
    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        source.Specs.TraverseM(spec => ProjectSpec(spec, ctx)).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));

    // The ONE discriminator — the substance subgraph (no Object) versus the Type subgraph (a minted Type Object), the
    // generated total Switch over the spec union, never two projector entrypoints.
    Fin<GraphDelta> ProjectSpec(ComponentProjectionSpec spec, ProjectionContext ctx) => spec.Switch(
        substance: s => ProjectSubstance(s.Spec, ctx),
        type:      c => ProjectType(c.Spec, ctx));

    // --- [SUBSTANCE_FOLD]
    // The pure-material subgraph (the prior MaterialProjector.ProjectMaterial, UNCHANGED): a content-addressed Material node
    // (the M7 section baked onto a ProfileSet), an optional content-keyed Appearance node, and the vouched element→material /
    // element→appearance Associate edges — built on GraphDelta.Empty (the seam Merge composes the per-spec deltas). Each id
    // is minted through the seam content address, so this folder owns NO hasher and two specs projecting the same material
    // mint ONE id — the duplicate add collapses at the seam WorkingGraph.Set upsert when AdmitOnto folds the merged delta.
    Fin<GraphDelta> ProjectSubstance(MaterialSpec spec, ProjectionContext ctx) =>
        BakeSection(spec.Composition, ctx.Key).Bind(composition => {
            double tolerance = ctx.Header.Tolerance;
            Node material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), spec.Material, composition, spec.Properties), tolerance);
            Option<Node> appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance));
            GraphDelta withNodes = appearance.Match(Some: a => GraphDelta.Empty.Put(material).Put(a), None: () => GraphDelta.Empty.Put(material));
            return AuthorBindings(spec, material.Id, appearance.Map(static a => a.Id), ctx, withNodes);
        });

    // --- [TYPE_FOLD]
    // The standardized-Component subgraph: MINT the deterministic-rooted Type Object, lower the structural Material node
    // (the CapacityKey material with the M7 section baked onto its composition), the optional content-keyed Appearance, and
    // the optional neutral DetailSchema realization bag, wire the Type→resource Associate/Assign.PropertyDefinition edges
    // (both endpoints owned — the Type minted here, the resources content-keyed — so no vouch), then bind every VOUCHED
    // occurrence to the Type via Assign.TypeDefinition. Classification.Of is the seam Fin admission (the IFC entity-class
    // pair), so the fold threads it; the lets are pure node mints; AuthorOccurrences is the only fallible tail.
    Fin<GraphDelta> ProjectType(ComponentSpec spec, ProjectionContext ctx) {
        double tolerance = ctx.Header.Tolerance;
        Component c = spec.Component;
        return
            from classification in Classification.Of("ifc", c.IfcEntity, ctx.Key)
            from baked in BakeSection(spec.Composition, ctx.Key)
            let type = MintType(c, classification, ctx)
            let material = Mint(new Node.Material(NodeId.Content(ReadOnlySpan<byte>.Empty), c.CapacityKey, baked, spec.Properties), tolerance)
            let appearance = spec.Appearance.Map(summary => Mint(new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary), tolerance))
            let detail = Detail(c).Map(bag => Mint(new Node.PropertySet(NodeId.Content(ReadOnlySpan<byte>.Empty), bag), tolerance))
            let seeded = SeedType(type, material, appearance, detail)
            from bound in AuthorOccurrences(spec.Occurrences, type.Id, ctx, seeded)
            select bound;
    }

    // MINT the deterministic-rooted Type Object: a ROOTED identity DERIVED from the Component's canonical content through
    // the seam Graph/element#NODE_MODEL NodeId.RootedType over Node.Object.ToTypeSeedBytes (which EXCLUDES the volatile
    // Representations, so a later geometry attach never re-keys the type and identical Components dedup to one Type). Kind
    // is the REUSED ObjectKind.Type static; the Classification("ifc", IfcEntity) + PredefinedType.Create(PredefinedToken)
    // stamp off the ComponentSection egress projections (the AdmitPredefined validity gate is Rasm.Bim's); the Designation
    // rides Name+Tag; the Representations are Empty (geometry host-materialized and content-key-attached later); the
    // SchemaSpan comes from the model Header. The draft carries a placeholder Rooted id (ToTypeSeedBytes excludes the id),
    // then Relabel re-stamps the derived NodeId.RootedType — a class-root [Union] Node case has NO compiler `with`, so Relabel.
    static Node MintType(Component c, Classification classification, ProjectionContext ctx) {
        Node.Object draft = new(
            NodeId.Rooted(), ObjectKind.Type, Option<string>.None, classification, PredefinedType.Create(c.PredefinedToken),
            c.Designation.Value, c.Designation.Value, RepresentationContentHash.Empty, Option<OwnerHistory>.None, SchemaSpan.From(ctx.Header.Schema));
        return draft.Relabel(NodeId.RootedType(draft.ToTypeSeedBytes(ctx.Header.Tolerance).Span));
    }

    // Author the Type subgraph: Put the minted Type Object, its content-keyed structural Material (with the baked section),
    // the optional content-keyed Appearance, and the optional neutral detail bag, plus the Type→Material / Type→Appearance
    // Associate edges (MaterialUsage.None — the TYPE-level association carries no per-occurrence usage; the occurrence usage
    // rides the occurrence's own binding [C7]) and the Type→detail Assign.PropertyDefinition (the type-level realization
    // detail occurrences inherit through the Bake type-bag merge). Both endpoints are owned here, so no vouch gates these
    // edges — the vouch gates only the occurrence Assign.TypeDefinition.
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
    // M7: resolve a ProfileSet's ProfileRef ONCE through the captured component#COMPONENT_RESOLUTION table and BAKE the
    // neutral seam SectionProperties onto the composition (WithSection), so the structural runner reads graph.SectionOf(member)
    // without re-resolving or admitting VividOrange. A non-ProfileSet (a Single/LayerSet/ConstituentSet) bakes nothing,
    // total; a ProfileSet ref present in the table with a None section (a non-profiled resolved component) bakes nothing;
    // a ProfileSet ref ABSENT from the table rails ProjectionFault.Unresolved — the M7 ComponentResolution.Build cache is
    // total over every catalogued component, so an absent ref is a caller-supplied incomplete-table bug surfaced rather
    // than a silently-dropped section. Shared by the substance and Type folds.
    Fin<MaterialComposition> BakeSection(MaterialComposition composition, Op key) =>
        composition is MaterialComposition.ProfileSet ps
            ? source.Sections.TryGetValue(ps.Profile, out ResolvedComponent resolved)
                ? Fin.Succ(resolved.Section.Match(Some: section => composition.WithSection(SeamSection(section)), None: () => composition))
                : ProjectionFault.Unresolved(key, $"<profile-ref-unresolved:{ps.Profile.Designation}>")
            : Fin.Succ(composition);

    // The neutral seam SectionProperties lifted from the component#COMPONENT_OWNER ComputedSection — the FULL twenty-field
    // structural-design + fire set in the SEAM's declared order, each a typed MeasureValue.OfSi (the TYPED 3-arg overload so
    // a SectionModulus never collides with a Volume by content key) converted from SI-millimetre to SI base (mm->m, mm2->m2,
    // mm3->m3, mm4->m4, mm6->m6). Iw is the FIFTH field (after J). The shear-area lift preserves the major/minor convention
    // (AvyMm2 MAJOR-axis/web -> AvY, AvzMm2 MINOR-axis/flange -> AvZ); the radii mirror it (RxMm major -> Major). The
    // asymmetric-section LTB columns the seam carries (ShearCentreY/ShearCentreZ length offsets, MonosymmetryFactor) lift the
    // matching ComputedSection columns (ShearCentreYMm/ShearCentreZMm the centroid->shear-centre offsets mm->m, Monosymmetry
    // Factor dimensionless) — engineering-zero for every doubly-symmetric/parametric family (the seam IsDoublySymmetric
    // reading zero-as-symmetric is EXACT there) and non-zero for an open thin-walled channel/tee/angle the steel#STEEL_FAMILY
    // SteelStiffness fills, so a singly-symmetric steel shape's flexural-torsional-buckling inputs cross the seam faithfully
    // rather than zeroing past it. Named arguments PIN each lift to its seam field so a future seam re-order cannot silently re-slot a column.
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
            Depth: Len(c.DepthMm.Value), Width: Len(c.WidthMm.Value), HeatedPerimeter: Len(c.HeatedPerimeterMm.Value), AxisDistance: Len(c.AxisDistanceMm),
            ShearCentreY: Len(c.ShearCentreYMm), ShearCentreZ: Len(c.ShearCentreZMm), MonosymmetryFactor: c.MonosymmetryFactor);
    }

    // --- [CONTENT_MINT]
    // Content-id mint — re-stamps a non-rooted node's id from its OWN canonical bytes (id-excluded, the seam
    // Graph/element#NODE_MODEL ToCanonicalBytes + the kernel seed-zero XxHash128 the Projection/address#CONTENT_ADDRESS
    // ContentAddress composes; this folder owns no hasher) through the seam Relabel re-stamp. A class-root [Union] Node case
    // has NO compiler-generated `with`, so Relabel — never a `draft with { Id }` a class case cannot honour. Two specs
    // minting the same node collapse to one id, idempotent under re-projection.
    static Node Mint(Node draft, double tolerance) =>
        draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));

    // --- [OCCURRENCE_EDGES]
    // H12 (substance): author element→material (and element→appearance) Associate edges ONLY for bindings whose Element
    // ctx.Owns vouches. The short-circuit is on spec.Bindings.IsEmpty (genuinely no edge — the pure-Materials subgraph
    // usable in isolation), NOT ctx.ElementIds.IsEmpty: a spec that DOES carry bindings rails ProjectionFault.Unvouched for
    // any binding the context does not vouch (an empty context vouches none, so every binding faults) — gating on context
    // emptiness SILENTLY DROPPING a bound spec's edges is the [H12] violation.
    static Fin<GraphDelta> AuthorBindings(MaterialSpec spec, NodeId materialId, Option<NodeId> appearanceId, ProjectionContext ctx, GraphDelta delta) =>
        spec.Bindings.IsEmpty
            ? Fin.Succ(delta)
            : spec.Bindings.Fold(
                Fin.Succ(delta),
                (acc, binding) => acc.Bind(g => ctx.Owns(binding.Element)
                    ? Fin.Succ(BindElement(g, binding, materialId, appearanceId))
                    : ProjectionFault.Unvouched(ctx.Key, $"<associate-element-not-in-context:{binding.Element.Value}>")));

    static GraphDelta BindElement(GraphDelta delta, MaterialBinding binding, NodeId materialId, Option<NodeId> appearanceId) =>
        appearanceId.Match(
            Some: appearance => delta
                .Link(new Relationship.Associate(binding.Element, materialId, binding.Usage))
                .Link(new Relationship.Associate(binding.Element, appearance, new MaterialUsage.None())),
            None: () => delta.Link(new Relationship.Associate(binding.Element, materialId, binding.Usage)));

    // H12 (Type): bind each VOUCHED occurrence to the minted Type via Assign.TypeDefinition (REUSED, never an IFC
    // DefinesByType name — the seam carries the neutral AssignKind). The occurrence is the sited piece the model author /
    // Bim ingest already rooted (in ctx.ElementIds); the Type is the identity THIS projection minted (NOT vouched — the
    // owner mints its own Type id). The short-circuit is on spec.Occurrences.IsEmpty (a pure type-authoring run with no
    // sited pieces yet), NEVER ctx.ElementIds.IsEmpty: a spec that DOES carry occurrences rails Unvouched for any occurrence
    // the context does not vouch (the [H12] gate), never a silently-dropped type binding.
    static Fin<GraphDelta> AuthorOccurrences(Seq<NodeId> occurrences, NodeId typeId, ProjectionContext ctx, GraphDelta delta) =>
        occurrences.IsEmpty
            ? Fin.Succ(delta)
            : occurrences.Fold(
                Fin.Succ(delta),
                (acc, occurrence) => acc.Bind(g => ctx.Owns(occurrence)
                    ? Fin.Succ(g.Link(new Relationship.Assign(occurrence, typeId, AssignKind.TypeDefinition)))
                    : ProjectionFault.Unvouched(ctx.Key, $"<type-occurrence-not-in-context:{occurrence.Value}>")));

    // --- [DETAIL]
    // The neutral realization-detail bag the seam Properties/property#DETAIL_SCHEMA declares (the DetailSchema.Realization
    // SetName + the canonical PropertyName vocabulary + the JointTypes allowed set; the IFC Rasm_ConnectionRealization Pset
    // name + the IFC predefined enums stay Bim-only). ONE polymorphic Detail over the ComponentSection arm: the four
    // standardized-PART families (reinforcement/fastener/connector/joint) carry a bar/thread/throat realization detail, and
    // the PANEL family a board layup/fastening/deck-rib realization detail (a generator round-trips the EdgeProfile + board
    // thickness + field/edge fastener stations + deck rib depth/pitch the baked ComputedSection cannot carry — a flat
    // sheathing board needs the fastening pattern even though it has no profile); the five OTHER profiled families (masonry/
    // cmu/steel/timber/glazing) carry NONE — their parametric data rides the baked ComputedSection + the Type Object
    // geometry, never a detail bag. The bag is bound to the Type Object via Assign.PropertyDefinition and re-read one-hop off
    // the seam Bake type-bag; the IMPORT round-trip splits by element genus, NOT one reader: the four realizing-element
    // families (IfcMechanicalFastener/IfcFastener/IfcDiscreteAccessory/IfcReinforcing*) round-trip through the Rasm.Bim
    // Semantics/connection#CONNECTION_DETAIL realizing-element reader, while the PANEL (an IfcBuiltElement covering/plate/slab
    // the connection reader's realizing-only Detail switch folds to its empty _ arm) round-trips through the GENERAL Bim
    // Object/property fold (Projection/egress#IFC_EGRESS ReauthorProperties on egress, Projection/semantic#SEMANTIC_PROJECTOR
    // Bags on ingest) — the SAME DetailSchema.Realization shape either way, differing only in which Bim reader recovers it.
    // The primary token
    // (BarType/FastenerType/AccessoryType) is the Component's PredefinedToken; the connector's SECOND FastenerType row is the
    // SEPARATE attaching IfcMechanicalFastenerTypeEnum (ConnectorType.IfcFastenerType) the Bim egress relates, distinct from
    // the IfcDiscreteAccessory the connector IS. The section's PositiveMagnitude columns coerce mm->SI-base through the
    // dimension-only MeasureValue.OfSi (the IFC scalars are SI-base, so an authored bag and an imported one content-key
    // identically). The JointType modality matches the DetailSchema.Realization closed allowed-set (the panel arm's "Fastened"
    // the screwed/nailed-board modality the seam allowed-set carries beside Bolted/Welded/Bonded/Bearing/Cast); the material
    // grade rides the substance Material subgraph (the Associate edge), never a row here.
    static Option<PropertyBag> Detail(Component item) => item.Section.Switch(
        masonry:       static _ => Option<PropertyBag>.None,
        cmu:           static _ => Option<PropertyBag>.None,
        steel:         static _ => Option<PropertyBag>.None,
        timber:        static _ => Option<PropertyBag>.None,
        glazing:       static _ => Option<PropertyBag>.None,
        reinforcement: r => Some(Rows(
            Joint("Cast"),
            Token(DetailSchema.BarType, item.PredefinedToken),
            Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, r.Bar.DiameterMm.Value * 1e-3),
            Measured(DetailSchema.CrossSectionArea, Dimension.AreaDim, r.Bar.NominalAreaMm2.Value * 1e-6))),
        fastener: f => Some(Rows(
            Joint("Bolted"),
            Token(DetailSchema.FastenerType, item.PredefinedToken),
            Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, f.Bolt.ThreadDiameterMm.Value * 1e-3),
            Measured(DetailSchema.NominalLength, Dimension.LengthDim, f.Bolt.LengthMm.Value * 1e-3))),
        connector: c => Some(Rows(
            Joint("Bolted"),
            Token(DetailSchema.AccessoryType, item.PredefinedToken),                        // the IfcDiscreteAccessoryTypeEnum the connector IS (= ConnectorType.IfcAccessoryType)
            Token(DetailSchema.FastenerType, c.Hardware.Type.IfcFastenerType),              // the SEPARATE attaching IfcMechanicalFastenerTypeEnum the Bim egress relates
            Measured(DetailSchema.CarriedMemberWidth, Dimension.LengthDim, c.Hardware.CarriedMemberWidthMm.Value * 1e-3),
            Measured(DetailSchema.CarriedMemberDepth, Dimension.LengthDim, c.Hardware.CarriedMemberDepthMm.Value * 1e-3))),
        joint: j => Some(j.Continuous.Switch(
            weld: w => Rows(
                Joint("Welded"),
                Token(DetailSchema.FastenerType, item.PredefinedToken),
                Measured(DetailSchema.EffectiveThroat, Dimension.LengthDim, j.Continuous.EffectiveThroatMm * 1e-3),  // the DERIVED structural throat (0.707·leg fillet / CJP-PJP groove / flare-factor·radius), NOT the nominal SizeMm
                Measured(DetailSchema.NominalLength, Dimension.LengthDim, w.LengthMm.Value * 1e-3)),
            adhesive: a => Rows(
                Joint("Bonded"),
                Token(DetailSchema.FastenerType, item.PredefinedToken),
                Measured(DetailSchema.BondLine, Dimension.LengthDim, j.Continuous.NominalMm.Value * 1e-3),
                Measured(DetailSchema.Overlap, Dimension.LengthDim, a.OverlapMm.Value * 1e-3)),
            stud: s => Rows(
                Joint("Welded"),
                Token(DetailSchema.FastenerType, item.PredefinedToken),
                Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, j.Continuous.NominalMm.Value * 1e-3),
                Measured(DetailSchema.NominalLength, Dimension.LengthDim, s.LengthBeforeWeldMm.Value * 1e-3)))),
        // The PANEL board's neutral generative-realization detail a sheathing generator round-trips off the seam: the
        // EdgeProfile token + the board thickness, the FastenPattern field/edge fastener stations, and (steel deck only)
        // the DeckRib depth/pitch the ComputedSection cannot carry. Joint("Fastened") is the panel realization modality
        // (the DetailSchema.Realization allowed-set token panel boards key on, distinct from the four discrete-part
        // Bolted/Welded/Bonded modalities). The Rib rows ride p.Board.Rib's Some only — a non-deck board (gypsum/sheathing/
        // ply/OSB/cement/rigid-board) carries Rib None, so a flat board's bag omits the rib rows entirely and content-keys
        // distinctly from a deck's; EdgeSpacing/FieldSpacing read p.Board.Fastening (the FastenPattern the SheetCoursing
        // layout reads), EdgeProfile the p.Board.Edge token, PanelThickness the p.Board.ThicknessMm board build.
        panel: p => Some(p.Board.Rib.Match(
            Some: rib => Rows(
                Joint("Fastened"),
                Token(DetailSchema.EdgeProfile, p.Board.Edge.Key),
                Measured(DetailSchema.PanelThickness, Dimension.LengthDim, p.Board.ThicknessMm.Value * 1e-3),
                Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, p.Board.Fastening.FieldSpacingMm.Value * 1e-3),
                Measured(DetailSchema.EdgeSpacing, Dimension.LengthDim, p.Board.Fastening.EdgeSpacingMm.Value * 1e-3),
                Measured(DetailSchema.RibDepth, Dimension.LengthDim, rib.DepthMm.Value * 1e-3),
                Measured(DetailSchema.RibPitch, Dimension.LengthDim, rib.PitchMm.Value * 1e-3)),
            None: () => Rows(
                Joint("Fastened"),
                Token(DetailSchema.EdgeProfile, p.Board.Edge.Key),
                Measured(DetailSchema.PanelThickness, Dimension.LengthDim, p.Board.ThicknessMm.Value * 1e-3),
                Measured(DetailSchema.FieldSpacing, Dimension.LengthDim, p.Board.Fastening.FieldSpacingMm.Value * 1e-3),
                Measured(DetailSchema.EdgeSpacing, Dimension.LengthDim, p.Board.Fastening.EdgeSpacingMm.Value * 1e-3)))));

    // --- [ROWS]
    // The bag-row constructors composing the seam DetailSchema.Realization conforming bag (the neutral SetName + the
    // OccurrenceWins precedence pinned by the schema, neither hand-spelled) so each arm is a flat declarative list, never a
    // repeated MeasureValue.OfSi construction. Joint is the JointType row VALUE through DetailSchema.Realization.Joint(kind)
    // (the PropertyValue.Enumerated over the schema's CLOSED allowed-set the Bim egress facet validates against — never a
    // local Enumerated re-spelling the allowed set). The Measured SI value carries the DIMENSION-only QuantityType (the
    // dimension-only OfSi overload Bim uses) so an authored and an imported NominalDiameter content-key identically; Rows
    // folds the rows into DetailSchema.Realization.Bag() through ValueBag.With (last-write-wins).
    static (PropertyName, PropertyValue) Joint(string kind) => (DetailSchema.JointType, DetailSchema.Realization.Joint(kind));
    static (PropertyName, PropertyValue) Token(PropertyName name, string value) => (name, new PropertyValue.Text(value));
    static (PropertyName, PropertyValue) Measured(PropertyName name, Dimension dim, double si) => (name, new PropertyValue.Measure(MeasureValue.OfSi(dim, si)));

    static PropertyBag Rows(params (PropertyName Name, PropertyValue Value)[] rows) =>
        rows.ToSeq().Fold(DetailSchema.Realization.Bag(), static (bag, r) => bag.With(r.Name, r.Value));
}
```

## [03]-[COMPONENT_SUBGRAPH]

- Owner: the `ComponentSubgraph` capture composition root — `Capture` (the homogeneous-substance fold building `Substance` specs, the prior `MaterialSubgraph.Capture` UNCHANGED) and `CaptureComponent` (the catalogue-`Component` fold building a `Type` spec) — plus the projector's `ProjectType` node authoring (the minted Type `Object`, the M7 `SeamSection` bake onto the twenty-field seam `SectionProperties`, the neutral `DetailSchema` realization bag) and the `ProjectSubstance` node authoring (the `Material`/`Appearance` content-keyed nodes + the `[H12]`/`[C7]` `Associate` edge), the family-specific composition selection delegated to the re-homed `Component/timber#TIMBER_FAMILY` and `Component/glazing#GLAZING_FAMILY` lowerings.
- Cases: two projected node families over one fold — the SUBSTANCE family (`Material`: the `MaterialId` + the M7-baked `MaterialComposition` + the `Discipline`-keyed `MaterialPropertySet` set, content-keyed · `Appearance`: the `Appearance/interchange#MATERIAL_WIRE` content-keyed `AppearanceSummary`) and the TYPE family (the rooted-deterministic Type `Object` + its structural `Material` with the baked section + its optional `Appearance` + its optional neutral `PropertySet` detail bag); the projector authors NO `Assessment` node (`Rasm.Compute` reads the `Material` plies directly and writes the `Assessment` `Result`). The capture entries: `Capture` folds a `Seq<MaterialId>` of library materials into `Substance` specs (the REQUIRED engineering set + the OPTIONAL lifecycle set + the OPTIONAL Object-node `Classification` riding the binding + the `[C7]` usage), `CaptureComponent` lowers a catalogue `Component` + its family-selected composition + its occurrences into a `Type` spec.
- Entry: `ComponentSubgraph.Capture(materials, elementOf, sections, key)` builds the source's `Substance` specs (each spec the seam `MaterialId` + the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.Single` composition + the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` REQUIRED engineering set + the `Properties/sustainability#SUSTAINABILITY_PROPERTY` OPTIONAL lifecycle set + the Object-node `Classification` egress + the `Appearance/graph#MATERIAL_LIBRARY`→`MATERIAL_WIRE` content-keyed `AppearanceSummary` + the `[C7]` `CompositionAuthor.UsageOf` binding), the entire fold on the ONE `Fin` rail so a from-scratch capture short-circuits on the first unregistered material; `ComponentSubgraph.CaptureComponent(source, component, composition, occurrences, key)` adds a `Type` spec — the `component.CapacityKey` engineering rows (REQUIRED) + the `component.AppearanceId` library appearance — the family lowering (`Component/timber#TIMBER_FAMILY` `ToLayerSet` for a CLT panel, `Component/glazing#GLAZING_FAMILY` `ToLayerSet` for an IGU, the seam `MaterialComposition.OfProfileSet` for a profiled steel/timber/cmu member) having selected the structural `MaterialComposition` (a `ProfileSet` for steel/timber/cmu the `ProjectType` `BakeSection` resolves the section onto, a `LayerSet` for a glazing IGU, a `Single` for a discrete part); the projector's `ProjectType` mints the Type subgraph + the `SeamSection` bake + the neutral `DetailSchema` bag from there.
- Packages: Rasm.Element (project — `Node` cases, `MaterialComposition`, `MaterialPropertySet`, `Relationship.Associate`/`Assign`, `MaterialUsage`, `Classification`, `AppearanceSummary`, `ProfileRef`, `ResolvedComponent` consumed via the section table), Rasm.Materials.Component (project — `Component`/`ComponentResolution`/`ResolvedComponent` the M7 table, `Component/timber`/`glazing` the re-homed family lowerings), Rasm.Materials.Properties (project — `MaterialPropertyCatalogue`/`SustainabilityCatalogue` the engineering/lifecycle/classification rows), Rasm.Materials.Construction (project — `CompositionAuthor` the `Single`/`UsageOf` composition author), Rasm.Materials.Appearance.Graph + Rasm.Materials.Appearance.Interchange (project — `MaterialLibrary`/`MaterialWire` the appearance lowering), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Option` for the capture folds).
- Growth: a new engineering discipline routed to a material is one `Discipline` row the seam `MaterialPropertySet` carries (Compute's route dispatch reads it off the `Material` node) — no capture arm; a new family's Type capture is one `CaptureComponent` call over the family's catalogue `Component` + its lowering-selected composition (the family page owns the composition selection, never the projector); a new realization-detail row is one `DetailSchema` `PropertyName` the projector `Detail` reads; the subgraph grows by seam case, catalogue row, and family lowering, never a new node author.
- Boundary: the family lowerings own the COMPOSITION SELECTION (`ProfileSet` for a steel/timber/cmu profiled member the `SeamSection` bakes the section onto, `LayerSet` for a glazing IGU / a CLT panel, `Single` for a discrete part / a homogeneous unit) — `CaptureComponent` threads the family-selected `MaterialComposition`, never forcing one shape, and the `ProfileSet` carries the M7 `ProfileRef` the `ProjectType` `BakeSection` path resolves from `source.Sections` so the structural runner reads `graph.SectionOf(member)` without re-resolving; the REQUIRED-vs-OPTIONAL `Lookup` asymmetry holds (`MaterialPropertyCatalogue.Lookup` rails the seam `ElementFault.ValueRejected` on an unregistered material — engineering properties REQUIRED; `SustainabilityCatalogue.Lookup` returns `Fin.Succ(empty)` for a material with no declared EPD — lifecycle OPTIONAL; `SustainabilityCatalogue.Classification` returns `None` when row or material absent); the resolved standard `Classification` rides the `MaterialBinding` to the bound element's `Object` node (an Object-node VALUE — NOT a `Node.Material` field, NOT an `Associate` edge payload — the `Relations/relation#EDGE_ALGEBRA` law), the Object-node owner (`Rasm.Bim` at IFC ingest, or the from-scratch app) folding it into the element's `Classifications` set the Bim `Semantics/classification` re-emits; the `Material` node carries its OWN `MaterialPropertySet` (the material physics keyed by `Discipline`) and NEVER an element-level `PropertySet`/`QuantitySet` bag (those are `Rasm.Bim`'s at IFC ingress); the neutral `DetailSchema` realization bag is authored for the four standardized-part families AND the panel family (the panel bag carrying the board `EdgeProfile`/`PanelThickness`, the `FieldSpacing`/`EdgeSpacing` fastener stations, and the steel-deck `RibDepth`/`RibPitch` a generator lays boards from; the OTHER five profiled families carry NONE — their data rides the baked `ComputedSection` + the Type Object geometry), the connection's MATERIAL binding (its steel grade, capacity, embodied carbon, appearance) riding the structural `Material` subgraph, NEVER a row on the detail bag — a `SteelGrade`/`EmbodiedCarbon` detail row is the named seam violation; the whole subgraph is one additive `GraphDelta` the seam `IGraphConstraint.Validate` gates for IFC-semantic legality before the seam folds it, the projector enforcing only the structural invariants (content-key idempotence, the Type-Object dedup, the context-vouched binding/occurrence endpoints).

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;                              // Op
using Rasm.Element;                             // MaterialId, NodeId, MaterialUsage, MaterialComposition, MaterialPropertySet, ProfileRef, Classification, AppearanceSummary
using Rasm.Materials.Component;                 // Component, ComponentResolution, ResolvedComponent (the M7 ProfileRef→ResolvedComponent table)
using Rasm.Materials.Properties;                // MaterialPropertyCatalogue, SustainabilityCatalogue
using Rasm.Materials.Construction;              // CompositionAuthor
using Rasm.Materials.Appearance.Graph;          // MaterialLibrary
using Rasm.Materials.Appearance.Interchange;    // MaterialWire (the AppearanceSummary lowering)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] --------------------------------------------------------------------------
// The capture composition root: fold the catalogues into the ComponentProjectionSource the ComponentProjector folds. The
// SUBSTANCE capture (homogeneous library materials → MaterialSpec) is the prior MaterialSubgraph.Capture UNCHANGED; the TYPE
// capture (a catalogue Component → ComponentSpec) is the new Type-minting source. The M7 ProfileRef→ResolvedComponent table
// (component#COMPONENT_RESOLUTION ComponentResolution.Build, supplied once by the caller so the section integral runs once
// per component, never per material) seeds the source's Sections. Every catalogue resolution rides the ONE Fin rail: the
// REQUIRED engineering set rails the seam ElementFault.ValueRejected for an unregistered material, the OPTIONAL lifecycle set
// returns Fin.Succ(empty), the OPTIONAL Object-node Classification (the SustainabilityCatalogue.Classification (system, code)
// egress, None when row or material absent) rides the MaterialBinding, and the C7 usage threads CompositionAuthor.UsageOf's
// Fin<MaterialUsage>; Rasm.Compute reads the projected Material node plies DIRECTLY and writes the Assessment result back —
// Materials authors no assessment-input node.
public static class ComponentSubgraph {
    // The HOMOGENEOUS-SUBSTANCE capture (UNCHANGED): one MaterialSpec per library material — the REQUIRED engineering set +
    // the OPTIONAL lifecycle set, the OPTIONAL Object-node Classification riding the binding, the C7 usage, the appearance.
    // A pure-substance run with no bound element (elementOf None) emits a content-keyed material subgraph with no dangling
    // edge; a bound run names the edge owner, every element vouched by ctx.Owns at Project.
    public static Fin<ComponentProjectionSource> Capture(
        Seq<MaterialId> materials,
        Func<MaterialId, Option<NodeId>> elementOf,
        FrozenDictionary<ProfileRef, ResolvedComponent> sections,
        Op key) =>
        materials.Fold(
            Fin.Succ(ComponentProjectionSource.Empty with { Sections = sections }),
            (acc, id) => acc.Bind(source =>
                from engineering in MaterialPropertyCatalogue.Lookup(id, key)                        // Properties/properties#MATERIAL_PROPERTY_CATALOGUE → seam Mechanical/Thermal/Acoustic/Fire (REQUIRED)
                from lifecycle in SustainabilityCatalogue.Lookup(id, key)                            // Properties/sustainability#SUSTAINABILITY_PROPERTY → seam Environmental/Cost (OPTIONAL, empty when absent)
                from classification in SustainabilityCatalogue.Classification(id, key)               // the material's Object-node Classification (the (system, code) egress, OPTIONAL — None when row or material absent)
                let properties = engineering + lifecycle                                             // the full Seq<MaterialPropertySet> the Material node carries (classification rides the Object node, never a property case)
                let composition = CompositionAuthor.Single(id)                                       // a homogeneous library material; layered/profiled compositions ride a Type spec's family lowering
                let appearance = MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary).ToOption()  // Appearance/graph#MATERIAL_LIBRARY → Appearance/interchange#MATERIAL_WIRE content-keyed AppearanceSummary
                from bindings in elementOf(id).Match(
                    Some: element => CompositionAuthor.UsageOf(composition, key).Map(usage => Seq(new MaterialBinding(element, usage, classification))),  // C7: thread the seam MaterialUsage Fin AND the Object-node Classification onto the binding
                    None: () => Fin.Succ(Seq<MaterialBinding>()))                                     // no bound element → a pure-Materials subgraph run, the resolved classification simply has no Object node to land on
                select source.With(new MaterialSpec(id, composition, properties, appearance, bindings))));

    // The TYPE capture: a catalogue Component + its family-selected MaterialComposition + its vouched occurrences → a
    // ComponentSpec the ProjectType fold mints into a Type Object. The structural-material engineering rows are the
    // Component.CapacityKey Mechanical row (REQUIRED), the appearance the Component.AppearanceId library row (OPTIONAL). The
    // composition arrives ALREADY SELECTED by the caller (the Component/timber#TIMBER_FAMILY ToLayerSet for a CLT panel and
    // Component/glazing#GLAZING_FAMILY ToLayerSet for an IGU, the seam MaterialComposition.OfProfileSet for a profiled
    // steel/timber/cmu member the ProjectType BakeSection resolves the section onto, a Single for a discrete part) — so the
    // projector never re-selects the shape.
    public static Fin<ComponentProjectionSource> CaptureComponent(
        ComponentProjectionSource source, Component component, MaterialComposition composition, Seq<NodeId> occurrences, Op key) =>
        from properties in MaterialPropertyCatalogue.Lookup(component.CapacityKey, key)              // the structural-material Mechanical row (REQUIRED — a Type's capacity material must register)
        let appearance = MaterialLibrary.Lookup(component.AppearanceId, key).Map(MaterialWire.Summary).ToOption()  // the AppearanceId render-row appearance (OPTIONAL)
        select source.With(new ComponentSpec(component, composition, properties, appearance, occurrences));
}
```

## [04]-[RESEARCH]

- [PROJECTOR_MERGE]: REALIZED — the prior `MaterialProjector` and `ConnectionProjector` collapse into ONE `ComponentProjector` whose single `Project` fold discriminates a `Substance(MaterialSpec)` arm from a `Type(ComponentSpec)` arm over the `ComponentProjectionSpec` `[Union]`. The dual-projector paradigm, the `ConnectionSpec`/`ConnectionSchedule` source pair (folded into `ComponentSpec` + `ComponentProjectionSource`), and the `ConnectionProjector` "authors NO `Object` node, mints NO element identity" stance are the deleted forms. One projector folds the whole subgraph; adding a future `Rasm.Fabrication` projector is one registration row the seam `Assemble` merges.
- [OWNER_MINTS_TYPE_IDENTITY]: the `[1].2` owner-mints-its-identity inversion — `Rasm.Materials` owns Component TYPES, so `ProjectType` MINTS the deterministic-rooted Type `Object` (REUSING the `Graph/element#NODE_MODEL` `ObjectKind.Type` static, deriving its rooted id from the seam `NodeId.RootedType` over `Node.Object.ToTypeSeedBytes` which EXCLUDES the volatile `Representations` so a later geometry attach never re-keys the type and identical `Component`s dedup to one Type), stamps `Classification("ifc", IfcEntity)` + `PredefinedType.Create(PredefinedToken)` off the `ComponentSection` egress projections, and binds vouched occurrences via `Assign.TypeDefinition` (REUSED, never an IFC `DefinesByType` spelling — the seam carries the neutral `AssignKind`). The Type `Object` minted here is NOT a context-vouched id; only the substance bindings' elements and the Type's occurrences are vouched, the resources content-keyed and owned by the minting projector. The Type mint composes the seam members `Graph/element#NODE_MODEL` `NodeId.RootedType(ReadOnlySpan<byte>)` (the deterministic-rooted Type derivation, stable under geometry attach) and `Node.Object.ToTypeSeedBytes(double)` (the canonical projection EXCLUDING `Representations` for the Type seed).
- [SECTION_BAKE_TWENTY_FIELD]: the M7 `SeamSection` bake (UNCHANGED in shape) lifts the TWENTY-field `component#COMPONENT_OWNER` `ComputedSection` onto the TWENTY-field seam `Composition/material#MATERIAL_COMPOSITION` `SectionProperties` column-for-column — the seam and the source receipt grew `ShearCentreY`/`ShearCentreZ`/`MonosymmetryFactor` in lockstep so the symmetric-only column set no longer leaves a channel/tee/angle un-flexural-torsional-buckling-checkable. `SeamSection` lifts the three asymmetry columns from the matching `ComputedSection.ShearCentreYMm`/`ShearCentreZMm`/`MonosymmetryFactor` (engineering-zero for every doubly-symmetric/parametric family, the seam `IsDoublySymmetric` reading zero-as-symmetric; non-zero for an open thin-walled channel/tee/angle the `steel#STEEL_FAMILY` `SteelStiffness` fills), and the `Iyy`/`Izz`/`J`/`Iw`/`Wely`/`Welz`/`Wply`/`Wplz`/`AvY`/`AvZ` lifts preserve the seam quantity-type tagging (a `SectionModulus` never colliding with a `Volume`). The cross-file widening LANDED: the `component#COMPONENT_OWNER` `ComputedSection` carries the three columns (zero-filled in the `ParametricSection` rectangle/hollow path), the `steel#STEEL_FAMILY` `SteelStiffness.Derive` computes the shear-centre offsets + SN030 β_y for the singly-symmetric open shapes, and `SeamSection` lifts them — so a PFC/tee/angle's EN 1993-1-1 §6.3.2 general-route inputs cross the seam faithfully, and the `component.md`/`steel.md` prose reads twenty-field throughout.
- [NEUTRAL_DETAIL_SCHEMA]: the `Rasm_ConnectionRealization` Bim round-trip SHAPE is PRESERVED (the neutral set-name analogue, the row vocabulary `JointType`/`FastenerType`/`AccessoryType`/`BarType`/`NominalDiameter`/`NominalLength`/`CrossSectionArea`/`CarriedMemberWidth`/`CarriedMemberDepth`/`EffectiveThroat`/`BondLine`/`Overlap`, the `OccurrenceWins` `InheritanceMode`, the dimension-only `MeasureValue.OfSi` columns, the closed `JointTypes` allowed set), but the literal IFC `Rasm_ConnectionRealization` Pset name + the IFC predefined enums move `Rasm.Bim`-only per `[1].5`; the seam `Properties/property#DETAIL_SCHEMA` `DetailSchema.Realization` carries the NEUTRAL realization-detail `SetName` + the `OccurrenceWins` `InheritanceMode` + the canonical `PropertyName` row vocabulary + the `JointTypes` allowed set, the `ComponentProjector` (author) composing `DetailSchema.Realization.Bag()`/`.Joint(kind)` (neither hand-spelling the set name nor re-stamping the precedence). The detail bag is the TYPE-level realization detail bound via `Assign.PropertyDefinition` (occurrences inherit it through the `Bake` type-bag merge), authored for the four standardized-PART families AND the panel family (the panel arm reading `EdgeProfile`/`PanelThickness`/`FieldSpacing`/`EdgeSpacing` always and `RibDepth`/`RibPitch` for a steel-deck board whose `Rib` is `Some`; the OTHER five profiled families carry NONE). The IMPORT reader differs by element genus over the ONE shared schema: the four realizing-element families round-trip through the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` realizing-element reader (targeting the same `DetailSchema.Realization`), while the panel — a `ComponentClass.Panel` `IfcBuiltElement` the realizing-only `Detail` switch folds to its empty arm — round-trips through the GENERAL Bim `Object`/property fold (`Projection/egress#IFC_EGRESS` `ReauthorProperties` on egress, `Projection/semantic#SEMANTIC_PROJECTOR` `Bags` on ingest). The seam `DetailSchema` is the `Properties/property#DETAIL_SCHEMA` owner this projection composes; the panel rows the arm reads — the six new `static readonly PropertyName` statics `EdgeProfile`/`PanelThickness`/`FieldSpacing`/`EdgeSpacing`/`RibDepth`/`RibPitch` AND the new `Fastened` token on `DetailSchema.Realization.JointTypes` (the screwed/nailed-board modality beside `Bolted`/`Welded`/`Bonded`/`Bearing`/`Cast`) — are a `Properties/property#DETAIL_SCHEMA` seam growth the WF-2 reconcile lands at the seam owner, exactly as the connector's `CarriedMemberWidth`/`CarriedMemberDepth` rows landed; the egress that maps the neutral `SetName` onto the IFC `Rasm_ConnectionRealization` Pset name is the `Rasm.Bim` `Projection/egress#IFC_EGRESS` `ReauthorProperties` for every detail-bag family (realizing or panel).
- [CLASS_ROOT_MINT]: `Mint` composes the seam `Node.Relabel` re-stamp — a class-root `[Union]` `Node` case has NO compiler-generated `with`, so the prior `draft with { Id = … }` (the form the old `MaterialProjector`/`ConnectionProjector` `Mint` carried) is the deleted form a class case cannot honour; the rebuild re-stamps the content-derived id through `Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span))`.
- [H12_OCCURRENCE_VOUCH]: the `ProjectionContext.Owns` vouch is per-binding (substance) and per-occurrence (Type) — the substance arm short-circuits on `spec.Bindings.IsEmpty`, the Type arm on `spec.Occurrences.IsEmpty` (genuinely nothing to bind — the pure-isolation subgraph), NEVER on `ctx.ElementIds.IsEmpty`: a spec that DOES carry bindings/occurrences rails `ProjectionFault.Unvouched` for every element an empty context cannot vouch (the [H12] gate), never a silently-dropped edge.
- [CONTENT_ADDRESSING]: every authored non-rooted node is minted via the seam `NodeId.Content` over its own `ToCanonicalBytes(tolerance)` (the `Projection/address#CANONICAL_WRITER` codec + the kernel seed-zero `XxHash128` the `Projection/address#CONTENT_ADDRESS` `ContentAddress` composes — the ONE hasher, NO second in this folder), so a `Material`/`Appearance`/detail node projected twice collapses to one node and the delta is idempotent under re-projection; the Type `Object`'s rooted `NodeId.RootedType` derives from its representation-excluded seed so identical Components dedup. The cross-runtime C#/Python/TypeScript parity corpus pins byte-for-byte agreement on the canonical bytes.
- [ASSESSMENT_OWNERSHIP]: `Rasm.Materials` authors NO `Assessment` node — the material's own `Discipline`-keyed `MaterialPropertySet` set on the projected `Material` node IS the assessment input `Rasm.Compute` reads DIRECTLY (above the seam, `id => graph.Material(id).Map(static m => m.Properties)`), runs the discipline route, and writes the seam `Assessment` `Result` node content-keyed on `(input key, route)`; the multi-ply `AssemblyAggregator` is `Rasm.Compute`'s, the seam carrying the per-material property set INPUT only.
- [IGRAPH_CONSTRAINT_GATE]: the projected `GraphDelta` is gated by the seam's second interface `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` (`[M3]`, `Rasm.Bim`-implemented with IFC-semantic legality — a `Type` may not aggregate an `Occurrence`, a predefined token must be in the entity's valid set) before the seam folds it; the projector enforces only the STRUCTURAL invariants it owns (content-key idempotence, the Type-Object dedup, context-vouched binding/occurrence endpoints) and defers IFC legality to the constraint, so the two interfaces stay orthogonal.
