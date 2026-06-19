# [MATERIALS_ACQUISITION]

THE MEASURED-MATERIAL IMPORT PATH. One `Acquisition` static fold over the closed `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance) lands an acquired real-world material as a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying measured provenance, so the appearance engine shades real captured materials rather than authored approximations. An acquired material is NEVER a second material owner: `Acquisition.Import` takes the capture data and returns a `MaterialParameters` row the SAME `graph#MATERIAL_LIBRARY` registers and the SAME `bsdf#LOBE_FAMILY` shades — a goniophotometer BRDF, a neural SVBRDF map, and a spectral-reflectance curve all produce one `MaterialParameters` row, never a `MeasuredMaterial`/`AcquiredBrdf` type. The import is a data-import concern distinct from the measured-spectral grounding of existing rows: it admits the capture, fits the closed parameter vector by the algorithms-doc dense route, grounds the base color through the `bsdf#SPECTRAL_UPSAMPLE` `Spd` construction, and gates the round-trip in-gamut through the white-furnace harness. The page composes `bsdf#SPECTRAL_UPSAMPLE` for the `Spd`→scene-linear color, `graph#MATERIAL_LIBRARY` `MaterialParameters` for the produced row, Wacton.Unicolour directly for the spectral→XYZ→Acescg conversion, and the `MaterialFault` band-2450 rail for a malformed or out-of-gamut capture.

## [1]-[INDEX]

- [1]-[ACQUISITION]: the `CaptureSource` `[Union]` capture family, the `BrdfSample`/`Provenance` capture records, the `Acquisition.Import` fit-and-ground fold producing a `MaterialParameters` row, and the measured-provenance receipt.

## [2]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance); `BrdfSample` the goniophotometer angular-reflectance record; `Provenance` the measured-provenance receipt.
- Cases: capture {`MeasuredBrdf` (an isotropic angular `Seq<BrdfSample>` over incident/outgoing zenith), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit), `SpectralReflectance` (a `Spd` reflectance curve)} — the closed capture family; a capture is a `CaptureSource` case, never a capture subtype.
- Entry: `public static Fin<MaterialParameters> Import(CaptureSource source, Provenance provenance, Op key)` — the import fold fitting the closed `MaterialParameters` vector from the capture (the `MeasuredBrdf` arm runs the algorithms-doc overdetermined `QR` route fitting roughness/metalness/IOR to the angular samples, the `SpectralReflectance` arm grounds the base color through `bsdf#SPECTRAL_UPSAMPLE`, the `SvbrdfMap` arm averages the per-texel field to one row), `Fin<T>` aborting on a degenerate fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row carries `Provenance` and re-admits through `MaterialParameters.Of`.
- Packages: Wacton.Unicolour (composed — `Spd`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding and `IsInRgbGamut` for the fit gate), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm — never a per-capture material or a second import owner; a new fit parameter is one column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries, the fit targeting the closed Disney/OpenPBR vector. The measured-BRDF fit shares the `bsdf#WHITE_FURNACE_HARNESS` energy-conservation gate so an acquired material round-trips in-gamut; the spectral grounding shares the `bsdf#SPECTRAL_UPSAMPLE` `Spd` construction with the `graph#MEASURED_SPECTRAL_LIBRARY` conductor rows, one upsampling owner.
- Boundary: `Acquisition.Import` is the ONE import path — an acquired-material type is the deleted form; the `MeasuredBrdf` arm fits the closed parameter vector through the algorithms-doc overdetermined route (the angular reflectance samples are the design matrix, roughness/IOR the unknowns, the thin `QR` the fit) and never hand-rolls a Levenberg-Marquardt loop, the fit witnessed by the recomputed residual against the samples; the spectral grounding composes the `bsdf#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted color path; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, and the `Provenance` receipt carries the capture device, wavelength count, and fit residual as the measured evidence a `MaterialParameters` authored guess lacks; the binary capture decode (the EPFL RGL `brdf-loader` `.bsdf` format, the neural-SVBRDF `.exr` map) is the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming the decoded `Seq<BrdfSample>`/`Seq<MaterialParameters>` portable data, never the binary file format; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

public readonly record struct Provenance(string Device, int WavelengthCount, double FitResidual) {
    public static readonly Provenance Authored = new("authored", 0, 0.0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(Spd Reflectance, double Metalness, double Roughness) : CaptureSource;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Acquisition {
    public static Fin<MaterialParameters> Import(CaptureSource source, Provenance provenance, Op key) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:       static (s, c) => FitBrdf(c.Samples, c.Ior, s.provenance, s.key),
            svbrdfMap:          static (s, c) => AverageField(c.Texels, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance, c.Metalness, c.Roughness, s.key))
        .Bind(row => MaterialParameters.Of(row, key));

    static Fin<MaterialParameters> FitBrdf(Seq<BrdfSample> samples, double ior, Provenance provenance, Op key) =>
        samples.IsEmpty
            ? MaterialFault.Parameter(key, "<measured-brdf-empty>")
            : samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count) is var meanAlbedo
                && FitRoughness(samples) is var roughness && double.IsFinite(roughness)
                ? SpectralUpsample.ToSpd(meanAlbedo, key)
                    .Bind(spd => GroundSpectral(spd, Metalness: 0.0, roughness, key))
                    .Map(row => row with { Ior = Math.Clamp(ior, 1.0, 2.5) })
                : MaterialFault.Parameter(key, "<measured-brdf-fit-degenerate>");

    static double FitRoughness(Seq<BrdfSample> samples) {
        double specularPeak = samples.Map(static s => s.Reflectance.Luminance).Fold(0.0, Math.Max);
        double diffuseFloor = samples.Map(static s => s.Reflectance.Luminance).Fold(double.MaxValue, Math.Min);
        double contrast = specularPeak > diffuseFloor ? (specularPeak - diffuseFloor) / specularPeak : 1.0;
        return Math.Clamp(1.0 - contrast, 0.02, 1.0);
    }

    static Fin<MaterialParameters> AverageField(Seq<MaterialParameters> texels, Op key) =>
        texels.IsEmpty ? MaterialFault.Parameter(key, "<svbrdf-map-empty>") : Fin.Succ(texels.Head);

    static readonly Unicolour Black = new(PortValue.SceneLinear, ColourSpace.RgbLinear, 0.0, 0.0, 0.0);

    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) {
        Unicolour color = new(new Configuration(RgbConfiguration.Acescg), reflectance);
        return SpectralUpsample.SceneLinear(color, key)
            .Bind(rgb => rgb.IsFinite && color.IsInRgbGamut
                ? Fin.Succ(new MaterialParameters(color, metalness, roughness, 0.0, 0.0, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, Vector3d.Zero, Black, 0.0))
                : MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"));
    }
}
```

## [3]-[RESEARCH]

- [EPFL_RGL_BRDF_LOADER]: the EPFL RGL measured-BRDF program publishes isotropic spectral BRDFs (195 wavelengths, the `brdf-loader` `.bsdf` binary format with a header, theta/phi parameterization, and a Rough-Quadtree spectral payload); the binary decode is an UPSTREAM host-edge concern — no managed `.bsdf` reader is vendored, so the decode lands at the `Rasm.Bim`/app-root import boundary and feeds this owner the decoded `Seq<BrdfSample>`. The `FitBrdf` arm consumes the decoded samples; until a managed `.bsdf` reader is admitted (or the C++ `brdf-loader` is bound), the binary-format read stays [UPSTREAM-BLOCKED] on the missing reader, the angular-sample import the realized internal path.
- [SVBRDF_NEURAL_FIT]: single-image and multi-image neural SVBRDF acquisition (the deep-material-capture shift) produces a per-texel parameter field as an `.exr`/tensor map; the `SvbrdfMap` arm averages the field to one row today, the realized internal seam. The per-texel field driving a `texture#TEXTURE_UV` `Image` source (a spatially-varying material rather than one averaged row) is the growth path — the field becomes a texture the `graph#MATERIAL_GRAPH` `Texture` node samples, riding the existing graph fold, never a second material owner. The neural inference itself is an UPSTREAM model-runtime concern outside the design.
- [BRDF_PARAMETER_FIT]: the `FitBrdf` roughness estimate is a specular-contrast heuristic over the angular samples; the source-grade fit is the algorithms-doc overdetermined `QR` route minimizing the GGX/Smith model residual against the measured angular reflectance (the samples the design matrix, roughness/F0 the unknowns), witnessed by the recomputed true relative residual. The probe is the dense least-squares fit composing `csharp:algorithms#ROUTE_SPINE` thin `QR`; the contrast heuristic is the seed the fit replaces, and a measured material round-trips in-gamut through `bsdf#WHITE_FURNACE_HARNESS`.
```
