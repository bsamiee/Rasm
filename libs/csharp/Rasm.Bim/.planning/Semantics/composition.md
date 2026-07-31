# [BIM_MATERIAL_COMPOSITION]

The IFC material PROJECTOR lowering the live GeometryGym `IfcMaterialSelect` surface onto the `Rasm.Element` seam `Material` node: `MaterialProjection.Project` discriminates the relating-material runtime entity — `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` — and folds it into one content-keyed seam `Node.Material` carrying the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`). The seam OWNS the construction-material algebra (`MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `ProfileRef`, `MaterialPropertySet`, `MaterialId`); this page owns ONLY the GeometryGym discrimination that fills it, never re-declaring a Bim `BimMaterial`/`BimMaterialComposition` — the retired `BimMaterial` record and the `BimElement.Materials` column are GONE, a material is a seam `Material` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, and the consumer reads `element.Materials` flat on the baked element rather than a second stored record keyed by `MaterialId`. The occurrence usage binding (layer direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the `Associate` edge `MaterialUsage` payload the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors [OCCURRENCE_USAGE_RULING], this owner producing only the type-level SET structure so a wall and its mirror share one `LayerSet` with two `Associate` usages. A linear member's section is a neutral `ProfileRef` (`Standard` + `Designation` + content key) the `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog [M7], its full `IfcProfileDef` parametric definition preserved in the content-addressed store the `ContentKey` keys — a compound set's declared `IfcCompositeProfileDef` `CompositeProfile` keyed over the primary row so a built-up section's combined geometry survives the store and re-stamps at egress — the page references NO VividOrange section type and folds NO parametric dimension onto the seam, because the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the profile geometry by content-hash reference, never a RhinoCommon type. An unresolvable material-select entity rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` lifted BARE (band 2600 IS the `Expected` `Code`; no `.ToError()` hop); a degenerate composition (empty set, non-positive layer thickness, unnormalized constituent fractions) rails the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` the seam `MaterialComposition` admission owns. The projector is BIDIRECTIONAL: `MaterialProjection.AuthorComposition`/`AuthorUsage` is the inverse half the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authors the type-level `MaterialComposition` back onto the GeometryGym material-definition family (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial`) ONCE per material and lowers the seam `MaterialPropertySet` set onto TYPED `IfcMaterialProperties` `Pset_Material*` rows (every case field round-trips — the `FireRating` class, the `Cost` `Currency`/`MeasurementBasis`, and the EPD id + expiry as the shared base-`Evidence` label columns every case appends the value-typed `IfcPropertySingleValue` ctor overload selects directly, never coerced to a lossy double, and the `Environmental` case emits its FULL EN 15978 per-`LifecycleStage` GWP vector one column per module, never a single aggregate sum, the newer `Optical`/`Damping`/`Hygrothermal`/`Durability` carriers each their own Pset), and `AuthorUsage` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Relations/relation#EDGE_ALGEBRA` `Associate` edge `MaterialUsage` carries [OCCURRENCE_USAGE_RULING]. This is the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — `Rasm.Bim` reads the projected `Material` subgraph directly, never a Materials wire. A `ProfileSet`'s full parametric `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the seam holds only the neutral `ProfileRef` + baked `SectionProperties`); a store-missed Rasm-authored profile authors as the entity the carried `DetailSchema.Realization` `ProfileSubtype` row names off the baked dims where the token's geometry completes, an unresolvable profile railing `BimFault.DanglingReference`.

## [01]-[INDEX]

- [02]-[MATERIAL_COMPOSITION]: `MaterialProjection.Project` the `IfcMaterialSelect`→seam `Node.Material` ingress fold, the per-modality `LayerSet`/`ProfileSet`/`ConstituentSet`/`Single` mapping onto the seam `MaterialComposition`, the per-row `ProfileRef` content-keyer beside the `CompositeOf` set-level outline, the `LayersOf`/`ConstituentsOf`/`ProfilesOf` row folds with their `PriorityOf`/`LogicalOf`/`OffsetsOf` column projections, the content-keyed `Mint` of the seam `Node.Material`, the `ImportedPsets` reader lowering `HasProperties` material Psets to neutral seam `PropertyBag`s, AND the inverse `MaterialProjection.AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node onto the GeometryGym material-definition family + the `MaterialPropertySet`→`IfcMaterialProperties` `Pset_Material*` rows + the `Associate`-edge `MaterialUsage`→`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [OCCURRENCE_USAGE_RULING].

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialProjection` the static BIDIRECTIONAL GeometryGym↔seam material projector — the `Project` ingress folding one `IfcMaterialSelect` runtime entity into one seam `Node.Material` (discriminating the entity, building the seam `MaterialComposition` through the seam smart-constructors, minting the content-keyed node id), and the `AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node back onto the GeometryGym material-definition family the `Projection/egress#IFC_EGRESS` `Emit` composes. The seam owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` rows, the `ProfileRef`, the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage`, the `ValueBag<V>` bag an imported Pset lands in, and the `MaterialPropertySet` engineering-property family; this page declares NONE of them — it composes the seam vocabulary, mapping the GeometryGym material-assembly entities onto it and back.
- Entry: `MaterialProjection.Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, Op key)` is the live-entity promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes when folding an `IfcRelAssociatesMaterial.RelatingMaterial` (the parameter IS the typed `IfcMaterialSelect` the property carries — a `BaseClassIfc` admission is the deleted weak form) — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` and `IfcMaterialProfileSetUsage` its `ForProfileSet`, the usage payload riding the `Associate` edge not this node; `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` its WHOLE `MaterialProfiles` list onto the seam per-row `MaterialProfile` spread beside the declared `CompositeProfile`, `IfcMaterialConstituentSet` its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and returns one content-keyed seam `Node.Material`; `MaterialProjection.ImportedPsets(definition, rooted, scale, templates, log, key)` is the peer ingress reading the imported `HasProperties` material Psets as neutral seam `PropertyBag`s the projector content-mints as `Node.PropertySet` nodes bound by one `Assign.PropertyDefinition` edge; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) and the seam `MaterialComposition` admission aborts a degenerate set (`ElementFault.ValueRejected`), each lifting BARE (the band IS the `Expected` `Code`; no `.ToError()` hop). `MaterialProjection.AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype)` is the egress entry the `Emit` composes — it authors the type-level `MaterialComposition` ONCE (`Single`→`IfcMaterial`, `LayerSet`→`IfcMaterialLayerSet`, `ProfileSet`→`IfcMaterialProfileSet`, `ConstituentSet`→`IfcMaterialConstituentSet`), folds the seam `MaterialPropertySet` set onto the `IfcMaterialProperties` named Psets as TYPED columns (each case field a `MeasureValue`/label/boolean plus row evidence where available, never a lossy double), and reconstitutes a `ProfileSet`'s `IfcProfileDef` from the injected `profiles` store — a store miss authoring the entity the carried `profileSubtype` token names (the `DetailSchema.Realization` `ProfileSubtype` row the `Emit` resolves off the graph: `IfcRectangleProfileDef` completes whole from the baked `SectionProperties` dims, a voided token's mandatory inner curves stay store-preserved-only) — with `Fin<T>` aborting `BimFault.DanglingReference` keyed on the page `Egress` gate on an unresolvable profile; `MaterialProjection.AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage)` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge carries [OCCURRENCE_USAGE_RULING], returning the bare definition for `MaterialUsage.None`.
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and builds the seam `MaterialComposition` through the seam `Of`-prefixed smart-constructors (the `Fin`-railing `MaterialComposition.OfLayerSet`/`OfConstituentSet` owning the empty-set / non-positive-thickness / unnormalized-fraction admission, the total `OfSingle`/`OfProfileSet` lifted into `Fin` for the `Mint` fold), then mints the seam `Node.Material` whose id is the kernel seed-zero `XxHash128` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical materials dedup to one node; `LayersOf` folds each `IfcMaterialLayer` onto a seam `MaterialLayer` carrying its `MaterialId`, a `MeasureValue` thickness over `Dimension.LengthDim` coerced to SI metres by the model's `IfcUnitAssignment.ScaleSI(LENGTHUNIT)` factor (the native `LayerThickness` is mm in most Revit/ArchiCAD exports, never pre-SI) and admitted through `MeasureValue.OfSi` so the seam carries the SI scalar `MeasureValue.Of` otherwise mandates, and its layer name; `ConstituentsOf` folds each `IfcMaterialConstituent` (read through the `Dictionary.Values`) onto a seam `MaterialConstituent` carrying its `MaterialId`, category, and `Fraction`; `ProfileRefOf` projects the KEYED section — the set's declared `IfcCompositeProfileDef` `CompositeProfile` when a compound set carries one (the combined built-up geometry, decompile-confirmed settable), else the primary row's `Profile` — onto a neutral `ProfileRef` whose `ContentKey` is the kernel seed-zero `XxHash128` `ContentHash.Of` over the tag-namespaced `IfcProfileDef` STEP (the full parametric section preserved in the content-addressed store; the ONE kernel hasher the `Model/elements#REPRESENTATION_KEYS` keyer also composes, never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]), the `Designation` the row profile's name, the `Standard` left to the one-hop catalog resolution; `ProfilesOf` folds EVERY `IfcMaterialProfile` row onto a seam `MaterialProfile` — its own material, its own content-keyed `ProfileRef`, its `[0,100]` junction `Priority`, its function `Category`, and its `IfcMaterialProfileWithOffsets.OffsetValues` reference-axis offsets SI-coerced by the same `LengthScale` — and `CompositeOf` preserves the set's declared `IfcCompositeProfileDef` as the seam `Composite`, so a built-up compound keeps every plate where the primary-only read kept one; `PriorityOf` is the `[SENTINEL_PROJECTION]` site retiring GeometryGym's `int.MinValue` unset priority to `None`, and `LogicalOf` narrows the three-state `IfcLogicalEnum` onto the seam `Option<bool>` so an `UNKNOWN` ventilation never reads as `FALSE` (the EN ISO 6946 falsification); the row `Description` is the one IFC annotation column the seam declines, carrying no analytical read where `Category` drives the assembly fold. Typed engineering property sets stay the AUTHORED lane's (the `Rasm.Materials` `ComponentProjector` lowers its catalog-backed `MaterialPropertySet` rows), so the IFC-ingest `Node.Material` carries an empty `Seq<MaterialPropertySet>` and `ImportedPsets` lands the imported `IfcMaterialDefinition.HasProperties` sets as NEUTRAL seam `PropertyBag`s instead — foreign set name, `PropertySource.Import` rank, `PropertyInheritance.ModeOf` precedence, values narrowed through the one `PropertyLowering.Lower` — because a partial imported Pset folded onto a full-vector typed case fabricates every undeclared column.
- Receipt: the seam `Node.Material` is the material evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Associate` edge into `element.Materials` (a `BakedMaterial` carrying the node plus its occurrence `MaterialUsage` — the seam Bake-folded accessor, DISTINCT from the `Rasm.Materials` projection-input `MaterialBinding` and the type→occurrence `TypeBinding`), the `Model/query#ELEMENT_SET` material predicate matches by `MaterialId` or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#BASE_QUANTITIES` layered-volume takeoff reads from the `LayerSet` thicknesses; the layer build-up, the section material, and the constituent mix each carry their real composition on one seam node, never a parallel layer/profile/constituent record family.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new material-assembly modality is one seam `MaterialComposition` union arm (the seam's, not this page's) plus one `Project` switch arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the seam `MaterialLayer`/`MaterialConstituent`/`MaterialProfile` filled by its owning `LayersOf`/`ConstituentsOf`/`ProfilesOf` fold and re-stamped by its `Layer`/`Row` egress peer; a new section catalog is one `ProfileRef.Standard` token the `Rasm.Materials` projector resolves, never a seam edit; a new emitted material property is one typed `IfcPropertySingleValue` column the value-typed ctor overload selects on the matching `AuthorPropertySet` arm (a new EN 15978 module is one `LifecycleStage` row the environmental fold iterates, never a hand-added column), never a per-property egress branch and never a re-widened double-only `Pset`; never a per-element-class material type, never a Bim `BimMaterial` record beside the seam node, and never a parallel material store.
- Boundary: the material model is the seam `Node.Material` + `MaterialComposition` and a Bim `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile`/`LayerSetUsage`/`ProfileSetUsage`/`ProfileDefKind`/`ProfileDims` re-declaration is the deleted form — the seam owns the algebra, this page owns only the GeometryGym discrimination that fills it; the retired `BimMaterial` record, the `BimElement.Materials` typed column, and the `BimModel.Project` material fold are GONE, a material being a seam node the `Bake` fold reads; the occurrence usage rides the `Associate` edge `MaterialUsage` payload [OCCURRENCE_USAGE_RULING] and threading `LayerSetUsage`/`ProfileSetUsage` onto this composition node is the named seam violation — the type-level SET structure is shared, the per-occurrence geometric binding is the edge's; the `ProfileSet` arm carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section type and NOT inline `IfcParameterizedProfileDef` dimensions — the full parametric section is preserved in the content-addressed store the `ContentKey` keys and the canonical section properties resolve one-hop to the catalog above the seam, so a profile-name-only `ProfileRef` that drops the content key is the deleted form, the content key is the kernel seed-zero `XxHash128` `ContentHash.Of` (the up-stratum `Rasm.Compute` `InterchangeIdentity` being the H7 strata defect), and a compound set preserves BOTH levels — every row's own profile AND the declared `CompositeProfile` as the seam `Composite` — so a `.Head`-only read that drops the trailing rows and a composite-over-primary read that destroys row zero's plate geometry are both deleted forms; every IFC per-row column the folds now carry is a round-trip FIXED POINT, not a one-way read — an unset `Priority` re-authors unwritten (assigning GeometryGym's `int.MinValue` back is the sentinel re-introduction this projection deletes), an `UNKNOWN` ventilation re-authors `IfcLogicalEnum.UNKNOWN`, and a row with offsets re-authors the `IfcMaterialProfileWithOffsets` subtype at its declared one-or-two arity, never padded; the carried-token authored profile is the SINGLE-row fallback alone, because that token names the whole member and applying it per row of a compound authors one member rectangle N times — a compound row missing its preserved fragment rails `BimFault.DanglingReference`; an imported material Pset lands as a NEUTRAL seam `PropertyBag` under `PropertySource.Import` and never as typed `MaterialPropertySet` columns, because that family is full-vector by construction and a partial foreign Pset folded onto a case fabricates every undeclared column — and its values narrow through the ONE `PropertyLowering.Lower`, a second `IfcValue` narrowing on this page being the deleted fork; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `MaterialLayer` thickness coerces the NATIVE-unit `LayerThickness` to SI metres through the model's `ScaleSI(LENGTHUNIT)` factor and admits through `MeasureValue.OfSi` over `Dimension.LengthDim` (the inverse `/ScaleSI` applied on egress for a non-SI target model) — a bare double, the raw `MeasureValue` ctor that bypasses the owner's SI admission, OR treating the native length as already-SI (the mm-vs-metre import trap a Revit export springs) is the named defect; the section geometry binds by content-hash reference and a RhinoCommon profile field or an in-process BRep evaluation is the named seam violation; an unresolvable material-select entity lifts `Model/faults#FAULT_BAND` `BimFault.ModelRejected` BARE (band 2600 IS the `Expected` `Code`, the ingress on `ctx.Key` and the egress on the page `Egress` gate) and the seam `MaterialComposition` admission lifts `ElementFault.ValueRejected` BARE on a degenerate set, a `.ToError()` lowering hop (or a hand-built `Error.New(2600, …)`) bypassing the typed case being the named seam defect; the EGRESS reads the seam `Material` node + the `Associate` edge `MaterialUsage` ONLY — a Materials `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into this owner is the deleted form (those Materials wires are retired, the material egress reading the projected seam subgraph), the type-level composition authored ONCE and the per-occurrence usage wrapping it so a wall and its mirror share one `IfcMaterialLayerSet` with two `IfcMaterialLayerSetUsage` instances, every authored material resolving through the db-scoped `MaterialOf` memo so one `IfcMaterial` per `(db, name)` serves every layer/constituent/profile/node (a fresh `IfcMaterial` per row is the named duplicate-entity bloat, the same memo shape the `ClassificationSystem` dictionary-source egress holds), the `IfcMaterialProperties` Pset attaching to the authored `IfcMaterialDefinition` and the `ProfileSet` `IfcProfileDef` reconstituting one-hop from the content-addressed STEP with a composite def re-stamping `CompositeProfile` on the authored set (a parametric dimension re-folded onto the seam being the deleted form); a store-missed Rasm-authored `ProfileSet` resolves its profile entity from the carried `DetailSchema.Realization` `ProfileSubtype` row and the baked `SectionProperties` dims — never a Materials call, and never a bare voided subtype with unassigned mandatory inner curves.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using Rasm.Bim;
using Rasm.Bim.Projection;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Semantics;

// --- [OPERATIONS] -------------------------------------------------------------------------
// The one GeometryGym->seam material lowering: IfcMaterialSelect -> seam Node.Material carrying the seam
// MaterialComposition. The seam OWNS the algebra (MaterialComposition/MaterialLayer/MaterialConstituent/
// ProfileRef); this projector only discriminates the IFC entity and fills it. The occurrence usage (layer
// direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the Associate edge
// MaterialUsage payload the Projection/semantic EdgeProjection authors [OCCURRENCE_USAGE_RULING]. The section is a neutral
// ProfileRef whose ContentKey keys the full IfcProfileDef STEP (parametric dims preserved in the store).
public static class MaterialProjection {
    // The page-local egress operation context: AuthorComposition/Definition are Emit-internal and carry no caller
    // Op, so an egress fault keys on this gate (the Projection/semantic#GRAPH_LEGALITY IfcLegality.Gate / Model/
    // faults#FAULT_BAND BimFault.Admission idiom) while the ingress Project threads the live ctx.Key. Every BimFault
    // lifts BARE (band 2600 IS the Expected Code per Model/faults#FAULT_BAND — the .ToError() lowering hop is its named defect).
    static readonly Op Egress = Op.Of(name: nameof(MaterialProjection));

    // ONE ingress fold: discriminate the IfcMaterialSelect runtime entity, build the seam MaterialComposition through
    // the seam Of-prefixed smart-constructors (the Fin-railing OfLayerSet/OfConstituentSet own the empty-set / non-positive-
    // thickness / unnormalized-fraction admission -> ElementFault, the total OfSingle/OfProfileSet lift into Fin for Mint),
    // and Mint the content-keyed Node.Material ONCE — the four per-modality factories
    // collapse onto the LayerSet/ProfileSet builders (reached from both the bare set and its occurrence-usage wrapper)
    // plus the two inline ConstituentSet/Single arms, the usage entities unwrapping to their underlying set (the
    // occurrence payload rides the Associate edge, not this node). A bare IfcMaterial folds to Single. `tolerance` is
    // the seam SI Header.Tolerance (NOT the native db.Tolerance), so the SI-coerced measures quantize on an SI grid in
    // ToCanonicalBytes — the SemanticProjector scales db.Tolerance by the model LengthScale before threading it here.
    // The parameter is the TYPED IfcMaterialSelect the IfcRelAssociatesMaterial.RelatingMaterial property carries (the
    // public GG select interface all six admitted cases implement) — a BaseClassIfc admission is the deleted weak form.
    // The deprecated IfcMaterialList also implements the select and lands on the boundary arm BY LAW: the seam
    // MaterialComposition trichotomy-plus-single is frozen and IfcMaterialList is never admitted.
    public static Fin<Node.Material> Project(IfcMaterialSelect relatingMaterial, double tolerance, IIfcProfileStore profiles, Op key) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage u    => Optional(u.ForLayerSet).ToFin(new BimFault.ModelRejected(key, "material-layer-set-usage-unbound")).Bind(set => LayerSetOf(set, tolerance, key)),
            IfcMaterialProfileSetUsage u  => Optional(u.ForProfileSet).ToFin(new BimFault.ModelRejected(key, "material-profile-set-usage-unbound")).Bind(set => ProfileSetOf(set, tolerance, profiles, key)),
            IfcMaterialLayerSet set       => LayerSetOf(set, tolerance, key),
            IfcMaterialProfileSet set     => ProfileSetOf(set, tolerance, profiles, key),
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
    static Fin<Node.Material> LayerSetOf(IfcMaterialLayerSet set, double tolerance, Op key) =>
        LayersOf(set).Bind(layers => Mint(set.Name ?? "", tolerance, MaterialComposition.OfLayerSet(layers, key)));

    static Fin<Node.Material> ProfileSetOf(IfcMaterialProfileSet set, double tolerance, IIfcProfileStore profiles, Op key) =>
        from rows in ProfilesOf(set, profiles, key)
        from material in Mint(set.Name ?? "", tolerance, MaterialComposition.OfProfileSet(rows, key, CompositeOf(set, profiles, key)))
        select material;

    // The model's length-unit -> SI-metre factor (ScaleSI returns 1.0 when no length unit is declared, so a unitless or
    // SI model needs no branch). GeometryGym stores IfcMaterialLayer.LayerThickness in the model's NATIVE units (mm in
    // most Revit/ArchiCAD exports), never pre-coerced — so every length crossing into the SI-canonical seam MeasureValue
    // multiplies by this factor on ingress and divides by the target model's factor on egress, the ONE coercion the seam
    // Properties/quantity#MEASURE_VALUE owner mandates "once at admission". Two callers (LayersOf + Definition), not thin.
    static double LengthScale(DatabaseIfc db) =>
        db?.Context?.UnitsInContext?.ScaleSI(IfcUnitEnum.LENGTHUNIT) ?? 1.0;

    // Each IfcMaterialLayer -> seam MaterialLayer (MaterialId + SI MeasureValue thickness + layer name + the IFC per-row
    // Priority/Category/IsVentilated columns). The native-unit LayerThickness is coerced to SI metres by the model's
    // LengthScale and admitted through MeasureValue.OfSi (the SI-native seam entry stamping the Dimension.LengthDim
    // canonical symbol), never the raw record ctor that strands a mm value as a "metre" measure (the mm-vs-metre import
    // trap). An IfcMaterialLayerWithOffsets row folds through these columns alone: the subtype declares no public
    // constructor and keeps its offset vector on internal fields with no accessor, so the per-layer offsets are
    // unreachable at the GeometryGym public surface while the PROFILE subtype's OffsetValues is public and does fold.
    // Row Description is the one IFC annotation column the seam declines: it carries no analytical read, where Category
    // drives the assembly fold and Priority the junction resolution.
    static Fin<Seq<MaterialLayer>> LayersOf(IfcMaterialLayerSet set) {
        double lengthScale = LengthScale(set.Database);
        return set.MaterialLayers.AsIterable()
            .ToSeq()
            .TraverseM(layer => MeasureValue.OfSi(Dimension.LengthDim, layer.LayerThickness * lengthScale)
                .Map(thickness => new MaterialLayer(
                    MaterialId.Of(layer.Material?.Name ?? ""), thickness, layer.Name ?? "",
                    PriorityOf(layer.Priority), layer.Category ?? "", LogicalOf(layer.IsVentilated))))
            .As();
    }

    // PartName is the constituent's own IFC Name — the part it FORMS, a different axis from its function Category, so two
    // rows sharing one category stay addressable where the name-dropping fold collapsed them.
    static Seq<MaterialConstituent> ConstituentsOf(IfcMaterialConstituentSet set) =>
        set.MaterialConstituents.Values.AsIterable()
            .Map(static constituent => new MaterialConstituent(
                MaterialId.Of(constituent.Material?.Name ?? ""),
                constituent.Category ?? "",
                constituent.Fraction,
                constituent.Name ?? ""))
            .ToSeq();

    // Every IfcMaterialProfile row -> a seam MaterialProfile: its own material (the set name the fallback when a row
    // declares none), its OWN content-keyed ProfileRef (the row's full parametric section preserved in the store, so a
    // plate girder's web and flange plates each survive where the primary-only read kept one), the [0,100] junction
    // Priority, the function Category, and the reference-axis Offsets. Identity is Designation plus the kernel seed-zero
    // XxHash128 content key of the FULL IfcProfileDef STEP through the ONE kernel ContentHash entry the Model/
    // elements#REPRESENTATION_KEYS keyer also composes (NEVER the up-stratum Rasm.Compute InterchangeIdentity, the H7
    // named strata defect); Standard is left to the one-hop VividOrange catalog resolution [M7].
    static Fin<Seq<MaterialProfile>> ProfilesOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, Op key) {
        double lengthScale = LengthScale(set.Database);
        return set.MaterialProfiles.AsIterable()
            .ToSeq()
            .TraverseM(row => Optional(row.Profile)
                .ToFin(new BimFault.ModelRejected(key, $"material-profile-missing:{set.Name}:{row.Name}"))
                .Bind(profile => OffsetsOf(row, lengthScale)
                    .Map(offsets => new MaterialProfile(
                        MaterialId.Of(row.Material?.Name ?? set.Name ?? ""),
                        profiles.Preserve(profile, key),
                        PriorityOf(row.Priority), row.Category ?? "", offsets))))
            .As();
    }

    // CompositeOf preserves the set-level combined outline — set.CompositeProfile when the set declares its compound
    // geometry (plate girder, steel-concrete composite — the settable IfcCompositeProfileDef) — in the SAME
    // content-addressed store as the rows, so seam Composite is the one-hop section identity and row zero keeps its plate.
    static Option<ProfileRef> CompositeOf(IfcMaterialProfileSet set, IIfcProfileStore profiles, Op key) =>
        Optional(set.CompositeProfile).Map(composite => profiles.Preserve(composite, key));

    // IfcMaterialProfileWithOffsets publishes OffsetValues as a public double[] of arity one or two (start then optional
    // end) — the ONE per-row offset channel GeometryGym exposes, each entry a native-unit IfcLengthMeasure coerced by the
    // model LengthScale and admitted through the same MeasureValue.OfSi gate the layer thickness crosses. A base
    // IfcMaterialProfile yields the EMPTY vector, the IFC LIST[1:2] arity making empty-versus-present a bijection.
    static Fin<Seq<MeasureValue>> OffsetsOf(IfcMaterialProfile row, double lengthScale) =>
        row is IfcMaterialProfileWithOffsets offsets
            ? toSeq(offsets.OffsetValues).TraverseM(value => MeasureValue.OfSi(Dimension.LengthDim, value * lengthScale)).As()
            : Fin.Succ(Seq<MeasureValue>());

    // GeometryGym spells an unset priority as int.MinValue (its setter clamps anything outside the IFC [0,100] percentage
    // to that sentinel and its STEP writer emits `$`), so this read is the [SENTINEL_PROJECTION] site: the sentinel dies
    // here as None and never reaches the seam, the content hash, or the wire.
    static Option<int> PriorityOf(int priority) => priority == int.MinValue ? None : Some(priority);

    // LogicalOf narrows the three-state IfcLogical onto the Option<bool> Properties/property#PROPERTY_VALUE Logical already
    // ratifies (None = UNKNOWN): EN ISO 6946 drops a well-ventilated layer from the series-resistance fold, so an UNKNOWN
    // silently read as FALSE falsifies every U-value downstream — absence stays a refusal input, never a default.
    static Option<bool> LogicalOf(IfcLogicalEnum value) => value switch {
        IfcLogicalEnum.TRUE => Some(true),
        IfcLogicalEnum.FALSE => Some(false),
        _ => None,
    };

    // The content-keyed seam Material node from a built composition: mint the id from its own canonical bytes (id
    // excluded) so two structurally-identical materials dedup to one node; the node MaterialKey is the IFC set/
    // material name, the typed property sets are the authored lane's (empty at IFC ingest — the imported
    // HasProperties lane is seam-owned, recorded at the seam). The draft id is a discarded placeholder, and a failed composition admission threads through. A
    // class-root [Union] Node case has NO compiler-generated `with`, so the content id re-stamps through the seam
    // Graph/element#NODE_MODEL Node.Relabel (a `draft with { Id }` a class case cannot honour is the deleted form, the
    // SAME re-stamp the Rasm.Materials Mint takes).
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
    // edge, exactly the landing every element-level bag takes.
    public static Fin<Seq<PropertyBag>> ImportedPsets(
        IfcMaterialDefinition definition, Map<string, NodeId> rooted, UnitScale scale, TemplateScope templates, FidelityLog log, Op key) =>
        definition.HasProperties.AsIterable().ToSeq()
            .TraverseM(pset => pset.Properties.Values.AsIterable().ToSeq()
                .TraverseM(property => PropertyLowering.Lower(property, rooted, scale, log, key)
                    .Map(value => (Name: PropertyName.Create(property.Name ?? ""), Value: value)))
                .As()
                .Map(rows => new PropertyBag(
                    pset.Name ?? "",
                    rows.Fold(Map<PropertyName, PropertyValue>(), static (bag, row) => bag.AddOrUpdate(row.Name, row.Value)),
                    PropertyInheritance.ModeOf(pset.Name ?? "", typeBound: false, templates),
                    PropertySource.Import)))
            .As();

    // --- [EGRESS] -------------------------------------------------------------------------
    // The inverse half the Projection/egress#IFC_EGRESS Emit composes per seam Material node: author the
    // type-level MaterialComposition ONCE onto the GeometryGym material-definition family + the MaterialPropertySet
    // set onto IfcMaterialProperties Pset_Material* rows. This REPLACES the retired Rasm.Materials
    // MaterialAssignmentWire/MaterialPropertyWire egress — the material subgraph reads off the seam graph directly.
    // A ProfileSet's parametric IfcProfileDef reconstitutes one-hop from the content-addressed STEP store the
    // ProfileRef.ContentKey keys (the seam holds only the neutral ProfileRef); a store-missed Rasm-authored ProfileSet
    // authors from the carried profileSubtype token (the DetailSchema.Realization ProfileSubtype row the Emit resolves
    // off the graph), an unresolvable profile railing.
    public static Fin<IfcMaterialDefinition> AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles, Option<string> profileSubtype) =>
        Definition(db, material.Composition, material.MaterialKey, profiles, profileSubtype)
            .Map(definition => { material.Properties.Iter(set => AuthorPropertySet(db, definition, set)); return definition; });

    // The db-scoped material memo: ONE IfcMaterial per (db, name) is shared across every Definition arm AND every
    // AuthorComposition call, so a wall LayerSet and a slab LayerSet both naming "Concrete" author ONE IfcMaterial
    // entity, not one per layer/constituent/node (the duplicate-material bloat). Keyed by the emit DatabaseIfc so the
    // cache is emit-scoped and GC-collected with the database; the emit is db-serial (DatabaseIfc is single-threaded),
    // the dictionary guards reentry — the SAME memo shape the ClassificationSystem dictionary-source egress holds
    // (there keyed (system, edition); a material carries no edition axis, so (db, name) is the whole identity).
    static readonly ConditionalWeakTable<DatabaseIfc, ConcurrentDictionary<string, IfcMaterial>> Materials = new();

    static IfcMaterial MaterialOf(DatabaseIfc db, string name) =>
        Materials.GetValue(db, static _ => new ConcurrentDictionary<string, IfcMaterial>(StringComparer.Ordinal))
            .GetOrAdd(name, n => new IfcMaterial(db, n));

    // Every seam row re-authors its own IFC row and every per-row column re-stamps, so the ingress fold's columns are a
    // FIXED POINT of the round-trip rather than a one-way read: an unset Priority stays unwritten (the seam Option is None,
    // and assigning int.MinValue is the sentinel re-introduction this projection deletes), an UNKNOWN ventilation writes
    // IfcLogicalEnum.UNKNOWN, and a row carrying offsets authors the IfcMaterialProfileWithOffsets subtype through its
    // public one- or two-offset constructor. The seam Composite re-stamps the authored set's CompositeProfile (settable,
    // decompile-confirmed) so a re-ingest keys the SAME composite CompositeOf preferred.
    static Fin<IfcMaterialDefinition> Definition(DatabaseIfc db, MaterialComposition composition, MaterialId key, IIfcProfileStore profiles, Option<string> profileSubtype) {
        double lengthScale = LengthScale(db);   // SI metre -> target-model unit on egress (the inverse of the ingress coercion)
        return composition.Switch(
            single:        s => Fin.Succ<IfcMaterialDefinition>(MaterialOf(db, s.Material.Value)),
            layerSet:      s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialLayerSet(
                                    s.Layers.Map(l => Layer(db, l, lengthScale)), key.Value)),
            profileSet:    s => Rows(db, s, profiles, profileSubtype, lengthScale).Map(rows => AuthorProfileSet(key, rows, s, profiles)),
            // Row name is the seam PartName, falling back to the material key when the part is unnamed — exactly the
            // blank-name convention GeometryGym's own IfcMaterialLayer constructor applies, never an empty IFC Name.
            constituentSet: s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialConstituentSet(key.Value,
                                    s.Constituents.Map(c => new IfcMaterialConstituent(
                                        string.IsNullOrEmpty(c.PartName) ? c.Material.Value : c.PartName,
                                        MaterialOf(db, c.Material.Value)) { Fraction = c.Fraction, Category = c.Category }))));
    }

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
    // thickness, and name alone). Priority assigns only when present, so an unset column stays `$` in the STEP rather than
    // round-tripping as a fabricated zero the ingress would then read as a real junction precedence.
    static IfcMaterialLayer Layer(DatabaseIfc db, MaterialLayer layer, double lengthScale) {
        var row = new IfcMaterialLayer(MaterialOf(db, layer.Material.Value), layer.Thickness.Si / lengthScale, layer.LayerName) {
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
    static Fin<Seq<IfcMaterialProfile>> Rows(DatabaseIfc db, MaterialComposition.ProfileSet set, IIfcProfileStore profiles, Option<string> profileSubtype, double lengthScale) =>
        set.Profiles.TraverseM(row => profiles.Find(row.Profile)
                .Match(Some: Some, None: () => set.Profiles.Count == 1 ? AuthoredProfile(db, set, profileSubtype, lengthScale) : None)
                .ToFin(new BimFault.DanglingReference(Egress, $"material-profile-step-unresolved:{row.Profile.Designation}"))
                .Map(profile => Row(db, row, profile, lengthScale)))
            .As();

    static IfcMaterialProfile Row(DatabaseIfc db, MaterialProfile row, IfcProfileDef profile, double lengthScale) {
        IfcMaterial material = MaterialOf(db, row.Material.Value);
        string name = row.Profile.Designation;
        IfcMaterialProfile authored = row.Offsets.Map(static offset => offset.Si).ToArray() switch {
            [var start] => new IfcMaterialProfileWithOffsets(name, material, profile, start / lengthScale),
            [var start, var end] => new IfcMaterialProfileWithOffsets(name, material, profile, start / lengthScale, end / lengthScale),
            _ => new IfcMaterialProfile(name, material, profile),
        };
        authored.Category = row.Category;
        row.Priority.IfSome(priority => authored.Priority = priority);
        return authored;
    }

    // The carried-row profile author: a Rasm-authored ProfileSet preserves no STEP fragment, so the carried
    // DetailSchema.Realization ProfileSubtype token (the Materials occupancy derivation the Emit resolves off the
    // graph) names the authored entity — IfcRectangleProfileDef completes whole from the baked SectionProperties
    // (XDim the profile width, YDim the profile depth, SI -> target units); a token whose mandatory interior geometry
    // only a preserved fragment carries (IfcArbitraryProfileDefWithVoids inner curves — inline curve geometry never
    // rides the seam) resolves None and the lane keeps its typed fault, never a bare subtype with unassigned mandatory curves.
    static Option<IfcProfileDef> AuthoredProfile(DatabaseIfc db, MaterialComposition.ProfileSet s, Option<string> subtype, double lengthScale) =>
        subtype.Filter(static name => name == nameof(IfcRectangleProfileDef))
            .Bind(_ => s.Section.Map(section => (IfcProfileDef)new IfcRectangleProfileDef(
                db, s.Profile.Designation, section.Width.Si / lengthScale, section.Depth.Si / lengthScale)));

    // The per-occurrence usage [OCCURRENCE_USAGE_RULING]: a generated TOTAL Switch over the closed MaterialUsage union wraps the shared
    // definition in the IfcMaterialLayerSetUsage/ProfileSetUsage the Associate edge carries — a new usage arm breaks
    // this at compile time, never a runtime-silent _. A definition/usage modality mismatch faults at this boundary;
    // returning the bare definition would silently erase the occurrence binding. The neutral
    // LayerSetDirection/DirectionSense map to the GeometryGym enums inline (the inverse of the ingress Projection/
    // Projection/relations#RELATION_ALGEBRA UsageOf); the in-range CardinalPoint int casts to IfcCardinalPointReference. The
    // layer-usage ReferenceExtent has NO public GG write channel (4-arg ctor only, setter non-public — decompile-
    // confirmed): the seam LayerSet.ReferenceExtent is ingest-only here; the profile-usage setter is public and round-trips.
    public static Fin<IfcMaterialSelect> AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage) => usage.Switch(
        none:       _ => FinSucc((IfcMaterialSelect)definition),
        layerSet:   u => definition is IfcMaterialLayerSet set
            ? FinSucc<IfcMaterialSelect>(new IfcMaterialLayerSetUsage(set,
                u.Direction switch { LayerSetDirection.Axis1 => IfcLayerSetDirectionEnum.AXIS1, LayerSetDirection.Axis2 => IfcLayerSetDirectionEnum.AXIS2, _ => IfcLayerSetDirectionEnum.AXIS3 },
                u.Sense == DirectionSense.Positive ? IfcDirectionSenseEnum.POSITIVE : IfcDirectionSenseEnum.NEGATIVE,
                u.OffsetFromReferenceLine.Map(static value => value.Si / LengthScale(set.Database)).IfNone(double.NaN)))
            : FinFail<IfcMaterialSelect>(new BimFault.CodecReject(Egress, $"layer-usage-on:{definition.GetType().Name}")),
        profileSet: u => definition is IfcMaterialProfileSet set
            ? FinSucc<IfcMaterialSelect>(new IfcMaterialProfileSetUsage(set,
                (IfcCardinalPointReference)u.CardinalPoint.Map(static point => point.Key).IfNone((int)IfcCardinalPointReference.MID)) {
                    ReferenceExtent = u.ReferenceExtent.Map(static value => value.Si / LengthScale(set.Database)).IfNone(double.NaN),
                })
            : FinFail<IfcMaterialSelect>(new BimFault.CodecReject(Egress, $"profile-usage-on:{definition.GetType().Name}")));

    // The seam MaterialPropertySet -> its IFC material Pset (IfcMaterialProperties : IfcExtendedProperties named set on
    // the IfcMaterialDefinition): one TOTAL generated Switch over the closed ELEVEN-case discipline family (a new case
    // breaks it at compile, never a runtime-silent _), each column the typed IfcPropertySingleValue the (DatabaseIfc,
    // string, double|string|bool) ctor overload selects DIRECTLY from the value — no Num/Text/Flag rename layer. Standard
    // buildingSMART Psets carry their names (Pset_MaterialMechanical/Thermal/Optical, Pset_EnvironmentalImpactValues,
    // Pset_ConstructionCosts), the seam-native carriers a Rasm_Material* name (Orthotropic/Damping/Hygrothermal/Durability);
    // the FireRating reaction class and the Cost Currency/MeasurementBasis ride label columns (never a lossy double), the
    // Environmental case emits the FULL EN 15978 per-LifecycleStage GWP vector (A1-A3..D) one column per module (never an
    // aggregate that strands the seam StageGwp), and the Damping Rayleigh pair / Hygrothermal capillary A-value emit only
    // when present (the Option spread). Provenance rides the ONE shared base-Evidence column set every case appends via
    // WithEvidence — the prior per-case Environmental Epd/ValidUntilYear double-store and its includeValidUntilYear
    // suppression flag are DELETED with the seam's collapse of provenance onto PropertyEvidence.
    static void AuthorPropertySet(DatabaseIfc db, IfcMaterialDefinition material, MaterialPropertySet set) => set.Switch(
        mechanical:    m => Pset(material, "Pset_MaterialMechanical", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "MassDensity", m.Density.Si), new IfcPropertySingleValue(db, "YoungModulus", m.YoungsModulus.Si), new IfcPropertySingleValue(db, "ShearModulus", m.ShearModulus.Si),
                                new IfcPropertySingleValue(db, "YieldStress", m.YieldStrength.Si), new IfcPropertySingleValue(db, "UltimateStress", m.UltimateStrength.Si),
                                new IfcPropertySingleValue(db, "PoissonRatio", m.PoissonsRatio), new IfcPropertySingleValue(db, "ThermalExpansionCoefficient", m.ThermalExpansionPerK))),
        orthotropic:   o => Pset(material, "Rasm_MaterialOrthotropic", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "MassDensity", o.Density.Si), new IfcPropertySingleValue(db, "E1Parallel", o.E1Parallel.Si),
                                new IfcPropertySingleValue(db, "E2Perpendicular", o.E2Perpendicular.Si), new IfcPropertySingleValue(db, "ShearModulus", o.ShearModulus.Si),
                                new IfcPropertySingleValue(db, "Strength1Parallel", o.Strength1Parallel.Si), new IfcPropertySingleValue(db, "Strength2Perpendicular", o.Strength2Perpendicular.Si),
                                new IfcPropertySingleValue(db, "ThermalExpansionCoefficient", o.ThermalExpansionPerK))),
        thermal:       t => Pset(material, "Pset_MaterialThermal", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "ThermalConductivity", t.Conductivity.Si), new IfcPropertySingleValue(db, "SpecificHeatCapacity", t.SpecificHeat.Si),
                                new IfcPropertySingleValue(db, "ThermalTransmittance", t.UValue.Si), new IfcPropertySingleValue(db, "VapourDiffusionResistance", t.VapourResistanceFactor))),
        acoustic:      a => Pset(material, "Pset_MaterialAcoustic", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "NoiseReductionCoefficient", a.Nrc), new IfcPropertySingleValue(db, "SoundAbsorptionAverage", a.Saa), new IfcPropertySingleValue(db, "SoundTransmissionClass", a.StcWeighted))),
        fire:          f => Pset(material, "Pset_MaterialFire", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "ReactionToFireClass", f.Reaction.Key), new IfcPropertySingleValue(db, "Combustible", f.Reaction.Combustible), new IfcPropertySingleValue(db, "SmokeProduction", f.Smoke.Key),
                                new IfcPropertySingleValue(db, "FlamingDroplets", f.Droplets.Key), new IfcPropertySingleValue(db, "FireResistanceR", f.Resistance.LoadBearingMinutes),
                                new IfcPropertySingleValue(db, "FireResistanceE", f.Resistance.IntegrityMinutes), new IfcPropertySingleValue(db, "FireResistanceI", f.Resistance.InsulationMinutes))),
        environmental: e => Pset(material, "Pset_EnvironmentalImpactValues", WithEvidence(db, set, EnvironmentalColumns(db, e))),
        cost:          c => Pset(material, "Pset_ConstructionCosts", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "Currency", c.Currency.Value), new IfcPropertySingleValue(db, "MeasurementBasis", c.Basis.Key),
                                new IfcPropertySingleValue(db, "SupplyCost", c.SupplyPerUnit), new IfcPropertySingleValue(db, "InstallationCost", c.InstallPerUnit), new IfcPropertySingleValue(db, "LifeCycleCost", c.LifecyclePerUnit))),
        damping:       d => Pset(material, "Rasm_MaterialDamping", WithEvidence(db, set,
                                [new IfcPropertySingleValue(db, "DampingRatio", d.DampingRatio), new IfcPropertySingleValue(db, "StructuralLossFactor", d.StructuralLossFactor),
                                 .. d.Rayleigh.Match(
                                     Some: r => Seq((IfcProperty)new IfcPropertySingleValue(db, "RayleighAlpha", r.AlphaPerS), new IfcPropertySingleValue(db, "RayleighBeta", r.BetaS)),
                                     None: static () => Seq<IfcProperty>())])),
        hygrothermal:  h => Pset(material, "Rasm_MaterialHygrothermal", WithEvidence(db, set,
                                [new IfcPropertySingleValue(db, "Porosity", h.Porosity), new IfcPropertySingleValue(db, "WaterContent80RH", h.WaterContent80Rh.Si), new IfcPropertySingleValue(db, "FreeWaterSaturation", h.FreeWaterSaturation.Si),
                                 .. h.WaterAbsorptionKgPerM2SqrtS.Match(
                                     Some: a => Seq((IfcProperty)new IfcPropertySingleValue(db, "WaterAbsorptionCoefficient", a)),
                                     None: static () => Seq<IfcProperty>())])),
        durability:    u => Pset(material, "Rasm_MaterialDurability", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "CarbonationRate", u.CarbonationRateMmPerSqrtYear), new IfcPropertySingleValue(db, "ChlorideMigrationCoefficient", u.ChlorideDiffusion.Si),
                                new IfcPropertySingleValue(db, "AgeingExponent", u.AgeingExponent))),
        optical:       o => Pset(material, "Pset_MaterialOptical", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "VisibleTransmittance", o.VisibleTransmittance), new IfcPropertySingleValue(db, "VisibleReflectanceFront", o.VisibleReflectanceFront), new IfcPropertySingleValue(db, "VisibleReflectanceBack", o.VisibleReflectanceBack),
                                new IfcPropertySingleValue(db, "SolarTransmittance", o.SolarTransmittance), new IfcPropertySingleValue(db, "SolarReflectanceFront", o.SolarReflectanceFront), new IfcPropertySingleValue(db, "SolarReflectanceBack", o.SolarReflectanceBack),
                                new IfcPropertySingleValue(db, "ThermalIrTransmittance", o.ThermalIrTransmittance), new IfcPropertySingleValue(db, "ThermalIrEmissivityFront", o.ThermalIrEmissivityFront), new IfcPropertySingleValue(db, "ThermalIrEmissivityBack", o.ThermalIrEmissivityBack))));

    // The ONE evidence-appending wrap every Pset arm composes: the typed discipline columns then the shared base-Evidence
    // provenance columns. NO per-case suppression overload — provenance is single-stored on the base PropertyEvidence, so
    // Environmental appends the SAME columns as every other case (the deleted includeValidUntilYear flag dodged a
    // double-store the seam no longer carries).
    static IfcProperty[] WithEvidence(DatabaseIfc db, MaterialPropertySet set, params IfcProperty[] columns) =>
        [.. columns, .. EvidenceColumns(db, set)];

    // The Environmental typed columns: the FULL EN 15978 per-LifecycleStage GwpTotal vector (one column per module off the
    // seam StageAt row) plus the two EN 15804 resource fractions. EPD identity + expiry ride the base Evidence (Source
    // "epd", Reference the registration number, ValidUntil the LocalDate) emitted by EvidenceColumns — never a per-case
    // EnvironmentalProductDeclaration/DataValidUntilYear column (the deleted double-store the seam collapsed onto Evidence).
    static IfcProperty[] EnvironmentalColumns(DatabaseIfc db, MaterialPropertySet.Environmental e) =>
        [.. LifecycleStage.Items.AsIterable().Map(s => (IfcProperty)new IfcPropertySingleValue(db, $"GlobalWarmingPotential_{s.Module}", e.StageAt(s))),
         new IfcPropertySingleValue(db, "RecycledContent", e.RecycledContent),
         new IfcPropertySingleValue(db, "EndOfLifeRecovery", e.EndOfLifeRecovery)];

    // The shared base-Evidence columns every Pset arm appends: the provenance Source + Reference always, and the expiry
    // ONLY when present — the seam PropertyEvidence.ValidUntil is an Option<LocalDate> (the exact EC3 declaration expiry,
    // never the deleted lossy int YEAR), lowered to the ISO-8601 DataValidUntil label so the full date round-trips intact.
    static Seq<IfcProperty> EvidenceColumns(DatabaseIfc db, MaterialPropertySet set) =>
        Seq(
            (IfcProperty)new IfcPropertySingleValue(db, "DataSource", set.Evidence.Source),
            new IfcPropertySingleValue(db, "DataReference", set.Evidence.Reference))
        + set.Evidence.ValidUntil.Match(
            Some: d => Seq((IfcProperty)new IfcPropertySingleValue(db, "DataValidUntil", LocalDatePattern.Iso.Format(d))),
            None: static () => Seq<IfcProperty>());

    // IfcMaterialProperties(string name, IfcMaterialDefinition mat) named Pset (the material already carries its db, so
    // none is threaded here); each typed column is an IfcPropertySingleValue keyed by its own Name on the inherited
    // Dictionary<string, IfcProperty> Properties. The (DatabaseIfc, string, double|string|bool) column ctors are decompile-confirmed.
    static void Pset(IfcMaterialDefinition material, string name, params IfcProperty[] columns) {
        var pset = new IfcMaterialProperties(name, material);
        columns.Iter(p => pset.Properties[p.Name] = p);
    }
}
```

## [03]-[RESEARCH]

(none)
