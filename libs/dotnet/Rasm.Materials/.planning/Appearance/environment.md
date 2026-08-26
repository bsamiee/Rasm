# [MATERIALS_ENVIRONMENT]

THE SKY, ENVIRONMENT-MAP, AND IMAGE-BASED-LIGHTING OWNER. One `SkyModel` `[Union]` synthesizes a scene-linear radiance field from an analytic daylight model — the Hosek-Wilkie fitted-coefficient asset over its solar-elevation Bézier table and the ISO 15469 fifteen-type CIE standard sky over one gradation × indicatrix algebra — one `EnvironmentMap` admits any equirect, cube-face, or octahedral layout under the frozen `+Z`-up correspondence and carries the whole layout relation in one `Project` fold, one `IblPrefilter.Prefilter` reduces an admitted map to the `IblProducts` set every renderer consumes (SH9 irradiance, the GGX roughness-ordered specular level set, split-sum BRDF LUT, marginal-conditional luminance CDF), and one `EnvironmentLight` record is the row `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` resolves over the declared `[BOUNDARY]` contract — directional radiance, importance sample, SH irradiance, specular level, and split-sum read all answering on that one owner so no consumer re-derives the mapping. `SkyModel` admits a sky variation as one case and `CieSkyType` as one ROW, `MapLayout` a storage arrangement as one ROW, and `IblProducts` a prefilter product as one COLUMN — never a per-model sky type, a direction-named converter pair, or a second SH spelling. Every owner here composes the `bsdf#MICROFACET_KERNEL` `Microfacet` VNDF sampler and Smith masking for every GGX integral (the prefiltered dome and the shaded surface integrate the SAME distribution), the `bsdf#SHADING_FRAME` `LocalVector<T>`/`RgbSpectrum`/`MaterialFault` band-2450 channel, the `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg working space, the `photometric#PHOTOMETRIC` unit gate for an authored zenith luminance, the `Raster/plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid` typed-texel arena with its `Read`/`Write` row accessors and `AsImage` sampler bridge, the `texture#TEXTURE_UV` sampler for every filtered read, the contract `Rasm.Element` `ContentAddress` for every blob key, the kernel `Dimension`/`UnitInterval`/`Op` atoms with `CommunityToolkit.HighPerformance` struct-action partitioning, and NodaTime `Instant` for solar position — re-minting no plane, no colour space, no fault, and no hash. Solar geometry composes the kernel `Rasm/Numerics/calculus#SOLAR_EPHEMERIS` almanac and projects HERE into the frozen frame, so no host sun object and no second ephemeris crosses the host-neutral boundary; the container decode of an ingested HDRI is `Raster/codec#RASTER_CODEC`, this owner consuming the decoded plane alone.

## [01]-[INDEX]

- [02]-[SKY_MODEL]: `CieGradation`/`CieIndicatrix`/`CieSkyType` close the standard-sky algebra, `SkyCoefficients` and `SolarCoefficients` carry the two content-keyed fitted assets over one `ControlGrid` Bézier algebra, `SolarFrame` projects the kernel almanac's geodetic sun into the frozen frame, `SkyModel` `[Union]` states the radiance law with its ground hemisphere and its solar disc, and `SkyRender.Radiance` supplies the per-texel radiance closure the press subject sweeps.
- [03]-[ENVIRONMENT_MAP]: `MapLayout` bands storage with its per-row coordinate law, `Equirectangular` freezes the correspondence, `EnvironmentMap.Of` admits with its per-layer sampler lift, `Stored`/`Radiance` answer the stored-frame and world-frame reads, and `Project` carries the one layout relation in both directions.
- [04]-[IBL_PREFILTER]: `ShBand` tables the nine-row basis, `Sh9` pairs projection with irradiance reconstruction, `IblPrefilter` integrates the GGX specular level set, the split-sum BRDF LUT, and the luminance CDF over the kernel `Deterministic.Hammersley` draw, `IblProducts` carries the CPU product set, and `IblProduct` splits the content-addressable mint from the accelerator lane's key-less preview.
- [05]-[ENVIRONMENT_LIGHT]: `EnvironmentLight` resolves the row the render boundary consumes, gates its own admission, and publishes the six reads an integrator and a raster shading pass share.

## [02]-[SKY_MODEL]

- Owner: `WorldDirection` the page-owned `+Z`-up WORLD direction carrier every dome surface speaks (structurally distinct from the `bsdf#SHADING_FRAME` tangent `LocalVector<T>`); `SkyModel` `[Union]` (`HosekWilkie` · `CieStandard`); `SkyCoefficients` and `SolarCoefficients` the two content-keyed fitted assets over the one `ControlGrid` interpolation algebra; `SolarDisc` the resolved direct-beam term; `CieSkyType`/`CieGradation`/`CieIndicatrix` `[SmartEnum<int>]` bands; `SolarFrame` the frame projection over the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac; `SkyAtmosphere` the turbidity, ground-albedo, admitted-zenith-level, exposure, and solar-angular-diameter row; `SkyRender` the radiance-closure surface; `SkySpectrum` the one band→scene colour path both fitted assets cross.
- Cases: sky {`HosekWilkie` (the fitted anisotropic-Mie daylight model over a `SkyCoefficients` diffuse asset paired with its `SolarCoefficients` limb-darkened disc asset), `CieStandard` (the ISO 15469 relative-luminance distribution over a `CieSkyType` row)}; gradation {`I`…`VI`}; indicatrix {`One`…`Six`}; sky-type {`Type01`…`Type15`, each binding one gradation and one indicatrix — `Type01` the CIE Overcast Sky, `Type12` the CIE Standard Clear Sky}.
- Entry: `public static Func<Vector3d, RgbSpectrum> Radiance(SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun)` is the ONE synthesis surface — the per-texel radiance closure `Raster/press#PRESS_PLAN` `PressSubject.Sky` calls under `PressProgram.Dome`, so the sky owner supplies the model and the press owns partitioning, cancellation, the run record, and the accelerator lane; `SolarFrame.Of(latitudeDegrees, longitudeDegrees, instant, key, elevationM)` is the ONE sun-direction entry, so a caller holding a measured direction passes it and a caller holding a site and a clock resolves it here; `SkyModel.Disc(WorldDirection sun, SkyAtmosphere atmosphere)` is the ONE direct-beam read both cases answer. `MaterialFault` fails a sub-unit or super-decade turbidity, a negative zenith level, a non-positive exposure, an out-of-band solar diameter, an out-of-range site, and a non-finite radiance.
- Law: radiance covers the WHOLE sphere. Each model distributes its own radiance over the upper hemisphere; the lower hemisphere is the GROUND — `GroundAlbedo` times the model's horizon radiance, evaluated once per texel through the same case — so a synthesized dome carries a real bounce rather than the mirrored bright band a clamped zenith cosine produces below the horizon. `GroundAlbedo` reaches that ground term as the same `RgbSpectrum` the Hosek-Wilkie fit consumes as its albedo axis, so one authored value drives both the sky's own inter-reflection and the dome's lower half.
- Law: the SYNTHESIZED FIELD carries the sky alone and the DISC rides its own term. A half-degree source four decades brighter than the sky around it lands in one texel of a bounded dome, so writing it into the plane makes the `[04]` guide's texel measure the only structure importance-sampling it — a firefly no tap budget resolves and a quadrature error the SH projection carries forever. `Radiance` is therefore the diffuse field at every direction and `Disc` the direct beam the `[05]` row publishes as its own arm, which is what lets one dome serve a raster read and a path-traced draw without double-counting the sun.
- Law: the MEASURED-KERNEL carve-out is declared ONCE here and nowhere per site — the `readonly struct` `IAction` row sweeps writing into the plane owner's `Write` accessor by row index (the carve `texture#TEXTURE_UV` `ProceduralNoise` also names), the band and Bernstein folds over a fitted block, the type-init basis reconstruction, the bounded disc quadrature, the running-mass guide and its bisection, and the fixture proof's own grid pass. Every other operation is expression-bodied and result-threaded, so a loop outside that carve is a defect readable against this one bullet rather than against a comment at each site.
- Law: radiance leaves a model in scene-linear AP1 channels at `PlaneTransfer.Linear`, so a display transfer never enters a SYNTHESIZED plane and the tone map stays `surface#TONE_MAP`'s. Every channel folds through `Finite.Spectrum` before `RgbSpectrum.Create`, because the validated carrier's own admission THROWS inside a partitioned sweep no result covers.
- Law: every published tolerance on this page is a kernel `Domain/context` `Tolerance` on a NAMED lane, admitted once at its own entry — the SH band bar on `ToleranceLane.Spectral`, the irradiance reconstruction bar on `ToleranceLane.Irradiance`, the solid-angle closure bar on `ToleranceLane.Conservation` — so a project tightening the dome proof reaches all three through `Context.Override` and none is a page constant a consumer cannot move.
- Packages: Wacton.Unicolour (composed at the consuming edge — the scene-linear basis is `graph#MATERIAL_GRAPH` `PortValue.SceneLinear`, and a chromatic sky lands as an admitted `photometric#PHOTOMETRIC` `EmissionInput` tint, never a re-minted illuminant), NodaTime (`Instant` the fold's clock carrier and `Offset` the site's zone — a `DateTime` with an inferred kind is the fabricated-instant defect), CommunityToolkit.HighPerformance (`SpanOwner<T>` per-row scratch, `ParallelHelper.For` over a struct `IAction` row), Rasm (project — `Dimension`/`UnitInterval`/`Op`, `Tolerance`/`ToleranceLane`, `Deterministic.Hammersley`, and the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition.At`/`SolarSite`/`SunPosition` almanac), Rasm.Element (project — `ContentAddress`), Rasm.Materials.Appearance.Bsdf (`LocalVector<T>`/`RgbSpectrum`/`MaterialFault`/`Microfacet<T>`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new standard sky is one `CieSkyType` row over the existing group pair — the fifteen admitted types PROJECT the six × six product, so a national-annex sky is a row and never a case; a new gradation or indicatrix is one row on its own band; a genuinely new analytic radiance law is one `SkyModel` case; a new atmospheric parameter is one `SkyAtmosphere` column; a new tolerance gate is one `ToleranceLane` row read here.
- Growth: `SkyCoefficients` and `SolarCoefficients` are TWO CONTENT-KEYED DATA ASSETS, this page's one carve from the generated-table law (branch RULINGS: a published table with no defining sequence stays a content-keyed asset) — both are least-squares fits over a brute-force simulation, so each `Of` admits a caller-supplied block against its own declared extents and a revised fit is a NEW digest. They stay TWO because their tables differ in RANK: the diffuse fit carries a ground-albedo axis the disc fit has no term for and the disc fit a limb-darkening axis the diffuse fit has no direction for, so one extent gate over a merged block admits either array in the other's slot. `ControlGrid` owns the interpolation ONCE — the Bernstein weights generate from the binomial at the declared degree — so a third fitted asset is a record with its own extents and no second derivation.
- Boundary: solar position resolves the apparent refraction-corrected topocentric direction in the frozen `+Z`-up frame — `+X` north, `+Y` west, azimuth FROM `+X` increasing EASTWARD onto `−Y`, the OPPOSITE angular sense of the `[03]` equirect `u`. The fold carries that sign exactly once (`−sin(azimuth)` on the `Y` lane), so the two conventions meet in the direction VALUE and never share an angular sense a transcriber could copy wrongly. Geodetic datum, site CRS, and reprojection stay the app-root edge's; this owner takes latitude, longitude, and site elevation as admitted scalars and pins the site zone at `Offset.Zero`, the almanac's true-solar-minutes fold cancelling it. Site HEIGHT is a real axis: the almanac corrects Bennett refraction by the barometric ratio at the site's own height, so a hardcoded sea level answers every alpine study at the wrong horizon band.
- Boundary: every authored light magnitude crosses `photometric#PHOTOMETRIC` `Photometric.Admit` — `SkyAtmosphere.Of` for the zenith level and `EnvironmentMap.Of` for the dome intensity — so a `cd/m²` sky and a `lux` sky reach one radiometric scalar with no page-local efficacy divide, and each row carries the whole `EmissionEvidence` rather than a bare scalar. Ground albedo enters as an `RgbSpectrum`, so a spectrally tinted bounce is representable and a scalar albedo is the grey triple. SOLAR ANGULAR DIAMETER is an admitted column with NO page default: the disc a study wants is the site's own apparent diameter at its own date, and a transcribed mean ships one epoch's astronomy as this owner's law. The `CieStandard` disc is its own indicatrix at zero angular distance, so the ratio distribution and its direct beam are ONE algebra.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Photometric;
using Rasm.Materials.Appearance.Texture;
using Rasm.Materials.Raster;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;

// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct WorldDirection(double X, double Y, double Z) {
    public double CosZenith => Z;
    public double Dot(WorldDirection o) => (X * o.X) + (Y * o.Y) + (Z * o.Z);
    public WorldDirection Add(WorldDirection o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public WorldDirection Scale(double s) => new(X * s, Y * s, Z * s);

    public WorldDirection Normalize() {
        double n = Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        return n > 1e-12 ? new(X / n, Y / n, Z / n) : Zenith;
    }

    public WorldDirection Oriented(double x, double y, double z) {
        WorldDirection up = Math.Abs(Z) < 0.999 ? Zenith : new WorldDirection(1.0, 0.0, 0.0);
        WorldDirection t = new WorldDirection((up.Y * Z) - (up.Z * Y), (up.Z * X) - (up.X * Z), (up.X * Y) - (up.Y * X)).Normalize();
        WorldDirection b = new((Y * t.Z) - (Z * t.Y), (Z * t.X) - (X * t.Z), (X * t.Y) - (Y * t.X));
        return t.Scale(x).Add(b.Scale(y)).Add(Scale(z)).Normalize();
    }

    public static readonly WorldDirection Zenith = new(0.0, 0.0, 1.0);
}

[SmartEnum<int>]
public sealed partial class CieGradation {
    public static readonly CieGradation I   = new(key: 1, a:  4.0, b: -0.70);
    public static readonly CieGradation II  = new(key: 2, a:  1.1, b: -0.80);
    public static readonly CieGradation III = new(key: 3, a:  0.0, b: -1.00);
    public static readonly CieGradation IV  = new(key: 4, a: -1.0, b: -0.55);
    public static readonly CieGradation V   = new(key: 5, a: -1.0, b: -0.32);
    public static readonly CieGradation VI  = new(key: 6, a: -1.0, b: -0.15);
    public double A { get; }
    public double B { get; }

    public double Phi(double cosZenith) => 1.0 + (A * Math.Exp(B / Math.Max(1e-4, cosZenith)));
    public double PhiZenith => 1.0 + (A * Math.Exp(B));
}

[SmartEnum<int>]
public sealed partial class CieIndicatrix {
    public static readonly CieIndicatrix One   = new(key: 1, c:  0.0, d: -1.0, e: 0.00);
    public static readonly CieIndicatrix Two   = new(key: 2, c:  2.0, d: -1.5, e: 0.15);
    public static readonly CieIndicatrix Three = new(key: 3, c:  5.0, d: -2.5, e: 0.30);
    public static readonly CieIndicatrix Four  = new(key: 4, c: 10.0, d: -3.0, e: 0.45);
    public static readonly CieIndicatrix Five  = new(key: 5, c: 16.0, d: -3.0, e: 0.30);
    public static readonly CieIndicatrix Six   = new(key: 6, c: 24.0, d: -2.8, e: 0.15);
    public double C { get; }
    public double D { get; }
    public double E { get; }

    public double F(double chi) =>
        1.0 + (C * (Math.Exp(D * chi) - Math.Exp(D * (Math.PI / 2.0)))) + (E * Math.Cos(chi) * Math.Cos(chi));
}

[SmartEnum<int>]
public sealed partial class CieSkyType {
    public static readonly CieSkyType Type01 = new(key:  1, gradation: CieGradation.I,   indicatrix: CieIndicatrix.One);
    public static readonly CieSkyType Type02 = new(key:  2, gradation: CieGradation.I,   indicatrix: CieIndicatrix.Two);
    public static readonly CieSkyType Type03 = new(key:  3, gradation: CieGradation.II,  indicatrix: CieIndicatrix.One);
    public static readonly CieSkyType Type04 = new(key:  4, gradation: CieGradation.II,  indicatrix: CieIndicatrix.Two);
    public static readonly CieSkyType Type05 = new(key:  5, gradation: CieGradation.III, indicatrix: CieIndicatrix.One);
    public static readonly CieSkyType Type06 = new(key:  6, gradation: CieGradation.III, indicatrix: CieIndicatrix.Two);
    public static readonly CieSkyType Type07 = new(key:  7, gradation: CieGradation.III, indicatrix: CieIndicatrix.Three);
    public static readonly CieSkyType Type08 = new(key:  8, gradation: CieGradation.III, indicatrix: CieIndicatrix.Four);
    public static readonly CieSkyType Type09 = new(key:  9, gradation: CieGradation.IV,  indicatrix: CieIndicatrix.Two);
    public static readonly CieSkyType Type10 = new(key: 10, gradation: CieGradation.IV,  indicatrix: CieIndicatrix.Three);
    public static readonly CieSkyType Type11 = new(key: 11, gradation: CieGradation.IV,  indicatrix: CieIndicatrix.Four);
    public static readonly CieSkyType Type12 = new(key: 12, gradation: CieGradation.V,   indicatrix: CieIndicatrix.Four);
    public static readonly CieSkyType Type13 = new(key: 13, gradation: CieGradation.V,   indicatrix: CieIndicatrix.Five);
    public static readonly CieSkyType Type14 = new(key: 14, gradation: CieGradation.VI,  indicatrix: CieIndicatrix.Five);
    public static readonly CieSkyType Type15 = new(key: 15, gradation: CieGradation.VI,  indicatrix: CieIndicatrix.Six);
    public CieGradation Gradation { get; }
    public CieIndicatrix Indicatrix { get; }

    public string WireKey => string.Create(CultureInfo.InvariantCulture, $"cie-standard-{Key:D2}");

    public double Relative(double cosZenith, double chi, double solarZenith) =>
        Indicatrix.F(chi) * Gradation.Phi(cosZenith) / (Indicatrix.F(solarZenith) * Gradation.PhiZenith);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapLayout {
    public static readonly MapLayout Equirect =
        new("equirect", layers: 1, law: LayerLaw.None, aspect: 2.0, forward: Equirectangular.Of, inverse: Equirectangular.Direction,
            layerOf: static _ => 0, solidAngle: Equirectangular.Measure,
            sampler: new SamplerState(AddressMode.Repeat, AddressMode.Clamp, FilterMode.Trilinear, UvFrame.Identity));
    public static readonly MapLayout CubeFaces =
        new("cube-faces", layers: 6, law: LayerLaw.CubeFaces, aspect: 1.0, forward: Cube.Of, inverse: Cube.Direction,
            layerOf: Cube.Face, solidAngle: Cube.Measure,
            sampler: new SamplerState(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Trilinear, UvFrame.Identity));
    public static readonly MapLayout Octahedral =
        new("octahedral", layers: 1, law: LayerLaw.None, aspect: 1.0, forward: Octahedron.Of, inverse: Octahedron.Direction,
            layerOf: static _ => 0, solidAngle: Octahedron.Measure,
            sampler: new SamplerState(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Trilinear, UvFrame.Identity));
    public int Layers { get; }
    public LayerLaw Law { get; }
    public double Aspect { get; }
    public SamplerState Sampler { get; }

    [UseDelegateFromConstructor]
    public partial (UnitInterval U, UnitInterval V) Forward(WorldDirection direction);
    [UseDelegateFromConstructor]
    public partial WorldDirection Inverse(UnitInterval u, UnitInterval v, int layer);

    [UseDelegateFromConstructor]
    public partial int LayerOf(WorldDirection direction);

    [UseDelegateFromConstructor]
    public partial double SolidAngle(WorldDirection direction, int width, int height);

    public (Dimension Width, Dimension Height) Extent(Dimension edge) =>
        (Dimension.Create((int)Math.Round(edge.Value * Aspect)), edge);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SkyAtmosphere(
    double Turbidity, RgbSpectrum GroundAlbedo, ReadOnlyMemory<double> BandAlbedo,
    EmissionEvidence ZenithLevel, double Exposure, double SolarDiameter) {
    public double ZenithRadiance => ZenithLevel.RadiometricSi;
    public const double SolarDiameterCeiling = Math.PI / 36.0;

    public double HalfAngleCosine => Math.Cos(SolarDiameter / 2.0);

    public double SolidAngle => 2.0 * Math.PI * (1.0 - HalfAngleCosine);

    public static Fin<SkyAtmosphere> Of(
        double turbidity, RgbSpectrum groundAlbedo, PhotometricQuantity zenithQuantity, double zenithValue, Enum zenithUnit,
        double exposure, double solarDiameter, Op key, Guid correlation) =>
        double.IsFinite(turbidity) && turbidity is >= 1.0 and <= 10.0
        && double.IsFinite(exposure) && exposure > 0.0
        && double.IsFinite(solarDiameter) && solarDiameter > 0.0 && solarDiameter <= SolarDiameterCeiling
            ? from level in Photometric.Admit(zenithQuantity, zenithValue, zenithUnit, key, correlation)
              from bands in SkySpectrum.BandAlbedo(groundAlbedo, key)
              select new SkyAtmosphere(turbidity, groundAlbedo, bands, level, exposure, solarDiameter)
            : new MaterialFault.Parameter(key,
                  $"<sky-atmosphere-out-of-range:{turbidity:R},{exposure:R},{solarDiameter:R}>");
}

public readonly record struct RenderBudget(int ParallelFloor, int Bands, bool Parallel, BakeGovernance Governance = default) {
    public static readonly RenderBudget Default = new(ParallelFloor: 32, Bands: 16, Parallel: true);
    public int Floor => Parallel ? Math.Max(1, ParallelFloor) : int.MaxValue;
    public RenderBudget Governed(Option<IProgress<double>> progress, CancellationToken cancel) =>
        this with { Governance = Governance.Governed(progress, cancel) };
    public Option<Error> Opened(int done, int total) =>
        Governance.Opened(total <= 0 ? 1.0 : done / (double)total);

    public Fin<Unit> Sweep<TAction>(int stacked, in TAction action, Op key) where TAction : struct, IAction {
        TAction seeded = action;
        return key.Catch(() => {
            int bands = Math.Max(1, Math.Min(Bands, stacked));
            for (int band = 0; band < bands; band++) {
                if (Opened(band, bands).Case is Error abandoned) { return Fin.Fail<Unit>(abandoned); }
                ParallelHelper.For((int)((long)stacked * band / bands), (int)((long)stacked * (band + 1) / bands), seeded, Floor);
            }
            return Fin.Succ(Unit.Default);
        });
    }
}

public sealed record SkyCoefficients(
    ReadOnlyMemory<double> Fitted, int Channels, int AlbedoNodes, int TurbidityNodes, int ControlPoints, int Terms, ContentAddress Key) {
    public static Fin<SkyCoefficients> Of(
        ReadOnlyMemory<double> fitted, int channels, int albedoNodes, int turbidityNodes, int controlPoints, int terms, Op key) =>
        channels == SkySpectrum.BandCount && albedoNodes > 0 && turbidityNodes > 0 && controlPoints > 1 && terms > 0
        && fitted.Length == channels * albedoNodes * turbidityNodes * controlPoints * terms
        && Finite.All(fitted.Span)
            ? Fin.Succ(new SkyCoefficients(fitted, channels, albedoNodes, turbidityNodes, controlPoints, terms,
                  ContentAddress.Of(MemoryMarshal.AsBytes(fitted.Span))))
            : new MaterialFault.Parameter(key, $"<sky-coefficients-extent:{fitted.Length}>");

    public double Term(int channel, double albedo, double turbidity, double elevation, int term) {
        ReadOnlySpan<double> block = Fitted.Span;
        (int a0, double at) = ControlGrid.Node(albedo, AlbedoNodes);
        (int t0, double tt) = ControlGrid.Node((turbidity - 1.0) / 9.0, TurbidityNodes);
        (int a1, int t1) = (Math.Min(a0 + 1, AlbedoNodes - 1), Math.Min(t0 + 1, TurbidityNodes - 1));
        double lo = ControlGrid.Lerp(Curve(block, channel, a0, t0, elevation, term), Curve(block, channel, a0, t1, elevation, term), tt);
        double hi = ControlGrid.Lerp(Curve(block, channel, a1, t0, elevation, term), Curve(block, channel, a1, t1, elevation, term), tt);
        return ControlGrid.Lerp(lo, hi, at);
    }

    double Curve(ReadOnlySpan<double> block, int channel, int albedo, int turbidity, double elevation, int term) =>
        ControlGrid.Bezier(block, At(channel, albedo, turbidity, controlPoint: 0, term), Terms, ControlPoints, elevation);

    int At(int channel, int albedo, int turbidity, int controlPoint, int term) =>
        ((((((channel * AlbedoNodes) + albedo) * TurbidityNodes) + turbidity) * ControlPoints) + controlPoint) * Terms + term;
}

public sealed record SolarCoefficients(
    ReadOnlyMemory<double> Fitted, int Channels, int TurbidityNodes, int ControlPoints, int LimbTerms, ContentAddress Key) {
    public static Fin<SolarCoefficients> Of(
        ReadOnlyMemory<double> fitted, int channels, int turbidityNodes, int controlPoints, int limbTerms, Op key) =>
        channels == SkySpectrum.BandCount && turbidityNodes > 0 && controlPoints > 1 && limbTerms > 0
        && fitted.Length == channels * turbidityNodes * controlPoints * limbTerms
        && Finite.All(fitted.Span)
            ? Fin.Succ(new SolarCoefficients(fitted, channels, turbidityNodes, controlPoints, limbTerms,
                  ContentAddress.Of(MemoryMarshal.AsBytes(fitted.Span))))
            : new MaterialFault.Parameter(key, $"<solar-coefficients-extent:{fitted.Length}>");

    public double Radiance(int channel, double turbidity, double elevation, UnitInterval discRadius) {
        ReadOnlySpan<double> block = Fitted.Span;
        (int t0, double tt) = ControlGrid.Node((turbidity - 1.0) / 9.0, TurbidityNodes);
        int t1 = Math.Min(t0 + 1, TurbidityNodes - 1);
        double mu = Math.Sqrt(Math.Max(0.0, 1.0 - (discRadius.Value * discRadius.Value))), sum = 0.0;
        for (int limb = LimbTerms - 1; limb >= 0; limb--) {
            double lo = ControlGrid.Bezier(block, At(channel, t0, controlPoint: 0, limb), LimbTerms, ControlPoints, elevation);
            double hi = ControlGrid.Bezier(block, At(channel, t1, controlPoint: 0, limb), LimbTerms, ControlPoints, elevation);
            sum = (sum * mu) + ControlGrid.Lerp(lo, hi, tt);
        }
        return sum;
    }

    int At(int channel, int turbidity, int controlPoint, int limb) =>
        ((((channel * TurbidityNodes) + turbidity) * ControlPoints) + controlPoint) * LimbTerms + limb;
}

internal static class ControlGrid {
    public static double Bezier(ReadOnlySpan<double> block, int first, int stride, int controlPoints, double parameter) {
        double s = Math.Clamp(parameter, 0.0, 1.0 - 1e-12), inverse = 1.0 - s, sum = 0.0, weight = Math.Pow(inverse, controlPoints - 1);
        for (int i = 0; i < controlPoints; i++) {
            sum += weight * block[first + (i * stride)];
            weight = i + 1 < controlPoints ? weight * s * (controlPoints - 1 - i) / ((i + 1) * inverse) : weight;
        }
        return sum;
    }

    public static (int Node, double Fraction) Node(double unit, int nodes) {
        double scaled = Math.Clamp(unit, 0.0, 1.0) * (nodes - 1);
        int index = Math.Clamp((int)Math.Floor(scaled), 0, Math.Max(0, nodes - 2));
        return (index, scaled - index);
    }

    public static double Lerp(double a, double b, double t) => a + ((b - a) * t);
}

public sealed record SolarDisc(
    WorldDirection Direction, double CosHalfAngle, double SolidAngle, Func<UnitInterval, RgbSpectrum> Limb) {
    private const int MeanNodes = 32;

    public RgbSpectrum Centre => Limb(UnitInterval.Create(0.0));
    public RgbSpectrum Mean { get; } = Average(Limb);

    public bool Contains(WorldDirection direction) => direction.Normalize().Dot(Direction) >= CosHalfAngle;

    public UnitInterval Radius(WorldDirection direction) =>
        Math.Clamp(direction.Normalize().Dot(Direction), -1.0, 1.0) switch {
            var cosine => UnitInterval.Create(Math.Clamp(
                Math.Sqrt(Math.Max(0.0, 1.0 - (cosine * cosine)) / Math.Max(1e-12, 1.0 - (CosHalfAngle * CosHalfAngle))), 0.0, 1.0)),
        };

    public RgbSpectrum Radiance(WorldDirection direction) =>
        Contains(direction) ? Limb(Radius(direction)) : RgbSpectrum.Black;

    static RgbSpectrum Average(Func<UnitInterval, RgbSpectrum> limb) {
        (double r, double g, double b) = (0.0, 0.0, 0.0);
        for (int i = 0; i < MeanNodes; i++) {
            double radius = (i + 0.5) / MeanNodes;
            RgbSpectrum sample = limb(UnitInterval.Create(radius));
            (r, g, b) = (r + (sample.R * radius), g + (sample.G * radius), b + (sample.B * radius));
        }
        Span<double> channels = [2.0 * r / MeanNodes, 2.0 * g / MeanNodes, 2.0 * b / MeanNodes];
        return Finite.Spectrum(channels);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SkyModel {
    private SkyModel() { }

    public sealed record HosekWilkie(SkyCoefficients Coefficients, SolarCoefficients Solar) : SkyModel;
    public sealed record CieStandard(CieSkyType Type, RgbSpectrum Tint) : SkyModel;

    private const int TermGradationScale = 0, TermGradationExponent = 1, TermConstant = 2,
        TermForwardScatter = 3, TermForwardDecay = 4, TermRayleigh = 5,
        TermMieScale = 6, TermZenith = 7, TermMieDirectionality = 8, TermRadiance = 9;

    public static SkyModel Cie(CieSkyType type, Option<EmissionInput> chroma) =>
        new CieStandard(type, chroma
            .Map(static input => input.Radiance.RgbLinear.Triplet switch {
                var t => Finite.Spectrum([t.First, t.Second, t.Third]),
            })
            .IfNone(RgbSpectrum.White));

    public string WireKey => Switch(
        hosekWilkie: static _ => "hosek-wilkie",
        cieStandard: static c => c.Type.WireKey);

    public SolarDisc Disc(WorldDirection sun, SkyAtmosphere atmosphere) => Switch(
        state: (Sun: sun.Normalize(), Atmosphere: atmosphere),
        hosekWilkie: static (s, h) => new SolarDisc(s.Sun, s.Atmosphere.HalfAngleCosine, s.Atmosphere.SolidAngle,
            radius => Limb(h.Solar, s.Sun, s.Atmosphere, radius)),
        cieStandard: static (s, c) => Circumsolar(c, s.Sun, s.Atmosphere) switch {
            var beam => new SolarDisc(s.Sun, s.Atmosphere.HalfAngleCosine, s.Atmosphere.SolidAngle, _ => beam),
        });

    internal RgbSpectrum Radiance(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) =>
        direction.CosZenith >= 0.0
            ? Sky(direction, sun, atmosphere)
            : Ground(sun, atmosphere);

    RgbSpectrum Sky(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) => Switch(
        state: (Direction: direction, Sun: sun, Atmosphere: atmosphere),
        hosekWilkie: static (s, h) => Fitted(h.Coefficients, s.Direction, s.Sun, s.Atmosphere),
        cieStandard: static (s, c) => Standard(c, s.Direction, s.Sun, s.Atmosphere));

    RgbSpectrum Ground(WorldDirection sun, SkyAtmosphere atmosphere) {
        WorldDirection horizon = Math.Abs(sun.X) + Math.Abs(sun.Y) > 1e-12
            ? new WorldDirection(sun.X, sun.Y, 0.0).Normalize()
            : new WorldDirection(1.0, 0.0, 0.0);
        return Sky(horizon, sun, atmosphere).Mul(atmosphere.GroundAlbedo);
    }

    static RgbSpectrum Fitted(SkyCoefficients fit, WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) {
        double cosTheta = Math.Max(0.0, direction.CosZenith);
        double cosGamma = Math.Clamp(direction.Dot(sun), -1.0, 1.0);
        double gamma = Math.Acos(cosGamma);
        double elevation = Elevation(sun);
        ReadOnlySpan<double> albedo = atmosphere.BandAlbedo.Span;
        Span<double> channels = stackalloc double[fit.Channels];
        for (int c = 0; c < fit.Channels; c++) {
            double Term(int slot) => fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, slot);
            double directionality = Term(TermMieDirectionality);
            double mie = (1.0 + (cosGamma * cosGamma))
                       / Math.Pow(Math.Max(1e-6, 1.0 + (directionality * directionality) - (2.0 * directionality * cosGamma)), 1.5);
            double expansion = (1.0 + (Term(TermGradationScale) * Math.Exp(Term(TermGradationExponent) / (cosTheta + 0.01))))
                             * (Term(TermConstant)
                                + (Term(TermForwardScatter) * Math.Exp(Term(TermForwardDecay) * gamma))
                                + (Term(TermRayleigh) * cosGamma * cosGamma)
                                + (Term(TermMieScale) * mie)
                                + (Term(TermZenith) * Math.Sqrt(cosTheta)));
            channels[c] = expansion * Term(TermRadiance);
        }
        return SkySpectrum.ToScene(channels, atmosphere.Exposure);
    }

    static RgbSpectrum Standard(CieStandard sky, WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) {
        double level = sky.Type.Relative(
            Math.Max(1e-4, direction.CosZenith),
            Math.Acos(Math.Clamp(direction.Dot(sun), -1.0, 1.0)),
            Math.Acos(Math.Clamp(sun.CosZenith, -1.0, 1.0))) * atmosphere.ZenithRadiance * atmosphere.Exposure;
        Span<double> channels = [level * sky.Tint.R, level * sky.Tint.G, level * sky.Tint.B];
        return Finite.Spectrum(channels);
    }

    static RgbSpectrum Circumsolar(CieStandard sky, WorldDirection sun, SkyAtmosphere atmosphere) =>
        Standard(sky, sun, sun, atmosphere);

    static RgbSpectrum Limb(SolarCoefficients solar, WorldDirection sun, SkyAtmosphere atmosphere, UnitInterval radius) {
        double elevation = Elevation(sun);
        Span<double> channels = stackalloc double[solar.Channels];
        for (int c = 0; c < solar.Channels; c++) {
            channels[c] = solar.Radiance(c, atmosphere.Turbidity, elevation, radius);
        }
        return SkySpectrum.ToScene(channels, atmosphere.Exposure);
    }

    static double Elevation(WorldDirection sun) =>
        Math.Cbrt(Math.Clamp(Math.Asin(Math.Clamp(sun.CosZenith, -1.0, 1.0)) / (Math.PI / 2.0), 0.0, 1.0));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolarFrame {
    public static Fin<WorldDirection> Of(
        double latitudeDegrees, double longitudeDegrees, Instant instant, Op key, double elevationM = 0.0) =>
        from site in key.AcceptValidated(SolarSite.Validate(
            latitudeDeg: latitudeDegrees,
            longitudeDeg: longitudeDegrees,
            timezone: Offset.Zero,
            elevationM: elevationM,
            out SolarSite? admitted), admitted)
        select Project(SolarPosition.At(site, instant));

    static WorldDirection Project(SunPosition sun) {
        double azimuth = sun.AzimuthDeg * (Math.PI / 180.0);
        double apparent = sun.AltitudeDeg * (Math.PI / 180.0);
        return new WorldDirection(Math.Cos(apparent) * Math.Cos(azimuth), -(Math.Cos(apparent) * Math.Sin(azimuth)), Math.Sin(apparent)).Normalize();
    }
}

internal static class SkySpectrum {
    public const int BandStartNm = 320, BandStepNm = 40, BandCount = 11;

    private static readonly (double R, double G, double B)[] Basis = [.. Enumerable.Range(0, BandCount).Select(static band =>
        new Unicolour(PortValue.SceneLinear, Reconstruct(band)).RgbLinear.Triplet switch {
            var t => (t.First, t.Second, t.Third),
        })];

    private static Spd Reconstruct(int band) =>
        Interpolate.CubicSplineMonotone(
            [.. Enumerable.Range(0, BandCount).Select(static i => (double)(BandStartNm + (i * BandStepNm)))],
            [.. Enumerable.Range(0, BandCount).Select(i => i == band ? 1.0 : 0.0)]) switch {
            var curve => new Spd(SpectralUpsample.SampleStart, SpectralUpsample.SampleStep,
                [.. Enumerable.Range(0, SpectralUpsample.SampleCount).Select(sample =>
                    Math.Max(0.0, curve.Interpolate(SpectralUpsample.SampleStart + (sample * SpectralUpsample.SampleStep))))]),
        };

    public static RgbSpectrum ToScene(ReadOnlySpan<double> bands, double exposure) {
        (double r, double g, double b) = (0.0, 0.0, 0.0);
        for (int band = 0; band < bands.Length; band++) {
            (double br, double bg, double bb) = Basis[band];
            (r, g, b) = (r + (bands[band] * br), g + (bands[band] * bg), b + (bands[band] * bb));
        }
        Span<double> channels = [r * exposure, g * exposure, b * exposure];
        return Finite.Spectrum(channels);
    }

    public static Fin<ReadOnlyMemory<double>> BandAlbedo(RgbSpectrum albedo, Op key) =>
        SpectralUpsample.ToCurve(albedo, key).Map(static curve =>
            (ReadOnlyMemory<double>)[.. Enumerable.Range(0, BandCount).Select(band =>
                Math.Clamp((BandStartNm + (band * BandStepNm) - SpectralUpsample.SampleStart) / (double)SpectralUpsample.SampleStep, 0.0, SpectralUpsample.SampleCount - 1.0) switch {
                    var position => (int)position switch {
                        var node when node >= SpectralUpsample.SampleCount - 1 => curve.Span[SpectralUpsample.SampleCount - 1],
                        var node => curve.Span[node] + ((curve.Span[node + 1] - curve.Span[node]) * (position - node)),
                    },
                })]);
}

internal static class Finite {
    public static bool All(ReadOnlySpan<double> values) {
        foreach (double value in values) { if (!double.IsFinite(value)) { return false; } }
        return true;
    }

    public static RgbSpectrum Spectrum(ReadOnlySpan<double> channels) =>
        All(channels)
            ? RgbSpectrum.Create(Math.Max(0.0, channels[0]), Math.Max(0.0, channels[1]), Math.Max(0.0, channels[2]))
            : RgbSpectrum.Black;
}

public static class SkyRender {
    public static Func<Vector3d, RgbSpectrum> Radiance(SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun) =>
        sun.Normalize() switch {
            var normalized => direction =>
                model.Radiance(new WorldDirection(direction.X, direction.Y, direction.Z).Normalize(), normalized, atmosphere),
        };
}
```

## [03]-[ENVIRONMENT_MAP]

- Owner: `EnvironmentMap` the admitted directional-radiance carrier over one `TexturePlane`; `MapLayout` the storage band; `Equirectangular`/`Cube`/`Octahedron` the three coordinate laws the rows bind.
- Entry: `public static Fin<EnvironmentMap> Of(TexturePlane plane, MapLayout layout, PhotometricQuantity quantity, double intensity, Enum unit, double rotation, Op key, Guid correlation)` and its already-admitted `EmissionEvidence` sibling shape admit a decoded plane against its layout's aspect law, its transfer band, its HDR depth gate, and its layer congruence, lifting ONE scene-linear sampler PER LAYER; `public Fin<EnvironmentMap> Project(MapLayout target, Dimension edge, RenderBudget budget, Op key)` is the ONE layout relation — equirect to cube faces, cube faces to equirect, either to octahedral — because a direction-indexed field admits an exact inverse and a direction-named sibling converter pair is the rejected split; `Stored(direction, lod)`, `Radiance(direction, lod)`, and `Texel(layer, x, y)` are the three reads.
- Law: STORED and WORLD are two frames on ONE field and the split is structural. `Stored` reads the plane as authored — no rotation, no intensity; `Radiance` un-applies the dome rotation and scales by intensity. Every PREFILTER product integrates `Stored`, so rotating or re-exposing a dome re-keys NOTHING: the SH vector, the specular level set, and the luminance guide are stored-frame blobs a rotation reads through rather than a policy baked into their bytes. Prefiltering over the world frame makes `EnvironmentLight.Rotation` a re-bake trigger while this owner's boundary law calls it read-time — that contradiction is what the split forecloses.
- Law: the equirect correspondence is FROZEN and single-sourced here — `u = 0.5 + atan2(d.Y, d.X) / 2π`, `v = acos(clamp(d.Z, −1, 1)) / π`, `v = 0` at `+Z`, `u` increasing counter-clockwise viewed from `+Z` — so the sky sweep, the prefilter, the CDF, and the `EnvironmentLight` lookup all address one mapping and a consumer re-deriving it forks the contract this owner exists to hold. `WorldDirection` is the PRODUCER-OWNED world carrier of that basis — the same `+Z`-up convention the `bsdf#SHADING_FRAME` `LocalVector<T>` tangent triple declares for its own frame, split into a DISTINCT type here so a tangent-frame vector cannot reach a dome read and the frame law needs no consumer-side prose; a Y-up runtime remaps the DIRECTION BASIS at its own read and never rewrites a plane.
- Law: the sampler lift is PER LAYER. `Raster/plane#TEXTURE_PYRAMID` `AsImage` carries one layer by construction, so a six-face cube admitted as one sampler refuses the bridge and leaves the layered arms declared capability that cannot run. Each layer extracts as its own scene-linear plane, folds its own Kaiser pyramid, and lifts one `TextureSource.Image`; the map HOLDS each pyramid beside its sampler — the bridge's levels window the pyramid's arenas, so the chain releases at the map's own `Dispose` and a lift-time dispose reads freed memory on the first tap.
- Growth: a new arrangement is one `MapLayout` row binding its layer count, `LayerLaw`, aspect, and the three coordinate delegates — the `Project` fold and the per-layer lift read all four off the row, so a dual-paraboloid or lat-long cube-cross lands as a row with zero new surface; a new admission gate is one predicate on `Of`.
- Boundary: `EnvironmentMap` NEVER decodes a container — `Raster/codec#RASTER_CODEC` sniffs the magic and produces the plane, so a Radiance `.hdr`, an OpenEXR half plane, and a synthesized sky reach ONE admission. `linear`, `pq`, and `hlg` are the ADMITTED transfers and this is the one corpus surface where the display-referred pair is legal; `srgb` and `raw` refuse, because an sRGB dome cannot carry a sun and a raw dome declares no light quantity. `Lift` lowers whatever admits to scene-linear ONCE through the arena's `Read` accessor, so no consumer re-applies a transfer.
- Boundary: rotation and intensity are READ-TIME values, so a re-oriented or re-exposed dome re-keys no blob and re-runs no prefilter. Intensity is ADMITTED evidence, never a bare multiplier: `Of` composes `Photometric.Admit` on the authored shape and re-seats an already-admitted `EmissionEvidence` on the `Project` shape, so a reprojection never double-coerces. Every read scales by `RadiometricSi`, and the dimensionless case admits as `PhotometricQuantity.Radiance` whose `Borrowed` coercion leaves `RadiometricSi == Measure.CanonicalValue` — one construction, no branch downstream. `MaterialFault` fails a wrong-aspect plane, a layer count contradicting the row, an integer-depth plane, a refused transfer, and an out-of-range read policy.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record EnvironmentMap(
    TexturePlane Plane, Seq<TexturePyramid> Pyramids, Seq<TextureSource.Image> Sources, MapLayout Layout,
    EmissionEvidence Intensity, double Rotation, Op Key) : IDisposable {

    static readonly Seq<PlaneTransfer> Admitted = Seq(PlaneTransfer.Linear, PlaneTransfer.Pq, PlaneTransfer.Hlg);

    public static Fin<EnvironmentMap> Of(
        TexturePlane plane, MapLayout layout, PhotometricQuantity quantity, double intensity, Enum unit,
        double rotation, Op key, Guid correlation) =>
        Photometric.Admit(quantity, intensity, unit, key, correlation).Bind(evidence => Of(plane, layout, evidence, rotation, key));

    public static Fin<EnvironmentMap> Of(TexturePlane plane, MapLayout layout, EmissionEvidence intensity, double rotation, Op key) =>
        from _ in guard(plane.Layers.Value == layout.Layers,
                new MaterialFault.Parameter(key, $"<environment-layer-count:{plane.Layers.Value}!={layout.Layers}>"))
        from __ in guard(layout.Law.Admits(plane.Layers.Value),
                new MaterialFault.Parameter(key, $"<environment-layer-law:{layout.Law.Key}:{plane.Layers.Value}>"))
        from ___ in guard(Math.Abs(((double)plane.Width.Value / plane.Height.Value) - layout.Aspect) < 1e-9,
                new MaterialFault.Parameter(key, $"<environment-aspect:{plane.Width.Value}x{plane.Height.Value}!={layout.Key}>"))
        from ____ in guard(plane.Format.Depth == ChannelDtype.Float16 || plane.Format.Depth == ChannelDtype.Float32,
                new MaterialFault.Parameter(key, $"<environment-not-hdr:{plane.Format.Key}>"))
        from _____ in guard(Admitted.Exists(t => t == plane.Transfer),
                new MaterialFault.Parameter(key, $"<environment-transfer:{plane.Transfer.Key}>"))
        from ______ in guard(double.IsFinite(intensity.RadiometricSi) && intensity.RadiometricSi >= 0.0
                          && double.IsFinite(rotation) && rotation is >= 0.0 and < (2.0 * Math.PI),
                new MaterialFault.Parameter(key, $"<environment-read-policy:{intensity.Measure.CanonicalUnit}:{intensity.RadiometricSi:R},{rotation:R}>"))
        from lifted in Lift(plane, key)
        select new EnvironmentMap(plane, lifted.Map(static pair => pair.Pyramid), lifted.Map(static pair => pair.Source),
            layout, intensity, rotation, key);

    static Fin<Seq<(TexturePyramid Pyramid, TextureSource.Image Source)>> Lift(TexturePlane plane, Op key) =>
        toSeq(Enumerable.Range(0, plane.Layers.Value))
            .Traverse(layer =>
                from face in Face(plane, layer, key)
                from pyramid in TexturePyramid.Of(face, MipPolicy.Kaiser, key)
                from source in pyramid.AsImage(key)
                select (Pyramid: pyramid, Source: source)).As()
            .Map(static pairs => pairs.Strict());

    static Fin<TexturePlane> Face(TexturePlane plane, int layer, Op key) =>
        TexturePlane.Of(PlaneFormat.Rgba32F, plane.Width, plane.Height, PlaneTransfer.Linear, AlphaMode.None, key)
            .Map(face => {
                using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(plane.Width.Value, AllocationMode.Clear);
                for (int row = 0; row < plane.Height.Value; row++) {
                    plane.ReadShade(row, layer, field.Span);
                    face.WriteShade(row, layer: 0, field.Span);
                }
                return face;
            });

    public RgbSpectrum Stored(WorldDirection direction, double lod) {
        WorldDirection local = direction.Normalize();
        (UnitInterval u, UnitInterval v) = Layout.Forward(local);
        return TextureUv.Sample(Sources[Layout.LayerOf(local)], new UvSample(u, v, Vector3d.Zero, Vector3d.ZAxis, lod), Layout.Sampler, Key)
            .Match(Succ: static f => f.IsFinite ? RgbSpectrum.Create(Math.Max(0.0, f.X), Math.Max(0.0, f.Y), Math.Max(0.0, f.Z))
                                                : RgbSpectrum.Black,
                   Fail: static _ => RgbSpectrum.Black);
    }

    public RgbSpectrum Radiance(WorldDirection direction, double lod) =>
        Stored(Rotated(direction.Normalize(), -Rotation), lod).Scale(Intensity.RadiometricSi);

    public RgbSpectrum Texel(int layer, int x, int y) =>
        Sources[layer].Levels[0].Span[y, x] is var texel && texel.IsFinite
            ? RgbSpectrum.Create(Math.Max(0.0, texel.X), Math.Max(0.0, texel.Y), Math.Max(0.0, texel.Z))
            : RgbSpectrum.Black;

    public Fin<EnvironmentMap> Project(MapLayout target, Dimension edge, RenderBudget budget, Op key) {
        if (target.Key == Layout.Key) { return Fin.Succ(this); }
        (Dimension width, Dimension height) = target.Extent(edge);
        return TexturePlane.Of(PlaneFormat.Rgba32F, width, height, PlaneTransfer.Linear, AlphaMode.None, key,
                    layers: Some(Dimension.Create(target.Layers)))
                .Bind(plane => budget
                    .Sweep(height.Value * target.Layers, new ProjectSweep(this, target, plane), key)
                    .Map(_ => plane)
                    .Rollback(plane))
                .Bind(plane => Of(plane, target, Intensity, Rotation, key)
                    .Rollback(plane));
    }

    public void Dispose() {
        Pyramids.Iter(static pyramid => pyramid.Dispose());
        Plane.Dispose();
    }

    internal static WorldDirection Rotated(WorldDirection d, double radians) {
        (double s, double c) = Math.SinCos(radians);
        return new WorldDirection((d.X * c) - (d.Y * s), (d.X * s) + (d.Y * c), d.Z);
    }

    readonly struct ProjectSweep(EnvironmentMap source, MapLayout target, TexturePlane plane) : IAction {
        public void Invoke(int stacked) {
            (int width, int height) = (plane.Width.Value, plane.Height.Value);
            (int layer, int y) = (stacked / height, stacked % height);
            using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(width, AllocationMode.Clear);
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            Span<ShadeVec4> lanes = field.Span;
            for (int x = 0; x < width; x++) {
                RgbSpectrum radiance = source.Stored(target.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer), lod: 0.0);
                lanes[x] = new ShadeVec4(radiance.R, radiance.G, radiance.B, 1.0);
            }
            plane.WriteShade(y, layer, lanes);
        }
    }
}

internal static class Equirectangular {
    public static (UnitInterval U, UnitInterval V) Of(WorldDirection d) {
        double u = 0.5 + (Math.Atan2(d.Y, d.X) / (2.0 * Math.PI));
        return (UnitInterval.Create(u - Math.Floor(u)), UnitInterval.Create(Math.Acos(Math.Clamp(d.Z, -1.0, 1.0)) / Math.PI));
    }

    public static WorldDirection Direction(UnitInterval u, UnitInterval v, int layer) {
        double phi = (u.Value - 0.5) * 2.0 * Math.PI, theta = v.Value * Math.PI, sinTheta = Math.Sin(theta);
        return new WorldDirection(sinTheta * Math.Cos(phi), sinTheta * Math.Sin(phi), Math.Cos(theta));
    }

    public static double Measure(WorldDirection d, int width, int height) =>
        Math.Sqrt(Math.Max(0.0, 1.0 - (d.Z * d.Z))) * (2.0 * Math.PI / width) * (Math.PI / height);
}

internal static class Cube {
    public static int Face(WorldDirection d) =>
        (Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z)) switch {
            var (ax, ay, az) when ax >= ay && ax >= az => d.X >= 0.0 ? 0 : 1,
            var (_, ay, az) when ay >= az => d.Y >= 0.0 ? 2 : 3,
            _ => d.Z >= 0.0 ? 4 : 5,
        };

    public static (UnitInterval U, UnitInterval V) Of(WorldDirection d) {
        (double major, double s, double t) = Axes(Face(d), d);
        return (UnitInterval.Create(Math.Clamp((s / major * 0.5) + 0.5, 0.0, 1.0)),
                UnitInterval.Create(Math.Clamp((t / major * -0.5) + 0.5, 0.0, 1.0)));
    }

    public static WorldDirection Direction(UnitInterval u, UnitInterval v, int layer) {
        (double s, double t) = ((u.Value * 2.0) - 1.0, 1.0 - (v.Value * 2.0));
        return (layer switch {
            0 => new WorldDirection(1.0, s, t),
            1 => new WorldDirection(-1.0, -s, t),
            2 => new WorldDirection(-s, 1.0, t),
            3 => new WorldDirection(s, -1.0, t),
            4 => new WorldDirection(s, t, 1.0),
            _ => new WorldDirection(s, -t, -1.0),
        }).Normalize();
    }

    public static double Measure(WorldDirection d, int width, int height) {
        double major = Math.Max(Math.Abs(d.X), Math.Max(Math.Abs(d.Y), Math.Abs(d.Z)));
        return 4.0 * major * major * major / (width * (double)height);
    }

    static (double Major, double S, double T) Axes(int face, WorldDirection d) => face switch {
        0 => (Math.Max(1e-9, d.X), d.Y, d.Z),
        1 => (Math.Max(1e-9, -d.X), -d.Y, d.Z),
        2 => (Math.Max(1e-9, d.Y), -d.X, d.Z),
        3 => (Math.Max(1e-9, -d.Y), d.X, d.Z),
        4 => (Math.Max(1e-9, d.Z), d.X, d.Y),
        _ => (Math.Max(1e-9, -d.Z), d.X, -d.Y),
    };
}

internal static class Octahedron {
    public static (UnitInterval U, UnitInterval V) Of(WorldDirection d) {
        double norm = Math.Max(1e-9, Math.Abs(d.X) + Math.Abs(d.Y) + Math.Abs(d.Z));
        (double px, double py) = (d.X / norm, d.Y / norm);
        (double fx, double fy) = d.Z >= 0.0
            ? (px, py)
            : ((1.0 - Math.Abs(py)) * Math.CopySign(1.0, px), (1.0 - Math.Abs(px)) * Math.CopySign(1.0, py));
        return (UnitInterval.Create(Math.Clamp((fx * 0.5) + 0.5, 0.0, 1.0)), UnitInterval.Create(Math.Clamp((fy * 0.5) + 0.5, 0.0, 1.0)));
    }

    public static WorldDirection Direction(UnitInterval u, UnitInterval v, int layer) {
        (double fx, double fy) = ((u.Value * 2.0) - 1.0, (v.Value * 2.0) - 1.0);
        double z = 1.0 - Math.Abs(fx) - Math.Abs(fy);
        return (z >= 0.0
            ? new WorldDirection(fx, fy, z)
            : new WorldDirection((1.0 - Math.Abs(fy)) * Math.CopySign(1.0, fx), (1.0 - Math.Abs(fx)) * Math.CopySign(1.0, fy), z)).Normalize();
    }

    public static double Measure(WorldDirection d, int width, int height) {
        double norm = Math.Abs(d.X) + Math.Abs(d.Y) + Math.Abs(d.Z);
        return 4.0 * norm * norm * norm / (width * (double)height);
    }
}
```

## [04]-[IBL_PREFILTER]

- Owner: `IblPrefilter` the reduction fold; `ShBand` the nine-row spherical-harmonic basis table; `Sh9` the twenty-seven-value irradiance carrier; `IblPolicy` the sampling-budget row; `IblProducts` the CPU product set and `IblProduct` the lane-product split over it; the kernel `Deterministic.Hammersley` the composed low-discrepancy draw.
- Entry: `public static Fin<IblProduct> Prefilter(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key, Option<PressDevice> device = default)` — ONE reduction producing every product, because the pyramid, the SH projection, and the CDF each sweep the same field and three entrypoints sweep it three times; governance rides the budget's own `Governance` column rather than a token-and-sink tail, so the page's longest operation is abortable and watchable with no signature widened, the MIP LADDER is the reported unit (the policy's own declared level count, each level a whole-dome sweep), and an abandoned run disposes every level it already integrated before failing `Errors.Cancelled`; `Sh9.Project(EnvironmentMap, RenderBudget, Op)` and `Sh9.Irradiance(WorldDirection)` are the projection and reconstruction halves of ONE correspondence on one owner; `IblProducts.SpecularLevel(UnitInterval)` maps a roughness onto the fractional mip the level set encodes.
- Law: every product integrates the map's STORED frame. Rotation and intensity apply at the `[05]` read, so a re-oriented or re-exposed dome reuses the same content-addressed blobs and a rotation is never a re-bake.
- Law: `ShBand` IS the FROZEN SH9 spelling — real orthonormal harmonics through `l = 2` in the right-handed `+Z`-up basis, band-major with RGB interleaved at `i·3 + c`, carrying the Lambertian convolution constants `Â₀ = π`, `Â₁ = 2π/3`, `Â₂ = π/4`. The normalization constants DERIVE from `sqrt((2l+1)/4π)` folded with each polynomial's own factor, so four expressions serve nine rows and the `Raster/gpu#WGSL_KERNEL` `irradianceSh` float literals are a DECLARED transcription of this primary (branch RULINGS: a WGSL twin transcribes its CPU owner's own members). `Sh9.Of` refuses a channel-major layout and any length other than twenty-seven.
- Law: `ShOracle.All` rosters two EVALUATED fixtures and `ShOracle.Prove` runs them against three admitted `Tolerance` bars. `L(ω) = 1` yields `sh_0 = √(4π)` with every other band zero and `E(n) = π`; `L(ω) = ω·ẑ` yields `sh_2 = √(4π/3)` with every other band zero and `E(+ẑ) = 2π/3` — both expectations DERIVED from their own analytic projections, so a Y-up transcription lands the axial energy at `sh_1`/`sh_3` and fails, and the reconstruction probe fails a wrong `Â` set the projection alone cannot see. The same proof sums each `MapLayout.SolidAngle` closed form to `4π` over its own grid, and it TRAVERSES both rosters on the result so a broken layout and a broken band each name themselves.
- Law: every GGX integral here reads the `bsdf#MICROFACET_KERNEL` kernel — `Microfacet<double>.SampleVisibleNormal` draws the half-vector, `Microfacet<double>.VisibleNormalPdf` supplies the density, `Microfacet<double>.MaskingShadowing` the Smith term — the generic kernel at this page's own instantiation — so the prefiltered dome and the shaded surface integrate the SAME distribution and a re-minted importance sampler is the deleted form. Sampling composes the kernel `Deterministic.Hammersley` equidistributed pair — the low-discrepancy member family the deterministic-draw owner carries BESIDE its splitmix64 stream, because splitmix64 clustering leaves visible prefilter noise at a bounded tap budget — so this page authors no sampling kernel of its own.
- Law: the specular tap reads the SOURCE mip whose solid angle matches the sample density. That term is the firefly suppression a bounded tap budget requires and is a declared column rather than a hidden clamp: a blown highlight spreads across the taps it covers instead of being clipped out of the integral.
- Law: the luminance guide resolves the DOME ALONE. The `[02]` synthesis keeps the disc out of the plane, so the guide never has to import-sample a source that spans one texel and out-shines its neighbours by four decades — the firefly no tap budget resolves and the quadrature error the SH would carry forever — and the `[05]` row prices the two arms against each other from the guide's own total and the disc's area-averaged radiance instead. An ingested HDRI with a baked sun carries that sun in its guide, which is exactly the arrangement the split is honest about: its disc is absent, its selection is zero, and the guide is the only structure there is.
- Law: every SOURCE-DOMAIN sweep partitions and reads by INDEX. `Sh9.Project` runs a commutative reduction, so each row accumulates its own band vector under the budget's governed `Sweep` and one fold sums them; the luminance guide's per-row conditional mass is likewise independent, and only the marginal prefix over row masses is sequential. This law forecloses a serial full-plane sweep beside partitioned siblings — at a four-thousand-texel dome the two reductions are the campaign's heaviest single-threaded folds.
- Growth: a new prefilter product is one column on `IblProducts` filled inside the one sweep; a new sampling budget is one `IblPolicy` column; a new execution lane is one `press#PRESS_PLAN` `PressBackend` row the policy already carries; a new mip ladder is a `MipPolicy` row on the pyramid owner. `BrdfLut` stays environment-INDEPENDENT and view-independent — a pure function of `(N·V, roughness)` — so it computes once per `IblPolicy` and a second environment reuses the same blob by content address.
- Boundary: prefiltering NEVER writes a file; `IblProducts` carries planes and the egress name grammar belongs to `Raster/set#TEXTURE_SET`. Plane bytes are always CPU-minted, so the GPU arm is an accelerator whose output is never content-addressed — STRUCTURALLY, not by rule: `IblProduct` splits `Minted` from `Preview` and `EnvironmentBlobs` is reachable only from the minted case. The preview omits the BRDF LUT and the luminance guide honestly rather than partially, since the LUT is environment-independent and the guide's marginal prefix is this page's declared sequential step, so neither has a kernel row to fill it. `IblPolicy` carries the `PressBackend` row and `Prefilter` takes the `Option<PressDevice>` its arm reads, so the lane is selected by data the plan already models; a degenerate all-black dome REFUSES the CDF rather than returning a flat table that samples uniformly while claiming importance.
- Boundary: a GPU prefilter arm writes the SAME equirect arrangement this fold does — an accelerator that changes the product's own layout is a second product, not a faster one. `Raster/gpu#WGSL_KERNEL` `prefilterSpecular` therefore inverts the frozen equirect correspondence per output texel and takes a plane extent, and `IblProducts.Specular` needs no arrangement column for a lane to fill it; the cube arrangement stays `equirectToCube`'s, the one kernel whose product IS a cube.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ShBand {
    private const double Quarter = 1.0 / (4.0 * Math.PI), Sixteenth = 1.0 / (16.0 * Math.PI);
    private static readonly double Band0 = Math.Sqrt(Quarter);
    private static readonly double Band1 = Math.Sqrt(3.0 * Quarter);
    private static readonly double Band2 = Math.Sqrt(15.0 * Quarter);

    public static readonly ShBand Sh0 = new(key: 0, l: 0, m:  0, constant: Band0,       basis: static d => 1.0,                       convolution: Math.PI);
    public static readonly ShBand Sh1 = new(key: 1, l: 1, m: -1, constant: Band1,       basis: static d => d.Y,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh2 = new(key: 2, l: 1, m:  0, constant: Band1,       basis: static d => d.Z,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh3 = new(key: 3, l: 1, m:  1, constant: Band1,       basis: static d => d.X,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh4 = new(key: 4, l: 2, m: -2, constant: Band2,       basis: static d => d.X * d.Y,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh5 = new(key: 5, l: 2, m: -1, constant: Band2,       basis: static d => d.Y * d.Z,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh6 = new(key: 6, l: 2, m:  0, constant: Math.Sqrt(5.0 * Sixteenth), basis: static d => (3.0 * d.Z * d.Z) - 1.0, convolution: Math.PI / 4.0);
    public static readonly ShBand Sh7 = new(key: 7, l: 2, m:  1, constant: Band2,       basis: static d => d.X * d.Z,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh8 = new(key: 8, l: 2, m:  2, constant: 0.5 * Band2, basis: static d => (d.X * d.X) - (d.Y * d.Y), convolution: Math.PI / 4.0);
    public int L { get; }
    public int M { get; }
    public double Constant { get; }
    public double Convolution { get; }

    [UseDelegateFromConstructor]
    public partial double Basis(WorldDirection direction);

    public double Evaluate(WorldDirection direction) => Constant * Basis(direction);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Sh9(ReadOnlyMemory<double> Bands) {
    public const int Slots = 27;

    public static Fin<Sh9> Of(ReadOnlyMemory<double> bands, Op key) =>
        bands.Length == Slots && Finite.All(bands.Span)
            ? Fin.Succ(new Sh9(bands))
            : new MaterialFault.Parameter(key, $"<sh9-layout:{bands.Length}!={Slots}>");

    public RgbSpectrum Radiant {
        get {
            ReadOnlySpan<double> bands = Bands.Span;
            double scale = 2.0 * Math.Sqrt(Math.PI);
            Span<double> channels = [bands[0] * scale, bands[1] * scale, bands[2] * scale];
            return Finite.Spectrum(channels);
        }
    }

    public RgbSpectrum Irradiance(WorldDirection normal) {
        ReadOnlySpan<double> bands = Bands.Span;
        WorldDirection n = normal.Normalize();
        (double r, double g, double b) = (0.0, 0.0, 0.0);
        foreach (ShBand band in ShBand.Items) {
            double weight = band.Convolution * band.Evaluate(n);
            (r, g, b) = (r + (weight * bands[band.Key * 3]), g + (weight * bands[(band.Key * 3) + 1]), b + (weight * bands[(band.Key * 3) + 2]));
        }
        Span<double> channels = [r, g, b];
        return Finite.Spectrum(channels);
    }

    public static Fin<Sh9> Project(EnvironmentMap map, RenderBudget budget, Op key) {
        (int w, int h, int layers) = (map.Plane.Width.Value, map.Plane.Height.Value, map.Plane.Layers.Value);
        double[] rows = new double[h * layers * Slots];
        return budget.Sweep(h * layers, new ProjectSweep(map, rows, w, h), key).Bind(_ => {
            double[] bands = new double[Slots];
            for (int row = 0; row < h * layers; row++) {
                for (int slot = 0; slot < Slots; slot++) { bands[slot] += rows[(row * Slots) + slot]; }
            }
            return Of(bands, key);
        });
    }

    readonly struct ProjectSweep(EnvironmentMap map, double[] rows, int width, int height) : IAction {
        public void Invoke(int stacked) {
            (int layer, int y) = (stacked / height, stacked % height);
            Span<double> lane = rows.AsSpan(stacked * Slots, Slots);
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            for (int x = 0; x < width; x++) {
                WorldDirection d = map.Layout.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer);
                double measure = map.Layout.SolidAngle(d, width, height);
                RgbSpectrum radiance = map.Texel(layer, x, y);
                foreach (ShBand band in ShBand.Items) {
                    double weight = band.Evaluate(d) * measure;
                    lane[band.Key * 3] += weight * radiance.R;
                    lane[(band.Key * 3) + 1] += weight * radiance.G;
                    lane[(band.Key * 3) + 2] += weight * radiance.B;
                }
            }
        }
    }
}

public readonly record struct IblPolicy(
    int SpecularTaps, int LutTaps, Dimension LutExtent, Dimension SpecularEdge, int Mips, bool ImportanceSampled, PressBackend Backend) {
    public static readonly IblPolicy Default = new(SpecularTaps: 1024, LutTaps: 512, LutExtent: Dimension.Create(256),
        SpecularEdge: Dimension.Create(256), Mips: 6, ImportanceSampled: true, Backend: PressBackend.Cpu);

    public double RoughnessAt(int mip) => Mips <= 1 ? 0.0 : (double)mip / (Mips - 1);
}

public sealed record LuminanceCdf(ReadOnlyMemory<double> Conditional, ReadOnlyMemory<double> Marginal, double Total, int Width, int Height) {
    public Fin<TexturePlane> Plane(Op key) =>
        TexturePlane.Of(PlaneFormat.R32F, Dimension.Create(Width), Dimension.Create(Height + 1), PlaneTransfer.Raw, AlphaMode.None, key)
            .Map(plane => {
                using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(Width, AllocationMode.Clear);
                for (int row = 0; row <= Height; row++) {
                    ReadOnlySpan<double> source = row < Height ? Conditional.Span.Slice(row * Width, Width) : Marginal.Span;
                    for (int x = 0; x < Width; x++) { field.Span[x] = new ShadeVec4(x < source.Length ? source[x] : 0.0, 0.0, 0.0, 1.0); }
                    plane.WriteShade(row, layer: 0, field.Span);
                }
                return plane;
            });

    public (int X, int Y) Draw(UnitInterval u0, UnitInterval u1) {
        int y = Locate(Marginal.Span[..Height], u1.Value * Total);
        ReadOnlySpan<double> row = Conditional.Span.Slice(y * Width, Width);
        return (Locate(row, u0.Value * row[Width - 1]), y);
    }

    public double Density(double luminance) => Total > 0.0 ? luminance / Total : 0.0;

    static int Locate(ReadOnlySpan<double> running, double target) {
        int lo = 0, hi = running.Length - 1;
        while (lo < hi) {
            int mid = lo + ((hi - lo) / 2);
            if (running[mid] < target) { lo = mid + 1; } else { hi = mid; }
        }
        return lo;
    }
}

public sealed record IblProducts(
    Sh9 Irradiance, Seq<TexturePlane> Specular, Seq<double> RoughnessPerMip, TexturePlane BrdfLut, TexturePyramid BrdfPyramid,
    TextureSource.Image BrdfSource, Option<LuminanceCdf> Cdf) : IDisposable {
    public void Dispose() { Specular.Iter(static level => level.Dispose()); BrdfPyramid.Dispose(); }

    public double SpecularLevel(UnitInterval roughness) =>
        RoughnessPerMip.Count <= 1 ? 0.0 : roughness.Value * (RoughnessPerMip.Count - 1);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IblProduct : IDisposable {
    private IblProduct() { }

    public sealed record Minted(IblProducts Products) : IblProduct;
    public sealed record Preview(Sh9 Irradiance, Seq<TexturePlane> Specular, Seq<double> RoughnessPerMip) : IblProduct;

    public void Dispose() => Switch(
        minted:  static m => m.Products.Dispose(),
        preview: static p => p.Specular.Iter(static level => level.Dispose()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class IblPrefilter {
    public static Fin<IblProduct> Prefilter(
        EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key, Option<PressDevice> device = default) =>
        (policy.Backend.ContentAuthoritative, device.Case) switch {
            (true, null) => Mint(map, policy, budget, key).Map(static products => (IblProduct)new IblProduct.Minted(products)),
            (true, _) => new MaterialFault.Parameter(key, $"<ibl-device-on-authoritative-backend:{policy.Backend.Key}>"),
            (false, PressDevice lease) => Accelerate(map, policy, lease, key),
            (false, _) => new MaterialFault.Parameter(key, $"<ibl-accelerator-without-device:{policy.Backend.Key}>"),
        };

    static Fin<IblProduct> Accelerate(EnvironmentMap map, IblPolicy policy, PressDevice device, Op key) =>
        from irradiance in device.Dispatch(WgslKernel.IrradianceSh, ShBinding(map), key)
            .Bind(readback => Sh9.Of(Widen(readback.Output), key))
        from specular in toSeq(Enumerable.Range(0, policy.Mips))
            .Fold(Fin.Succ(Seq<TexturePlane>()), (acc, mip) => acc.Bind(levels =>
                device.Dispatch(WgslKernel.PrefilterSpecular, LevelBinding(map, policy, mip), key)
                    .Bind(readback => Decode(policy, mip, readback.Output, key))
                    .Map(level => levels.Add(level))
                    .Rollback([.. levels])))
        select (IblProduct)new IblProduct.Preview(irradiance, specular.Strict(),
            toSeq(Enumerable.Range(0, policy.Mips).Select(policy.RoughnessAt)));

    static ReadOnlyMemory<double> Widen(ReadOnlyMemory<float> output) =>
        (ReadOnlyMemory<double>)[.. output.ToArray().Select(static lane => (double)lane)];

    static KernelBinding ShBinding(EnvironmentMap map) =>
        ((uint)Math.Max(1, ((map.Plane.Width.Value * map.Plane.Height.Value) + 63) / 64)) switch {
            var groups => new KernelBinding(
                Seq(KernelUniform.Empty.Extent(map.Plane.Width, map.Plane.Height).U32(groups).U32(0u).Block,
                    new KernelBuffer.Read(Flatten(map)),
                    new KernelBuffer.Write((int)groups * Sh9.Slots)),
                GroupsX: groups, GroupsY: 1u, GroupsZ: 1u),
        };

    static KernelBinding LevelBinding(EnvironmentMap map, IblPolicy policy, int mip) =>
        Extent(policy, mip) switch {
            var (width, height) => new KernelBinding(
                Seq(KernelUniform.Empty
                        .Extent(Dimension.Create(width), Dimension.Create(height))
                        .U32((uint)map.Plane.Width.Value)
                        .U32((uint)map.Plane.Height.Value)
                        .F32((float)policy.RoughnessAt(mip))
                        .U32(policy.SpecularTaps)
                        .U32(0u).U32(0u)
                        .Block,
                    new KernelBuffer.Read(Flatten(map)),
                    new KernelBuffer.Write(width * height * 4)),
                GroupsX: (uint)((width + 7) / 8), GroupsY: (uint)((height + 7) / 8), GroupsZ: 1u),
        };

    static (int Width, int Height) Extent(IblPolicy policy, int mip) =>
        Math.Max(1, policy.SpecularEdge.Value >> mip) switch { var edge => (edge * 2, edge) };

    static ReadOnlyMemory<float> Flatten(EnvironmentMap map) =>
        (ReadOnlyMemory<float>)[.. Enumerable.Range(0, map.Plane.Height.Value)
            .SelectMany(y => Enumerable.Range(0, map.Plane.Width.Value)
                .SelectMany(x => map.Texel(0, x, y) switch {
                    var texel => new[] { (float)texel.R, (float)texel.G, (float)texel.B, 1.0f },
                }))];

    static Fin<TexturePlane> Decode(IblPolicy policy, int mip, ReadOnlyMemory<float> output, Op key) =>
        Extent(policy, mip) switch {
            var (width, height) => TexturePlane.Of(PlaneFormat.Rgba32F, Dimension.Create(width), Dimension.Create(height),
                    PlaneTransfer.Linear, AlphaMode.None, key)
                .Map(plane => {
                    using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(width, AllocationMode.Clear);
                    ReadOnlySpan<float> lanes = output.Span;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            int lane = ((y * width) + x) * 4;
                            field.Span[x] = new ShadeVec4(lanes[lane], lanes[lane + 1], lanes[lane + 2], 1.0);
                        }
                        plane.WriteShade(y, layer: 0, field.Span);
                    }
                    return plane;
                }),
        };

    static Fin<IblProducts> Mint(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key) =>
        from irradiance in Sh9.Project(map, budget, key)
        from specular in Specular(map, policy, budget, key)
        from lut in BrdfLut(policy, budget, key)
        from lutPyramid in TexturePyramid.Of(lut, MipPolicy.None, key)
        from lutSource in lutPyramid.AsImage(key)
        from cdf in policy.ImportanceSampled ? Guide(map, budget, key).Map(Some) : Fin.Succ(Option<LuminanceCdf>.None)
        select new IblProducts(irradiance, specular, toSeq(Enumerable.Range(0, policy.Mips).Select(policy.RoughnessAt)), lut, lutPyramid, lutSource, cdf);

    static Fin<Seq<TexturePlane>> Specular(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key) =>
        toSeq(Enumerable.Range(0, policy.Mips))
            .Fold(Fin.Succ(Seq<TexturePlane>()), (acc, mip) => acc.Bind(levels =>
                budget.Opened(mip, policy.Mips).Match(
                    Some: abandoned => Fin.Fail<Seq<TexturePlane>>(abandoned).Rollback([.. levels]),
                    None: () => Level(map, policy, mip, budget, key)
                        .Map(level => levels.Add(level))
                        .Rollback([.. levels])))
            .Map(static levels => levels.Strict());

    static Fin<TexturePlane> Level(EnvironmentMap map, IblPolicy policy, int mip, RenderBudget budget, Op key) {
        int edge = Math.Max(1, policy.SpecularEdge.Value >> mip);
        double sourceSolidAngle = 4.0 * Math.PI / (map.Plane.Width.Value * map.Plane.Height.Value * map.Plane.Layers.Value);
        return TexturePlane.Of(PlaneFormat.Rgba32F, Dimension.Create(edge * 2), Dimension.Create(edge),
                PlaneTransfer.Linear, AlphaMode.None, key)
            .Bind(plane => budget
                .Sweep(edge, new SpecularSweep(map, policy, Microfacet<double>.AlphaOf(policy.RoughnessAt(mip)), sourceSolidAngle, plane), key)
                .Map(_ => plane)
                .Rollback(plane));
    }

    readonly struct SpecularSweep(EnvironmentMap map, IblPolicy policy, double alpha, double sourceSolidAngle, TexturePlane plane) : IAction {
        public void Invoke(int y) {
            (int width, int height) = (plane.Width.Value, plane.Height.Value);
            using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(width, AllocationMode.Clear);
            Span<ShadeVec4> lanes = field.Span;
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            for (int x = 0; x < width; x++) {
                WorldDirection n = MapLayout.Equirect.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer: 0);
                (double r, double g, double b, double weight) = (0.0, 0.0, 0.0, 0.0);
                for (int i = 0; i < policy.SpecularTaps; i++) {
                    (double u0, double u1) = Deterministic.Hammersley(i, policy.SpecularTaps);
                    LocalVector<double> half = Microfacet<double>.SampleVisibleNormal(LocalVector<double>.Normal, alpha, alpha, u0, u1);
                    LocalVector<double> light = half.Scale(2.0 * half.CosTheta).Add(LocalVector<double>.Normal.Scale(-1.0)).Normalize();
                    if (light.CosTheta <= 0.0) { continue; }
                    double pdf = Math.Max(1e-6, Microfacet<double>.VisibleNormalPdf(LocalVector<double>.Normal, half, alpha, alpha) / (4.0 * Math.Abs(half.CosTheta)));
                    double lod = 0.5 * Math.Log2(Math.Max(1e-12, 1.0 / (policy.SpecularTaps * pdf) / sourceSolidAngle));
                    RgbSpectrum radiance = map.Stored(n.Oriented(light.X, light.Y, light.Z), Math.Max(0.0, lod));
                    (r, g, b, weight) = (r + (radiance.R * light.CosTheta), g + (radiance.G * light.CosTheta),
                                         b + (radiance.B * light.CosTheta), weight + light.CosTheta);
                }
                double norm = weight > 0.0 ? 1.0 / weight : 0.0;
                lanes[x] = new ShadeVec4(r * norm, g * norm, b * norm, 1.0);
            }
            plane.WriteShade(y, layer: 0, lanes);
        }
    }

    static Fin<TexturePlane> BrdfLut(IblPolicy policy, RenderBudget budget, Op key) =>
        TexturePlane.Of(PlaneFormat.Rg16, policy.LutExtent, policy.LutExtent, PlaneTransfer.Raw, AlphaMode.None, key)
            .Bind(plane => budget
                .Sweep(policy.LutExtent.Value, new LutSweep(policy, plane), key)
                .Map(_ => plane)
                .Rollback(plane));

    readonly struct LutSweep(IblPolicy policy, TexturePlane plane) : IAction {
        public void Invoke(int y) {
            int extent = plane.Width.Value;
            using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(extent, AllocationMode.Clear);
            Span<ShadeVec4> lanes = field.Span;
            double alpha = Microfacet<double>.AlphaOf((y + 0.5) / extent);
            for (int x = 0; x < extent; x++) {
                double cosView = Math.Max(1e-3, (x + 0.5) / extent);
                LocalVector<double> view = new(Math.Sqrt(Math.Max(0.0, 1.0 - (cosView * cosView))), 0.0, cosView);
                (double scale, double bias) = (0.0, 0.0);
                for (int i = 0; i < policy.LutTaps; i++) {
                    (double u0, double u1) = Deterministic.Hammersley(i, policy.LutTaps);
                    LocalVector<double> half = Microfacet<double>.SampleVisibleNormal(view, alpha, alpha, u0, u1);
                    LocalVector<double> light = half.Scale(2.0 * view.Dot(half)).Add(view.Scale(-1.0)).Normalize();
                    if (light.CosTheta <= 0.0) { continue; }
                    double visibility = Microfacet<double>.MaskingShadowing(view, light, alpha, alpha)
                                      / Math.Max(1e-6, Microfacet<double>.Masking(view, alpha, alpha));
                    double fresnel = Math.Pow(1.0 - Math.Abs(view.Dot(half)), 5.0);
                    (scale, bias) = (scale + ((1.0 - fresnel) * visibility), bias + (fresnel * visibility));
                }
                lanes[x] = new ShadeVec4(scale / policy.LutTaps, bias / policy.LutTaps, 0.0, 1.0);
            }
            plane.WriteShade(y, layer: 0, lanes);
        }
    }

    static Fin<LuminanceCdf> Guide(EnvironmentMap map, RenderBudget budget, Op key) =>
        map.Layout.Key == MapLayout.Equirect.Key
            ? Accumulate(map, budget, key)
            : map.Project(MapLayout.Equirect, map.Plane.Height, budget, key).Bind(projected => {
                  using (projected) { return Accumulate(projected, budget, key); }
              });

    static Fin<LuminanceCdf> Accumulate(EnvironmentMap map, RenderBudget budget, Op key) {
        (int w, int h) = (map.Plane.Width.Value, map.Plane.Height.Value);
        double[] conditional = new double[h * w], marginal = new double[h];
        return budget.Sweep(h, new GuideSweep(map, conditional, marginal, w, h), key).Bind(_ => {
            double total = 0.0;
            for (int y = 0; y < h; y++) { total += marginal[y]; marginal[y] = total; }
            return total > 0.0
                ? Fin.Succ(new LuminanceCdf(conditional, marginal, total, w, h))
                : new MaterialFault.Parameter(key, "<environment-zero-luminance>");
        });
    }

    readonly struct GuideSweep(EnvironmentMap map, double[] conditional, double[] marginal, int width, int height) : IAction {
        public void Invoke(int y) {
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            double mass = 0.0;
            for (int x = 0; x < width; x++) {
                WorldDirection d = map.Layout.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer: 0);
                mass += map.Texel(0, x, y).Luminance * map.Layout.SolidAngle(d, width, height);
                conditional[(y * width) + x] = mass;
            }
            marginal[y] = mass;
        }
    }
}

public sealed record ShOracle(string Name, Func<WorldDirection, double> Radiance, int Band, double Expected, double IrradianceAtZenith) {
    const double BandBar = 1e-6, IrradianceBar = 1e-5, MeasureBar = 1e-4;
    public const int ProofRows = 2048;

    static Fin<(Tolerance Band, Tolerance Irradiance, Tolerance Measure)> Bars(Op key) =>
        (Tolerance.Of(ToleranceLane.Spectral, BandBar, key).ToValidation(),
         Tolerance.Of(ToleranceLane.Irradiance, IrradianceBar, key).ToValidation(),
         Tolerance.Of(ToleranceLane.Conservation, MeasureBar, key).ToValidation())
            .Apply(static (band, irradiance, measure) => (Band: band, Irradiance: irradiance, Measure: measure))
            .As().ToFin();

    public static readonly Seq<ShOracle> All = Seq(
        new ShOracle("uniform", static _ => 1.0, Band: 0, Expected: Math.Sqrt(4.0 * Math.PI), IrradianceAtZenith: Math.PI),
        new ShOracle("axial-cosine", static d => d.Z, Band: 2, Expected: Math.Sqrt(4.0 * Math.PI / 3.0), IrradianceAtZenith: 2.0 * Math.PI / 3.0));

    public static Fin<Unit> Prove(Op key) =>
        from bars in Bars(key)
        from measures in toSeq(MapLayout.Items).Traverse(layout =>
            MeasureSum(layout, ProofRows) switch {
                var sum when Math.Abs(sum - (4.0 * Math.PI)) <= bars.Measure.Value => Fin.Succ(unit),
                var sum => Fin.Fail<Unit>(new MaterialFault.Parameter(key, $"<sh-oracle-measure:{layout.Key}:{sum:R}>")),
            }).As()
        from bands in All.Traverse(row => row.Project(bars, key)).As()
        select unit;

    Fin<Unit> Project((Tolerance Band, Tolerance Irradiance, Tolerance Measure) bars, Op key) {
        (int width, int height) = (ProofRows * 2, ProofRows);
        Span<double> bands = stackalloc double[9];
        for (int y = 0; y < height; y++) {
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            for (int x = 0; x < width; x++) {
                WorldDirection d = MapLayout.Equirect.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer: 0);
                double weighted = Radiance(d) * MapLayout.Equirect.SolidAngle(d, width, height);
                foreach (ShBand band in ShBand.Items) { bands[band.Key] += band.Evaluate(d) * weighted; }
            }
        }
        for (int slot = 0; slot < 9; slot++) {
            double expected = slot == Band ? Expected : 0.0;
            if (Math.Abs(bands[slot] - expected) > bars.Band.Value) {
                return new MaterialFault.Parameter(key, $"<sh-oracle-band:{Name}:{slot}:{bands[slot]:R}>");
            }
        }
        double[] interleaved = new double[Sh9.Slots];
        for (int slot = 0; slot < 9; slot++) {
            (interleaved[slot * 3], interleaved[(slot * 3) + 1], interleaved[(slot * 3) + 2]) = (bands[slot], bands[slot], bands[slot]);
        }
        return Sh9.Of(interleaved, key).Bind(sh =>
            Math.Abs(sh.Irradiance(WorldDirection.Zenith).R - IrradianceAtZenith) <= bars.Irradiance.Value
                ? Fin.Succ(unit)
                : (Fin<Unit>)new MaterialFault.Parameter(key, $"<sh-oracle-irradiance:{Name}:{sh.Irradiance(WorldDirection.Zenith).R:R}>"));
    }

    static double MeasureSum(MapLayout layout, int edge) {
        (Dimension width, Dimension height) = layout.Extent(Dimension.Create(edge));
        double sum = 0.0;
        for (int layer = 0; layer < layout.Layers; layer++) {
            for (int y = 0; y < height.Value; y++) {
                UnitInterval v = UnitInterval.Create((y + 0.5) / height.Value);
                for (int x = 0; x < width.Value; x++) {
                    WorldDirection d = layout.Inverse(UnitInterval.Create((x + 0.5) / width.Value), v, layer);
                    sum += layout.SolidAngle(d, width.Value, height.Value);
                }
            }
        }
        return sum;
    }
}
```

## [05]-[ENVIRONMENT_LIGHT]

- Owner: `EnvironmentLight` the resolved row the render boundary consumes, its admission gates, and the reads it publishes; `SkySource` the synthesized-dome provenance; `EnvironmentSample` `[Union]` (`Dome` · `Sun`) the drawn direction with its arm.
- Entry: `public static Fin<EnvironmentLight> Of(string lightKey, EnvironmentMap map, IblProducts products, EnvironmentBlobs blobs, Option<SkySource> sky, Op key)` admits the resolved row once and resolves the dome's `SolarDisc` from that provenance; `Radiance(WorldDirection)`, `Irradiance(WorldDirection)`, `Sample(u0, u1)`, `Pdf(WorldDirection)`, `SpecularLevel(UnitInterval)`, `SplitSum(cosView, roughness)`, and the `Sun` disc row are the reads a path-trace integrator and a raster shading pass share — every direction-typed read takes the producer's own `WorldDirection`, so a tangent-frame query is a compile error rather than a frame-law violation.
- Law: the SUN is the row's own arm. `Radiance` composes the dome field with the disc so a camera ray through the sun is never black; `Sample` splits between the guided dome and a uniform cap draw on a power ratio DERIVED from the SH band-zero integral and the disc's own area-averaged radiance; every returned `Pdf` is the combined balance density, so an integrator MIS-weights the two arms and its own BSDF draw against one number and never learns the split exists. `SunSelection` is reachable for a readout and decides nothing a caller passes.
- Law: the products are STORED-frame and every read applies the dome's rotation and intensity HERE. `Irradiance` un-rotates the queried normal before reconstructing the SH; the guided draw rotates the sampled direction into world; the density reads the STORED luminance the guide's own total was built from, so a re-exposed dome does not skew a multiple-importance weight by its own intensity factor. One read policy, applied at one altitude, over blobs no policy edit re-keys.
- Output: the row supplies the generated `Set.Ibl` projection at `interchange#TEXTURE_EGRESS` — stored product references, SH bands, roughness ladder, and read-time intensity/rotation cross there without a second environment message. `SkyModelKey` carries the generated set's optional `source` for a synthesized dome and stays absent for an ingested HDRI; `CoefficientKey`, `SolarKey`, the authored intensity evidence, and the source transfer remain domain and analytics facts because the corpus carries no such columns. A revised Hosek-Wilkie fit still re-keys the light through those domain digests, while the peer reads only the resolved stored products and policy the generated document proves.
- Boundary: `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` `LightSource.Environment` carries THIS row as its dome VALUE over the `[BOUNDARY]` contract — the render arm answers directional radiance, importance draw, SH irradiance, specular level, split-sum, and the SUN DISC (direction, cap, and radiance profile) on the owner that prefiltered the map, while Materials keeps the whole mapping, sampling, and prefilter algebra and the consumer re-derives no equirect correspondence, SH band order, roughness ladder, or solar geometry. `Sample` returns direction, radiance, combined density, and arm TOGETHER so a multiple-importance-sampling integrator balances with no second query, and an absent CDF answers the uniform-dome density as a declared degradation the row states rather than a silent fallback.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EnvironmentBlobs(ContentAddress Equirect, ContentAddress Specular, ContentAddress BrdfLut, Option<ContentAddress> LuminanceCdf);

public readonly record struct SkySource(SkyModel Model, SkyAtmosphere Atmosphere, WorldDirection Sun);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnvironmentSample {
    private EnvironmentSample(WorldDirection direction, RgbSpectrum radiance, double pdf) =>
        (Direction, Radiance, Pdf) = (direction, radiance, pdf);

    public WorldDirection Direction { get; }
    public RgbSpectrum Radiance { get; }
    public double Pdf { get; }

    public sealed record Dome(WorldDirection Direction, RgbSpectrum Radiance, double Pdf)
        : EnvironmentSample(Direction, Radiance, Pdf);
    public sealed record Sun(WorldDirection Direction, RgbSpectrum Radiance, double Pdf, UnitInterval DiscRadius)
        : EnvironmentSample(Direction, Radiance, Pdf);
}

public sealed record EnvironmentLight(
    string LightKey, EnvironmentMap Map, IblProducts Products, EnvironmentBlobs Blobs,
    string SkyModelKey, Option<ContentAddress> CoefficientKey, Option<ContentAddress> SolarKey, Option<SolarDisc> Sun) {

    public static Fin<EnvironmentLight> Of(
        string lightKey, EnvironmentMap map, IblProducts products, EnvironmentBlobs blobs, Option<SkySource> sky, Op key) =>
        from _ in guard(!string.IsNullOrWhiteSpace(lightKey), new MaterialFault.Parameter(key, "<environment-light-key-blank>"))
        from _layout in guard(map.Layout.Key == MapLayout.Equirect.Key,
                new MaterialFault.Parameter(key, $"<environment-light-layout:{map.Layout.Key}>"))
        from __ in guard(products.RoughnessPerMip.Count == products.Specular.Count && products.Specular.Count > 0,
                new MaterialFault.Parameter(key, $"<environment-level-ladder:{products.Specular.Count}!={products.RoughnessPerMip.Count}>"))
        from ___ in guard(products.RoughnessPerMip.Zip(products.RoughnessPerMip.Tail).ForAll(static pair => pair.First <= pair.Second),
                new MaterialFault.Parameter(key, "<environment-roughness-ladder-unordered>"))
        from ____ in guard(products.Cdf.Map(static cdf => cdf.Total > 0.0).IfNone(true),
                new MaterialFault.Parameter(key, "<environment-guide-zero-mass>"))
        select new EnvironmentLight(lightKey, map, products, blobs,
            sky.Map(static s => s.Model.WireKey).IfNone(string.Empty),
            sky.Bind(static s => s.Model is SkyModel.HosekWilkie fitted ? Some(fitted.Coefficients.Key) : None),
            sky.Bind(static s => s.Model is SkyModel.HosekWilkie fitted ? Some(fitted.Solar.Key) : None),
            sky.Map(static s => s.Model.Disc(s.Sun, s.Atmosphere)));

    public RgbSpectrum Radiance(WorldDirection direction) =>
        Map.Radiance(direction, lod: 0.0).Add(SunRadiance(direction.Normalize()));

    RgbSpectrum SunRadiance(WorldDirection direction) =>
        Sun.Map(disc => disc.Radiance(direction).Scale(Map.Intensity.RadiometricSi)).IfNone(RgbSpectrum.Black);

    public double SpecularLevel(UnitInterval roughness) => Products.SpecularLevel(roughness);

    public RgbSpectrum Irradiance(WorldDirection normal) =>
        Products.Irradiance.Irradiance(EnvironmentMap.Rotated(normal.Normalize(), -Map.Rotation)).Scale(Map.Intensity.RadiometricSi);

    public (double Scale, double Bias) SplitSum(UnitInterval cosView, UnitInterval roughness) =>
        TextureUv.Sample(Products.BrdfSource, new UvSample(cosView, roughness, Vector3d.Zero, Vector3d.ZAxis, 0.0), LutSampler, Map.Key)
            .Match(Succ: static f => f.IsFinite ? (f.X, f.Y) : (0.0, 0.0), Fail: static _ => (0.0, 0.0));

    static readonly SamplerState LutSampler = new(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Bilinear, UvFrame.Identity);

    public double SunSelection =>
        (Products.Irradiance.Radiant.Luminance, SunPower) switch {
            var (dome, sun) => sun + dome > 0.0 ? sun / (sun + dome) : 0.0,
        };

    double SunPower =>
        Sun.Map(static disc => disc.Mean.Luminance * disc.SolidAngle).IfNone(0.0)
        * Map.Intensity.RadiometricSi;

    public EnvironmentSample Sample(UnitInterval u0, UnitInterval u1) =>
        (Sun.Case, SunSelection) switch {
            (SolarDisc disc, > 0.0 and var p) when u0.Value < p =>
                Solar(disc, UnitInterval.Create(Math.Min(u0.Value / p, 1.0 - 1e-12)), u1),
            (SolarDisc _, > 0.0 and var p) =>
                Dome(UnitInterval.Create(Math.Min((u0.Value - p) / (1.0 - p), 1.0 - 1e-12)), u1),
            _ => Dome(u0, u1),
        };

    public double Pdf(WorldDirection direction) =>
        direction.Normalize() switch {
            var d => SunSelection switch {
                var p => (p * SunDensity(d)) + ((1.0 - p) * DomeDensity(d)),
            },
        };

    double SunDensity(WorldDirection direction) =>
        Sun.Map(disc => disc.Contains(direction) && disc.SolidAngle > 0.0 ? 1.0 / disc.SolidAngle : 0.0).IfNone(0.0);

    double DomeDensity(WorldDirection direction) =>
        Products.Cdf
            .Map(guide => guide.Density(Map.Stored(EnvironmentMap.Rotated(direction, -Map.Rotation), lod: 0.0).Luminance))
            .IfNone(1.0 / (4.0 * Math.PI));

    EnvironmentSample Solar(SolarDisc disc, UnitInterval u0, UnitInterval u1) {
        double cosTheta = 1.0 - (u0.Value * (1.0 - disc.CosHalfAngle));
        double sinTheta = Math.Sqrt(Math.Max(0.0, 1.0 - (cosTheta * cosTheta))), phi = 2.0 * Math.PI * u1.Value;
        WorldDirection direction = disc.Direction.Oriented(sinTheta * Math.Cos(phi), sinTheta * Math.Sin(phi), cosTheta);
        UnitInterval radius = disc.Radius(direction);
        return new EnvironmentSample.Sun(
            direction, disc.Limb(radius).Scale(Map.Intensity.RadiometricSi), Pdf(direction), radius);
    }

    EnvironmentSample Dome(UnitInterval u0, UnitInterval u1) =>
        Products.Cdf
            .Map(guide => Guided(guide, u0, u1))
            .IfNone(() => UniformDirection(u0, u1) switch {
                var d => (EnvironmentSample)new EnvironmentSample.Dome(d, Map.Radiance(d, lod: 0.0), Pdf(d)),
            });

    EnvironmentSample Guided(LuminanceCdf guide, UnitInterval u0, UnitInterval u1) {
        (int x, int y) = guide.Draw(u0, u1);
        WorldDirection local = MapLayout.Equirect.Inverse(
            UnitInterval.Create((x + 0.5) / guide.Width), UnitInterval.Create((y + 0.5) / guide.Height), layer: 0);
        WorldDirection world = EnvironmentMap.Rotated(local, Map.Rotation);
        return new EnvironmentSample.Dome(
            world, Map.Stored(local, lod: 0.0).Scale(Map.Intensity.RadiometricSi), Pdf(world));
    }

    static WorldDirection UniformDirection(UnitInterval u0, UnitInterval u1) {
        double z = 1.0 - (2.0 * u1.Value), r = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z))), phi = 2.0 * Math.PI * u0.Value;
        return new WorldDirection(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }
}
```

## [06]-[RESEARCH]

(none)
