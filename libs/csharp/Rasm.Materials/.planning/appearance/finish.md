# [MATERIALS_FINISH]

THE KUBELKA-MUNK PIGMENT/COAT-STACK FINISH ENGINE. One `Finish.Resolve` static fold over a `FinishMix` pigment-weight vector and a `FinishLayer` coat stack produces a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying a spectrally-grounded scene-linear `BaseColor` and measured `Provenance`, so an architectural finish — a paint, a coating, a stain, a plaster — is a PIGMENT MIX PLUS A COAT STACK rather than a hand-keyed base-color triple. A finish is NEVER a second appearance surface: `Finish.Resolve` mixes the `Pigment[]`/`double[]` weight vector through the one admitted `new Unicolour(Pigment[], double[])` Kubelka-Munk constructor, gates the resolved reflectance against the `graph#MATERIAL_LIBRARY` `PointerAdmit` real-surface gamut and the `NearestChecker` ColorChecker drift, lowers the primer/base/topcoat coat stack onto the realized `bsdf#OPENPBR_SLAB` `Slab.Coat` slab, and seeds ONE `MaterialParameters` row the SAME `graph#MATERIAL_GRAPH` evaluates and the SAME `bsdf#LOBE_FAMILY` shades — a `Paint`/`Stain`/`Plaster`/`Coating` type is the deleted form, the variation a `FinishKind` config-as-value row carrying its pigment-handling behavior, never a flag set. Kubelka-Munk reflectance mixing is OWNED by the Unicolour `Pigment` constructor and the `Wacton.Unicolour.Datasets` `ArtistPaint` Golden 19-pigment set; Materials NEVER re-derives the two-constant scattering math. The page composes `graph#MATERIAL_LIBRARY` for the produced `MaterialParameters` row plus its `PointerAdmit`/`NearestChecker` admission predicates (the W1B Pointer real-surface gate imported, never a second gamut owner), `bsdf#OPENPBR_SLAB` `Slab.Coat`/`SlabStack` for the coat-stack lowering target (the same slab `weathering#WEATHERING` chalking drives, no second color register), Wacton.Unicolour directly as the scene-linear/spectral color owner, and the `MaterialFault` band-2450 rail for a Pointer-unreproducible reflectance (reusing the `Gamut` case) or a degenerate mix.

## [1]-[INDEX]

- [1]-[FINISH]: the `FinishKind` `[SmartEnum<string>]` discriminant (paint · coating · stain · plaster, each carrying its pigment-handling behavior row), the `FinishMix` pigment-weight vector resolving through the admitted Kubelka-Munk constructor over the `ArtistPaint` set, the `FinishLayer` `[Union]` primer/base/topcoat coat stack lowering to the `bsdf#OPENPBR_SLAB` `Slab.Coat` slab, and the one `Finish.Resolve` fold producing a `graph#MATERIAL_LIBRARY` `MaterialParameters` row with measured `Provenance`.

## [2]-[FINISH]

- Owner: `Finish` static resolve fold; `FinishKind` `[SmartEnum<string>]` the finish-handling discriminant; `FinishMix` the `Pigment[]`/`double[]` Kubelka-Munk weight vector; `FinishLayer` `[Union]` the primer/base/topcoat ordered coat stack; `FinishPigment` the `ArtistPaint`-handle catalogue resolving a named pigment to its `Pigment` reflectance value.
- Cases: kind {`Paint` (opaque pigment-load over a hiding base, base color the mix reflectance), `Coating` (a thin tinted topcoat over a substrate, the coat slab carrying the tint), `Stain` (translucent over-substrate transmission, the mix modulating the substrate base color rather than hiding it), `Plaster` (opaque high-scattering single-constant pigment, near-Lambertian high-roughness diffuse)} — the closed finish family; a finish is a `FinishKind` case carrying its `FinishHandling` row, never a finish subtype or a `IsOpaque`/`IsTranslucent` bool ladder. layer {`Primer` (the hiding undercoat, occluding base color), `Base` (the pigment-bearing color coat), `Topcoat` (the protective clear/tinted coat lowering to `Slab.Coat`)} — the ordered coat stack outermost-to-substrate, a `FinishLayer` `[Union]` case, never a per-coat type.
- Entry: `public static Fin<(MaterialParameters Row, Provenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key)` — the resolve fold mixing the `FinishMix` pigment-weight vector to a scene-linear `Unicolour` reflectance through the admitted `new Unicolour(Pigment[], double[])` Kubelka-Munk constructor under the `ArtistPaint` sRGB/D50 `Configuration`, gating the reflectance through the imported `graph#MATERIAL_LIBRARY` `PointerAdmit` real-surface gamut and `NearestChecker` ColorChecker drift, applying the `FinishKind` handling row to seed the `MaterialParameters` columns, validating the coat stack lowers cleanly onto the `bsdf#OPENPBR_SLAB` `Slab.Coat` slab, re-admitting the produced row through `MaterialParameters.Of`, and pairing it with the measured `acquisition#ACQUISITION` `Provenance` receipt (the pigment count and mix-weight sum a hand-keyed triple lacks — the receipt rides beside the row exactly as `graph#MATERIAL_LIBRARY` `NearestChecker` returns its `(Patch, DeltaE)` pair, never a phantom column on the closed 16-column `MaterialParameters`); `Fin<T>` aborts on an empty or weight-mismatched mix (`MaterialFault.Parameter`), a Pointer-unreproducible reflectance or a ColorChecker drift beyond tolerance (`MaterialFault.Gamut`, the case reused), or an out-of-unit coat weight; arity is one — a multi-layer finish folds the `FinishLayer` `Seq` outermost-to-substrate, never a per-layer method.
- Packages: Wacton.Unicolour (composed — `new Unicolour(Pigment[], double[])` Kubelka-Munk weighted pigment mix, `new Pigment(...)` single/two-constant construction, `Configuration`, the `.RgbLinear`/`IsInRgbGamut` accessors), Wacton.Unicolour.Datasets (composed — the `ArtistPaint` Golden 19-pigment `Pigment` reflectance set, `ArtistPaint.All`, and the `ArtistPaint.Configuration` sRGB/D50 working space), Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new finish modality is one `FinishKind` row carrying its `FinishHandling` pigment-handling behavior — never a per-finish material or a second appearance owner; a new coat role is one `FinishLayer` `[Union]` case lowering to its slab; a new pigment is one `FinishPigment` row binding an `ArtistPaint` handle (or a `new Pigment` measured-reflectance construction) — the Kubelka-Munk mix is the closed admitted constructor, a pigment is a reflectance value not a mixing class. A measured pigment reflectance curve admitted from a spectrophotometer lands as one `new Pigment(start, interval, k, s, k1, k2, name)` two-constant construction the SAME mix consumes, never a re-derived scattering solver. The finish output aligns to the MaterialX `standard_surface`/`open_pbr_surface` coat inputs through the `bsdf#OPENPBR_SLAB` lowering the `interchange#MATERIAL_WIRE` projects.
- Boundary: `Finish.Resolve` is the ONE finish path — a `PaintFinish`/`StainFinish` type is the deleted form; the `FinishMix` weight vector resolves to a reflectance EXCLUSIVELY through the admitted `new Unicolour(mix.Pigments, mix.Weights)` Kubelka-Munk constructor (the `ArtistPaint.Configuration` sRGB/D50 working space the `ArtistPaint` pigments were measured under, read through Unicolour's `.RgbLinear` to the scene-linear pipeline), so Materials NEVER re-derives the two-constant `K/S` scattering or hand-rolls a pigment lerp — a finish color IS the Kubelka-Munk mix of measured pigments, not an authored triple; the resolved reflectance gates the `graph#MATERIAL_LIBRARY` `PointerAdmit` Pointer real-surface gamut (the W1B-step1 predicate imported, never a second gamut owner) so a finish a real pigment cannot physically reflect rails `MaterialFault.Gamut` with the Pointer-domain reason and `MapToPointer` supplies the nearest in-gamut Pointer `Unicolour` for the recoverable path, then the `NearestChecker` `Macbeth.All` ColorChecker `DeltaE.Ciede2000` drift gate witnesses the mix against the nearest reference patch so a mix that drifts beyond tolerance rails the SAME `Gamut` case rather than admitting a mis-mixed color; the `FinishKind` `FinishHandling` row is config-as-value — `Plaster` seeds a high-roughness near-Lambertian diffuse from a single-constant high-scattering pigment, `Stain` seeds a low base weight and rides the substrate `BaseColor` through transmission rather than hiding it, `Paint` an opaque hiding base, `Coating` a thin tinted topcoat — never a `switch` on a finish enum inside `Resolve`, the handling delegate carried on the SmartEnum row the fold reads; the `FinishLayer` coat stack lowers outermost-to-substrate onto the realized `bsdf#OPENPBR_SLAB` `Slab.Coat` slab (the `Topcoat` becoming the `Coat` slab weighted by its coat weight, the `Primer`/`Base` folding into the `MaterialParameters` base columns the `SlabStack.Lower` already lowers) so the coat stack targets an EXISTING slab and NEVER mints a second color register or a parallel coat owner — the same `Slab.Coat` the `weathering#WEATHERING` chalking trajectory raises; the produced row carries `Provenance` (the pigment count and the resolved-mix evidence a hand-keyed triple lacks) and re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so a finish row passes the same gamut/unit/IOR gate a registered row passes, and an empty mix, a pigment/weight length mismatch, or a non-finite reflectance rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class FinishKind {
    public static readonly FinishKind Paint    = new("paint",    new FinishHandling(BaseWeight: 1.0, Roughness: 0.45, Transmission: 0.0, ClearcoatBias: 0.0));
    public static readonly FinishKind Coating   = new("coating",  new FinishHandling(BaseWeight: 1.0, Roughness: 0.25, Transmission: 0.0, ClearcoatBias: 0.6));
    public static readonly FinishKind Stain     = new("stain",    new FinishHandling(BaseWeight: 0.35, Roughness: 0.55, Transmission: 0.5, ClearcoatBias: 0.0));
    public static readonly FinishKind Plaster   = new("plaster",  new FinishHandling(BaseWeight: 1.0, Roughness: 0.85, Transmission: 0.0, ClearcoatBias: 0.0));

    public FinishHandling Handling { get; }

    private FinishKind(string key, FinishHandling handling) : this(key) => Handling = handling;

    public MaterialParameters Seed(Unicolour reflectance, Unicolour substrate, double coatWeight, double coatRoughness) =>
        Switch(
            state: (reflectance, substrate, coatWeight, coatRoughness, Handling),
            paint:    static s => Row(s.reflectance, s.Handling, s.coatWeight, s.coatRoughness),
            coating:  static s => Row(s.reflectance, s.Handling, Math.Max(s.coatWeight, s.Handling.ClearcoatBias), s.coatRoughness),
            stain:    static s => Row(Finish.Blend(s.substrate, s.reflectance, s.Handling.BaseWeight), s.Handling, s.coatWeight, s.coatRoughness),
            plaster:  static s => Row(s.reflectance, s.Handling, 0.0, s.coatRoughness));

    static MaterialParameters Row(Unicolour baseColor, FinishHandling h, double coatWeight, double coatRoughness) =>
        new(baseColor, Metalness: 0.0, h.Roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
            Transmission: h.Transmission, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0,
            Clearcoat: Math.Clamp(coatWeight, 0.0, 1.0), ClearcoatRoughness: Math.Clamp(coatRoughness, 0.0, 1.0),
            Subsurface: 0.0, SubsurfaceRadius: Vector3d.Zero, Emission: Finish.Black, EmissionLuminance: 0.0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FinishLayer {
    private FinishLayer() { }

    public sealed record Primer(double Weight, Unicolour Reflectance) : FinishLayer;
    public sealed record Base(double Weight, Unicolour Reflectance) : FinishLayer;
    public sealed record Topcoat(double Weight, double Roughness, Option<Unicolour> Tint) : FinishLayer;

    public Unicolour Compose(Unicolour below) => Switch<Unicolour>(
        primer:  p => Finish.Blend(below, p.Reflectance, Math.Clamp(p.Weight, 0.0, 1.0)),
        @base:   b => Finish.Blend(below, b.Reflectance, Math.Clamp(b.Weight, 0.0, 1.0)),
        topcoat: static _ => below);

    public Option<Slab.Coat> Coat() => Switch(
        primer:  static _ => Option<Slab.Coat>.None,
        @base:   static _ => Option<Slab.Coat>.None,
        topcoat: t => Some(new Slab.Coat(Math.Clamp(t.Weight, 0.0, 1.0), t.Tint.Match(c => Finish.AcescgRgb(c), static () => RgbSpectrum.White), Math.Clamp(t.Roughness, 0.0, 1.0), Ior: 1.5, ThinFilmThickness: Option<double>.None, ThinFilmIor: 1.5)));
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct FinishHandling(double BaseWeight, double Roughness, double Transmission, double ClearcoatBias);

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

    public Fin<Unicolour> Reflectance(Op key) {
        Unicolour mixed = new(Pigments.ToArray(), Weights.ToArray());
        var lin = mixed.RgbLinear;
        return double.IsFinite(lin.R) && double.IsFinite(lin.G) && double.IsFinite(lin.B)
            ? Fin.Succ(new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, lin.R, lin.G, lin.B))
            : MaterialFault.Gamut(key, "<finish-mix-non-finite-reflectance>");
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class FinishPigment {
    public static readonly FrozenDictionary<string, Pigment> Catalogue =
        Wacton.Unicolour.Datasets.ArtistPaint.All
            .Select((p, i) => (Key: ArtistPaintName(i), Pigment: p))
            .ToFrozenDictionary(static r => r.Key, static r => r.Pigment, StringComparer.Ordinal);

    public static Fin<Pigment> Resolve(string name, Op key) =>
        Catalogue.TryGetValue(name, out Pigment? pigment)
            ? Fin.Succ(pigment!)
            : Fin.Fail<Pigment>(MaterialFault.Parameter(key, $"<unregistered-pigment:{name}>"));

    static string ArtistPaintName(int index) => index switch {
        0 => "bone-black", 1 => "burnt-sienna", 2 => "burnt-umber", 3 => "cadmium-orange",
        4 => "cadmium-red", 5 => "cadmium-yellow", 6 => "cerulean-blue", 7 => "cobalt-blue",
        8 => "dioxazine-purple", 9 => "hansa-yellow", 10 => "naphthol-red", 11 => "phthalo-blue",
        12 => "phthalo-green", 13 => "quinacridone-magenta", 14 => "raw-sienna", 15 => "raw-umber",
        16 => "titanium-white", 17 => "ultramarine-blue", 18 => "yellow-ochre", _ => $"pigment-{index}" };
}

public static class Finish {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    internal static readonly Unicolour Black = Linear(0.0, 0.0, 0.0);
    internal static readonly Unicolour Substrate = Linear(0.92, 0.92, 0.90);
    const double CheckerTolerance = 12.0;

    static readonly ConductorMetal NeutralBase = ConductorMetal.Aluminum;

    public static Fin<(MaterialParameters Row, Provenance Provenance)> Resolve(FinishKind kind, FinishMix mix, Seq<FinishLayer> stack, Op key) =>
        from reflectance in mix.Reflectance(key)
        let composed = stack.Fold(reflectance, static (below, layer) => layer.Compose(below))
        from admitted in Admit(composed, key)
        let topcoat = TopcoatOf(stack)
        let seed = kind.Seed(admitted, Substrate, topcoat.Weight, topcoat.Roughness)
        from lowered in LowerStack(stack, seed, key)
        from row in MaterialLibrary.Of(seed, key)
        select (row, MixProvenance(mix));

    static Fin<Unicolour> Admit(Unicolour reflectance, Op key) =>
        MaterialLibrary.PointerAdmit(reflectance, key)
            .Bind(admitted => MaterialLibrary.NearestChecker(admitted, CheckerTolerance, key).Map(_ => admitted));

    static Fin<LayeredBsdf> LowerStack(Seq<FinishLayer> stack, MaterialParameters seed, Op key) =>
        new SlabStack(stack.Choose(static l => l.Coat())
            .Fold(SlabStack.Lower(seed, NeutralBase).Slabs.Filter(static s => s is not Slab.Coat), static (slabs, coat) => slabs.Add(coat)))
            .ToLayered(key);

    static (double Weight, double Roughness) TopcoatOf(Seq<FinishLayer> stack) =>
        stack.Choose(static l => l is FinishLayer.Topcoat t ? Some((t.Weight, t.Roughness)) : Option<(double, double)>.None)
            .HeadOrNone().IfNone((0.0, 0.0));

    static Provenance MixProvenance(FinishMix mix) => new($"kubelka-munk:{mix.Pigments.Count}-pigment", mix.Pigments.Count, mix.Weights.Sum());

    internal static Unicolour Blend(Unicolour substrate, Unicolour pigment, double pigmentWeight) =>
        substrate.Mix(pigment, ColourSpace.RgbLinear, Math.Clamp(pigmentWeight, 0.0, 1.0), premultiplyAlpha: false);

    internal static RgbSpectrum AcescgRgb(Unicolour colour) { var lin = colour.RgbLinear; return new RgbSpectrum(lin.R, lin.G, lin.B); }
}
```

## [3]-[RESEARCH]

- [PIGMENT_MEASUREMENT_PROVENANCE]: the `ArtistPaint` Golden 19-pigment set is the seed Kubelka-Munk reflectance table — the two-constant `K/S` scattering coefficients measured under the sRGB/D50 `ArtistPaint.Configuration`, mixed through the admitted `new Unicolour(Pigment[], double[])` constructor. A custom architectural finish (a measured masonry-coating reflectance, a manufacturer paint chip) admits as one `new Pigment(start, interval, k, s, k1, k2, name)` two-constant construction the SAME `FinishMix` consumes — the probe is the spectrophotometer-to-`Pigment` measurement protocol per finish system, not a re-architecture of the mix. The `FinishPigment.Catalogue` index-to-name map is the alignment seed against the Golden pigment nomenclature; a measured pigment lands one named row.
- [POINTER_GAMUT_FINISH_ADMISSION]: the `graph#MATERIAL_LIBRARY` `PointerAdmit` Pointer real-surface gamut is the physical-reproducibility gate a pigment-mixed finish must pass — a mix that resolves to a chromaticity no real pigment can reflect (the Pointer gamut the empirical bound on real-surface colors) rails `MaterialFault.Gamut` and `MapToPointer` supplies the nearest reproducible reflectance. The `NearestChecker` `Macbeth.All` ColorChecker `DeltaE.Ciede2000` drift is the secondary witness — a mix within the Pointer gamut but drifting from every reference patch beyond the `CheckerTolerance` ΔE signals a mis-specified mix. The probe is the per-finish-system tolerance calibration against measured ColorChecker captures, not a second gamut owner.
- [COAT_STACK_THIN_FILM]: the `FinishLayer.Topcoat` lowers to the `bsdf#OPENPBR_SLAB` `Slab.Coat` slab whose `ThinFilmThickness`/`ThinFilmIor` fields carry the iridescent thin-film modifier when a coating's interference film is specified — a pearlescent automotive clearcoat or an anodized architectural finish rides the existing `Slab.Coat` thin-film field rather than a parallel iridescence owner. The probe is wiring a measured film thickness onto the topcoat's `ThinFilm` field; the flat tinted coat is the form until a consumer drives the interference path. The same `Slab.Coat` the `weathering#WEATHERING` chalking trajectory raises (`coat_roughness`) is the shared aging target, so a finish ages through the weathering operator over its own coat stack, never a per-finish aging variant.
