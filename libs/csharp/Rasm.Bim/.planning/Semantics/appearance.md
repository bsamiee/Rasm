# [BIM_APPEARANCE]

The IFC surface-style PROJECTOR lowering the live GeometryGym `IfcStyledItem`/`IfcSurfaceStyle` presentation graph onto the `Rasm.Element` seam `Graph/element#NODE_MODEL` `Node.Appearance` carrying the neutral `AppearanceSummary`: `AppearanceProjection.Project` extracts the front-face `IfcSurfaceStyle`, folds its `IfcSurfaceStyleRendering`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRefraction` element selects onto one neutral PBR vector (scene-linear base colour + metalness + roughness + opacity + a `Transmissive` refractive flag DISTINCT from opacity, so an opaque-alpha glass still carries transmission), classifies the `IfcSurfaceStyleWithTextures` select onto the `SurfaceTexture` roster, and mints the content-keyed seam `Node.Appearance` the `Bake` fold reads through the `Relations/relation#EDGE_ALGEBRA` `Associate` edge into `element.Appearance`. The seam OWNS the `AppearanceSummary` PBR vocabulary and the `Projection/address#CONTENT_ADDRESS` content-key derivation; this page owns ONLY the GeometryGym discrimination that fills it, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster, and the `TextureMode`/`SurfaceTexture`/`UvTransform` IFC texture vocabulary, never re-declaring an appearance record. The projector is BIDIRECTIONAL: `AppearanceProjection.Author` is the inverse half the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` (diffuse/specular/highlight/reflectance-method/transparency) plus, for a textured appearance, an `IfcSurfaceStyleWithTextures` and, for a transmissive one, an `IfcSurfaceStyleRefraction`, all combined in ONE `IfcSurfaceStyle` through the five-slot constructor, so the surface style round-trips.

A texture is a FIELD the frozen seven-value `AppearanceSummary` preimage structurally cannot carry, so the roster rides BESIDE the summary on the `StyledAppearance` ingest product and behind the same `AppearanceKey` at the app-root edge: folding a map into a scalar is the averaged-map defect, and an eighth summary column re-keys every stored `Node.Appearance` and forks this projector's own cross-folder dedup key in one edit. Each `SurfaceTexture` names its canonical channel through its `TextureMode` row and carries the gloss/transparency polarity IFC declares and the channel name does not, so the texture-set owner binds and inverts decoded texels this host-neutral leaf never opens.

IFC presentation colours are display-referred sRGB; the projector lowers each channel to scene-linear through the sRGB EOTF (a pure host-neutral transfer, IEC 61966-2-1) so the seam `BaseColor` aligns with the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` scene-linear convention, and the egress encodes back through the inverse OETF — the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here. The appearance reconciles with the `Rasm.Materials` OpenPBR owner at the content key: the `AppearanceSummary.AppearanceKey` is the kernel seed-zero `XxHash128` over the neutral PBR vector — the shared seam-owned `AppearanceSummary.Of` derivation the `Rasm.Materials` `ComponentProjector` composes identically, so a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key, the `Rasm.Materials` owner the authority for the full BSDF and this page producing only the IFC-derived neutral summary. A re-mint of the `surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder seam violation.

The page is HOST-NEUTRAL — no `Rhino.Geometry`, no `Unicolour`, no `System.Drawing.Color` crosses a signature (the GeometryGym `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members are the deleted form, only the `Red`/`Green`/`Blue` doubles + the `(DatabaseIfc, double, double, double)` ctor are read/authored). The retired `BimAppearance`/`AppearanceColor` records and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE: appearance is element-scoped — a seam `Node.Appearance` the `Object` carries through the `Associate` edge, never a record nested in the retired `BimMaterial`, mirroring how the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` lowers onto the seam `Classification` value and `Semantics/composition#MATERIAL_COMPOSITION` onto the seam `Node.Material`. A malformed presentation graph rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` (`surface-style-miss`), lifted BARE onto the `Fin` rail.

## [01]-[INDEX]

- [02]-[APPEARANCE_PROJECTION]: `AppearanceProjection.Project` the `IfcStyledItem`→`StyledAppearance` ingress fold over the presentation graph, the `ReflectanceModel` `[SmartEnum<string>]` IFC reflectance-method roster carrying the typed PBR bias (`Metalness`/`RoughnessHint`/`Transmissive`) plus the `ForPbr` reverse classifier, the `TextureMode` mode→canonical-channel roster with its polarity column, the `SurfaceTexture` `[Union]` over the three concrete IFC texture payloads and the `UvTransform` frame they carry, the sRGB `Linearize`/`Encode` transfer pair, the `AppearanceKey` content-key derivation shared with `Rasm.Materials`, and the inverse `AppearanceProjection.Author` egress re-authoring the `IfcSurfaceStyleRendering`+`IfcSurfaceStyleWithTextures`+`IfcSurfaceStyleRefraction` surface style the `Projection/egress#IFC_EGRESS` `Emit` composes per `Object` node.

## [02]-[APPEARANCE_PROJECTION]

- Owner: `AppearanceProjection` the static BIDIRECTIONAL GeometryGym↔seam surface-style projector — the `Project` ingress folding one `IfcStyledItem`'s front-face `IfcSurfaceStyle` into one `StyledAppearance` (the content-keyed seam `Node.Appearance` beside its texture roster), and the `Author` egress re-authoring a seam `AppearanceSummary` and that roster back onto the GeometryGym presentation graph the `Emit` composes; `ReflectanceModel` the `[SmartEnum<string>]` IFC reflectance-method roster the projection folds the method onto without re-reading the enum string; `TextureMode` the IFC texture-mode roster carrying each token's canonical channel name and its gloss/transparency polarity; `SurfaceTexture` the `[Union]` over the three concrete `IfcSurfaceTexture` payloads with `UvTransform` the UV frame both projector halves carry; `StyledAppearance` the ingest product pairing summary and roster. The seam owns the `AppearanceSummary` neutral PBR record and its key derivation — the `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress` — and this page declares neither, composing the seam vocabulary, mapping the GeometryGym presentation entities onto it and back.
- Cases: `ReflectanceModel` arms `Blinn`/`Flat`/`Glass`/`Matt`/`Metal`/`Mirror`/`Phong`/`Plastic`/`Strauss`/`NotDefined` (10), the full IFC4.3 `IfcReflectanceMethodEnum` partition keyed on the schema constant, each carrying its typed PBR bias — `Metalness` (`METAL`/`MIRROR` → 1.0, `STRAUSS` → 0.5, every dielectric → 0.0), `RoughnessHint` (the fallback when the style supplies no `IfcSpecularHighlight` — `MIRROR` → 0.0, `MATT`/`FLAT` → 1.0), and `Transmissive` (`GLASS` → true); `TextureMode` spans BOTH the IFC2x3 `IfcSurfaceTextureEnum` tokens and the IFC4 free-identifier spellings authoring tools emit, each row naming the canonical channel it resolves to and whether its stored value is that channel's complement (`SHININESS` gloss, `TRANSPARENCYMAP` transparency), `NotDefined` the unresolved row; `SurfaceTexture` arms `Url`/`Blob`/`Pixels` over `IfcImageTexture`/`IfcBlobTexture`/`IfcPixelTexture`, each sharing the mode, wrap pair, and optional UV frame through the root's positional columns; the appearance is the seam's ONE `AppearanceSummary`, never a `RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` sibling triple and never a Bim `BimAppearance`/`AppearanceColor` record beside the seam.
- Entry: `AppearanceProjection.Project(IfcStyledItem styledItem, double tolerance, Op key)` returning `Fin<StyledAppearance>` is the per-styled-item leaf the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes from its per-`Object` representation walk — a dedicated appearance fold (the sibling of `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.MaterialEdges`) that discovers each object's styled items through the GeometryGym `IfcRepresentationItem.StyledByItem` inverse, calls `Project`, dedups the minted node by id, authors the `Object`→`Appearance` `Associate` edge against the object's rooted `NodeId` with `MaterialUsage.None` (the appearance `Associate` edge carries no material usage), and hands the `StyledAppearance` texture roster onward at the composition root keyed by the same `AppearanceKey` — the roster never enters the seam graph, which owns the summary alone; a `Material`-scoped style instead rides the `Rasm.Materials` `ComponentProjector`, which authors its own `element→appearance` edge — extracting the front-face `IfcSurfaceStyle` (`Side` `BOTH`/`POSITIVE`) through `BaseClassIfc.Extract<IfcSurfaceStyle>()` (version-agnostic: it flattens an IFC2x3 `IfcPresentationStyleAssignment` wrapper), folding its element selects onto the neutral PBR vector, and minting the content-keyed seam `Node.Appearance`; `Fin<T>` aborts on a presentation graph carrying no front-face surface style (`Model/faults#FAULT_BAND` `BimFault.ModelRejected` `surface-style-miss`) and carries the seam factory's own `ElementFault.ValueRejected` through on an out-of-unit channel, both lifted BARE (the `Expected`-derived band is the `Error`, no `.ToError()` hop). `AppearanceProjection.Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures)` is the egress entry the `Emit` composes per `Object` node carrying an appearance — re-authoring the `IfcSurfaceStyleRendering` from the neutral summary, the `IfcSurfaceStyleWithTextures` from a non-empty roster, and the `IfcSurfaceStyleRefraction` from a transmissive summary, all through ONE five-slot `IfcSurfaceStyle` constructor; total (authoring from a valid summary cannot fail), returning the `IfcStyledItem` the `Emit` binds onto the representation.
- Auto: `Project` reads the front-face `IfcSurfaceStyle.Styles` element selects — the `IfcSurfaceStyleRendering` (an `IfcSurfaceStyleShading` subtype, so it supplies the inherited `SurfaceColour`/`Transparency` plus the `DiffuseColour`/`ReflectanceMethod`/`SpecularHighlight` rendering channels), a bare `IfcSurfaceStyleShading` fallback (colour/transparency only), and the `IfcSurfaceStyleRefraction` optical signal — and folds the channel precedence: the rendering `DiffuseColour` overrides the shading `SurfaceColour` for the base colour through BOTH `IfcColourOrFactor` select arms — an `IfcColourRgb` replaces (each channel lowered to scene-linear through `Linearize`), an `IfcNormalisedRatioMeasure` SCALES the linearized surface colour (reflectance is linear-domain energy; the GG ctor clamps the ratio [0,1]; the `as IfcColourRgb` cast that ignored the factor arm is the deleted form), defaulting grey when absent — the opacity is the transparency complement (a `double.NaN` transparency, the GeometryGym unset sentinel, defaulting to opaque), the metalness is `ReflectanceModel.FromIfc(ReflectanceMethod).Metalness`, the roughness reads the `IfcSpecularHighlightSelect` (`IfcSpecularRoughness` directly as a [0,1] roughness, `IfcSpecularExponent` converted through the Phong `α = √(2/(n+2))`) and falls back to the row's `RoughnessHint`, and the transmissive flag is the REFRACTIVE signal (the row's `Transmissive` GLASS method or a present `IfcSurfaceStyleRefraction`, NEVER a sub-unit opacity — IFC `Transparency` is the alpha/opacity channel, physically distinct from transmission), PERSISTED on the summary apart from opacity (so an opaque-alpha refractive glass keeps its transmission, the round-trip symmetric with the egress `IfcSurfaceStyleRefraction`); the seam-owned `AppearanceSummary.Of` then admits the seven frozen preimage values under this call's `Op` key and derives the `AppearanceKey` itself as the kernel seed-zero `XxHash128` over the canonical PBR bytes — the factory's own writer runs at raw IEEE bits because PBR scalars are not `Header`-quantized measures, so no tolerance argument exists to pass and none forks the shared dedup key (the seam `CanonicalWriter` → `ContentAddress.Of`, the ONE hasher the `Rasm.Materials` owner composes identically, this page assembling no key bytes of its own); `TexturesOf` folds every `IfcSurfaceStyleWithTextures` select's `Textures` list through `SurfaceTexture.Of` into the roster riding beside the summary — the concrete subtype discriminates the payload case, the `Mode` token resolves its canonical channel and polarity, and an optional `IfcCartesianTransformationOperator2D` lifts to the `UvTransform` frame — while the neutral scalars stay untouched, so a textured style never averages a map into a scalar; `Mint` content-keys the seam `Node.Appearance` whose id is the seam `Node.ToCanonicalBytes` (id excluded) re-stamped through the seam `Node.Relabel` — the class-root `[Union]` `Node` case generates no `with`, so the `draft with { Id = … }` spelling is the deleted form, the mint the `Rasm.Materials` `ComponentProjector` composes identically — so two structurally-identical appearances dedup to one node. `Author` encodes the scene-linear base colour back to display sRGB through `Encode`, picks the `IfcReflectanceMethodEnum` from the neutral PBR through `ReflectanceModel.ForPbr` (a transmissive surface `GLASS`, a metallic mirror `MIRROR`, a rough metal `METAL`, a matte dielectric `MATT` so an imported `MATT`/`FLAT` finish round-trips via its `1.0` `RoughnessHint`, every remaining dielectric `PLASTIC` — the modern method subset only, a legacy `BLINN`/`PHONG`/`STRAUSS` being import vocabulary the neutral vector absorbs, never re-authored), tints the specular from the base colour for a metal and reflects neutral for a dielectric, authors `IfcSpecularRoughness` from the summary roughness, re-authors each `SurfaceTexture` through its own total `Switch` into one `IfcSurfaceStyleWithTextures`, and lands every element select through the ONE five-slot `IfcSurfaceStyle` constructor whose null slots carry the absent axes — a ternary ladder growing an arm per transmission×texturing combination is the deleted form.
- Receipt: the seam `Node.Appearance` is the appearance evidence the `Projection/semantic#SEMANTIC_PROJECTOR` projector lands (authoring the `Object`→`Appearance` `Associate` edge) and the `Graph/element#ELEMENT_GRAPH` `Bake` fold reads into `element.Appearance` (an `Option<AppearanceSummary>` a consumer reads flat), the `Exchange/export#EXPORT_RAIL` `MaterialFinish` carries whole — its scene-linear base colour and opacity entering `baseColorFactor` unencoded, its metalness and roughness written on every material because the glTF defaults are both unity, and its `Transmissive` bit writing `KHR_materials_transmission` while the opacity alone drives `AlphaMode` — and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` OpenPBR owner reconciles at the `AppearanceKey` content key; the `ReflectanceModel` typed PBR bias is the IFC-side mapping a downstream metallic/dielectric author folds, never a re-read of the IFC enum string.
- Packages: GeometryGymIFC_Core (`IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleShading`/`IfcSurfaceStyleRendering`/`IfcSurfaceStyleRefraction`/`IfcSurfaceStyleWithTextures`/`IfcSurfaceTexture`/`IfcImageTexture`/`IfcBlobTexture`/`IfcPixelTexture`/`IfcBinary`/`IfcCartesianTransformationOperator2D`/`IfcCartesianPoint`/`IfcDirection`/`IfcSpecularRoughness`/`IfcSpecularExponent`/`IfcColourRgb`/`IfcReflectanceMethodEnum`/`IfcSurfaceTextureEnum`/`IfcSurfaceSide` — `.api/api-geometrygym-ifc` presentation rows 01/03-13/16 + reflectance/side enum rows 04-05, the `IfcSurfaceStyleRendering` PBR-channel, `IfcSpecularHighlightSelect`, and texture-payload member surfaces decompile-confirmed), Rasm.Element (the seam `Node.Appearance`/`AppearanceSummary` + its seam-owned `AppearanceSummary.Of` content-key factory/`NodeId.Content`/`Node.ToCanonicalBytes` — the page composes the factory, never a local `CanonicalWriter`/`ContentAddress` key assembly), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Iterable`), BCL inbox (`System.Numerics.Vector2` — the UV offset/scale frame), Rasm (the kernel `Op` op-key + the seed-zero `XxHash128` content hash the seam `AppearanceSummary.Of`/`NodeId.Content` compose); the `Rasm.Materials` OpenPBR owner is reconciled at the `AppearanceKey` content key alone — no `Rasm.Materials` assembly reference crosses into this host-neutral leaf.
- Growth: a new IFC reflectance method is one `ReflectanceModel` row carrying its schema constant and typed PBR bias; a new texture-mode spelling is one `TextureMode` row naming its canonical channel and polarity; a new IFC texture payload subtype is one `SurfaceTexture` case the `Of` fold and the `Author` `Switch` are both compiler-forced to route; a new SCALAR presentation-channel read (the `IfcSurfaceStyleLighting` `DiffuseTransmissionColour`/`DiffuseReflectionColour`/`TransmissionColour`/`ReflectanceColour` four-colour coefficients) is one more `Styles` element-select arm folding onto the neutral summary, and a new FIELD-valued read is one more roster beside the textures on `StyledAppearance`, never a summary column; the seam `AppearanceSummary` absorbs the neutral vector with no seam edit; never a per-style appearance class and never a Bim appearance record beside the seam node.
- Boundary: the appearance model is the seam `Node.Appearance` + `AppearanceSummary` and a Bim `BimAppearance`/`AppearanceColor`/`RenderingAppearance`/`ShadingAppearance`/`TexturedAppearance` re-declaration is the deleted form — the seam owns the neutral PBR record, this page owns only the GeometryGym discrimination that fills it, so the appearance lowers onto the one seam summary with the absent channels defaulted, never a parallel per-style class; the retired `BimAppearance` record and the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier are GONE, appearance being element-scoped (a seam `Node.Appearance` the `Object` carries through the `Associate` edge the `Bake` fold reads into `element.Appearance`), never a record nested in a material; the projection rides the GeometryGym `IfcStyledItem`/`IfcSurfaceStyle`/`IfcSurfaceStyleRendering`/`IfcColourRgb` surface consumed as settled vocabulary (`.api/api-geometrygym-ifc` presentation rows) through `BaseClassIfc.Extract<IfcSurfaceStyle>()`, and a hand-rolled STEP presentation-style reader is the deleted form; the page is HOST-NEUTRAL — a `Rhino.Geometry` colour, a `System.Drawing.Color` (the `IfcColourRgb.Color()`/`IfcColourRgb(DatabaseIfc, Color)` host-coupled members), or a `Unicolour` object crossing a signature is the named host-coupling defect, only the `Red`/`Green`/`Blue` scene-linear doubles cross; IFC presentation colour is display-referred sRGB lowered to scene-linear through the `Linearize` EOTF and encoded back through the `Encode` OETF — a raw-channel pass-through that calls the unlinearized value "scene-linear" is the deleted form, and the working-space PRIMARIES conversion stays the `Rasm.Materials` Unicolour owner's concern, never re-derived here; the OpenPBR reconciliation rides the `AppearanceKey` content key — a re-mint of the `Rasm.Materials/Appearance/surface#OPENPBR_SLAB` `OpenPbrSurface` vector, the `surface#CONDUCTOR_IOR` conductor-IOR table, or the OpenPBR slab algebra in this owner is the named cross-folder seam violation; the rich IFC rendering channels (`SpecularColour`/`TransmissionColour`/`ReflectionColour`/`DiffuseTransmissionColour`, the `IfcSurfaceStyleRefraction` IOR/dispersion MAGNITUDE, the `IfcSurfaceStyleLighting` coefficients) are NOT retained by the thin seam summary — a Bim-imported style collapses DELIBERATELY to base colour/metalness/roughness/opacity + a transmissive flag (the refraction PRESENCE is kept as the transmissive bit DISTINCT from the opacity/alpha channel so opaque-alpha glass round-trips, its IOR/dispersion magnitude dropped; the seam's chosen shape, lossy by design, NOT an unintended gap), and full specular/reflection/transmission-colour + dispersion BSDF fidelity exists ONLY when the `Rasm.Materials` owner AUTHORS the appearance and holds the lobe graph keyed by the shared `AppearanceKey` (claiming a Bim round-trip preserves the dropped colour/IOR channels is the deleted overclaim); TEXTURES are the one FIELD-valued presentation fact this projector retains, and they retain BESIDE the summary, never inside it — `StyledAppearance` pairs the content-keyed node with the `SurfaceTexture` roster, the frozen seven-value `AppearanceSummary` preimage stays sealed (an eighth column re-keys every stored `Node.Appearance` and forks the very dedup key this page mints against), and folding a map's mean into a scalar channel is the averaged-map defect the roster exists to refuse; this leaf CLASSIFIES and CARRIES alone — it opens no image, so a texel decode, the `TextureMode` gloss/transparency inversion, and any resampling ride the texture-set owner the app-root edge hands the roster to, and an `IfcPixelTexture` egresses its declared extent with an empty pixel run because GeometryGym exposes that run only through its constructor; the `ReflectanceModel` keys its `FromIfc` resolution through the `Items`-derived frozen index on the typed `Method` constant — no `switch` over the enum, no `ToString` hop, `NotDefined` the total fallback, and `TextureMode.From` mirrors that admission through the generated `Validate` with `NotDefined` the unresolved row rather than a guessed channel; faults route through the `Fin` rail and lift BARE (the `Expected`-derived `BimFault.ModelRejected` IS the `Error`, never a `.ToError()` hop and never an exception across a domain signature).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics;                            // Vector2 — the UV offset/scale frame both projector halves carry
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Graph;
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

    // The Items-derived frozen index keyed on the typed schema constant — FromIfc resolves the row without the
    // enum-to-string hop (the method value IS the symbol; ToString + key TryGet restated it), NotDefined the
    // total fallback for a future schema member.
    private static readonly Lazy<FrozenDictionary<IfcReflectanceMethodEnum, ReflectanceModel>> ByMethod =
        new(static () => Items.ToFrozenDictionary(static row => row.Method));

    public static ReflectanceModel FromIfc(IfcReflectanceMethodEnum method) =>
        ByMethod.Value.GetValueOrDefault(method, NotDefined);

    public IfcReflectanceMethodEnum ToIfc() => Method;

    // The reverse classifier the egress picks the IFC method from a neutral PBR vector through — a transmissive
    // surface authors GLASS, a metallic mirror MIRROR, a rough metal METAL, a matte dielectric MATT (the diffuse-only
    // band, so an imported MATT/FLAT round-trips MATT via its 1.0 RoughnessHint), every remaining dielectric PLASTIC.
    // The egress authors ONLY this modern subset — a legacy BLINN/PHONG/STRAUSS is import vocabulary the neutral
    // vector absorbs, never re-authored.
    public static ReflectanceModel ForPbr(double metallic, double roughness, bool transmissive) =>
        transmissive          ? Glass
        : metallic >= 0.5     ? (roughness <= 0.05 ? Mirror : Metal)
        : roughness >= 0.9    ? Matt
        : Plastic;
}

// The IFC surface-texture MODE roster: each row keys on the `IfcSurfaceTexture.Mode` token and carries the
// CANONICAL snake_case channel name the texture-set vocabulary keys on. IFC4 types Mode as a free IfcIdentifier
// while IFC2x3 constrained it to IfcSurfaceTextureEnum, so the roster spans BOTH — the nine legacy enum tokens
// and the modern glTF-aligned spellings authoring tools emit — under one case-insensitive key, and an unclaimed
// token resolves NotDefined (empty channel) into the manifest's unresolved list rather than guessing a channel.
// The channel VALUE is the transcription boundary: a channel name is the shared frozen vocabulary and this leaf
// re-spells no roster of its own, so the app-root edge hands the key straight to the texture-set owner and no
// Rasm.Materials type crosses into this host-neutral page. The SAME key is what Exchange/export#EXPORT_RAIL
// GltfChannel.From resolves onto its glTF channel targets, so an IFC mode token reaches a glTF slot through two
// rosters over one canonical name and never through a call-site correspondence either roster could contradict.
//
// Invert is the ONE per-row polarity fact IFC carries and the channel key does not: a SHININESS map stores gloss
// and a TRANSPARENCYMAP stores transparency, both the complement of the channel they resolve to. The flag TRAVELS
// on the row because inversion evaluates in the LINEAR domain over decoded texels — this page opens no image, so
// applying it here is unspellable and dropping it is the silent-roughness-fork defect.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TextureMode {
    public static readonly TextureMode Texture          = new("TEXTURE",          channel: "base_color",          invert: false);
    public static readonly TextureMode Diffuse          = new("DIFFUSE",          channel: "base_color",          invert: false);
    public static readonly TextureMode Specular         = new("SPECULAR",         channel: "specular_color",      invert: false);
    public static readonly TextureMode Reflection       = new("REFLECTION",       channel: "specular_color",      invert: false);
    public static readonly TextureMode Shininess        = new("SHININESS",        channel: "specular_roughness",  invert: true);
    public static readonly TextureMode Roughness        = new("ROUGHNESS",        channel: "specular_roughness",  invert: false);
    // The glTF-aligned packed spelling resolves to the ORM pack name, not a channel — the pack roster owns the
    // slot order, so a packed stem never resolves to one of its components.
    public static readonly TextureMode Packed           = new("METALLICROUGHNESS", channel: "orm",                 invert: false);
    public static readonly TextureMode Normal           = new("NORMAL",           channel: "geometry_normal",     invert: false);
    public static readonly TextureMode Bump             = new("BUMP",             channel: "height",              invert: false);
    public static readonly TextureMode Occlusion        = new("OCCLUSION",        channel: "occlusion",           invert: false);
    public static readonly TextureMode Opacity          = new("OPACITY",          channel: "geometry_opacity",    invert: false);
    public static readonly TextureMode TransparencyMap  = new("TRANSPARENCYMAP",  channel: "geometry_opacity",    invert: true);
    public static readonly TextureMode SelfIllumination = new("SELFILLUMINATION", channel: "emission_color",      invert: false);
    public static readonly TextureMode Emissive         = new("EMISSIVE",         channel: "emission_color",      invert: false);
    public static readonly TextureMode NotDefined       = new("NOTDEFINED",       channel: "",                    invert: false);

    public string Channel { get; }
    public bool Invert { get; }
    public bool Resolved => Channel.Length > 0;

    private TextureMode(string key, string channel, bool invert) : this(key) => (Channel, Invert) = (channel, invert);

    // The generated keyed lookup is the one-hop admission from the raw IFC token under the row comparer's own
    // case-insensitivity; an unmatched token is NotDefined, never a fabricated channel — a guessed binding
    // lights the wrong slot with no diagnostic and no consumer can tell it from a correct one.
    public static TextureMode From(string mode) => TryGet(mode, out TextureMode? row) && row is not null ? row : NotDefined;
}

// The KHR_texture_transform-shaped UV frame an IfcSurfaceTexture carries on its optional
// IfcCartesianTransformationOperator2D: the LocalOrigin is the offset, Scale the uniform scale, and Axis1 the
// rotated U direction whose atan2 IS the rotation. Bim owns this ONE UV frame — the Exchange/export#EXPORT_RAIL
// glTF binding composes it rather than re-deriving a second offset/scale/rotation triple.
public readonly record struct UvTransform(Vector2 Offset, Vector2 Scale, double Rotation) {
    public static readonly UvTransform Identity = new(Vector2.Zero, Vector2.One, 0d);

    // GeometryGym leaves Axis1/Axis2/LocalOrigin null on an unconstrained operator and Scale 0 when the STEP
    // field was unset, so the absent-axis default is +U and a non-positive scale reads as unity — a raw 0 scale
    // would collapse every texel of a texture whose author simply omitted the optional field.
    public static UvTransform Of(IfcCartesianTransformationOperator2D operatorRef) {
        double scale = operatorRef.Scale > 0d ? operatorRef.Scale : 1d;
        IfcCartesianPoint? origin = operatorRef.LocalOrigin;
        IfcDirection? axis = operatorRef.Axis1;
        (double u, double v) = axis is null ? (1d, 0d) : (axis.DirectionRatioX, axis.DirectionRatioY);
        return new UvTransform(
            new Vector2((float)(origin?.CoordinateX ?? 0d), (float)(origin?.CoordinateY ?? 0d)),
            new Vector2((float)scale, (float)scale),
            Math.Atan2(v, u));
    }
}

// One ingested IFC surface texture. The IFC payload varies over exactly three concrete IfcSurfaceTexture
// subtypes — a referenced URL, an inline raster blob, and a raw pixel grid — so the family is one RECORD-root
// [Union] (structural equality across the closed set, no drillable payload to descend into) whose universal
// columns thread the root's own positional parameters and each case passes them straight through beside its own
// payload; a case declaring a same-named column of its own instead of forwarding suppresses the root's property
// and silently drops the argument, which is the one shape this family forecloses by construction.
// This page classifies and carries; it decodes no image — an IfcPixelTexture's own pixel run stays behind
// GeometryGym's ctor-only field, so the Pixels case declares the grid EXTENT it can read and no texel payload
// it cannot, and the app-root edge resolves the bytes through the texture-set owner.
[Union]
public abstract partial record SurfaceTexture(TextureMode Mode, bool RepeatS, bool RepeatT, Option<UvTransform> Uv) {
    public sealed record Url(TextureMode Mode, bool RepeatS, bool RepeatT, Option<UvTransform> Uv, string Reference)
        : SurfaceTexture(Mode, RepeatS, RepeatT, Uv);
    public sealed record Blob(TextureMode Mode, bool RepeatS, bool RepeatT, Option<UvTransform> Uv, string RasterFormat, ReadOnlyMemory<byte> Raster)
        : SurfaceTexture(Mode, RepeatS, RepeatT, Uv);
    public sealed record Pixels(TextureMode Mode, bool RepeatS, bool RepeatT, Option<UvTransform> Uv, int Width, int Height, int Components)
        : SurfaceTexture(Mode, RepeatS, RepeatT, Uv);

    // The IfcSurfaceTexture -> case fold: the concrete subtype IS the discriminant, so an unrecognized future
    // subtype is None rather than a lossy Url with an empty reference. GeometryGym spells the URL field
    // UrlReference (never URLReference) and types RasterCode as IfcBinary whose `Binary` member is the raw
    // byte[] — the ValueString hex render is the STEP wire form, so lifting it as text would re-parse bytes the
    // decoder already holds.
    public static Option<SurfaceTexture> Of(IfcSurfaceTexture texture) {
        TextureMode mode = TextureMode.From(texture.Mode);
        Option<UvTransform> uv = Optional(texture.TextureTransform).Map(UvTransform.Of);
        return texture switch {
            IfcImageTexture image => Some<SurfaceTexture>(new Url(mode, texture.RepeatS, texture.RepeatT, uv, image.UrlReference)),
            IfcBlobTexture blob   => Some<SurfaceTexture>(new Blob(mode, texture.RepeatS, texture.RepeatT, uv, blob.RasterFormat, Optional(blob.RasterCode).Map(static code => (ReadOnlyMemory<byte>)code.Binary).IfNone(ReadOnlyMemory<byte>.Empty))),
            IfcPixelTexture grid  => Some<SurfaceTexture>(new Pixels(mode, texture.RepeatS, texture.RepeatT, uv, grid.Width, grid.Height, grid.ColourComponents)),
            _                     => Option<SurfaceTexture>.None,
        };
    }

    // The egress inverse: re-author the GeometryGym texture entity from the carried case. Total over the closed
    // family, so a new case breaks the build. A Pixels case re-authors its EXTENT with an empty pixel run —
    // GeometryGym exposes the run only through the ctor and this page never held the texels — so a pixel-texture
    // round-trip that claimed the raster back is the deleted overclaim; an ingested pixel grid egresses as its
    // declared extent and the texture-set owner supplies bytes when one is bound.
    public IfcSurfaceTexture Author(DatabaseIfc db) {
        IfcSurfaceTexture authored = Switch<IfcSurfaceTexture>(
            url:    u => new IfcImageTexture(db, u.RepeatS, u.RepeatT, u.Reference),
            blob:   b => new IfcBlobTexture(db, b.RepeatS, b.RepeatT, b.RasterFormat, new IfcBinary(b.Raster.ToArray())),
            pixels: p => new IfcPixelTexture(db, p.RepeatS, p.RepeatT, p.Width, p.Height, p.Components, []));
        authored.Mode = Mode.Key;
        Uv.IfSome(uv => authored.TextureTransform = Operator(db, uv));
        return authored;
    }

    // IfcCartesianTransformationOperator2D(DatabaseIfc) seeds an origin-anchored operator, so the frame writes
    // through the mutable Axis1/LocalOrigin/Scale members; a uniform Scale is the whole IFC 2D affine axis, so a
    // non-uniform glTF scale narrows to its U component here and the divergence rides the texture-set owner.
    static IfcCartesianTransformationOperator2D Operator(DatabaseIfc db, UvTransform uv) =>
        new(db) {
            LocalOrigin = new IfcCartesianPoint(db, uv.Offset.X, uv.Offset.Y),
            Axis1 = new IfcDirection(db, Math.Cos(uv.Rotation), Math.Sin(uv.Rotation)),
            Scale = uv.Scale.X,
        };
}

// The bidirectional projector's INGEST product: the content-keyed seam node plus the texture roster the frozen
// seven-scalar AppearanceSummary structurally cannot carry. A texture is a FIELD; folding one into a scalar is
// the averaged-map defect, and widening AppearanceSummary re-keys every stored Node.Appearance and forks the
// cross-folder dedup key, so the roster rides BESIDE the summary and the app-root edge hands it to the texture-set
// owner keyed by the same AppearanceKey. An untextured style yields an empty roster, never a second entrypoint.
public readonly record struct StyledAppearance(Node.Appearance Appearance, Seq<SurfaceTexture> Textures);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The one GeometryGym<->seam surface-style projector: IfcStyledItem -> seam Node.Appearance carrying the neutral
// AppearanceSummary, and back. The seam OWNS the AppearanceSummary PBR vocabulary + the content-key derivation;
// this projector discriminates the IFC presentation graph and folds it onto the neutral vector, never re-minting
// the Rasm.Materials OpenPBR vector / conductor-IOR table / slab algebra (the named cross-folder seam violation).
public static class AppearanceProjection {
    // The egress optical index for a transmissive style whose summary carries no IOR (the thin seam drops the
    // magnitude — a Rasm.Materials BSDF concern): crown-glass 1.5, the conventional neutral.
    const double DefaultRefractionIndex = 1.5;

    public static Fin<StyledAppearance> Project(IfcStyledItem styledItem, double tolerance, Op key) =>
        styledItem.Extract<IfcSurfaceStyle>().AsIterable()
            .Filter(static surface => surface.Side is IfcSurfaceSide.BOTH or IfcSurfaceSide.POSITIVE)
            .Head
            .ToFin(new BimFault.ModelRejected(key, $"surface-style-miss:{styledItem.StepId}"))
            .Bind(surface => SummaryOf(surface, key).Map(summary =>
                new StyledAppearance(Mint(summary, tolerance), TexturesOf(surface))));

    // The IfcSurfaceStyleWithTextures element-select arm: every carried IfcSurfaceTexture classifies onto its
    // canonical channel and NONE of them touches the neutral scalars — a texture is a field, so folding one into
    // a scalar is the averaged-map defect and widening the frozen seven-value AppearanceSummary preimage re-keys
    // every stored Node.Appearance. Multiple texture styles on one surface concatenate in Styles order, and an
    // unclaimed Mode rides its NotDefined row so the app-root edge accumulates it as unresolved rather than
    // binding a guessed channel.
    static Seq<SurfaceTexture> TexturesOf(IfcSurfaceStyle surface) =>
        toSeq(surface.Styles)
            .Choose(static style => style is IfcSurfaceStyleWithTextures textured ? Some(textured) : Option<IfcSurfaceStyleWithTextures>.None)
            .Bind(static textured => toSeq(textured.Textures))
            .Choose(SurfaceTexture.Of);

    // Fold the front-face surface-style element selects onto the neutral PBR vector: the rendering (an
    // IfcSurfaceStyleShading subtype) supplies the colour/transparency base + the reflectance method + the specular
    // highlight, a bare shading supplies only colour/transparency, the refraction supplies the transmissive signal.
    // The seam factory admits, so the fold returns Fin and the Op key correlates its ValueRejected rail.
    static Fin<AppearanceSummary> SummaryOf(IfcSurfaceStyle surface, Op key) {
        // The front-face element-select picker, ONE polymorphic surface over the three style subtypes the fold reads —
        // an IfcSurfaceStyleRendering is itself an IfcSurfaceStyleShading, so First<IfcSurfaceStyleShading> resolves the
        // rendering as the shading fallback when no bare shading is present.
        Option<T> First<T>() where T : class =>
            surface.Styles.AsIterable().Choose(static s => s is T t ? Some(t) : Option<T>.None).Head;
        Option<IfcSurfaceStyleRendering> rendering = First<IfcSurfaceStyleRendering>();
        Option<IfcSurfaceStyleShading> shading = First<IfcSurfaceStyleShading>();
        Option<IfcSurfaceStyleRefraction> refraction = First<IfcSurfaceStyleRefraction>();

        (double R, double G, double B) surfaceBase = shading.Bind(static sh => Optional(sh.SurfaceColour)).Map(Lin).IfNone((0.5, 0.5, 0.5));
        // IfcColourOrFactor is a TWO-arm select: an IfcColourRgb REPLACES the surface colour, an
        // IfcNormalisedRatioMeasure SCALES it — reflectance is linear-domain energy, so the ratio (GG-clamped
        // [0,1] at its ctor) multiplies the linearized triple; the `as IfcColourRgb` cast that silently ignored
        // the factor arm is the deleted form.
        (double R, double G, double B) baseColor = rendering.Bind(static r => Optional(r.DiffuseColour)).Map(diffuse => diffuse switch {
            IfcColourRgb rgb                => Lin(rgb),
            IfcNormalisedRatioMeasure ratio => (surfaceBase.R * ratio.Measure, surfaceBase.G * ratio.Measure, surfaceBase.B * ratio.Measure),
            _                               => surfaceBase,
        }).IfNone(surfaceBase);

        ReflectanceModel reflectance = rendering.Map(static r => ReflectanceModel.FromIfc(r.ReflectanceMethod)).IfNone(ReflectanceModel.NotDefined);
        double opacity = shading.Map(static sh => double.IsNaN(sh.Transparency) ? 1.0 : 1.0 - Math.Clamp(sh.Transparency, 0.0, 1.0)).IfNone(1.0);
        double roughness = rendering.Bind(static r => RoughnessOf(r.SpecularHighlight)).IfNone(reflectance.RoughnessHint);
        // Transmission is the REFRACTIVE signal (the GLASS method or a present IfcSurfaceStyleRefraction), NOT a sub-unit
        // opacity: IFC IfcSurfaceStyleShading.Transparency IS the alpha/opacity channel (carried by `opacity`), distinct
        // from physical transmission — conflating the two is the deleted form (a half-alpha plastic is not glass).
        bool transmissive = reflectance.Transmissive || refraction.IsSome;

        // The seam factory owns the whole key derivation — its own writer runs at the FROZEN raw-IEEE-bit
        // tolerance because PBR scalars are not Header-quantized measures — so this call spells the seven frozen
        // preimage values and the Op key alone. A tolerance argument here is unspellable by construction: the
        // eighth parameter IS the Op key, and passing the document tolerance would have forked the cross-folder
        // dedup key had the factory ever taken one.
        return AppearanceSummary.Of(baseColor.R, baseColor.G, baseColor.B, reflectance.Metalness, Math.Clamp(roughness, 0.0, 1.0), opacity, transmissive, key);
    }

    // The IfcSpecularHighlightSelect -> PBR roughness fold: an IfcSpecularRoughness is a [0,1] roughness read
    // directly; an IfcSpecularExponent is a Phong exponent converted through the standard alpha = sqrt(2/(n+2)).
    static Option<double> RoughnessOf(IfcSpecularHighlightSelect? highlight) => highlight switch {
        IfcSpecularRoughness r => Some(Math.Clamp(r.SpecularRoughness, 0.0, 1.0)),
        IfcSpecularExponent e  => Some(Math.Clamp(Math.Sqrt(2.0 / (Math.Max(0.0, e.SpecularExponent) + 2.0)), 0.0, 1.0)),
        _                      => Option<double>.None,
    };

    // The content-keyed seam Node.Appearance: the AppearanceKey is minted ONLY by the seam-owned AppearanceSummary.Of
    // (this page assembles no key bytes — a local CanonicalWriter beside the seam factory is the byte-order divergence
    // defect); the NodeId is the content hash of ToCanonicalBytes (id excluded) re-stamped through the seam
    // Node.Relabel (a class-root [Union] Node case generates no `with`; the cast is arm-guaranteed) — the SAME Mint
    // the Rasm.Materials ComponentProjector composes, so two structurally-identical appearances dedup to one node.
    static Node.Appearance Mint(AppearanceSummary summary, double tolerance) {
        Node.Appearance draft = new(NodeId.Content(ReadOnlySpan<byte>.Empty), summary);
        return (Node.Appearance)draft.Relabel(NodeId.Content(draft.ToCanonicalBytes(tolerance).Span));
    }

    static (double R, double G, double B) Lin(IfcColourRgb c) => (Linearize(c.Red), Linearize(c.Green), Linearize(c.Blue));

    // --- [EGRESS] -------------------------------------------------------------------------
    // The inverse half the Projection/egress#IFC_EGRESS Emit composes per Object node carrying an appearance:
    // re-author the surface style from the neutral summary AND its texture roster. The scene-linear base colour
    // encodes back to display sRGB through Encode; ForPbr picks the IFC reflectance method from the neutral PBR; a
    // metal tints its specular from the base colour, a dielectric reflects neutral; a transmissive appearance also
    // authors an IfcSurfaceStyleRefraction at DefaultRefractionIndex — the neutral summary carries no IOR, so the
    // precise refraction index/dispersion is NOT round-tripped here (a Rasm.Materials BSDF concern the thin summary
    // deliberately drops) and only the transmissive SIGNAL round-trips.
    //
    // ONE ctor spelling, always the 5-arg overload: transmission and texturing are INDEPENDENT axes, so a ternary
    // ladder over them grows an arm per combination while the 5-arg form takes each axis as a null-or-value slot
    // and stays one expression as the axes grow (lighting and externally-defined style are the two remaining
    // slots, each a row this projector fills the moment its ingest arm lands). GeometryGym resolves the ctor's
    // DatabaseIfc from the first non-null slot, so the rendering leads and every other slot may be null.
    public static IfcStyledItem Author(DatabaseIfc db, AppearanceSummary summary, Seq<SurfaceTexture> textures) {
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
        IfcSurfaceStyle style = new(
            rendering,
            null,
            // IfcSurfaceStyleWithTextures admits only a NON-EMPTY texture list (its ctor reads textures[0] for the
            // database), so an untextured appearance leaves the slot null rather than authoring an empty style.
            textures.IsEmpty ? null : new IfcSurfaceStyleWithTextures([.. textures.Map(texture => texture.Author(db))]),
            null,
            transmissive ? new IfcSurfaceStyleRefraction(db) { RefractionIndex = DefaultRefractionIndex } : null) {
            Side = IfcSurfaceSide.BOTH,
        };
        return new IfcStyledItem(style);
    }

    // The sRGB transfer pair (IEC 61966-2-1): Linearize lowers a display-referred IFC channel to scene-linear at
    // import, Encode encodes a scene-linear channel back to display at egress — a pure host-neutral TRANSFER, NOT
    // a working-space/primaries conversion (that stays the Rasm.Materials Unicolour owner's concern). The pair is
    // PUBLIC because it is this package's ONE sRGB curve: the Exchange/export#EXPORT_RAIL dotbim Color column
    // projects the same scene-linear summary to display bytes through Encode, and a second copy of the piecewise
    // curve there would fork the round-trip this projector's own egress proves. ALPHA never takes the curve.
    public static double Linearize(double c) {
        double s = Math.Clamp(c, 0.0, 1.0);
        return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
    public static double Encode(double c) {
        double l = Math.Clamp(c, 0.0, 1.0);
        return l <= 0.0031308 ? l * 12.92 : 1.055 * Math.Pow(l, 1.0 / 2.4) - 0.055;
    }
}
```

## [03]-[RESEARCH]

- [SURFACE_STYLE_MEMBERS]: the GeometryGym presentation surface is verified against the live `GeometryGymIFC_Core` 25.7.30 decompile and the `.api/api-geometrygym-ifc` presentation rows — `IfcStyledItem.Styles` (`SET<IfcStyleAssignmentSelect>`) + `BaseClassIfc.Extract<IfcSurfaceStyle>()` (the version-agnostic traversal flattening an IFC2x3 `IfcPresentationStyleAssignment` wrapper) + ctor `IfcStyledItem(IfcStyleAssignmentSelect)` (an `IfcSurfaceStyle` satisfying it via `IfcPresentationStyle`); `IfcSurfaceStyle.Side` (`IfcSurfaceSide` `BOTH`/`POSITIVE`/`NEGATIVE`, default `BOTH`) + `.Styles` (`SET<IfcSurfaceStyleElementSelect>`) + ctors `IfcSurfaceStyle(IfcSurfaceStyleShading)` and the 5-arg `IfcSurfaceStyle(IfcSurfaceStyleShading, IfcSurfaceStyleLighting, IfcSurfaceStyleWithTextures, IfcExternallyDefinedSurfaceStyle, IfcSurfaceStyleRefraction)` that combines the rendering + refraction in one style; `IfcSurfaceStyleShading.SurfaceColour` (`IfcColourRgb`) + `.Transparency` (`double`, default `double.NaN` unset sentinel) + ctor `IfcSurfaceStyleShading(IfcColourRgb)`; `IfcSurfaceStyleRendering : IfcSurfaceStyleShading` carrying `DiffuseColour`/`TransmissionColour`/`DiffuseTransmissionColour`/`ReflectionColour`/`SpecularColour` (`IfcColourOrFactor` — a TWO-arm select whose implementors are `IfcColourRgb` and `IfcNormalisedRatioMeasure : IfcMeasureValue` with `.Measure : double` clamped [0,1] at its ctor; the diffuse read discriminates both arms, the factor scaling the surface colour), `SpecularHighlight` (`IfcSpecularHighlightSelect`), and `ReflectanceMethod` (`IfcReflectanceMethodEnum`) + ctor `IfcSurfaceStyleRendering(IfcColourRgb surfaceColour)`; `IfcSpecularHighlightSelect` is `IfcSpecularRoughness` (`.SpecularRoughness`, clamped [0,1] at its ctor — a direct PBR roughness) or `IfcSpecularExponent` (`.SpecularExponent`, a Phong exponent); `IfcSurfaceStyleRefraction.RefractionIndex`/`.DispersionFactor` (`double`) + ctor `IfcSurfaceStyleRefraction(DatabaseIfc)`; `IfcColourRgb.Red`/`.Green`/`.Blue` (`double`) + ctor `IfcColourRgb(DatabaseIfc, double, double, double)` (the `(DatabaseIfc, Color)` ctor and the `Color()` accessor are the host-coupled `System.Drawing.Color` members this host-neutral owner never touches); `IfcReflectanceMethodEnum` = `BLINN`/`FLAT`/`GLASS`/`MATT`/`METAL`/`MIRROR`/`PHONG`/`PLASTIC`/`STRAUSS`/`NOTDEFINED` (the `ReflectanceModel` `[SmartEnum<string>]` partition). The `IfcSpecularHighlightSelect`/`IfcSpecularRoughness`/`IfcSpecularExponent` concrete-type rows are decompile-confirmed beyond the catalogue's current `IfcSurfaceStyleRendering` row 05 summary.
- [TEXTURE_STYLE_MEMBERS]: the GeometryGym texture surface carries the shapes `.api/api-geometrygym-ifc` rosters — `IfcSurfaceStyleWithTextures : IfcPresentationItem, IfcSurfaceStyleElementSelect` carrying `Textures` (`LIST<IfcSurfaceTexture>`) + ctors `(IfcSurfaceTexture)` and `(List<IfcSurfaceTexture>)`, BOTH of which read a texture for the database, so an empty roster has no constructible style and the egress leaves the slot null; abstract `IfcSurfaceTexture : IfcPresentationItem` carrying `RepeatS`/`RepeatT` (`bool`), `Mode` (`string` — IFC4 types it a free `IfcIdentifier` while the IFC2x3 STEP leg parses it against `IfcSurfaceTextureEnum` = `NOTDEFINED`/`BUMP`/`OPACITY`/`REFLECTION`/`SELFILLUMINATION`/`SHININESS`/`SPECULAR`/`TEXTURE`/`TRANSPARENCYMAP`, which is why the `TextureMode` roster spans that legacy set AND the modern glTF-aligned spellings), `TextureTransform` (`IfcCartesianTransformationOperator2D`), and `Parameter` (`List<string>` — the UV-coordinate-name list this projector does not read, the coordinate binding riding `IfcTextureCoordinate` on the representation item rather than the style); `IfcImageTexture.UrlReference` (`string`, spelled `Url` not `URL`) + ctor `(DatabaseIfc, bool, bool, string)`; `IfcBlobTexture.RasterFormat` (`string`) + `.RasterCode` (`IfcBinary`, whose `Binary` member is the raw `byte[]` and whose `ValueString` is the STEP hex render) + ctor `(DatabaseIfc, bool, bool, string, IfcBinary)` with `IfcBinary(byte[])`; `IfcPixelTexture.Width`/`.Height`/`.ColourComponents` (`int`) + ctor `(DatabaseIfc, bool, bool, int, int, int, List<string>)` — the pixel run has NO public member and enters only through that constructor, so an ingested grid carries its extent alone and its re-author writes an empty run; `IfcCartesianTransformationOperator2D` ctors `(DatabaseIfc)` (origin-anchored) and `(IfcCartesianPoint)` over the base `IfcCartesianTransformationOperator` members `Axis1`/`Axis2` (`IfcDirection`, `.DirectionRatioX`/`.DirectionRatioY`), `LocalOrigin` (`IfcCartesianPoint`, `.CoordinateX`/`.CoordinateY`), and `Scale` (`double`, 0 when the optional STEP field was unset); `IfcSurfaceStyle`'s five-slot ctor `(IfcSurfaceStyleShading, IfcSurfaceStyleLighting, IfcSurfaceStyleWithTextures, IfcExternallyDefinedSurfaceStyle, IfcSurfaceStyleRefraction)` resolves its `DatabaseIfc` from the first non-null slot and adds each non-null select, so one spelling serves every axis combination.
- [SEAM_APPEARANCE_OWNERSHIP]: the seam owns the appearance node — `ELEMENT-REBUILD-PLAN.md` §4B (`Node` `[Union]` carries an `Appearance` case = a content-keyed `AppearanceSummary`) and the seam `Graph/element#NODE_MODEL` `AppearanceSummary(UInt128 AppearanceKey, double BaseColorR, double BaseColorG, double BaseColorB, double Metallic, double Roughness, double Opacity, bool Transmissive)` "a content-keyed reference to the full BSDF (authored in Rasm.Materials) plus the neutral canonical PBR scalars a consumer reads flat without the full lobe graph" — the `Transmissive` refractive bit DISTINCT from `Opacity` (alpha) so a refractive opaque-alpha glass round-trips and the GLB `KHR_materials_transmission` channel reads it apart from the alpha, the seam record carrying it beside the `AppearanceSummary.Of` factory — so this page projects the GeometryGym presentation graph onto the seam summary and mints the seam `Node.Appearance` through `NodeId.Content` over `Node.ToCanonicalBytes` re-stamped by the seam `Node.Relabel` (a class-root `[Union]` `Node` case generates no `with`, so the `draft with { Id = … }` spelling is the deleted form — the one mint idiom the `Rasm.Materials` `ComponentProjector` shares), declaring no Bim appearance record; the `BimAppearance`/`AppearanceColor` record + the `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial.Option<BimAppearance>` carrier retirement grounds against §2 (two parallel unaligned element owners collapsed) and §4B (the consumer-facing `Element` is the `Bake` fold reading `element.Appearance` through the `Associate` edge, never a record nested in a material), mirroring the rebuilt `Semantics/classification#CLASSIFICATION_AXIS` and `Semantics/composition#MATERIAL_COMPOSITION` projector shape.
- [OPENPBR_CONTENT_KEY]: the `AppearanceKey` content-key reconciliation grounds the cross-folder seam — `ELEMENT-REBUILD-PLAN.md` §4-RT H7 (one seam-owned canonical value codec; the diff `ContentBytes` and the id hash SHARE it) and the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter` codec hashed through the `Projection/address#CONTENT_ADDRESS` `ContentAddress.Of` (the kernel seed-zero `XxHash128`, the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, and the Python/TypeScript peers) — so the `AppearanceKey` is the content hash over the neutral PBR vector (base colour + metalness + roughness + opacity + transmissive), minted by the SEAM-OWNED `Graph/element#NODE_MODEL` `AppearanceSummary.Of` factory both this projector and the `Rasm.Materials/Appearance/interchange#MATERIAL_WIRE` owner compose under the FROZEN eight-parameter arity — the seven preimage values plus the `Op` key, returning `Fin<AppearanceSummary>` — while the raw-IEEE-bit writer stays INSIDE the factory, so neither caller can pass a tolerance and neither can fork the dedup key by passing the document one (never two convention-coupled local `CanonicalWriter` assemblies that could fork the byte order) — every neutral PBR scalar — base colour, metalness, roughness, opacity, AND transmissive — is load-bearing in the key because the seam `Node.ToCanonicalBytes` appearance arm writes ONLY the `AppearanceKey` (the key already folds the whole vector through `AppearanceSummary.Of`, so re-writing the scalars on the node arm would be a redundant second copy), the `AppearanceKey` carrying the full vector into node identity so two appearances differing only in alpha or in the refractive flag get distinct `Node.Appearance` ids — and a BIM-imported `IfcSurfaceStyleRendering` style and a `Rasm.Materials` OpenPBR row describing the same surface dedup to one content key and one `Node.Appearance` id; the `Rasm.Materials` owner mints the full OpenPBR vector once (`surface#OPENPBR_SLAB` `OpenPbrSurface`, `surface#CONDUCTOR_IOR` `ConductorMetal`) and this page never re-derives it — the `ReflectanceModel` `Metalness`/`Transmissive` bias is the IFC-side seed a downstream `Rasm.Materials` mapping reads for `BaseMetalness`/transmission when an authored OpenPBR row is absent, never a re-mint of the branch OpenPBR algebra in `Rasm.Bim`.
- [COLOR_TRANSFER]: the sRGB `Linearize`/`Encode` transfer pair is the IEC 61966-2-1 sRGB EOTF/OETF (the `0.04045`/`0.0031308` piecewise thresholds, the `12.92` linear segment, the `((x+0.055)/1.055)^2.4` curve) authored exactly — IFC presentation colour carries no declared colour space and authoring tools emit display-referred sRGB, so the TRANSFER linearization to scene-linear is the correct lowering onto the seam `BaseColor` the `Rasm.Materials` scene-linear convention expects; the working-space PRIMARIES conversion (Rec.709→AP1/ACEScg) is a separate concern the `Rasm.Materials/Appearance/surface#SPECTRAL_UPSAMPLE` Unicolour owner holds (`RgbConfiguration.Acescg` + `.RgbLinear`), so this host-neutral leaf carries the transfer-curve math (a pure closed form, no `Unicolour` object) and defers the gamut/primaries to the Materials owner — the prior raw-channel pass-through that called the unlinearized clamp "scene-linear" is the corrected defect.
