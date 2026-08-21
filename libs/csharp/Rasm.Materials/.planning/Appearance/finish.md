# [MATERIALS_FINISH]

THE KUBELKA-MUNK PIGMENT/COAT-STACK FINISH ENGINE. One `Finish.Resolve` static fold over a `FinishMix` pigment-weight vector and a `FinishLayer` coat stack produces a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying a spectrally-grounded scene-linear `BaseColor` and a measured `acquisition#ACQUISITION` `CaptureProvenance`, so an architectural finish — a paint, an enamel, a lacquer, a varnish, a stain, a plaster, a limewash, a powder coat, a suede-effect coat, a metal-flake basecoat, a brushed-metal coat, a pearlescent mica coat, an anodized oxide film — is a PIGMENT MIX PLUS A COAT STACK rather than a hand-keyed base-color triple. A finish is NEVER a second appearance surface: `Finish.Resolve` admits the coat stack ONCE, mixes the `Pigment[]`/`double[]` weight vector through the one admitted `new Unicolour(Configuration, Pigment[], double[])` Kubelka-Munk constructor under the pigments' own measurement space, grounds the mix into the ACEScg scene-linear pipeline through the `surface#SPECTRAL_UPSAMPLE` `SceneLinear` owner, composes each coat over the mix through the W3C `Unicolour.Blend(backdrop, BlendMode)` compositing algebra (never a hand-rolled channel lerp), gates the composite against the `graph#MATERIAL_LIBRARY` `SpectralAdmit` MacAdam spectral-limit bound, the `PointerAdmit` real-surface gamut, the per-kind `NearestChecker` ColorChecker drift policy, and the `HueConstant` Ebner-Fairchild constant-hue witness, and seeds ONE `MaterialParameters` row — the row's `Film` thin-film column carrying the pearlescent/anodized interference film — the SAME `graph#MATERIAL_GRAPH` evaluates and the SAME `bsdf#LOBE_FAMILY` shades. A `Paint`/`Stain`/`Plaster` type is the deleted form; the variation is a `FinishKind` config-as-value row carrying its `FinishHandling` behavior columns, and `FinishKind.Seed` is ONE derivation over those columns — never a per-kind `Switch` arm, never a flag set. Kubelka-Munk reflectance MIXING is OWNED by the Unicolour `Pigment` constructor and the `Wacton.Unicolour.Datasets` `ArtistPaint` Golden 19-pigment set; Materials re-derives no forward scattering math and owns exactly one inverse the library has no member for — `PigmentCapture.Extract`, the two-background per-wavelength `K`/`S` separation that mints the custom `Pigment` that same constructor consumes. The page composes `graph#MATERIAL_LIBRARY` for the produced row plus its `SpectralAdmit`/`PointerAdmit`/`NearestChecker`/`HueConstant` admission predicates (imported by domain, never a second gamut owner), the `graph#MATERIAL_LIBRARY` `ThinFilm` carrier for the interference film the topcoat or the kind row seeds, Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner, an `Option<MaterialParameters>` substrate so a stain or varnish composites over the real row it coats (the named `PrimedGround` canonical when none is supplied), and the `MaterialFault` (`FaultBand.Appearance`) rail for a Pointer-unreproducible reflectance, a hue-shifted tint, an out-of-unit coat, or a degenerate mix.

## [01]-[INDEX]

- [02]-[FINISH]: the `FinishKind` `[SmartEnum<string>]` discriminant — fourteen architectural finish-system rows (paint · enamel · lacquer · coating · varnish · stain · plaster · limewash · powdercoat · suede · metallic · brushed · pearlescent · anodized), each a `FinishHandling` behavior row — the `FinishMix` pigment-weight vector resolving through the admitted Kubelka-Munk constructor over the `ArtistPaint` set, the `FinishLayer` `[Union]` primer/base/glaze/topcoat coat stack composing through the W3C `BlendMode` algebra, and the one `Finish.Resolve` fold producing a `graph#MATERIAL_LIBRARY` `MaterialParameters` row (its `Film` column the pearlescent/anodized interference carrier) with a measured `CaptureProvenance`.

## [02]-[FINISH]

- Owner: `Finish` static resolve fold; `FinishKind` `[SmartEnum<string>]` the finish-system discriminant whose fourteen rows ARE the finish space; `FinishHandling` the config-as-value behavior row (hiding, gloss, transmission, transmission roughness, coat bias, IOR, metalness, anisotropy and its grain rotation, sheen, specular tint, subsurface weight, the scatter radius, the substrate `BlendMode`, the `DeltaE` drift policy, the hue-constancy tolerance, the `ThinFilm` seed); `FinishMix` the `Pigment[]`/`double[]` Kubelka-Munk weight vector; `FinishLayer` `[Union]` the primer/base/glaze/topcoat ordered coat stack; `FinishPigment` `[SmartEnum<string>]` the closed Golden pigment vocabulary, one row per shipped `ArtistPaint` handle carrying its `Pigment` reflectance value and exposing the `MeasurementSpace` the mix runs under; `PigmentCapture` the two-background measured-pigment admission owning `Extract`, the per-wavelength `K`/`S` separation that mints a custom `Pigment` the same mix consumes.
- Cases: kind {`Paint` (opaque dispersion wall paint, mid-rough), `Enamel` (hard alkyd/oil gloss trim film), `Lacquer` (thin fast-dry film reading as a built coat, `ClearcoatBias` 0.5), `Coating` (a thin tinted topcoat over a substrate, bias 0.6), `Varnish` (translucent amber wood film, `Multiply` over the substrate, coat-forward), `Stain` (penetrating translucent, `Multiply` over the substrate, `Transmission` 0.5 at `TransmissionRoughness` 0.35 — the in-film scatter), `Plaster` (opaque high-scattering near-Lambertian at `Subsurface` 0.35 over a millimetre-scale mineral scatter radius; a sealed/polished plaster takes its coat from the stack, never a hardcoded zero), `Limewash` (breathable semi-hiding mineral coat, ultra-matte, `Subsurface` 0.25 at the thinner sub-millimetre radius its single application leaves), `Powdercoat` (electrostatic polyester on architectural metal, satin, IOR 1.55), `Suede` (flocked suede-effect coat, `Sheen` 0.80 over `Subsurface` 0.20 with the forward-reddening fibre radius), `Metallic` (metal-flake basecoat, `Metalness` 0.85, always clearcoated), `Brushed` (directional brushed-metal coat — brushed stainless/aluminum trim — `Metalness` 1.0, `Anisotropy` 0.65 along a grain the row's own `AnisotropyRotation` states rather than one the lowering invents), `Pearlescent` (TiO2-coated mica interference basecoat, `Film` 380 nm at IOR 2.0 under a clearcoat), `Anodized` (electrolytic Al2O3 oxide on aluminum, `Metalness` 1.0, `Film` 220 nm at IOR 1.65)} — the closed finish-system family; a finish is a `FinishKind` row carrying its `FinishHandling` columns, never a finish subtype, an `IsOpaque` bool ladder, or a per-kind `Seed` arm. layer {`Primer` (hiding undercoat, `Normal` composite), `Base` (pigment-bearing color coat, `Normal` composite), `Glaze` (translucent decorative effect coat compositing by its OWN named `BlendMode`), `Topcoat` (protective clear/tinted coat — its tint `Multiply`-filters the composite, its `ThinFilm` rides to the row `Film` column, its weight/roughness seed the `Clearcoat` columns)} — the ordered coat stack substrate-to-outermost (each layer composites OVER the color below it, the topcoat last), a `FinishLayer` `[Union]` case, never a per-coat type.
- Entry: `public static Fin<(MaterialParameters Row, CaptureProvenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key, Option<MaterialParameters> substrate = default)` — the resolve fold admitting the coat stack ONCE (`AdmitStack` faults any non-finite or out-of-unit layer weight/roughness, so `Compose` never re-clamps), mixing the `FinishMix` through the admitted Kubelka-Munk constructor under the `ArtistPaint` sRGB/D50 `Configuration`, rebasing to ACEScg and grounding through the `surface#SPECTRAL_UPSAMPLE` `SceneLinear` owner, folding the coat stack over the mix through `Unicolour.Blend`, gating the composite through the imported `graph#MATERIAL_LIBRARY` ladder — the two row tolerance columns admitting ONCE through the kernel `Tolerance.Of(ToleranceLane.Spectral, …)` carrier before either gate reads one, then `SpectralAdmit` then `PointerAdmit` then `NearestChecker(composite, drift, row.Drift, key)` under the KIND'S OWN `DeltaE` metric (`Ciede2000` for pigment paints, `Cam16` for effect finishes, `Hyab` for the large-difference stain/varnish composites) then `HueConstant(composite, mix, hue, key)` witnessing the composite against the mix's Ebner-Fairchild constant-hue locus so a tint that walks off-hue rails rather than admitting a shifted color — seeding the row through the ONE `FinishKind.Seed` derivation over the handling columns (the substrate ground `substrate.Map(s => s.BaseColor).IfNone(PrimedGround)` so a stain over `wood.oak` composites over the REAL row it coats), landing the merged interference film (topcoat film wins over the kind seed) on the row's `Film` column, and re-admitting through `MaterialParameters.Of`; `Fin<T>` aborts on an empty or weight-mismatched mix (`MaterialFault.Parameter`), an out-of-unit coat weight or roughness (`MaterialFault.Parameter`, the stack admission), a Pointer-unreproducible reflectance, a ColorChecker drift beyond the row tolerance, or a hue-shifted tint (`MaterialFault.Gamut`, the case reused); arity is one — a multi-layer finish folds the `FinishLayer` `Seq` substrate-to-outermost (the left fold composites each successive layer OVER the accumulated color, so the topcoat lands last), never a per-layer method.
- Packages: Wacton.Unicolour (composed — `new Unicolour(Configuration, Pigment[], double[])` Kubelka-Munk weighted pigment mix under the pigments' own measurement space, `new Pigment(...)` single/two-constant construction, `Configuration`, `ConvertToConfiguration` for the scene-space rebase, `Blend(backdrop, BlendMode)` the W3C separable/non-separable compositing algebra with coverage riding the source alpha, `Mix` the scene-linear lerp, the `DeltaE` metric selector the drift policy names, the `.RgbLinear` accessor), Wacton.Unicolour.Datasets (composed — the `ArtistPaint` Golden 19-pigment `Pigment` reflectance set, `ArtistPaint.All`, the per-pigment `Pigment.Name`, and the `ArtistPaint.Configuration` sRGB/D50 working space; the `EbnerFairchild` constant-hue loci and `Macbeth` patches consumed through the imported `graph#MATERIAL_LIBRARY` gates), `surface#SPECTRAL_UPSAMPLE` (composed — `SpectralUpsample.SceneLinear` the ONE grounding owner over the `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg working space), `graph#MATERIAL_LIBRARY` (composed — `MaterialParameters` + `ThinFilm` + the four-gate admission ladder), Rasm (project — `Op`, and the kernel `Domain/context#TOLERANCE_LANES` `Tolerance`/`ToleranceLane.Spectral` pair the two drift columns admit through, so no bare tolerance double reaches a gate), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new finish system is one `FinishKind` row — fourteen rows already span opaque paints, solvent films, over-substrate translucents, mineral coats, industrial polymer, sheened decorative, and effect/metal finishes (metallic, brushed, pearlescent, anodized); the row IS the behavior, `Seed` never grows an arm. A new coat role is one `FinishLayer` `[Union]` case naming its composite; a new blend behavior is a `BlendMode` member already owned by Unicolour, selected as a `Glaze` row value; a new drift policy is a `DeltaE` member on the kind row. A new pigment is one `FinishPigment` row binding an `ArtistPaint` handle or its own `new Pigment` measured-reflectance construction — the Kubelka-Munk mix is the closed admitted constructor, a pigment is a reflectance value not a mixing class, and a compile-time row is what makes an unregistered pigment unspellable rather than a runtime refusal; a measured pigment reflectance curve admitted from a spectrophotometer lands as one `new Pigment(start, interval, k, s, k1, k2, name)` two-constant construction — or, for an opaque single-scatterer like a `Plaster` lime/gypsum coat, one `new Pigment(start, interval, r, name)` single-constant construction — the SAME mix consumes. The finish output aligns to the OpenPBR `base`/`specular`/`coat`/`fuzz`/`thin_film` groups through the row columns the `surface#OPENPBR_SLAB` lowering reads and the `interchange#MATERIAL_WIRE` projects.
- Law: the Kubelka-Munk inverse carries NO per-loop exemption. `PigmentCapture.Extract` walks its wavelength grid as a `Traverse` over `Enumerable.Range`, so the per-band solve is an expression rail that accumulates its own refusal with the wavelength named — the page declares ZERO `for`, `foreach`, and `if` statements, and its only branching is the pattern switch and the `guard` ladder the rail already owns.
- Boundary: `Finish.Resolve` is the ONE finish path — a `PaintFinish`/`StainFinish` type is the deleted form; the `FinishMix` weight vector NORMALIZES at `FinishMix.Of` (Kubelka-Munk mixes concentrations, so an unnormalized vector is a second spelling of one physical paint the admitted value refuses to carry) and resolves to a reflectance EXCLUSIVELY through the admitted `new Unicolour(Configuration, Pigment[], double[])` Kubelka-Munk constructor — whose signature carries NO illuminant slot, the working space being the `Configuration`'s own `XyzConfiguration` — under the `ArtistPaint.Configuration` sRGB/D50 working space the pigments were measured in, then `ConvertToConfiguration(PortValue.SceneLinear)` rebases the mix to the ONE ACEScg scene-linear `Configuration` instance and `surface#SPECTRAL_UPSAMPLE` `SceneLinear` grounds it — the SAME grounding owner `acquisition#ACQUISITION` `GroundSpectral` composes, never a parallel inline scene-linear construction nor a default-D65 mix mislabelled as scene-linear — so Materials NEVER re-derives the FORWARD two-constant `K/S` mix and a finish color IS the Kubelka-Munk mix of measured pigments grounded into the one pipeline, not an authored triple — the INVERSE is the one leg this page owns, because Unicolour carries no member that separates `K` from `S` and `PigmentCapture.Extract` is what turns a two-background capture into the `new Pigment` construction its constructor consumes; that inverse is a 2x2 linear solve in `u = a + b·coth(bSX)`/`v = a − b·coth(bSX)` whose determinant IS the backing-separation admission read at the solver's grain, so the published ideal-black closed form is its `Rg = 0` specialization rather than a second algebra and a real black tile never has to pretend to be an ideal one; layer compositing is `Finish.Composite` — the layer's coverage rides the SOURCE ALPHA into `Unicolour.Blend(backdrop, mode)` so ONE library call runs the named W3C blend AND the alpha-composited coverage (under the linear ACEScg working space the blend's encoded-Rgb domain IS scene-linear, and reflectance channels stay in `[0,1]` where the W3C algebra is total) — the prior `substrate.Mix(pigment, …)` linear lerp that LIGHTENED a translucent stain toward its pigment is the deleted form, a stain/varnish/glaze now `Multiply`-darkens its substrate as the physics demands; the coat stack is admitted ONCE at `AdmitStack` (`BOUNDARY_ADMISSION`: every weight/roughness proven finite in `[0,1]` before the fold, the interior clamp-free), a violation railing `MaterialFault.Parameter` with the layer role — the prior fence that PROMISED an out-of-unit-coat fault while silently clamping is the deleted illusion; the four-gate admission ladder is imported from `graph#MATERIAL_LIBRARY` by domain — MacAdam spectral limit, Pointer real-surface, ColorChecker drift under the ROW'S `DeltaE` policy, Ebner-Fairchild hue constancy — never a second gamut owner, and the drift/hue tolerances are `FinishHandling` POLICY VALUES, never a page-level const; the two APPEARANCE metrics (`Cam16` on the four effect rows) measure under the package-bound `CamConfiguration.StandardRgb` condition and that condition is STATED rather than defaulted-into, because a CAM distance is a function of the observer's adaptation and a tolerance calibrated against an unnamed surround is a number nobody can re-derive — declaring a different surround is a kernel `RgbProfile.Condition`/`Viewed` mint, never a Materials-side `Configuration`, so the row carries the metric and the folder carries the condition once; `FinishKind.Seed` is ONE expression over the handling row — hiding is `BaseWeight` (a translucent kind composites over the substrate ground by its `Substrate` blend row), gloss is the row roughness, the coat is the stack topcoat floored by `ClearcoatBias`, metalness/anisotropy/sheen/specular-tint/transmission-roughness/IOR are row columns — so the fourteen kinds share one derivation and a fifteenth kind is one row with ZERO dispatch edits (the prior four-arm `Switch`, and its hardcoded `Metalness: 0.0`/`Sheen: 0.0` that made metallic, pearlescent, and sheened finishes UNREACHABLE, are the deleted forms — and the same defect wearing a `Subsurface: 0.0`/`SubsurfaceRadius.None` literal is deleted with them, so the three genuinely scattering mineral and flocked rows reach the `surface#OPENPBR_SLAB` `Subsurface` lobe the lowering already routes; only emission stays a Seed literal, and that carve is DECLARED at the derivation because a finish is a reflectance system rather than a light); the substrate enters as `Option<MaterialParameters>` — a stain composites over the row it coats, the `PrimedGround` near-white the named canonical when none is supplied (the prior hardcoded constant that made "rides the substrate" a fiction is the deleted form); the interference film lands on the row's `Film` `ThinFilm` column (topcoat film over kind seed), the `surface#OPENPBR_SLAB` lowering reading it into the `thin_film` group and the `Slab.Coat` interference lobe — the prior path that wrote the film onto a `Slab.Coat` inside a DISCARDED validation binding, so no pearlescent finish could ever reach shading, is the deleted illusion, and the full coat-stack-to-`LayeredBsdf` lowering now happens exactly once downstream of the row, never re-derived here; the coat columns the `weathering#WEATHERING` chalking trajectory raises (`coat_roughness`) are the shared aging target, so a finish ages through the weathering operator over its own row and a per-finish aging variant is the deleted form; the produced row carries the `acquisition#ACQUISITION` `CaptureProvenance` receipt (the pigment count and the resolved-mix evidence a hand-keyed triple lacks) and re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so a finish row passes the same gamut/unit/IOR gate a registered row passes, and an empty mix, a pigment/weight length mismatch, or a non-finite reflectance rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Surface;
using Thinktecture;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;

// --- [TYPES] -------------------------------------------------------------------------------
// Fourteen architectural finish SYSTEMS as behavior rows. Seed is ONE derivation over the row; a new system is one
// row, never a Switch arm.
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
    public static readonly FinishKind Plaster     = new("plaster",     new(1.00, 0.85, 0.0, 0.0, 1.50, Subsurface: 0.35, HueTolerance: 12.0) { Scatter = SubsurfaceRadius.Create(1.20, 1.10, 1.00) });
    public static readonly FinishKind Limewash    = new("limewash",    new(0.85, 0.95, 0.0, 0.0, 1.49, Subsurface: 0.25, DriftTolerance: 14.0, HueTolerance: 12.0) { Scatter = SubsurfaceRadius.Create(0.80, 0.75, 0.70) });
    public static readonly FinishKind Powdercoat  = new("powdercoat",  new(1.00, 0.35, 0.0, 0.0, 1.55, DriftTolerance: 10.0));
    public static readonly FinishKind Suede       = new("suede",       new(1.00, 0.90, 0.0, 0.0, 1.50, Sheen: 0.80, SheenTint: 0.40, Subsurface: 0.20) { Scatter = SubsurfaceRadius.Create(0.60, 0.45, 0.35) });
    public static readonly FinishKind Metallic    = new("metallic",    new(1.00, 0.30, 0.0, 1.0, 1.50, Metalness: 0.85, Drift: DeltaE.Cam16, DriftTolerance: 10.0));
    public static readonly FinishKind Brushed     = new("brushed",     new(1.00, 0.35, 0.0, 0.0, 1.50, Metalness: 1.00, Anisotropy: 0.65, AnisotropyRotation: 0.5, Drift: DeltaE.Cam16, DriftTolerance: 10.0));
    public static readonly FinishKind Pearlescent = new("pearlescent", new(1.00, 0.25, 0.0, 1.0, 1.50, Metalness: 0.20, Drift: DeltaE.Cam16, DriftTolerance: 10.0, HueTolerance: 8.0) { Film = ThinFilm.Create(1.0, 380.0, 2.0) });
    public static readonly FinishKind Anodized    = new("anodized",    new(1.00, 0.30, 0.0, 0.0, 1.50, Metalness: 1.00, Drift: DeltaE.Cam16, DriftTolerance: 10.0) { Film = ThinFilm.Create(1.0, 220.0, 1.65) });

    public FinishHandling Handling { get; }

    private FinishKind(string key, FinishHandling handling) : this(key) => Handling = handling;

    // ONE derivation over the handling row — no per-kind Switch: hiding is BaseWeight, the coat is the stack topcoat
    // floored by ClearcoatBias, and the merged film lands on the row's Film column. Emission is the ONE column held
    // at a literal, a DECLARED BOUND rather than a walled-off axis: a self-luminous coating enters as its own kind.
    // Every other column is a row value, which is what makes the three mineral/flocked rows reachable at all.
    public MaterialParameters Seed(Unicolour composite, Unicolour substrate, double coatWeight, double coatRoughness, ThinFilm film) =>
        new(BaseColor: Handling.BaseWeight >= 1.0 ? composite : Finish.Composite(substrate, composite, Handling.Substrate, Handling.BaseWeight),
            Metalness: Handling.Metalness, Roughness: Handling.Roughness, SpecularTint: Handling.SpecularTint, Anisotropy: Handling.Anisotropy, Ior: Handling.Ior,
            Transmission: Handling.Transmission, TransmissionRoughness: Handling.TransmissionRoughness, Sheen: Handling.Sheen, SheenTint: Handling.SheenTint,
            Clearcoat: Math.Max(coatWeight, Handling.ClearcoatBias), ClearcoatRoughness: coatRoughness,
            Subsurface: Handling.Subsurface, SubsurfaceRadius: Handling.Scatter, Emission: PortValue.Black, EmissionLuminance: 0.0) {
            Film = film,
            // The grain AZIMUTH rides the row beside its magnitude: an anisotropy weight with no direction is a
            // brushed finish whose grain the lowering invents, and it always invented zero. The unit convention is
            // the row's own (1 is a half turn), so the value crosses the wire unconverted.
            AnisotropyRotation = Handling.AnisotropyRotation,
        };
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

    // Substrate-out composite — each layer composites OVER the colour below it, so the left fold applies the topcoat
    // LAST; the topcoat's weight/roughness/film seed the row coat columns instead of compositing.
    public Unicolour Compose(Unicolour below) => Switch(
        state: below,
        primer:  static (b, p) => Finish.Composite(b, p.Reflectance, BlendMode.Normal, p.Weight),
        @base:   static (b, l) => Finish.Composite(b, l.Reflectance, BlendMode.Normal, l.Weight),
        glaze:   static (b, g) => Finish.Composite(b, g.Tint, g.Blend, g.Weight),
        topcoat: static (b, t) => t.Tint.Map(tint => Finish.Composite(b, tint, BlendMode.Multiply, t.Weight)).IfNone(b));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The config-as-value behavior row — hiding/gloss/transmission/coat-bias/IOR positional, the effect columns, the
// substrate BlendMode, the per-kind DeltaE drift policy, the hue tolerance, the init-defaulted ThinFilm seed — all
// POLICY VALUES the one Seed derivation reads, never a literal walling a column off. The four Cam16 rows measure
// under the package-bound CamConfiguration.StandardRgb the kernel RgbProfile.Condition row publishes; not a column,
// because that kernel row owns the ONE cam-bearing Configuration mint.
// THE TWO TOLERANCES ARE POLICY SEEDED FROM PUBLISHED PRACTICE, NEVER MEASUREMENTS: ASTM D2244 standardizes the
// formulae and leaves the tolerance to buyer-seller agreement outright, so a quoted "ASTM colour tolerance" names
// something that does not exist. What IS published bounds them — the CIE76 JND near 2.3 (Mahy 1994), ISO 12647-2
// permitting 5 on a process solid while capping the HUE contribution at 2.5, CGATS TR016 levels 1.4 to 6.0. They
// are NOT one number twice: DriftTolerance is a PLAUSIBILITY RADIUS to the nearest of twenty-four sparse Macbeth
// patches, so pulling it to 2.5 rejects every legitimate finish BETWEEN patches, while HueTolerance is a deviation
// from the mix's own constant-hue locus — the column the published hue cap bounds, held above it because the loci
// sample hue coarsely and a locus is a curve rather than an aim point. Both cross Tolerance.Of at Finish.Admit.
// [SPIKE]: refinement converges on a drift census over real samples measured under this surround alone.
public readonly record struct FinishHandling(
    double BaseWeight, double Roughness, double Transmission, double ClearcoatBias, double Ior,
    double Metalness = 0.0, double Anisotropy = 0.0, double AnisotropyRotation = 0.0, double Sheen = 0.0, double SheenTint = 0.0, double SpecularTint = 0.0, double TransmissionRoughness = 0.0,
    double Subsurface = 0.0,
    BlendMode Substrate = BlendMode.Normal, DeltaE Drift = DeltaE.Ciede2000, double DriftTolerance = 12.0, double HueTolerance = 10.0) {
    public ThinFilm Film { get; init; } = ThinFilm.None;

    // Scatter rides an init property because a value-object instance is no compile-time constant, and it is named
    // Scatter rather than after its own type so `SubsurfaceRadius.None` inside this struct still resolves to the
    // TYPE. A weight without a radius scatters at zero magnitude, so the pair is authored together on every row.
    public SubsurfaceRadius Scatter { get; init; } = SubsurfaceRadius.None;
}

public readonly record struct FinishMix(Seq<FinishPigment> Pigments, Seq<double> Weights) {
    // The weight vector NORMALIZES at admission — a structural invariant. Kubelka-Munk mixing is defined over
    // CONCENTRATIONS summing to one, so (1, 1) and (0.5, 0.5) are one physical paint, and normalizing HERE makes
    // the admitted value canonical so a mix compares, hashes, and prints as itself.
    public static Fin<FinishMix> Of(Seq<FinishPigment> pigments, Seq<double> weights, Op key) =>
        pigments.IsEmpty
            ? new MaterialFault.Parameter(key, "<finish-mix-empty>")
            : pigments.Count != weights.Count
                ? new MaterialFault.Parameter(key, $"<finish-mix-arity:{pigments.Count}!={weights.Count}>")
                : weights.Exists(static w => !double.IsFinite(w) || w < 0.0)
                    ? new MaterialFault.Parameter(key, "<finish-mix-weight-negative>")
                    : weights.Fold(0.0, static (total, w) => total + w) switch {
                        <= 0.0 => new MaterialFault.Parameter(key, "<finish-mix-weight-zero>"),
                        var total => Fin.Succ(new FinishMix(pigments, weights.Map(w => w / total))),
                    };

    // The Kubelka-Munk mix runs under the pigments' OWN measurement working space (ArtistPaint sRGB/D50), rebases to
    // ACEScg, and grounds through the ONE surface#SPECTRAL_UPSAMPLE SceneLinear owner acquisition#ACQUISITION also
    // composes — so the reflectance enters gamut-pulled and genuinely Acescg, never a default-D65 mix mislabelled.
    public Fin<Unicolour> Reflectance(Op key) =>
        key.Catch(() => SpectralUpsample.SceneLinear(
                new Unicolour(FinishPigment.MeasurementSpace, Pigments.Map(static p => p.Reflectance).ToArray(), Weights.ToArray())
                    .ConvertToConfiguration(PortValue.SceneLinear), key)
            .Map(static rgb => new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R, rgb.G, rgb.B)));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// FinishPigment is the Golden set as a CLOSED VOCABULARY, one row per shipped handle, so a pigment is a compile-time
// name a mix composes rather than a string a fold resolves at runtime. The prior reflection-derived dictionary paid
// a per-call lookup and a Fin rail for a set the package fixes at nineteen, and the whole failure mode it railed
// was a caller typing a name — which a row makes unspellable. Key IS the wire spelling an interchange row prints.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FinishPigment {
    public static readonly FinishPigment BoneBlack               = new("bone-black", ArtistPaint.BoneBlack);
    public static readonly FinishPigment HansaYellowOpaque       = new("hansa-yellow-opaque", ArtistPaint.HansaYellowOpaque);
    public static readonly FinishPigment DiarylideYellow         = new("diarylide-yellow", ArtistPaint.DiarylideYellow);
    public static readonly FinishPigment BismuthVanadateYellow   = new("bismuth-vanadate-yellow", ArtistPaint.BismuthVanadateYellow);
    public static readonly FinishPigment CadmiumOrange           = new("cadmium-orange", ArtistPaint.CadmiumOrange);
    public static readonly FinishPigment PyrroleOrange           = new("pyrrole-orange", ArtistPaint.PyrroleOrange);
    public static readonly FinishPigment CadmiumRedLight         = new("cadmium-red-light", ArtistPaint.CadmiumRedLight);
    public static readonly FinishPigment PyrroleRed              = new("pyrrole-red", ArtistPaint.PyrroleRed);
    public static readonly FinishPigment QuinacridoneRed         = new("quinacridone-red", ArtistPaint.QuinacridoneRed);
    public static readonly FinishPigment QuinacridoneMagenta     = new("quinacridone-magenta", ArtistPaint.QuinacridoneMagenta);
    public static readonly FinishPigment DioxazinePurple         = new("dioxazine-purple", ArtistPaint.DioxazinePurple);
    public static readonly FinishPigment UltramarineBlue         = new("ultramarine-blue", ArtistPaint.UltramarineBlue);
    public static readonly FinishPigment CobaltBlue              = new("cobalt-blue", ArtistPaint.CobaltBlue);
    public static readonly FinishPigment CeruleanBlueChromium    = new("cerulean-blue-chromium", ArtistPaint.CeruleanBlueChromium);
    public static readonly FinishPigment PhthaloBlueRedShade     = new("phthalo-blue-red-shade", ArtistPaint.PhthaloBlueRedShade);
    public static readonly FinishPigment PhthaloBlueGreenShade   = new("phthalo-blue-green-shade", ArtistPaint.PhthaloBlueGreenShade);
    public static readonly FinishPigment PhthaloGreenBlueShade   = new("phthalo-green-blue-shade", ArtistPaint.PhthaloGreenBlueShade);
    public static readonly FinishPigment PhthaloGreenYellowShade = new("phthalo-green-yellow-shade", ArtistPaint.PhthaloGreenYellowShade);
    public static readonly FinishPigment TitaniumWhite           = new("titanium-white", ArtistPaint.TitaniumWhite);

    // Reflectance carries the package's own measured Kubelka-Munk value; the row is the NAME, the package the data.
    // A custom pigment enters as one more row over its own `new Pigment(...)`, which is why the column is the VALUE.
    public Pigment Reflectance { get; }

    // The sRGB/D50 working space the Golden pigments were measured under — FinishMix.Reflectance runs under THIS
    // space so the K/S reflectance reads its measurement white point, never Configuration.Default.
    public static readonly Configuration MeasurementSpace = ArtistPaint.Configuration;
}

// --- [BOUNDARIES] --------------------------------------------------------------------------
// The MEASURED-PIGMENT admission. A spectrophotometer reports REFLECTANCE, never the two-constant K and S, so this
// boundary turns a capture into the `new Pigment(...)` construction the mix consumes and every step is a contract.
//   GEOMETRY is the authority: a reflectance quoted without its geometry is not a measurement, so a pair captured
//   under two geometries refuses rather than averaging.
//   SATURATION is why TWO backgrounds are mandatory. One backing yields the K/S RATIO alone — at complete hiding
//   (1−R∞)²/(2·R∞), with K and S individually unrecoverable — so the DIFFERENCE between a white and a black reading
//   is the whole of the separating information and a one-background capture refuses at admission.
//   The SAUNDERSON CORRECTION IS MANDATORY: a measured reflectance includes surface reflections the layer equations
//   do not model, so raw R attributes an interface effect to the pigment's own scattering; k1 and k2 ride the
//   Pigment construction so the pair that corrected the capture reproduces it at render.
//   The BACKINGS ARE SPECTRA: the equations read the backing AT EACH WAVELENGTH, so a scalar seats its own
//   spectral error inside every extracted coefficient.
public readonly record struct PigmentCapture(
    int StartNm, int IntervalNm, Seq<double> OverWhite, Seq<double> OverBlack,
    Seq<double> BackingWhite, Seq<double> BackingBlack, double ThicknessMicrons, double K1, double K2) {

    // Admit proves the pair BEFORE any solve: the four series share a grid, every reflectance sits inside the OPEN
    // unit interval (0 and 1 are both singular for the layer equations), the two backings differ AT EVERY WAVELENGTH
    // (an identical pair is the one-background refusal renamed), and K1/K2 BIND TO THE INSTRUMENT GEOMETRY.
    public Fin<PigmentCapture> Of(Op key) =>
        from _ in guard(Bands.ForAll(band => band.Count == OverWhite.Count) && !OverWhite.IsEmpty,
                new MaterialFault.Parameter(key, $"<pigment-capture-grid:{OverWhite.Count},{OverBlack.Count},{BackingWhite.Count},{BackingBlack.Count}>"))
        from __ in guard(Bands.Bind(static band => band).ForAll(Interior),
                new MaterialFault.Parameter(key, "<pigment-capture-reflectance-singular>"))
        from ___ in guard(BackingWhite.Zip(BackingBlack).ForAll(static pair => Math.Abs(pair.Left - pair.Right) > BackingSeparation),
                new MaterialFault.Parameter(key, "<pigment-capture-backings-degenerate>"))
        from ____ in guard(double.IsFinite(ThicknessMicrons) && ThicknessMicrons > 0.0,
                new MaterialFault.Parameter(key, $"<pigment-capture-thickness:{ThicknessMicrons:R}>"))
        from _____ in guard(Interior(K1) || K1 == 0.0, new MaterialFault.Parameter(key, $"<pigment-saunderson-k1:{K1:R}>"))
        from ______ in guard(Interior(K2) || K2 == 0.0, new MaterialFault.Parameter(key, $"<pigment-saunderson-k2:{K2:R}>"))
        select this;

    Seq<Seq<double>> Bands => Seq(OverWhite, OverBlack, BackingWhite, BackingBlack);

    // The two backings must differ by more than instrument noise; below this they carry one measurement twice.
    const double BackingSeparation = 0.05;
    static bool Interior(double r) => double.IsFinite(r) && r is > 0.0 and < 1.0;

    // The INVERSE Saunderson every reflectance crosses before the layer equations see it — the internal reflectance
    // the pigment produced, interface terms removed. The ONE place the correction runs, so a capture cannot reach
    // the solve uncorrected and a render cannot re-apply it.
    public static double Internal(double measured, double k1, double k2) =>
        (measured - k1) / (((1.0 - k1) * (1.0 - k2)) + (k2 * (measured - k1)));

    // The complete-hiding K/S ratio: at full opacity the layer equations collapse to (1−R∞)²/(2·R∞) — the ratio a
    // single background reaches and the anchor the two-background separation measures against.
    public static double RatioAtHiding(double internalInfinite) =>
        (1.0 - internalInfinite) * (1.0 - internalInfinite) / (2.0 * internalInfinite);

    // Extract separates K from S at every wavelength and lands the two-constant Pigment the mix consumes — the
    // MEASURED half of the growth path, where ArtistPaint supplies the shipped half.
    // THE SOLVE IS LINEAR, and that is the whole of its density. The layer equation
    //   R = [1 − Rg(a − b·coth(bSX))] / [(a + b·coth(bSX)) − Rg]
    // is hyperbolic in the film yet linear in the PAIR u = a + b·coth(bSX), v = a − b·coth(bSX): substituting them
    // rearranges to R·u + Rg·v = 1 + R·Rg, so two backings are a 2x2 system rather than the four-equation nonlinear
    // solve the ratio form suggests, and NEITHER backing need be an ideal black. Its determinant D = Rw·Gb − Rb·Gw
    // IS the separating information the backing guard states physically, so the admission gate and the solve's
    // singularity are ONE condition at two grains. From the pair: a = (u+v)/2, b = √(a²−1), K/S = a−1, S·X =
    // arccoth(c)/b at c = (u−v)/(2b) — spelled atanh(1/c) because coth⁻¹ is singular where the film vanishes.
    public Fin<Pigment> Extract(string name, Op key) =>
        from admitted in Of(key)
        from bands in toSeq(Enumerable.Range(0, OverWhite.Count))
            .Traverse(i => Separate(
                Internal(OverWhite[i], K1, K2), Internal(OverBlack[i], K1, K2),
                Internal(BackingWhite[i], K1, K2), Internal(BackingBlack[i], K1, K2),
                ThicknessMicrons / 1000.0, StartNm + (i * IntervalNm), key)).As()
        select new Pigment(StartNm, IntervalNm,
            bands.Map(static band => band.K).ToArray(), bands.Map(static band => band.S).ToArray(), K1, K2, name);

    // One wavelength: the 2x2 solve and its three physical-consistency rails, each naming the wavelength it failed
    // at. a ≤ 1 is a film the layer model cannot describe and c ≤ 1 a coth argument no positive optical thickness
    // produces, so either refuses rather than seating a fabricated coefficient. Coefficients land PER MILLIMETRE.
    static Fin<(double K, double S)> Separate(double rw, double rb, double gw, double gb, double thicknessMm, int nm, Op key) =>
        from d in Separating((rw * gb) - (rb * gw), nm, key)
        let u = (((1.0 + (rw * gw)) * gb) - ((1.0 + (rb * gb)) * gw)) / d
        let v = ((rw * (1.0 + (rb * gb))) - (rb * (1.0 + (rw * gw)))) / d
        let a = (u + v) / 2.0
        from _ in guard(a > 1.0, new MaterialFault.Parameter(key, $"<pigment-solve-absorption:{nm}:{a:R}>"))
        let b = Math.Sqrt((a * a) - 1.0)
        let coth = (u - v) / (2.0 * b)
        from __ in guard(coth > 1.0, new MaterialFault.Parameter(key, $"<pigment-solve-thickness:{nm}:{coth:R}>"))
        let s = Math.Atanh(1.0 / coth) / (b * thicknessMm)
        select ((a - 1.0) * s, s);

    static Fin<double> Separating(double d, int nm, Op key) =>
        Math.Abs(d) > Determinant
            ? Fin<double>.Succ(d)
            : Fin<double>.Fail(new MaterialFault.Parameter(key, $"<pigment-solve-degenerate:{nm}:{d:R}>"));

    // The determinant floor is the solve's own grain of the backing-separation guard above.
    const double Determinant = 1e-6;
}

public static class Finish {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    // The named canonical ground the Option substrate defaults to: a primed near-white, never an implicit hardcode.
    internal static readonly Unicolour PrimedGround = Linear(0.92, 0.92, 0.90);

    public static Fin<(MaterialParameters Row, CaptureProvenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key, Option<MaterialParameters> substrate = default) =>
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
                Some: bad => Fin.Fail<Seq<FinishLayer>>(new MaterialFault.Parameter(key, $"<finish-layer-out-of-unit:{bad.Role}>")),
                None: () => Fin.Succ(stack));

    // The four-gate ladder by domain, tolerances the KIND'S policy row: MacAdam spectral limit → Pointer real-surface
    // → ColorChecker drift under the row's DeltaE metric → Ebner-Fairchild hue constancy of the composite vs the mix.
    // BOUNDARY_ADMISSION for the two tolerance numbers: the kernel Tolerance carrier proves each before a gate reads
    // it, both on ToleranceLane.Spectral — the dimensionless colour lane whose Band.Length window is a delta-E
    // magnitude's shape — so a NaN or negative tolerance refuses HERE rather than passing every candidate.
    static Fin<Unicolour> Admit(Unicolour composed, Unicolour mix, FinishHandling handling, Op key) =>
        from drift in Tolerance.Of(ToleranceLane.Spectral, handling.DriftTolerance, key)
        from hue in Tolerance.Of(ToleranceLane.Spectral, handling.HueTolerance, key)
        from spectral in MaterialLibrary.SpectralAdmit(composed, key)
        from surface in MaterialLibrary.PointerAdmit(spectral, key)
        from _ in MaterialLibrary.NearestChecker(surface, drift, handling.Drift, key)
        from anchored in MaterialLibrary.HueConstant(surface, mix, hue, key)
        select anchored;

    // The stack is substrate-to-outermost, so the LAST topcoat is the outer coat whose weight/roughness/film seed
    // the row coat columns — a first-wins Head would silently shade an inner coat under a re-coated stack.
    static (double Weight, double Roughness, ThinFilm Film) TopcoatOf(Seq<FinishLayer> stack) =>
        stack.Fold(Option<(double Weight, double Roughness, ThinFilm Film)>.None,
                static (outer, l) => l is FinishLayer.Topcoat t ? Some((t.Weight, t.Roughness, t.Film)) : outer)
            .IfNone((0.0, 0.0, ThinFilm.None));

    // W3C blend+composite through the library in ONE call: coverage rides the SOURCE ALPHA into Unicolour.Blend,
    // which runs B(backdrop, layer) and the alpha-composited lerp together. Under the linear Acescg space the
    // blend's encoded-Rgb domain IS scene-linear, and coverage is admitted upstream so no interior clamp exists.
    internal static Unicolour Composite(Unicolour below, Unicolour layer, BlendMode mode, double coverage) {
        ColourTriplet lin = layer.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet;
        return new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, lin.First, lin.Second, lin.Third, coverage).Blend(below, mode);
    }

    internal static bool Unit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;

    // A Kubelka-Munk mix is a MEASURED-pigment finish, so it stamps CaptureMethod.PigmentMix — structurally !=
    // Authored, so interchange#MATERIAL_WIRE reads Measured true. The pigment count rides AngularSamples; no band
    // count and no fit residual exist for a mix, so both are 0 rather than the prior misuse of those columns.
    static CaptureProvenance MixProvenance(FinishMix mix) =>
        CaptureProvenance.Of(CaptureMethod.PigmentMix, $"kubelka-munk:{mix.Pigments.Count}-pigment", wavelengthCount: 0, angularSamples: mix.Pigments.Count, fitResidual: 0.0);
}
```

## [03]-[RESEARCH]

(none)
