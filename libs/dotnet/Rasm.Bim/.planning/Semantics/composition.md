# [BIM_MATERIAL_COMPOSITION]

`MaterialProjection` is the IFC material projector lowering the live GeometryGym `IfcMaterialSelect` surface onto the `Rasm.Element` seam `Material` node: `MaterialProjection.Project` discriminates the relating-material runtime entity — `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` — and folds it into one content-keyed seam `Node.Material` carrying the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`). `Rasm.Element` OWNS the construction-material algebra (`MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `ProfileRef`, `MaterialPropertySet`, `MaterialId`); this page owns ONLY the GeometryGym discrimination that fills it, never re-declaring a Bim `BimMaterial`/`BimMaterialComposition` — the retired `BimMaterial` record and the `BimElement.Materials` column are GONE, a material is a seam `Material` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, and the consumer reads `element.Materials` flat on the baked element rather than a second stored record keyed by `MaterialId`. Occurrence usage binding (layer direction/sense/offset, profile cardinal-point/extent) does NOT live here — it rides the `Associate` edge `MaterialUsage` payload the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors [OCCURRENCE_USAGE_RULING], this owner producing only the type-level SET structure so a wall and its mirror share one `LayerSet` with two `Associate` usages. Linear members carry their section as a neutral `ProfileRef` (`Standard` + `Designation` + content key) the `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog [M7], its full `IfcProfileDef` parametric definition preserved in the content-addressed store the `ContentKey` keys — a compound set's declared `IfcCompositeProfileDef` `CompositeProfile` keyed over the primary row so a built-up section's combined geometry survives the store and re-stamps at egress — the page references NO VividOrange section type and folds NO parametric dimension onto the seam, because the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam. `MaterialProjection` stays HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the profile geometry by content-hash reference, never a RhinoCommon type. Unresolvable material-select entities rail `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` lifted BARE (band 2600 owns the generated `Code`; no `.ToError()` hop); a degenerate composition (empty set, non-positive layer thickness, unnormalized constituent fractions) rails the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` the seam `MaterialComposition` admission owns.

Every native-unit magnitude crossing this projector rides the ONE `Projection/value#UNIT_INGRESS` `UnitScheme` regime — `Coerce` on ingress, `Declare` on egress — threaded from the projection's single regime, so no member on this page returns a bare multiplier and the mm-vs-metre import trap a Revit export springs closes at one entry [`Projection/semantic#SEMANTIC_PROJECTOR`]. Every GeometryGym nullable name crossing the boundary narrows at the ONE `Projection/value#PROPERTY_LOWERING` `Stated` door through the `MaterialShape.Key`/`Label` carriers, so absence never coalesces to a blank inside a domain body. `MaterialProjection` runs BIDIRECTIONAL: its `[03]-[EGRESS]` half is the inverse the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authors the type-level `MaterialComposition` back onto the GeometryGym material-definition family (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial`) ONCE per material and lowers the seam `MaterialPropertySet` set onto TYPED `IfcMaterialProperties` `Pset_Material*` rows through one column table (every case field round-trips — the `FireRating` class, the `Cost` `Currency`/`MeasurementBasis`, and the EPD id + expiry as the shared base-`Evidence` label columns, each row carrying the GeometryGym `IfcValue` leaf its own datum names, never coerced to a lossy double, and the `Environmental` rows spread the FULL EN 15978 per-`LifecycleStage` GWP vector one column per module, never a single aggregate sum, the newer `Optical`/`Damping`/`Hygrothermal`/`Durability` carriers each their own Pset), and `AuthorUsage` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge `MaterialUsage` carries [OCCURRENCE_USAGE_RULING]. This is the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — `Rasm.Bim` reads the projected `Material` subgraph directly, never a Materials wire. `ProfileSet` reconstitutes its full parametric `IfcProfileDef` one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the seam holds only the neutral `ProfileRef` + baked `SectionProperties`); a store-missed Rasm-authored profile authors as the entity the carried `DetailSchema.Realization` `ProfileSubtype` row names off the baked dims where the token's geometry completes, an unresolvable profile railing `BimFault.Refused` with `BimReason.DanglingReference`.

## [01]-[INDEX]

- [02]-[MATERIAL_COMPOSITION]: `MaterialProjection.Project` the `IfcMaterialSelect`→seam `Node.Material` ingress fold, the four per-modality `LayerSetOf`/`ProfileSetOf`/`ConstituentSetOf`/`SingleOf` builders mapping onto the seam `MaterialComposition`, the per-row `ProfileRef` content-keyer beside the `CompositeOf` set-level outline, the `LayersOf`/`ProfilesOf` row folds with their `MaterialShape` column carriers and `OffsetsOf` projections, the content-keyed `Mint` of the seam `Node.Material`, and the `ImportedPsets` reader lowering `HasProperties` material Psets to neutral seam `PropertyBag`s on the `Projection/fidelity#FIDELITY_LEDGER` writer carrier.
- [03]-[EGRESS]: `MaterialProjection.AuthorComposition`/`AuthorUsage` the inverse half re-authoring a seam `Material` node onto the GeometryGym material-definition family + the `MaterialPropertySet`→`IfcMaterialProperties` `Pset_Material*` column table beside the `EvidenceRows` provenance tail + the `Associate`-edge `MaterialUsage`→`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [OCCURRENCE_USAGE_RULING], over the `EmitMemo<TKey,TValue>` db-scoped entity memo this cluster declares and the classification egress composes.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialProjection` the static BIDIRECTIONAL GeometryGym↔seam material projector, this cluster owning its `Project` ingress — folding one `IfcMaterialSelect` runtime entity into one seam `Node.Material` (discriminating the entity, building the seam `MaterialComposition` through the seam smart-constructors, minting the content-keyed node id) — beside `MaterialShape` the `[Mapper]` boundary transcription owning the mechanical column crossings BOTH halves compose (the `IfcMaterial`→`MaterialId` substance carrier over the ONE `Key` nullable-name admission, the `int.MinValue`→`Option<int>` junction sentinel, the `IfcLogicalEnum`→`Option<bool>` ventilation narrowing, and the whole `IfcMaterialConstituent`→`MaterialConstituent` row). `Rasm.Element` owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` rows, the `ProfileRef`, the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage`, the `ValueBag<V>` bag an imported Pset lands in, and the `MaterialPropertySet` engineering-property family; this page declares NONE of them — it composes the seam vocabulary, mapping the GeometryGym material-assembly entities onto it and back.
- Entry: `MaterialProjection.Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, UnitScheme scale, Op key)` is the live-entity promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes when folding an `IfcRelAssociatesMaterial.RelatingMaterial` (the parameter IS the typed `IfcMaterialSelect` the property carries — a `BaseClassIfc` admission is the deleted weak form) — dispatching the runtime entity onto ONE builder per seam union arm (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` and `IfcMaterialProfileSetUsage` its `ForProfileSet` into the same two set builders the bare sets reach, the usage payload riding the `Associate` edge not this node; `ConstituentSetOf` folds `MaterialConstituents.Values`; `SingleOf` reads the bare substance once for both the node key and the `Single` case) — and returns one content-keyed seam `Node.Material`; `MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key)` is the peer ingress reading the imported `HasProperties` material Psets as neutral seam `PropertyBag`s on the `Projection/fidelity#FIDELITY_LEDGER` `WriterT<FidelityLog, Fin, A>` carrier, the narrowing's own drops returned beside the bags, the `Projection/semantic#SEMANTIC_PROJECTOR` `Materials` fold content-minting each bag as a `Node.PropertySet` node the `Projection/relations#RELATION_ALGEBRA` `MaterialEdges` fold binds by one `Assign.PropertyDefinition` edge; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected`) and the seam `MaterialComposition` admission aborts a degenerate set (`ElementFault.ValueRejected`), each lifting BARE (the band owns the generated `Code`; no `.ToError()` hop).
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and builds the seam `MaterialComposition` through the seam `Of`-prefixed smart-constructors (the `Fin`-railing `MaterialComposition.OfLayerSet`/`OfConstituentSet` owning the empty-set / non-positive-thickness / unnormalized-fraction admission, the total `OfSingle`/`OfProfileSet` lifted into `Fin` for the `Mint` fold), then mints the seam `Node.Material` whose id is the kernel seed-zero `XxHash128` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical materials dedup to one node; `LayersOf` folds each `IfcMaterialLayer` onto a seam `MaterialLayer` carrying its `MaterialId`, a `MeasureValue` thickness whose native `LayerThickness` coerces to SI metres through `UnitScheme.Coerce` over the length axis and admits through the QTO-identity `MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, …)` (a layer build-up thickness IS a length takeoff the `Semantics/properties#BASE_QUANTITIES` fold reads, so a dimension-anonymous admit strips the QTO read off every derived-wins row), and its layer name; `MaterialShape.Row` transcribes each `IfcMaterialConstituent` (read through the `Dictionary.Values`) onto a seam `MaterialConstituent` carrying its `MaterialId`, category, `Fraction`, and part name; `ProfileRefOf` projects the KEYED section — the set's declared `IfcCompositeProfileDef` `CompositeProfile` when a compound set carries one (the combined built-up geometry, decompile-confirmed settable), else the primary row's `Profile` — onto a neutral `ProfileRef` whose `ContentKey` is the kernel seed-zero `XxHash128` `ContentHash.Of` over the tag-namespaced `IfcProfileDef` STEP (the full parametric section preserved in the content-addressed store; the ONE kernel hasher the `Model/elements#REPRESENTATION_KEYS` keyer also composes, never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]), the `Designation` the row profile's name, the `Standard` left to the one-hop catalog resolution; `ProfilesOf` folds EVERY `IfcMaterialProfile` row onto a seam `MaterialProfile` — its own material under the row-then-set name fallback the one `MaterialShape.Key` admission owns, its own content-keyed `ProfileRef`, its `[0,100]` junction `Priority` through `MaterialShape.Junction`, its function `Category`, and its `IfcMaterialProfileWithOffsets.OffsetValues` reference-axis offsets SI-coerced through the same `UnitScheme` entry — and `CompositeOf` preserves the set's declared `IfcCompositeProfileDef` as the seam `Composite`, so a built-up compound keeps every plate where the primary-only read kept one; `MaterialShape.Junction` is the `[SENTINEL_PROJECTION]` site retiring GeometryGym's `int.MinValue` unset priority to `None`, and `MaterialShape.Ventilation` narrows the three-state `IfcLogicalEnum` onto the seam `Option<bool>` so an `UNKNOWN` ventilation never reads as `FALSE` (the EN ISO 6946 falsification); the row `Description` is the one IFC annotation column the seam declines, carrying no analytical read where `Category` drives the assembly fold. Typed engineering property sets stay the AUTHORED lane's (the `Rasm.Materials` `ComponentProjector` lowers its catalog-backed `MaterialPropertySet` rows), so the IFC-ingest `Node.Material` carries an empty `Seq<MaterialPropertySet>` and `ImportedPsets` lands the imported `IfcMaterialDefinition.HasProperties` sets as NEUTRAL seam `PropertyBag`s instead — foreign set name, `EvidenceGrade.Import` rank, `PropertyInheritance.ModeOf` precedence, values narrowed through the one `PropertyLowering.Lower` — because a partial imported Pset folded onto a full-vector typed case fabricates every undeclared column.
- Receipt: the seam `Node.Material` is the material evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Associate` edge into `element.Materials` (a `BakedMaterial` carrying the node with its occurrence `MaterialUsage` — the seam Bake-folded accessor, DISTINCT from the `Rasm.Materials` projection-input `MaterialBinding` and the type→occurrence `TypeBinding`), the `Model/query#ELEMENT_SET` material predicate matches by `MaterialId` or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#BASE_QUANTITIES` layered-volume takeoff reads from the `LayerSet` thicknesses; the imported-Pset lowering returns its own `FidelityLog` on the writer carrier as well, so which foreign values narrowed is per-exchange evidence rather than a silent read; the layer build-up, the section material, and the constituent mix each carry their real composition on one seam node, never a parallel layer/profile/constituent record family.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, Riok.Mapperly, LanguageExt.Core, NodaTime
- Growth: a new material-assembly modality is one seam `MaterialComposition` union arm (the seam owns it, not this page) and one `Project` arm naming its own `*Of` builder; a new assembly-row field is one column on the seam `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` filled by its owning `LayersOf`/`MaterialShape.Row`/`ProfilesOf` fold and re-stamped by its `[03]-[EGRESS]` peer; a new section catalog is one `ProfileRef.Standard` token the `Rasm.Materials` projector resolves, never a seam edit; a new mechanical column crossing is one `[UserMapping]` carrier on `MaterialShape`, never a second hand-rolled narrowing beside it; never a per-element-class material type, never a Bim `BimMaterial` record beside the seam node, and never a parallel material store.
- Boundary: the material model is the seam `Node.Material` + `MaterialComposition` and a Bim `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile`/`LayerSetUsage`/`ProfileSetUsage`/`ProfileDefKind`/`ProfileDims` re-declaration is the deleted form — the seam owns the algebra, this page owns only the GeometryGym discrimination that fills it; the occurrence usage rides the `Associate` edge `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and threading `LayerSetUsage`/`ProfileSetUsage` onto this composition node is the named seam violation — the type-level SET structure is shared, the per-occurrence geometric binding is the edge's; the `ProfileSet` arm carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section type and NOT inline `IfcParameterizedProfileDef` dimensions — the full parametric section is preserved in the content-addressed store the `ContentKey` keys and the canonical section properties resolve one-hop to the catalog above the seam, so a profile-name-only `ProfileRef` that drops the content key is the deleted form, the content key is the kernel seed-zero `XxHash128` `ContentHash.Of` (the up-stratum `Rasm.Compute` `InterchangeIdentity` being the H7 strata defect), and a compound set preserves BOTH levels — every row's own profile AND the declared `CompositeProfile` as the seam `Composite` — so a `.Head`-only read that drops the trailing rows and a composite-over-primary read that destroys row zero's plate geometry are both deleted forms; every IFC per-row COMPOSITION column the folds carry is a round-trip FIXED POINT, not a one-way read, its egress peer stated at `[03]-[EGRESS]` — the typed-Pset half is a declared ASYMMETRY (typed out, neutral bag back) and claiming a fixed point over it is the overclaim; an imported material Pset lands as a NEUTRAL seam `PropertyBag` under `EvidenceGrade.Import` and never as typed `MaterialPropertySet` columns, because that family is full-vector by construction and a partial foreign Pset folded onto a case fabricates every undeclared column — its values narrow through the ONE `PropertyLowering.Lower`, a second `IfcValue` narrowing on this page being the deleted fork, and its row keys mint through the owner-blessed `PropertyCategory.Seam.Row` EMPTY-prefix category so a round-tripped foreign name stays bare in the one key space every reader shares, a call-site `PropertyName.Create` being the fork the branch row-name custody ruling deletes; the imported-Pset drops RETURN on the `Fidelity` carrier and a mutable log field beside the returned bags is the deleted form that gave one fact two write orders, so `ImportedPsets` neither takes an accumulator parameter nor owns a ledger — the parent fold's `Traverse` `Combine`s it; every GeometryGym nullable name narrows through the ONE `PropertyLowering.Stated` door behind `MaterialShape.Key`/`Label` and a `?? ""` inside a fold body is the deleted form that spelled absence as a blank the seam then read as a declared identity; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `MaterialLayer` thickness coerces the NATIVE-unit `LayerThickness` through `UnitScheme.Coerce` and admits through the QTO-identity `MeasureValue.OfSi(QuantityType, Dimension, double)` — a bare double, a page-local factor member, the raw `MeasureValue` ctor that bypasses the owner's SI admission, a dimension-anonymous admit that strips the takeoff's QTO identity, OR treating the native length as already-SI (the mm-vs-metre import trap) are the named defects; the section geometry binds by content-hash reference and a RhinoCommon profile field or an in-process BRep evaluation is the named seam violation; an unresolvable material-select entity lifts `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Rejected` BARE (band 2600 owns the generated `Code`, the ingress on `ctx.Key` and the egress on the page `Egress` gate) and the seam `MaterialComposition` admission lifts `ElementFault.ValueRejected` BARE on a degenerate set, a `.ToError()` lowering hop (or a hand-built `Error.New(2600, …)`) bypassing the typed case being the named seam defect; the mechanical column crossings are ONE `[Mapper]` boundary transcription and a hand-rolled sentinel/logical/substance narrowing beside `MaterialShape` is the deleted form, while a crossing carrying a rail, a store call, or a memo stays hand-written by law — Mapperly transcribes shape, never a `Fin` lane.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using Riok.Mapperly.Abstractions;
using Rasm.Bim;
using Rasm.Bim.Model;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Semantics;

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class MaterialShape {
    [MapProperty(nameof(IfcMaterialConstituent.Name), nameof(MaterialConstituent.PartName))]
    public static partial MaterialConstituent Row(IfcMaterialConstituent constituent);

    [UserMapping]
    internal static MaterialId Substance(IfcMaterial? material) => Key(material?.Name);

    internal static MaterialId Key(string? name, string? fallback = null) =>
        MaterialId.Of(PropertyLowering.Stated(name).IfNone(() => PropertyLowering.Stated(fallback).IfNone("")));

    internal static Option<int> Junction(int priority) => priority == int.MinValue ? None : Some(priority);

    internal static Option<bool> Ventilation(IfcLogicalEnum value) => value switch {
        IfcLogicalEnum.TRUE => Some(true),
        IfcLogicalEnum.FALSE => Some(false),
        _ => None,
    };

    [UserMapping]
    internal static string Label(string? value) => PropertyLowering.Stated(value).IfNone("");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class MaterialProjection {
    public static Fin<Node.Material> Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, UnitScheme scale, Op key) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage u    => Optional(u.ForLayerSet).ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "material-usage-unbound", "layer-set" }))).Bind(set => LayerSetOf(set, tolerance, scale, key)),
            IfcMaterialProfileSetUsage u  => Optional(u.ForProfileSet).ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "material-usage-unbound", "profile-set" }))).Bind(set => ProfileSetOf(set, tolerance, profiles, scale, key)),
            IfcMaterialLayerSet set       => LayerSetOf(set, tolerance, scale, key),
            IfcMaterialProfileSet set     => ProfileSetOf(set, tolerance, profiles, scale, key),
            IfcMaterialConstituentSet set => ConstituentSetOf(set, tolerance, key),
            IfcMaterial material          => SingleOf(material, tolerance),
            _                             => Fin.Fail<Node.Material>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "material-select-unresolved", relatingMaterial.GetType().Name }))),
        };

    static Fin<Node.Material> LayerSetOf(IfcMaterialLayerSet set, double tolerance, UnitScheme scale, Op key) =>
        LayersOf(set, scale, key).Bind(layers => Mint(MaterialShape.Key(set.Name), tolerance, MaterialComposition.OfLayerSet(layers, key)));

    static Fin<Node.Material> ProfileSetOf(IfcMaterialProfileSet set, double tolerance, IIfcProfileStore profiles, UnitScheme scale, Op key) =>
        from rows in ProfilesOf(set, profiles, scale, key)
        from material in Mint(MaterialShape.Key(set.Name), tolerance, MaterialComposition.OfProfileSet(rows, key, CompositeOf(set, profiles, key)))
        select material;

    static Fin<Node.Material> ConstituentSetOf(IfcMaterialConstituentSet set, double tolerance, Op key) =>
        Mint(MaterialShape.Key(set.Name), tolerance, MaterialComposition.OfConstituentSet(
            set.MaterialConstituents.Values.AsIterable().Map(MaterialShape.Row).ToSeq(), key));

    static Fin<Node.Material> SingleOf(IfcMaterial material, double tolerance) {
        MaterialId id = MaterialShape.Substance(material);
        return Mint(id, tolerance, Fin.Succ<MaterialComposition>(MaterialComposition.OfSingle(id)));
    }

    static Fin<Seq<MaterialLayer>> LayersOf(IfcMaterialLayerSet set, UnitScheme scale, Op key) =>
        set.MaterialLayers.AsIterable()
            .ToSeq()
            .TraverseM(layer => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, scale.Coerce(layer.LayerThickness, QuantityType.Length, Dimension.LengthDim), key: key)
                .Map(thickness => new MaterialLayer(
                    MaterialShape.Substance(layer.Material), thickness, MaterialShape.Label(layer.Name),
                    MaterialShape.Junction(layer.Priority), MaterialShape.Label(layer.Category), MaterialShape.Ventilation(layer.IsVentilated))))
            .As();

    static Fin<Seq<MaterialProfile>> ProfilesOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, UnitScheme scale, Op key) =>
        set.MaterialProfiles.AsIterable()
            .ToSeq()
            .TraverseM(row => Optional(row.Profile)
                .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "material-profile-missing", set.Name, row.Name })))
                .Bind(profile => OffsetsOf(row, scale, key)
                    .Map(offsets => new MaterialProfile(
                        MaterialShape.Key(row.Material?.Name, set.Name),
                        profiles.Preserve(profile, key),
                        MaterialShape.Junction(row.Priority), MaterialShape.Label(row.Category), offsets))))
            .As();

    static Option<ProfileRef> CompositeOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, Op key) =>
        Optional(set.CompositeProfile).Map(composite => profiles.Preserve(composite, key));

    static Fin<Seq<MeasureValue>> OffsetsOf(IfcMaterialProfile row, UnitScheme scale, Op key) =>
        row is IfcMaterialProfileWithOffsets offsets
            ? toSeq(offsets.OffsetValues).TraverseM(value => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, scale.Coerce(value, QuantityType.Length, Dimension.LengthDim), key: key)).As()
            : Fin.Succ(Seq<MeasureValue>());

    static Fin<Node.Material> Mint(MaterialId key, double tolerance, Fin<MaterialComposition> composition) =>
        composition.Map(c => {
            var draft = new Node.Material(NodeId.Of(new NodeSeed.Placement()), key, c, Seq<MaterialPropertySet>());
            return (Node.Material)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, tolerance)));
        });

    public static WriterT<FidelityLog, Fin, Seq<PropertyBag>> ImportedPsets(
        IfcMaterialDefinition definition, Map<string, NodeId> rooted, UnitScheme scale, TemplateScope templates, Op key) =>
        definition.HasProperties.AsIterable().ToSeq()
            .Traverse(pset => Bag(pset, rooted, scale, templates, key))
            .As();

    static WriterT<FidelityLog, Fin, PropertyBag> Bag(
        IfcMaterialProperties pset, Map<string, NodeId> rooted, UnitScheme scale, TemplateScope templates, Op key) {
        string name = MaterialShape.Label(pset.Name);
        return pset.Properties.Values.AsIterable().ToSeq()
            .Traverse(property => PropertyLowering.Lower(property, rooted, scale, key)
                .Map(value => (Name: PropertyCategory.Seam.Row(MaterialShape.Label(property.Name)), Value: value)))
            .As()
            .Map(rows => new PropertyBag(
                name,
                rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)),
                PropertyInheritance.ModeOf(name, TypeBinding.Occurrence, templates),
                EvidenceGrade.Import));
    }
}
```

## [03]-[EGRESS]

- Owner: the `MaterialProjection` egress half the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authoring the type-level `MaterialComposition` onto the GeometryGym material-definition family and lowering the seam `MaterialPropertySet` set onto its `IfcMaterialProperties` Pset through the `MaterialColumn` table beside the `EvidenceRows` provenance tail, `AuthorUsage` wrapping that shared definition in the per-occurrence usage entity the `Associate` edge carries [OCCURRENCE_USAGE_RULING], and `EmitMemo<TKey,TValue>` the db-scoped emit memo this cluster DECLARES and the `Semantics/classification#CLASSIFICATION_AXIS` dictionary-source egress composes — ONE owner for the `ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<…>>` shape, so a second hand-declared table anywhere in the package is the deleted duplicate.
- Entry: `MaterialProjection.AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScheme scale)` authors the type-level `MaterialComposition` ONCE (`Single`→`IfcMaterial`, `LayerSet`→`IfcMaterialLayerSet`, `ProfileSet`→`IfcMaterialProfileSet`, `ConstituentSet`→`IfcMaterialConstituentSet`), rails the seam `MaterialPropertySet` set through `AuthorPropertySet` onto the `IfcMaterialProperties` named Psets as TYPED columns, and reconstitutes a `ProfileSet`'s `IfcProfileDef` from the injected `profiles` store — a store miss authoring the entity the carried `profileSubtype` token names (the `DetailSchema.Realization` `ProfileSubtype` row the `Emit` resolves off the graph: `IfcRectangleProfileDef` completes whole from the baked `SectionProperties` dims, a voided token's mandatory inner curves stay store-preserved-only) — with `Fin<T>` aborting `BimFault.Refused` with `BimReason.DanglingReference` keyed on the page `Egress` gate on an unresolvable profile and `BimFault.Refused` with `BimReason.Codec` on a discipline case the column table holds no row for; `MaterialProjection.AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage, UnitScheme scale)` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge carries, returning the bare definition for `MaterialUsage.Unbound`.
- Auto: every SI seam magnitude re-declares through `UnitScheme.Render` against the TARGET model's regime — the exact inverse of the ingress `Coerce`, so a metre-declared seam re-emits mm on a mm-declared target with no page-local factor; every seam row re-authors its own IFC row and every per-row column re-stamps, so the COMPOSITION columns — layer, constituent, profile row, composite outline, occurrence usage — are a FIXED POINT of the round-trip rather than a one-way read; the typed `MaterialPropertySet` half is DECLARED asymmetric and its fixed point is not claimed: this egress authors typed columns and the ingress lands an imported Pset as a NEUTRAL `PropertyBag` by law (the Element ruling bars a partial foreign set from folding onto a full-vector typed case), so a re-ingest of an authored `Pset_MaterialThermal` returns as an Import-ranked neutral bag on the SAME material node and never as the `Thermal` case that wrote it — the inverse fold is structurally barred, not missing; `AuthorPropertySet` folds the ONE `MaterialColumn` table — each row a `(set name, column name, typed reader)` triple whose reader type-tests its own discipline case and answers `Option<IfcValue>`, so a present column emits and an absent optional carrier (a Damping Rayleigh pair, a Hygrothermal capillary A-value) answers `None` where the retired arms branched per presence — and the `Environmental` module vector is minted from the seam `LifecycleStage` roster so a new EN 15978 module is a seam row and never a hand-added column; the shared base-`Evidence` provenance columns fold the same presence-answering `EvidenceRows` table once at the fold tail rather than per arm, the always-declared `Source`/`Grade` and the Option-carried `Reference`/`ValidUntil` reading as one row family; a discipline case the table holds no row for rails `BimFault.Refused` with `BimReason.Codec` because an empty authored Pset silently drops a whole discipline set; every authored material resolves through the db-scoped `MaterialOf` memo so ONE `IfcMaterial` per `(db, name)` serves every layer, constituent, profile, and node.
- Receipt: the authored `IfcMaterialDefinition` + its `IfcMaterialProperties` Psets + the per-occurrence usage entity are the IFC material subgraph the `Emit` writes, the type-level composition authored ONCE and the per-occurrence usage wrapping it so a wall and its mirror share one `IfcMaterialLayerSet` with two `IfcMaterialLayerSetUsage` instances; a re-ingest of that file re-keys the SAME seam `Node.Material` off the composition alone, which is what makes the COMPOSITION column set a fixed point rather than a claim — its typed property sets return as neutral Import bags bound to that same node, the declared asymmetry the `[02]` boundary states.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, NodaTime, LanguageExt.Core
- Growth: a new emitted material property is one `MaterialColumn` row on its discipline table naming its own `IfcValue` leaf (a new EN 15978 module is one seam `LifecycleStage` row the `Stages` mint iterates), never a per-property egress branch, never a re-widened double-only Pset, and never a twelfth bespoke `Pset(...)`+evidence scaffold; a new provenance axis on the seam `PropertyEvidence` is one `EvidenceRows` row answering its own presence; a new discipline case is one `Discipline<TCase>` block on the same table; a new db-scoped emit entity is one `EmitMemo<TKey,TValue>` field, never a second `ConditionalWeakTable` declaration.
- Boundary: the EGRESS reads the seam `Material` node + the `Associate` edge `MaterialUsage` ONLY — a Materials `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into this owner is the deleted form (those Materials wires are retired, the material egress reading the projected seam subgraph); an unset `Priority` re-authors unwritten (assigning GeometryGym's `int.MinValue` back is the sentinel re-introduction this projection deletes), an `UNKNOWN` ventilation re-authors `IfcLogicalEnum.UNKNOWN`, an absent `CardinalPoint` re-authors `IfcCardinalPointReference.DEFAULT` (the unset member both writers omit — electing `MID` promotes every unset usage into a declared mid-point on re-export), an absent `OffsetFromReferenceLine` RAILS `BimFault.Refused` with `BimReason.Codec` because that attribute is MANDATORY and its writer emits unconditionally (a `double.NaN` into a required real writes a malformed STEP token), and a row with offsets re-authors the `IfcMaterialProfileWithOffsets` subtype at its declared one-or-two arity, never padded; the carried-token authored profile is the SINGLE-row fallback alone, because that token names the whole member and applying it per row of a compound authors one member rectangle N times — a compound row missing its preserved fragment rails `BimFault.Refused` with `BimReason.DanglingReference`; the `IfcMaterialProperties` Pset attaches to the authored `IfcMaterialDefinition` and the `ProfileSet` `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP with a composite def re-stamping `CompositeProfile` on the authored set (a parametric dimension re-folded onto the seam being the deleted form); a store-missed Rasm-authored `ProfileSet` resolves its profile entity from the carried `DetailSchema.Realization` `ProfileSubtype` row and the baked `SectionProperties` dims — never a Materials call, and never a bare voided subtype with unassigned mandatory inner curves; every emitted column — the table's discipline rows AND the shared evidence tail alike — carries the GeometryGym `IfcValue` leaf its datum names into the `IfcPropertySingleValue(DatabaseIfc, string, IfcValue)` ctor, so a `Num`/`Text`/`Flag` rename layer over that surface, a primitive `(…, string)`/`(…, double)` overload that picks the leaf on the column's behalf, and a lossy double flattening of the `FireRating` class or the `Cost` `Currency`/`MeasurementBasis` label are all deleted forms; provenance is single-stored on the seam `PropertyEvidence` (the retired per-case `Environmental` EPD/`ValidUntilYear` double-store and its suppression flag are GONE) and reads through that owner's OWN shapes — `Reference` is an `Option<string>` whose absent column does not emit (a blank `IfcLabel` re-authors a citation the source never declared), `Grade` the `EvidenceGrade.Token` rank so a re-ingest of an authored Pset reads its declared provenance tier rather than inferring one, and the expiry lowers through the ISO-8601 `LocalDatePattern.Iso` so the full date round-trips intact; the egress `AuthorPropertySet` is RAILED and a `void` Pset author sequenced through `Map` is the deleted form that made an uncolumned discipline case indistinguishable from a written one; the `EmitMemo` owner is keyed by the emit `DatabaseIfc` so the cache is emit-scoped and GC-collected with the database, and a durable or process-static material cache is the deleted form.

```csharp signature
// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EmitMemo<TKey, TValue>
    where TKey : notnull
    where TValue : class {
    readonly ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<TKey, TValue>> tables = new();

    public TValue Of(DatabaseIfc db, TKey key, Func<TKey, TValue> mint) =>
        tables.GetValue(db, static _ => new ConcurrentDictionary<TKey, TValue>()).GetOrAdd(key, mint);
}

// --- [MODELS] --------------------------------------------------------------------------
readonly record struct MaterialColumn(string Set, string Name, Func<MaterialPropertySet, Option<IfcValue>> Read);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class MaterialProjection {
    static readonly Op Egress = Op.Of(name: nameof(MaterialProjection));

    static readonly EmitMemo<string, IfcMaterial> Materials = new();

    static IfcMaterial MaterialOf(DatabaseIfc db, string name) => Materials.Of(db, name, n => new IfcMaterial(db, n));

    public static Fin<IfcMaterialDefinition> AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScheme scale) =>
        Definition(db, material.Composition, material.MaterialKey, profiles, profileSubtype, scale)
            .Bind(definition => material.Properties.TraverseM(set => AuthorPropertySet(db, definition, set)).As().Map(_ => definition));

    static Fin<IfcMaterialDefinition> Definition(DatabaseIfc db, MaterialComposition composition, MaterialId key, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScheme scale) =>
        composition.Switch(
            single:        s => Fin.Succ<IfcMaterialDefinition>(MaterialOf(db, s.Material.Value)),
            layerSet:      s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialLayerSet(
                                    s.Layers.Map(l => Layer(db, l, scale)), key.Value)),
            profileSet:    s => Rows(db, s, profiles, profileSubtype, scale).Map(rows => AuthorProfileSet(key, rows, s, profiles)),
            constituentSet: s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialConstituentSet(key.Value,
                                    s.Constituents.Map(c => new IfcMaterialConstituent(
                                        string.IsNullOrEmpty(c.PartName) ? c.Material.Value : c.PartName,
                                        MaterialOf(db, c.Material.Value)) { Fraction = c.Fraction, Category = c.Category }))));

    static IfcMaterialDefinition AuthorProfileSet(MaterialId key, Seq<IfcMaterialProfile> rows, MaterialComposition.ProfileSet set, IIfcProfileStore profiles) {
        var authored = new IfcMaterialProfileSet(key.Value, [.. rows]);
        set.Composite.Bind(profiles.Find).Bind(static profile => Optional(profile as IfcCompositeProfileDef))
            .IfSome(composite => authored.CompositeProfile = composite);
        return authored;
    }

    static IfcMaterialLayer Layer(DatabaseIfc db, MaterialLayer layer, UnitScheme scale) {
        var row = new IfcMaterialLayer(MaterialOf(db, layer.Material.Value), scale.Render(layer.Thickness).Value, layer.LayerName) {
            Category = layer.Category,
            IsVentilated = layer.Ventilated.Match(Some: static v => v ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN),
        };
        layer.Priority.IfSome(priority => row.Priority = priority);
        return row;
    }

    static Fin<Seq<IfcMaterialProfile>> Rows(DatabaseIfc db, MaterialComposition.ProfileSet set, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScheme scale) =>
        set.Profiles.TraverseM(row => profiles.Find(row.Profile)
                .Match(Some: Some, None: () => set.Profiles.Count == 1 ? AuthoredProfile(db, set, profileSubtype, scale) : None)
                .ToFin(new BimFault.Refused(Egress, BimScope.Semantics, BimReason.DanglingReference, string.Join(':', new object?[] { "material-profile-step-unresolved", row.Profile.Designation })))
                .Map(profile => Row(db, row, profile, scale)))
            .As();

    static IfcMaterialProfile Row(DatabaseIfc db, MaterialProfile row, IfcProfileDef profile, UnitScheme scale) {
        IfcMaterial material = MaterialOf(db, row.Material.Value);
        string name = row.Profile.Designation;
        IfcMaterialProfile authored = row.Offsets.Map(offset => scale.Render(offset).Value).ToArray() switch {
            [var start] => new IfcMaterialProfileWithOffsets(name, material, profile, start),
            [var start, var end] => new IfcMaterialProfileWithOffsets(name, material, profile, start, end),
            _ => new IfcMaterialProfile(name, material, profile),
        };
        authored.Category = row.Category;
        row.Priority.IfSome(priority => authored.Priority = priority);
        return authored;
    }

    static Option<IfcProfileDef> AuthoredProfile(DatabaseIfc db, MaterialComposition.ProfileSet s, Option<string> subtype, UnitScheme scale) =>
        subtype.Filter(static name => name == nameof(IfcRectangleProfileDef))
            .Bind(_ => s.Section.Map(section => (IfcProfileDef)new IfcRectangleProfileDef(
                db, s.Profile.Designation, scale.Render(section.Width).Value, scale.Render(section.Depth).Value)));

    public static Fin<IfcMaterialSelect> AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage, UnitScheme scale) => usage.Switch(
        unbound:    _ => Fin.Succ((IfcMaterialSelect)definition),
        layerSet:   u => definition is IfcMaterialLayerSet set
            ? u.OffsetFromReferenceLine
                .ToFin(new BimFault.Refused(Egress, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "layer-usage-offset-absent", set.Name })))
                .Map(offset => (IfcMaterialSelect)new IfcMaterialLayerSetUsage(set,
                    u.Direction switch { LayerSetDirection.Axis1 => IfcLayerSetDirectionEnum.AXIS1, LayerSetDirection.Axis2 => IfcLayerSetDirectionEnum.AXIS2, _ => IfcLayerSetDirectionEnum.AXIS3 },
                    u.Sense == DirectionSense.Positive ? IfcDirectionSenseEnum.POSITIVE : IfcDirectionSenseEnum.NEGATIVE,
                    scale.Render(offset).Value))
            : Fin.Fail<IfcMaterialSelect>(new BimFault.Refused(Egress, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "material-usage-on", "layer", definition.GetType().Name }))),
        profileSet: u => definition is IfcMaterialProfileSet set
            ? Fin.Succ<IfcMaterialSelect>(new IfcMaterialProfileSetUsage(set, u.CardinalPoint.Match(
                Some: static point => (IfcCardinalPointReference)point.Key,
                None: static () => IfcCardinalPointReference.DEFAULT)) {
                    ReferenceExtent = u.ReferenceExtent.Map(value => scale.Render(value).Value).IfNone(double.NaN),
                })
            : Fin.Fail<IfcMaterialSelect>(new BimFault.Refused(Egress, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "material-usage-on", "profile", definition.GetType().Name }))));

    // --- [COLUMN_TABLE] ----------------------------------------------------------------
    static readonly Seq<MaterialColumn> Columns =
        Discipline<MaterialPropertySet.Mechanical>("Pset_MaterialMechanical",
            ("MassDensity", static m => Some<IfcValue>(new IfcMassDensityMeasure(m.Density.Si))),
            ("YoungModulus", static m => Some<IfcValue>(new IfcModulusOfElasticityMeasure(m.YoungsModulus.Si))),
            ("ShearModulus", static m => Some<IfcValue>(new IfcModulusOfElasticityMeasure(m.ShearModulus.Si))),
            ("YieldStress", static m => Some<IfcValue>(new IfcPressureMeasure(m.YieldStrength.Si))),
            ("UltimateStress", static m => Some<IfcValue>(new IfcPressureMeasure(m.UltimateStrength.Si))),
            ("PoissonRatio", static m => Some<IfcValue>(new IfcPositiveRatioMeasure(m.PoissonsRatio))),
            ("ThermalExpansionCoefficient", static m => Some<IfcValue>(new IfcThermalExpansionCoefficientMeasure(m.ThermalExpansionPerK))))
        + Discipline<MaterialPropertySet.Orthotropic>("Rasm_MaterialOrthotropic",
            ("MassDensity", static o => Some<IfcValue>(new IfcMassDensityMeasure(o.Density.Si))),
            ("E1Parallel", static o => Some<IfcValue>(new IfcModulusOfElasticityMeasure(o.E1Parallel.Si))),
            ("E2Perpendicular", static o => Some<IfcValue>(new IfcModulusOfElasticityMeasure(o.E2Perpendicular.Si))),
            ("ShearModulus", static o => Some<IfcValue>(new IfcModulusOfElasticityMeasure(o.ShearModulus.Si))),
            ("Strength1Parallel", static o => Some<IfcValue>(new IfcPressureMeasure(o.Strength1Parallel.Si))),
            ("Strength2Perpendicular", static o => Some<IfcValue>(new IfcPressureMeasure(o.Strength2Perpendicular.Si))),
            ("ThermalExpansionCoefficient", static o => Some<IfcValue>(new IfcThermalExpansionCoefficientMeasure(o.ThermalExpansionPerK))))
        + Discipline<MaterialPropertySet.Thermal>("Pset_MaterialThermal",
            ("ThermalConductivity", static t => Some<IfcValue>(new IfcThermalConductivityMeasure(t.Conductivity.Si))),
            ("SpecificHeatCapacity", static t => Some<IfcValue>(new IfcSpecificHeatCapacityMeasure(t.SpecificHeat.Si))),
            ("ThermalTransmittance", static t => Some<IfcValue>(new IfcThermalTransmittanceMeasure(t.UValue.Si))),
            ("VapourDiffusionResistance", static t => Some<IfcValue>(new IfcPositiveRatioMeasure(t.VapourResistanceFactor))))
        + Discipline<MaterialPropertySet.Acoustic>("Pset_MaterialAcoustic",
            ("NoiseReductionCoefficient", static a => Some<IfcValue>(new IfcNormalisedRatioMeasure(a.Nrc))),
            ("SoundAbsorptionAverage", static a => Some<IfcValue>(new IfcNormalisedRatioMeasure(a.Saa))),
            ("SoundTransmissionClass", static a => Some<IfcValue>(new IfcReal(a.StcWeighted))))
        + Discipline<MaterialPropertySet.Fire>("Pset_MaterialFire",
            ("ReactionToFireClass", static f => Some<IfcValue>(new IfcLabel(f.Reaction.Key))),
            ("Combustible", static f => Some<IfcValue>(new IfcBoolean(f.Reaction.Combustible))),
            ("SmokeProduction", static f => Some<IfcValue>(new IfcLabel(f.Smoke.Key))),
            ("FlamingDroplets", static f => Some<IfcValue>(new IfcLabel(f.Droplets.Key))),
            ("FireResistanceR", static f => Some<IfcValue>(new IfcReal(f.Resistance.LoadBearingMinutes))),
            ("FireResistanceE", static f => Some<IfcValue>(new IfcReal(f.Resistance.IntegrityMinutes))),
            ("FireResistanceI", static f => Some<IfcValue>(new IfcReal(f.Resistance.InsulationMinutes))))
        + Stages("Pset_EnvironmentalImpactValues")
        + Discipline<MaterialPropertySet.Environmental>("Pset_EnvironmentalImpactValues",
            ("RecycledContent", static e => Some<IfcValue>(new IfcNormalisedRatioMeasure(e.RecycledContent))),
            ("EndOfLifeRecovery", static e => Some<IfcValue>(new IfcNormalisedRatioMeasure(e.EndOfLifeRecovery))))
        + Discipline<MaterialPropertySet.Cost>("Pset_ConstructionCosts",
            ("Currency", static c => Some<IfcValue>(new IfcLabel(c.Currency.Value))),
            ("MeasurementBasis", static c => Some<IfcValue>(new IfcLabel(c.Basis.Key))),
            ("SupplyCost", static c => Some<IfcValue>(new IfcMonetaryMeasure(c.SupplyPerUnit))),
            ("InstallationCost", static c => Some<IfcValue>(new IfcMonetaryMeasure(c.InstallPerUnit))),
            ("LifeCycleCost", static c => Some<IfcValue>(new IfcMonetaryMeasure(c.LifecyclePerUnit))))
        + Discipline<MaterialPropertySet.Damping>("Rasm_MaterialDamping",
            ("DampingRatio", static d => Some<IfcValue>(new IfcPositiveRatioMeasure(d.DampingRatio))),
            ("StructuralLossFactor", static d => Some<IfcValue>(new IfcPositiveRatioMeasure(d.StructuralLossFactor))),
            ("RayleighAlpha", static d => d.Rayleigh.Map(static r => (IfcValue)new IfcReal(r.AlphaPerS))),
            ("RayleighBeta", static d => d.Rayleigh.Map(static r => (IfcValue)new IfcTimeMeasure(r.BetaS))))
        + Discipline<MaterialPropertySet.Hygrothermal>("Rasm_MaterialHygrothermal",
            ("Porosity", static h => Some<IfcValue>(new IfcNormalisedRatioMeasure(h.Porosity))),
            ("WaterContent80RH", static h => Some<IfcValue>(new IfcMassDensityMeasure(h.WaterContent80Rh.Si))),
            ("FreeWaterSaturation", static h => Some<IfcValue>(new IfcMassDensityMeasure(h.FreeWaterSaturation.Si))),
            ("WaterAbsorptionCoefficient", static h => h.WaterAbsorptionKgPerM2SqrtS.Map(static a => (IfcValue)new IfcReal(a))))
        + Discipline<MaterialPropertySet.Durability>("Rasm_MaterialDurability",
            ("CarbonationRate", static u => Some<IfcValue>(new IfcReal(u.CarbonationRateMmPerSqrtYear))),
            ("ChlorideMigrationCoefficient", static u => Some<IfcValue>(new IfcReal(u.ChlorideDiffusion.Si))),
            ("AgeingExponent", static u => Some<IfcValue>(new IfcReal(u.AgeingExponent))))
        + Discipline<MaterialPropertySet.Optical>("Pset_MaterialOptical",
            ("VisibleTransmittance", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.VisibleTransmittance))),
            ("VisibleReflectanceFront", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.VisibleReflectanceFront))),
            ("VisibleReflectanceBack", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.VisibleReflectanceBack))),
            ("SolarTransmittance", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.SolarTransmittance))),
            ("SolarReflectanceFront", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.SolarReflectanceFront))),
            ("SolarReflectanceBack", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.SolarReflectanceBack))),
            ("ThermalIrTransmittance", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.ThermalIrTransmittance))),
            ("ThermalIrEmissivityFront", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.ThermalIrEmissivityFront))),
            ("ThermalIrEmissivityBack", static o => Some<IfcValue>(new IfcNormalisedRatioMeasure(o.ThermalIrEmissivityBack))));

    static Seq<MaterialColumn> Discipline<TCase>(string set, params (string Name, Func<TCase, Option<IfcValue>> Read)[] columns)
        where TCase : MaterialPropertySet =>
        toSeq(columns).Map(column => new MaterialColumn(set, column.Name,
            value => value is TCase typed ? column.Read(typed) : None));

    static Seq<MaterialColumn> Stages(string set) =>
        LifecycleStage.Items.AsIterable().ToSeq().Map(stage => new MaterialColumn(set, $"GlobalWarmingPotential_{stage.Module}",
            value => value is MaterialPropertySet.Environmental e ? Some<IfcValue>(new IfcReal(e.StageAt(stage))) : None));

    static readonly Seq<(string Name, Func<PropertyEvidence, Option<IfcValue>> Read)> EvidenceRows = Seq<(string, Func<PropertyEvidence, Option<IfcValue>>)>(
        ("DataSource", static e => Some<IfcValue>(new IfcLabel(e.Source))),
        ("DataGrade", static e => Some<IfcValue>(new IfcLabel(e.Grade.Token))),
        ("DataReference", static e => e.Reference.Map(static r => (IfcValue)new IfcLabel(r))),
        ("DataValidUntil", static e => e.ValidUntil.Map(static d => (IfcValue)new IfcLabel(LocalDatePattern.Iso.Format(d)))));

    static Fin<Unit> AuthorPropertySet(DatabaseIfc db, IfcMaterialDefinition material, MaterialPropertySet set) {
        Seq<(string Set, IfcProperty Column)> typed = Columns.Choose(column =>
            column.Read(set).Map(value => (column.Set, Column: (IfcProperty)new IfcPropertySingleValue(db, column.Name, value))));
        return typed.Head
            .ToFin(new BimFault.Refused(Egress, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "material-pset-uncolumned", set.GetType().Name })))
            .Map(head => ignore(Pset(material, head.Set, typed.Map(static row => row.Column) + EvidenceColumns(db, set.Evidence))));
    }

    static Seq<IfcProperty> EvidenceColumns(DatabaseIfc db, PropertyEvidence evidence) =>
        EvidenceRows.Choose(row => row.Read(evidence).Map(value => (IfcProperty)new IfcPropertySingleValue(db, row.Name, value)));

    static IfcMaterialProperties Pset(IfcMaterialDefinition material, string name, Seq<IfcProperty> columns) {
        var pset = new IfcMaterialProperties(name, material);
        columns.Iter(p => pset.Properties[p.Name] = p);
        return pset;
    }
}
```

## [04]-[RESEARCH]

(none)
