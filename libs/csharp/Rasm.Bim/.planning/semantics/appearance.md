# [BIM_APPEARANCE]

The IFC surface-style PROJECTOR lowering the live GeometryGym `IfcStyledItem`/`IfcSurfaceStyle` presentation graph onto the `Rasm.Element` seam `Graph/element#NODE_MODEL` `Node.Appearance` carrying the neutral `AppearanceSummary`: `AppearanceProjection.Project` extracts the front-face `IfcSurfaceStyle`, folds its `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRefraction` element selects onto one neutral PBR vector (scene-linear base colour + metalness + roughness + opacity + a `Transmissive` refractive flag DISTINCT from opacity, so an opaque-alpha glass still carries transmission), and mints the content-keyed seam `Node.Appearance` the `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge into `element.Appearance`. The seam OWNS the `AppearanceSummary` PBR vocabulary and the `Projection/address#CONTENT_ADDRESS` content-key derivation; this page owns ONLY the GeometryGym discrimination that fills it and the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster, never re-declaring an appearance record. The projector is BIDIRECTIONAL: `AppearanceProjection.Author` is the inverse half the `Projection/semantic#IFC_EGRESS` `Emit` composes per `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` (diffuse/specular/highlight/reflectance-method/transparency) plus, for a transmissive appearance, an `IfcSurfaceStyleRefraction` combined in one `IfcSurfaceStyle`, so the surface style round-trips.

IFC presentation colours are display-referred sRGB; the projector lowers each channel to scene-linear through the sRGB EOTF (a pure host-neutral transfer, IEC 61966-2-1) so the seam `BaseColor` aligns with the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` scene-linear convention, and the egress encodes back through the inverse OETF — the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here. The appearance reconciles with the `Rasm.Materials` OpenPBR owner at the content key: the `AppearanceSummary.AppearanceKey` is the kernel seed-zero `XxHash128` over the neutral PBR vector — the shared seam-owned `AppearanceSummary.Of` derivation the `Rasm.Materials` `ComponentProjector` composes identically, so a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key, the `Rasm.Materials` owner the authority for the full BSDF and this page producing only the IFC-derived neutral summary. A re-mint of the `surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder seam violation.

The page is HOST-NEUTRAL — no `Rhino.Geometry`, no `Unicolour`, no `System.Drawing.Color` crosses a signature (the GeometryGym `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members are the deleted form, only the `Red`/`Green`/`Blue` doubles + the `(DatabaseIfc, double, double, double)` ctor are read/authored). The retired `BimAppearance`/`AppearanceColor` records and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE: appearance is element-scoped — a seam `Node.Appearance` the `Object` carries through the `Associate` edge, never a record nested in the retired `BimMaterial`, mirroring how the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` lowers onto the seam `Classification` value and `Semantics/composition#MATERIAL_COMPOSITION` onto the seam `Node.Material`. A malformed presentation graph rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (`surface-style-miss`), lifted BARE onto the `Fin` rail.

## [01]-[INDEX]

- [01]-[APPEARANCE_PROJECTION]: `AppearanceProjection.Project` the `IfcStyledItem`→seam `Node.Appearance` ingress fold over the presentation graph, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster carrying the typed PBR bias (`Metalness`/`RoughnessHint`/`Transmissive`) plus the `ForPbr` reverse classifier, the sRGB `Linearize`/`Encode` transfer pair, the `AppearanceKey` content-key derivation shared with `Rasm.Materials`, and the inverse `AppearanceProjection.Author` egress re-authoring the `IfcSurfaceStyleRendering`+`IfcSurfaceStyleRefraction` surface style the `Projection/semantic#IFC_EGRESS` `Emit` composes per `Object` node.

## [02]-[APPEARANCE_PROJECTION]

- Owner: `AppearanceProjection` the static BIDIRECTIONAL GeometryGym↔seam surface-style projector — the `Project` ingress folding one `IfcStyledItem`'s front-face `IfcSurfaceStyle` into one content-keyed seam `Node.Appearance`, and the `Author` egress re-authoring a seam `AppearanceSummary` back onto the GeometryGym presentation graph the `Emit` composes; `ReflectanceModel` the `[SmartEnum<string>]` IFC reflectance-method roster the projection folds the method onto without re-reading the enum string. The seam owns the `AppearanceSummary` neutral PBR record and its key derivation — the `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress` — and this page declares neither, composing the seam vocabulary, mapping the GeometryGym presentation entities onto it and back.
- Cases: `ReflectanceModel` arms `Blinn`/`Flat`/`Glass`/`Matt`/`Metal`/`Mirror`/`Phong`/`Plastic`/`Strauss`/`NotDefined` (10), the full IFC4.3 `IfcReflectanceMethodEnum` partition keyed on the schema constant, each carrying its typed PBR bias — `Metalness` (`METAL`/`MIRROR` → 1.0, `STRAUSS` → 0.5, every dielectric → 0.0), `RoughnessHint` (the fallback when the style supplies no `IfcSpecularHighlight` — `MIRROR` → 0.0, `MATT`/`FLAT` → 1.0), and `Transmissive` (`GLASS` → true); the appearance is the seam's ONE `AppearanceSummary`, never a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` sibling triple and never a Bim `BimAppearance`/`AppearanceColor` record beside the seam.
- Entry: `AppearanceProjection.Project(IfcStyledItem styledItem, double tolerance, Op key)` is the per-styled-item leaf the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes from its per-`Object` representation walk — a dedicated appearance fold (the sibling of `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection.MaterialEdges`) that discovers each object's styled items through the GeometryGym `IfcRepresentationItem.StyledByItem` inverse, calls `Project`, dedups the minted node by id, and authors the `Object`→`Appearance` `Associate` edge against the object's rooted `NodeId` with `MaterialUsage.None` (the appearance `Associate` edge carries no material usage); a `Material`-scoped style instead rides the `Rasm.Materials` `ComponentProjector`, which authors its own `element→appearance` edge — extracting the front-face `IfcSurfaceStyle` (`Side` `BOTH`/`POSITIVE`) through `BaseClassIfc.Extract<IfcSurfaceStyle>()` (version-agnostic: it flattens an IFC2x3 `IfcPresentationStyleAssignment` wrapper), folding its element selects onto the neutral PBR vector, and minting the content-keyed seam `Node.Appearance`; `Fin<T>` aborts on a presentation graph carrying no front-face surface style (`Model/faults#FAULT_BAND` `BimFault.ModelRejected` `surface-style-miss`), lifted BARE (the `Expected`-derived band is the `Error`, no `.ToError()` hop). `AppearanceProjection.Author(DatabaseIfc db, AppearanceSummary summary)` is the egress entry the `Emit` composes per `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` from the neutral summary and, for a transmissive summary, the `IfcSurfaceStyleRefraction`, combined in one `IfcSurfaceStyle`; total (authoring from a valid summary cannot fail), returning the `IfcStyledItem` the `Emit` binds onto the representation.
- Auto: `Project` reads the front-face `IfcSurfaceStyle.Styles` element selects — the `IfcSurfaceStyleRendering` (an `IfcSurfaceStyleShading` subtype, so it supplies the inherited `SurfaceColour`/`Transparency` plus the `DiffuseColour`/`ReflectanceMethod`/`SpecularHighlight` rendering channels), a bare `IfcSurfaceStyleShading` fallback (colour/transparency only), and the `IfcSurfaceStyleRefraction` optical signal — and folds the channel precedence: the rendering `DiffuseColour` overrides the shading `SurfaceColour` for the base colour (each `IfcColourRgb` channel lowered to scene-linear through `Linearize`, defaulting grey when absent), the opacity is the transparency complement (a `double.NaN` transparency, the GeometryGym unset sentinel, defaulting to opaque), the metalness is `ReflectanceModel.FromIfc(ReflectanceMethod).Metalness`, the roughness reads the `IfcSpecularHighlightSelect` (`IfcSpecularRoughness` directly as a [0,1] roughness, `IfcSpecularExponent` converted through the Phong `α = √(2/(n+2))`) and falls back to the row's `RoughnessHint`, and the transmissive flag is the REFRACTIVE signal (the row's `Transmissive` GLASS method or a present `IfcSurfaceStyleRefraction`, NEVER a sub-unit opacity — IFC `Transparency` is the alpha/opacity channel, physically distinct from transmission), PERSISTED on the summary apart from opacity (so an opaque-alpha refractive glass keeps its transmission, the round-trip symmetric with the egress `IfcSurfaceStyleRefraction`); the seam-owned `AppearanceSummary.Of` then derives the `AppearanceKey` as the kernel seed-zero `XxHash128` over the canonical PBR bytes (base + metalness + roughness + opacity + transmissive — the seam `CanonicalWriter` → `ContentAddress.Of`, the ONE hasher the `Rasm.Materials` owner composes identically, this page assembling no key bytes of its own) and `Mint` content-keys the seam `Node.Appearance` whose id is the seam `Node.ToCanonicalBytes` (id excluded), so two structurally-identical appearances dedup to one node. `Author` encodes the scene-linear base colour back to display sRGB through `Encode`, picks the `IfcReflectanceMethodEnum` from the neutral PBR through `ReflectanceModel.ForPbr`, tints the specular from the base colour for a metal and reflects neutral for a dielectric, and authors `IfcSpecularRoughness` from the summary roughness.
- Receipt: the seam `Node.Appearance` is the appearance evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands (authoring the `Object`→`Appearance` `Associate` edge) and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads into `element.Appearance` (an `Option<AppearanceSummary>` a consumer reads flat), the `Exchange/export#EXPORT_RAIL` GLB material author reads for the `KHR_materials_pbrMetallicRoughness` channels plus the `KHR_materials_transmission` extension off the `Transmissive` bit (distinct from the base-colour alpha the opacity drives), and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner reconciles at the `AppearanceKey` content key; the `ReflectanceModel` typed PBR bias is the IFC-side mapping a downstream metallic/dielectric author folds, never a re-read of the IFC enum string.
- Packages: GeometryGymIFC_Core (`IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRendering`/`IfcSurfaceStyleRefraction`/`IfcSpecularRoughness`/`IfcSpecularExponent`/`IfcColourRgb`/`IfcReflectanceMethodEnum`/`IfcSurfaceSide` — `.api/api-geometrygym-ifc` presentation rows 01/03-08/13/16 + reflectance/side enum rows 04-05, the `IfcSurfaceStyleRendering` PBR-channel + `IfcSpecularHighlightSelect` member surface decompile-confirmed), Rasm.Element (the seam `Node.Appearance`/`AppearanceSummary` + its seam-owned `AppearanceSummary.Of` content-key factory/`NodeId.Content`/`Node.ToCanonicalBytes` — the page composes the factory, never a local `CanonicalWriter`/`ContentAddress` key assembly), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Fin`/`Option`/`Iterable`), Rasm (the kernel `Op` op-key + the seed-zero `XxHash128` content hash the seam `AppearanceSummary.Of`/`NodeId.Content` compose); the `Rasm.Materials` OpenPBR owner is reconciled at the `AppearanceKey` content key alone — no `Rasm.Materials` assembly reference crosses into this host-neutral leaf.
- Growth: a new IFC reflectance method is one `ReflectanceModel` row carrying its schema constant and typed PBR bias; a new presentation-channel read (the `IfcSurfaceStyleLighting` `DiffuseTransmissionColour`/`DiffuseReflectionColour`/`TransmissionColour`/`ReflectanceColour` four-colour coefficients, or the `IfcSurfaceStyleWithTextures` texture style) is one more `Styles` element-select arm in the fold collapsing onto the neutral summary; the seam `AppearanceSummary` absorbs the neutral vector with no seam edit; never a per-style appearance class and never a Bim appearance record beside the seam node.
- Boundary: the appearance model is the seam `Node.Appearance` + `AppearanceSummary` and a Bim `BimAppearance`/`AppearanceColor`/`RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` re-declaration is the deleted form — the seam owns the neutral PBR record, this page owns only the GeometryGym discrimination that fills it, so the appearance lowers onto the one seam summary with the absent channels defaulted, never a parallel per-style class; the retired `BimAppearance` record and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, appearance being element-scoped (a seam `Node.Appearance` the `Object` carries through the `Associate` edge the `Bake` fold reads into `element.Appearance`), never a record nested in a material; the projection rides the GeometryGym `IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleRendering`/`IfcColourRgb` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` presentation rows) through `BaseClassIfc.Extract<IfcSurfaceStyle>()`, and a hand-rolled STEP presentation-style reader is the deleted form; the page is HOST-NEUTRAL — a `Rhino.Geometry` colour, a `System.Drawing.Color` (the `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members), or a `Unicolour` object crossing a signature is the named host-coupling defect, only the `Red`/`Green`/`Blue` scene-linear doubles cross; IFC presentation colour is display-referred sRGB lowered to scene-linear through the `Linearize` EOTF and encoded back through the `Encode` OETF — a raw-channel pass-through that calls the unlinearized value "scene-linear" is the deleted form, and the working-space PRIMARIES conversion stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here; the OpenPBR reconciliation rides the `AppearanceKey` content key — a re-mint of the `Rasm.Materials/Appearance/surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder seam violation; the rich IFC rendering channels (`SpecularColour`/`TransmissionColour`/`ReflectionColour`/`DiffuseTransmissionColour`, the `IfcSurfaceStyleRefraction` IOR/dispersion MAGNITUDE, the `IfcSurfaceStyleLighting` coefficients) are NOT retained by the thin seam summary — a Bim-imported style collapses DELIBERATELY to base colour/metalness/roughness/opacity + a transmissive flag (the refraction PRESENCE is kept as the transmissive bit DISTINCT from the opacity/alpha channel so opaque-alpha glass round-trips, its IOR/dispersion magnitude dropped; the seam's chosen shape, lossy by design, NOT an unintended gap), and full specular/reflection/transmission-colour + dispersion BSDF fidelity exists ONLY when the `Rasm.Materials` owner AUTHORS the appearance and holds the lobe graph keyed by the shared `AppearanceKey` (claiming a Bim round-trip preserves the dropped colour/IOR channels is the deleted overclaim); the `ReflectanceModel` keys its `FromIfc` resolution through the smart-enum `TryGet`, no `switch` over the enum string at call sites; faults route through the `Fin` rail and lift BARE (the `Expected`-derived `BimFault.ModelRejected` IS the `Error`, never a `.ToError()` hop and never an exception across a domain signature).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The IFC reflectance-method roster lowering IfcReflectanceMethodEnum onto a typed PBR bias the projection folds
// WITHOUT re-reading the enum string. Each row carries its metalness, its roughness fallback (used when the style
// supplies no IfcSpecularHighlight), and whether the method is transmissive — the IFC reflectance vocabulary's PBR
// meaning captured once as POLICY_VALUES, so import folds the method onto metalness/roughness and ForPbr picks the
// method back from a neutral PBR vector at egress. Ten cases = the full IFC4.3 IfcReflectanceMethodEnum partition.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ReflectanceModel {
    public static readonly ReflectanceModel Blinn      = new("BLINN",      IfcReflectanceMethodEnum.BLINN,      metalness: 0.0, roughnessHint: 0.5,  transmissive: false);
    public static readonly ReflectanceModel Flat       = new("FLAT",       IfcReflectanceMethodEnum.FLAT,       metalness: 0.0, roughnessHint: 1.0,  transmissive: false);
    public static readonly ReflectanceModel Glass      = new("GLASS",      IfcReflectanceMethodEnum.GLASS,      metalness: 0.0, roughnessHint: 0.05, transmissive: true);
    public static readonly ReflectanceModel Matt       = new("MATT",       IfcReflectanceMethodEnum.MATT,       metalness: 0.0, roughnessHint: 1.0,  transmissive: false);
    public static readonly ReflectanceModel Metal      = new("METAL",      IfcReflectanceMethodEnum.METAL,      metalness: 1.0, roughnessHint: 0.3,  transmissive: false);
    public static readonly ReflectanceModel Mirror     = new("MIRROR",     IfcReflectanceMethodEnum.MIRROR,     metalness: 1.0, roughnessHint: 0.0,  transmissive: false);
    public static readonly ReflectanceModel Phong      = new("PHONG",      IfcReflectanceMethodEnum.PHONG,      metalness: 0.0, roughnessHint: 0.4,  transmissive: false);
    public static readonly ReflectanceModel Plastic    = new("PLASTIC",    IfcReflectanceMethodEnum.PLASTIC,    metalness: 0.0, roughnessHint: 0.3,  transmissive: false);
    public static readonly ReflectanceModel Strauss    = new("STRAUSS",    IfcReflectanceMethodEnum.STRAUSS,    metalness: 0.5, roughnessHint: 0.4,  transmissive: false);
    public static readonly ReflectanceModel NotDefined = new("NOTDEFINED", IfcReflectanceMethodEnum.NOTDEFINED, metalness: 0.0, roughnessHint: 0.5,  transmissive: false);

    public IfcReflectanceMethodEnum Method { get; }
    public double Metalness { get; }
    public double RoughnessHint { get; }
    public bool Transmissive { get; }

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape): the row carries its typed IfcReflectanceMethodEnum so ToIfc is a field read, never
    // an Enum.Parse over the key, plus the PBR bias the import fold lowers the method onto without re-reading the enum.
    private ReflectanceModel(string key, IfcReflectanceMethodEnum method, double metalness, double roughnessHint, bool transmissive) : this(key) =>
        (Method, Metalness, RoughnessHint, Transmissive) = (method, metalness, roughnessHint, transmissive);

    public static ReflectanceModel FromIfc(IfcReflectanceMethodEnum method) =>
        TryGet(method.ToString(), out ReflectanceModel? row) && row is { } resolved ? resolved : NotDefined;

    public IfcReflectanceMethodEnum ToIfc() => Method;

    // The reverse classifier the egress picks the IFC method from a neutral PBR vector through — a transmissive
    // surface authors GLASS, a metallic mirror MIRROR, a rough metal METAL, every dielectric the PLASTIC default.
    public static ReflectanceModel ForPbr(double metallic, double roughness, bool transmissive) =>
        transmissive          ? Glass
        : metallic >= 0.5     ? (roughness <= 0.05 ? Mirror : Metal)
        : Plastic;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The one GeometryGym<->seam surface-style projector: IfcStyledItem -> seam Node.Appearance carrying the neutral
// AppearanceSummary, and back. The seam OWNS the AppearanceSummary PBR vocabulary + the content-key derivation;
// this projector discriminates the IFC presentation graph and folds it onto the neutral vector, never re-minting
// the Rasm.Materials OpenPBR vector / conductor-IOR table / slab algebra (the named cross-folder seam violation).
public static class AppearanceProjection {
    public static Fin<Node.Appearance> Project(IfcStyledItem styledItem, double tolerance, Op key) =>
        styledItem.Extract<IfcSurfaceStyle>().AsIterable()
            .Filter(static surface => surface.Side is IfcSurfaceSide.BOTH or IfcSurfaceSide.POSITIVE)
            .Head
            .Map(surface => Mint(SummaryOf(surface, tolerance), tolerance))
            .ToFin(new BimFault.ModelRejected(key, $"surface-style-miss:{styledItem.StepId}"));

    // Fold the front-face surface-style element selects onto the neutral PBR vector: the rendering (an
    // IfcSurfaceStyleShading subtype) supplies the colour/transparency base + the reflectance method + the specular
    // highlight, a bare shading supplies only colour/transparency, the refraction supplies the transmissive signal.
    static AppearanceSummary SummaryOf(IfcSurfaceStyle surface, double tolerance) {
        // The front-face element-select picker, ONE polymorphic surface over the three style subtypes the fold reads —
        // an IfcSurfaceStyleRendering is itself an IfcSurfaceStyleShading, so First<IfcSurfaceStyleShading> resolves the
        // rendering as the shading fallback when no bare shading is present.
        Option<T> First<T>() where T : class =>
            surface.Styles.AsIterable().Choose(static s => s is T t ? Some(t) : Option<T>.None).Head;
        Option<IfcSurfaceStyleRendering> rendering = First<IfcSurfaceStyleRendering>();
        Option<IfcSurfaceStyleShading> shading = First<IfcSurfaceStyleShading>();
        Option<IfcSurfaceStyleRefraction> refraction = First<IfcSurfaceStyleRefraction>();

        (double R, double G, double B) surfaceBase = shading.Bind(static sh => Optional(sh.SurfaceColour)).Map(Lin).IfNone((0.5, 0.5, 0.5));
        (double R, double G, double B) baseColor = rendering.Bind(static r => Optional(r.DiffuseColour as IfcColourRgb)).Map(Lin).IfNone(surfaceBase);

        ReflectanceModel reflectance = rendering.Map(static r => ReflectanceModel.FromIfc(r.ReflectanceMethod)).IfNone(ReflectanceModel.NotDefined);
        double opacity = shading.Map(static sh => double.IsNaN(sh.Transparency) ? 1.0 : 1.0 - Math.Clamp(sh.Transparency, 0.0, 1.0)).IfNone(1.0);
        double roughness = rendering.Bind(static r => RoughnessOf(r.SpecularHighlight)).IfNone(reflectance.RoughnessHint);
        // Transmission is the REFRACTIVE signal (the GLASS method or a present IfcSurfaceStyleRefraction), NOT a sub-unit
        // opacity: IFC IfcSurfaceStyleShading.Transparency IS the alpha/opacity channel (carried by `opacity`), distinct
        // from physical transmission — conflating the two is the deleted form (a half-alpha plastic is not glass).
        bool transmissive = reflectance.Transmissive || refraction.IsSome;

        return AppearanceSummary.Of(baseColor.R, baseColor.G, baseColor.B, reflectance.Metalness, Math.Clamp(roughness, 0.0, 1.0), opacity, transmissive, tolerance);
    }

    // The IfcSpecularHighlightSelect -> PBR roughness fold: an IfcSpecularRoughness is a [0,1] roughness read
    // directly; an IfcSpecularExponent is a Phong exponent converted through the standard alpha = sqrt(2/(n+2)).
    static Option<double> RoughnessOf(IfcSpecularHighlightSelect? highlight) => highlight switch {
        IfcSpecularRoughness r => Some(Math.Clamp(r.SpecularRoughness, 0.0, 1.0)),
        IfcSpecularExponent e  => Some(Math.Clamp(Math.Sqrt(2.0 / (Math.Max(0.0, e.SpecularExponent) + 2.0)), 0.0, 1.0)),
        _                      => Option<double>.None,
    };

    // The content-keyed seam Node.Appearance. The AppearanceKey is minted by the SEAM-OWNED AppearanceSummary.Of (the
    // kernel seed-zero XxHash128 over the neutral PBR vector via the seam CanonicalWriter -> ContentAddress.Of, the ONE
    // hasher) — the SHARED derivation the Rasm.Materials owner composes identically, so a BIM IfcSurfaceStyleRendering
    // style and a Rasm.Materials OpenPBR row describing one surface dedup to one AppearanceKey; this page re-assembles
    // NO key bytes of its own (a local CanonicalWriter over the PBR vector beside the seam factory is the divergence
    // defect — Bim and Materials would couple by byte-order convention). The NodeId is the content hash of the node's
    // ToCanonicalBytes (id excluded, the empty-span placeholder discarded), so two structurally-identical appearances
    // dedup to one node — the SAME Mint the Rasm.Materials ComponentProjector uses.
    static Node.Appearance Mint(AppearanceSummary summary, double tolerance) {
        var draft = new Node.Appearance(NodeId.Content(ReadOnlySpan<byte>.Empty), summary);
        return draft with { Id = NodeId.Content(draft.ToCanonicalBytes(tolerance).Span) };
    }

    static (double R, double G, double B) Lin(IfcColourRgb c) => (Linearize(c.Red), Linearize(c.Green), Linearize(c.Blue));

    // --- [EGRESS] -------------------------------------------------------------------------
    // The inverse half the Projection/semantic#IFC_EGRESS Emit composes per Object node carrying an appearance:
    // re-author the surface style from the neutral summary. The scene-linear base colour encodes back to display
    // sRGB through Encode; ForPbr picks the IFC reflectance method from the neutral PBR; a metal tints its specular
    // from the base colour, a dielectric reflects neutral; a transmissive appearance also authors an
    // IfcSurfaceStyleRefraction at a DEFAULT optical index 1.5 — the neutral summary carries no IOR, so the precise
    // refraction index/dispersion is NOT round-tripped here (it is a Rasm.Materials BSDF concern the thin summary
    // deliberately drops); only the transmissive SIGNAL round-trips, combined with the rendering in one
    // IfcSurfaceStyle via the 5-arg ctor.
    public static IfcStyledItem Author(DatabaseIfc db, AppearanceSummary summary) {
        IfcColourRgb surfaceColour = new(db, Encode(summary.BaseColorR), Encode(summary.BaseColorG), Encode(summary.BaseColorB));
        bool transmissive = summary.Transmissive;  // the refractive signal, DISTINCT from the opacity/alpha channel
        ReflectanceModel reflectance = ReflectanceModel.ForPbr(summary.Metallic, summary.Roughness, transmissive);
        IfcColourRgb specular = summary.Metallic >= 0.5 ? surfaceColour : new IfcColourRgb(db, 1.0, 1.0, 1.0);
        IfcSurfaceStyleRendering rendering = new(surfaceColour) {
            Transparency = 1.0 - summary.Opacity,
            ReflectanceMethod = reflectance.ToIfc(),
            DiffuseColour = surfaceColour,
            SpecularColour = specular,
            SpecularHighlight = new IfcSpecularRoughness(summary.Roughness),
        };
        IfcSurfaceStyle style = transmissive
            ? new IfcSurfaceStyle(rendering, null, null, null, new IfcSurfaceStyleRefraction(db) { RefractionIndex = 1.5 }) { Side = IfcSurfaceSide.BOTH }
            : new IfcSurfaceStyle(rendering) { Side = IfcSurfaceSide.BOTH };
        return new IfcStyledItem(style);
    }

    // The sRGB transfer pair (IEC 61966-2-1): Linearize lowers a display-referred IFC channel to scene-linear at
    // import, Encode encodes a scene-linear channel back to display at egress — a pure host-neutral TRANSFER, NOT
    // a working-space/primaries conversion (that stays the Rasm.Materials Unicolour owner's concern).
    static double Linearize(double c) {
        double s = Math.Clamp(c, 0.0, 1.0);
        return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
    static double Encode(double c) {
        double l = Math.Clamp(c, 0.0, 1.0);
        return l <= 0.0031308 ? l * 12.92 : 1.055 * Math.Pow(l, 1.0 / 2.4) - 0.055;
    }
}
```

## [03]-[RESEARCH]

- [SURFACE_STYLE_MEMBERS]: the GeometryGym presentation surface is verified against the live `GeometryGymIFC_Core` 25.7.30 decompile and the `.api/api-geometrygym-ifc` presentation rows — `IfcStyledItem.Styles` (`SET<IfcStyleAssignmentSelect>`) + `BaseClassIfc.Extract<IfcSurfaceStyle>()` (the version-agnostic traversal flattening an IFC2x3 `IfcPresentationStyleAssignment` wrapper) + ctor `IfcStyledItem(IfcStyleAssignmentSelect)` (an `IfcSurfaceStyle` satisfying it via `IfcPresentationStyle`); `IfcSurfaceStyle.Side` (`IfcSurfaceSide` `BOTH`/`POSITIVE`/`NEGATIVE`, default `BOTH`) + `.Styles` (`SET<IfcSurfaceStyleElementSelect>`) + ctors `IfcSurfaceStyle(IfcSurfaceStyleShading)` and the 5-arg `IfcSurfaceStyle(IfcSurfaceStyleShading, IfcSurfaceStyleLighting, IfcSurfaceStyleWithTextures, IfcExternallyDefinedSurfaceStyle, IfcSurfaceStyleRefraction)` that combines the rendering + refraction in one style; `IfcSurfaceStyleShading.SurfaceColour` (`IfcColourRgb`) + `.Transparency` (`double`, default `double.NaN` unset sentinel) + ctor `IfcSurfaceStyleShading(IfcColourRgb)`; `IfcSurfaceStyleRendering : IfcSurfaceStyleShading` carrying `DiffuseColour`/`TransmissionColour`/`DiffuseTransmissionColour`/`ReflectionColour`/`SpecularColour` (`IfcColourOrFactor`, the read side casting to `IfcColourRgb`), `SpecularHighlight` (`IfcSpecularHighlightSelect`), and `ReflectanceMethod` (`IfcReflectanceMethodEnum`) + ctor `IfcSurfaceStyleRendering(IfcColourRgb surfaceColour)`; `IfcSpecularHighlightSelect` is `IfcSpecularRoughness` (`.SpecularRoughness`, clamped [0,1] at its ctor — a direct PBR roughness) or `IfcSpecularExponent` (`.SpecularExponent`, a Phong exponent); `IfcSurfaceStyleRefraction.RefractionIndex`/`.DispersionFactor` (`double`) + ctor `IfcSurfaceStyleRefraction(DatabaseIfc)`; `IfcColourRgb.Red`/`.Green`/`.Blue` (`double`) + ctor `IfcColourRgb(DatabaseIfc, double, double, double)` (the `(DatabaseIfc, Color)` ctor and the `Color()` accessor are the host-coupled `System.Drawing.Color` members this host-neutral owner never touches); `IfcReflectanceMethodEnum` = `BLINN`/`FLAT`/`GLASS`/`MATT`/`METAL`/`MIRROR`/`PHONG`/`PLASTIC`/`STRAUSS`/`NOTDEFINED` (the `ReflectanceModel` `[SmartEnum<string>]` partition). The `IfcSpecularHighlightSelect`/`IfcSpecularRoughness`/`IfcSpecularExponent` concrete-type rows are decompile-confirmed beyond the catalogue's current `IfcSurfaceStyleRendering` row 05 summary.
- [SEAM_APPEARANCE_OWNERSHIP]: the seam owns the appearance node — `ELEMENT-REBUILD-PLAN.md` §4B (`Node` `[Union]` carries an `Appearance` case = a content-keyed `AppearanceSummary`) and the seam `Graph/element#NODE_MODEL` `AppearanceSummary(UInt128 AppearanceKey, double BaseColorR, double BaseColorG, double BaseColorB, double Metallic, double Roughness, double Opacity, bool Transmissive)` "a content-keyed reference to the full BSDF (authored in Rasm.Materials) plus the neutral canonical PBR scalars a consumer reads flat without the full lobe graph" — the `Transmissive` refractive bit DISTINCT from `Opacity` (alpha) so a refractive opaque-alpha glass round-trips and the GLB `KHR_materials_transmission` channel reads it apart from the alpha, the seam record carrying it beside the `AppearanceSummary.Of` factory — so this page projects the GeometryGym presentation graph onto the seam summary and mints the seam `Node.Appearance` through `NodeId.Content` over `Node.ToCanonicalBytes`, declaring no Bim appearance record; the `BimAppearance`/`AppearanceColor` record + the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier retirement grounds against §2 (two parallel unaligned element owners collapsed) and §4B (the consumer-facing `Element` is the `Bake` fold reading `element.Appearance` through the `Associate` edge, never a record nested in a material), mirroring the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` and `Semantics/composition#MATERIAL_COMPOSITION` projector shape.
- [OPENPBR_CONTENT_KEY]: the `AppearanceKey` content-key reconciliation grounds the cross-folder seam — `ELEMENT-REBUILD-PLAN.md` §4-RT H7 (one seam-owned canonical value codec; the diff `ContentBytes` and the id hash SHARE it) and the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` (the kernel seed-zero `XxHash128`, the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, and the Python/TypeScript peers) — so the `AppearanceKey` is the content hash over the neutral PBR vector (base colour + metalness + roughness + opacity + transmissive), minted by the SEAM-OWNED `Graph/element#NODE_MODEL` `AppearanceSummary.Of` factory both this projector and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` owner compose (never two convention-coupled local `CanonicalWriter` assemblies that could fork the byte order) — every neutral PBR scalar — base colour, metalness, roughness, opacity, AND transmissive — is load-bearing in the key because the seam `Node.ToCanonicalBytes` appearance arm writes ONLY the `AppearanceKey` (the key already folds the whole vector through `AppearanceSummary.Of`, so re-writing the scalars on the node arm would be a redundant second copy), the `AppearanceKey` carrying the full vector into node identity so two appearances differing only in alpha or in the refractive flag get distinct `Node.Appearance` ids — and a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key and one `Node.Appearance` id; the `Rasm.Materials` owner mints the full OpenPBR vector once (`surface#OPENPBR_SLAB` `OpenPbrSurface`, `surface#CONDUCTOR_IOR` `ConductorMetal`) and this page never re-derives it — the `ReflectanceModel` `Metalness`/`Transmissive` bias is the IFC-side seed a downstream `Rasm.Materials` mapping reads for `BaseMetalness`/transmission when an authored OpenPBR row is absent, never a re-mint of the branch OpenPBR algebra in `Rasm.Bim`.
- [COLOR_TRANSFER]: the sRGB `Linearize`/`Encode` transfer pair is the IEC 61966-2-1 sRGB EOTF/OETF (the `0.04045`/`0.0031308` piecewise thresholds, the `12.92` linear segment, the `((x+0.055)/1.055)^2.4` curve) authored exactly — IFC presentation colour carries no declared colour space and authoring tools emit display-referred sRGB, so the TRANSFER linearization to scene-linear is the correct lowering onto the seam `BaseColor` the `Rasm.Materials` scene-linear convention expects; the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) is a separate concern the `Rasm.Materials/Appearance/surface#SPECTRAL_UPSAMPLE` Unicolour owner holds (`RgbConfiguration.Acescg` + `.RgbLinear`), so this host-neutral leaf carries the transfer-curve math (a pure closed form, no `Unicolour` object) and defers the gamut/primaries to the Materials owner — the prior raw-channel pass-through that called the unlinearized clamp "scene-linear" is the corrected defect.
