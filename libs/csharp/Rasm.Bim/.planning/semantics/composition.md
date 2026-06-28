# [BIM_MATERIAL_COMPOSITION]

The IFC material PROJECTOR lowering the live GeometryGym `IfcMaterialSelect` surface onto the `Rasm.Element` seam `Material` node: `MaterialProjection.Project` discriminates the relating-material runtime entity — `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` — and folds it into one content-keyed seam `Node.Material` carrying the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`). The seam OWNS the construction-material algebra (`MaterialComposition`, `MaterialLayer`, `MaterialConstituent`, `ProfileRef`, `MaterialPropertySet`, `MaterialId`); this page owns ONLY the GeometryGym discrimination that fills it, never re-declaring a Bim `BimMaterial`/`BimMaterialComposition` — the retired `BimMaterial` record and the `BimElement.Materials` column are GONE, a material is a seam `Material` node the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge, and the consumer reads `element.Materials` flat on the baked element rather than a second stored record keyed by `MaterialId`. The occurrence usage binding (layer direction/sense/offset, profile cardinal-point/extent) is NOT here — it rides the `Associate` edge `MaterialUsage` payload the `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection` authors [C7], this owner producing only the type-level SET structure so a wall and its mirror share one `LayerSet` with two `Associate` usages. A linear member's section is a neutral `ProfileRef` (`Standard` + `Designation` + content key) the `Rasm.Materials` projector resolves one-hop to the VividOrange section-property catalog [M7], its full `IfcProfileDef` parametric definition preserved in the content-addressed store the `ContentKey` keys — the page references NO VividOrange section type and folds NO parametric dimension onto the seam, because the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam. The projector is HOST-NEUTRAL: it reads the in-process GeometryGym graph and binds the profile geometry by content-hash reference, never a RhinoCommon type. An unresolvable material-select entity rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` via `.ToError()`; a degenerate composition (empty set, non-positive layer thickness, unnormalized constituent fractions) rails the seam `Rasm.Element/Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` the seam `MaterialComposition` admission owns. The projector is BIDIRECTIONAL: `MaterialProjection.Author` is the inverse half the `Projection/semantic#IFC_EGRESS` `Emit` composes per seam `Material` node — `AuthorComposition` re-authors the type-level `MaterialComposition` back onto the GeometryGym material-definition family (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial`) ONCE per material and lowers the seam `MaterialPropertySet` set onto `IfcMaterialProperties` `Pset_Material*` rows, and `AuthorUsage` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Relations/relation#EDGE_ALGEBRA` `Associate` edge `MaterialUsage` carries [C7]. This is the seam-graph egress that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers — `Rasm.Bim` reads the projected `Material` subgraph directly, never a Materials wire. A `ProfileSet`'s full parametric `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the seam holds only the neutral `ProfileRef` + baked `SectionProperties`), an unresolved profile railing `BimFault.DanglingReference`.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: `MaterialProjection.Project` the `IfcMaterialSelect`→seam `Node.Material` ingress fold, the per-modality `LayerSet`/`ProfileSet`/`ConstituentSet`/`Single` mapping onto the seam `MaterialComposition`, the `ProfileRef` content-keyer over the `IfcProfileDef` STEP, the `LayersOf`/`ConstituentsOf` row folds, the content-keyed `Mint` of the seam `Node.Material`, AND the inverse `MaterialProjection.AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node onto the GeometryGym material-definition family + the `MaterialPropertySet`→`IfcMaterialProperties` `Pset_Material*` rows + the `Associate`-edge `MaterialUsage`→`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence binding [C7].

## [02]-[MATERIAL_COMPOSITION]

- Owner: `MaterialProjection` the static BIDIRECTIONAL GeometryGym↔seam material projector — the `Project` ingress folding one `IfcMaterialSelect` runtime entity into one seam `Node.Material` (discriminating the entity, building the seam `MaterialComposition` through the seam smart-constructors, minting the content-keyed node id), and the `AuthorComposition`/`AuthorUsage` egress re-authoring a seam `Material` node back onto the GeometryGym material-definition family the `Projection/semantic#IFC_EGRESS` `Emit` composes. The seam owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent` rows, the `ProfileRef`, the `Relations/relation#EDGE_ALGEBRA` `MaterialUsage`, and the `MaterialPropertySet` engineering-property family; this page declares NONE of them — it composes the seam vocabulary, mapping the GeometryGym material-assembly entities onto it and back.
- Entry: `MaterialProjection.Project(BaseClassIfc relatingMaterial, double tolerance, Op key)` is the live-entity promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes when folding an `IfcRelAssociatesMaterial.RelatingMaterial` — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` and `IfcMaterialProfileSetUsage` its `ForProfileSet`, the usage payload riding the `Associate` edge not this node; `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` its primary `MaterialProfile`, `IfcMaterialConstituentSet` its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and returns one content-keyed seam `Node.Material`; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) and the seam `MaterialComposition` admission aborts a degenerate set (`ElementFault.ValueRejected`), each lowered with `.ToError()`. `MaterialProjection.AuthorComposition(DatabaseIfc db, Node.Material material, Func<ProfileRef, Option<IfcProfileDef>> profiles)` is the egress entry the `Emit` composes — it authors the type-level `MaterialComposition` ONCE (`Single`→`IfcMaterial`, `LayerSet`→`IfcMaterialLayerSet`, `ProfileSet`→`IfcMaterialProfileSet`, `ConstituentSet`→`IfcMaterialConstituentSet`), folds the seam `MaterialPropertySet` set onto the `IfcMaterialProperties` named Psets, and reconstitutes a `ProfileSet`'s `IfcProfileDef` from the content-addressed STEP store the injected `profiles` resolver reads (`Fin<T>` aborting `BimFault.DanglingReference` on an unresolved profile); `MaterialProjection.AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage)` wraps that shared definition in the per-occurrence `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` the `Associate` edge carries [C7], returning the bare definition for `MaterialUsage.None`.
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and builds the seam `MaterialComposition` through the seam Op-keyed smart-constructors (`MaterialComposition.LayerSet`/`ProfileSet`/`ConstituentSet`/`Single`), then mints the seam `Node.Material` whose id is the kernel seed-zero `XxHash128` over the seam `Node.ToCanonicalBytes` (id excluded) so two structurally-identical materials dedup to one node; `LayersOf` folds each `IfcMaterialLayer` onto a seam `MaterialLayer` carrying its `MaterialId`, an SI `MeasureValue` thickness over `Dimension.LengthDim` (the IFC length is SI-base, wrapped directly, never re-coerced), and its layer name; `ConstituentsOf` folds each `IfcMaterialConstituent` (read through the `Dictionary.Values`) onto a seam `MaterialConstituent` carrying its `MaterialId`, category, and `Fraction`; `ProfileRefOf` projects the primary `IfcMaterialProfile.Profile` onto a neutral `ProfileRef` whose `ContentKey` is the kernel `InterchangeIdentity.Key` over the `IfcProfileDef` STEP (the full parametric section preserved in the content-addressed store), the `Designation` the profile name, the `Standard` left to the one-hop catalog resolution; the engineering property sets arrive via the `Rasm.Materials` projector and the `Semantics/properties#PROPERTY_SETS` Pset round-trip, so the IFC-ingest `Node.Material` carries an empty `Seq<MaterialPropertySet>`.
- Receipt: the seam `Node.Material` is the material evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads through the `Associate` edge into `element.Materials` (a `MaterialBinding` carrying the node plus its occurrence `MaterialUsage`), the `Model/query#ELEMENT_SET` material predicate matches by `MaterialId` or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#PROPERTY_SETS` layered-volume takeoff reads from the `LayerSet` thicknesses; the layer build-up, the section material, and the constituent mix each carry their real composition on one seam node, never a parallel layer/profile/constituent record family.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new material-assembly modality is one seam `MaterialComposition` union arm (the seam's, not this page's) plus one `Project` switch arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the seam `MaterialLayer`/`MaterialConstituent`; a new section catalog is one `ProfileRef.Standard` token the `Rasm.Materials` projector resolves, never a seam edit; never a per-element-class material type, never a Bim `BimMaterial` record beside the seam node, and never a parallel material store.
- Boundary: the material model is the seam `Node.Material` + `MaterialComposition` and a Bim `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile`/`LayerSetUsage`/`ProfileSetUsage`/`ProfileDefKind`/`ProfileDims` re-declaration is the deleted form — the seam owns the algebra, this page owns only the GeometryGym discrimination that fills it; the retired `BimMaterial` record, the `BimElement.Materials` typed column, and the `BimModel.Project` material fold are GONE, a material being a seam node the `Bake` fold reads; the occurrence usage rides the `Associate` edge `MaterialUsage` payload [C7] and threading `LayerSetUsage`/`ProfileSetUsage` onto this composition node is the named seam violation — the type-level SET structure is shared, the per-occurrence geometric binding is the edge's; the `ProfileSet` arm carries a neutral `ProfileRef` (`Standard` + `Designation` + content key), NOT a VividOrange section type and NOT inline `IfcParameterizedProfileDef` dimensions — the full parametric section is preserved in the content-addressed store the `ContentKey` keys and the canonical section properties resolve one-hop to the catalog above the seam, so a profile-name-only `ProfileRef` that drops the content key is the deleted form; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `MaterialLayer` thickness is an already-SI kernel/IFC measure wrapped into a seam `MeasureValue` over `Dimension.LengthDim`, never a bare double and never a re-coercion; the section geometry binds by content-hash reference and a RhinoCommon profile field or an in-process BRep evaluation is the named seam violation; an unresolvable material-select entity lowers `Model/faults#FAULT_BAND` `BimFault.ModelRejected` through `.ToError()` and the seam `MaterialComposition` admission lowers `ElementFault.ValueRejected` on a degenerate set, a bare `Fin.Fail` without that lowering being the named seam defect; the EGRESS reads the seam `Material` node + the `Associate` edge `MaterialUsage` ONLY — a Materials `MaterialAssignmentWire`/`MaterialPropertyWire` carrier crossing into this owner is the deleted form (those Materials wires are retired, the material egress reading the projected seam subgraph), the type-level composition authored ONCE and the per-occurrence usage wrapping it so a wall and its mirror share one `IfcMaterialLayerSet` with two `IfcMaterialLayerSetUsage` instances, the `IfcMaterialProperties` Pset attaching to the authored `IfcMaterialDefinition` and the `ProfileSet` `IfcProfileDef` reconstituting one-hop from the content-addressed STEP (a parametric dimension re-folded onto the seam being the deleted form).

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
    public static Fin<Node.Material> Project(BaseClassIfc relatingMaterial, double tolerance, Op key) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage usage =>
                Optional(usage.ForLayerSet)
                    .ToFin(new BimFault.ModelRejected("material-layer-set-usage-unbound").ToError())
                    .Bind(set => LayerSetNode(set, tolerance, key)),
            IfcMaterialProfileSetUsage usage =>
                Optional(usage.ForProfileSet)
                    .ToFin(new BimFault.ModelRejected("material-profile-set-usage-unbound").ToError())
                    .Bind(set => ProfileSetNode(set, tolerance, key)),
            IfcMaterialLayerSet set       => LayerSetNode(set, tolerance, key),
            IfcMaterialProfileSet set     => ProfileSetNode(set, tolerance, key),
            IfcMaterialConstituentSet set => ConstituentSetNode(set, tolerance, key),
            IfcMaterial material          => SingleNode(material, tolerance, key),
            _ => Fin.Fail<Node.Material>(new BimFault.ModelRejected($"material-select-unresolved:{relatingMaterial.GetType().Name}").ToError()),
        };

    // Each modality builds the seam MaterialComposition through the seam Op-keyed smart-constructor (which
    // owns the admission — empty-set / non-positive-thickness / unnormalized-fraction rail ElementFault),
    // then mints the content-keyed seam Node.Material; the node MaterialKey is the IFC set/material name.
    static Fin<Node.Material> LayerSetNode(IfcMaterialLayerSet set, double tolerance, Op key) =>
        MaterialComposition.LayerSet(LayersOf(set), key)
            .Map(composition => Mint(MaterialId.Create(set.Name ?? ""), composition, tolerance));

    static Fin<Node.Material> ProfileSetNode(IfcMaterialProfileSet set, double tolerance, Op key) =>
        Optional(set.MaterialProfiles.FirstOrDefault())
            .ToFin(new BimFault.ModelRejected($"material-profile-set-empty:{set.Name}").ToError())
            .Bind(profile => MaterialComposition.ProfileSet(
                MaterialId.Create(profile.Material?.Name ?? set.Name ?? ""), ProfileRefOf(profile, tolerance), key))
            .Map(composition => Mint(MaterialId.Create(set.Name ?? ""), composition, tolerance));

    static Fin<Node.Material> ConstituentSetNode(IfcMaterialConstituentSet set, double tolerance, Op key) =>
        MaterialComposition.ConstituentSet(ConstituentsOf(set), key)
            .Map(composition => Mint(MaterialId.Create(set.Name ?? ""), composition, tolerance));

    static Fin<Node.Material> SingleNode(IfcMaterial material, double tolerance, Op key) =>
        MaterialComposition.Single(MaterialId.Create(material.Name ?? ""), key)
            .Map(composition => Mint(MaterialId.Create(material.Name ?? ""), composition, tolerance));

    // Each IfcMaterialLayer -> seam MaterialLayer (MaterialId + SI MeasureValue thickness + layer name).
    // The IFC layer thickness is SI-base, wrapped directly into the seam MeasureValue over Dimension.LengthDim.
    static Seq<MaterialLayer> LayersOf(IfcMaterialLayerSet set) =>
        set.MaterialLayers.AsIterable()
            .Map(static layer => new MaterialLayer(
                MaterialId.Create(layer.Material?.Name ?? ""),
                new MeasureValue(Dimension.LengthDim, layer.LayerThickness, "m"),
                layer.Name ?? ""))
            .ToSeq();

    static Seq<MaterialConstituent> ConstituentsOf(IfcMaterialConstituentSet set) =>
        set.MaterialConstituents.Values.AsIterable()
            .Map(static constituent => new MaterialConstituent(
                MaterialId.Create(constituent.Material?.Name ?? ""),
                constituent.Category ?? "",
                constituent.Fraction))
            .ToSeq();

    // The neutral profile reference: identity (Standard left to the one-hop catalog resolution, Designation
    // the profile name) plus the content key of the FULL IfcProfileDef STEP, so the parametric section is
    // recoverable from the content-addressed store and a standard section resolves one-hop to the VividOrange
    // catalog [M7]; no parametric dimension folds onto the seam (the kernel seed-zero XxHash128, no 2nd hasher).
    static ProfileRef ProfileRefOf(IfcMaterialProfile profile, double tolerance) =>
        new("", profile.Profile?.ProfileName ?? "",
            InterchangeIdentity.Key("ifc-profile",
                System.Text.Encoding.UTF8.GetBytes(profile.Profile?.StringSTEP() ?? ""), tolerance, tolerance, tolerance));

    // The content-keyed seam Material node: the id derives from the seam Node.ToCanonicalBytes (id excluded),
    // so two structurally-identical materials dedup to one node; the engineering property sets arrive via the
    // Rasm.Materials projector / Pset round-trip, empty at IFC ingest. The draft id is a discarded placeholder.
    static Node.Material Mint(MaterialId materialKey, MaterialComposition composition, double tolerance) {
        var draft = new Node.Material(NodeId.Content(default), materialKey, composition, Seq<MaterialPropertySet>());
        return draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };
    }

    // --- [EGRESS] -------------------------------------------------------------------------
    // The inverse half the Projection/semantic#IFC_EGRESS Emit composes per seam Material node: author the
    // type-level MaterialComposition ONCE onto the GeometryGym material-definition family + the MaterialPropertySet
    // set onto IfcMaterialProperties Pset_Material* rows. This REPLACES the retired Rasm.Materials
    // MaterialAssignmentWire/MaterialPropertyWire egress — the material subgraph reads off the seam graph directly.
    // A ProfileSet's parametric IfcProfileDef reconstitutes one-hop from the content-addressed STEP store the
    // ProfileRef.ContentKey keys (the seam holds only the neutral ProfileRef), an unresolved profile railing.
    public static Fin<IfcMaterialDefinition> AuthorComposition(DatabaseIfc db, Node.Material material, Func<ProfileRef, Option<IfcProfileDef>> profiles) =>
        Definition(db, material.Composition, material.MaterialKey, profiles)
            .Map(definition => { material.Properties.Iter(set => AuthorPropertySet(db, definition, set)); return definition; });

    static Fin<IfcMaterialDefinition> Definition(DatabaseIfc db, MaterialComposition composition, MaterialId key, Func<ProfileRef, Option<IfcProfileDef>> profiles) =>
        composition.Switch(
            single:        s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterial(db, s.Material.Value)),
            layerSet:      s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialLayerSet(
                                    s.Layers.Map(l => new IfcMaterialLayer(new IfcMaterial(db, l.Material.Value), l.ThicknessMm.Si, l.LayerName)), key.Value)),
            profileSet:    s => profiles(s.Profile).Match(
                                    Some: profile => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialProfileSet(key.Value,
                                                        new IfcMaterialProfile(s.Profile.Designation, new IfcMaterial(db, s.Material.Value), profile))),
                                    None: () => Fin.Fail<IfcMaterialDefinition>(new BimFault.DanglingReference($"material-profile-step-unresolved:{s.Profile.Designation}").ToError())),
            constituentSet: s => Fin.Succ<IfcMaterialDefinition>(new IfcMaterialConstituentSet(key.Value,
                                    s.Constituents.Map(c => new IfcMaterialConstituent(c.Category, new IfcMaterial(db, c.Material.Value)) { Fraction = c.Fraction }))));

    // The per-occurrence usage [C7]: wrap the shared definition in the IfcMaterialLayerSetUsage/ProfileSetUsage the
    // Associate edge MaterialUsage carries, or the bare definition for None / a Single / ConstituentSet (no usage).
    public static IfcMaterialSelect AuthorUsage(IfcMaterialDefinition definition, MaterialUsage usage) => (definition, usage) switch {
        (IfcMaterialLayerSet ls, MaterialUsage.LayerSet u)     => new IfcMaterialLayerSetUsage(ls, DirectionOf(u.Direction), SenseOf(u.Sense), u.OffsetFromReferenceLine),
        (IfcMaterialProfileSet ps, MaterialUsage.ProfileSet u) => new IfcMaterialProfileSetUsage(ps, (IfcCardinalPointReference)u.CardinalPoint) { ReferenceExtent = u.ReferenceExtent },
        _                                                      => definition,
    };

    static IfcLayerSetDirectionEnum DirectionOf(LayerSetDirection direction) => direction switch {
        LayerSetDirection.Axis1 => IfcLayerSetDirectionEnum.AXIS1,
        LayerSetDirection.Axis2 => IfcLayerSetDirectionEnum.AXIS2,
        _                       => IfcLayerSetDirectionEnum.AXIS3,
    };

    static IfcDirectionSenseEnum SenseOf(DirectionSense sense) =>
        sense == DirectionSense.Positive ? IfcDirectionSenseEnum.POSITIVE : IfcDirectionSenseEnum.NEGATIVE;

    // The seam MaterialPropertySet → its standard IFC material Pset (IfcMaterialProperties : IfcExtendedProperties
    // named set on the IfcMaterialDefinition): one Switch over the closed discipline family, each column an
    // IfcPropertySingleValue on the inherited Properties dict; the Environmental/Cost rows carry the seam's
    // embodied-carbon and procurement scalars (the Cost case tagging its Currency at the IfcMonetaryUnit join).
    static void AuthorPropertySet(DatabaseIfc db, IfcMaterialDefinition material, MaterialPropertySet set) => set.Switch(
        mechanical:    m => Pset(db, material, "Pset_MaterialMechanical", ("MassDensity", m.Density.Si), ("YoungModulus", m.YoungsModulus.Si), ("YieldStress", m.YieldStrength.Si), ("PoissonRatio", m.PoissonsRatio), ("ThermalExpansionCoefficient", m.ThermalExpansionPerK)),
        thermal:       t => Pset(db, material, "Pset_MaterialThermal", ("ThermalConductivity", t.Conductivity.Si), ("SpecificHeatCapacity", t.SpecificHeat.Si), ("ThermalTransmittance", t.UValue.Si)),
        acoustic:      a => Pset(db, material, "Pset_MaterialAcoustic", ("NoiseReductionCoefficient", a.Nrc), ("SoundAbsorptionAverage", a.Saa), ("SoundTransmissionClass", a.StcWeighted)),
        fire:          f => Pset(db, material, "Pset_MaterialFire", ("Combustible", f.Reaction.Combustible ? 1.0 : 0.0), ("FireResistanceRating", f.ResistanceMinutes)),
        environmental: e => Pset(db, material, "Pset_EnvironmentalImpactValues", ("Carbon", e.Gwp), ("WholeLifeCarbon", e.WholeLifeGwp), ("RecycledContent", e.RecycledContent), ("ExpectedServiceLife", e.ValidUntilYear)),
        cost:          c => Pset(db, material, "Pset_ConstructionCosts", ("SupplyCost", c.SupplyPerUnit), ("InstallationCost", c.InstallPerUnit), ("LifeCycleCost", c.LifecyclePerUnit)));

    // IfcMaterialProperties(string name, IfcMaterialDefinition mat) named Pset; each column lands as an
    // IfcPropertySingleValue through the public (DatabaseIfc, string, double) ctor on the inherited Properties dict
    // (member surface decompile-confirmed, .api/api-geometrygym-ifc material families rows 15/19/20 + [MATERIALS_EGRESS]).
    static void Pset(DatabaseIfc db, IfcMaterialDefinition material, string name, params (string Name, double Value)[] columns) {
        var pset = new IfcMaterialProperties(name, material);
        columns.Iter(c => pset.Properties[c.Name] = new IfcPropertySingleValue(db, c.Name, c.Value));
    }
}
```

## [03]-[RESEARCH]

- [MATERIAL_SELECT_DISPATCH]: the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` runtime-type discrimination — the `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` cases the `Project` switch folds — grounds against the GeometryGym `IfcMaterialSelect` member surface (`.api/api-geometrygym-ifc` material families) so the fold discriminates the real material-assembly entity rather than a guessed shape; the `IfcMaterialLayerSet.MaterialLayers` (`LIST<IfcMaterialLayer>`)/`IfcMaterialLayer.Material`/`LayerThickness` (non-nullable double)/`Name`, `IfcMaterialProfileSet.MaterialProfiles` (`LIST<IfcMaterialProfile>` read first)/`IfcMaterialProfile.Material`/`Profile.ProfileName`/`Profile.StringSTEP()`, `IfcMaterialConstituentSet.MaterialConstituents` (`Dictionary<string, IfcMaterialConstituent>` read through `.Values`)/`IfcMaterialConstituent.Material`/`Category`/`Fraction` (non-nullable double), and `IfcMaterialLayerSetUsage.ForLayerSet`/`IfcMaterialProfileSetUsage.ForProfileSet` member spellings are verified against the live GeometryGym decompile; the usage entities are unwrapped to their underlying set here (the `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and `CardinalPoint`/`ReferenceExtent` occurrence payload riding the `Projection/semantic#RELATION_ALGEBRA` `Associate` edge, not this composition node).
- [SEAM_COMPOSITION_OWNERSHIP]: the seam owns the construction-material algebra — `ELEMENT-REBUILD-PLAN.md` §4B (`Material` node: `MaterialComposition` `[Union]` `Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`; `MaterialPropertySet` `[Union]` keyed to the one `Discipline`) and §4D (`Rasm.Element/Composition/material.md` owns the `Material` node, the `MaterialComposition`, and the `ProfileRef`) — so this page declares no `BimMaterial`/`BimMaterialComposition`/`MaterialLayer`/`MaterialProfile` and instead composes the seam `Composition/material#MATERIAL_COMPOSITION` smart-constructors (`MaterialComposition.LayerSet`/`ProfileSet`/`ConstituentSet`/`Single`, each Op-keyed and admission-railing) and mints the seam `Graph/element#NODE_MODEL` `Node.Material` through the seam `NodeId.Content` over `Node.ToCanonicalBytes`; the `BimElement.Materials`/`BimModel.Project` retirement grounds against §2 (two parallel unaligned element owners collapsed) and §4B (the consumer-facing `Element` is the `Bake` fold, never a second stored record).
- [PROFILE_REF_RESOLUTION]: the `ProfileRef` (`Standard` + `Designation` + content key) is the one-hop resolution seam — `ELEMENT-REBUILD-PLAN.md` §4-RT M7 (settle `ProfileRef`→section-property (VividOrange) as a ONE-HOP resolution so structural consumers do not re-resolve per call) and the seam `Composition/material#MATERIAL_COMPOSITION` boundary (the seam references no VividOrange, the `Rasm.Materials` projector resolving the `ProfileRef` once to the catalog) — so this page projects the `IfcMaterialProfile.Profile` onto the neutral `ProfileRef` and content-keys the full `IfcProfileDef` STEP through the kernel `InterchangeIdentity.Key` (`.api/api-hashing` + the kernel identity owner, the same seed-zero `XxHash128` the `Model/elements#REPRESENTATION_KEYS` representation keyer composes, never a second hasher [H7]), the parametric dimensions preserved in the content-addressed store rather than folded onto the seam; the `IfcParameterizedProfileDef` subtype reading the migration source carried inline is superseded — the dimensions live in the content-keyed STEP and the canonical section properties resolve one-hop above the seam.
- [USAGE_ON_EDGE]: the occurrence usage binding (the IFC `IfcMaterialLayerSetUsage` `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` and the `IfcMaterialProfileSetUsage` `CardinalPoint`/`ReferenceExtent`) rides the `Relations/relation#EDGE_ALGEBRA` `Associate` edge `MaterialUsage` payload [C7], NOT this owner — `ELEMENT-REBUILD-PLAN.md` §4-RT C7 (the `Associate` material edge carries the typed usage payload; the type-level `MaterialComposition` set stays; usage is the occurrence binding on the edge) — so `Project` unwraps an `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` to its underlying set and produces only the type-level composition, the `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection` authoring the usage onto the neutral edge; a layer set's direction never duplicates onto the composition, so a wall and its mirror share one `LayerSet` node with two `Associate` usages, and on egress `AuthorComposition` authors the one shared `IfcMaterialLayerSet` while `AuthorUsage` authors the two per-occurrence `IfcMaterialLayerSetUsage` instances.
- [MATERIAL_EGRESS]: the `MaterialProjection.AuthorComposition`/`AuthorUsage` egress half is the seam-graph reader that REPLACES the retired `Rasm.Materials` `MaterialAssignmentWire`/`MaterialPropertyWire` carriers (`ELEMENT-REBUILD-PLAN.md` §6 `Rasm.Materials` ripple — the `MaterialProjector` lowers the material subgraph onto the seam graph, `Rasm.Bim` reads it; §4C `Emit` is the Bim-internal IFC egress) — so the material/composition/property egress reads the projected seam `Material` node and the `Associate` edge `MaterialUsage`, never a Materials wire, the `Projection/semantic#IFC_EGRESS` `Emit` composing it per Material node. The egress GeometryGym surface is verified against the live decompile (`.api/api-geometrygym-ifc` material families): `IfcMaterialLayerSet(IEnumerable<IfcMaterialLayer> layers, string name)` + `IfcMaterialLayer(IfcMaterial, double thickness, string name)`, `IfcMaterialProfileSet(string name, IfcMaterialProfile)` + `IfcMaterialProfile(string name, IfcMaterial, IfcProfileDef)`, `IfcMaterialConstituentSet(string name, IEnumerable<IfcMaterialConstituent>)` + `IfcMaterialConstituent(string name, IfcMaterial)` with the settable `Fraction`, the usage `IfcMaterialLayerSetUsage(IfcMaterialLayerSet, IfcLayerSetDirectionEnum, IfcDirectionSenseEnum, double)` over `IfcLayerSetDirectionEnum` (`AXIS1`/`AXIS2`/`AXIS3`) + `IfcDirectionSenseEnum` (`POSITIVE`/`NEGATIVE`) and `IfcMaterialProfileSetUsage(IfcMaterialProfileSet, IfcCardinalPointReference)` with the settable `ReferenceExtent`, and `IfcMaterialProperties(string name, IfcMaterialDefinition mat)` whose inherited `Properties` dict takes `IfcPropertySingleValue(DatabaseIfc, string, double)` columns — `IfcMaterial`/the three sets all satisfying `IfcMaterialSelect` so the `Associate` egress authors `IfcRelAssociatesMaterial(IfcMaterialSelect, IEnumerable<IfcDefinitionSelect>)`; the `ProfileSet` `IfcProfileDef` reconstitutes one-hop from the content-addressed STEP store the `ProfileRef.ContentKey` keys (the same key `Project` `ProfileRefOf` mints), never a parametric dimension re-folded onto the seam.
