# [MATERIALS_ENVIRONMENT]

THE SKY, ENVIRONMENT-MAP, AND IMAGE-BASED-LIGHTING OWNER. One `SkyModel` `[Union]` synthesizes a scene-linear radiance field from an analytic daylight model — the Hosek-Wilkie fitted-coefficient asset over its solar-elevation Bézier lattice and the ISO 15469 fifteen-type CIE standard sky over one gradation × indicatrix algebra — one `EnvironmentMap` admits any equirect, cube-face, or octahedral layout under the frozen `+Z`-up correspondence and carries the whole layout relation in one `Project` fold, one `IblPrefilter.Prefilter` reduces an admitted map to the `IblProducts` receipt every renderer consumes (SH9 irradiance, the GGX roughness-ordered specular level set, split-sum BRDF LUT, marginal-conditional luminance CDF), and one `EnvironmentLight` record is the row `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` resolves over the declared `[BOUNDARY]` seam — directional radiance, importance sample, SH irradiance, specular level, and split-sum read all answering on that one owner so no consumer re-derives the mapping. `SkyModel` admits a sky variation as one case and `CieSkyType` as one ROW, `MapLayout` a storage arrangement as one ROW, and `IblProducts` a prefilter product as one COLUMN — never a per-model sky type, a direction-named converter pair, or a second SH spelling. Every owner here composes the `bsdf#MICROFACET_KERNEL` `Microfacet` VNDF sampler and Smith masking for every GGX integral (the prefiltered dome and the shaded surface integrate the SAME distribution), the `bsdf#SHADING_FRAME` `LocalVector`/`RgbSpectrum`/`MaterialFault` band-2450 rail, the `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg working space, the `photometric#PHOTOMETRIC` unit gate for an authored zenith luminance, the `Raster/plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid` typed-texel arena with its `Read`/`Write` row rails and `AsImage` sampler bridge, the `texture#TEXTURE_UV` sampler for every filtered read, the seam `Rasm.Element` `ContentAddress` for every blob key, the kernel `Dimension`/`UnitInterval`/`Op` atoms with `CommunityToolkit.HighPerformance` struct-action partitioning, and NodaTime `Instant` for solar position — re-minting no plane, no colour space, no fault, and no hash. Solar geometry composes the kernel `Rasm/Numerics/calculus#SOLAR_EPHEMERIS` almanac and projects HERE into the frozen frame, so no host sun object and no second ephemeris crosses the host-neutral boundary; the container decode of an ingested HDRI is `Raster/codec#RASTER_CODEC`, this owner consuming the decoded plane alone.

## [01]-[INDEX]

- [02]-[SKY_MODEL]: `CieGradation`/`CieIndicatrix`/`CieSkyType` close the standard-sky algebra, `SkyCoefficients` carries the content-keyed fitted asset over its solar-elevation Bézier lattice, `SolarPosition` projects the kernel almanac's geodetic sun into the frozen frame, `SkyModel` `[Union]` states the radiance law with its ground hemisphere, and `SkyRender.Render` runs the one layout-parameterized synthesis.
- [03]-[ENVIRONMENT_MAP]: `MapLayout` bands storage with its per-row coordinate law, `Equirectangular` freezes the correspondence, `EnvironmentMap.Of` admits with its per-layer sampler lift, `Stored`/`Radiance` answer the stored-frame and world-frame reads, and `Project` carries the one layout relation in both directions.
- [04]-[IBL_PREFILTER]: `ShBand` tables the nine-row basis, `Sh9` pairs projection with irradiance reconstruction, `IblPrefilter` integrates the GGX specular level set, the split-sum BRDF LUT, and the luminance CDF over the kernel `Deterministic.Hammersley` draw, and `IblProducts` carries the receipt.
- [05]-[ENVIRONMENT_LIGHT]: `EnvironmentLight` resolves the row the render seam consumes, gates its own admission, and publishes the six reads an integrator and a raster shading pass share.
- [06]-[RESEARCH]: open epistemic debt with its verification route.

## [02]-[SKY_MODEL]

- Owner: `WorldDirection` the page-owned `+Z`-up WORLD direction carrier every dome surface speaks (structurally distinct from the `bsdf#SHADING_FRAME` tangent `LocalVector`); `SkyModel` `[Union]` (`HosekWilkie` · `CieStandard`); `SkyCoefficients` the content-keyed fitted-coefficient asset; `CieSkyType`/`CieGradation`/`CieIndicatrix` `[SmartEnum<int>]` bands; `SolarPosition` the frame adapter over the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac; `SkyAtmosphere` the turbidity, ground-albedo, zenith-level, and exposure row; `SkyRender` the synthesis fold.
- Cases: sky {`HosekWilkie` (the fitted anisotropic-Mie daylight model over a `SkyCoefficients` asset), `CieStandard` (the ISO 15469 relative-luminance distribution over a `CieSkyType` row)}; gradation {`I`…`VI`}; indicatrix {`One`…`Six`}; sky-type {`Type01`…`Type15`, each binding one gradation and one indicatrix — `Type01` the CIE Overcast Sky, `Type12` the CIE Standard Clear Sky}.
- Entry: `public static Fin<TexturePlane> Render(SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun, MapLayout layout, Dimension edge, RenderBudget budget, Op key)` is the ONE synthesis fold — the LAYOUT ROW supplies the extent, the layer count, and the coordinate law, so a cube-face sky and an equirect sky are one body and an extent contradicting the arrangement is unrepresentable rather than caught by a parity gate; `SolarPosition.Of(latitudeDegrees, longitudeDegrees, instant, key)` is the ONE sun-direction entry, so a caller holding a measured direction passes it and a caller holding a site and a clock resolves it here. `MaterialFault` rails a sub-unit or super-decade turbidity, a negative zenith level, a non-positive exposure, an out-of-range site, and a non-finite radiance.
- Law: radiance covers the WHOLE sphere. Each model distributes its own radiance over the upper hemisphere; the lower hemisphere is the GROUND — `GroundAlbedo` times the model's horizon radiance, evaluated once per texel through the same case — so a synthesized dome carries a real bounce rather than the mirrored bright band a clamped zenith cosine produces below the horizon. `GroundAlbedo` reaches that ground term as the same `RgbSpectrum` the Hosek-Wilkie fit consumes as its albedo axis, so one authored value drives both the sky's own inter-reflection and the dome's lower half.
- Law: the per-texel sweeps on this page are its `[EXPRESSION_SPINE]` kernel exemption — a `readonly struct` `IAction` row writes into the plane owner's `Write` rail by row index, the carve-out `texture#TEXTURE_UV` `ProceduralNoise` and `acquisition#ACQUISITION` `SolveGgx` also name; every other operation is expression-bodied and rail-threaded. Radiance leaves a model in scene-linear AP1 channels at `PlaneTransfer.Linear`, so a display transfer never enters a SYNTHESIZED plane and the tone map that makes it viewable stays `surface#TONE_MAP`'s. Every channel folds through a finiteness gate before `RgbSpectrum.Create`: a non-finite fitted coefficient throws the carrier's own admission inside a partitioned sweep where no rail carries it.
- Packages: Wacton.Unicolour (composed at the consuming edge — the scene-linear channel basis is `graph#MATERIAL_GRAPH` `PortValue.SceneLinear`, and a chromatic sky lands through the `photometric#PHOTOMETRIC` `EmissionSpectrum.Chromatic` arm rather than a re-minted illuminant here), NodaTime (`Instant` — the solar fold's clock carrier; a `DateTime` with an inferred kind is the fabricated-instant defect), CommunityToolkit.HighPerformance (`SpanOwner<T>` per-row scratch and `ParallelHelper.For` over a struct `IAction` row; the plane arena and its row windows are `Raster/plane#TEXTURE_PLANE`'s), Rasm (project — `Dimension`/`UnitInterval`/`Op`, and the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition.At`/`SolarSite`/`SunPosition` almanac the frame adapter projects), Rasm.Element (project — `ContentAddress` for the coefficient digest), Rasm.Materials.Appearance.Bsdf (`LocalVector`/`RgbSpectrum`/`MaterialFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new standard sky is one `CieSkyType` row over the existing group pair — the fifteen admitted types are a PROJECTION of the six × six product, so a national-annex sky is a row and never a case; a new gradation or indicatrix is one row on its own band; a genuinely new analytic radiance law is one `SkyModel` case; a new atmospheric parameter is one `SkyAtmosphere` column every case reads or ignores. `SkyCoefficients` carries the Hosek-Wilkie coefficient block as a CONTENT-KEYED DATA ASSET, this page's one carve-out from the generated-table law: the coefficients are a least-squares fit over a brute-force atmospheric simulation, so generating them from a defining sequence is fiction — `SkyCoefficients.Of` admits the caller-supplied block against its declared extents and stamps a `ContentAddress`, and a revised fit is a NEW digest rather than an edit. `ControlPoints` carries the fit's SOLAR-ELEVATION axis as a Bézier control lattice, so the block's degree is a declared extent and the Bernstein weights generate from the binomial rather than from a transcribed row.
- Boundary: solar position resolves the apparent refraction-corrected topocentric direction in the frozen `+Z`-up local frame — `+X` geographic north, `+Y` west, azimuth measured FROM `+X` increasing EASTWARD onto `−Y`: CLOCKWISE viewed from `+Z`, the OPPOSITE angular sense of the `[03]` equirect `u`, and the fold carries that sign exactly once (`−sin(azimuth)` on the `Y` lane), so the two conventions meet in the direction VALUE and never share an angular sense a transcriber could copy wrongly — while the geodetic datum, the site CRS, and any reprojection stay the app-root edge's and this owner takes latitude and longitude as admitted degrees. Ground albedo enters as an `RgbSpectrum`, the validated non-negative three-band carrier, so a spectrally tinted ground bounce is representable and a scalar albedo is the grey triple rather than a second parameter shape. `photometric#PHOTOMETRIC` `Photometric.Admit` clears every authored ZENITH LEVEL before the `CieStandard` arm distributes it, so a `cd/m²` sky and a `lux` sky reach one radiometric scalar and no page-local efficacy divide exists.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                          // CultureInfo (the invariant wire-key spelling)
using System.Linq;                                   // the level and band folds over generated rosters
using System.Runtime.InteropServices;                // MemoryMarshal (the coefficient-block byte projection)
using CommunityToolkit.HighPerformance;              // IAction, ReadOnlyMemory2D (the struct partition row, the level window)
using CommunityToolkit.HighPerformance.Buffers;      // SpanOwner, AllocationMode
using CommunityToolkit.HighPerformance.Helpers;      // ParallelHelper
using LanguageExt;                                   // Fin, Seq, Option
using NodaTime;                                      // Instant
using Rasm.Domain;                                   // Op, Deterministic (Hammersley — the composed low-discrepancy draw)
using Rasm.Element.Projection;                       // ContentAddress (the seam content key)
using Rasm.Materials.Appearance.Bsdf;                // LocalVector, RgbSpectrum, MaterialFault (band 2450), Microfacet
using Rasm.Materials.Appearance.Texture;             // TextureSource, TextureUv, SamplerState, UvSample, ShadeVec4, AddressMode, FilterMode
using Rasm.Materials.Raster;                         // TexturePlane, TexturePyramid, PlaneFormat, PlaneTransfer, PlaneDepth, AlphaMode, MipPolicy, LayerLaw
using Rasm.Numerics;                                 // Dimension, UnitInterval, SolarSite, SunPosition (the kernel almanac the adapter projects)
using Rhino.Geometry;                                // Vector3d — the UvSample world/normal lanes the dome leaves unused
using Thinktecture;
using static LanguageExt.Prelude;

// Folder-root namespace beside acquisition#ACQUISITION and finish#FINISH: an `Environment` sub-namespace captures
// System.Environment inside every declaration under it (the colour-colour naming trap), so the owner keeps the
// folder-root seat and the eventual source file is Appearance/Environment.cs.
namespace Rasm.Materials.Appearance;

// --- [TYPES] -------------------------------------------------------------------------------
// THE WORLD-DIRECTION CARRIER — the frozen +Z-up WORLD basis every dome surface speaks, a DISTINCT type from the
// bsdf#SHADING_FRAME WorldDirection tangent triple ON PURPOSE: the two frames share axis labels and nothing else,
// and one type serving both admitted a surface tangent-frame vector into a dome read as a silent re-lighting no
// gate could see. The split is STRUCTURAL — a dome entry takes WorldDirection, a lobe kernel takes WorldDirection,
// and the one legal crossing is an explicit basis rotation (the specular sweep's Oriented completion here; the
// consumer's own OracleFrame transform at the render seam). CosZenith reads the +Z zenith cosine every sky and
// measure law consumes; Zenith is the degenerate-normalize floor, matching WorldDirection's own convention.
public readonly record struct WorldDirection(double X, double Y, double Z) {
    public double CosZenith => Z;
    public double Dot(WorldDirection o) => (X * o.X) + (Y * o.Y) + (Z * o.Z);
    public WorldDirection Add(WorldDirection o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public WorldDirection Scale(double s) => new(X * s, Y * s, Z * s);

    public WorldDirection Normalize() {
        double n = Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        return n > 1e-12 ? new(X / n, Y / n, Z / n) : Zenith;
    }

    public static readonly WorldDirection Zenith = new(0.0, 0.0, 1.0);
}

// CieGradation rows the ISO 15469 luminance-gradation group: phi(Z) = 1 + a·exp(b / cos Z) is the zenith-angle
// falloff, read at the sample zenith and at zero (phi(0) = 1 + a·e^b) so the standard's ratio is scale-free. cos Z
// floors at the horizon epsilon — this group is defined on the upper hemisphere, and the radiance law routes a
// below-horizon direction to the ground term rather than a divide by zero here.
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

// CieIndicatrix rows the ISO 15469 scattering-indicatrix group: f(chi) = 1 + c·(exp(d·chi) − exp(d·pi/2)) + e·cos²chi
// over the angular distance chi between a sample and the sun. One is the isotropic overcast row; Six is the sharpest
// circumsolar peak.
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

// CieSkyType projects the fifteen ISO 15469 standard skies out of the gradation × indicatrix product — Type01 the
// CIE Overcast Sky, Type12 the CIE Standard Clear Sky. Key IS the standard's type number and the wire spelling is
// `cie-standard-NN`, so the whole family crosses on ONE EnvironmentLight field and a reader resolves the row from the
// key alone; a national-annex sky is one more row over the SAME groups, never a parallel case.
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
    public static readonly CieSkyType Type12 = new(key: 12, gradation: CieGradation.IV,  indicatrix: CieIndicatrix.Five);
    public static readonly CieSkyType Type13 = new(key: 13, gradation: CieGradation.V,   indicatrix: CieIndicatrix.Four);
    public static readonly CieSkyType Type14 = new(key: 14, gradation: CieGradation.V,   indicatrix: CieIndicatrix.Five);
    public static readonly CieSkyType Type15 = new(key: 15, gradation: CieGradation.VI,  indicatrix: CieIndicatrix.Five);
    public CieGradation Gradation { get; }
    public CieIndicatrix Indicatrix { get; }

    public string WireKey => string.Create(CultureInfo.InvariantCulture, $"cie-standard-{Key:D2}");

    // Relative answers the standard's L(sample) / L_zenith: the numerator reads the sample's zenith cosine and its
    // angular distance to the sun, the denominator the same pair evaluated at the zenith, so the quotient carries the
    // distribution alone and the absolute level rides SkyAtmosphere.ZenithRadiance.
    public double Relative(double cosZenith, double chi, double solarZenith) =>
        Indicatrix.F(chi) * Gradation.Phi(cosZenith) / (Indicatrix.F(solarZenith) * Gradation.PhiZenith);
}

// MapLayout bands the storage arrangements over ONE directional field. Each row carries its own coordinate law as
// delegate columns, so the [03] Project fold reads direction-to-coordinate and coordinate-to-direction off the row
// and a dual-paraboloid or lat-long-cross arrangement lands as a row with zero new surface. Layers binds the LayerLaw
// a TextureSet row declares, so a cube map is six layers under CubeFaces and never a six-plane sibling type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapLayout {
    // Sampler is a ROW column because addressing is a property of the ARRANGEMENT: equirect wraps U across the
    // azimuth seam and clamps V at the poles; a cube face clamps BOTH axes — a Repeat there wraps a face-edge tap
    // onto the opposite edge of the SAME face, which is never the adjacent face the sphere continues onto, so the
    // face-seam blend is the projection relation's concern and the sampler stays face-local; the octahedral fold is
    // continuous across its diagonal by construction and clamps its outer border.
    public static readonly MapLayout Equirect =
        new("equirect", layers: 1, law: LayerLaw.None, aspect: 2.0, forward: Equirectangular.Of, inverse: Equirectangular.Direction,
            layerOf: static _ => 0, measure: Equirectangular.Measure,
            sampler: new SamplerState(AddressMode.Repeat, AddressMode.Clamp, FilterMode.Trilinear));
    public static readonly MapLayout CubeFaces =
        new("cube-faces", layers: 6, law: LayerLaw.CubeFaces, aspect: 1.0, forward: Cube.Of, inverse: Cube.Direction,
            layerOf: Cube.Face, measure: Cube.Measure,
            sampler: new SamplerState(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Trilinear));
    public static readonly MapLayout Octahedral =
        new("octahedral", layers: 1, law: LayerLaw.None, aspect: 1.0, forward: Octahedron.Of, inverse: Octahedron.Direction,
            layerOf: static _ => 0, measure: Octahedron.Measure,
            sampler: new SamplerState(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Trilinear));
    public int Layers { get; }
    public LayerLaw Law { get; }
    public double Aspect { get; }
    public SamplerState Sampler { get; }

    [UseDelegateFromConstructor]
    public partial (UnitInterval U, UnitInterval V) Forward(WorldDirection direction);
    [UseDelegateFromConstructor]
    public partial WorldDirection Inverse(UnitInterval u, UnitInterval v, int layer);

    // LayerOf answers the layer a direction addresses. Equirect and octahedral answer zero by construction, so a read
    // never branches on the row's identity — the layer is a property of the DIRECTION and the arrangement together.
    [UseDelegateFromConstructor]
    public partial int LayerOf(WorldDirection direction);

    // SolidAngle measures what ONE texel of this arrangement subtends at the given direction — the differential of the
    // row's own mapping. Every measure-weighted reduction on this page reads it, so the SH projection and the
    // luminance guide carry no latitude formula written for an arrangement they were not handed; each row's closed
    // form is a norm cubed against the plane's texel area, so the three laws are one shape at three norms.
    [UseDelegateFromConstructor]
    public partial double SolidAngle(WorldDirection direction, int width, int height);

    // Extent answers the plane extent a target layout demands at a requested edge: the aspect column IS the law, so an
    // extent that contradicts the row is unrepresentable rather than caught downstream.
    public (Dimension Width, Dimension Height) Extent(Dimension edge) =>
        (Dimension.Create((int)Math.Round(edge.Value * Aspect)), edge);
}

// --- [MODELS] ------------------------------------------------------------------------------
// SkyAtmosphere rows the atmospheric parameters every sky case reads: Turbidity the Linke coefficient the
// Hosek-Wilkie fit is parameterized on, GroundAlbedo the spectral bounce folded into the dome AND distributed over
// its lower hemisphere, ZenithRadiance the admitted absolute level (already radiometric through the photometric
// gate) the CIE ratio scales, Exposure the scene-linear multiplier a caller re-applies without re-rendering. One
// row, every case.
public readonly record struct SkyAtmosphere(double Turbidity, RgbSpectrum GroundAlbedo, double ZenithRadiance, double Exposure) {
    public static Fin<SkyAtmosphere> Of(double turbidity, RgbSpectrum groundAlbedo, double zenithRadiance, double exposure, Op key) =>
        double.IsFinite(turbidity) && turbidity is >= 1.0 and <= 10.0
        && double.IsFinite(zenithRadiance) && zenithRadiance >= 0.0
        && double.IsFinite(exposure) && exposure > 0.0
            ? Fin.Succ(new SkyAtmosphere(turbidity, groundAlbedo, zenithRadiance, exposure))
            : MaterialFault.Parameter(key, $"<sky-atmosphere-out-of-range:{turbidity:R},{zenithRadiance:R},{exposure:R}>");
}

// RenderBudget carries the partition budget every sweep on this page reads: ParallelFloor bounds per-thread work off the
// caller's own policy rather than a page literal, and Parallel is the arming column a benchmark receipt flips.
public readonly record struct RenderBudget(int ParallelFloor, bool Parallel) {
    public static readonly RenderBudget Default = new(ParallelFloor: 32, Parallel: true);
    public int Floor => Parallel ? Math.Max(1, ParallelFloor) : int.MaxValue;
}

// SkyCoefficients carries the Hosek-Wilkie fitted dataset as a CONTENT-KEYED ASSET — this page's one carve-out from
// its generated-table law. Its block is a caller-supplied least-squares fit whose extents are DECLARED, never
// inferred: Of admits the exact element count and stamps the seed-zero ContentAddress the EnvironmentLight row
// carries, so a revised fit is a new digest and an in-place edit is unrepresentable. Lattice axes run (channel,
// albedo node, turbidity node, Bezier control point, term) — the SOLAR-ELEVATION axis is the control-point dimension,
// which is why a lookup takes an elevation parameter rather than a control-point index a caller would have to know
// how to choose.
public sealed record SkyCoefficients(
    ReadOnlyMemory<double> Fitted, int Channels, int AlbedoNodes, int TurbidityNodes, int ControlPoints, int Terms, ContentAddress Key) {
    public static Fin<SkyCoefficients> Of(
        ReadOnlyMemory<double> fitted, int channels, int albedoNodes, int turbidityNodes, int controlPoints, int terms, Op key) =>
        channels > 0 && albedoNodes > 0 && turbidityNodes > 0 && controlPoints > 1 && terms > 0
        && fitted.Length == channels * albedoNodes * turbidityNodes * controlPoints * terms
        && Finite.All(fitted.Span)
            ? Fin.Succ(new SkyCoefficients(fitted, channels, albedoNodes, turbidityNodes, controlPoints, terms,
                  ContentAddress.Of(MemoryMarshal.AsBytes(fitted.Span))))
            : MaterialFault.Parameter(key, $"<sky-coefficients-extent:{fitted.Length}>");

    // ONE read: bilinear over the (albedo, turbidity) control lattice, Bernstein over the solar-elevation control
    // points, at one channel and one term. The fit is piecewise over its own nodes and polynomial over its own
    // degree, so both interpolations are the DATASET's contract and neither is a caller policy.
    public double Term(int channel, double albedo, double turbidity, double elevation, int term) {
        ReadOnlySpan<double> block = Fitted.Span;
        (int a0, double at) = Node(albedo, AlbedoNodes);
        (int t0, double tt) = Node((turbidity - 1.0) / 9.0, TurbidityNodes);
        (int a1, int t1) = (Math.Min(a0 + 1, AlbedoNodes - 1), Math.Min(t0 + 1, TurbidityNodes - 1));
        double lo = Lerp(Bezier(block, channel, a0, t0, elevation, term), Bezier(block, channel, a0, t1, elevation, term), tt);
        double hi = Lerp(Bezier(block, channel, a1, t0, elevation, term), Bezier(block, channel, a1, t1, elevation, term), tt);
        return Lerp(lo, hi, at);
    }

    // Exemption: the Bernstein evaluation is a measured kernel — the weights GENERATE from the binomial at the
    // declared degree through the recurrence w(i+1) = w(i)·(n−i)/(i+1)·s/(1−s), so a fit shipped at a different
    // control-point count evaluates without a transcribed row. The parameter clamps a hair below one because the
    // recurrence divides by (1 − s): at the exact endpoint every weight would collapse to zero rather than to the
    // terminal control point, which is a zenith sun reading a black sky.
    double Bezier(ReadOnlySpan<double> block, int channel, int albedo, int turbidity, double elevation, int term) {
        double s = Math.Clamp(elevation, 0.0, 1.0 - 1e-12), inverse = 1.0 - s, sum = 0.0, weight = Math.Pow(inverse, ControlPoints - 1);
        for (int i = 0; i < ControlPoints; i++) {
            sum += weight * block[At(channel, albedo, turbidity, i, term)];
            weight = i + 1 < ControlPoints ? weight * s * (ControlPoints - 1 - i) / ((i + 1) * inverse) : weight;
        }
        return sum;
    }

    int At(int channel, int albedo, int turbidity, int controlPoint, int term) =>
        ((((((channel * AlbedoNodes) + albedo) * TurbidityNodes) + turbidity) * ControlPoints) + controlPoint) * Terms + term;

    static (int Node, double Fraction) Node(double unit, int nodes) {
        double scaled = Math.Clamp(unit, 0.0, 1.0) * (nodes - 1);
        int index = Math.Clamp((int)Math.Floor(scaled), 0, Math.Max(0, nodes - 2));
        return (index, scaled - index);
    }

    static double Lerp(double a, double b, double t) => a + ((b - a) * t);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SkyModel {
    private SkyModel() { }

    public sealed record HosekWilkie(SkyCoefficients Coefficients) : SkyModel;
    public sealed record CieStandard(CieSkyType Type) : SkyModel;

    // TermRadiance indexes the radiance level the published fit carries past its nine configuration terms per
    // (channel, albedo, turbidity, elevation). Term COUNT is the asset's declared extent; these are the slot names the
    // expansion reads, so a fit shipped with more terms admits and the extra slots go unread by this expansion.
    private const int TermRadiance = 9;

    public string WireKey => Switch(
        hosekWilkie: static _ => "hosek-wilkie",
        cieStandard: static c => c.Type.WireKey);

    // Per-direction scene-linear radiance in the +Z-up local frame over the WHOLE sphere. Both arms are pure over
    // (direction, sun, atmosphere) and the render fold owns the sweep, so a per-model entrypoint cannot exist. Below the
    // horizon the value is the ground bounce — albedo times the model's own horizon radiance — so the lower hemisphere
    // carries a real diffuse floor rather than a mirrored sky the clamped zenith cosine produces.
    internal RgbSpectrum Radiance(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) =>
        direction.CosZenith >= 0.0
            ? Sky(direction, sun, atmosphere)
            : Ground(sun, atmosphere);

    RgbSpectrum Sky(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) => Switch(
        state: (Direction: direction, Sun: sun, Atmosphere: atmosphere),
        hosekWilkie: static (s, h) => Fitted(h.Coefficients, s.Direction, s.Sun, s.Atmosphere),
        cieStandard: static (s, c) => Standard(c.Type, s.Direction, s.Sun, s.Atmosphere));

    // Ground reads the model's own radiance at the horizon on the sun's azimuth and multiplies by the spectral
    // albedo: one evaluation, both cases, no second lighting model and no flat authored constant. A zenith sun has
    // no azimuth — its horizon probe seats on +X rather than normalizing a zero vector.
    RgbSpectrum Ground(WorldDirection sun, SkyAtmosphere atmosphere) {
        WorldDirection horizon = Math.Abs(sun.X) + Math.Abs(sun.Y) > 1e-12
            ? new WorldDirection(sun.X, sun.Y, 0.0).Normalize()
            : new WorldDirection(1.0, 0.0, 0.0);
        return Sky(horizon, sun, atmosphere).Mul(atmosphere.GroundAlbedo);
    }

    // Exemption: the Hosek-Wilkie channel loop is a measured kernel — nine configuration terms plus the radiance term
    // per channel over the fitted lattice, with the anisotropic Mie phase term carried by the h coefficient. The Mie
    // numerator is (1 + cos²γ) and the gradation exponent divides by (cos θ + 0.01) — the published model's own
    // horizon offset, never a clamp standing in for it. GroundAlbedo reads PER CHANNEL and the solar elevation drives
    // the Bezier axis, so a spectrally tinted bounce reaches each channel independently and a low sun reads its own
    // fitted configuration rather than the zenith's. Elevation is the fit's own cube-root parameterization.
    static RgbSpectrum Fitted(SkyCoefficients fit, WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) {
        double cosTheta = Math.Max(0.0, direction.CosZenith);
        double cosGamma = Math.Clamp(direction.Dot(sun), -1.0, 1.0);
        double gamma = Math.Acos(cosGamma);
        double elevation = Math.Cbrt(Math.Clamp(Math.Asin(Math.Clamp(sun.CosZenith, -1.0, 1.0)) / (Math.PI / 2.0), 0.0, 1.0));
        ReadOnlySpan<double> albedo = [atmosphere.GroundAlbedo.R, atmosphere.GroundAlbedo.G, atmosphere.GroundAlbedo.B];
        Span<double> channels = stackalloc double[3];
        for (int c = 0; c < 3; c++) {
            double a = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 0), b = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 1);
            double p = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 2), d = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 3);
            double e = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 4), f = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 5);
            double g = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 6), h = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 7);
            double i = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, 8);
            double level = fit.Term(c, albedo[c], atmosphere.Turbidity, elevation, TermRadiance);
            double mie = (1.0 + (cosGamma * cosGamma)) / Math.Pow(Math.Max(1e-6, 1.0 + (h * h) - (2.0 * h * cosGamma)), 1.5);
            double expansion = (1.0 + (a * Math.Exp(b / (cosTheta + 0.01))))
                             * (p + (d * Math.Exp(e * gamma)) + (f * cosGamma * cosGamma) + (g * mie) + (i * Math.Sqrt(cosTheta)));
            channels[c] = expansion * level * atmosphere.Exposure;
        }
        return Finite.Spectrum(channels);
    }

    // Standard distributes the CIE standard sky's LUMINANCE ratio, so this arm is achromatic in the working space: a
    // chromatic overcast dome is the photometric#PHOTOMETRIC EmissionSpectrum.Chromatic arm a caller composes, never a
    // second colour path minted here.
    static RgbSpectrum Standard(CieSkyType type, WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) {
        double level = type.Relative(
            Math.Max(1e-4, direction.CosZenith),
            Math.Acos(Math.Clamp(direction.Dot(sun), -1.0, 1.0)),
            Math.Acos(Math.Clamp(sun.CosZenith, -1.0, 1.0))) * atmosphere.ZenithRadiance * atmosphere.Exposure;
        Span<double> channels = [level, level, level];
        return Finite.Spectrum(channels);
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// SolarPosition is the FRAME ADAPTER over the kernel Rasm/Numerics/calculus#SOLAR_EPHEMERIS almanac: the apparent
// azimuth/altitude ANGLES resolve at the kernel (nutation, quadratic mean longitude, pressure-corrected refraction),
// and this owner projects them into the frozen +X-north/+Y-west/+Z-up frame — the one part of the fold that is this
// page's own correspondence. The kernel SunPosition.Direction (+Y-north survey frame) is deliberately unread here.
public static class SolarPosition {
    public static Fin<WorldDirection> Of(double latitudeDegrees, double longitudeDegrees, Instant instant, Op key) =>
        double.IsFinite(latitudeDegrees) && latitudeDegrees is >= -90.0 and <= 90.0
        && double.IsFinite(longitudeDegrees) && longitudeDegrees is >= -180.0 and <= 180.0
            ? Fin.Succ(Project(Rasm.Numerics.SolarPosition.At(
                SolarSite.Create(latitudeDegrees, longitudeDegrees, timezoneHours: 0.0, elevationM: 0.0), instant)))
            : MaterialFault.Parameter(key, $"<solar-site-out-of-range:{latitudeDegrees:R},{longitudeDegrees:R}>");

    // Azimuth measures from north INCREASING EASTWARD; the frozen local frame is +X north / +Y WEST, so the
    // east component seats on −Y — a morning sun lands ESE, never in the west. Timezone cancels inside the
    // almanac's true-solar-minutes fold and elevation zero reads sea-level refraction — the Bennett floor the
    // deleted page-local fold carried.
    static WorldDirection Project(SunPosition sun) {
        double azimuth = sun.AzimuthDeg * (Math.PI / 180.0);
        double apparent = sun.AltitudeDeg * (Math.PI / 180.0);
        return new WorldDirection(Math.Cos(apparent) * Math.Cos(azimuth), -(Math.Cos(apparent) * Math.Sin(azimuth)), Math.Sin(apparent)).Normalize();
    }
}

// Finite folds both finiteness gates under the declared kernel exemption: All gates an admitted block, Spectrum gates
// a per-texel channel triple before the validated carrier's own Create THROWS inside a partitioned sweep no rail
// covers. A non-finite fitted coefficient reaches black rather than tearing down the fold.
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
    // ONE synthesis entry, parameterized by the LAYOUT ROW. The extent, the layer count, and the inverse coordinate
    // law all read off the row, so a cube-face dome and an equirect dome are one body and the 2:1 equirect law is the
    // row's aspect column rather than a width-parity gate a cube layout would fail for the wrong reason. The sweep
    // partitions stacked (layer, row) indices through ParallelHelper.For over a readonly-struct IAction row that
    // writes through the plane owner's own Write rail — the transfer and lane gather are the arena's, never re-spelled.
    public static Fin<TexturePlane> Render(
        SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun, MapLayout layout, Dimension edge, RenderBudget budget, Op key) {
        (Dimension width, Dimension height) = layout.Extent(edge);
        return TexturePlane.Of(PlaneFormat.Rgba32F, width, height, PlaneTransfer.Linear, AlphaMode.None, key,
                    layers: Some(Dimension.Create(layout.Layers)))
                .Map(plane => {
                    ParallelHelper.For(0, height.Value * layout.Layers, new SkySweep(model, atmosphere, sun.Normalize(), layout, plane), budget.Floor);
                    return plane;
                });
    }

    // Exemption: one row per partitioned index, each texel resolving the [03] correspondence into the ShadeVec4 field the
    // Write rail gathers. Scratch is rented per invocation off the array pool the SpanOwner rail owns, never held on the
    // plane — a plane-held buffer serializes every row fold this page and the filter passes drive.
    readonly struct SkySweep(SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun, MapLayout layout, TexturePlane plane) : IAction {
        public void Invoke(int stacked) {
            (int width, int height) = (plane.Width.Value, plane.Height.Value);
            (int layer, int y) = (stacked / height, stacked % height);
            using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(width, AllocationMode.Clear);
            UnitInterval v = UnitInterval.Create((y + 0.5) / height);
            Span<ShadeVec4> lanes = field.Span;
            for (int x = 0; x < width; x++) {
                RgbSpectrum radiance = model.Radiance(
                    layout.Inverse(UnitInterval.Create((x + 0.5) / width), v, layer), sun, atmosphere);
                lanes[x] = new ShadeVec4(radiance.R, radiance.G, radiance.B, 1.0);
            }
            plane.WriteShade(y, layer, lanes);
        }
    }
}
```

## [03]-[ENVIRONMENT_MAP]

- Owner: `EnvironmentMap` the admitted directional-radiance carrier over one `TexturePlane`; `MapLayout` the storage band; `Equirectangular`/`Cube`/`Octahedron` the three coordinate laws the rows bind.
- Entry: `public static Fin<EnvironmentMap> Of(TexturePlane plane, MapLayout layout, double intensity, double rotation, Op key)` admits a decoded plane against its layout's aspect law, its transfer band, its HDR depth gate, and its layer congruence, lifting ONE scene-linear sampler PER LAYER; `public Fin<EnvironmentMap> Project(MapLayout target, Dimension edge, RenderBudget budget, Op key)` is the ONE layout relation — equirect to cube faces, cube faces to equirect, either to octahedral — because a direction-indexed field admits an exact inverse and a direction-named sibling converter pair is the rejected split; `Stored(direction, lod)`, `Radiance(direction, lod)`, and `Texel(layer, x, y)` are the three reads.
- Law: STORED and WORLD are two frames on ONE field and the split is structural. `Stored` reads the plane as authored — no rotation, no intensity; `Radiance` un-applies the dome rotation and scales by intensity. Every PREFILTER product integrates `Stored`, so rotating or re-exposing a dome re-keys NOTHING: the SH vector, the specular level set, and the luminance guide are stored-frame blobs a rotation reads through rather than a policy baked into their bytes. Prefiltering over the world frame makes `EnvironmentLight.Rotation` a re-bake trigger while this owner's boundary law calls it read-time — that contradiction is what the split forecloses.
- Law: the equirect correspondence is FROZEN and single-sourced here — `u = 0.5 + atan2(d.Y, d.X) / 2π`, `v = acos(clamp(d.Z, −1, 1)) / π`, `v = 0` at `+Z`, `u` increasing counter-clockwise viewed from `+Z` — so the sky sweep, the prefilter, the CDF, and the `EnvironmentLight` lookup all address one mapping and a consumer re-deriving it forks the seam this owner exists to hold. `WorldDirection` is the PRODUCER-OWNED world carrier of that basis — the same `+Z`-up convention the `bsdf#SHADING_FRAME` `LocalVector` tangent triple declares for its own frame, split into a DISTINCT type here so a tangent-frame vector cannot reach a dome read and the frame law needs no consumer-side prose; a Y-up runtime remaps the DIRECTION BASIS at its own read and never rewrites a plane.
- Law: the sampler lift is PER LAYER. `Raster/plane#TEXTURE_PYRAMID` `AsImage` carries one layer by construction, so a six-face cube admitted as one sampler refuses the bridge and leaves the layered arms declared capability that cannot run. Each layer extracts as its own scene-linear plane, folds its own Kaiser pyramid, and lifts one `TextureSource.Image`; the map HOLDS each pyramid beside its sampler — the bridge's levels window the pyramid's arenas, so the chain releases at the map's own `Dispose` and a lift-time dispose reads freed memory on the first tap.
- Growth: a new arrangement is one `MapLayout` row binding its layer count, `LayerLaw`, aspect, and the three coordinate delegates — the `Project` fold and the per-layer lift read all four off the row, so a dual-paraboloid or lat-long cube-cross lands as a row with zero new surface; a new admission gate is one predicate on `Of`.
- Boundary: `EnvironmentMap` NEVER decodes a container — `Raster/codec#RASTER_CODEC` sniffs the magic and produces the plane, this owner admitting the decoded plane alone, so a Radiance `.hdr`, an OpenEXR half plane, and a synthesized sky reach ONE admission. `linear`, `pq`, and `hlg` are the ADMITTED transfers and this is the one surface in the corpus where the display-referred pair is legal; `srgb` and `raw` refuse, because an sRGB-encoded dome cannot carry a sun and a raw dome declares no light quantity at all. `Lift` lowers whatever admits to scene-linear ONCE through the arena's own `Read` rail, so every value this owner hands out is scene-referred and no consumer re-applies a transfer. Rotation and intensity are READ-TIME values — a re-oriented or re-exposed dome never re-keys a blob and never re-runs a prefilter — and a rotated dome is a coordinate change rather than a resampled plane. `MaterialFault` rails a wrong-aspect plane, a layer count contradicting the row, an integer-depth plane, a refused transfer, and an out-of-range read policy.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Sources is one texture#TEXTURE_UV sampler view PER LAYER over the scene-linear lift of the admitted plane, built
// once at admission — so every filtered read on this page reconstructs through the one admitted sampler set, a
// six-face cube reads face by face, and no second filter exists. Plane is the SOURCE arena the blob key addresses.
// Pyramids HOLDS each layer's mip chain for the map's own per-level reads (EXR ladder egress, layer extraction).
// The sampler images are AsImage COPIES — independent of the pyramid arenas per plane#TEXTURE_PYRAMID — so Sources
// outlive any chain release; the chains still release with the map, one owner, one Dispose.
public sealed record EnvironmentMap(
    TexturePlane Plane, Seq<TexturePyramid> Pyramids, Seq<TextureSource.Image> Sources, MapLayout Layout,
    double Intensity, double Rotation, Op Key) : IDisposable {

    // Admitted rows the three transfers. pq and hlg are legal HERE and nowhere else in the corpus: an environment map
    // is the one display-referred ingest a scene-linear pipeline admits, and the per-layer lift lowers it once.
    static readonly Seq<PlaneTransfer> Admitted = Seq(PlaneTransfer.Linear, PlaneTransfer.Pq, PlaneTransfer.Hlg);

    public static Fin<EnvironmentMap> Of(TexturePlane plane, MapLayout layout, double intensity, double rotation, Op key) =>
        from _ in guard(plane.Layers.Value == layout.Layers,
                MaterialFault.Parameter(key, $"<environment-layer-count:{plane.Layers.Value}!={layout.Layers}>"))
        from __ in guard(layout.Law.Admits(plane.Layers.Value),
                MaterialFault.Parameter(key, $"<environment-layer-law:{layout.Law.Key}:{plane.Layers.Value}>"))
        from ___ in guard(Math.Abs(((double)plane.Width.Value / plane.Height.Value) - layout.Aspect) < 1e-9,
                MaterialFault.Parameter(key, $"<environment-aspect:{plane.Width.Value}x{plane.Height.Value}!={layout.Key}>"))
        // Integer depth cannot carry a dome: an 8- or 16-bit plane clips the sun by orders of magnitude and the
        // prefilter then integrates a truncated distribution no downstream gate can recover.
        from ____ in guard(plane.Format.Depth == PlaneDepth.F16 || plane.Format.Depth == PlaneDepth.F32,
                MaterialFault.Parameter(key, $"<environment-not-hdr:{plane.Format.Key}>"))
        from _____ in guard(Admitted.Exists(t => t == plane.Transfer),
                MaterialFault.Parameter(key, $"<environment-transfer:{plane.Transfer.Key}>"))
        from ______ in guard(double.IsFinite(intensity) && intensity >= 0.0
                          && double.IsFinite(rotation) && rotation is >= 0.0 and < (2.0 * Math.PI),
                MaterialFault.Parameter(key, $"<environment-read-policy:{intensity:R},{rotation:R}>"))
        from lifted in Lift(plane, key)
        select new EnvironmentMap(plane, lifted.Map(static pair => pair.Pyramid), lifted.Map(static pair => pair.Source),
            layout, intensity, rotation, key);

    // ONE sampler per layer. Each layer extracts as its own scene-linear plane through the arena's paired Read/Write
    // rails — the read decodes whatever transfer admitted, the write stores linear — then folds its Kaiser pyramid
    // and lifts the sampler bridge. The pyramid is HELD: the bridge's levels window the pyramid's arenas, so the
    // chain lives exactly as long as the samplers reading it and releases at the map's own Dispose.
    static Fin<Seq<(TexturePyramid Pyramid, TextureSource.Image Source)>> Lift(TexturePlane plane, Op key) =>
        toSeq(Enumerable.Range(0, plane.Layers.Value))
            .Traverse(layer =>
                from face in Face(plane, layer, key)
                from pyramid in TexturePyramid.Of(face, MipPolicy.Kaiser, key)
                from source in pyramid.AsImage(key)
                select (Pyramid: pyramid, Source: source)).As()
            .Map(static pairs => pairs.Strict());

    // Exemption: the layer extraction is a measured kernel — one row read from the source layer, one row written to the
    // linear face, the arena owning both directions so no transfer or lane arithmetic is re-spelled here.
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

    // Radiance in the STORED frame: the layout row addresses the field, the layer follows the direction, the lod
    // selects the pyramid level a filtered-importance tap wants. No rotation, no intensity — this is the read every
    // prefilter integrates, which is exactly why its products survive a rotation with no re-bake.
    public RgbSpectrum Stored(WorldDirection direction, double lod) {
        WorldDirection local = direction.Normalize();
        (UnitInterval u, UnitInterval v) = Layout.Forward(local);
        return TextureUv.Sample(Sources[Layout.LayerOf(local)], new UvSample(u, v, Vector3d.Zero, Vector3d.ZAxis, lod), Layout.Sampler, Key)
            .Match(Succ: static f => f.IsFinite ? RgbSpectrum.Create(Math.Max(0.0, f.X), Math.Max(0.0, f.Y), Math.Max(0.0, f.Z))
                                                : RgbSpectrum.Black,
                   Fail: static _ => RgbSpectrum.Black);
    }

    // Radiance at a WORLD direction: the dome rotation un-applies, the stored read answers, intensity scales. The
    // two reads are ONE correspondence, so a consumer never assembles the world frame from parts.
    public RgbSpectrum Radiance(WorldDirection direction, double lod) =>
        Stored(Rotated(direction.Normalize(), -Rotation), lod).Scale(Intensity);

    // Texel answers the EXACT base-level texel a source-domain sweep wants. The SH projection and the luminance guide
    // iterate the stored grid itself, so a direction round-trip through the trilinear sampler prices a filtered
    // reconstruct per texel and biases the pole rows through the clamped V address — the projection's own measure
    // already carries the latitude weight, and re-filtering it is the second error the exact read forecloses.
    public RgbSpectrum Texel(int layer, int x, int y) =>
        Sources[layer].Levels[0].Span[y, x] is var texel && texel.IsFinite
            ? RgbSpectrum.Create(Math.Max(0.0, texel.X), Math.Max(0.0, texel.Y), Math.Max(0.0, texel.Z))
            : RgbSpectrum.Black;

    // ONE layout relation carrying both directions: the target row supplies its own inverse, so equirect→cube,
    // cube→equirect, and either→octahedral are one resample and a direction-named sibling owner is deleted. The
    // resample reads the STORED frame and carries the read policy forward, so a projection is not a re-orientation.
    public Fin<EnvironmentMap> Project(MapLayout target, Dimension edge, RenderBudget budget, Op key) {
        if (target.Key == Layout.Key) { return Fin.Succ(this); }
        (Dimension width, Dimension height) = target.Extent(edge);
        return TexturePlane.Of(PlaneFormat.Rgba32F, width, height, PlaneTransfer.Linear, AlphaMode.None, key,
                    layers: Some(Dimension.Create(target.Layers)))
                .Map(plane => {
                    ParallelHelper.For(0, height.Value * target.Layers, new ProjectSweep(this, target, plane), budget.Floor);
                    return plane;
                })
                .Bind(plane => Of(plane, target, Intensity, Rotation, key));
    }

    public void Dispose() {
        Pyramids.Iter(static pyramid => pyramid.Dispose());
        Plane.Dispose();
    }

    internal static WorldDirection Rotated(WorldDirection d, double radians) {
        (double s, double c) = Math.SinCos(radians);
        return new WorldDirection((d.X * c) - (d.Y * s), (d.X * s) + (d.Y * c), d.Z);
    }

    // Exemption: the resample kernel — each target texel inverts to a direction through the TARGET row and reads the
    // source through the SOURCE row, so the two coordinate laws meet on the direction and never on an index formula.
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

// Equirectangular, Cube, and Octahedron own the three coordinate laws the MapLayout rows bind. Each is a pure pair —
// direction to (u, v) and (u, v, layer) back to direction — so the relation is exact in both directions and a row
// cannot carry a half-defined mapping.
internal static class Equirectangular {
    public static (UnitInterval U, UnitInterval V) Of(WorldDirection d) {
        double u = 0.5 + (Math.Atan2(d.Y, d.X) / (2.0 * Math.PI));
        return (UnitInterval.Create(u - Math.Floor(u)), UnitInterval.Create(Math.Acos(Math.Clamp(d.Z, -1.0, 1.0)) / Math.PI));
    }

    public static WorldDirection Direction(UnitInterval u, UnitInterval v, int layer) {
        double phi = (u.Value - 0.5) * 2.0 * Math.PI, theta = v.Value * Math.PI, sinTheta = Math.Sin(theta);
        return new WorldDirection(sinTheta * Math.Cos(phi), sinTheta * Math.Sin(phi), Math.Cos(theta));
    }

    // sin(theta)·(2pi/w)·(pi/h): the pole rows vanish, so a projection weighted by this measure is not
    // latitude-biased and the grid sums to 4pi.
    public static double Measure(WorldDirection d, int width, int height) =>
        Math.Sqrt(Math.Max(0.0, 1.0 - (d.Z * d.Z))) * (2.0 * Math.PI / width) * (Math.PI / height);
}

// Cube faces in the frozen +X, −X, +Y, −Y, +Z, −Z layer order; Face reads the dominant axis, so the layer is a
// property of the direction rather than a caller argument the projection could contradict.
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

    // (4/(w·h))·|dominant|³ over ONE face: the face-local texel spans 4/(w·h) in (s,t) and the projection onto the
    // sphere scales by (1 + s² + t²)^(−3/2), which for a unit direction IS the dominant component cubed. Six faces
    // sum to 4pi, the same closed form the octahedral row states at its own norm.
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

// Octahedron folds one square plane with no seam gutters and a filtered read that stays continuous across the
// diagonal — the cheap single-plane arrangement a GPU bake pass prefers over six cube faces.
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

    // (4/(w·h))·L1(d)³: the unfold onto the square is linear in area, so the only non-constant term is the
    // octahedron-to-sphere Jacobian, which for a unit direction reduces to the L1 norm cubed — the cube row's law at
    // a different norm, so both arrangements state one shape rather than two derivations.
    public static double Measure(WorldDirection d, int width, int height) {
        double norm = Math.Abs(d.X) + Math.Abs(d.Y) + Math.Abs(d.Z);
        return 4.0 * norm * norm * norm / (width * (double)height);
    }
}
```

## [04]-[IBL_PREFILTER]

- Owner: `IblPrefilter` the reduction fold; `ShBand` the nine-row spherical-harmonic basis table; `Sh9` the twenty-seven-value irradiance carrier; `IblPolicy` the sampling-budget row; `IblProducts` the receipt; the kernel `Deterministic.Hammersley` the composed low-discrepancy draw.
- Entry: `public static Fin<IblProducts> Prefilter(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key)` — ONE reduction producing every product, because the pyramid, the SH projection, and the CDF each sweep the same field and three entrypoints sweep it three times; `Sh9.Project(EnvironmentMap, RenderBudget, Op)` and `Sh9.Irradiance(WorldDirection)` are the projection and reconstruction halves of ONE correspondence on one owner; `IblProducts.SpecularLevel(UnitInterval)` maps a roughness onto the fractional mip the level set encodes.
- Law: every product integrates the map's STORED frame. Rotation and intensity apply at the `[05]` read, so a re-oriented or re-exposed dome reuses the same content-addressed blobs and a rotation is never a re-bake.
- Law: the SH9 spelling is FROZEN and this table IS it — real orthonormal harmonics through `l = 2` in the right-handed `+Z`-up basis, band-major with RGB interleaved at index `i·3 + c`, carrying the Lambertian convolution constants `Â₀ = π`, `Â₁ = 2π/3`, `Â₂ = π/4`. `ShGolden.All` carries the two frozen vectors as an EVALUATED fixture roster and `ShGolden.Prove` runs them: `L(ω) = 1` yields `sh_0 = 3.5449077018110318` (`2√π`) with every other band zero and `E(n) = π` for all `n`; `L(ω) = ω·ẑ` yields `sh_2 = 2.046653415892977` with every other band zero and `E(+ẑ) = 2π/3` — a Y-up transcription places the axial energy at `sh_1` or `sh_3` and fails, and the reconstruction probe fails a wrong `Â` set the projection alone cannot see. The same proof sums each `MapLayout.SolidAngle` closed form to `4π` over its own grid. `Sh9.Of` refuses a channel-major layout and any length other than twenty-seven.
- Law: every GGX integral here reads the `bsdf#MICROFACET_KERNEL` kernel — `Microfacet.SampleVisibleNormal` draws the half-vector, `Microfacet.VisibleNormalPdf` supplies the density, `Microfacet.MaskingShadowing` the Smith term — so the prefiltered dome and the shaded surface integrate the SAME distribution and a re-minted importance sampler is the deleted form. Sampling composes the kernel `Deterministic.Hammersley` equidistributed pair — the low-discrepancy member family the deterministic-draw owner carries BESIDE its splitmix64 stream, because splitmix64 clustering leaves visible prefilter noise at a bounded tap budget — so this page authors no sampling kernel of its own.
- Law: the specular tap reads the SOURCE mip whose solid angle matches the sample density. That term is the firefly suppression a bounded tap budget requires and is a declared column rather than a hidden clamp: a blown highlight spreads across the taps it covers instead of being clipped out of the integral.
- Law: every SOURCE-DOMAIN sweep partitions and reads by INDEX. `Sh9.Project` runs a commutative reduction, so each row accumulates its own band vector under `ParallelHelper.For` and one fold sums them; the luminance guide's per-row conditional mass is likewise independent, and only the marginal prefix over row masses is sequential. This law forecloses a serial full-plane sweep beside partitioned siblings — at a four-thousand-texel dome the two reductions are the campaign's heaviest single-threaded folds.
- Growth: a new prefilter product is one column on `IblProducts` filled inside the one sweep; a new sampling budget is one `IblPolicy` column; a new mip ladder is a `MipPolicy` row on the pyramid owner. `BrdfLut` stays environment-INDEPENDENT and view-independent — a pure function of `(N·V, roughness)` — so it computes once per `IblPolicy` and a second environment reuses the same blob by content address.
- Boundary: prefiltering NEVER writes a file; `IblProducts` carries planes and the egress name grammar belongs to `Raster/set#TEXTURE_SET`. Plane bytes are always CPU-minted, so a GPU prefilter arm is an accelerator whose output is never content-addressed. Building the CDF refuses a degenerate all-black dome rather than returning a flat table that samples uniformly while claiming importance.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// ShBand rows the frozen SH9 basis: (l, m), the normalization constant, the basis polynomial, and the Lambertian
// convolution coefficient per band. Projection AND irradiance reconstruction are ONE fold over Items, so band order,
// normalization, and convolution can never disagree between the two directions.
[SmartEnum<int>]
public sealed partial class ShBand {
    public static readonly ShBand Sh0 = new(key: 0, l: 0, m:  0, constant: 0.28209479177387814, basis: static d => 1.0,                       convolution: Math.PI);
    public static readonly ShBand Sh1 = new(key: 1, l: 1, m: -1, constant: 0.4886025119029199,  basis: static d => d.Y,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh2 = new(key: 2, l: 1, m:  0, constant: 0.4886025119029199,  basis: static d => d.Z,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh3 = new(key: 3, l: 1, m:  1, constant: 0.4886025119029199,  basis: static d => d.X,                       convolution: 2.0 * Math.PI / 3.0);
    public static readonly ShBand Sh4 = new(key: 4, l: 2, m: -2, constant: 1.0925484305920792,  basis: static d => d.X * d.Y,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh5 = new(key: 5, l: 2, m: -1, constant: 1.0925484305920792,  basis: static d => d.Y * d.Z,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh6 = new(key: 6, l: 2, m:  0, constant: 0.31539156525252005, basis: static d => (3.0 * d.Z * d.Z) - 1.0,   convolution: Math.PI / 4.0);
    public static readonly ShBand Sh7 = new(key: 7, l: 2, m:  1, constant: 1.0925484305920792,  basis: static d => d.X * d.Z,                 convolution: Math.PI / 4.0);
    public static readonly ShBand Sh8 = new(key: 8, l: 2, m:  2, constant: 0.5462742152960396,  basis: static d => (d.X * d.X) - (d.Y * d.Y), convolution: Math.PI / 4.0);
    public int L { get; }
    public int M { get; }
    public double Constant { get; }
    public double Convolution { get; }

    [UseDelegateFromConstructor]
    public partial double Basis(WorldDirection direction);

    public double Evaluate(WorldDirection direction) => Constant * Basis(direction);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Twenty-seven values, band-major with RGB interleaved: index i·3 + c holds band i channel c. A channel-major
// layout is the decode fork this owner forecloses, and the length gate is structural at Of.
public sealed record Sh9(ReadOnlyMemory<double> Bands) {
    public const int Slots = 27;

    public static Fin<Sh9> Of(ReadOnlyMemory<double> bands, Op key) =>
        bands.Length == Slots && Finite.All(bands.Span)
            ? Fin.Succ(new Sh9(bands))
            : MaterialFault.Parameter(key, $"<sh9-layout:{bands.Length}!={Slots}>");

    // Cosine-convolved irradiance at a surface normal: E(n) = sum over bands of convolution · L_i · Y_i(n). The
    // Lambertian outgoing radiance a shading rail wants is albedo · E(n) / pi — that divide belongs to the lobe.
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

    // Project runs the forward half of that same correspondence, over the STORED frame and PER LAYER so a cube dome
    // projects as faithfully as an equirect one. Each partitioned row accumulates its own band vector and the fold sums
    // them — this projection is a commutative reduction, so partitioning costs no ordering and the pole rows carry
    // their true vanishing measure through the layout's own inverse rather than a latitude formula assumed here.
    public static Fin<Sh9> Project(EnvironmentMap map, RenderBudget budget, Op key) {
        (int w, int h, int layers) = (map.Plane.Width.Value, map.Plane.Height.Value, map.Plane.Layers.Value);
        double[] rows = new double[h * layers * Slots];
        ParallelHelper.For(0, h * layers, new ProjectSweep(map, rows, w, h), budget.Floor);
        double[] bands = new double[Slots];
        for (int row = 0; row < h * layers; row++) {
            for (int slot = 0; slot < Slots; slot++) { bands[slot] += rows[(row * Slots) + slot]; }
        }
        return Of(bands, key);
    }

    // Exemption: the projection kernel. The solid-angle measure comes from the layout's own inverse — the equirect
    // sin(theta) factor and the cube face's own Jacobian are both the differential of the row's mapping — evaluated
    // as the local area the texel spans, so no arrangement carries a measure formula written for another.
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

// IblPolicy rows the whole prefilter budget: taps per specular texel, LUT taps, the two extents, the pyramid depth,
// and the importance-sampling arm. Every knob a caller could reach for is a column, never a parameter tail.
public readonly record struct IblPolicy(int SpecularTaps, int LutTaps, Dimension LutExtent, Dimension SpecularEdge, int Mips, bool ImportanceSampled) {
    public static readonly IblPolicy Default = new(SpecularTaps: 1024, LutTaps: 512, LutExtent: Dimension.Create(256),
        SpecularEdge: Dimension.Create(256), Mips: 6, ImportanceSampled: true);

    public double RoughnessAt(int mip) => Mips <= 1 ? 0.0 : (double)mip / (Mips - 1);
}

// LuminanceCdf owns the dome sampling structure as a TYPED owner rather than a plane a consumer re-interprets:
// Conditional holds each row's running mass across u, Marginal the running mass down v, Total the whole
// measure-weighted luminance. Plane is the PERSISTED projection the blob carries — one owner, two faces, so the
// sampler never parses a raster.
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

    // Draw guides the pick: the marginal picks the row, that row's conditional picks the column. Both searches read the
    // running form directly, so no per-draw normalization exists.
    public (int X, int Y) Draw(UnitInterval u0, UnitInterval u1) {
        int y = Locate(Marginal.Span[..Height], u1.Value * Total);
        ReadOnlySpan<double> row = Conditional.Span.Slice(y * Width, Width);
        return (Locate(row, u0.Value * row[Width - 1]), y);
    }

    // Density answers in SOLID ANGLE at a texel whose stored luminance is known. Texel mass already carries that
    // texel's own solid angle, so converting from texel measure to solid angle divides by the same factor and it
    // CANCELS — the density is the plain luminance quotient, layout-independent, integrating to unity by
    // construction. An arrangement-specific constant here is the equirect Jacobian written twice.
    public double Density(double luminance) => Total > 0.0 ? luminance / Total : 0.0;

    // Exemption: the running-mass bisection is a measured kernel — the running form is non-decreasing by
    // construction, so the search is total and needs no equality tolerance.
    static int Locate(ReadOnlySpan<double> running, double target) {
        int lo = 0, hi = running.Length - 1;
        while (lo < hi) {
            int mid = lo + ((hi - lo) / 2);
            if (running[mid] < target) { lo = mid + 1; } else { hi = mid; }
        }
        return lo;
    }
}

// IblProducts carries the reduction receipt. LuminanceCdf is Option because importance sampling is an IblPolicy flip
// and an absent CDF is a DECLARED uniform-dome fallback the light row reports, never a zero-filled plane a reader
// mistakes for data. Specular is a Seq of INDEPENDENT levels, never a TexturePyramid: a GGX prefilter chain is not a
// mip fold of one base plane — each level is its own integral at its own roughness — so folding it through the
// pyramid owner claims a downsample relation the levels do not have. RoughnessPerMip is the ladder that binds them.
// BrdfPyramid HOLDS the single-level chain for the receipt's own plane reads; BrdfSource is an AsImage COPY
// independent of the chain per plane#TEXTURE_PYRAMID, and the pyramid (owning its base plane) releases exactly
// once at Dispose.
public sealed record IblProducts(
    Sh9 Irradiance, Seq<TexturePlane> Specular, Seq<double> RoughnessPerMip, TexturePlane BrdfLut, TexturePyramid BrdfPyramid,
    TextureSource.Image BrdfSource, Option<LuminanceCdf> Cdf) : IDisposable {
    public void Dispose() { Specular.Iter(static level => level.Dispose()); BrdfPyramid.Dispose(); }

    // SpecularLevel answers the fractional level a roughness addresses — the inverse of IblPolicy.RoughnessAt, so a
    // shading read and a prefilter write agree by construction and a renderer never re-derives the ladder.
    public double SpecularLevel(UnitInterval roughness) =>
        RoughnessPerMip.Count <= 1 ? 0.0 : roughness.Value * (RoughnessPerMip.Count - 1);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class IblPrefilter {
    public static Fin<IblProducts> Prefilter(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key) =>
        from irradiance in Sh9.Project(map, budget, key)
        from specular in Specular(map, policy, budget, key)
        from lut in BrdfLut(policy, budget, key)
        // BrdfSource reads through the SAME sampler the dome does — a single-level pyramid bridged by AsImage — so the
        // split-sum read and the environment read share one reconstruction and no point-read surface is minted. The
        // pyramid rides the receipt: its arenas back the sampler, so it releases with the products, never here.
        from lutPyramid in TexturePyramid.Of(lut, MipPolicy.None, key)
        from lutSource in lutPyramid.AsImage(key)
        from cdf in policy.ImportanceSampled ? Guide(map, budget, key).Map(Some) : Fin.Succ(Option<LuminanceCdf>.None)
        select new IblProducts(irradiance, specular, toSeq(Enumerable.Range(0, policy.Mips).Select(policy.RoughnessAt)), lut, lutPyramid, lutSource, cdf);

    // Each mip integrates the dome against the VNDF at that mip's roughness with the view direction pinned to the
    // normal — the split-sum approximation's declared simplification, stated here rather than discovered downstream.
    static Fin<Seq<TexturePlane>> Specular(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key) =>
        toSeq(Enumerable.Range(0, policy.Mips))
            .Map(mip => Level(map, policy, mip, budget, key))
            .Traverse(identity).As()
            .Map(static levels => levels.Strict());

    static Fin<TexturePlane> Level(EnvironmentMap map, IblPolicy policy, int mip, RenderBudget budget, Op key) {
        int edge = Math.Max(1, policy.SpecularEdge.Value >> mip);
        double sourceSolidAngle = 4.0 * Math.PI / (map.Plane.Width.Value * map.Plane.Height.Value * map.Plane.Layers.Value);
        return TexturePlane.Of(PlaneFormat.Rgba32F, Dimension.Create(edge * 2), Dimension.Create(edge),
                PlaneTransfer.Linear, AlphaMode.None, key)
            .Map(plane => {
                ParallelHelper.For(0, edge, new SpecularSweep(map, policy,
                    Microfacet.AlphaOf(policy.RoughnessAt(mip)), sourceSolidAngle, plane), budget.Floor);
                return plane;
            });
    }

    // Exemption: the tap loop is a measured kernel. The lod term is the filtered-importance-sampling mip selection —
    // log2 of the tap's own solid angle over one source texel's — so a bright pixel spreads instead of sparkling.
    // Every level lands EQUIRECT whatever the source arrangement, because a shading read addresses it by
    // direction and one storage arrangement for the products keeps the [05] read single.
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
                    LocalVector half = Microfacet.SampleVisibleNormal(LocalVector.Normal, alpha, alpha, u0, u1);
                    LocalVector light = half.Scale(2.0 * half.CosTheta).Add(LocalVector.Normal.Scale(-1.0)).Normalize();
                    if (light.CosTheta <= 0.0) { continue; }
                    double pdf = Math.Max(1e-6, Microfacet.VisibleNormalPdf(LocalVector.Normal, half, alpha, alpha) / (4.0 * Math.Abs(half.CosTheta)));
                    double lod = 0.5 * Math.Log2(Math.Max(1e-12, 1.0 / (policy.SpecularTaps * pdf) / sourceSolidAngle));
                    RgbSpectrum radiance = map.Stored(Oriented(n, light), Math.Max(0.0, lod));
                    (r, g, b, weight) = (r + (radiance.R * light.CosTheta), g + (radiance.G * light.CosTheta),
                                         b + (radiance.B * light.CosTheta), weight + light.CosTheta);
                }
                double norm = weight > 0.0 ? 1.0 / weight : 0.0;
                lanes[x] = new ShadeVec4(r * norm, g * norm, b * norm, 1.0);
            }
            plane.WriteShade(y, layer: 0, lanes);
        }

        // The ONE tangent-to-world crossing on this page: an orthonormal completion per texel rotates every
        // tangent-space Microfacet tap (a LocalVector by that kernel's own contract) onto the texel's WORLD
        // normal — the explicit basis rotation the WorldDirection/LocalVector split makes mandatory.
        static WorldDirection Oriented(WorldDirection n, LocalVector local) {
            WorldDirection up = Math.Abs(n.Z) < 0.999 ? WorldDirection.Zenith : new WorldDirection(1.0, 0.0, 0.0);
            WorldDirection t = new WorldDirection((up.Y * n.Z) - (up.Z * n.Y), (up.Z * n.X) - (up.X * n.Z), (up.X * n.Y) - (up.Y * n.X)).Normalize();
            WorldDirection b = new((n.Y * t.Z) - (n.Z * t.Y), (n.Z * t.X) - (n.X * t.Z), (n.X * t.Y) - (n.Y * t.X));
            return t.Scale(local.X).Add(b.Scale(local.Y)).Add(n.Scale(local.Z)).Normalize();
        }
    }

    // BrdfLut integrates a pure function of (N·V, roughness), so one plane serves every environment and re-keys only
    // on an IblPolicy change. R carries the Fresnel scale, G the bias — the two-component rg16 plane is the declared
    // storage, never an rgba plane wasting half its texels.
    static Fin<TexturePlane> BrdfLut(IblPolicy policy, RenderBudget budget, Op key) =>
        TexturePlane.Of(PlaneFormat.Rg16, policy.LutExtent, policy.LutExtent, PlaneTransfer.Raw, AlphaMode.None, key)
            .Map(plane => {
                ParallelHelper.For(0, policy.LutExtent.Value, new LutSweep(policy, plane), budget.Floor);
                return plane;
            });

    // Exemption: the split-sum integration kernel. Under the VNDF sampler the per-tap estimator COLLAPSES to
    // G₂/G₁(view) — the D·(V·H)/(N·H·N·V) Karis weight belongs to D-proportional half-vector sampling and pairing it
    // with SampleVisibleNormal double-counts the visible-normal density, biasing exactly the grazing band the LUT
    // exists to correct. Both Smith terms are the kernel's own, so the LUT and the shaded surface cannot drift; both
    // outputs land in [0,1] by construction, which is why the rg16 normalized depth carries them without a scale column.
    readonly struct LutSweep(IblPolicy policy, TexturePlane plane) : IAction {
        public void Invoke(int y) {
            int extent = plane.Width.Value;
            using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(extent, AllocationMode.Clear);
            Span<ShadeVec4> lanes = field.Span;
            double alpha = Microfacet.AlphaOf((y + 0.5) / extent);
            for (int x = 0; x < extent; x++) {
                double cosView = Math.Max(1e-3, (x + 0.5) / extent);
                LocalVector view = new(Math.Sqrt(Math.Max(0.0, 1.0 - (cosView * cosView))), 0.0, cosView);
                (double scale, double bias) = (0.0, 0.0);
                for (int i = 0; i < policy.LutTaps; i++) {
                    (double u0, double u1) = Deterministic.Hammersley(i, policy.LutTaps);
                    LocalVector half = Microfacet.SampleVisibleNormal(view, alpha, alpha, u0, u1);
                    LocalVector light = half.Scale(2.0 * view.Dot(half)).Add(view.Scale(-1.0)).Normalize();
                    if (light.CosTheta <= 0.0) { continue; }
                    double visibility = Microfacet.MaskingShadowing(view, light, alpha, alpha)
                                      / Math.Max(1e-6, Microfacet.Masking(view, alpha, alpha));
                    double fresnel = Math.Pow(1.0 - Math.Abs(view.Dot(half)), 5.0);
                    (scale, bias) = (scale + ((1.0 - fresnel) * visibility), bias + (fresnel * visibility));
                }
                lanes[x] = new ShadeVec4(scale / policy.LutTaps, bias / policy.LutTaps, 0.0, 1.0);
            }
            plane.WriteShade(y, layer: 0, lanes);
        }
    }

    // Guide builds the marginal + conditional luminance guide the LIGHT_RIG importance-samples, over the STORED frame.
    // Each row's mass weights by the layout's own solid angle, so a pole row carries its true vanishing measure and a
    // bright zenith is not over-sampled; the persisted plane packs the marginal into row h, which the 2:1 equirect law
    // guarantees fits. A non-equirect dome PROJECTS through the owner's own layout relation first rather than refusing
    // — the guide is one arrangement so its row packing and its draw stay single, and the projection is the relation
    // this page already holds, not a second conversion minted for the sampler.
    static Fin<LuminanceCdf> Guide(EnvironmentMap map, RenderBudget budget, Op key) =>
        map.Layout.Key == MapLayout.Equirect.Key
            ? Accumulate(map, budget, key)
            : map.Project(MapLayout.Equirect, map.Plane.Height, budget, key).Bind(projected => {
                  using (projected) { return Accumulate(projected, budget, key); }
              });

    // Accumulate partitions the per-row conditional pass — the rows are independent — and only the marginal prefix
    // over row masses runs in order, the one genuinely sequential step in either source-domain reduction.
    static Fin<LuminanceCdf> Accumulate(EnvironmentMap map, RenderBudget budget, Op key) {
        (int w, int h) = (map.Plane.Width.Value, map.Plane.Height.Value);
        double[] conditional = new double[h * w], marginal = new double[h];
        ParallelHelper.For(0, h, new GuideSweep(map, conditional, marginal, w, h), budget.Floor);
        double total = 0.0;
        for (int y = 0; y < h; y++) { total += marginal[y]; marginal[y] = total; }
        return total > 0.0
            ? Fin.Succ(new LuminanceCdf(conditional, marginal, total, w, h))
            : MaterialFault.Parameter(key, "<environment-zero-luminance>");
    }

    // Exemption: the running-mass accumulation is a measured kernel — the running form IS the searchable structure,
    // so a fold into a fresh array per row would allocate h times to produce the same monotone spine. The row mass
    // lands unaccumulated and the caller's single prefix pass turns the column into the marginal.
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

// ShGolden rows the frozen [08.3] fixtures as an EVALUATED roster, the PeriodGolden/Prove pattern: two analytic
// fields with exact projections, jointly discriminating band order, normalization, up-axis, and the convolution
// constants — a Y-up transcription lands the axial energy at sh_1/sh_3 and fails, a channel-major layout fails every
// slot, a wrong Â set fails the reconstruction probe. Prove also sums the layout measures against 4π, so the three
// SolidAngle closed forms carry a per-run numeric proof rather than a reader's trust.
public sealed record ShGolden(string Name, Func<WorldDirection, double> Radiance, int Band, double Expected, double IrradianceAtZenith) {
    // The band bar is the frozen 1e-6 absolute; the midpoint quadrature over the equirect grid meets it from 2048
    // rows up, and the reconstruction bar carries the convolution's own amplification of that quadrature error.
    public const double BandTolerance = 1e-6;
    public const double IrradianceTolerance = 1e-5;
    public const int ProofRows = 2048;

    public static readonly Seq<ShGolden> All = Seq(
        new ShGolden("uniform", static _ => 1.0, Band: 0, Expected: 3.5449077018110318, IrradianceAtZenith: Math.PI),
        new ShGolden("axial-cosine", static d => d.Z, Band: 2, Expected: 2.046653415892977, IrradianceAtZenith: 2.0 * Math.PI / 3.0));

    public static Fin<Unit> Prove(Op key) {
        foreach (MapLayout layout in MapLayout.Items) {
            double sum = MeasureSum(layout, ProofRows);
            if (Math.Abs(sum - (4.0 * Math.PI)) > 1e-4) {
                return MaterialFault.Parameter(key, $"<sh-golden-measure:{layout.Key}:{sum:R}>");
            }
        }
        foreach (ShGolden row in All) {
            Fin<Unit> verdict = row.Project(key);
            if (verdict.IsFail) { return verdict; }
        }
        return Fin.Succ(Unit.Default);
    }

    // Exemption: the proof sweeps are measured kernels — one projection per fixture row over the equirect grid at the
    // layout's own measure, then the nine-band gate, then the +Z reconstruction through the SAME Sh9 owner the wire
    // carries, so the fixture proves the shipping fold and never a private re-derivation.
    Fin<Unit> Project(Op key) {
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
            if (Math.Abs(bands[slot] - expected) > BandTolerance) {
                return MaterialFault.Parameter(key, $"<sh-golden-band:{Name}:{slot}:{bands[slot]:R}>");
            }
        }
        double[] interleaved = new double[Sh9.Slots];
        for (int slot = 0; slot < 9; slot++) {
            (interleaved[slot * 3], interleaved[(slot * 3) + 1], interleaved[(slot * 3) + 2]) = (bands[slot], bands[slot], bands[slot]);
        }
        return Sh9.Of(interleaved, key).Bind(sh =>
            Math.Abs(sh.Irradiance(WorldDirection.Zenith).R - IrradianceAtZenith) <= IrradianceTolerance
                ? Fin.Succ(Unit.Default)
                : (Fin<Unit>)MaterialFault.Parameter(key, $"<sh-golden-irradiance:{Name}:{sh.Irradiance(WorldDirection.Zenith).R:R}>"));
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

- Owner: `EnvironmentLight` the resolved row the render seam consumes, its admission gates, and the six reads it publishes.
- Entry: `public static Fin<EnvironmentLight> Of(string lightKey, EnvironmentMap map, IblProducts products, EnvironmentBlobs blobs, Option<SkyModel> sky, Op key)` admits the resolved row once; `Radiance(WorldDirection)`, `Irradiance(WorldDirection)`, `Sample(u0, u1)`, `Pdf(WorldDirection)`, `SpecularLevel(UnitInterval)`, and `SplitSum(cosView, roughness)` are the reads a path-trace integrator and a raster shading pass share — every direction-typed read takes the producer's own `WorldDirection`, so a tangent-frame query is a compile error rather than a frame-law violation.
- Law: the products are STORED-frame and every read applies the dome's rotation and intensity HERE. `Irradiance` un-rotates the queried normal before reconstructing the SH; the guided draw rotates the sampled direction into world; the density reads the STORED luminance the guide's own total was built from, so a re-exposed dome does not skew a multiple-importance weight by its own intensity factor. One read policy, applied at one altitude, over blobs no policy edit re-keys.
- Receipt: the row IS the receipt — every field of the frozen `EnvironmentLightWire` roster resolves here, so the wire projection at `interchange#TEXTURE_EGRESS` is a mechanical mirror with no derivation of its own. `SkyModelKey` carries the model key for a synthesized dome and empty for an ingested HDRI; `CoefficientKey` carries the Hosek-Wilkie asset digest, so a revised fit re-keys the light. `transfer` on that wire reads the SOURCE plane's declared transfer, which is why `pq` and `hlg` reach the wire while every value this row hands out is scene-linear.
- Boundary: `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` `LightSource.Environment` carries THIS row as its dome VALUE over the `[BOUNDARY]` seam — the render arm answers directional radiance, importance draw, SH irradiance, specular level, and split-sum on the owner that prefiltered the map, while Materials keeps the whole mapping, sampling, and prefilter algebra and the consumer re-derives no equirect correspondence, SH band order, or roughness ladder. `Sample` returns direction, radiance, and solid-angle density TOGETHER so a multiple-importance-sampling integrator balances against its BSDF density with no second query, and an absent CDF answers the uniform-dome density as a declared degradation the row states rather than a silent fallback.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// EnvironmentBlobs carries the four content addresses a resolved light holds. One carrier rather than four positional
// keys keeps the Of signature honest and makes a missing CDF a typed absence rather than an empty-string sentinel.
public readonly record struct EnvironmentBlobs(ContentAddress Equirect, ContentAddress Specular, ContentAddress BrdfLut, Option<ContentAddress> LuminanceCdf);

// One directional sample: the direction, its radiance, and its solid-angle density together, so a MIS integrator
// balances against its BSDF density with no second query into this row.
public readonly record struct EnvironmentSample(WorldDirection Direction, RgbSpectrum Radiance, double Pdf);

public sealed record EnvironmentLight(
    string LightKey, EnvironmentMap Map, IblProducts Products, EnvironmentBlobs Blobs,
    string SkyModelKey, Option<ContentAddress> CoefficientKey) {

    public static Fin<EnvironmentLight> Of(
        string lightKey, EnvironmentMap map, IblProducts products, EnvironmentBlobs blobs, Option<SkyModel> sky, Op key) =>
        from _ in guard(!string.IsNullOrWhiteSpace(lightKey), MaterialFault.Parameter(key, "<environment-light-key-blank>"))
        // The wire's equirectKey names an EQUIRECT plane and the freeze admits it 2:1: a cube or octahedral dome
        // projects through the map's own layout relation BEFORE this row resolves — the gate makes the conversion
        // step structural rather than a consumer discovering a square blob behind an equirect field name.
        from _layout in guard(map.Layout.Key == MapLayout.Equirect.Key,
                MaterialFault.Parameter(key, $"<environment-light-layout:{map.Layout.Key}>"))
        from __ in guard(products.RoughnessPerMip.Count == products.Specular.Count && products.Specular.Count > 0,
                MaterialFault.Parameter(key, $"<environment-level-ladder:{products.Specular.Count}!={products.RoughnessPerMip.Count}>"))
        from ___ in guard(products.RoughnessPerMip.Zip(products.RoughnessPerMip.Tail).ForAll(static pair => pair.Item1 <= pair.Item2),
                MaterialFault.Parameter(key, "<environment-roughness-ladder-unordered>"))
        from ____ in guard(products.Cdf.Map(static cdf => cdf.Total > 0.0).IfNone(true),
                MaterialFault.Parameter(key, "<environment-guide-zero-mass>"))
        select new EnvironmentLight(lightKey, map, products, blobs,
            sky.Map(static s => s.WireKey).IfNone(string.Empty),
            sky.Bind(static s => s is SkyModel.HosekWilkie fitted ? Some(fitted.Coefficients.Key) : None));

    public RgbSpectrum Radiance(WorldDirection direction) => Map.Radiance(direction, lod: 0.0);
    public double SpecularLevel(UnitInterval roughness) => Products.SpecularLevel(roughness);

    // Irradiance at a WORLD normal. The SH vector is stored-frame, so the normal un-rotates before reconstruction
    // and intensity scales after — the same two-step the directional read applies, on one owner, so a rotated dome
    // lights a surface correctly without a re-projection the blob key would then have to carry.
    public RgbSpectrum Irradiance(WorldDirection normal) =>
        Products.Irradiance.Irradiance(EnvironmentMap.Rotated(normal.Normalize(), -Map.Rotation)).Scale(Map.Intensity);

    // SplitSum answers the pair a shading pass multiplies its F0 by: X the Fresnel scale, Y the bias, read off the LUT the
    // prefilter integrated with the SAME Smith visibility the surface shades under, through the SAME sampler. That
    // LUT is environment-independent, so neither rotation nor intensity touches this read.
    public (double Scale, double Bias) SplitSum(UnitInterval cosView, UnitInterval roughness) =>
        TextureUv.Sample(Products.BrdfSource, new UvSample(cosView, roughness, Vector3d.Zero, Vector3d.ZAxis, 0.0), LutSampler, Map.Key)
            .Match(Succ: static f => f.IsFinite ? (f.X, f.Y) : (0.0, 0.0), Fail: static _ => (0.0, 0.0));

    static readonly SamplerState LutSampler = new(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Bilinear);

    // Importance sample the dome through the guide: the marginal picks the row, that row's conditional picks the
    // column, the texel lifts through the frozen equirect inverse, and the dome rotation carries it into world. The
    // density reads the STORED luminance the guide's own mass was built from, so intensity never skews a MIS weight.
    // With no guide the draw is the uniform sphere and the density SAYS SO — a declared degradation on the same
    // return shape, never a silent fallback.
    public EnvironmentSample Sample(UnitInterval u0, UnitInterval u1) =>
        Products.Cdf.Match(
            Some: guide => Guided(guide, u0, u1),
            None: () => Uniform(UniformDirection(u0, u1)));

    // Pdf answers the paired density every MIS weight reads: the guide's own texel density at the queried direction, or
    // uniform-sphere density when the guide is absent — one member, both arms, so a caller never branches on Option.
    public double Pdf(WorldDirection direction) =>
        Products.Cdf.Match(
            Some: guide => guide.Density(Map.Stored(EnvironmentMap.Rotated(direction.Normalize(), -Map.Rotation), lod: 0.0).Luminance),
            None: static () => 1.0 / (4.0 * Math.PI));

    // ONE guided draw. Every guide grid is equirect — a non-equirect dome projected before accumulation — so the drawn
    // texel lifts through the frozen equirect inverse and the radiance reads the SOURCE map BY DIRECTION, never by a
    // texel index the guide's grid and the map's grid would have to share. Stored is read once and serves both the
    // sample and its density, so answering one query never evaluates the field twice.
    EnvironmentSample Guided(LuminanceCdf guide, UnitInterval u0, UnitInterval u1) {
        (int x, int y) = guide.Draw(u0, u1);
        WorldDirection local = MapLayout.Equirect.Inverse(
            UnitInterval.Create((x + 0.5) / guide.Width), UnitInterval.Create((y + 0.5) / guide.Height), layer: 0);
        RgbSpectrum stored = Map.Stored(local, lod: 0.0);
        return new EnvironmentSample(
            EnvironmentMap.Rotated(local, Map.Rotation), stored.Scale(Map.Intensity), guide.Density(stored.Luminance));
    }

    EnvironmentSample Uniform(WorldDirection direction) =>
        new(direction, Radiance(direction), 1.0 / (4.0 * Math.PI));

    // UniformDirection draws area-preserving over the sphere: z is linear in u1 so the shell measure is uniform, which
    // a naive (theta, phi) grid draw is not — it clusters at the poles exactly where the equirect texels already do.
    static WorldDirection UniformDirection(UnitInterval u0, UnitInterval u1) {
        double z = 1.0 - (2.0 * u1.Value), r = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z))), phi = 2.0 * Math.PI * u0.Value;
        return new WorldDirection(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }
}
```

## [06]-[RESEARCH]

- [SKY_COEFFICIENT_EXTENTS]-[OPEN]: which channel, albedo-node, turbidity-node, Bezier-control-point, and term extents does the published Hosek-Wilkie fit carry, and does the radiance dataset share the config dataset's lattice so both read as term slots of one block?; verify against the fitted dataset the app-root import boundary supplies and bake the extents into the asset's declared columns.
- [SKY_ELEVATION_PARAMETER]-[OPEN]: is the fit's solar-elevation Bezier parameter the cube root of the normalized solar altitude, and does it measure from the horizon or the zenith?; verify against the published model's own parameterization and correct the one `Math.Cbrt` expression in the fitted arm.
- [CIE_SKY_TABLE]-[OPEN]: do the six gradation `(a, b)` rows, the six indicatrix `(c, d, e)` rows, and the fifteen type-to-group bindings match ISO 15469:2004 / CIE S 011 Table 1 exactly?; verify against the standard's own table before the rows leave design.
- [SOLAR_ALMANAC_BAND]-[OPEN]: over which date span does the low-precision almanac fold hold the arc-minute accuracy a daylight study needs, and does the equation-of-time expansion need its higher-order terms inside that span?; verify against a reference ephemeris at the span's endpoints.
