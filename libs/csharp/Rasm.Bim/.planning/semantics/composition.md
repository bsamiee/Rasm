# [BIM_MATERIAL_COMPOSITION]

The host-neutral construction-material composition owner: one `BimMaterial` record carrying the named material plus a closed `BimMaterialComposition` `[Union]` (`LayerSet`/`ProfileSet`/`ConstituentSet`) — one family, three cases, never three sibling material types — and the `MaterialProjection` fold that promotes the `Exchange/import#IMPORT_RAIL` flat `IfcSemanticModel.MaterialRow(OwnerGlobalId, MaterialName)` name-string projection into the real IFC layered/profiled/constituent assembly the schema models. A wall's ordered thickness-keyed layer build-up, a beam's cross-section profile material, and a façade's named constituent-fraction mix each carry their full composition projected from the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` surface, never a single material name. The `ProfileSet` arm is the reciprocal ingress for the `Rasm.Materials/Profiles/steel#STEEL_FAMILY` egress — each `MaterialProfile` discriminates the `IfcMaterialProfile.Profile` runtime type onto a `ProfileDefKind` (the reciprocal of the catalogue-grounded `SteelClass.IfcSubtype` axis) and folds the `IfcParameterizedProfileDef` dimensional members onto a `ProfileDims` (the reciprocal of the steel `SectionDims`, the `DoubleL` back-to-back spacing on `BackToBackMm`), and the arm carries the `IfcMaterialProfileSetUsage` cardinal-point/reference-extent placement on a `ProfileSetUsage` (the reciprocal of the layer-set `LayerSetUsage`), so a linear member round-trips its section subtype, dimensions, and placement rather than collapsing to a bare profile-name string. The composition is HOST-NEUTRAL — the profile geometry binds the kernel `Rasm` geometry by reference and never carries a RhinoCommon type — and the surface-style PBR appearance the same `IfcMaterial.HasRepresentation` ships is the twin `Semantics/appearance#MATERIAL_APPEARANCE` owner's concern reconciled with `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` at the content-key seam, so this page owns the construction-material algebra and never the rendering style. The typed `Seq<BimMaterial>` binding this owner produces is the promotion the `Model/elements#ELEMENT_MODEL` `BimElement.Materials` column swaps to, the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` layered-volume takeoff reads, and the `Model/query#ELEMENT_SET` material predicate matches; a material rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[MATERIAL_COMPOSITION]: `BimMaterial` record with its `Of(MaterialRow)` flat-name seam, the `BimMaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet`/`ConstituentSet`), the `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` rows, the `ProfileDefKind` `IfcProfileDef`-subtype discriminant with its `ProfileDims`/`ProfileSetUsage`/`CardinalPoint` profile-set reciprocal of the `Rasm.Materials` steel egress, the `MaterialProjection.Project`/`ProjectAll` fold from the live `IfcMaterialSelect` surface, and the `BimElement.Materials` typed binding.

## [02]-[MATERIAL_COMPOSITION]

- Owner: `BimMaterial` the single host-neutral material record carrying the stable material name and one `BimMaterialComposition`; `BimMaterialComposition` the closed `[Union]` discriminating the three IFC material-assembly modalities — `LayerSet` (the ordered thickness-keyed `IfcMaterialLayerSet` wall/slab/roof build-up with its optional `IfcMaterialLayerSetUsage` application axis), `ProfileSet` (the `IfcMaterialProfileSet` cross-section profile material for linear members with its optional `IfcMaterialProfileSetUsage` cardinal-point application axis), `ConstituentSet` (the `IfcMaterialConstituentSet` named fraction mix for composites), plus the degenerate `Single` (a bare `IfcMaterial` name with no assembly) so a name-only material is one case of the same family rather than an `Option`-wrapped escape; `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` the `[ValueObject]` assembly rows keyed by their natural identity (the layer by its ordinal position and thickness, the profile by its section name plus its `ProfileDefKind` `IfcProfileDef`-subtype discriminant and `ProfileDims` parametric columns, the constituent by its name); `ProfileDefKind`/`CardinalPoint` the smart-enum reciprocals of the `Rasm.Materials/Profiles/steel#STEEL_FAMILY` `SteelClass.IfcSubtype` and orientation axes, `ProfileSetUsage` the profile placement reciprocal of `LayerSetUsage`; `MaterialProjection` the static fold over the GeometryGym `IfcMaterialSelect` surface.
- Cases: `BimMaterialComposition` arms `Single` (a bare `IfcMaterial`, no assembly — `name`) · `LayerSet` (`Seq<MaterialLayer>` ordered build-up plus the optional `LayerSetUsage` direction/sense/offset application) · `ProfileSet` (`Seq<MaterialProfile>` section-material rows plus the optional `ProfileSetUsage` cardinal-point/reference-extent application) · `ConstituentSet` (`Seq<MaterialConstituent>` fraction rows) (4); the `MaterialLayer` row carries its `Material` name, `Thickness`, ordinal `Index`, optional `Category` and `IsVentilated`; the `MaterialProfile` row carries its `Material` name, `ProfileName`, the `ProfileDefKind` `IfcProfileDef`-subtype discriminant, the optional `ProfileDims` parametric columns (depth/width/web/flange/fillet plus the `DoubleL` `BackToBackMm`), and the kernel-geometry handle the section profile binds by reference; the `MaterialConstituent` row carries its `Material` name, `Name`, and `Fraction`.
- Entry: `BimMaterial.Of(IfcSemanticModel.MaterialRow row)` is the flat-name seam the `Model/elements#ELEMENT_MODEL` `BimModel.Project` fold binds — it folds the import rail name-only `MaterialRow` into a `Single`-arm `BimMaterial` so the element collection always carries a typed material even when only the flat row survives; `MaterialProjection.Project(BaseClassIfc relatingMaterial)` is the live-entity richer promotion folding the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` into one `BimMaterial` — discriminating the runtime entity (`IfcMaterialLayerSetUsage` unwraps its `ForLayerSet` then folds the layers carrying the layer-set usage axis, `IfcMaterialProfileSetUsage` unwraps its `ForProfileSet` then folds the profiles carrying the cardinal-point usage axis, `IfcMaterialLayerSet` folds its `MaterialLayers`, `IfcMaterialProfileSet` folds its `MaterialProfiles` onto the subtype-discriminated `MaterialProfile` rows, `IfcMaterialConstituentSet` folds its `MaterialConstituents.Values`, a bare `IfcMaterial` folds to `Single`) — and `MaterialProjection.ProjectAll(IfcRelAssociatesMaterial rel)` lifts the relationship's related-object set onto the per-occurrence `(GlobalId, BimMaterial)` binding a layered-assembly-aware re-projection reads; `Fin<T>` aborts on an unresolvable material-select entity (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`.
- Auto: `Project` reads the `IfcMaterialSelect` runtime type and folds the GeometryGym assembly into the typed composition — `IfcMaterialLayerSetUsage` reads `ForLayerSet.MaterialLayers` and threads `LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine` onto the `LayerSetUsage`, `IfcMaterialProfileSetUsage` reads `ForProfileSet.MaterialProfiles` and threads its `CardinalPoint`/`ReferenceExtent` onto the `ProfileSetUsage` through the keyed `CardinalPoint.Get`, each `IfcMaterialLayer` projects to a `MaterialLayer` row carrying `Material.Name`/`LayerThickness`/its ordinal index/`Category`/`IsVentilated`, each `IfcMaterialProfile` projects to a `MaterialProfile` carrying `Material.Name`/`Profile.ProfileName`, the `ProfileDefKind.Of(Profile)` `IfcProfileDef`-subtype discriminant, the `DimsOf(Profile)` parametric columns (the `IfcParameterizedProfileDef` depth/width/web/flange/fillet members, `None` for the arbitrary-closed double-angle/composite), and the kernel-geometry handle the section binds by reference, each `IfcMaterialConstituent` projects to a `MaterialConstituent` carrying `Material.Name`/`Name`/`Fraction`; the `BimMaterial.TotalThickness` fold sums the `LayerSet` thicknesses so the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` reads the layered-wall build-up volume from the layer rows rather than re-tessellating, and the `BimMaterial.AppearanceKey` projects the material name as the content key the twin `Semantics/appearance#MATERIAL_APPEARANCE` owner resolves the `IfcSurfaceStyleRendering` PBR appearance through.
- Receipt: the typed `Seq<BimMaterial>` is the material evidence the `Model/elements#ELEMENT_MODEL` `BimElement.Materials` binding carries (the `Seq<string>` name-only column the import rail produced is the deleted form), the `Model/query#ELEMENT_SET` material predicate matches by material name or composition modality, the `Review/validation#IDS_FACETS` Material facet matches against, and the `Semantics/properties#PROPERTY_SETS` layered-volume takeoff folds; the layer build-up, the section material, and the constituent mix each carry their real composition on one record.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new material-assembly modality is one `BimMaterialComposition` union arm reading the next `IfcMaterialSelect` entity; a new assembly-row field is one column on the existing `MaterialLayer`/`MaterialProfile`/`MaterialConstituent` value object; a new `IfcProfileDef` parametric subtype is one `ProfileDefKind` case plus one `DimsOf` arm folding its members onto the canonical `ProfileDims` columns, never a parallel profile-kind enum; a new occurrence binding rides the existing `ProjectAll` fold on one row; never a per-element-class material type, never a second material store, and never a parallel layer/profile/constituent record family.
- Boundary: `BimMaterial` is ONE record discriminated by the `BimMaterialComposition` union — a `LayeredMaterial`/`ProfiledMaterial`/`ConstituentMaterial` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the name-only material is the `Single` union arm, never an `Option<BimMaterialComposition>` escape; the GeometryGym `IfcMaterialLayerSet`/`IfcMaterialLayerSetUsage`/`IfcMaterialProfileSet`/`IfcMaterialProfileSetUsage`/`IfcMaterialConstituentSet`/`IfcMaterial` surface (`.api/api-geometrygym-ifc` material families plus the parameterized-profile-definition family) is consumed as settled vocabulary through the `IfcMaterialSelect` discrimination and a hand-rolled material-assembly reader is the deleted form; the `ProfileSet` arm is the reciprocal ingress of the `Rasm.Materials/Construction/assembly#MATERIAL_ASSIGNMENT` `ProfileSet` egress — the `IfcMaterialProfileSetUsage` `Project` arm (the reciprocal of the `IfcMaterialLayerSetUsage` arm, absent before and the named seam asymmetry that dropped a beam/column profile binding to `FinFail`) folds `ForProfileSet`/`CardinalPoint`/`ReferenceExtent`, and the `MaterialProfile` row's `ProfileDefKind`/`ProfileDims` discriminate the `IfcMaterialProfile.Profile` `IfcProfileDef` subtype (the reciprocal of the `Rasm.Materials/Profiles/steel#STEEL_FAMILY` `SteelClass.IfcSubtype` 9-case axis, the `DoubleL` back-to-back spacing on `ProfileDims.BackToBackMm`) rather than discarding it to a bare `ProfileName` string — a profile-name-only `MaterialProfile` is the deleted form; the surface-style PBR appearance the same `IfcMaterial.HasRepresentation`/`IfcMaterialDefinitionRepresentation` ships is the twin `Semantics/appearance#MATERIAL_APPEARANCE` owner reconciled with `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` at the material-name content key — projecting the appearance here is the named seam violation, and re-minting the OpenPBR algebra in either Bim owner is the named cross-folder drift defect; the `MaterialProfile` section geometry binds the kernel `Rasm` geometry by reference and a RhinoCommon profile field is the named seam violation; the `Exchange/import#IMPORT_RAIL` flat `IfcSemanticModel.MaterialRow(OwnerGlobalId, MaterialName)` stays the wire shape and this owner is the typed promotion the element/query/IDS/property consumers read; a material rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The reciprocal of the Materials Profiles/steel#STEEL_FAMILY SteelClass.IfcSubtype axis: the IfcProfileDef
// parametric subtype the IfcMaterialProfile.Profile runtime type carries, captured on ingress so a steel member
// round-trips its section subtype rather than collapsing to a bare profile-name string (the 9 SteelClass cases fold
// onto these 8 IFC subtypes — I/U/L/T/HSS-rect/HSS-round/circle parametric, double-angle+composite to arbitrary-closed).
[SmartEnum<string>]
public sealed partial class ProfileDefKind {
    public static readonly ProfileDefKind IShape          = new("IfcIShapeProfileDef");
    public static readonly ProfileDefKind UShape          = new("IfcUShapeProfileDef");
    public static readonly ProfileDefKind LShape          = new("IfcLShapeProfileDef");
    public static readonly ProfileDefKind TShape          = new("IfcTShapeProfileDef");
    public static readonly ProfileDefKind RectangleHollow = new("IfcRectangleHollowProfileDef");
    public static readonly ProfileDefKind CircleHollow    = new("IfcCircleHollowProfileDef");
    public static readonly ProfileDefKind Circle          = new("IfcCircleProfileDef");
    public static readonly ProfileDefKind ArbitraryClosed = new("IfcArbitraryClosedProfileDef");

    // Discriminate the IfcProfileDef runtime type — hollow subtypes BEFORE their solid bases (IfcCircleHollowProfileDef
    // : IfcCircleProfileDef, IfcRectangleHollowProfileDef : IfcRectangleProfileDef) so the wall section wins the arm.
    public static ProfileDefKind Of(IfcProfileDef profile) => profile switch {
        IfcIShapeProfileDef          => IShape,
        IfcUShapeProfileDef          => UShape,
        IfcLShapeProfileDef          => LShape,
        IfcTShapeProfileDef          => TShape,
        IfcRectangleHollowProfileDef => RectangleHollow,
        IfcCircleHollowProfileDef    => CircleHollow,
        IfcCircleProfileDef          => Circle,
        _                            => ArbitraryClosed,
    };
}

// The IfcCardinalPointReference profile-placement reference axis — the orientation half of the reciprocal the steel
// round-trip carries through IfcMaterialProfileSetUsage.CardinalPoint; keyed by the IFC ordinal so any reference
// round-trips (Mid the schema default), the 9-point structural grid plus the centroid/shear-centre reference set.
[SmartEnum<int>]
public sealed partial class CardinalPoint {
    public static readonly CardinalPoint Default      = new(0);
    public static readonly CardinalPoint BottomLeft   = new(1);
    public static readonly CardinalPoint BottomMid    = new(2);
    public static readonly CardinalPoint BottomRight  = new(3);
    public static readonly CardinalPoint MidLeft      = new(4);
    public static readonly CardinalPoint Mid          = new(5);
    public static readonly CardinalPoint MidRight     = new(6);
    public static readonly CardinalPoint TopLeft      = new(7);
    public static readonly CardinalPoint TopMid       = new(8);
    public static readonly CardinalPoint TopRight     = new(9);
    public static readonly CardinalPoint Centroid     = new(10);
    public static readonly CardinalPoint BottomCentre = new(11);
    public static readonly CardinalPoint LeftCentre   = new(12);
    public static readonly CardinalPoint RightCentre  = new(13);
    public static readonly CardinalPoint TopCentre    = new(14);
    public static readonly CardinalPoint ShearCentre  = new(15);
    public static readonly CardinalPoint BottomShear  = new(16);
    public static readonly CardinalPoint LeftShear    = new(17);
    public static readonly CardinalPoint RightShear   = new(18);
    public static readonly CardinalPoint TopShear     = new(19);
}

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

// The reciprocal of the Materials Profiles/steel#STEEL_FAMILY SectionDims: the IfcParameterizedProfileDef dimensional
// columns read off the IfcProfileDef subtype on ingress (depth/width/web/flange/fillet folded across the I/U/L/T/HSS
// members); BackToBackMm carries the DoubleL spacing the IfcArbitraryClosedProfileDef double-angle has no parametric
// member for (the Materials IDoubleAngle.BackToBackDistance crossing), None for a single-profile or rolled section.
public sealed record ProfileDims(
    double DepthMm,
    double WidthMm,
    double WebThicknessMm,
    double FlangeThicknessMm,
    double FilletMm,
    Option<double> BackToBackMm);

public sealed record MaterialProfile(
    string Material,
    string ProfileName,
    ProfileDefKind Kind,
    Option<ProfileDims> Dims,
    GeometryHandle Section);

public sealed record MaterialConstituent(
    string Material,
    string Name,
    double Fraction);

public sealed record LayerSetUsage(
    int LayerSetDirection,
    int DirectionSense,
    double OffsetFromReferenceLine);

// The IfcMaterialProfileSetUsage application axis — the reciprocal of LayerSetUsage on the LayerSet arm: the
// CardinalPoint reference axis the linear member's profile sits on plus the ReferenceExtent, so a beam/column
// round-trips its profile placement the way a layered wall round-trips its layer-set direction/sense/offset.
public sealed record ProfileSetUsage(CardinalPoint CardinalPoint, double ReferenceExtent);

[Union]
public partial record BimMaterialComposition {
    partial record Single(string Material);
    partial record LayerSet(Seq<MaterialLayer> Layers, Option<LayerSetUsage> Usage);
    partial record ProfileSet(Seq<MaterialProfile> Profiles, Option<ProfileSetUsage> Usage);
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
            IfcMaterialProfileSetUsage usage =>
                Optional(usage.ForProfileSet)
                    .Map(static set => new BimMaterial(
                        set.Name ?? "",
                        new BimMaterialComposition.ProfileSet(ProfilesOf(set), UsageOf(usage))))
                    .ToFin(new BimFault.ModelRejected("material-profile-set-usage-unbound").ToError()),
            IfcMaterialLayerSet set =>
                FinSucc(new BimMaterial(set.Name ?? "", new BimMaterialComposition.LayerSet(LayersOf(set), None))),
            IfcMaterialProfileSet set =>
                FinSucc(new BimMaterial(set.Name ?? "", new BimMaterialComposition.ProfileSet(ProfilesOf(set), None))),
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
                Optional(profile.Profile).Map(ProfileDefKind.Of).IfNone(ProfileDefKind.ArbitraryClosed),
                Optional(profile.Profile).Bind(DimsOf),
                GeometryHandle.Pending(profile.Profile?.ProfileName ?? "")))
            .ToSeq();

    // The reciprocal of the Materials SectionReader.ReadDims: each IfcParameterizedProfileDef subtype's parametric
    // members fold onto the canonical depth/width/web/flange/fillet columns (hollow subtypes before their solid bases),
    // so the round-trip recovers the section dimensions; the arbitrary-closed double-angle/composite yields its geometry
    // by reference and no parametric dims (None) — its DoubleL back-to-back spacing rides ProfileDims.BackToBackMm.
    static Option<ProfileDims> DimsOf(IfcProfileDef profile) => profile switch {
        IfcIShapeProfileDef i          => Some(new ProfileDims(i.OverallDepth, i.OverallWidth, i.WebThickness, i.FlangeThickness, i.FilletRadius, None)),
        IfcUShapeProfileDef u          => Some(new ProfileDims(u.Depth, u.FlangeWidth, u.WebThickness, u.FlangeThickness, u.FilletRadius, None)),
        IfcTShapeProfileDef t          => Some(new ProfileDims(t.Depth, t.FlangeWidth, t.WebThickness, t.FlangeThickness, t.FilletRadius, None)),
        IfcLShapeProfileDef l          => Some(new ProfileDims(l.Depth, l.Width, l.Thickness, l.Thickness, l.FilletRadius, None)),
        IfcRectangleHollowProfileDef r => Some(new ProfileDims(r.YDim, r.XDim, r.WallThickness, r.WallThickness, r.OuterFilletRadius, None)),
        IfcCircleHollowProfileDef c    => Some(new ProfileDims(c.Radius * 2d, c.Radius * 2d, c.WallThickness, c.WallThickness, 0d, None)),
        IfcCircleProfileDef c          => Some(new ProfileDims(c.Radius * 2d, c.Radius * 2d, 0d, 0d, 0d, None)),
        _                              => None,
    };

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

    // The IfcMaterialProfileSetUsage application axis — the reciprocal of the layer-set-usage overload; the cardinal
    // point resolves through the keyed CardinalPoint smart-enum so any IfcCardinalPointReference ordinal round-trips.
    static Option<ProfileSetUsage> UsageOf(IfcMaterialProfileSetUsage usage) =>
        Some(new ProfileSetUsage(
            CardinalPoint.Get((int)usage.CardinalPoint),
            usage.ReferenceExtent));
}
```

## [03]-[RESEARCH]

- [MATERIAL_SELECT_DISPATCH]: the `IfcRelAssociatesMaterial.RelatingMaterial` `IfcMaterialSelect` runtime-type discrimination — the `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage`/`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet`/`IfcMaterial` cases the `Project` switch folds — grounds against the GeometryGym `IfcMaterialSelect` member surface (`.api/api-geometrygym-ifc` material families) so the `Project` fold discriminates the real material-assembly entity rather than a guessed shape; the `IfcMaterialProfileSetUsage` arm is the reciprocal of the layer-set-usage arm (`ForProfileSet` `IfcMaterialProfileSet`, `CardinalPoint` `IfcCardinalPointReference` default `MID`, `ReferenceExtent` `double` default `NaN`), absent before so a linear member's profile binding fell through to `FinFail`; the `IfcMaterialLayerSet.MaterialLayers` (`LIST<IfcMaterialLayer>`)/`IfcMaterialLayer.Material`/`LayerThickness` (non-nullable double)/`Category`/`IsVentilated` (`IfcLogicalEnum`), `IfcMaterialProfileSet.MaterialProfiles` (`LIST<IfcMaterialProfile>`)/`IfcMaterialProfile.Material`/`Profile.ProfileName`, `IfcMaterialConstituentSet.MaterialConstituents` (`Dictionary<string, IfcMaterialConstituent>` read through `.Values`)/`IfcMaterialConstituent.Material`/`Name`/`Fraction` (non-nullable double), and `IfcMaterialLayerSetUsage.ForLayerSet`/`LayerSetDirection` (`IfcLayerSetDirectionEnum`)/`DirectionSense` (`IfcDirectionSenseEnum`)/`OffsetFromReferenceLine` member spellings are verified against the live GeometryGym decompile, fixing the `ConstituentsOf` fold to read the constituent `Dictionary.Values` and the non-nullable `Fraction`.
- [LAYERED_VOLUME_DERIVE]: the `BimMaterialComposition.LayerSet` `TotalThickness` fold the `Semantics/properties#PROPERTY_SETS` `QuantitySet.Derive` reads for a layered-wall volume takeoff grounds against the buildingSMART base-quantity definitions and the kernel `Rasm` geometry the element binds by reference, so a per-layer `Qto_WallBaseQuantities.NetVolume` derives from the layer thicknesses and the bound geometry's net area rather than re-tessellating; the cross-page contract confirms the property owner reads the layer rows this owner produces, never a second thickness source.
- [APPEARANCE_CONTENT_KEY]: the `BimMaterial.AppearanceKey` material-name content key the twin `Semantics/appearance#MATERIAL_APPEARANCE` owner resolves the `IfcSurfaceStyleRendering` PBR appearance through grounds against the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` single-mint OpenPBR vocabulary, so the construction-material composition and the rendering style meet at the material-name key and the appearance never re-mints the OpenPBR algebra in the Bim folder; the `IfcMaterialDefinitionRepresentation`/`IfcMaterial.HasRepresentation` style-binding traversal stays the appearance owner's concern, confirmed against the catalogued `.api/api-geometrygym-ifc` row 15 surface, never re-cased on this composition page.
- [PROFILE_SUBTYPE_RECIPROCAL]: the `ProfileSet` arm is the reciprocal ingress of the `Rasm.Materials/Profiles/steel#STEEL_FAMILY` `IfcMaterialProfileSet` egress, closing the seam the steel page flagged as the remaining `Rasm.Bim` probe. `ProfileDefKind.Of(IfcProfileDef)` discriminates the `IfcMaterialProfile.Profile` runtime type onto the 8 IFC subtypes the 9-case `SteelClass.IfcSubtype` axis folds onto (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcTShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcCircleProfileDef`/`IfcArbitraryClosedProfileDef`, the hollow subtypes ordered before their `IfcRectangleProfileDef`/`IfcCircleProfileDef` solid bases), every subtype verified against the live GeometryGym decompile under the `IfcParameterizedProfileDef` base (the arbitrary-closed under `IfcProfileDef` with its `OuterCurve` `IfcCurve`). `DimsOf` reads each parametric subtype's members — `IfcIShapeProfileDef.OverallDepth`/`OverallWidth`/`WebThickness`/`FlangeThickness`/`FilletRadius`, `IfcUShapeProfileDef`/`IfcTShapeProfileDef.Depth`/`FlangeWidth`/`WebThickness`/`FlangeThickness`/`FilletRadius`, `IfcLShapeProfileDef.Depth`/`Width`/`Thickness`/`FilletRadius`, `IfcRectangleHollowProfileDef.XDim`/`YDim`/`WallThickness`/`OuterFilletRadius`, `IfcCircleHollowProfileDef.Radius`/`WallThickness` — onto the canonical `ProfileDims` columns reciprocal to the steel `SectionDims`, the `DoubleL` back-to-back spacing the `IfcArbitraryClosedProfileDef` cannot express parametrically riding `ProfileDims.BackToBackMm` (the `Rasm.Materials` `IDoubleAngle.BackToBackDistance` crossing). The `IfcMaterialProfileSetUsage.CardinalPoint` resolves through the keyed `CardinalPoint` smart-enum mirroring the full 20-member `IfcCardinalPointReference` ordinal set (`DEFAULT`..`TOPSHEAR`, default `MID`), so the per-family cardinal-point/orientation the steel egress writes round-trips on the `ProfileSetUsage`.
