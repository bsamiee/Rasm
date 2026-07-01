# [BIM_MATERIAL_COMPOSITION]

The IFC material PROJECTOR lowering the live GeometryGym `IfcMaterialSelect` surface onto the `Rasm.Element` seam `Material` node: `MaterialProjection.Project` discriminates the relating-material runtime entity — `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` — and folds it into one content-keyed seam `Node.Material` carrying the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`). The seam OWNS the construction-material algebra (`MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `ProfileRef`, `MaterialPropertySet`, `MaterialId`); this page owns ONLY the GeometryGym discrimination that fills it, never re-declaring a Bim `BimMaterial`/`BimMaterialComposition` — the retired `BimMaterial` record and the `BimElement.Materials` column are GONE, a material is a seam `Material` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, and the consumer reads `element.Materials` flat on the baked element rather than a second stored record keyed by `MaterialId`. The occurrence usage binding (layer direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the `Associate` edge `MaterialUsage` payload the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authors [C7], this owner producing only the type-level SET structure so a wall and its mirror share one `LayerSet` with two `Associate` usages. A linear member's section is a neutral `ProfileRef` (`Standard` + `Designation` + content key) the `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog [M7], its full `IfcProfileDef` parametric definition preserved in the content-addressed store the `ContentKey` keys — the page references NO VividOrange section type and folds NO parametric dimension onto the seam, because the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the profile geometry by content-hash reference, never a RhinoCommon type. An unresolvable material-select entity rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` lifted BARE (band 2600 IS the `Expected` `Code`; no `.ToError()` hop); a degenerate composition (empty set, non-positive layer thickness, unnormalized constituent fractions) rails the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` the seam `MaterialComposition` admission owns. The projector is BIDIRECTIONAL: `MaterialProjection.AuthorComposition`/`AuthorUsage` is the inverse half the `Projection/egress#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authors the type-level `MaterialComposition` back onto the GeometryGym material-definition family (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial`) ONCE per material and lowers the seam `MaterialPropertySet` set onto TYPED `IfcMaterialProperties` `Pset_Material*` rows (every case field round-trips — the `FireRating` class, the `Cost` `Currency`/`MeasurementBasis`, and the EPD id as typed label columns the value-typed `IfcPropertySingleValue` ctor overload selects directly, never coerced to a lossy double, and the `Environmental` case emits its FULL EN 15978 per-`LifecycleStage` GWP vector one column per module, never a single aggregate sum), and `AuthorUsage` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Relations/relation#EDGE_ALGEBRA` `Associate` edge `MaterialUsage` carries [C7]. This is the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — `Rasm.Bim` reads the projected `Material` subgraph directly, never a Materials wire. A `ProfileSet`'s full parametric `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the seam holds only the neutral `ProfileRef` + baked `SectionProperties`), an unresolved profile railing `BimFault.DanglingReference`.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: `MaterialProjection.Project` the `IfcMaterialSelect`→seam `Node.Material` ingress fold, the per-modality `LayerSet`/`ProfileSet`/`ConstituentSet`/`Single` mapping onto the seam `MaterialComposition`, the `ProfileRef` content-keyer over the `IfcProfileDef` STEP, the `LayersOf`/`ConstituentsOf` row folds, the content-keyed `Mint` of the seam `Node.Material`, AND the inverse `MaterialProjection.AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node onto the GeometryGym material-definition family + the `MaterialPropertySet`→`IfcMaterialProperties` `Pset_Material*` rows + the `Associate`-edge `MaterialUsage`→`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [C7].

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialProjection` the static BIDIRECTIONAL GeometryGym↔seam material projector — the `Project` ingress folding one `IfcMaterialSelect` runtime entity into one seam `Node.Material` (discriminating the entity, building the seam `MaterialComposition` through the seam smart-constructors, minting the content-keyed node id), and the `AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node back onto the GeometryGym material-definition family the `Projection/egress#IFC_EGRESS` `Emit` composes. The seam owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent` rows, the `ProfileRef`, the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage`, and the `MaterialPropertySet` engineering-property family; this page declares NONE of them — it composes the seam vocabulary, mapping the GeometryGym material-assembly entities onto it and back.
- Entry: `MaterialProjection.Project(BaseClassIfc relatingMaterial, double tolerance, Op key)` is the live-entity promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes when folding an `IfcRelAssociatesMaterial.RelatingMaterial` — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` and `IfcMaterialProfileSetUsage` its `ForProfileSet`, the usage payload riding the `Associate` edge not this node; `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` its primary `MaterialProfile`, `IfcMaterialConstituentSet` its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and returns one content-keyed seam `Node.Material`; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) and the seam `MaterialComposition` admission aborts a degenerate set (`ElementFault.ValueRejected`), each lifting BARE (the band IS the `Expected` `Code`; no `.ToError()` hop). `MaterialProjection.AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles)` is the egress entry the `Emit` composes — it authors the type-level `MaterialComposition` ONCE (`Single`→`IfcMaterial`, `LayerSet`→`IfcMaterialLayerSet`, `ProfileSet`→`IfcMaterialProfileSet`, `ConstituentSet`→`IfcMaterialConstituentSet`), folds the seam `MaterialPropertySet` set onto the `IfcMaterialProperties` named Psets as TYPED columns (each case field a `MeasureValue`/label/boolean plus row evidence where available, never a lossy double), and reconstitutes a `ProfileSet`'s `IfcProfileDef` from the injected `profiles` store (`Fin<T>` aborting `BimFault.DanglingReference` keyed on the page `Egress` gate on an unresolved profile); `MaterialProjection.AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage)` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge carries [C7], returning the bare definition for `MaterialUsage.None`.
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and builds the seam `MaterialComposition` through the seam `Of`-prefixed smart-constructors (the `Fin`-railing `MaterialComposition.OfLayerSet`/`OfConstituentSet` owning the empty-set / non-positive-thickness / unnormalized-fraction admission, the total `OfSingle`/`OfProfileSet` lifted into `Fin` for the `Mint` fold), then mints the seam `Node.Material` whose id is the kernel seed-zero `XxHash128` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical materials dedup to one node; `LayersOf` folds each `IfcMaterialLayer` onto a seam `MaterialLayer` carrying its `MaterialId`, a `MeasureValue` thickness over `Dimension.LengthDim` coerced to SI metres by the model's `IfcUnitAssignment.ScaleSI(LENGTHUNIT)` factor (the native `LayerThickness` is mm in most Revit/ArchiCAD exports, never pre-SI) and admitted through `MeasureValue.OfSi` so the seam carries the SI scalar `MeasureValue.Of` otherwise mandates, and its layer name; `ConstituentsOf` folds each `IfcMaterialConstituent` (read through the `Dictionary.Values`) onto a seam `MaterialConstituent` carrying its `MaterialId`, category, and `Fraction`; `ProfileRefOf` projects the primary `IfcMaterialProfile.Profile` onto a neutral `ProfileRef` whose `ContentKey` is the kernel seed-zero `XxHash128` `ContentHash.Of` over the tag-namespaced `IfcProfileDef` STEP (the full parametric section preserved in the content-addressed store; the ONE kernel hasher the `Model/elements#REPRESENTATION_KEYS` keyer also composes, never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]), the `Designation` the profile name, the `Standard` left to the one-hop catalog resolution; the engineering property sets arrive via the `Rasm.Materials` projector and the `Semantics/properties#PROPERTY_TEMPLATES` Pset round-trip, so the IFC-ingest `Node.Material` carries an empty `Seq<MaterialPropertySet>`.
- Receipt: the seam `Node.Material` is the material evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Associate` edge into `element.Materials` (a `BakedMaterial` carrying the node plus its occurrence `MaterialUsage` — the seam Bake-folded accessor, DISTINCT from the `Rasm.Materials` projection-input `MaterialBinding` and the type→occurrence `TypeBinding`), the `Model/query#ELEMENT_SET` material predicate matches by `MaterialId` or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#BASE_QUANTITIES` layered-volume takeoff reads from the `LayerSet` thicknesses; the layer build-up, the section material, and the constituent mix each carry their real composition on one seam node, never a parallel layer/profile/constituent record family.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new material-assembly modality is one seam `MaterialComposition` union arm (the seam's, not this page's) plus one `Project` switch arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the seam `MaterialLayer`/`MaterialConstituent`; a new section catalog is one `ProfileRef.Standard` token the `Rasm.Materials` projector resolves, never a seam edit; a new emitted material property is one typed `IfcPropertySingleValue` column the value-typed ctor overload selects on the matching `AuthorPropertySet` arm (a new EN 15978 module is one `LifecycleStage` row the environmental fold iterates, never a hand-added column), never a per-property egress branch and never a re-widened double-only `Pset`; never a per-element-class material type, never a Bim `BimMaterial` record beside the seam node, and never a parallel material store.
- Boundary: the material model is the seam `Node.Material` + `MaterialComposition` and a Bim `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile`/`LayerSetUsage`/`ProfileSetUsage`/`ProfileDefKind`/`ProfileDims` re-declaration is the deleted form — the seam owns the algebra, this page owns only the GeometryGym discrimination that fills it; the retired `BimMaterial` record, the `BimElement.Materials` typed column, and the `BimModel.Project` material fold are GONE, a material being a seam node the `Bake` fold reads; the occurrence usage rides the `Associate` edge `MaterialUsage` payload [C7] and threading `LayerSetUsage`/`ProfileSetUsage` onto this composition node is the named seam violation — the type-level SET structure is shared, the per-occurrence geometric binding is the edge's; the `ProfileSet` arm carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section type and NOT inline `IfcParameterizedProfileDef` dimensions — the full parametric section is preserved in the content-addressed store the `ContentKey` keys and the canonical section properties resolve one-hop to the catalog above the seam, so a profile-name-only `ProfileRef` that drops the content key is the deleted form, and the content key is the kernel seed-zero `XxHash128` `ContentHash.Of` (the up-stratum `Rasm.Compute` `InterchangeIdentity` being the H7 strata defect); the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `MaterialLayer` thickness coerces the NATIVE-unit `LayerThickness` to SI metres through the model's `ScaleSI(LENGTHUNIT)` factor and admits through `MeasureValue.OfSi` over `Dimension.LengthDim` (the inverse `/ScaleSI` applied on egress for a non-SI target model) — a bare double, the raw `MeasureValue` ctor that bypasses the owner's SI admission, OR treating the native length as already-SI (the mm-vs-metre import trap a Revit export springs) is the named defect; the section geometry binds by content-hash reference and a RhinoCommon profile field or an in-process BRep evaluation is the named seam violation; an unresolvable material-select entity lifts `Model/faults#FAULT_BAND` `BimFault.ModelRejected` BARE (band 2600 IS the `Expected` `Code`, the ingress on `ctx.Key` and the egress on the page `Egress` gate) and the seam `MaterialComposition` admission lifts `ElementFault.ValueRejected` BARE on a degenerate set, a `.ToError()` lowering hop (or a hand-built `Error.New(2600, …)`) bypassing the typed case being the named seam defect; the EGRESS reads the seam `Material` node + the `Associate` edge `MaterialUsage` ONLY — a Materials `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into this owner is the deleted form (those Materials wires are retired, the material egress reading the projected seam subgraph), the type-level composition authored ONCE and the per-occurrence usage wrapping it so a wall and its mirror share one `IfcMaterialLayerSet` with two `IfcMaterialLayerSetUsage` instances, the `IfcMaterialProperties` Pset attaching to the authored `IfcMaterialDefinition` and the `ProfileSet` `IfcProfileDef` reconstituting one-hop from the content-addressed STEP (a parametric dimension re-folded onto the seam being the deleted form).

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
// The one GeometryGym->seam material lowering: IfcMaterialSelect -> seam Node.Material carrying the seam
// MaterialComposition. The seam OWNS the algebra (MaterialComposition/MaterialLayer/MaterialConstituent/
// ProfileRef); this projector only discriminates the IFC entity and fills it. The occurrence usage (layer
// direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the Associate edge
// MaterialUsage payload the Projection/semantic EdgeProjection authors [C7]. The section is a neutral
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
    public static Fin<Node.Material> Project(BaseClassIfc relatingMaterial, double tolerance, Op key) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage u    => Optional(u.ForLayerSet).ToFin(new BimFault.ModelRejected(key, "material-layer-set-usage-unbound")).Bind(set => LayerSetOf(set, tolerance, key)),
            IfcMaterialProfileSetUsage u  => Optional(u.ForProfileSet).ToFin(new BimFault.ModelRejected(key, "material-profile-set-usage-unbound")).Bind(set => ProfileSetOf(set, tolerance, key)),
            IfcMaterialLayerSet set       => LayerSetOf(set, tolerance, key),
            IfcMaterialProfileSet set     => ProfileSetOf(set, tolerance, key),
            IfcMaterialConstituentSet set => Mint(set.Name ?? "", tolerance, MaterialComposition.OfConstituentSet(ConstituentsOf(set), key)),
            IfcMaterial material          => Mint(material.Name ?? "", tolerance, Fin.Succ<MaterialComposition>(MaterialComposition.OfSingle(MaterialId.Of(material.Name ?? "")))),
            _                             => Fin.Fail<Node.Material>(new BimFault.ModelRejected(key, $"material-select-unresolved:{relatingMaterial.GetType().Name}")),
        };

    // The two SET modalities each reachable from a bare set AND its occurrence-usage wrapper (so the usage arm and
    // the set arm share one builder): the LayerSet folds its rows, the ProfileSet reads its primary profile and
    // content-keys the section. The node MaterialKey is the IFC set name; the composition carries its own materials.
    static Fin<Node.Material> LayerSetOf(IfcMaterialLayerSet set, double tolerance, Op key) =>
        Mint(set.Name ?? "", tolerance, MaterialComposition.OfLayerSet(LayersOf(set), key));

    static Fin<Node.Material> ProfileSetOf(IfcMaterialProfileSet set, double tolerance, Op key) =>
        set.MaterialProfiles.AsIterable().Head
            .ToFin(new BimFault.ModelRejected(key, $"material-profile-set-empty:{set.Name}"))
            .Bind(profile => Mint(set.Name ?? "", tolerance,
                Fin.Succ<MaterialComposition>(MaterialComposition.OfProfileSet(MaterialId.Of(profile.Material?.Name ?? set.Name ?? ""), ProfileRefOf(profile)))));

    // The model's length-unit -> SI-metre factor (ScaleSI returns 1.0 when no length unit is declared, so a unitless or
    // SI model needs no branch). GeometryGym stores IfcMaterialLayer.LayerThickness in the model's NATIVE units (mm in
    // most Revit/ArchiCAD exports), never pre-coerced — so every length crossing into the SI-canonical seam MeasureValue
    // multiplies by this factor on ingress and divides by the target model's factor on egress, the ONE coercion the seam
    // Properties/quantity#MEASURE_VALUE owner mandates "once at admission". Two callers (LayersOf + Definition), not thin.
    static double LengthScale(DatabaseIfc db) =>
        db?.Context?.UnitsInContext?.ScaleSI(IfcUnitEnum.LENGTHUNIT) ?? 1.0;

    // Each IfcMaterialLayer -> seam MaterialLayer (MaterialId + SI MeasureValue thickness + layer name). The native-unit
    // LayerThickness is coerced to SI metres by the model's LengthScale and admitted through MeasureValue.OfSi (the
    // SI-native seam entry stamping the Dimension.LengthDim canonical symbol), never the raw record ctor that strands
    // a mm value as a "metre" measure (the mm-vs-metre import trap).
    static Seq<MaterialLayer> LayersOf(IfcMaterialLayerSet set) {
        double lengthScale = LengthScale(set.Database);
        return set.MaterialLayers.AsIterable()
            .Map(layer => new MaterialLayer(
                MaterialId.Of(layer.Material?.Name ?? ""),
                MeasureValue.OfSi(Dimension.LengthDim, layer.LayerThickness * lengthScale),
                layer.Name ?? ""))
            .ToSeq();
    }

    static Seq<MaterialConstituent> ConstituentsOf(IfcMaterialConstituentSet set) =>
        set.MaterialConstituents.Values.AsIterable()
            .Map(static constituent => new MaterialConstituent(
                MaterialId.Of(constituent.Material?.Name ?? ""),
                constituent.Category ?? "",
                constituent.Fraction))
            .ToSeq();

    // The neutral profile reference: identity (Standard left to the one-hop catalog resolution, Designation the
    // profile name) plus the kernel seed-zero XxHash128 content key of the FULL IfcProfileDef STEP through the ONE
    // kernel ContentHash entry the Model/elements#REPRESENTATION_KEYS keyer also composes (NEVER the up-stratum
    // Rasm.Compute InterchangeIdentity, the H7 named strata defect), so the parametric section is recoverable from
    // the content-addressed store and a standard section resolves one-hop to the VividOrange catalog [M7].
    static ProfileRef ProfileRefOf(IfcMaterialProfile profile) =>
        new("", profile.Profile?.ProfileName ?? "",
            ContentHash.Of(System.Text.Encoding.UTF8.GetBytes(string.Concat("ifc-profile ", profile.Profile?.StringSTEP() ?? ""))));

    // The content-keyed seam Material node from a built composition: mint the id from its own canonical bytes (id
    // excluded) so two structurally-identical materials dedup to one node; the node MaterialKey is the IFC set/
    // material name, the engineering property sets arrive via the Rasm.Materials projector / Pset round-trip (empty
    // at IFC ingest). The draft id is a discarded placeholder, and a failed composition admission threads through. A
    // class-root [Union] Node case has NO compiler-generated `with`, so the content id re-stamps through the seam
    // Graph/element#NODE_MODEL Node.Relabel (a `draft with { Id }` a class case cannot honour is the deleted form, the
    // SAME re-stamp the Rasm.Materials Mint takes).
    static Fin<Node.Material> Mint(string name, double tolerance, Fin<MaterialComposition> composition) =>
        composition.Map(c => {
            var draft = new Node.Material(NodeId.Content(default), MaterialId.Of(name), c, Seq<MaterialPropertySet>());
            return (Node.Material)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
        });

    // --- [EGRESS] -------------------------------------------------------------------------
    // The inverse half the Projection/egress#IFC_EGRESS Emit composes per seam Material node: author the
    // type-level MaterialComposition ONCE onto the GeometryGym material-definition family + the MaterialPropertySet
    // set onto IfcMaterialProperties Pset_Material* rows. This REPLACES the retired Rasm.Materials
    // MaterialAssignmentWire/MaterialPropertyWire egress — the material subgraph reads off the seam graph directly.
    // A ProfileSet's parametric IfcProfileDef reconstitutes one-hop from the content-addressed STEP store the
    // ProfileRef.ContentKey keys (the seam holds only the neutral ProfileRef), an unresolved profile railing.
    public static Fin<IfcMaterialDefinition> AuthorComposition(DatabaseIfc db, Node.Material material, IIfcProfileStore profiles) =>
        Definition(db, material.Composition, material.MaterialKey, profiles)
            .Map(definition => { material.Properties.Iter(set => AuthorPropertySet(db, definition, set)); return definition; });

    static Fin<IfcMaterialDefinition> Definition(DatabaseIfc db, MaterialComposition composition, MaterialId key, IIfcProfileStore profiles) {
        double lengthScale = LengthScale(db);   // SI metre -> target-model unit on egress (the inverse of the ingress coercion)
        return composition.Switch(
            single:        s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterial(db, s.Material.Value)),
            layerSet:      s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialLayerSet(
                                    s.Layers.Map(l => new IfcMaterialLayer(new IfcMaterial(db, l.Material.Value), l.Thickness.Si / lengthScale, l.LayerName)), key.Value)),
            profileSet:    s => profiles.Find(s.Profile).Match(
                                    Some: profile => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialProfileSet(key.Value,
                                                        new IfcMaterialProfile(s.Profile.Designation, new IfcMaterial(db, s.Material.Value), profile))),
                                    None: () => Fin.Fail<IfcMaterialDefinition>(new BimFault.DanglingReference(Egress, $"material-profile-step-unresolved:{s.Profile.Designation}"))),
            constituentSet: s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialConstituentSet(key.Value,
                                    s.Constituents.Map(c => new IfcMaterialConstituent(c.Material.Value, new IfcMaterial(db, c.Material.Value)) { Fraction = c.Fraction, Category = c.Category }))));
    }

    // The per-occurrence usage [C7]: a generated TOTAL Switch over the closed MaterialUsage union wraps the shared
    // definition in the IfcMaterialLayerSetUsage/ProfileSetUsage the Associate edge carries — a new usage arm breaks
    // this at compile time, never a runtime-silent _ — returning the bare definition for None or a definition/usage kind
    // mismatch (impossible by construction: a LayerSet usage always pairs a LayerSet composition). The neutral
    // LayerSetDirection/DirectionSense map to the GeometryGym enums inline (the inverse of the ingress Projection/
    // semantic#RELATION_ALGEBRA UsageOf); the in-range CardinalPoint int casts to IfcCardinalPointReference.
    public static IfcMaterialSelect AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage) => usage.Switch(
        none:       _ => (IfcMaterialSelect)definition,
        layerSet:   u => definition is IfcMaterialLayerSet ls
                           ? new IfcMaterialLayerSetUsage(ls,
                                 u.Direction switch { LayerSetDirection.Axis1 => IfcLayerSetDirectionEnum.AXIS1, LayerSetDirection.Axis2 => IfcLayerSetDirectionEnum.AXIS2, _ => IfcLayerSetDirectionEnum.AXIS3 },
                                 u.Sense == DirectionSense.Positive ? IfcDirectionSenseEnum.POSITIVE : IfcDirectionSenseEnum.NEGATIVE,
                                 u.OffsetFromReferenceLine)
                           : (IfcMaterialSelect)definition,
        profileSet: u => definition is IfcMaterialProfileSet ps
                           ? new IfcMaterialProfileSetUsage(ps, (IfcCardinalPointReference)u.CardinalPoint.Key) { ReferenceExtent = u.ReferenceExtent }
                           : (IfcMaterialSelect)definition);

    // The seam MaterialPropertySet -> its standard IFC material Pset (IfcMaterialProperties : IfcExtendedProperties
    // named set on the IfcMaterialDefinition): one Switch over the closed discipline family, each column the typed
    // IfcPropertySingleValue the (DatabaseIfc, string, double|string|bool) ctor overload selects DIRECTLY from the value
    // (the package surface IS the typed-column dispatch — a measure double, a label string, a boolean flag — so no
    // Num/Text/Flag rename layer), the WHOLE case round-tripping: the FireRating reaction class, the Cost Currency/
    // MeasurementBasis, and the EPD id ride label columns (never coerced to a lossy double), and the Environmental case
    // emits the FULL EN 15978 per-LifecycleStage GWP vector (A1-A3..D), one column per module, never a single aggregate
    // sum that strands the per-stage breakdown the seam StageGwp carries — the rich life-cycle vector round-trips intact.
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
                                new IfcPropertySingleValue(db, "ReactionToFireClass", f.Reaction.Key), new IfcPropertySingleValue(db, "SmokeProduction", f.Smoke.Key),
                                new IfcPropertySingleValue(db, "FlamingDroplets", f.Droplets.Key), new IfcPropertySingleValue(db, "FireResistanceR", f.Resistance.LoadBearingMinutes),
                                new IfcPropertySingleValue(db, "FireResistanceE", f.Resistance.IntegrityMinutes), new IfcPropertySingleValue(db, "FireResistanceI", f.Resistance.InsulationMinutes))),
        environmental: e => Pset(material, "Pset_EnvironmentalImpactValues", WithEvidence(db, set, false, EnvironmentalColumns(db, e))),
        cost:          c => Pset(material, "Pset_ConstructionCosts", WithEvidence(db, set,
                                new IfcPropertySingleValue(db, "Currency", c.Currency.Value), new IfcPropertySingleValue(db, "MeasurementBasis", c.Basis.Key),
                                new IfcPropertySingleValue(db, "SupplyCost", c.SupplyPerUnit), new IfcPropertySingleValue(db, "InstallationCost", c.InstallPerUnit), new IfcPropertySingleValue(db, "LifeCycleCost", c.LifecyclePerUnit))));

    static IfcProperty[] WithEvidence(DatabaseIfc db, MaterialPropertySet set, params IfcProperty[] columns) =>
        WithEvidence(db, set, includeValidUntilYear: true, columns);

    static IfcProperty[] WithEvidence(DatabaseIfc db, MaterialPropertySet set, bool includeValidUntilYear, params IfcProperty[] columns) =>
        [.. columns, .. EvidenceColumns(db, set, includeValidUntilYear)];

    static IfcProperty[] EnvironmentalColumns(DatabaseIfc db, MaterialPropertySet.Environmental e) =>
        [.. LifecycleStage.Items.AsIterable().Map(s => (IfcProperty)new IfcPropertySingleValue(db, $"GlobalWarmingPotential_{s.Module}", e.StageAt(s))),
         new IfcPropertySingleValue(db, "RecycledContent", e.RecycledContent),
         new IfcPropertySingleValue(db, "EndOfLifeRecovery", e.EndOfLifeRecovery),
         new IfcPropertySingleValue(db, "DataValidUntilYear", e.ValidUntilYear),
         new IfcPropertySingleValue(db, "EnvironmentalProductDeclaration", e.Epd)];

    static Seq<IfcProperty> EvidenceColumns(DatabaseIfc db, MaterialPropertySet set, bool includeValidUntilYear) =>
        Seq(
            (IfcProperty)new IfcPropertySingleValue(db, "DataSource", set.Evidence.Source),
            new IfcPropertySingleValue(db, "DataReference", set.Evidence.Reference))
        + (includeValidUntilYear ? set.Evidence.ValidUntilYear : Option<int>.None).Match(
            Some: y => Seq((IfcProperty)new IfcPropertySingleValue(db, "DataValidUntilYear", y)),
            None: () => Seq<IfcProperty>());

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

- [MATERIAL_SELECT_DISPATCH]: the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` runtime-type discrimination — the `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` cases the `Project` switch folds — grounds against the GeometryGym `IfcMaterialSelect` member surface (`.api/api-geometrygym-ifc` material families) so the fold discriminates the real material-assembly entity rather than a guessed shape; the `IfcMaterialLayerSet.MaterialLayers` (`LIST<IfcMaterialLayer>`)/`IfcMaterialLayer.Material`/`LayerThickness` (non-nullable double)/`Name`, `IfcMaterialProfileSet.MaterialProfiles` (`LIST<IfcMaterialProfile>` read first)/`IfcMaterialProfile.Material`/`Profile.ProfileName`/`Profile.StringSTEP()`, `IfcMaterialConstituentSet.MaterialConstituents` (`Dictionary<string, IfcMaterialConstituent>` read through `.Values`)/`IfcMaterialConstituent.Material`/`Category`/`Fraction` (non-nullable double), and `IfcMaterialLayerSetUsage.ForLayerSet`/`IfcMaterialProfileSetUsage.ForProfileSet` member spellings are verified against the live GeometryGym decompile; the usage entities are unwrapped to their underlying set here (the `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and `CardinalPoint`/`ReferenceExtent` occurrence payload riding the `Projection/relations#RELATION_ALGEBRA` `Associate` edge, not this composition node).
- [UNIT_COERCION]: the `IfcMaterialLayer.LayerThickness` (and every IFC length) is stored in the model's NATIVE units, NOT pre-coerced to SI — the GeometryGym decompile keeps a `IfcUnitAssignment.ScaleSI(IfcUnitEnum)` unit→SI factor (it does not normalize on the property), and the `parse`/`BuildStringSTEP` round-trip carries the raw STEP magnitude — so a Revit/ArchiCAD mm export delivers a millimetre `LayerThickness` that the seam SI-canonical `MeasureValue` must coerce. The ingress reads the factor through `set.Database.Context.UnitsInContext.ScaleSI(IfcUnitEnum.LENGTHUNIT)` and multiplies, admitting through `MeasureValue.OfSi`; the egress divides by the target model's factor. `DatabaseIfc.Context`, `IfcContext.UnitsInContext`, `BaseClassIfc.Database`, `IfcUnitAssignment.ScaleSI(IfcUnitEnum)`, and `IfcUnitEnum.LENGTHUNIT` are verified against the live decompile (`ScaleSI` returns `1.0` for an undeclared length unit, so a unitless or already-SI model is the identity case — no branch); treating the native length as already-SI is the mm-vs-metre import trap, and the seam `Header.Tolerance` threaded into `Project` must therefore itself be the SI tolerance for the content-hash quantization to grid against the SI measures.
- [SEAM_COMPOSITION_OWNERSHIP]: the seam `Composition/material#MATERIAL_COMPOSITION` owns the construction-material algebra — the `Material` node carrying the `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`) and the `MaterialPropertySet` `[Union]` keyed to the one `Discipline`, and owning the `MaterialComposition`/`MaterialLayer`/`MaterialConstituent`/`ProfileRef` rows — so this page declares no `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile` and instead composes the seam smart-constructors (the `Fin`-railing `MaterialComposition.OfLayerSet`/`OfConstituentSet` and the total `OfSingle`/`OfProfileSet`) and mints the seam `Graph/element#NODE_MODEL` `Node.Material` through the seam `NodeId.Content` over `Node.ToCanonicalBytes`; the `BimMaterial`/`BimElement.Materials`/`BimModel.Project` retirement collapses the two parallel unaligned element owners onto the one seam graph, the consumer-facing `Element` the `Graph/element#ELEMENT_GRAPH` `Bake` fold, never a second stored record.
- [PROFILE_REF_RESOLUTION]: the `ProfileRef` (`Standard` + `Designation` + content key) is the one-hop resolution seam — the `Rasm.Materials` projector resolves a `ProfileRef`→section-property (VividOrange) ONCE so a structural consumer never re-resolves per call, and the seam `Composition/material#MATERIAL_COMPOSITION` boundary references no VividOrange — so this page projects the `IfcMaterialProfile.Profile` onto the neutral `ProfileRef` and content-keys the full `IfcProfileDef` STEP through the kernel seed-zero `XxHash128` `Rasm.Domain.ContentHash.Of` (`libs/csharp/.api/api-hashing` + the kernel `Rasm/Domain/ContentHash` owner, the SAME hasher the `Model/elements#REPRESENTATION_KEYS` representation keyer composes via its `RepKey`, never a second hasher and never the up-stratum APP-PLATFORM `Rasm.Compute` `InterchangeIdentity` whose down-reference inverts the dependency DAG [H7]), the parametric dimensions preserved in the content-addressed store rather than folded onto the seam; an inline `IfcParameterizedProfileDef` dimension on the seam is the deleted form — the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam.
- [USAGE_ON_EDGE]: the occurrence usage binding (the IFC `IfcMaterialLayerSetUsage` `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and the `IfcMaterialProfileSetUsage` `CardinalPoint`/`ReferenceExtent`) rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge `MaterialUsage` payload [C7], NOT this owner — the `Associate` material edge carries the typed usage payload, the type-level `MaterialComposition` set stays the node's, and usage is the occurrence binding on the edge — so `Project` unwraps an `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` to its underlying set and produces only the type-level composition, the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection` authoring the usage onto the neutral edge; a layer set's direction never duplicates onto the composition, so a wall and its mirror share one `LayerSet` node with two `Associate` usages, and on egress `AuthorComposition` authors the one shared `IfcMaterialLayerSet` while `AuthorUsage` authors the two per-occurrence `IfcMaterialLayerSetUsage` instances.
- [MATERIAL_EGRESS]: the `MaterialProjection.AuthorComposition`/`AuthorUsage` egress half is the seam-graph reader that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — the `Rasm.Materials` `ComponentProjector` lowers the material subgraph onto the seam graph and `Rasm.Bim` reads it, the `Projection/egress#IFC_EGRESS` `Emit` the Bim-internal IFC egress — so the material/composition/property egress reads the projected seam `Material` node and the `Associate` edge `MaterialUsage`, never a Materials wire, the `Emit` composing it per Material node. The egress GeometryGym surface is verified against the live decompile (`.api/api-geometrygym-ifc` material families): `IfcMaterialLayerSet(IEnumerable<IfcMaterialLayer> layers, string name)` + `IfcMaterialLayer(IfcMaterial, double thickness, string name)`, `IfcMaterialProfileSet(string name, IfcMaterialProfile)` + `IfcMaterialProfile(string name, IfcMaterial, IfcProfileDef)`, `IfcMaterialConstituentSet(string name, IEnumerable<IfcMaterialConstituent>)` + `IfcMaterialConstituent(string name, IfcMaterial)` with the settable `Fraction`/`Category` (so the seam `MaterialConstituent.Category` round-trips onto the IFC `Category`, not the constituent name), the usage `IfcMaterialLayerSetUsage(IfcMaterialLayerSet, IfcLayerSetDirectionEnum, IfcDirectionSenseEnum, double)` over `IfcLayerSetDirectionEnum` (`AXIS1`/`AXIS2`/`AXIS3`) + `IfcDirectionSenseEnum` (`POSITIVE`/`NEGATIVE`) and `IfcMaterialProfileSetUsage(IfcMaterialProfileSet, IfcCardinalPointReference)` with the settable `ReferenceExtent`, and `IfcMaterialProperties(string name, IfcMaterialDefinition mat)` whose inherited `Dictionary<string, IfcProperty>` `Properties` takes TYPED `IfcPropertySingleValue` columns the value-typed ctor overload selects directly (the `(db,name,double)`/`(db,name,string)`/`(db,name,bool)` ctors decompile-confirmed — the package surface IS the typed-column dispatch, so no `Num`/`Text`/`Flag` rename layer wraps it — so the `FireRating` reaction class, the `Cost` `Currency`/`MeasurementBasis`, and the EPD id round-trip as typed label columns and the `Combustible` flag as a `Boolean`, never a lossy double — and the `Mechanical` `ShearModulus`/`UltimateStrength`, the `Thermal` vapour-resistance factor, the `Environmental` end-of-life recovery + EPD validity, AND the full EN 15978 per-`LifecycleStage` GWP vector (`A1-A3`..`D`, one column per module folded off `LifecycleStage.Items`, never a single aggregate that strands the per-stage breakdown the seam `StageGwp` carries) a double-only `Pset` strands are all emitted) — `IfcMaterial`/the three sets all satisfying `IfcMaterialSelect` so the `Associate` egress authors `IfcRelAssociatesMaterial(IfcMaterialSelect, IEnumerable<IfcDefinitionSelect>)`; the `ProfileSet` `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the same key `Project` `ProfileRefOf` mints), never a parametric dimension re-folded onto the seam.
