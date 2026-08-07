# [BIM_MATERIAL_COMPOSITION]

The IFC material PROJECTOR lowering the live GeometryGym `IfcMaterialSelect` surface onto the `Rasm.Element` seam `Material` node: `MaterialProjection.Project` discriminates the relating-material runtime entity — `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` — and folds it into one content-keyed seam `Node.Material` carrying the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`). The seam OWNS the construction-material algebra (`MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `ProfileRef`, `MaterialPropertySet`, `MaterialId`); this page owns ONLY the GeometryGym discrimination that fills it, never re-declaring a Bim `BimMaterial`/`BimMaterialComposition` — the retired `BimMaterial` record and the `BimElement.Materials` column are GONE, a material is a seam `Material` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, and the consumer reads `element.Materials` flat on the baked element rather than a second stored record keyed by `MaterialId`. The occurrence usage binding (layer direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the `Associate` edge `MaterialUsage` payload the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors [OCCURRENCE_USAGE_RULING], this owner producing only the type-level SET structure so a wall and its mirror share one `LayerSet` with two `Associate` usages. A linear member's section is a neutral `ProfileRef` (`Standard` + `Designation` + content key) the `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog [M7], its full `IfcProfileDef` parametric definition preserved in the content-addressed store the `ContentKey` keys — a compound set's declared `IfcCompositeProfileDef` `CompositeProfile` keyed over the primary row so a built-up section's combined geometry survives the store and re-stamps at egress — the page references NO VividOrange section type and folds NO parametric dimension onto the seam, because the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the profile geometry by content-hash reference, never a RhinoCommon type. An unresolvable material-select entity rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` lifted BARE (band 2600 IS the `Expected` `Code`; no `.ToError()` hop); a degenerate composition (empty set, non-positive layer thickness, unnormalized constituent fractions) rails the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` the seam `MaterialComposition` admission owns.

Every native-unit magnitude crossing this projector rides the ONE `Projection/semantic#SEMANTIC_PROJECTOR` `UnitScale` pair — `Coerce` on ingress, `Declare` on egress — threaded from the projection's single regime, so no member on this page returns a bare multiplier and the mm-vs-metre import trap a Revit export springs closes at one entry [`Projection/semantic#SEMANTIC_PROJECTOR`]. The projector is BIDIRECTIONAL: the `[03]-[EGRESS]` half is the inverse the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authors the type-level `MaterialComposition` back onto the GeometryGym material-definition family (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial`) ONCE per material and lowers the seam `MaterialPropertySet` set onto TYPED `IfcMaterialProperties` `Pset_Material*` rows through one column table (every case field round-trips — the `FireRating` class, the `Cost` `Currency`/`MeasurementBasis`, and the EPD id + expiry as the shared base-`Evidence` label columns, each row carrying the GeometryGym `IfcValue` leaf its own datum names, never coerced to a lossy double, and the `Environmental` rows spread the FULL EN 15978 per-`LifecycleStage` GWP vector one column per module, never a single aggregate sum, the newer `Optical`/`Damping`/`Hygrothermal`/`Durability` carriers each their own Pset), and `AuthorUsage` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge `MaterialUsage` carries [OCCURRENCE_USAGE_RULING]. This is the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — `Rasm.Bim` reads the projected `Material` subgraph directly, never a Materials wire. A `ProfileSet`'s full parametric `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the seam holds only the neutral `ProfileRef` + baked `SectionProperties`); a store-missed Rasm-authored profile authors as the entity the carried `DetailSchema.Realization` `ProfileSubtype` row names off the baked dims where the token's geometry completes, an unresolvable profile railing `BimFault.DanglingReference`.

## [01]-[INDEX]

- [02]-[MATERIAL_COMPOSITION]: `MaterialProjection.Project` the `IfcMaterialSelect`→seam `Node.Material` ingress fold, the per-modality `LayerSet`/`ProfileSet`/`ConstituentSet`/`Single` mapping onto the seam `MaterialComposition`, the per-row `ProfileRef` content-keyer beside the `CompositeOf` set-level outline, the `LayersOf`/`ProfilesOf` row folds with their `MaterialShape` column carriers and `OffsetsOf` projections, the content-keyed `Mint` of the seam `Node.Material`, and the `ImportedPsets` reader lowering `HasProperties` material Psets to neutral seam `PropertyBag`s beside their `Noted` narrowing log.
- [03]-[EGRESS]: `MaterialProjection.AuthorComposition`/`AuthorUsage` the inverse half re-authoring a seam `Material` node onto the GeometryGym material-definition family + the `MaterialPropertySet`→`IfcMaterialProperties` `Pset_Material*` column table + the `Associate`-edge `MaterialUsage`→`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [OCCURRENCE_USAGE_RULING], over the `EmitMemo<TKey,TValue>` db-scoped entity memo this cluster declares and the classification egress composes.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialProjection` the static BIDIRECTIONAL GeometryGym↔seam material projector, this cluster owning its `Project` ingress — folding one `IfcMaterialSelect` runtime entity into one seam `Node.Material` (discriminating the entity, building the seam `MaterialComposition` through the seam smart-constructors, minting the content-keyed node id) — beside `MaterialShape` the `[Mapper]` boundary transcription owning the mechanical column crossings BOTH halves compose (the `IfcMaterial`→`MaterialId` substance carrier, the `int.MinValue`→`Option<int>` junction sentinel, the `IfcLogicalEnum`→`Option<bool>` ventilation narrowing, and the whole `IfcMaterialConstituent`→`MaterialConstituent` row). The seam owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` rows, the `ProfileRef`, the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage`, the `ValueBag<V>` bag an imported Pset lands in, and the `MaterialPropertySet` engineering-property family; this page declares NONE of them — it composes the seam vocabulary, mapping the GeometryGym material-assembly entities onto it and back.
- Entry: `MaterialProjection.Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, UnitScale scale, Op key)` is the live-entity promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes when folding an `IfcRelAssociatesMaterial.RelatingMaterial` (the parameter IS the typed `IfcMaterialSelect` the property carries — a `BaseClassIfc` admission is the deleted weak form) — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` and `IfcMaterialProfileSetUsage` its `ForProfileSet`, the usage payload riding the `Associate` edge not this node; `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` its WHOLE `MaterialProfiles` list onto the seam per-row `MaterialProfile` spread beside the declared `CompositeProfile`, `IfcMaterialConstituentSet` its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and returns one content-keyed seam `Node.Material`; `MaterialProjection.ImportedPsets(definition, rooted, scale, templates, key)` is the peer ingress reading the imported `HasProperties` material Psets as neutral seam `PropertyBag`s beside the `Noted` narrowing facts the lowering incurred, the `Projection/semantic#SEMANTIC_PROJECTOR` `Materials` fold content-minting each as a `Node.PropertySet` node the `Projection/relations#RELATION_ALGEBRA` `MaterialEdges` fold binds by one `Assign.PropertyDefinition` edge; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) and the seam `MaterialComposition` admission aborts a degenerate set (`ElementFault.ValueRejected`), each lifting BARE (the band IS the `Expected` `Code`; no `.ToError()` hop).
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and builds the seam `MaterialComposition` through the seam `Of`-prefixed smart-constructors (the `Fin`-railing `MaterialComposition.OfLayerSet`/`OfConstituentSet` owning the empty-set / non-positive-thickness / unnormalized-fraction admission, the total `OfSingle`/`OfProfileSet` lifted into `Fin` for the `Mint` fold), then mints the seam `Node.Material` whose id is the kernel seed-zero `XxHash128` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical materials dedup to one node; `LayersOf` folds each `IfcMaterialLayer` onto a seam `MaterialLayer` carrying its `MaterialId`, a `MeasureValue` thickness whose native `LayerThickness` coerces to SI metres through `UnitScale.Coerce` over `MeasureRow.Length` and admits through the QTO-identity `MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, …)` (a layer build-up thickness IS a length takeoff the `Semantics/properties#BASE_QUANTITIES` fold reads, so a dimension-anonymous admit strips the QTO read off every derived-wins row), and its layer name; `MaterialShape.Row` transcribes each `IfcMaterialConstituent` (read through the `Dictionary.Values`) onto a seam `MaterialConstituent` carrying its `MaterialId`, category, `Fraction`, and part name; `ProfileRefOf` projects the KEYED section — the set's declared `IfcCompositeProfileDef` `CompositeProfile` when a compound set carries one (the combined built-up geometry, decompile-confirmed settable), else the primary row's `Profile` — onto a neutral `ProfileRef` whose `ContentKey` is the kernel seed-zero `XxHash128` `ContentHash.Of` over the tag-namespaced `IfcProfileDef` STEP (the full parametric section preserved in the content-addressed store; the ONE kernel hasher the `Model/elements#REPRESENTATION_KEYS` keyer also composes, never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]), the `Designation` the row profile's name, the `Standard` left to the one-hop catalog resolution; `ProfilesOf` folds EVERY `IfcMaterialProfile` row onto a seam `MaterialProfile` — its own material, its own content-keyed `ProfileRef`, its `[0,100]` junction `Priority` through `MaterialShape.Junction`, its function `Category`, and its `IfcMaterialProfileWithOffsets.OffsetValues` reference-axis offsets SI-coerced through the same `UnitScale` entry — and `CompositeOf` preserves the set's declared `IfcCompositeProfileDef` as the seam `Composite`, so a built-up compound keeps every plate where the primary-only read kept one; `MaterialShape.Junction` is the `[SENTINEL_PROJECTION]` site retiring GeometryGym's `int.MinValue` unset priority to `None`, and `MaterialShape.Ventilation` narrows the three-state `IfcLogicalEnum` onto the seam `Option<bool>` so an `UNKNOWN` ventilation never reads as `FALSE` (the EN ISO 6946 falsification); the row `Description` is the one IFC annotation column the seam declines, carrying no analytical read where `Category` drives the assembly fold. Typed engineering property sets stay the AUTHORED lane's (the `Rasm.Materials` `ComponentProjector` lowers its catalog-backed `MaterialPropertySet` rows), so the IFC-ingest `Node.Material` carries an empty `Seq<MaterialPropertySet>` and `ImportedPsets` lands the imported `IfcMaterialDefinition.HasProperties` sets as NEUTRAL seam `PropertyBag`s instead — foreign set name, `PropertySource.Import` rank, `PropertyInheritance.ModeOf` precedence, values narrowed through the one `PropertyLowering.Lower` — because a partial imported Pset folded onto a full-vector typed case fabricates every undeclared column.
- Receipt: the seam `Node.Material` is the material evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Associate` edge into `element.Materials` (a `BakedMaterial` carrying the node plus its occurrence `MaterialUsage` — the seam Bake-folded accessor, DISTINCT from the `Rasm.Materials` projection-input `MaterialBinding` and the type→occurrence `TypeBinding`), the `Model/query#ELEMENT_SET` material predicate matches by `MaterialId` or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#BASE_QUANTITIES` layered-volume takeoff reads from the `LayerSet` thicknesses; the layer build-up, the section material, and the constituent mix each carry their real composition on one seam node, never a parallel layer/profile/constituent record family.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, Riok.Mapperly, LanguageExt.Core
- Growth: a new material-assembly modality is one seam `MaterialComposition` union arm (the seam's, not this page's) plus one `Project` switch arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the seam `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` filled by its owning `LayersOf`/`MaterialShape.Row`/`ProfilesOf` fold and re-stamped by its `[03]-[EGRESS]` peer; a new section catalog is one `ProfileRef.Standard` token the `Rasm.Materials` projector resolves, never a seam edit; a new mechanical column crossing is one `[UserMapping]` carrier on `MaterialShape`, never a second hand-rolled narrowing beside it; never a per-element-class material type, never a Bim `BimMaterial` record beside the seam node, and never a parallel material store.
- Boundary: the material model is the seam `Node.Material` + `MaterialComposition` and a Bim `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile`/`LayerSetUsage`/`ProfileSetUsage`/`ProfileDefKind`/`ProfileDims` re-declaration is the deleted form — the seam owns the algebra, this page owns only the GeometryGym discrimination that fills it; the retired `BimMaterial` record, the `BimElement.Materials` typed column, and the `BimModel.Project` material fold are GONE, a material being a seam node the `Bake` fold reads; the occurrence usage rides the `Associate` edge `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and threading `LayerSetUsage`/`ProfileSetUsage` onto this composition node is the named seam violation — the type-level SET structure is shared, the per-occurrence geometric binding is the edge's; the `ProfileSet` arm carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section type and NOT inline `IfcParameterizedProfileDef` dimensions — the full parametric section is preserved in the content-addressed store the `ContentKey` keys and the canonical section properties resolve one-hop to the catalog above the seam, so a profile-name-only `ProfileRef` that drops the content key is the deleted form, the content key is the kernel seed-zero `XxHash128` `ContentHash.Of` (the up-stratum `Rasm.Compute` `InterchangeIdentity` being the H7 strata defect), and a compound set preserves BOTH levels — every row's own profile AND the declared `CompositeProfile` as the seam `Composite` — so a `.Head`-only read that drops the trailing rows and a composite-over-primary read that destroys row zero's plate geometry are both deleted forms; every IFC per-row COMPOSITION column the folds carry is a round-trip FIXED POINT, not a one-way read, its egress peer stated at `[03]-[EGRESS]` — the typed-Pset half is a declared ASYMMETRY (typed out, neutral bag back) and claiming a fixed point over it is the overclaim; an imported material Pset lands as a NEUTRAL seam `PropertyBag` under `PropertySource.Import` and never as typed `MaterialPropertySet` columns, because that family is full-vector by construction and a partial foreign Pset folded onto a case fabricates every undeclared column — its values narrow through the ONE `PropertyLowering.Lower`, a second `IfcValue` narrowing on this page being the deleted fork, and its row keys mint through the owner-blessed `PropertyCategory.Seam.Row` EMPTY-prefix category so a round-tripped foreign name stays bare in the one key space every reader shares, a call-site `PropertyName.Create` being the fork the branch row-name custody ruling deletes; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `MaterialLayer` thickness coerces the NATIVE-unit `LayerThickness` through `UnitScale.Coerce` and admits through the QTO-identity `MeasureValue.OfSi(QuantityType, Dimension, double)` — a bare double, a page-local factor member, the raw `MeasureValue` ctor that bypasses the owner's SI admission, a dimension-anonymous admit that strips the takeoff's QTO identity, OR treating the native length as already-SI (the mm-vs-metre import trap) are the named defects; the section geometry binds by content-hash reference and a RhinoCommon profile field or an in-process BRep evaluation is the named seam violation; an unresolvable material-select entity lifts `Model/faults#FAULT_BAND` `BimFault.ModelRejected` BARE (band 2600 IS the `Expected` `Code`, the ingress on `ctx.Key` and the egress on the page `Egress` gate) and the seam `MaterialComposition` admission lifts `ElementFault.ValueRejected` BARE on a degenerate set, a `.ToError()` lowering hop (or a hand-built `Error.New(2600, …)`) bypassing the typed case being the named seam defect; the mechanical column crossings are ONE `[Mapper]` boundary transcription and a hand-rolled sentinel/logical/substance narrowing beside `MaterialShape` is the deleted form, while a crossing carrying a rail, a store call, or a memo stays hand-written by law — Mapperly transcribes shape, never a `Fin` lane.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using Riok.Mapperly.Abstractions;
using Rasm.Bim;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Semantics;

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The mechanical GeometryGym<->seam column crossings BOTH projector halves compose, generated rather than
// hand-transcribed: every member here is a 1:1 name-or-carrier map with no rail, no store call, and no memo, which
// is exactly the boundary Riok.Mapperly owns. The constituent row generates WHOLE (its four columns cross through
// Substance and Label); the layer/profile folds stay hand-written because their thickness and offsets ride the Fin
// rail and their profile rides the content-addressed store — Mapperly transcribes SHAPE, never a lane. [UserMapping]
// marks the carriers the GENERATED Row reaches; the junction and logical narrowings belong to those hand-written
// folds alone (no seam constituent column carries a priority or a ventilation), so marking them declares a
// generated consumer that does not exist.
[Mapper]
public static partial class MaterialShape {
    // PartName is the constituent's own IFC Name — the part it FORMS, a different axis from its function Category,
    // so two rows sharing one category stay addressable where the name-dropping fold collapsed them.
    [MapProperty(nameof(IfcMaterialConstituent.Name), nameof(MaterialConstituent.PartName))]
    public static partial MaterialConstituent Row(IfcMaterialConstituent constituent);

    // The substance carrier every row crosses: an unbound IfcMaterial lands the blank MaterialId the seam admits.
    [UserMapping]
    internal static MaterialId Substance(IfcMaterial? material) => MaterialId.Of(material?.Name ?? "");

    // The [SENTINEL_PROJECTION] site: GeometryGym spells an unset priority as int.MinValue (its setter clamps
    // anything outside the IFC [0,100] percentage to that sentinel and its STEP writer emits `$`), so the sentinel
    // dies here as None and never reaches the seam, the content hash, or the wire.
    internal static Option<int> Junction(int priority) => priority == int.MinValue ? None : Some(priority);

    // The three-state IfcLogical narrowed onto the Option<bool> Properties/property#PROPERTY_VALUE Logical already
    // ratifies (None = UNKNOWN): EN ISO 6946 drops a well-ventilated layer from the series-resistance fold, so an
    // UNKNOWN silently read as FALSE falsifies every U-value downstream — absence stays a refusal input, never a default.
    internal static Option<bool> Ventilation(IfcLogicalEnum value) => value switch {
        IfcLogicalEnum.TRUE => Some(true),
        IfcLogicalEnum.FALSE => Some(false),
        _ => None,
    };

    [UserMapping]
    internal static string Label(string? value) => value ?? "";
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The one GeometryGym->seam material lowering: IfcMaterialSelect -> seam Node.Material carrying the seam
// MaterialComposition. The seam OWNS the algebra (MaterialComposition/MaterialLayer/MaterialConstituent/
// ProfileRef); this projector only discriminates the IFC entity and fills it. The occurrence usage (layer
// direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the Associate edge
// MaterialUsage payload the Projection/semantic EdgeProjection authors [OCCURRENCE_USAGE_RULING]. The section is a
// neutral ProfileRef whose ContentKey keys the full IfcProfileDef STEP (parametric dims preserved in the store).
// The class is partial across its ingress cluster and its [03]-[EGRESS] half, one owner in one source file.
public static partial class MaterialProjection {
    // ONE ingress fold: discriminate the IfcMaterialSelect runtime entity, build the seam MaterialComposition through
    // the seam Of-prefixed smart-constructors (the Fin-railing OfLayerSet/OfConstituentSet own the empty-set / non-positive-
    // thickness / unnormalized-fraction admission -> ElementFault, the total OfSingle/OfProfileSet lift into Fin for Mint),
    // and Mint the content-keyed Node.Material ONCE — the per-modality factories
    // collapse onto the LayerSet/ProfileSet builders (reached from both the bare set and its occurrence-usage wrapper)
    // plus the two inline ConstituentSet/Single arms, the usage entities unwrapping to their underlying set (the
    // occurrence payload rides the Associate edge, not this node). A bare IfcMaterial folds to Single. `tolerance` is
    // the seam SI Header.Tolerance (NOT the native db.Tolerance), so the SI-coerced measures quantize on an SI grid in
    // ToCanonicalBytes — the SemanticProjector coerces db.Tolerance through the same UnitScale before threading it here.
    // The parameter is the TYPED IfcMaterialSelect the IfcRelAssociatesMaterial.RelatingMaterial property carries (the
    // public GG select interface all six admitted cases implement) — a BaseClassIfc admission is the deleted weak form.
    // The superseded IfcMaterialList also implements the select and lands on the boundary arm BY LAW: the seam
    // MaterialComposition trichotomy-plus-single is frozen and IfcMaterialList is never admitted.
    public static Fin<Node.Material> Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, UnitScale scale, Op key) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage u    => Optional(u.ForLayerSet).ToFin(new BimFault.ModelRejected(key, "material-layer-set-usage-unbound")).Bind(set => LayerSetOf(set, tolerance, scale, key)),
            IfcMaterialProfileSetUsage u  => Optional(u.ForProfileSet).ToFin(new BimFault.ModelRejected(key, "material-profile-set-usage-unbound")).Bind(set => ProfileSetOf(set, tolerance, profiles, scale, key)),
            IfcMaterialLayerSet set       => LayerSetOf(set, tolerance, scale, key),
            IfcMaterialProfileSet set     => ProfileSetOf(set, tolerance, profiles, scale, key),
            IfcMaterialConstituentSet set => Mint(set.Name ?? "", tolerance, MaterialComposition.OfConstituentSet(ConstituentsOf(set), key)),
            IfcMaterial material          => Mint(material.Name ?? "", tolerance, Fin.Succ<MaterialComposition>(MaterialComposition.OfSingle(MaterialId.Of(material.Name ?? "")))),
            _                             => Fin.Fail<Node.Material>(new BimFault.ModelRejected(key, $"material-select-unresolved:{relatingMaterial.GetType().Name}")),
        };

    // The two SET modalities each reachable from a bare set AND its occurrence-usage wrapper (so the usage arm and
    // the set arm share one builder): the LayerSet folds its rows; the ProfileSet folds the WHOLE MaterialProfiles
    // list onto the seam compound arm — every row keeping its own material, content-keyed profile, junction priority,
    // function category, and reference-axis offsets — beside the set's declared CompositeProfile as the seam Composite
    // (the full compound geometry of a built-up/composite member, the section identity a consumer resolves one-hop).
    // Empty-set refusal belongs to the seam OfProfileSet admission, never a second Bim-local guard. Node MaterialKey
    // carries the IFC set name; each composition carries its own materials.
    static Fin<Node.Material> LayerSetOf(IfcMaterialLayerSet set, double tolerance, UnitScale scale, Op key) =>
        LayersOf(set, scale).Bind(layers => Mint(set.Name ?? "", tolerance, MaterialComposition.OfLayerSet(layers, key)));

    static Fin<Node.Material> ProfileSetOf(IfcMaterialProfileSet set, double tolerance, IIfcProfileStore profiles, UnitScale scale, Op key) =>
        from rows in ProfilesOf(set, profiles, scale, key)
        from material in Mint(set.Name ?? "", tolerance, MaterialComposition.OfProfileSet(rows, key, CompositeOf(set, profiles, key)))
        select material;

    // Each IfcMaterialLayer -> seam MaterialLayer (MaterialId + SI MeasureValue thickness + layer name + the IFC per-row
    // Priority/Category/IsVentilated columns). GeometryGym stores LayerThickness in the model's NATIVE units (mm in most
    // Revit/ArchiCAD exports), never pre-coerced, so it crosses the ONE UnitScale.Coerce entry over MeasureRow.Length and
    // admits through the QTO-identity OfSi overload: a layer thickness IS the length takeoff the BASE_QUANTITIES fold
    // reads, and the dimension-only admit would stamp the dimension-anonymous QuantityType and strip that read.
    // An IfcMaterialLayerWithOffsets row folds through these columns alone: the subtype declares no public
    // constructor and keeps its offset vector on internal fields with no accessor, so the per-layer offsets are
    // unreachable at the GeometryGym public surface while the PROFILE subtype's OffsetValues is public and does fold.
    // Row Description is the one IFC annotation column the seam declines: it carries no analytical read, where Category
    // drives the assembly fold and Priority the junction resolution.
    static Fin<Seq<MaterialLayer>> LayersOf(IfcMaterialLayerSet set, UnitScale scale) =>
        set.MaterialLayers.AsIterable()
            .ToSeq()
            .TraverseM(layer => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, scale.Coerce(layer.LayerThickness, MeasureRow.Length, null))
                .Map(thickness => new MaterialLayer(
                    MaterialShape.Substance(layer.Material), thickness, MaterialShape.Label(layer.Name),
                    MaterialShape.Junction(layer.Priority), MaterialShape.Label(layer.Category), MaterialShape.Ventilation(layer.IsVentilated))))
            .As();

    static Seq<MaterialConstituent> ConstituentsOf(IfcMaterialConstituentSet set) =>
        set.MaterialConstituents.Values.AsIterable().Map(MaterialShape.Row).ToSeq();

    // Every IfcMaterialProfile row -> a seam MaterialProfile: its own material (the set name the fallback when a row
    // declares none), its OWN content-keyed ProfileRef (the row's full parametric section preserved in the store, so a
    // plate girder's web and flange plates each survive where the primary-only read kept one), the [0,100] junction
    // Priority, the function Category, and the reference-axis Offsets. Identity is Designation plus the kernel seed-zero
    // XxHash128 content key of the FULL IfcProfileDef STEP through the ONE kernel ContentHash entry the Model/
    // elements#REPRESENTATION_KEYS keyer also composes (NEVER the up-stratum Rasm.Compute InterchangeIdentity, the H7
    // named strata defect); Standard is left to the one-hop VividOrange catalog resolution [M7].
    static Fin<Seq<MaterialProfile>> ProfilesOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, UnitScale scale, Op key) =>
        set.MaterialProfiles.AsIterable()
            .ToSeq()
            .TraverseM(row => Optional(row.Profile)
                .ToFin(new BimFault.ModelRejected(key, $"material-profile-missing:{set.Name}:{row.Name}"))
                .Bind(profile => OffsetsOf(row, scale)
                    .Map(offsets => new MaterialProfile(
                        row.Material is { } material ? MaterialShape.Substance(material) : MaterialId.Of(set.Name ?? ""),
                        profiles.Preserve(profile, key),
                        MaterialShape.Junction(row.Priority), MaterialShape.Label(row.Category), offsets))))
            .As();

    // CompositeOf preserves the set-level combined outline — set.CompositeProfile when the set declares its compound
    // geometry (plate girder, steel-concrete composite — the settable IfcCompositeProfileDef) — in the SAME
    // content-addressed store as the rows, so seam Composite is the one-hop section identity and row zero keeps its plate.
    static Option<ProfileRef> CompositeOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, Op key) =>
        Optional(set.CompositeProfile).Map(composite => profiles.Preserve(composite, key));

    // IfcMaterialProfileWithOffsets publishes OffsetValues as a public double[] of arity one or two (start then optional
    // end) — the ONE per-row offset channel GeometryGym exposes, each entry a native-unit IfcLengthMeasure crossing the
    // same UnitScale.Coerce entry and the same QTO-identity OfSi gate the layer thickness crosses. A base
    // IfcMaterialProfile yields the EMPTY vector, the IFC LIST[1:2] arity making empty-versus-present a bijection.
    static Fin<Seq<MeasureValue>> OffsetsOf(IfcMaterialProfile row, UnitScale scale) =>
        row is IfcMaterialProfileWithOffsets offsets
            ? toSeq(offsets.OffsetValues).TraverseM(value => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, scale.Coerce(value, MeasureRow.Length, null))).As()
            : Fin.Succ(Seq<MeasureValue>());

    // The content-keyed seam Material node from a built composition: mint the id from its own canonical bytes (id
    // excluded) so two structurally-identical materials dedup to one node; the node MaterialKey is the IFC set/
    // material name, the typed property sets are the authored lane's (empty at IFC ingest — the imported
    // HasProperties lane is seam-owned, recorded at the seam). The draft id is a discarded placeholder, and a failed
    // composition admission threads through. A class-root [Union] Node case has NO compiler-generated `with`, so the
    // content id re-stamps through the seam Graph/element#NODE_MODEL Node.Relabel (a `draft with { Id }` a class case
    // cannot honour is the deleted form, the SAME re-stamp the Rasm.Materials Mint takes).
    static Fin<Node.Material> Mint(string name, double tolerance, Fin<MaterialComposition> composition) =>
        composition.Map(c => {
            var draft = new Node.Material(NodeId.Content(default), MaterialId.Of(name), c, Seq<MaterialPropertySet>());
            return (Node.Material)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
        });

    // IfcMaterialDefinition.HasProperties is a public SET<IfcMaterialProperties> the AUTHORED
    // typed lane never fills, so an imported Pset_MaterialMechanical/Thermal or a vendor set lands as a NEUTRAL seam
    // PropertyBag keyed by its own foreign set name over the OPEN Properties/property#PROPERTY_VALUE PropertyName key,
    // ranked PropertySource.Import beneath any derived or user value, its precedence read from the catalogue's own
    // templatetype through the ONE Semantics/properties#PROPERTY_TEMPLATES PropertyInheritance.ModeOf classifier
    // (PSET_MATERIALDRIVEN resolving OccurrenceWins) rather than a mode hardcoded here. It NEVER widens the typed seam
    // MaterialPropertySet cases: that family is full-vector by construction, so folding a partial imported Pset onto a case
    // would fabricate every column the foreign author never declared. Values narrow through the ONE Projection/
    // semantic#SEMANTIC_PROJECTOR PropertyLowering.Lower the element-level bag ingest already composes — a second IfcValue
    // narrowing here is the deleted fork. The bag carries no node id because an IfcMaterialProperties is not IfcRoot: the
    // projector content-mints its Node.PropertySet and binds it to the Material node by one Assign.PropertyDefinition
    // edge, exactly the landing every element-level bag takes — this reader answers BAGS and the
    // Projection/semantic#SEMANTIC_PROJECTOR Materials fold is the node producer, so no id is assembled twice.
    // Lowering a FOREIGN value is the one place this owner can narrow, so the reader RETURNS its narrowing facts beside
    // its bags in a Noted<A> and the parent joins them monoidally — the fold's own state, never a FidelityLog parameter
    // this reader mutates on a caller's behalf (an accumulator passed IN reads as a value and behaves as a channel).
    public static Fin<Noted<Seq<PropertyBag>>> ImportedPsets(
        IfcMaterialDefinition definition, Map<string, NodeId> rooted, UnitScale scale, TemplateScope templates, Op key) =>
        definition.HasProperties.AsIterable().ToSeq()
            .TraverseM(pset => pset.Properties.Values.AsIterable().ToSeq()
                .TraverseM(property => PropertyLowering.Lower(property, rooted, scale, key)
                    .Map(lowered => (Name: PropertyCategory.Seam.Row(property.Name ?? ""), Lowered: lowered)))
                .As()
                .Map(rows => new Noted<PropertyBag>(
                    rows.Fold(FidelityLog.Empty, static (log, row) => log + row.Lowered.Log),
                    new PropertyBag(
                        pset.Name ?? "",
                        rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Lowered.Value)),
                        PropertyInheritance.ModeOf(pset.Name ?? "", typeBound: false, templates),
                        PropertySource.Import))))
            .As()
            .Map(static bags => Noted.Join(bags));
}
```

## [03]-[EGRESS]

- Owner: the `MaterialProjection` egress half the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authoring the type-level `MaterialComposition` onto the GeometryGym material-definition family and lowering the seam `MaterialPropertySet` set onto its `IfcMaterialProperties` Pset through the `MaterialColumn` table, `AuthorUsage` wrapping that shared definition in the per-occurrence usage entity the `Associate` edge carries [OCCURRENCE_USAGE_RULING], and `EmitMemo<TKey,TValue>` the db-scoped emit memo this cluster DECLARES and the `Semantics/classification#CLASSIFICATION_AXIS` dictionary-source egress composes — ONE owner for the `ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<…>>` shape, so a second hand-declared table anywhere in the package is the deleted duplicate.
- Entry: `MaterialProjection.AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScale scale)` authors the type-level `MaterialComposition` ONCE (`Single`→`IfcMaterial`, `LayerSet`→`IfcMaterialLayerSet`, `ProfileSet`→`IfcMaterialProfileSet`, `ConstituentSet`→`IfcMaterialConstituentSet`), rails the seam `MaterialPropertySet` set through `AuthorPropertySet` onto the `IfcMaterialProperties` named Psets as TYPED columns, and reconstitutes a `ProfileSet`'s `IfcProfileDef` from the injected `profiles` store — a store miss authoring the entity the carried `profileSubtype` token names (the `DetailSchema.Realization` `ProfileSubtype` row the `Emit` resolves off the graph: `IfcRectangleProfileDef` completes whole from the baked `SectionProperties` dims, a voided token's mandatory inner curves stay store-preserved-only) — with `Fin<T>` aborting `BimFault.DanglingReference` keyed on the page `Egress` gate on an unresolvable profile and `BimFault.CodecReject` on a discipline case the column table holds no row for; `MaterialProjection.AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage, UnitScale scale)` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge carries, returning the bare definition for `MaterialUsage.None`.
- Auto: every SI seam magnitude leaves through `UnitScale.Declare` over `MeasureRow.Length` against the TARGET model's regime — the exact inverse of the ingress `Coerce`, so a metre-declared seam re-emits mm on a mm-declared target with no page-local factor; every seam row re-authors its own IFC row and every per-row column re-stamps, so the COMPOSITION columns — layer, constituent, profile row, composite outline, occurrence usage — are a FIXED POINT of the round-trip rather than a one-way read; the typed `MaterialPropertySet` half is DECLARED asymmetric and its fixed point is not claimed: this egress authors typed columns and the ingress lands an imported Pset as a NEUTRAL `PropertyBag` by law (the Element ruling bars a partial foreign set from folding onto a full-vector typed case), so a re-ingest of an authored `Pset_MaterialThermal` returns as an Import-ranked neutral bag on the SAME material node and never as the `Thermal` case that wrote it — the inverse fold is structurally barred, not missing; `AuthorPropertySet` folds the ONE `MaterialColumn` table — each row a `(set name, column name, typed reader)` triple whose reader type-tests its own discipline case and answers `Option<IfcValue>`, so a present column emits and an absent optional carrier (a Damping Rayleigh pair, a Hygrothermal capillary A-value) answers `None` where the retired arms branched per presence — and the `Environmental` module vector is minted from the seam `LifecycleStage` roster so a new EN 15978 module is a seam row and never a hand-added column; the shared base-`Evidence` provenance columns append once at the fold tail rather than per arm; a discipline case the table holds no row for rails `BimFault.CodecReject` because an empty authored Pset silently drops a whole discipline set; every authored material resolves through the db-scoped `MaterialOf` memo so ONE `IfcMaterial` per `(db, name)` serves every layer, constituent, profile, and node.
- Receipt: the authored `IfcMaterialDefinition` + its `IfcMaterialProperties` Psets + the per-occurrence usage entity are the IFC material subgraph the `Emit` writes, the type-level composition authored ONCE and the per-occurrence usage wrapping it so a wall and its mirror share one `IfcMaterialLayerSet` with two `IfcMaterialLayerSetUsage` instances; a re-ingest of that file re-keys the SAME seam `Node.Material` off the composition alone, which is what makes the COMPOSITION column set a fixed point rather than a claim — its typed property sets return as neutral Import bags bound to that same node, the declared asymmetry the `[02]` boundary states.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, NodaTime, LanguageExt.Core
- Growth: a new emitted material property is one `MaterialColumn` row on its discipline table naming its own `IfcValue` leaf (a new EN 15978 module is one seam `LifecycleStage` row the `Stages` mint iterates), never a per-property egress branch, never a re-widened double-only Pset, and never a twelfth bespoke `Pset(...)`+evidence scaffold; a new discipline case is one `Discipline<TCase>` block on the same table; a new db-scoped emit entity is one `EmitMemo<TKey,TValue>` field, never a second `ConditionalWeakTable` declaration.
- Boundary: the EGRESS reads the seam `Material` node + the `Associate` edge `MaterialUsage` ONLY — a Materials `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into this owner is the deleted form (those Materials wires are retired, the material egress reading the projected seam subgraph); an unset `Priority` re-authors unwritten (assigning GeometryGym's `int.MinValue` back is the sentinel re-introduction this projection deletes), an `UNKNOWN` ventilation re-authors `IfcLogicalEnum.UNKNOWN`, an absent `CardinalPoint` re-authors `IfcCardinalPointReference.DEFAULT` (the unset member both writers omit — electing `MID` promotes every unset usage into a declared mid-point on re-export), an absent `OffsetFromReferenceLine` RAILS `BimFault.CodecReject` because that attribute is MANDATORY and its writer emits unconditionally (a `double.NaN` into a required real writes a malformed STEP token), and a row with offsets re-authors the `IfcMaterialProfileWithOffsets` subtype at its declared one-or-two arity, never padded; the carried-token authored profile is the SINGLE-row fallback alone, because that token names the whole member and applying it per row of a compound authors one member rectangle N times — a compound row missing its preserved fragment rails `BimFault.DanglingReference`; the `IfcMaterialProperties` Pset attaches to the authored `IfcMaterialDefinition` and the `ProfileSet` `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP with a composite def re-stamping `CompositeProfile` on the authored set (a parametric dimension re-folded onto the seam being the deleted form); a store-missed Rasm-authored `ProfileSet` resolves its profile entity from the carried `DetailSchema.Realization` `ProfileSubtype` row and the baked `SectionProperties` dims — never a Materials call, and never a bare voided subtype with unassigned mandatory inner curves; every emitted column — the table's discipline rows AND the shared evidence tail alike — carries the GeometryGym `IfcValue` leaf its datum names into the `IfcPropertySingleValue(DatabaseIfc, string, IfcValue)` ctor, so a `Num`/`Text`/`Flag` rename layer over that surface, a primitive `(…, string)`/`(…, double)` overload that picks the leaf on the column's behalf, and a lossy double flattening of the `FireRating` class or the `Cost` `Currency`/`MeasurementBasis` label are all deleted forms; provenance is single-stored on the base `PropertyEvidence` (the retired per-case `Environmental` EPD/`ValidUntilYear` double-store and its suppression flag are GONE) and the expiry lowers through the ISO-8601 `LocalDatePattern.Iso` so the full date round-trips intact; the egress `AuthorPropertySet` is RAILED and a `void` Pset author sequenced through `Map` is the deleted form that made an uncolumned discipline case indistinguishable from a written one; the `EmitMemo` owner is keyed by the emit `DatabaseIfc` so the cache is emit-scoped and GC-collected with the database, and a durable or process-static material cache is the deleted form.

```csharp signature
// --- [SERVICES] -----------------------------------------------------------------------------
// The db-scoped emit memo every egress owner in this package keys its SHARED entities through: an
// IfcMaterial per (db, name) here, an IfcClassification per (db, system, edition) at Semantics/classification —
// ONE declaration of the ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<TKey,TValue>> shape, so a second
// hand-rolled table is the deleted duplicate. Keyed by the emit DatabaseIfc so the cache is emit-scoped and
// GC-collected with the database; the emit is db-serial (DatabaseIfc is single-threaded) and the inner dictionary
// guards reentry. EqualityComparer<TKey>.Default IS the ordinal comparison for a string key and the value-ordinal
// comparison for a tuple key, so no comparer knob exists to get wrong at a call site.
public sealed class EmitMemo<TKey, TValue>
    where TKey : notnull
    where TValue : class {
    readonly ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<TKey, TValue>> tables = new();

    public TValue Of(DatabaseIfc db, TKey key, Func<TKey, TValue> mint) =>
        tables.GetValue(db, static _ => new ConcurrentDictionary<TKey, TValue>()).GetOrAdd(key, mint);
}

// --- [MODELS] --------------------------------------------------------------------------------
// One authored material-Pset column: the IFC set it belongs to, its IFC property name, and the reader that
// type-tests its own discipline case and answers the GeometryGym IfcValue leaf ITS OWN DATUM names — the SI-typed
// IfcDerivedMeasureValue/IfcMeasureValue subtype for a dimensioned scalar (IfcMassDensityMeasure, IfcPressureMeasure,
// IfcThermalConductivityMeasure, IfcMonetaryMeasure, IfcMassMeasure), IfcNormalisedRatioMeasure/IfcPositiveRatioMeasure
// for a bounded ratio, IfcLabel for a class/currency/basis token, IfcBoolean for a flag, and IfcReal for the residual
// PURE numbers IFC declares no measure type for (an STC index, a per-second Rayleigh alpha, a mm/sqrt(year) carbonation
// rate, a per-minute fire rating whose name carries its unit). The blanket IfcReal the table formerly emitted made every
// dimensioned datum indistinguishable from a pure number on re-ingest, so the seam MeasureValue coercion had no declared
// type to key on and the Semantics/properties#TEMPLATE_AUDIT WrongDimension verdict went dark on every authored column.
// The leaf CONSTRUCTOR is the row's datatype declaration — a parallel DataType string column beside it would be a second
// spelling of the same fact with nothing reading it, so the row names its type exactly once, where it builds the value.
// An OPTIONAL carrier answers None and its column does not emit, which is what collapsed the per-arm presence branches
// the eleven bespoke Pset scaffolds each spelled by hand.
readonly record struct MaterialColumn(string Set, string Name, Func<MaterialPropertySet, Option<IfcValue>> Read);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The inverse half the Projection/egress#IFC_EGRESS Emit composes per seam Material node. This REPLACES the retired
// Rasm.Materials MaterialAssignmentWire/MaterialPropertyWire egress — the material subgraph reads off the seam graph
// directly. A ProfileSet's parametric IfcProfileDef reconstitutes one-hop from the content-addressed STEP store the
// ProfileRef.ContentKey keys; a store-missed Rasm-authored ProfileSet authors from the carried profileSubtype token,
// an unresolvable profile railing.
public static partial class MaterialProjection {
    // The page-local egress operation context: AuthorComposition/Definition are Emit-internal and carry no caller
    // Op, so an egress fault keys on this gate (the Projection/semantic#GRAPH_LEGALITY IfcLegality.Gate / Model/
    // faults#FAULT_BAND BimFault.Admission idiom) while the ingress Project threads the live ctx.Key. Every BimFault
    // lifts BARE (band 2600 IS the Expected Code per Model/faults#FAULT_BAND — the .ToError() lowering hop is its named defect).
    static readonly Op Egress = Op.Of(name: nameof(MaterialProjection));

    // ONE IfcMaterial per (db, name) shared across every Definition arm AND every AuthorComposition call, so a wall
    // LayerSet and a slab LayerSet both naming "Concrete" author ONE IfcMaterial entity, not one per layer/constituent/
    // node (the duplicate-material bloat). A material carries no edition axis, so (db, name) is the whole identity.
    static readonly EmitMemo<string, IfcMaterial> Materials = new();

    static IfcMaterial MaterialOf(DatabaseIfc db, string name) => Materials.Of(db, name, n => new IfcMaterial(db, n));

    public static Fin<IfcMaterialDefinition> AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScale scale) =>
        Definition(db, material.Composition, material.MaterialKey, profiles, profileSubtype, scale)
            .Bind(definition => material.Properties.TraverseM(set => AuthorPropertySet(db, definition, set)).As().Map(_ => definition));

    // The seam Composite re-stamps the authored set's CompositeProfile (settable, decompile-confirmed) so a re-ingest
    // keys the SAME composite CompositeOf preferred.
    static Fin<IfcMaterialDefinition> Definition(DatabaseIfc db, MaterialComposition composition, MaterialId key, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScale scale) =>
        composition.Switch(
            single:        s => Fin.Succ<IfcMaterialDefinition>(MaterialOf(db, s.Material.Value)),
            layerSet:      s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialLayerSet(
                                    s.Layers.Map(l => Layer(db, l, scale)), key.Value)),
            profileSet:    s => Rows(db, s, profiles, profileSubtype, scale).Map(rows => AuthorProfileSet(key, rows, s, profiles)),
            // Row name is the seam PartName, falling back to the material key when the part is unnamed — exactly the
            // blank-name convention GeometryGym's own IfcMaterialLayer constructor applies, never an empty IFC Name.
            constituentSet: s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialConstituentSet(key.Value,
                                    s.Constituents.Map(c => new IfcMaterialConstituent(
                                        string.IsNullOrEmpty(c.PartName) ? c.Material.Value : c.PartName,
                                        MaterialOf(db, c.Material.Value)) { Fraction = c.Fraction, Category = c.Category }))));

    // AuthorProfileSet stamps the compound marker: CompositeProfile lands ONLY when the seam Composite resolves to a
    // preserved IfcCompositeProfileDef, so a non-composite set never writes a null marker and the compound round-trip
    // reaches its fixed point (a re-ingest keys the SAME composite CompositeOf preferred).
    static IfcMaterialDefinition AuthorProfileSet(MaterialId key, Seq<IfcMaterialProfile> rows, MaterialComposition.ProfileSet set, IIfcProfileStore profiles) {
        var authored = new IfcMaterialProfileSet(key.Value, [.. rows]);
        set.Composite.Bind(profiles.Find).Bind(static profile => Optional(profile as IfcCompositeProfileDef))
            .IfSome(composite => authored.CompositeProfile = composite);
        return authored;
    }

    // One seam layer -> its IFC row with every column re-stamped through the property setters (the ctor carries material,
    // thickness, and name alone). The SI thickness leaves through UnitScale.Declare, the exact inverse of the ingress
    // Coerce against the TARGET model's declared unit. Priority assigns only when present, so an unset column stays `$`
    // in the STEP rather than round-tripping as a fabricated zero the ingress would then read as a real junction precedence.
    static IfcMaterialLayer Layer(DatabaseIfc db, MaterialLayer layer, UnitScale scale) {
        var row = new IfcMaterialLayer(MaterialOf(db, layer.Material.Value), scale.Declare(layer.Thickness.Si, MeasureRow.Length, null), layer.LayerName) {
            Category = layer.Category,
            IsVentilated = layer.Ventilated.Match(Some: static v => v ? IfcLogicalEnum.TRUE : IfcLogicalEnum.FALSE, None: static () => IfcLogicalEnum.UNKNOWN),
        };
        layer.Priority.IfSome(priority => row.Priority = priority);
        return row;
    }

    // Every compound row re-authors from its OWN preserved STEP fragment; the carried-token authored profile is the fallback
    // for a SINGLE-row set only, because that token names the whole member's geometry — applying it to each row of a
    // multi-row compound would author one member rectangle N times, so a compound row without its fragment rails instead.
    // Offsets on a row select the IfcMaterialProfileWithOffsets subtype — arity one takes the start-only constructor,
    // arity two the start-and-end pair — so the seam's LIST[1:2] vector round-trips at its declared arity, never padded.
    static Fin<Seq<IfcMaterialProfile>> Rows(DatabaseIfc db, MaterialComposition.ProfileSet set, IIfcProfileStore profiles, Option<string> profileSubtype, UnitScale scale) =>
        set.Profiles.TraverseM(row => profiles.Find(row.Profile)
                .Match(Some: Some, None: () => set.Profiles.Count == 1 ? AuthoredProfile(db, set, profileSubtype, scale) : None)
                .ToFin(new BimFault.DanglingReference(Egress, $"material-profile-step-unresolved:{row.Profile.Designation}"))
                .Map(profile => Row(db, row, profile, scale)))
            .As();

    static IfcMaterialProfile Row(DatabaseIfc db, MaterialProfile row, IfcProfileDef profile, UnitScale scale) {
        IfcMaterial material = MaterialOf(db, row.Material.Value);
        string name = row.Profile.Designation;
        IfcMaterialProfile authored = row.Offsets.Map(offset => scale.Declare(offset.Si, MeasureRow.Length, null)).ToArray() switch {
            [var start] => new IfcMaterialProfileWithOffsets(name, material, profile, start),
            [var start, var end] => new IfcMaterialProfileWithOffsets(name, material, profile, start, end),
            _ => new IfcMaterialProfile(name, material, profile),
        };
        authored.Category = row.Category;
        row.Priority.IfSome(priority => authored.Priority = priority);
        return authored;
    }

    // The carried-row profile author: a Rasm-authored ProfileSet preserves no STEP fragment, so the carried
    // DetailSchema.Realization ProfileSubtype token (the Materials occupancy derivation the Emit resolves off the
    // graph) names the authored entity — IfcRectangleProfileDef completes whole from the baked SectionProperties
    // (XDim the profile width, YDim the profile depth, SI -> declared units); a token whose mandatory interior geometry
    // only a preserved fragment carries (IfcArbitraryProfileDefWithVoids inner curves — inline curve geometry never
    // rides the seam) resolves None and the lane keeps its typed fault, never a bare subtype with unassigned mandatory curves.
    static Option<IfcProfileDef> AuthoredProfile(DatabaseIfc db, MaterialComposition.ProfileSet s, Option<string> subtype, UnitScale scale) =>
        subtype.Filter(static name => name == nameof(IfcRectangleProfileDef))
            .Bind(_ => s.Section.Map(section => (IfcProfileDef)new IfcRectangleProfileDef(
                db, s.Profile.Designation, scale.Declare(section.Width.Si, MeasureRow.Length, null), scale.Declare(section.Depth.Si, MeasureRow.Length, null))));

    // The per-occurrence usage [OCCURRENCE_USAGE_RULING]: a generated TOTAL Switch over the closed MaterialUsage union wraps the shared
    // definition in the IfcMaterialLayerSetUsage/ProfileSetUsage the Associate edge carries — a new usage arm breaks
    // this at compile time, never a runtime-silent _. A definition/usage modality mismatch faults at this boundary;
    // returning the bare definition would silently erase the occurrence binding. The neutral
    // LayerSetDirection/DirectionSense map to the GeometryGym enums inline (the inverse of the ingress Projection/
    // relations#RELATION_ALGEBRA UsageOf). The two optional occurrence columns take OPPOSITE landings because the
    // schema does: OffsetFromReferenceLine is MANDATORY and its STEP writer emits it unconditionally through
    // ParserSTEP.DoubleToString, so an absent seam offset RAILS rather than writing a NaN token into a required real;
    // CardinalPoint is OPTIONAL and IfcCardinalPointReference.DEFAULT is the package's own unset member both the STEP
    // and JSON writers omit, so absence re-authors as `$` where MID would promote every unset usage into a declared
    // mid-point on re-export. The layer-usage ReferenceExtent has NO public GG write channel (4-arg ctor only, setter
    // non-public — decompile-confirmed): the seam LayerSet.ReferenceExtent is ingest-only here; the profile-usage
    // setter is public, and NaN IS that field's own initializer and its DoubleOptionalToString absence sentinel.
    // The offset arms close over `scale`, so their lambdas are non-static by construction (CS8820).
    public static Fin<IfcMaterialSelect> AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage, UnitScale scale) => usage.Switch(
        none:       _ => Fin.Succ((IfcMaterialSelect)definition),
        layerSet:   u => definition is IfcMaterialLayerSet set
            ? u.OffsetFromReferenceLine
                .ToFin(new BimFault.CodecReject(Egress, $"layer-usage-offset-absent:{set.Name}"))
                .Map(offset => (IfcMaterialSelect)new IfcMaterialLayerSetUsage(set,
                    u.Direction switch { LayerSetDirection.Axis1 => IfcLayerSetDirectionEnum.AXIS1, LayerSetDirection.Axis2 => IfcLayerSetDirectionEnum.AXIS2, _ => IfcLayerSetDirectionEnum.AXIS3 },
                    u.Sense == DirectionSense.Positive ? IfcDirectionSenseEnum.POSITIVE : IfcDirectionSenseEnum.NEGATIVE,
                    scale.Declare(offset.Si, MeasureRow.Length, null)))
            : Fin.Fail<IfcMaterialSelect>(new BimFault.CodecReject(Egress, $"layer-usage-on:{definition.GetType().Name}")),
        profileSet: u => definition is IfcMaterialProfileSet set
            ? Fin.Succ<IfcMaterialSelect>(new IfcMaterialProfileSetUsage(set, u.CardinalPoint.Match(
                Some: static point => (IfcCardinalPointReference)point.Key,
                None: static () => IfcCardinalPointReference.DEFAULT)) {
                    ReferenceExtent = u.ReferenceExtent.Map(value => scale.Declare(value.Si, MeasureRow.Length, null)).IfNone(double.NaN),
                })
            : Fin.Fail<IfcMaterialSelect>(new BimFault.CodecReject(Egress, $"profile-usage-on:{definition.GetType().Name}")));

    // --- [COLUMN_TABLE] ------------------------------------------------------------------------
    // The ONE seam MaterialPropertySet -> IFC material Pset column table (IfcMaterialProperties : IfcExtendedProperties
    // named set on the IfcMaterialDefinition). Standard buildingSMART Psets carry their names
    // (Pset_MaterialMechanical/Thermal/Optical, Pset_EnvironmentalImpactValues, Pset_ConstructionCosts), the seam-native
    // carriers a Rasm_Material* name (Orthotropic/Damping/Hygrothermal/Durability). The FireRating reaction class and the
    // Cost Currency/MeasurementBasis ride IfcLabel columns (never a lossy double); the Damping Rayleigh pair and the
    // Hygrothermal capillary A-value are Option carriers whose rows answer None when absent, so presence is a row's own
    // answer rather than a per-arm spread.
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
            // A weighted STC is a dimensionless RATING index on no SI scale, so IfcReal is its honest leaf.
            ("SoundTransmissionClass", static a => Some<IfcValue>(new IfcReal(a.StcWeighted))))
        + Discipline<MaterialPropertySet.Fire>("Pset_MaterialFire",
            ("ReactionToFireClass", static f => Some<IfcValue>(new IfcLabel(f.Reaction.Key))),
            ("Combustible", static f => Some<IfcValue>(new IfcBoolean(f.Reaction.Combustible))),
            ("SmokeProduction", static f => Some<IfcValue>(new IfcLabel(f.Smoke.Key))),
            ("FlamingDroplets", static f => Some<IfcValue>(new IfcLabel(f.Droplets.Key))),
            // The R/E/I ratings are the EN 13501-2 MINUTE classes the seam stores and the standard names; an
            // IfcTimeMeasure leaf would declare SI seconds over a minute magnitude, so the pure number stands and the
            // column name carries the unit — the one place a rating's own vocabulary outranks an SI measure type.
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
            // A damping ratio passes unity on an overdamped system, so the POSITIVE ratio is its leaf and the
            // normalised one (whose GG ctor clamps to [0,1]) would silently cap the overdamped case at critical.
            ("DampingRatio", static d => Some<IfcValue>(new IfcPositiveRatioMeasure(d.DampingRatio))),
            ("StructuralLossFactor", static d => Some<IfcValue>(new IfcPositiveRatioMeasure(d.StructuralLossFactor))),
            // Rayleigh alpha is per-second (IFC declares no reciprocal-time measure); beta IS seconds.
            ("RayleighAlpha", static d => d.Rayleigh.Map(static r => (IfcValue)new IfcReal(r.AlphaPerS))),
            ("RayleighBeta", static d => d.Rayleigh.Map(static r => (IfcValue)new IfcTimeMeasure(r.BetaS))))
        + Discipline<MaterialPropertySet.Hygrothermal>("Rasm_MaterialHygrothermal",
            ("Porosity", static h => Some<IfcValue>(new IfcNormalisedRatioMeasure(h.Porosity))),
            ("WaterContent80RH", static h => Some<IfcValue>(new IfcMassDensityMeasure(h.WaterContent80Rh.Si))),
            ("FreeWaterSaturation", static h => Some<IfcValue>(new IfcMassDensityMeasure(h.FreeWaterSaturation.Si))),
            // kg/(m^2 sqrt(s)) — an EN ISO 15148 A-value on no IFC measure scale.
            ("WaterAbsorptionCoefficient", static h => h.WaterAbsorptionKgPerM2SqrtS.Map(static a => (IfcValue)new IfcReal(a))))
        + Discipline<MaterialPropertySet.Durability>("Rasm_MaterialDurability",
            // mm/sqrt(year), m^2/s diffusivity, and a bare exponent — three fib-34 durability scales IFC declares no
            // measure type for, so all three stand as pure numbers rather than borrowing an unrelated measure.
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

    // The per-case row mint: each reader is typed to ITS case and the table entry closes the type test over it, so a
    // row body never re-tests and the table stays one flat Seq over the whole discipline family.
    static Seq<MaterialColumn> Discipline<TCase>(string set, params (string Name, Func<TCase, Option<IfcValue>> Read)[] columns)
        where TCase : MaterialPropertySet =>
        toSeq(columns).Map(column => new MaterialColumn(set, column.Name,
            value => value is TCase typed ? column.Read(typed) : None));

    // The EN 15978 module vector is the roster-driven family the table exists for: ONE column per seam LifecycleStage
    // row (A1-A3..D) off the StageAt projection, never an aggregate that strands the seam StageGwp and never a
    // hand-added per-module column. EPD identity + expiry ride the base Evidence tail, not a per-case column.
    static Seq<MaterialColumn> Stages(string set) =>
        LifecycleStage.Items.AsIterable().ToSeq().Map(stage => new MaterialColumn(set, $"GlobalWarmingPotential_{stage.Module}",
            value => value is MaterialPropertySet.Environmental e ? Some<IfcValue>(new IfcReal(e.StageAt(stage))) : None));

    // ONE fold over the table per seam set: the rows that ANSWER become the Pset's typed columns, the shared
    // base-Evidence provenance columns append at the tail, and every row of one case shares one set name so the head
    // row names the Pset. A case NO row answers for is a table hole — authoring an evidence-only Pset would drop a
    // whole discipline set silently, so it rails instead.
    static Fin<Unit> AuthorPropertySet(DatabaseIfc db, IfcMaterialDefinition material, MaterialPropertySet set) {
        Seq<(string Set, IfcProperty Column)> typed = Columns.Choose(column =>
            column.Read(set).Map(value => (column.Set, Column: (IfcProperty)new IfcPropertySingleValue(db, column.Name, value))));
        return typed.Head
            .ToFin(new BimFault.CodecReject(Egress, $"material-pset-uncolumned:{set.GetType().Name}"))
            .Map(head => ignore(Pset(material, head.Set, typed.Map(static row => row.Column) + EvidenceColumns(db, set))));
    }

    // The shared base-Evidence columns every Pset appends: the provenance Source + Reference always, and the expiry
    // ONLY when present — the seam PropertyEvidence.ValidUntil is an Option<LocalDate> (the exact EC3 declaration expiry,
    // never the deleted lossy int YEAR), lowered to the ISO-8601 DataValidUntil label so the full date round-trips intact.
    // Each row names its IfcLabel leaf into the SAME (DatabaseIfc, string, IfcValue) ctor the column table takes: the
    // primitive (…, string) overload picks the leaf itself, which is the un-named datum the table exists to delete.
    static Seq<IfcProperty> EvidenceColumns(DatabaseIfc db, MaterialPropertySet set) =>
        Seq(
            (IfcProperty)new IfcPropertySingleValue(db, "DataSource", new IfcLabel(set.Evidence.Source)),
            new IfcPropertySingleValue(db, "DataReference", new IfcLabel(set.Evidence.Reference)))
        + set.Evidence.ValidUntil.Match(
            Some: d => Seq((IfcProperty)new IfcPropertySingleValue(db, "DataValidUntil", new IfcLabel(LocalDatePattern.Iso.Format(d)))),
            None: static () => Seq<IfcProperty>());

    // IfcMaterialProperties(string name, IfcMaterialDefinition mat) named Pset (the material already carries its db, so
    // none is threaded here); each column keys by its own Name on the inherited Dictionary<string, IfcProperty> Properties.
    // The (DatabaseIfc, string, IfcValue) ctor is decompile-confirmed, as are the IfcReal/IfcLabel/IfcBoolean leaves the
    // primitive overloads themselves construct.
    static IfcMaterialProperties Pset(IfcMaterialDefinition material, string name, Seq<IfcProperty> columns) {
        var pset = new IfcMaterialProperties(name, material);
        columns.Iter(p => pset.Properties[p.Name] = p);
        return pset;
    }
}
```

## [04]-[RESEARCH]

(none)
