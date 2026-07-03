# [MATERIALS_FINISH]

THE KUBELKA-MUNK PIGMENT/COAT-STACK FINISH ENGINE. One `Finish.Resolve` static fold over a `FinishMix` pigment-weight vector and a `FinishLayer` coat stack produces a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying a spectrally-grounded scene-linear `BaseColor` and measured `Provenance`, so an architectural finish — a paint, an enamel, a lacquer, a varnish, a stain, a plaster, a limewash, a powder coat, a suede-effect coat, a metal-flake basecoat, a brushed-metal coat, a pearlescent mica coat, an anodized oxide film — is a PIGMENT MIX PLUS A COAT STACK rather than a hand-keyed base-color triple. A finish is NEVER a second appearance surface: `Finish.Resolve` admits the coat stack ONCE, mixes the `Pigment[]`/`double[]` weight vector through the one admitted `new Unicolour(Configuration, Pigment[], double[])` Kubelka-Munk constructor under the pigments' own measurement space, grounds the mix into the ACEScg scene-linear pipeline through the `surface#SPECTRAL_UPSAMPLE` `SceneLinear` owner, composes each coat over the mix through the W3C `Unicolour.Blend(backdrop, BlendMode)` compositing algebra (never a hand-rolled channel lerp), gates the composite against the `graph#MATERIAL_LIBRARY` `SpectralAdmit` MacAdam spectral-limit bound, the `PointerAdmit` real-surface gamut, the per-kind `NearestChecker` ColorChecker drift policy, and the `HueConstant` Ebner-Fairchild constant-hue witness, and seeds ONE `MaterialParameters` row — the row's `Film` thin-film column carrying the pearlescent/anodized interference film — the SAME `graph#MATERIAL_GRAPH` evaluates and the SAME `bsdf#LOBE_FAMILY` shades. A `Paint`/`Stain`/`Plaster` type is the deleted form; the variation is a `FinishKind` config-as-value row carrying its `FinishHandling` behavior columns, and `FinishKind.Seed` is ONE derivation over those columns — never a per-kind `Switch` arm, never a flag set. Kubelka-Munk reflectance mixing is OWNED by the Unicolour `Pigment` constructor and the `Wacton.Unicolour.Datasets` `ArtistPaint` Golden 19-pigment set; Materials NEVER re-derives the two-constant scattering math. The page composes `graph#MATERIAL_LIBRARY` for the produced row plus its `SpectralAdmit`/`PointerAdmit`/`NearestChecker`/`HueConstant` admission predicates (imported by domain, never a second gamut owner), the `graph#MATERIAL_LIBRARY` `ThinFilm` carrier for the interference film the topcoat or the kind row seeds, Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner, an `Option<MaterialParameters>` substrate so a stain or varnish composites over the real row it coats (the named `PrimedGround` canonical when none is supplied), and the `MaterialFault` (`FaultBand.Material`) rail for a Pointer-unreproducible reflectance, a hue-shifted tint, an out-of-unit coat, or a degenerate mix.

## [01]-[INDEX]

- [01]-[FINISH]: the `FinishKind` `[SmartEnum<string>]` discriminant — fourteen architectural finish-system rows (paint · enamel · lacquer · coating · varnish · stain · plaster · limewash · powdercoat · suede · metallic · brushed · pearlescent · anodized), each a `FinishHandling` behavior row — the `FinishMix` pigment-weight vector resolving through the admitted Kubelka-Munk constructor over the `ArtistPaint` set, the `FinishLayer` `[Union]` primer/base/glaze/topcoat coat stack composing through the W3C `BlendMode` algebra, and the one `Finish.Resolve` fold producing a `graph#MATERIAL_LIBRARY` `MaterialParameters` row (its `Film` column the pearlescent/anodized interference carrier) with measured `Provenance`.

## [02]-[FINISH]

- Owner: `Finish` static resolve fold; `FinishKind` `[SmartEnum<string>]` the finish-system discriminant whose fourteen rows ARE the finish space; `FinishHandling` the config-as-value behavior row (hiding, gloss, transmission, transmission roughness, coat bias, IOR, metalness, anisotropy, sheen, specular tint, the substrate `BlendMode`, the `DeltaE` drift policy, the hue-constancy tolerance, the `ThinFilm` seed); `FinishMix` the `Pigment[]`/`double[]` Kubelka-Munk weight vector; `FinishLayer` `[Union]` the primer/base/glaze/topcoat ordered coat stack; `FinishPigment` the `ArtistPaint`-backed catalogue resolving a pigment's own measured `Name` to its `Pigment` reflectance value and exposing the `MeasurementSpace` the mix runs under.
- Cases: kind {`Paint` (opaque dispersion wall paint, mid-rough), `Enamel` (hard alkyd/oil gloss trim film), `Lacquer` (thin fast-dry film reading as a built coat, `ClearcoatBias` 0.5), `Coating` (a thin tinted topcoat over a substrate, bias 0.6), `Varnish` (translucent amber wood film, `Multiply` over the substrate, coat-forward), `Stain` (penetrating translucent, `Multiply` over the substrate, `Transmission` 0.5 at `TransmissionRoughness` 0.35 — the in-film scatter), `Plaster` (opaque high-scattering near-Lambertian; a sealed/polished plaster takes its coat from the stack, never a hardcoded zero), `Limewash` (breathable semi-hiding mineral coat, ultra-matte), `Powdercoat` (electrostatic polyester on architectural metal, satin, IOR 1.55), `Suede` (flocked suede-effect coat, `Sheen` 0.80), `Metallic` (metal-flake basecoat, `Metalness` 0.85, always clearcoated), `Brushed` (directional brushed-metal coat — brushed stainless/aluminum trim — `Metalness` 1.0, `Anisotropy` 0.65 along the brushing grain), `Pearlescent` (TiO2-coated mica interference basecoat, `Film` 380 nm at IOR 2.0 under a clearcoat), `Anodized` (electrolytic Al2O3 oxide on aluminum, `Metalness` 1.0, `Film` 220 nm at IOR 1.65)} — the closed finish-system family; a finish is a `FinishKind` row carrying its `FinishHandling` columns, never a finish subtype, an `IsOpaque` bool ladder, or a per-kind `Seed` arm. layer {`Primer` (hiding undercoat, `Normal` composite), `Base` (pigment-bearing color coat, `Normal` composite), `Glaze` (translucent decorative effect coat compositing by its OWN named `BlendMode`), `Topcoat` (protective clear/tinted coat — its tint `Multiply`-filters the composite, its `ThinFilm` rides to the row `Film` column, its weight/roughness seed the `Clearcoat` columns)} — the ordered coat stack substrate-to-outermost (each layer composites OVER the color below it, the topcoat last), a `FinishLayer` `[Union]` case, never a per-coat type.
- Entry: `public static Fin<(MaterialParameters Row, Provenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key, Option<MaterialParameters> substrate = default)` — the resolve fold admitting the coat stack ONCE (`AdmitStack` faults any non-finite or out-of-unit layer weight/roughness, so `Compose` never re-clamps), mixing the `FinishMix` through the admitted Kubelka-Munk constructor under the `ArtistPaint` sRGB/D50 `Configuration`, rebasing to ACEScg and grounding through the `surface#SPECTRAL_UPSAMPLE` `SceneLinear` owner, folding the coat stack over the mix through `Unicolour.Blend`, gating the composite through the imported `graph#MATERIAL_LIBRARY` ladder — `SpectralAdmit` then `PointerAdmit` then `NearestChecker(composite, row.DriftTolerance, row.Drift, key)` under the KIND'S OWN `DeltaE` metric (`Ciede2000` for pigment paints, `Cam16` for effect finishes, `Hyab` for the large-difference stain/varnish composites) then `HueConstant(composite, mix, row.HueTolerance, key)` witnessing the composite against the mix's Ebner-Fairchild constant-hue locus so a tint that walks off-hue rails rather than admitting a shifted color — seeding the row through the ONE `FinishKind.Seed` derivation over the handling columns (the substrate ground `substrate.Map(s => s.BaseColor).IfNone(PrimedGround)` so a stain over `wood.oak` composites over the REAL row it coats), landing the merged interference film (topcoat film wins over the kind seed) on the row's `Film` column, and re-admitting through `MaterialParameters.Of`; `Fin<T>` aborts on an empty or weight-mismatched mix (`MaterialFault.Parameter`), an out-of-unit coat weight or roughness (`MaterialFault.Parameter`, the stack admission), a Pointer-unreproducible reflectance, a ColorChecker drift beyond the row tolerance, or a hue-shifted tint (`MaterialFault.Gamut`, the case reused); arity is one — a multi-layer finish folds the `FinishLayer` `Seq` substrate-to-outermost (the left fold composites each successive layer OVER the accumulated color, so the topcoat lands last), never a per-layer method.
- Packages: Wacton.Unicolour (composed — `new Unicolour(Configuration, Pigment[], double[])` Kubelka-Munk weighted pigment mix under the pigments' own measurement space, `new Pigment(...)` single/two-constant construction, `Configuration`, `ConvertToConfiguration` for the scene-space rebase, `Blend(backdrop, BlendMode)` the W3C separable/non-separable compositing algebra with coverage riding the source alpha, `Mix` the scene-linear lerp, the `DeltaE` metric selector the drift policy names, the `.RgbLinear` accessor), Wacton.Unicolour.Datasets (composed — the `ArtistPaint` Golden 19-pigment `Pigment` reflectance set, `ArtistPaint.All`, the per-pigment `Pigment.Name`, and the `ArtistPaint.Configuration` sRGB/D50 working space; the `EbnerFairchild` constant-hue loci and `Macbeth` patches consumed through the imported `graph#MATERIAL_LIBRARY` gates), `surface#SPECTRAL_UPSAMPLE` (composed — `SpectralUpsample.SceneLinear` the ONE grounding owner over the `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg working space), `graph#MATERIAL_LIBRARY` (composed — `MaterialParameters` + `ThinFilm` + the four-gate admission ladder), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new finish system is one `FinishKind` row — fourteen rows already span opaque paints, solvent films, over-substrate translucents, mineral coats, industrial polymer, sheened decorative, and effect/metal finishes (metallic, brushed, pearlescent, anodized); the row IS the behavior, `Seed` never grows an arm. A new coat role is one `FinishLayer` `[Union]` case naming its composite; a new blend behavior is a `BlendMode` member already owned by Unicolour, selected as a `Glaze` row value; a new drift policy is a `DeltaE` member on the kind row. A new pigment is one `FinishPigment` row binding an `ArtistPaint` handle (or a `new Pigment` measured-reflectance construction) — the Kubelka-Munk mix is the closed admitted constructor, a pigment is a reflectance value not a mixing class; a measured pigment reflectance curve admitted from a spectrophotometer lands as one `new Pigment(start, interval, k, s, k1, k2, name)` two-constant construction the SAME mix consumes. The finish output aligns to the OpenPBR `base`/`specular`/`coat`/`fuzz`/`thin_film` groups through the row columns the `surface#OPENPBR_SLAB` lowering reads and the `interchange#MATERIAL_WIRE` projects.
- Boundary: `Finish.Resolve` is the ONE finish path — a `PaintFinish`/`StainFinish` type is the deleted form; the `FinishMix` weight vector resolves to a reflectance EXCLUSIVELY through the admitted `new Unicolour(FinishPigment.MeasurementSpace, mix.Pigments, mix.Weights)` Kubelka-Munk constructor under the `ArtistPaint.Configuration` sRGB/D50 working space the pigments were measured in, then `ConvertToConfiguration(PortValue.SceneLinear)` rebases the mix to the ONE ACEScg scene-linear `Configuration` instance and `surface#SPECTRAL_UPSAMPLE` `SceneLinear` grounds it — the SAME grounding owner `acquisition#ACQUISITION` `GroundSpectral` composes, never a parallel inline scene-linear construction nor a default-D65 mix mislabelled as scene-linear — so Materials NEVER re-derives the two-constant `K/S` scattering and a finish color IS the Kubelka-Munk mix of measured pigments grounded into the one pipeline, not an authored triple; layer compositing is `Finish.Composite` — the layer's coverage rides the SOURCE ALPHA into `Unicolour.Blend(backdrop, mode)` so ONE library call runs the named W3C blend AND the alpha-composited coverage (under the linear ACEScg working space the blend's encoded-Rgb domain IS scene-linear, and reflectance channels stay in `[0,1]` where the W3C algebra is total) — the prior `substrate.Mix(pigment, …)` linear lerp that LIGHTENED a translucent stain toward its pigment is the deleted form, a stain/varnish/glaze now `Multiply`-darkens its substrate as the physics demands; the coat stack is admitted ONCE at `AdmitStack` (`BOUNDARY_ADMISSION`: every weight/roughness proven finite in `[0,1]` before the fold, the interior clamp-free), a violation railing `MaterialFault.Parameter` with the layer role — the prior fence that PROMISED an out-of-unit-coat fault while silently clamping is the deleted illusion; the four-gate admission ladder is imported from `graph#MATERIAL_LIBRARY` by domain — MacAdam spectral limit, Pointer real-surface, ColorChecker drift under the ROW'S `DeltaE` policy, Ebner-Fairchild hue constancy — never a second gamut owner, and the drift/hue tolerances are `FinishHandling` POLICY VALUES, never a page-level const; `FinishKind.Seed` is ONE expression over the handling row — hiding is `BaseWeight` (a translucent kind composites over the substrate ground by its `Substrate` blend row), gloss is the row roughness, the coat is the stack topcoat floored by `ClearcoatBias`, metalness/anisotropy/sheen/specular-tint/transmission-roughness/IOR are row columns — so the fourteen kinds share one derivation and a fifteenth kind is one row with ZERO dispatch edits (the prior four-arm `Switch`, and its hardcoded `Metalness: 0.0`/`Sheen: 0.0` that made metallic, pearlescent, and sheened finishes UNREACHABLE, are the deleted forms); the substrate enters as `Option<MaterialParameters>` — a stain composites over the row it coats, the `PrimedGround` near-white the named canonical when none is supplied (the prior hardcoded constant that made "rides the substrate" a fiction is the deleted form); the interference film lands on the row's `Film` `ThinFilm` column (topcoat film over kind seed), the `surface#OPENPBR_SLAB` lowering reading it into the `thin_film` group and the `Slab.Coat` interference lobe — the prior path that wrote the film onto a `Slab.Coat` inside a DISCARDED validation binding, so no pearlescent finish could ever reach shading, is the deleted illusion, and the full coat-stack-to-`LayeredBsdf` lowering now happens exactly once downstream of the row, never re-derived here; the produced row carries `Provenance` (the pigment count and the resolved-mix evidence a hand-keyed triple lacks) and re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so a finish row passes the same gamut/unit/IOR gate a registered row passes, and an empty mix, a pigment/weight length mismatch, or a non-finite reflectance rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;                         // Seq, Option, Fin
using Rasm.Domain;                         // Op
using Rasm.Materials.Appearance.Bsdf;      // MaterialFault (FaultBand.Material, composed from bsdf#SHADING_FRAME)
using Rasm.Materials.Appearance.Graph;     // MaterialParameters, MaterialLibrary, SubsurfaceRadius, ThinFilm, PortValue (the SceneLinear Acescg Configuration)
using Rasm.Materials.Appearance.Surface;   // SpectralUpsample (the ONE grounding owner)
using Thinktecture;                        // ComparerAccessors, [SmartEnum]/[Union]/[KeyMember*], ConversionOperatorsGeneration
using Wacton.Unicolour;                    // Unicolour, Pigment, Configuration, ColourSpace, BlendMode, DeltaE
using Wacton.Unicolour.Datasets;           // ArtistPaint (the Golden pigment set + its measurement Configuration)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;       // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters and acquisition#ACQUISITION Provenance

// --- [TYPES] -------------------------------------------------------------------------------
// Fourteen architectural finish SYSTEMS as behavior rows — dispersion paint, alkyd enamel, nitro lacquer, tinted
// coating, wood varnish, penetrating stain, gypsum/lime plaster, limewash, Qualicoat powder coat, suede-effect coat,
// AAMA metal-flake basecoat, brushed stainless/aluminum, TiO2-mica pearlescent, AAMA-611 anodized oxide. Seed is ONE
// derivation over the row; a new system is one row, never a Switch arm.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FinishKind {
    public static readonly FinishKind Paint       = new("paint",       new(1.00, 0.45, 0.0, 0.0, 1.50));
    public static readonly FinishKind Enamel      = new("enamel",      new(1.00, 0.15, 0.0, 0.0, 1.52, DriftTolerance: 10.0));
    public static readonly FinishKind Lacquer     = new("lacquer",     new(1.00, 0.08, 0.0, 0.5, 1.50, DriftTolerance: 10.0));
    public static readonly FinishKind Coating     = new("coating",     new(1.00, 0.25, 0.0, 0.6, 1.50, HueTolerance: 8.0));
    public static readonly FinishKind Varnish     = new("varnish",     new(0.30, 0.20, 0.0, 0.7, 1.52, Substrate: BlendMode.Multiply, Drift: DeltaE.Hyab, DriftTolerance: 16.0, HueTolerance: 8.0));
    public static readonly FinishKind Stain       = new("stain",       new(0.35, 0.55, 0.5, 0.0, 1.50, TransmissionRoughness: 0.35, Substrate: BlendMode.Multiply, Drift: DeltaE.Hyab, DriftTolerance: 16.0, HueTolerance: 8.0));
    public static readonly FinishKind Plaster     = new("plaster",     new(1.00, 0.85, 0.0, 0.0, 1.50, HueTolerance: 12.0));
    public static readonly FinishKind Limewash    = new("limewash",    new(0.85, 0.95, 0.0, 0.0, 1.49, DriftTolerance: 14.0, HueTolerance: 12.0));
    public static readonly FinishKind Powdercoat  = new("powdercoat",  new(1.00, 0.35, 0.0, 0.0, 1.55, DriftTolerance: 10.0));
    public static readonly FinishKind Suede       = new("suede",       new(1.00, 0.90, 0.0, 0.0, 1.50, Sheen: 0.80, SheenTint: 0.40));
    public static readonly FinishKind Metallic    = new("metallic",    new(1.00, 0.30, 0.0, 1.0, 1.50, Metalness: 0.85, Drift: DeltaE.Cam16, DriftTolerance: 10.0));
    public static readonly FinishKind Brushed     = new("brushed",     new(1.00, 0.35, 0.0, 0.0, 1.50, Metalness: 1.00, Anisotropy: 0.65, Drift: DeltaE.Cam16, DriftTolerance: 10.0));
    public static readonly FinishKind Pearlescent = new("pearlescent", new(1.00, 0.25, 0.0, 1.0, 1.50, Metalness: 0.20, Drift: DeltaE.Cam16, DriftTolerance: 10.0, HueTolerance: 8.0) { Film = ThinFilm.Create(1.0, 380.0, 2.0) });
    public static readonly FinishKind Anodized    = new("anodized",    new(1.00, 0.30, 0.0, 0.0, 1.50, Metalness: 1.00, Drift: DeltaE.Cam16, DriftTolerance: 10.0) { Film = ThinFilm.Create(1.0, 220.0, 1.65) });

    public FinishHandling Handling { get; }

    private FinishKind(string key, FinishHandling handling) : this(key) => Handling = handling;

    // ONE derivation over the handling row — no per-kind Switch: hiding is BaseWeight (a translucent kind composites
    // over the substrate ground by its blend row), the coat is the stack topcoat floored by ClearcoatBias, and the
    // merged interference film lands on the row's Film column the surface#OPENPBR_SLAB thin_film group reads.
    public MaterialParameters Seed(Unicolour composite, Unicolour substrate, double coatWeight, double coatRoughness, ThinFilm film) =>
        new(BaseColor: Handling.BaseWeight >= 1.0 ? composite : Finish.Composite(substrate, composite, Handling.Substrate, Handling.BaseWeight),
            Metalness: Handling.Metalness, Roughness: Handling.Roughness, SpecularTint: Handling.SpecularTint, Anisotropy: Handling.Anisotropy, Ior: Handling.Ior,
            Transmission: Handling.Transmission, TransmissionRoughness: Handling.TransmissionRoughness, Sheen: Handling.Sheen, SheenTint: Handling.SheenTint,
            Clearcoat: Math.Max(coatWeight, Handling.ClearcoatBias), ClearcoatRoughness: coatRoughness,
            Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Finish.Black, EmissionLuminance: 0.0) { Film = film };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FinishLayer {
    private FinishLayer() { }

    public sealed record Primer(double Weight, Unicolour Reflectance) : FinishLayer;
    public sealed record Base(double Weight, Unicolour Reflectance) : FinishLayer;
    public sealed record Glaze(double Weight, Unicolour Tint, BlendMode Blend) : FinishLayer;
    public sealed record Topcoat(double Weight, double Roughness, Option<Unicolour> Tint, ThinFilm Film) : FinishLayer;

    public string Role => Switch(primer: static _ => "primer", @base: static _ => "base", glaze: static _ => "glaze", topcoat: static _ => "topcoat");

    // The once-at-admission proof AdmitStack reads: every layer weight/roughness finite in [0,1], so Compose is clamp-free.
    public bool Admissible => Switch(
        primer:  static p => Finish.Unit(p.Weight),
        @base:   static b => Finish.Unit(b.Weight),
        glaze:   static g => Finish.Unit(g.Weight),
        topcoat: static t => Finish.Unit(t.Weight) && Finish.Unit(t.Roughness));

    // Substrate-out composite — each layer composites OVER the color below it, so the left fold applies the topcoat
    // LAST: primer/base hide (Normal), a glaze composites by its OWN blend row, a tinted topcoat Multiply-filters the
    // color below it; the topcoat's weight/roughness/film seed the row coat columns instead.
    public Unicolour Compose(Unicolour below) => Switch(
        state: below,
        primer:  static (b, p) => Finish.Composite(b, p.Reflectance, BlendMode.Normal, p.Weight),
        @base:   static (b, l) => Finish.Composite(b, l.Reflectance, BlendMode.Normal, l.Weight),
        glaze:   static (b, g) => Finish.Composite(b, g.Tint, g.Blend, g.Weight),
        topcoat: static (b, t) => t.Tint.Match(Some: tint => Finish.Composite(b, tint, BlendMode.Multiply, t.Weight), None: () => b));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The config-as-value behavior row: hiding/gloss/transmission/coat-bias/IOR positional, the effect columns
// (metalness · anisotropy · sheen · sheen tint · specular tint · transmission roughness — every MaterialParameters
// column the finish space reaches is a row value, never a Seed literal that walls a column off), the substrate
// BlendMode, the per-kind DeltaE drift policy, the hue-constancy tolerance, and the init-defaulted ThinFilm seed —
// POLICY VALUES the one Seed derivation reads.
public readonly record struct FinishHandling(
    double BaseWeight, double Roughness, double Transmission, double ClearcoatBias, double Ior,
    double Metalness = 0.0, double Anisotropy = 0.0, double Sheen = 0.0, double SheenTint = 0.0, double SpecularTint = 0.0, double TransmissionRoughness = 0.0,
    BlendMode Substrate = BlendMode.Normal, DeltaE Drift = DeltaE.Ciede2000, double DriftTolerance = 12.0, double HueTolerance = 10.0) {
    public ThinFilm Film { get; init; } = ThinFilm.None;
}

public readonly record struct FinishMix(Seq<Pigment> Pigments, Seq<double> Weights) {
    public static Fin<FinishMix> Of(Seq<Pigment> pigments, Seq<double> weights, Op key) =>
        pigments.IsEmpty
            ? MaterialFault.Parameter(key, "<finish-mix-empty>")
            : pigments.Count != weights.Count
                ? MaterialFault.Parameter(key, $"<finish-mix-arity:{pigments.Count}!={weights.Count}>")
                : weights.Exists(static w => !double.IsFinite(w) || w < 0.0)
                    ? MaterialFault.Parameter(key, "<finish-mix-weight-negative>")
                    : weights.Sum() <= 0.0
                        ? MaterialFault.Parameter(key, "<finish-mix-weight-zero>")
                        : Fin.Succ(new FinishMix(pigments, weights));

    // The Kubelka-Munk mix runs under the pigments' OWN measurement working space (ArtistPaint sRGB/D50), then rebases
    // to the ACEScg scene-linear space and grounds through the ONE surface#SPECTRAL_UPSAMPLE SceneLinear owner — the
    // same grounding gate acquisition#ACQUISITION GroundSpectral composes — so the reflectance enters the pipeline as a
    // gamut-pulled, finite-gated, genuinely-Acescg scene-linear Unicolour, never a default-D65 mix mislabelled as scene.
    public Fin<Unicolour> Reflectance(Op key) =>
        SpectralUpsample.SceneLinear(
                new Unicolour(FinishPigment.MeasurementSpace, Pigments.ToArray(), Weights.ToArray())
                    .ConvertToConfiguration(PortValue.SceneLinear), key)
            .Map(static rgb => new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R, rgb.G, rgb.B))
            .MapFail(_ => MaterialFault.Gamut(key, "<finish-mix-non-finite-reflectance>"));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class FinishPigment {
    // The Golden 19-pigment set keyed by each Pigment's OWN measured Name (kebab-cased), never a hand-keyed
    // positional index→name table: the key is DERIVED from the authoritative ArtistPaint.<Pigment>.Name the
    // Datasets assembly ships ("BoneBlack"→"bone-black", "PhthaloGreenBlueShade"→"phthalo-green-blue-shade"),
    // so the catalogue can never drift from the set's nomenclature and a resolve names a pigment that exists.
    public static readonly FrozenDictionary<string, Pigment> Catalogue =
        ArtistPaint.All.ToFrozenDictionary(static p => Slug(p.Name), static p => p, StringComparer.Ordinal);

    // The sRGB/D50 working space the Golden pigments were measured under — the Kubelka-Munk mix in FinishMix.Reflectance
    // runs under THIS working space so the K/S reflectance reads its measurement white point, never Configuration.Default.
    public static readonly Configuration MeasurementSpace = ArtistPaint.Configuration;

    public static Fin<Pigment> Resolve(string name, Op key) =>
        Catalogue.TryGetValue(Slug(name), out Pigment? pigment)
            ? Fin.Succ(pigment!)
            : MaterialFault.Parameter(key, $"<unregistered-pigment:{name}>");

    // PascalCase measured Name → kebab key: a boundary before each upper run (skipping the first char), lowered.
    static string Slug(string name) =>
        string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? $"-{char.ToLowerInvariant(c)}" : $"{char.ToLowerInvariant(c)}"));
}

public static class Finish {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    internal static readonly Unicolour Black = Linear(0.0, 0.0, 0.0);
    // The named canonical ground the Option substrate defaults to: a primed near-white, never an implicit hardcode.
    internal static readonly Unicolour PrimedGround = Linear(0.92, 0.92, 0.90);

    public static Fin<(MaterialParameters Row, Provenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key, Option<MaterialParameters> substrate = default) =>
        from layers in AdmitStack(stack, key)
        from reflectance in mix.Reflectance(key)
        let composed = layers.Fold(reflectance, static (below, layer) => layer.Compose(below))
        from admitted in Admit(composed, reflectance, kind.Handling, key)
        let top = TopcoatOf(layers)
        let seed = kind.Seed(admitted, substrate.Map(static s => s.BaseColor).IfNone(PrimedGround), top.Weight, top.Roughness,
            top.Film.Weight > 0.0 ? top.Film : kind.Handling.Film)
        from row in MaterialParameters.Of(seed, key)
        select (row, MixProvenance(mix));

    // BOUNDARY_ADMISSION: the coat stack is proven once — a non-finite or out-of-unit layer rails with its role,
    // making the out-of-unit-coat rail REAL (the prior fence promised the fault while silently clamping).
    static Fin<Seq<FinishLayer>> AdmitStack(Seq<FinishLayer> stack, Op key) =>
        stack.Find(static layer => !layer.Admissible)
            .Match(
                Some: bad => Fin.Fail<Seq<FinishLayer>>(MaterialFault.Parameter(key, $"<finish-layer-out-of-unit:{bad.Role}>")),
                None: () => Fin.Succ(stack));

    // The four-gate ladder by domain, tolerances the KIND'S policy row: MacAdam spectral limit → Pointer real-surface →
    // ColorChecker drift under the row's DeltaE metric → Ebner-Fairchild hue constancy of the composite vs the raw mix.
    static Fin<Unicolour> Admit(Unicolour composed, Unicolour mix, FinishHandling handling, Op key) =>
        MaterialLibrary.SpectralAdmit(composed, key)
            .Bind(spectral => MaterialLibrary.PointerAdmit(spectral, key))
            .Bind(surface => MaterialLibrary.NearestChecker(surface, handling.DriftTolerance, handling.Drift, key).Map(_ => surface))
            .Bind(anchored => MaterialLibrary.HueConstant(anchored, mix, handling.HueTolerance, key));

    // The stack is substrate-to-outermost, so the LAST topcoat is the outer protective coat whose weight/roughness/film
    // seed the row coat columns — a first-wins Head would silently shade an inner coat under a re-coated stack.
    static (double Weight, double Roughness, ThinFilm Film) TopcoatOf(Seq<FinishLayer> stack) =>
        stack.Fold(Option<(double Weight, double Roughness, ThinFilm Film)>.None,
                static (outer, l) => l is FinishLayer.Topcoat t ? Some((t.Weight, t.Roughness, t.Film)) : outer)
            .IfNone((0.0, 0.0, ThinFilm.None));

    // W3C blend+composite through the library in ONE call: the layer's coverage rides the SOURCE ALPHA into
    // Unicolour.Blend, which runs B(backdrop, layer) and the alpha-composited coverage lerp together. Under the linear
    // Acescg working space the blend's encoded-Rgb domain IS scene-linear; reflectance channels stay in [0,1] where the
    // W3C algebra is total. Coverage is admitted upstream (AdmitStack / row data), so no interior clamp.
    internal static Unicolour Composite(Unicolour below, Unicolour layer, BlendMode mode, double coverage) {
        ColourTriplet lin = layer.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, lin.First, lin.Second, lin.Third, coverage).Blend(below, mode);
    }

    internal static bool Unit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;

    // A Kubelka-Munk pigment mix is a MEASURED-pigment finish (the pigments carry measured K/S reflectance), so it stamps
    // the acquisition#ACQUISITION CaptureMethod.PigmentMix instrument through Provenance.Of — structurally != Authored, so
    // interchange#MATERIAL_WIRE WireProvenance.Measured reads true and Method/AngularSamples carry the instrument + the
    // pigment count over the wire. The pigment count rides AngularSamples (the sample-count analog); no per-wavelength band
    // count and no fit residual exist for a mix, so both are 0 — never the prior wavelengthCount=count / fitResidual=sum misuse.
    static Provenance MixProvenance(FinishMix mix) =>
        Provenance.Of(CaptureMethod.PigmentMix, $"kubelka-munk:{mix.Pigments.Count}-pigment", wavelengthCount: 0, angularSamples: mix.Pigments.Count, fitResidual: 0.0);
}
```

## [03]-[RESEARCH]

- [PIGMENT_MEASUREMENT_PROVENANCE]: the `ArtistPaint` Golden 19-pigment set is the seed Kubelka-Munk reflectance table — the two-constant `K/S` scattering coefficients measured under the sRGB/D50 `ArtistPaint.Configuration`, mixed through the admitted `new Unicolour(Configuration, Pigment[], double[])` constructor under that measurement space. A custom architectural finish (a measured masonry-coating reflectance, a manufacturer paint chip) admits as one `new Pigment(start, interval, k, s, k1, k2, name)` two-constant — or, for an opaque single-scatterer like a `Plaster` lime/gypsum coat, one `new Pigment(start, interval, r, name)` single-constant — construction the SAME `FinishMix` consumes, the probe the spectrophotometer-to-`Pigment` measurement protocol per finish system, not a re-architecture of the mix. The `FinishPigment.Catalogue` key is DERIVED from each `Pigment`'s own measured `Name` (kebab-cased through `Slug`), never a hand-keyed positional index→name table, so the catalogue cannot drift from the set's nomenclature and `Resolve("phthalo-green-blue-shade", key)` names a pigment the assembly actually carries; a measured pigment lands one named row under its own `Name`.
- [FINISH_ADMISSION_POLICY]: the `graph#MATERIAL_LIBRARY` admission runs four gates by domain, tolerances and metric the `FinishHandling` policy row. `SpectralAdmit` is the absolute MacAdam optimal-colour spectral-limit bound (`IsImaginary` first, `IsInMacAdamLimits` at the composite's luminance; the owner's `MapToSpectral` supplies the nearest spectrally-valid recovery). `PointerAdmit` is the Pointer real-surface gamut inside that bound — a composite no real pigment can reflect rails `MaterialFault.Gamut`, `MapToPointer` the recoverable projection. `NearestChecker` witnesses the composite against the nearest `Macbeth.All` ColorChecker patch under the ROW'S `DeltaE` metric — `Ciede2000` for pigment paints, `Cam16` (CAM16-UCS appearance difference) for the metallic/pearlescent/anodized effect rows whose angular color reads through an appearance model, `Hyab` for the stain/varnish composites whose large lightness-chroma distance from any patch anchor is exactly the regime HyAB measures better than CIEDE2000. `HueConstant` is the innermost witness — the composite must stay within the row's tolerance of the raw mix's nearest `EbnerFairchild` constant-hue locus, so a `Multiply` glaze driven hard enough to walk the tint off-hue rails rather than admitting a shifted color; the loci are the admitted Datasets constant-hue data read through the one `graph#MATERIAL_LIBRARY` reflection-derived table, `HungBerns` the alternate loci family held as reference. The probe is the per-finish-system tolerance calibration against measured captures, not a second gamut owner.
- [COAT_STACK_THIN_FILM]: REALIZED — the interference film is row DATA end-to-end: a `Pearlescent`/`Anodized` `FinishKind` row (or a measured `FinishLayer.Topcoat.Film`) seeds the `graph#MATERIAL_LIBRARY` `ThinFilm` carrier onto the produced row's `Film` column, the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` reads it into the OpenPBR `thin_film` group, and the `Slab.Coat` lowering carries it to the `bsdf#LOBE_FAMILY` `ThinFilm` interference lobe — the prior design wrote the film onto a coat slab inside a discarded validation binding, so pearlescence could never reach shading. The mica seed (380 nm at IOR 2.0) and the anodic-oxide seed (220 nm at IOR 1.65) are first-order interference anchors; the probe is the measured film thickness per coating system. The same coat columns the `weathering#WEATHERING` chalking trajectory raises (`coat_roughness`) remain the shared aging target, so a finish ages through the weathering operator over its own row, never a per-finish aging variant.
