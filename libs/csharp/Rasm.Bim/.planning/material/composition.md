# [BIM_MATERIAL_COMPOSITION]

The host-neutral construction-material composition owner: one `BimMaterial` record carrying the named material plus a closed `BimMaterialComposition` `[Union]` (`LayerSet`/`ProfileSet`/`ConstituentSet`) — one family, three cases, never three sibling material types — and the `MaterialProjection` fold that promotes the `exchange/import-rail#IMPORT_RAIL` flat `IfcSemanticModel.MaterialRow(OwnerGlobalId, MaterialName)` name-string projection into the real IFC layered/profiled/constituent assembly the schema models. A wall's ordered thickness-keyed layer build-up, a beam's cross-section profile material, and a façade's named constituent-fraction mix each carry their full composition projected from the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` surface, never a single material name. The composition is HOST-NEUTRAL — the profile geometry binds the kernel `Rasm` geometry by reference and never carries a RhinoCommon type — and the surface-style PBR appearance the same `IfcMaterial.HasRepresentation` ships is the twin `material/appearance#MATERIAL_APPEARANCE` owner's concern reconciled with `Rasm.Materials/appearance/interchange#MATERIAL_WIRE` at the content-key seam, so this page owns the construction-material algebra and never the rendering style. The typed `Seq<BimMaterial>` binding this owner produces is the promotion the `model/elements#ELEMENT_MODEL` `BimElement.Materials` column swaps to, the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` layered-volume takeoff reads, and the `query/element-set#ELEMENT_SET` material predicate matches; a material rejection lowers onto `faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [1]-[INDEX]

- [2]-[MATERIAL_COMPOSITION]: `BimMaterial` record with its `Of(MaterialRow)` flat-name seam, the `BimMaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), the `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` rows, the `MaterialProjection.Project`/`ProjectAll` fold from the live `IfcMaterialSelect` surface, and the `BimElement.Materials` typed binding.

## [2]-[MATERIAL_COMPOSITION]

- Owner: `BimMaterial` the single host-neutral material record carrying the stable material name and one `BimMaterialComposition`; `BimMaterialComposition` the closed `[Union]` discriminating the three IFC material-assembly modalities — `LayerSet` (the ordered thickness-keyed `IfcMaterialLayerSet` wall/slab/roof build-up with its optional `IfcMaterialLayerSetUsage` application axis), `ProfileSet` (the `IfcMaterialProfileSet` cross-section profile material for linear members), `ConstituentSet` (the `IfcMaterialConstituentSet` named fraction mix for composites), plus the degenerate `Single` (a bare `IfcMaterial` name with no assembly) so a name-only material is one case of the same family rather than an `Option`-wrapped escape; `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` the `[ValueObject]` assembly rows keyed by their natural identity (the layer by its ordinal position and thickness, the profile by its section name, the constituent by its name); `MaterialProjection` the static fold over the GeometryGym `IfcMaterialSelect` surface.
- Cases: `BimMaterialComposition` arms `Single` (a bare `IfcMaterial`, no assembly — `name`) · `LayerSet` (`Seq<MaterialLayer>` ordered build-up plus the optional `LayerSetUsage` direction/sense/offset application) · `ProfileSet` (`Seq<MaterialProfile>` section-material rows) · `ConstituentSet` (`Seq<MaterialConstituent>` fraction rows) (4); the `MaterialLayer` row carries its `Material` name, `Thickness`, ordinal `Index`, optional `Category` and `IsVentilated`; the `MaterialProfile` row carries its `Material` name, `ProfileName`, and the kernel-geometry handle the section profile binds by reference; the `MaterialConstituent` row carries its `Material` name, `Name`, and `Fraction`.
- Entry: `BimMaterial.Of(IfcSemanticModel.MaterialRow row)` is the flat-name seam the `model/elements#ELEMENT_MODEL` `BimModel.Project` fold binds — it folds the import-rail name-only `MaterialRow` into a `Single`-arm `BimMaterial` so the element collection always carries a typed material even when only the flat row survives; `MaterialProjection.Project(BaseClassIfc relatingMaterial)` is the live-entity richer promotion folding the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` into one `BimMaterial` — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` then folds the layers carrying the usage axis, `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` folds its `MaterialProfiles`, `IfcMaterialConstituentSet` folds its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and `MaterialProjection.ProjectAll(IfcRelAssociatesMaterial rel)` lifts the relationship's related-object set onto the per-occurrence `(GlobalId, BimMaterial)` binding a layered-assembly-aware re-projection reads; `Fin<T>` aborts on an unresolvable material-select entity (`faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`.
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and folds the GeometryGym assembly into the typed composition — `IfcMaterialLayerSetUsage` reads `ForLayerSet.MaterialLayers` and threads `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` onto the `LayerSetUsage`, each `IfcMaterialLayer` projects to a `MaterialLayer` row carrying `Material.Name`/`LayerThickness`/its ordinal index/`Category`/`IsVentilated`, each `IfcMaterialProfile` projects to a `MaterialProfile` carrying `Material.Name`/`Profile.ProfileName` and the kernel-geometry handle the section binds by reference, each `IfcMaterialConstituent` projects to a `MaterialConstituent` carrying `Material.Name`/`Name`/`Fraction`; the `BimMaterial.TotalThickness` fold sums the `LayerSet` thicknesses so the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` reads the layered-wall build-up volume from the layer rows rather than re-tessellating, and the `BimMaterial.AppearanceKey` projects the material name as the content key the twin `material/appearance#MATERIAL_APPEARANCE` owner resolves the `IfcSurfaceStyleRendering` PBR appearance through.
- Receipt: the typed `Seq<BimMaterial>` is the material evidence the `model/elements#ELEMENT_MODEL` `BimElement.Materials` binding carries (the `Seq<string>` name-only column the import-rail produced is the deleted form), the `query/element-set#ELEMENT_SET` material predicate matches by material name or composition modality, the `validation/ids#IDS_FACETS` Material facet matches against, and the `properties/property-sets#PROPERTY_SETS` layered-volume takeoff folds; the layer build-up, the section material, and the constituent mix each carry their real composition on one record.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new material-assembly modality is one `BimMaterialComposition` union arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the existing `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` value object; a new occurrence binding rides the existing `ProjectAll` fold on one row; never a per-element-class material type, never a second material store, and never a parallel layer/profile/constituent record family.
- Boundary: `BimMaterial` is ONE record discriminated by the `BimMaterialComposition` union — a `LayeredMaterial`/`ProfiledMaterial`/`ConstituentMaterial` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `model/elements#ELEMENT_MODEL`; the name-only material is the `Single` union arm, never an `Option<BimMaterialComposition>` escape; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` rows 11-14, 10) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the surface-style PBR appearance the same `IfcMaterial.HasRepresentation`/`IfcMaterialDefinitionRepresentation` ships is the twin `material/appearance#MATERIAL_APPEARANCE` owner reconciled with `Rasm.Materials/appearance/interchange#MATERIAL_WIRE` at the material-name content key — projecting the appearance here is the named seam violation, and re-minting the OpenPBR algebra in either Bim owner is the named cross-folder drift defect; the `MaterialProfile` section geometry binds the kernel `Rasm` geometry by reference and a RhinoCommon profile field is the named seam violation; the `exchange/import-rail#IMPORT_RAIL` flat `IfcSemanticModel.MaterialRow(OwnerGlobalId, MaterialName)` stays the wire shape and this owner is the typed promotion the element/query/IDS/property consumers read; a material rejection lowers onto `faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<double>]
[KeyMemberComparer<InterchangeKeyPolicy, double>]
public readonly partial struct LayerThickness {
    static partial void ValidateFactoryArguments(ref ValidationError? error, ref double value) {
        if (value <= 0d || double.IsNaN(value) || double.IsInfinity(value))
            error = new ValidationError($"layer-thickness-non-positive:{value}");
    }
}

public sealed record MaterialLayer(
    int Index,
    string Material,
    LayerThickness Thickness,
    string Category,
    bool IsVentilated);

public sealed record MaterialProfile(
    string Material,
    string ProfileName,
    GeometryHandle Section);

public sealed record MaterialConstituent(
    string Material,
    string Name,
    double Fraction);

public sealed record LayerSetUsage(
    int LayerSetDirection,
    int DirectionSense,
    double OffsetFromReferenceLine);

[Union]
public partial record BimMaterialComposition {
    partial record Single(string Material);
    partial record LayerSet(Seq<MaterialLayer> Layers, Option<LayerSetUsage> Usage);
    partial record ProfileSet(Seq<MaterialProfile> Profiles);
    partial record ConstituentSet(Seq<MaterialConstituent> Constituents);

    public double TotalThickness => Switch(
        single:        static _ => 0d,
        layerSet:      static c => c.Layers.Fold(0d, static (sum, l) => sum + l.Thickness.Value),
        profileSet:    static _ => 0d,
        constituentSet: static _ => 0d);
}

public sealed record BimMaterial(string Name, BimMaterialComposition Composition) {
    public double TotalThickness => Composition.TotalThickness;
    public string AppearanceKey => Name;

    public static BimMaterial Of(IfcSemanticModel.MaterialRow row) =>
        new(row.MaterialName, new BimMaterialComposition.Single(row.MaterialName));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class MaterialProjection {
    public static Fin<BimMaterial> Project(BaseClassIfc relatingMaterial) =>
        relatingMaterial switch {
            IfcMaterialLayerSetUsage usage =>
                Optional(usage.ForLayerSet)
                    .Map(static set => new BimMaterial(
                        set.Name ?? "",
                        new BimMaterialComposition.LayerSet(LayersOf(set), UsageOf(usage))))
                    .ToFin(new BimFault.ModelRejected("material-layer-set-usage-unbound").ToError()),
            IfcMaterialLayerSet set =>
                FinSucc(new BimMaterial(set.Name ?? "", new BimMaterialComposition.LayerSet(LayersOf(set), None))),
            IfcMaterialProfileSet set =>
                FinSucc(new BimMaterial(set.Name ?? "", new BimMaterialComposition.ProfileSet(ProfilesOf(set)))),
            IfcMaterialConstituentSet set =>
                FinSucc(new BimMaterial(set.Name ?? "", new BimMaterialComposition.ConstituentSet(ConstituentsOf(set)))),
            IfcMaterial material =>
                FinSucc(new BimMaterial(material.Name ?? "", new BimMaterialComposition.Single(material.Name ?? ""))),
            _ =>
                FinFail<BimMaterial>(new BimFault.ModelRejected($"material-select-unresolved:{relatingMaterial.GetType().Name}").ToError()),
        };

    public static Fin<Seq<(string GlobalId, BimMaterial Material)>> ProjectAll(IfcRelAssociatesMaterial rel) =>
        Optional(rel.RelatingMaterial as BaseClassIfc)
            .ToFin(new BimFault.ModelRejected($"material-relation-unbound:{rel.GlobalId}").ToError())
            .Bind(Project)
            .Map(material => rel.RelatedObjects
                .AsIterable()
                .Map(o => (o.GlobalId, material))
                .ToSeq());

    static Seq<MaterialLayer> LayersOf(IfcMaterialLayerSet set) =>
        set.MaterialLayers
            .AsIterable()
            .Map(static (index, layer) => new MaterialLayer(
                index,
                layer.Material?.Name ?? "",
                LayerThickness.Create(layer.LayerThickness),
                layer.Category ?? "",
                layer.IsVentilated == IfcLogicalEnum.TRUE))
            .ToSeq();

    static Seq<MaterialProfile> ProfilesOf(IfcMaterialProfileSet set) =>
        set.MaterialProfiles
            .AsIterable()
            .Map(static profile => new MaterialProfile(
                profile.Material?.Name ?? "",
                profile.Profile?.ProfileName ?? "",
                GeometryHandle.Pending(profile.Profile?.ProfileName ?? "")))
            .ToSeq();

    static Seq<MaterialConstituent> ConstituentsOf(IfcMaterialConstituentSet set) =>
        set.MaterialConstituents.Values
            .AsIterable()
            .Map(static constituent => new MaterialConstituent(
                constituent.Material?.Name ?? "",
                constituent.Name ?? "",
                constituent.Fraction))
            .ToSeq();

    static Option<LayerSetUsage> UsageOf(IfcMaterialLayerSetUsage usage) =>
        Some(new LayerSetUsage(
            (int)usage.LayerSetDirection,
            (int)usage.DirectionSense,
            usage.OffsetFromReferenceLine));
}
```

## [3]-[RESEARCH]

- [MATERIAL_SELECT_DISPATCH]: the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` runtime-type discrimination — the `IfcMaterialLayerSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` cases the `Project` switch folds — grounds against the GeometryGym `IfcMaterialSelect` member surface (`.api/api-geometrygym-ifc` rows 10-14) so the `Project` fold discriminates the real material-assembly entity rather than a guessed shape; the `IfcMaterialLayerSet.MaterialLayers` (`LIST<IfcMaterialLayer>`)/`IfcMaterialLayer.Material`/`LayerThickness` (non-nullable double)/`Category`/`IsVentilated` (`IfcLogicalEnum`), `IfcMaterialProfileSet.MaterialProfiles` (`LIST<IfcMaterialProfile>`)/`IfcMaterialProfile.Material`/`Profile.ProfileName`, `IfcMaterialConstituentSet.MaterialConstituents` (`Dictionary<string, IfcMaterialConstituent>` read through `.Values`)/`IfcMaterialConstituent.Material`/`Name`/`Fraction` (non-nullable double), and `IfcMaterialLayerSetUsage.ForLayerSet`/`LayerSetDirection` (`IfcLayerSetDirectionEnum`)/`DirectionSense` (`IfcDirectionSenseEnum`)/`OffsetFromReferenceLine` member spellings are verified against the live GeometryGym decompile, fixing the `ConstituentsOf` fold to read the constituent `Dictionary.Values` and the non-nullable `Fraction`.
- [LAYERED_VOLUME_DERIVE]: the `BimMaterialComposition.LayerSet` `TotalThickness` fold the `properties/property-sets#PROPERTY_SETS` `QuantitySet.Derive` reads for a layered-wall volume takeoff grounds against the buildingSMART base-quantity definitions and the kernel `Rasm` geometry the element binds by reference, so a per-layer `Qto_WallBaseQuantities.NetVolume` derives from the layer thicknesses and the bound geometry's net area rather than re-tessellating; the cross-page contract confirms the property owner reads the layer rows this owner produces, never a second thickness source.
- [APPEARANCE_CONTENT_KEY]: the `BimMaterial.AppearanceKey` material-name content key the twin `material/appearance#MATERIAL_APPEARANCE` owner resolves the `IfcSurfaceStyleRendering` PBR appearance through grounds against the `Rasm.Materials/appearance/interchange#MATERIAL_WIRE` single-mint OpenPBR vocabulary, so the construction-material composition and the rendering style meet at the material-name key and the appearance never re-mints the OpenPBR algebra in the Bim folder; the `IfcMaterialDefinitionRepresentation`/`IfcMaterial.HasRepresentation` style-binding traversal stays the appearance owner's concern, confirmed against the catalogued `.api/api-geometrygym-ifc` row 15 surface, never re-cased on this composition page.
