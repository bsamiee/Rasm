# [BIM_APPEARANCE]

The host-neutral surface-appearance owner: one `BimAppearance` record projecting the IFC `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading` presentation style onto a host-neutral PBR appearance — a scene-linear `AppearanceColor` triple per channel, the `ReflectanceModel` projection of `IfcReflectanceMethodEnum`, the shading transparency and the optical refraction/lighting coefficients, and the `MaterialId` content key that reconciles the appearance with the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner at the seam. This owner promotes the IFC presentation graph (`IfcStyledItem` → `IfcSurfaceStyle` → `IfcSurfaceStyleRendering`) the `Exchange/import#IMPORT_RAIL` traverses through `BaseClassIfc.Extract<IfcStyledItem>()` and the `IfcMaterialDefinitionRepresentation.HasRepresentation` material-style binding onto one typed appearance the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial` carries as an `Option<BimAppearance>` and the `Exchange/export#EXPORT_RAIL` GLB `KHR_materials_pbrSpecularGlossiness`/`pbrMetallicRoughness` author reads — never a per-style appearance class family and never a second material-name string. The split from `Semantics/composition#MATERIAL_COMPOSITION` is deliberate: composition owns the layered/profiled/constituent construction-material algebra, appearance owns the rendering style, so the appearance record reconciles with `Rasm.Materials` at the content-key seam without coupling the construction build-up to the PBR vector. The page is HOST-NEUTRAL — it carries no `Rhino.Geometry`, no `Unicolour`, and no OpenPBR construction; the `AppearanceColor` is a portable scene-linear triple the `Exchange/wire#WIRE_PROJECTION` peers decode and the `Rasm.Materials` owner reads at the key, and a re-mint of the OpenPBR parameter algebra here is the named cross-folder seam violation.

## [1]-[INDEX]

- [1]-[MATERIAL_APPEARANCE]: the `BimAppearance` host-neutral surface-style record, the `AppearanceColor` scene-linear color value object, the `ReflectanceModel` `[SmartEnum<string>]` projection of `IfcReflectanceMethodEnum`, the `BimAppearance.Project` import fold over the IFC presentation graph, the `BimAppearance.Author` export round-trip, and the `MaterialId` content-key seam to the `Rasm.Materials` OpenPBR owner.

## [2]-[MATERIAL_APPEARANCE]

- Owner: `BimAppearance` the host-neutral surface-appearance record carrying the projected PBR style — the `SurfaceColour` shading base, the `DiffuseColour`/`SpecularColour` rendering channels, the `Transparency`/`RefractionIndex` optical coefficients, the `ReflectanceModel` shading-model row, and the `MaterialKey` content key naming the `Rasm.Materials` row the appearance reconciles with; `AppearanceColor` the scene-linear `[ComplexValueObject]` RGB triple over three kernel `UnitInterval` channels carrying its clipped `Hex` preview; `ReflectanceModel` the `[SmartEnum<string>]` projection of the ten `IfcReflectanceMethodEnum` rows (`BLINN`/`FLAT`/`GLASS`/`MATT`/`METAL`/`MIRROR`/`PHONG`/`PLASTIC`/`STRAUSS`/`NOTDEFINED`) carrying its metalness-bias hint so a downstream PBR consumer maps a `METAL`/`MIRROR` row toward a metallic surface and a `MATT`/`PLASTIC` row toward a dielectric without re-reading the IFC enum — one closed style family, three cases of one concept, never a per-style class.
- Cases: `ReflectanceModel` arms `Blinn`/`Flat`/`Glass`/`Matt`/`Metal`/`Mirror`/`Phong`/`Plastic`/`Strauss`/`NotDefined` (10), the IFC4.3 `IfcReflectanceMethodEnum` partition keyed on the schema constant; `AppearanceColor` is one value object, never a per-channel type; `BimAppearance` is one record, never a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` sibling triple — the texture/lighting/refraction breadth is a defaulted column on the one record, not a parallel case.
- Entry: `BimAppearance.Project(IfcStyledItem styledItem)` folds the IFC presentation graph — it reads the `IfcStyledItem.Styles` element set, selects the `IfcSurfaceStyle` whose `Side` is `BOTH`/`POSITIVE`, walks its `Styles` to the most specific `IfcSurfaceStyleRendering` (falling back to the base `IfcSurfaceStyleShading` when no rendering style is present), and projects the `SurfaceColour`/`DiffuseColour`/`SpecularColour` `IfcColourRgb` triples onto `AppearanceColor`, the `Transparency` onto the opacity complement, the `IfcSurfaceStyleRefraction.RefractionIndex` onto the optical index, and the `ReflectanceMethod` onto the `ReflectanceModel` row; `Fin<T>` aborts on a malformed presentation graph (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()`. `BimAppearance.Author(DatabaseIfc database)` mints the inverse — it constructs the `IfcColourRgb` triples through the `database` factory, builds the `IfcSurfaceStyleRendering`, wraps it in an `IfcSurfaceStyle` and an `IfcStyledItem` for the material-definition representation, and returns the styled item the `Exchange/export#EXPORT_RAIL` binds. `BimAppearance.OpenPbrKey(string materialId)` resolves the content-key seam — it stamps the `MaterialKey` `family.name` string onto the appearance so a `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` `MaterialWire` reconciles by `family.name` without re-deriving the OpenPBR vector and without the `Rasm.Materials.MaterialId` value object crossing into this host-neutral owner.
- Auto: `Project` traverses through `BaseClassIfc.Extract<IfcStyledItem>()` on the owning representation, reads `IfcSurfaceStyle.Styles` for the rendering-versus-shading discrimination, and folds the colour triples with the IFC channel precedence (a rendering `DiffuseColour` overrides the shading `SurfaceColour` for the diffuse base, the `SurfaceColour` is the diffuse fallback); the `ReflectanceModel.FromIfc` row resolves the enum constant onto the smart-enum row carrying the metalness-bias hint, defaulting `NOTDEFINED` to the `Plastic` dielectric bias; `AppearanceColor.FromIfc` clamps each `IfcColourRgb` channel onto the `UnitInterval` and derives the clipped `Hex` preview byte string so a web swatch reads the appearance without re-deriving the working space, the linear triple the shading truth and the hex the preview; `OpenPbrKey` stamps the content key so the cross-folder `Rasm.Materials` reconciliation reads the appearance by `MaterialId`, never re-minting the OpenPBR parameter groups in this owner.
- Receipt: the typed `BimAppearance` is the appearance evidence the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial` carries as an `Option<BimAppearance>`, the `Exchange/export#EXPORT_RAIL` GLB material author reads for the PBR channels, and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` owner reconciles at the `MaterialId` key; the `ReflectanceModel` metalness-bias hint is the typed mapping a downstream metallic/dielectric author folds, never a re-read of the IFC enum string.
- Packages: GeometryGymIFC_Core (`IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRendering`/`IfcSurfaceStyleRefraction`/`IfcColourRgb`/`IfcReflectanceMethodEnum`/`IfcMaterialDefinitionRepresentation` — surface-style and presentation entities, `.api/api-geometrygym-ifc` rows 4-5/16 and presentation rows 3-8/13-16), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[ComplexValueObject]`), LanguageExt.Core (`Fin`/`Option`/`Seq`), Rasm (the kernel `UnitInterval` value object the `AppearanceColor` channels compose); the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner is reconciled at the `MaterialId` `family.name` content-key seam alone — no `Rasm.Materials` assembly reference crosses into this host-neutral leaf, the key is a bare string.
- Growth: a new reflectance row is one `ReflectanceModel` smart-enum entry carrying its schema constant and metalness-bias hint; a new optical coefficient (an `IfcSurfaceStyleLighting` ambient/transmission term) is one defaulted column on `BimAppearance`; a new color preview is one derivation on `AppearanceColor`; the texture style (`IfcSurfaceStyleWithTextures`) lands as a defaulted `Option<AppearanceTexture>` column on the one record, never a `TexturedAppearance` sibling — never a per-style appearance class and never a second material-name string.
- Boundary: there is ONE appearance record — a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` class family is the deleted form, the one `BimAppearance` carries every channel with the absent-channel arms defaulted; the projection rides the GeometryGym `IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleRendering`/`IfcColourRgb` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` presentation rows) — a hand-rolled STEP presentation-style reader is the deleted form, the traversal rides `BaseClassIfc.Extract<IfcStyledItem>()` and `IfcMaterialDefinitionRepresentation.HasRepresentation`; the appearance is HOST-NEUTRAL — a `Rhino.Geometry` color, a `System.Drawing.Color`, or a `Unicolour` object crossing this signature is the named host-coupling defect, the `AppearanceColor` carries only the scene-linear triple and the clipped hex; the OpenPBR reconciliation rides the `MaterialId` content key alone — a re-mint of the `Rasm.Materials/Appearance/bsdf#OPENPBR_SLAB` `OpenPbrSurface` vector, the conductor-IOR table, or the OpenPBR parameter groups in this owner is the named cross-folder seam violation, the appearance produces the IFC-derived PBR style and names the material row, never re-deriving the branch OpenPBR algebra; the `ReflectanceModel` keys its `FromIfc` resolution through the smart-enum `Get`, no `switch` over the enum string at call sites; faults route through the `Fin` rail and a captured GeometryGym presentation-graph fault enters through the `BimFault.ModelRejected` funnel, never an exception across a domain signature.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Vectors;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class ReflectanceModel {
    public static readonly ReflectanceModel Blinn = new("BLINN", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Flat = new("FLAT", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Glass = new("GLASS", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Matt = new("MATT", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Metal = new("METAL", MetalnessBias: 1.0);
    public static readonly ReflectanceModel Mirror = new("MIRROR", MetalnessBias: 1.0);
    public static readonly ReflectanceModel Phong = new("PHONG", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Plastic = new("PLASTIC", MetalnessBias: 0.0);
    public static readonly ReflectanceModel Strauss = new("STRAUSS", MetalnessBias: 0.5);
    public static readonly ReflectanceModel NotDefined = new("NOTDEFINED", MetalnessBias: 0.0);

    public double MetalnessBias { get; }

    public static ReflectanceModel FromIfc(IfcReflectanceMethodEnum method) =>
        TryGet(method.ToString()).IfNone(NotDefined);

    public IfcReflectanceMethodEnum ToIfc() =>
        Enum.Parse<IfcReflectanceMethodEnum>(Key);
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct AppearanceColor {
    public UnitInterval Red { get; }
    public UnitInterval Green { get; }
    public UnitInterval Blue { get; }

    public static AppearanceColor FromIfc(IfcColourRgb colour) =>
        Create(
            UnitInterval.Create(Math.Clamp(colour.Red, 0.0, 1.0)),
            UnitInterval.Create(Math.Clamp(colour.Green, 0.0, 1.0)),
            UnitInterval.Create(Math.Clamp(colour.Blue, 0.0, 1.0)));

    public IfcColourRgb ToIfc(DatabaseIfc database) =>
        new(database, Red.Value, Green.Value, Blue.Value);

    public string Hex =>
        $"#{(byte)Math.Round(Red.Value * 255.0):X2}{(byte)Math.Round(Green.Value * 255.0):X2}{(byte)Math.Round(Blue.Value * 255.0):X2}";

    public static readonly AppearanceColor Grey = Create(UnitInterval.Create(0.5), UnitInterval.Create(0.5), UnitInterval.Create(0.5));
}

public sealed record BimAppearance(
    AppearanceColor SurfaceColour,
    AppearanceColor DiffuseColour,
    AppearanceColor SpecularColour,
    double Transparency,
    double RefractionIndex,
    ReflectanceModel Reflectance,
    Option<string> MaterialKey) {

    public double Opacity => 1.0 - Math.Clamp(Transparency, 0.0, 1.0);

    public BimAppearance OpenPbrKey(string materialId) => this with { MaterialKey = Some(materialId) };

    public static Fin<BimAppearance> Project(IfcStyledItem styledItem) =>
        styledItem.Styles
            .Choose(static style => style is IfcSurfaceStyle surface ? Some(surface) : Option<IfcSurfaceStyle>.None)
            .Filter(static surface => surface.Side is IfcSurfaceSide.BOTH or IfcSurfaceSide.POSITIVE)
            .HeadOrNone()
            .Map(ProjectStyle)
            .ToFin(new BimFault.ModelRejected($"surface-style-miss:{styledItem.StepId}").ToError());

    static BimAppearance ProjectStyle(IfcSurfaceStyle surface) {
        var rendering = surface.Styles
            .Choose(static s => s is IfcSurfaceStyleRendering r ? Some(r) : Option<IfcSurfaceStyleRendering>.None)
            .HeadOrNone();
        var shading = surface.Styles
            .Choose(static s => s is IfcSurfaceStyleShading sh ? Some(sh) : Option<IfcSurfaceStyleShading>.None)
            .HeadOrNone();
        var refraction = surface.Styles
            .Choose(static s => s is IfcSurfaceStyleRefraction rf ? Some(rf) : Option<IfcSurfaceStyleRefraction>.None)
            .HeadOrNone();

        var surfaceColour = shading
            .Map(static sh => AppearanceColor.FromIfc(sh.SurfaceColour))
            .IfNone(AppearanceColor.Grey);
        var diffuse = rendering
            .Bind(static r => Optional(r.DiffuseColour as IfcColourRgb))
            .Map(AppearanceColor.FromIfc)
            .IfNone(surfaceColour);
        var specular = rendering
            .Bind(static r => Optional(r.SpecularColour as IfcColourRgb))
            .Map(AppearanceColor.FromIfc)
            .IfNone(AppearanceColor.Grey);
        var transparency = shading.Map(static sh => sh.Transparency).IfNone(0.0);
        var index = refraction.Map(static rf => rf.RefractionIndex).IfNone(1.0);
        var reflectance = rendering
            .Map(static r => ReflectanceModel.FromIfc(r.ReflectanceMethod))
            .IfNone(ReflectanceModel.NotDefined);

        return new BimAppearance(surfaceColour, diffuse, specular, transparency, index, reflectance, Option<string>.None);
    }

    public IfcStyledItem Author(DatabaseIfc database) {
        var rendering = new IfcSurfaceStyleRendering(SurfaceColour.ToIfc(database)) {
            Transparency = Transparency,
            ReflectanceMethod = Reflectance.ToIfc(),
            DiffuseColour = DiffuseColour.ToIfc(database),
            SpecularColour = SpecularColour.ToIfc(database),
        };
        var style = new IfcSurfaceStyle(rendering) { Side = IfcSurfaceSide.BOTH };
        return new IfcStyledItem(style);
    }
}
```

## [3]-[RESEARCH]

- [SURFACE_STYLE_MEMBERS]: the GeometryGym `IfcSurfaceStyleShading.SurfaceColour`/`Transparency`, the `IfcSurfaceStyleRendering.DiffuseColour`/`SpecularColour`/`ReflectanceMethod`, the `IfcSurfaceStyleRefraction.RefractionIndex`, the `IfcColourRgb.Red`/`Green`/`Blue` channels, and the `IfcSurfaceStyle.Side`/`Styles` traversal member spellings confirm against the `.api/api-geometrygym-ifc` presentation rows (`IfcSurfaceStyle` row 3, `IfcSurfaceStyleShading` row 4, `IfcSurfaceStyleRendering` row 5, `IfcSurfaceStyleRefraction` row 8, `IfcColourRgb` row 16) and the `IfcStyledItem` → `IfcSurfaceStyle` → `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading` import traversal the catalogue notes through `BaseClassIfc.Extract<IfcStyledItem>()` — verified against the live GeometryGym decompile: `IfcSurfaceStyleRendering(IfcColourRgb surfaceColour)` is the sole single-argument constructor with `Transparency` (inherited from `IfcSurfaceStyleShading`), `ReflectanceMethod`, and the `IfcColourOrFactor`-typed `DiffuseColour`/`SpecularColour` as settable properties (the read side casts `IfcColourOrFactor` to `IfcColourRgb`); `IfcColourRgb(DatabaseIfc db, double red, double green, double blue)` carries no name argument; `IfcSurfaceStyle(IfcSurfaceStyleShading shading)` takes the rendering style with `Side` settable; and `IfcStyledItem(IfcStyleAssignmentSelect style)` takes the style directly, `IfcSurfaceStyle` satisfying `IfcStyleAssignmentSelect` — so the import projection and the export round-trip author the real presentation graph.
- [OPENPBR_CONTENT_KEY]: the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` `MaterialWire` `MaterialId` `family.name` content key the `BimAppearance.MaterialKey` reconciles against grounds the cross-folder seam — the appearance stamps the key and the `Rasm.Materials` owner mints the OpenPBR vector once, so a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row reconcile by the shared `MaterialId` without this owner re-deriving the `bsdf#OPENPBR_SLAB` `OpenPbrSurface` parameter groups or the conductor-IOR table; the `ReflectanceModel` metalness-bias hint is the IFC-side bias a downstream `Rasm.Materials` mapping reads to seed `BaseMetalness` when an authored OpenPBR row is absent, never a re-mint of the branch OpenPBR algebra in `Rasm.Bim`.
