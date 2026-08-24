# [MATERIALS_ENVIRONMENT]

THE SKY, ENVIRONMENT-MAP, AND IMAGE-BASED-LIGHTING OWNER. One `SkyModel` `[Union]` synthesizes a scene-linear radiance field from an analytic daylight model — the Hosek-Wilkie fitted-coefficient asset over its solar-elevation Bézier lattice and the ISO 15469 fifteen-type CIE standard sky over one gradation × indicatrix algebra — one `EnvironmentMap` admits any equirect, cube-face, or octahedral layout under the frozen `+Z`-up correspondence and carries the whole layout relation in one `Project` fold, one `IblPrefilter.Prefilter` reduces an admitted map to the `IblProducts` receipt every renderer consumes (SH9 irradiance, the GGX roughness-ordered specular level set, split-sum BRDF LUT, marginal-conditional luminance CDF), and one `EnvironmentLight` record is the row `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` resolves over the declared `[BOUNDARY]` seam — directional radiance, importance sample, SH irradiance, specular level, and split-sum read all answering on that one owner so no consumer re-derives the mapping. `SkyModel` admits a sky variation as one case and `CieSkyType` as one ROW, `MapLayout` a storage arrangement as one ROW, and `IblProducts` a prefilter product as one COLUMN — never a per-model sky type, a direction-named converter pair, or a second SH spelling. Every owner here composes the `bsdf#MICROFACET_KERNEL` `Microfacet` VNDF sampler and Smith masking for every GGX integral (the prefiltered dome and the shaded surface integrate the SAME distribution), the `bsdf#SHADING_FRAME` `LocalVector<T>`/`RgbSpectrum`/`MaterialFault` band-2450 rail, the `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg working space, the `photometric#PHOTOMETRIC` unit gate for an authored zenith luminance, the `Raster/plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid` typed-texel arena with its `Read`/`Write` row rails and `AsImage` sampler bridge, the `texture#TEXTURE_UV` sampler for every filtered read, the seam `Rasm.Element` `ContentAddress` for every blob key, the kernel `Dimension`/`UnitInterval`/`Op` atoms with `CommunityToolkit.HighPerformance` struct-action partitioning, and NodaTime `Instant` for solar position — re-minting no plane, no colour space, no fault, and no hash. Solar geometry composes the kernel `Rasm/Numerics/calculus#SOLAR_EPHEMERIS` almanac and projects HERE into the frozen frame, so no host sun object and no second ephemeris crosses the host-neutral boundary; the container decode of an ingested HDRI is `Raster/codec#RASTER_CODEC`, this owner consuming the decoded plane alone.

## [01]-[INDEX]

- [02]-[SKY_MODEL]: `CieGradation`/`CieIndicatrix`/`CieSkyType` close the standard-sky algebra, `SkyCoefficients` and `SolarCoefficients` carry the two content-keyed fitted assets over one `ControlLattice` Bézier algebra, `SolarFrame` projects the kernel almanac's geodetic sun into the frozen frame, `SkyModel` `[Union]` states the radiance law with its ground hemisphere and its solar disc, and `SkyRender.Radiance` supplies the per-texel radiance closure the press subject sweeps.
- [03]-[ENVIRONMENT_MAP]: `MapLayout` bands storage with its per-row coordinate law, `Equirectangular` freezes the correspondence, `EnvironmentMap.Of` admits with its per-layer sampler lift, `Stored`/`Radiance` answer the stored-frame and world-frame reads, and `Project` carries the one layout relation in both directions.
- [04]-[IBL_PREFILTER]: `ShBand` tables the nine-row basis, `Sh9` pairs projection with irradiance reconstruction, `IblPrefilter` integrates the GGX specular level set, the split-sum BRDF LUT, and the luminance CDF over the kernel `Deterministic.Hammersley` draw, `IblProducts` carries the CPU receipt, and `IblProduct` splits the content-addressable mint from the accelerator lane's key-less preview.
- [05]-[ENVIRONMENT_LIGHT]: `EnvironmentLight` resolves the row the render seam consumes, gates its own admission, and publishes the six reads an integrator and a raster shading pass share.

## [02]-[SKY_MODEL]

- Owner: `WorldDirection` the page-owned `+Z`-up WORLD direction carrier every dome surface speaks (structurally distinct from the `bsdf#SHADING_FRAME` tangent `LocalVector<T>`); `SkyModel` `[Union]` (`HosekWilkie` · `CieStandard`); `SkyCoefficients` and `SolarCoefficients` the two content-keyed fitted assets over the one `ControlLattice` interpolation algebra; `SolarDisc` the resolved direct-beam term; `CieSkyType`/`CieGradation`/`CieIndicatrix` `[SmartEnum<int>]` bands; `SolarFrame` the frame projection over the kernel `Numerics/calculus#SOLAR_EPHEMERIS` almanac; `SkyAtmosphere` the turbidity, ground-albedo, admitted-zenith-level, exposure, and solar-angular-diameter row; `SkyRender` the radiance-closure surface; `SkySpectrum` the one band→scene colour path both fitted assets cross.
- Cases: sky {`HosekWilkie` (the fitted anisotropic-Mie daylight model over a `SkyCoefficients` diffuse asset paired with its `SolarCoefficients` limb-darkened disc asset), `CieStandard` (the ISO 15469 relative-luminance distribution over a `CieSkyType` row)}; gradation {`I`…`VI`}; indicatrix {`One`…`Six`}; sky-type {`Type01`…`Type15`, each binding one gradation and one indicatrix — `Type01` the CIE Overcast Sky, `Type12` the CIE Standard Clear Sky}.
- Entry: `public static Func<Vector3d, RgbSpectrum> Radiance(SkyModel model, SkyAtmosphere atmosphere, WorldDirection sun)` is the ONE synthesis surface — the per-texel radiance closure `Raster/press#PRESS_PLAN` `PressSubject.Sky` calls under `PressProgram.Dome`, so the sky owner supplies the model and the press owns partitioning, cancellation, the receipt, and the accelerator lane; `SolarFrame.Of(latitudeDegrees, longitudeDegrees, instant, key, elevationM)` is the ONE sun-direction entry, so a caller holding a measured direction passes it and a caller holding a site and a clock resolves it here; `SkyModel.Disc(WorldDirection sun, SkyAtmosphere atmosphere)` is the ONE direct-beam read both cases answer. `MaterialFault` rails a sub-unit or super-decade turbidity, a negative zenith level, a non-positive exposure, an out-of-band solar diameter, an out-of-range site, and a non-finite radiance.
- Law: radiance covers the WHOLE sphere. Each model distributes its own radiance over the upper hemisphere; the lower hemisphere is the GROUND — `GroundAlbedo` times the model's horizon radiance, evaluated once per texel through the same case — so a synthesized dome carries a real bounce rather than the mirrored bright band a clamped zenith cosine produces below the horizon. `GroundAlbedo` reaches that ground term as the same `RgbSpectrum` the Hosek-Wilkie fit consumes as its albedo axis, so one authored value drives both the sky's own inter-reflection and the dome's lower half.
- Law: the SYNTHESIZED FIELD carries the sky alone and the DISC rides its own term. A half-degree source four decades brighter than the sky around it lands in one texel of a bounded dome, so writing it into the plane makes the `[04]` guide's texel measure the only structure importance-sampling it — a firefly no tap budget resolves and a quadrature error the SH projection carries forever. `Radiance` is therefore the diffuse field at every direction and `Disc` the direct beam the `[05]` row publishes as its own arm, which is what lets one dome serve a raster read and a path-traced draw without double-counting the sun.
- Law: the MEASURED-KERNEL carve-out is declared ONCE here and nowhere per site — the `readonly struct` `IAction` row sweeps writing into the plane owner's `Write` rail by row index (the carve `texture#TEXTURE_UV` `ProceduralNoise` also names), the band and Bernstein folds over a fitted block, the type-init basis reconstruction, the bounded disc quadrature, the running-mass guide and its bisection, and the fixture proof's own grid pass. Every other operation is expression-bodied and rail-threaded, so a loop outside that carve is a defect readable against this one bullet rather than against a comment at each site.
- Law: radiance leaves a model in scene-linear AP1 channels at `PlaneTransfer.Linear`, so a display transfer never enters a SYNTHESIZED plane and the tone map stays `surface#TONE_MAP`'s. Every channel folds through `Finite.Spectrum` before `RgbSpectrum.Create`, because the validated carrier's own admission THROWS inside a partitioned sweep no rail covers.
- Law: every published tolerance on this page is a kernel `Domain/context` `Tolerance` on a NAMED lane, admitted once at its own entry — the SH band bar on `ToleranceLane.Spectral`, the irradiance reconstruction bar on `ToleranceLane.Irradiance`, the solid-angle closure bar on `ToleranceLane.Conservation` — so a project tightening the dome proof reaches all three through `Context.Override` and none is a page constant a consumer cannot move.
- Packages: Wacton.Unicolour (composed at the consuming edge — the scene-linear basis is `graph#MATERIAL_GRAPH` `PortValue.SceneLinear`, and a chromatic sky lands as an admitted `photometric#PHOTOMETRIC` `EmissionInput` tint, never a re-minted illuminant), NodaTime (`Instant` the fold's clock carrier and `Offset` the site's zone — a `DateTime` with an inferred kind is the fabricated-instant defect), CommunityToolkit.HighPerformance (`SpanOwner<T>` per-row scratch, `ParallelHelper.For` over a struct `IAction` row), Rasm (project — `Dimension`/`UnitInterval`/`Op`, `Tolerance`/`ToleranceLane`, `Deterministic.Hammersley`, and the `Numerics/calculus#SOLAR_EPHEMERIS` `SolarPosition.At`/`SolarSite`/`SunPosition` almanac), Rasm.Element (project — `ContentAddress`), Rasm.Materials.Appearance.Bsdf (`LocalVector<T>`/`RgbSpectrum`/`MaterialFault`/`Microfacet<T>`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new standard sky is one `CieSkyType` row over the existing group pair — the fifteen admitted types PROJECT the six × six product, so a national-annex sky is a row and never a case; a new gradation or indicatrix is one row on its own band; a genuinely new analytic radiance law is one `SkyModel` case; a new atmospheric parameter is one `SkyAtmosphere` column; a new tolerance gate is one `ToleranceLane` row read here.
- Growth: `SkyCoefficients` and `SolarCoefficients` are TWO CONTENT-KEYED DATA ASSETS, this page's one carve from the generated-table law (branch RULINGS: a published table with no defining sequence stays a content-keyed asset) — both are least-squares fits over a brute-force simulation, so each `Of` admits a caller-supplied block against its own declared extents and a revised fit is a NEW digest. They stay TWO because their lattices differ in RANK: the diffuse fit carries a ground-albedo axis the disc fit has no term for and the disc fit a limb-darkening axis the diffuse fit has no direction for, so one extent gate over a merged block admits either array in the other's slot. `ControlLattice` owns the interpolation ONCE — the Bernstein weights generate from the binomial at the declared degree — so a third fitted asset is a record with its own extents and no second derivation.
- Boundary: solar position resolves the apparent refraction-corrected topocentric direction in the frozen `+Z`-up frame — `+X` north, `+Y` west, azimuth FROM `+X` increasing EASTWARD onto `−Y`, the OPPOSITE angular sense of the `[03]` equirect `u`. The fold carries that sign exactly once (`−sin(azimuth)` on the `Y` lane), so the two conventions meet in the direction VALUE and never share an angular sense a transcriber could copy wrongly. Geodetic datum, site CRS, and reprojection stay the app-root edge's; this owner takes latitude, longitude, and site elevation as admitted scalars and pins the site zone at `Offset.Zero`, the almanac's true-solar-minutes fold cancelling it. Site HEIGHT is a real axis: the almanac corrects Bennett refraction by the barometric ratio at the site's own height, so a hardcoded sea level answers every alpine study at the wrong horizon band.
- Boundary: every authored light magnitude crosses `photometric#PHOTOMETRIC` `Photometric.Admit` — `SkyAtmosphere.Of` for the zenith level and `EnvironmentMap.Of` for the dome intensity — so a `cd/m²` sky and a `lux` sky reach one radiometric scalar with no page-local efficacy divide, and each row carries the whole `EmissionEvidence` receipt rather than a bare scalar. Ground albedo enters as an `RgbSpectrum`, so a spectrally tinted bounce is representable and a scalar albedo is the grey triple. SOLAR ANGULAR DIAMETER is an admitted column with NO page default: the disc a study wants is the site's own apparent diameter at its own date, and a transcribed mean ships one epoch's astronomy as this owner's law. The `CieStandard` disc is its own indicatrix at zero angular distance, so the ratio distribution and its direct beam are ONE algebra.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
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

// Folder-root namespace beside acquisition#ACQUISITION and finish#FINISH: an `Environment` sub-namespace captures
// System.Environment inside every declaration under it (the colour-colour naming trap), so the owner keeps the
// folder-root seat and the eventual source file is Appearance/Environment.cs.
namespace Rasm.Materials.Appearance;

// --- [TYPES] -------------------------------------------------------------------------------
// The two frames share axis labels and nothing else, and one type serving both admitted a surface tangent-frame
// vector into a dome read as a silent re-lighting no gate could see. The one legal crossing is an explicit basis
// rotation (the specular sweep's Oriented completion here; the consumer's own OracleFrame transform at the render
// seam). CosZenith reads the +Z zenith cosine every sky and measure law consumes; Zenith is the
// degenerate-normalize floor, matching LocalVector<T>'s own convention.
public readonly record struct WorldDirection(double X, double Y, double Z) {
    public double CosZenith => Z;
    public double Dot(WorldDirection o) => (X * o.X) + (Y * o.Y) + (Z * o.Z);
    public WorldDirection Add(WorldDirection o) => new(X + o.X, Y + o.Y, Z + o.Z);
    public WorldDirection Scale(double s) => new(X * s, Y * s, Z * s);

    public WorldDirection Normalize() {
        double n = Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        return n > 1e-12 ? new(X / n, Y / n, Z / n) : Zenith;
    }

    // Oriented completes an orthonormal basis on THIS direction and rotates a frame-local triple onto it — the one
    // legal tangent-to-world crossing the WorldDirection/LocalVector<T> split makes explicit, declared once so the
    // specular sweep's VNDF tap and the solar cap draw share one completion rather than two that could disagree.
    public WorldDirection Oriented(double x, double y, double z) {
        WorldDirection up = Math.Abs(Z) < 0.999 ? Zenith : new WorldDirection(1.0, 0.0, 0.0);
        WorldDirection t = new WorldDirection((up.Y * Z) - (up.Z * Y), (up.Z * X) - (up.X * Z), (up.X * Y) - (up.Y * X)).Normalize();
        WorldDirection b = new((Y * t.Z) - (Z * t.Y), (Z * t.X) - (X * t.Z), (X * t.Y) - (Y * t.X));
        return t.Scale(x).Add(b.Scale(y)).Add(Scale(z)).Normalize();
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

// Type05 is the sky of uniform luminance and Type13 the polluted-atmosphere twin of Type12 on the SAME gradation.
// Key IS the standard's type number and the wire spelling is
// `cie-standard-NN`, so the whole family crosses on ONE EnvironmentLight field and a reader resolves the row from the
// key alone. The fifteen bindings
// are SELECTED by the standard, not generated: they walk the product monotonically but skip most of its thirty-six
// cells, so the pairs are data the standard fixes and a binding derived from the key by arithmetic is fiction —
// which is exactly how a transposed tail (a clear sky reading its neighbour's gradation) survives every gate that
// only checks the groups themselves.
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

    // Relative answers the standard's L(sample) / L_zenith: the numerator reads the sample's zenith cosine and its
    // angular distance to the sun, the denominator the same pair evaluated at the zenith, so the quotient carries the
    // distribution alone and the absolute level rides SkyAtmosphere.ZenithRadiance.
    public double Relative(double cosZenith, double chi, double solarZenith) =>
        Indicatrix.F(chi) * Gradation.Phi(cosZenith) / (Indicatrix.F(solarZenith) * Gradation.PhiZenith);
}

// Layers binds the LayerLaw a TextureSet row declares, so a cube map is six layers under CubeFaces and never a
// six-plane sibling type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MapLayout {
    // Sampler is a ROW column because addressing is a property of the ARRANGEMENT: equirect wraps U across the
    // azimuth seam and clamps V at the poles; a cube face clamps BOTH axes — a Repeat there wraps a face-edge tap
    // onto the opposite edge of the SAME face, which is never the adjacent face the sphere continues onto, so the
    // face-seam blend is the projection relation's concern and the sampler stays face-local; the octahedral fold is
    // continuous across its diagonal by construction and clamps its outer border. Every row's UvFrame is IDENTITY
    // and stays so: a dome is addressed BY DIRECTION through the row's own correspondence, so a bind-time UV
    // transform here would slide the mapping out from under the prefilter products that integrated it.
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
// Turbidity is the Linke coefficient the Hosek-Wilkie fit is parameterized on; Exposure the scene-linear multiplier
// a caller re-applies without re-rendering; SolarDiameter the APPARENT angular diameter of the disc in RADIANS.
// BandAlbedo is the authored GroundAlbedo resolved onto the fitted assets' own eleven-band grid, DERIVED once here
// because the value is constant over a whole dome and the fit indexes it per band per texel — the RgbSpectrum stays
// the authored column the folder law names, and this is its spectral projection rather than a second parameter.
// Carrying the ZenithLevel receipt whole rather than its radiometric field alone is what lets the light row state
// WHICH unit a dome was authored in on the wire it mirrors.
public readonly record struct SkyAtmosphere(
    double Turbidity, RgbSpectrum GroundAlbedo, ReadOnlyMemory<double> BandAlbedo,
    EmissionEvidence ZenithLevel, double Exposure, double SolarDiameter) {
    public double ZenithRadiance => ZenithLevel.RadiometricSi;
    // The upper bound is the geometry the disc algebra stays valid under — a source subtending more than a twentieth
    // of the hemisphere is an area light, not a disc.
    public const double SolarDiameterCeiling = Math.PI / 36.0;

    // HalfAngleCosine is the disc membership test every direct-beam read shares: an angular distance whose cosine
    // meets it lies ON the disc. One derivation, so the render, the light row, and the density never disagree about
    // where the sun ends.
    public double HalfAngleCosine => Math.Cos(SolarDiameter / 2.0);

    // SolidAngle is the disc's own measure, the spherical cap 2pi(1 - cos(half-angle)) — the density denominator a
    // uniform disc draw divides by and the factor a radiance-to-power reduction multiplies through.
    public double SolidAngle => 2.0 * Math.PI * (1.0 - HalfAngleCosine);

    // A dimensionless authoring passes PhotometricQuantity.Radiance, whose Borrowed coercion leaves
    // RadiometricSi == Measure.CanonicalValue, so the unitless case costs one construction and no branch downstream.
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

// RenderBudget carries the partition budget every sweep on this page reads: ParallelFloor bounds per-thread work off the
// caller's own policy rather than a page literal, Bands sets the governance granularity the same way, and Parallel is
// the arming column a benchmark receipt flips.
// GOVERNANCE rides the SAME row rather than a parameter tail, because every long entry on this page already threads
// the budget and none of them threaded a token: a layout reprojection and an IBL prefilter each sweep
// a whole environment field, and widening those signatures with two tails is the form the corpus deletes. The value
// is the folder's ONE filter#PLANE_OP BakeGovernance carrier, so the sky dome, the texture press, and the plane
// transform chain report on one shape and a caller composes one sink across all three. Default is INERT — an
// ungoverned sweep pays one struct copy — and Governed is the seat an ambient-effect caller destructures into.
public readonly record struct RenderBudget(int ParallelFloor, int Bands, bool Parallel, BakeGovernance Governance = default) {
    public static readonly RenderBudget Default = new(ParallelFloor: 32, Bands: 16, Parallel: true);
    public int Floor => Parallel ? Math.Max(1, ParallelFloor) : int.MaxValue;
    public RenderBudget Governed(Option<IProgress<double>> progress, CancellationToken cancel) =>
        this with { Governance = Governance.Governed(progress, cancel) };
    // The publish-and-check seam every sweep opens its level, face, or band group on. The unit is the sweep's own
    // outer step — a mip level, a cube face, a CDF row block — because that is the boundary whose count the entry
    // already knows; a per-texel report over a sixteen-million-texel field publishes one number sixteen million times.
    public Option<Error> Opened(int done, int total) =>
        Governance.Opened(total <= 0 ? 1.0 : done / (double)total);

    // Sweep is the ONE governed partition every whole-field entry on this page runs — the layout reprojection, the SH
    // projection, the luminance guide, the split-sum LUT, and each specular level (the dome synthesis sweeps on the
    // press engine instead, which is why no sky sweep appears beside these). The stacked
    // index space splits into the CALLER'S declared band count, each band partitioning through ParallelHelper.For and
    // each band boundary opening the publish-and-check seam, so a dome sweep is watchable and abortable at a
    // granularity no page literal fixes and an abandoned run stops at a band edge rather than after the whole field.
    // A raw ParallelHelper.For beside this member is the ungoverned form: it takes the budget and reports nothing,
    // which is a signature claiming governance the body never honours.
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

// Lattice axes run (channel,
// albedo node, turbidity node, Bezier control point, term) — the SOLAR-ELEVATION axis is the control-point dimension,
// which is why a lookup takes an elevation parameter rather than a control-point index a caller would have to know
// how to choose. The published fit's own extents are 10 turbidity nodes (integer 1..10), 2 albedo nodes (0 and 1),
// 6 quintic control points, 9 configuration terms, and 1 radiance term, over the SkySpectrum eleven-band grid — the
// tristimulus block is refused here rather than admitted and re-based, because it is fitted against sRGB primaries
// no column declares and its own solar counterpart was never published in tristimulus form at all, so admitting it
// would put a dome and its sun on two colour paths. The configuration and radiance datasets ride the IDENTICAL (albedo, turbidity, control point) lattice in the
// identical order, which is what lets them read as term slots 0..8 and 9 of ONE block; they ship as SEPARATE
// arrays, so admitting them means INTERLEAVING each radiance value after its own nine-term group, never concatenating the
// two arrays — a concatenated block admits the extent gate cleanly and then reads every radiance level as a
// configuration term of the wrong cell.
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

    // ONE read: bilinear over the (albedo, turbidity) control lattice, Bernstein over the solar-elevation control
    // points, at one channel and one term. The fit is piecewise over its own nodes and polynomial over its own
    // degree, so both interpolations are the DATASET's contract and neither is a caller policy.
    public double Term(int channel, double albedo, double turbidity, double elevation, int term) {
        ReadOnlySpan<double> block = Fitted.Span;
        (int a0, double at) = ControlLattice.Node(albedo, AlbedoNodes);
        (int t0, double tt) = ControlLattice.Node((turbidity - 1.0) / 9.0, TurbidityNodes);
        (int a1, int t1) = (Math.Min(a0 + 1, AlbedoNodes - 1), Math.Min(t0 + 1, TurbidityNodes - 1));
        double lo = ControlLattice.Lerp(Curve(block, channel, a0, t0, elevation, term), Curve(block, channel, a0, t1, elevation, term), tt);
        double hi = ControlLattice.Lerp(Curve(block, channel, a1, t0, elevation, term), Curve(block, channel, a1, t1, elevation, term), tt);
        return ControlLattice.Lerp(lo, hi, at);
    }

    // Terms IS the control-point stride: the elevation axis is the next-outer lattice dimension, so consecutive
    // control points sit one term-block apart and the Bernstein fold needs the first index and that step alone.
    double Curve(ReadOnlySpan<double> block, int channel, int albedo, int turbidity, double elevation, int term) =>
        ControlLattice.Bezier(block, At(channel, albedo, turbidity, controlPoint: 0, term), Terms, ControlPoints, elevation);

    int At(int channel, int albedo, int turbidity, int controlPoint, int term) =>
        ((((((channel * AlbedoNodes) + albedo) * TurbidityNodes) + turbidity) * ControlPoints) + controlPoint) * Terms + term;
}

// A disc's own emission does not read the ground it lights, so this fit carries no ground-albedo axis; a
// concatenated block would read every limb coefficient as a configuration term.
// Lattice axes run (channel, turbidity node, Bezier control point, limb term) — the solar-elevation axis is again the
// control-point dimension, so a lookup takes an elevation parameter rather than an index a caller would have to
// choose, and the limb terms are the polynomial in the cosine of the emission angle across the visible disc.
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

    // ONE read: the turbidity lattice interpolates, the elevation control points fold through Bernstein per limb
    // term, and the limb polynomial evaluates by Horner in the EMISSION-ANGLE COSINE — the disc-relative radius
    // enters as sqrt(1 − r²), which is that cosine on a sphere, so the centre reads the full polynomial and the limb
    // reads its constant term. Feeding the radius itself would darken the CENTRE and brighten the rim, which is the
    // inversion a plausible-looking disc hides.
    public double Radiance(int channel, double turbidity, double elevation, UnitInterval discRadius) {
        ReadOnlySpan<double> block = Fitted.Span;
        (int t0, double tt) = ControlLattice.Node((turbidity - 1.0) / 9.0, TurbidityNodes);
        int t1 = Math.Min(t0 + 1, TurbidityNodes - 1);
        double mu = Math.Sqrt(Math.Max(0.0, 1.0 - (discRadius.Value * discRadius.Value))), sum = 0.0;
        for (int limb = LimbTerms - 1; limb >= 0; limb--) {
            double lo = ControlLattice.Bezier(block, At(channel, t0, controlPoint: 0, limb), LimbTerms, ControlPoints, elevation);
            double hi = ControlLattice.Bezier(block, At(channel, t1, controlPoint: 0, limb), LimbTerms, ControlPoints, elevation);
            sum = (sum * mu) + ControlLattice.Lerp(lo, hi, tt);
        }
        return sum;
    }

    int At(int channel, int turbidity, int controlPoint, int limb) =>
        ((((channel * TurbidityNodes) + turbidity) * ControlPoints) + controlPoint) * LimbTerms + limb;
}

// Node and Lerp are the piecewise half; Bezier is the polynomial half over an axis addressed by a first index and a
// stride, which is what lets one fold serve two lattices of different rank.
internal static class ControlLattice {
    // Bernstein weights GENERATE from the binomial at the declared degree through the recurrence
    // w(i+1) = w(i)·(n−i)/(i+1)·s/(1−s), so a fit shipped at a different control-point count evaluates without a
    // transcribed row. The parameter clamps a hair below one because the recurrence divides by (1 − s): at the exact
    // endpoint every weight collapses to zero rather than to the terminal control point — a zenith sun reading a
    // black sky.
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

// SolarDisc is the resolved DIRECT BEAM every case answers: the sun's own direction, the spherical cap it subtends,
// and the radiance profile across that cap. Limb is a ROW DELEGATE because the two cases genuinely differ in what
// they publish — the Hosek-Wilkie fit carries a limb-darkening polynomial over the disc, the CIE standard sky carries
// the one circumsolar value its indicatrix already implies — so a caller reads ONE profile and no draw branches on
// the model. Mean is the area-weighted disc average the [05] row publishes to the render seam and the [04] power
// split reads; reading the centre as the whole disc over-states a limb-darkened sun by its own darkening ratio.
public sealed record SolarDisc(
    WorldDirection Direction, double CosHalfAngle, double SolidAngle, Func<UnitInterval, RgbSpectrum> Limb) {
    // Limb darkening is a low-degree polynomial in the emission-angle cosine, so a midpoint rule over the cap's own
    // linear-in-radius area measure resolves it well past the precision any radiance consumer reads, and the node
    // count is a declared extent rather than a convergence target.
    private const int MeanNodes = 32;

    public RgbSpectrum Centre => Limb(UnitInterval.Create(0.0));
    public RgbSpectrum Mean { get; } = Average(Limb);

    public bool Contains(WorldDirection direction) => direction.Normalize().Dot(Direction) >= CosHalfAngle;

    // Radius answers WHERE on the disc a direction lands, so the profile reads one parameter rather than an angle
    // every caller converts. Outside the cap the answer saturates at the rim, which is what keeps the profile total.
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

    // The published fit's own BLOCK ORDER, named by the ROLE each term plays in the expansion rather than by the
    // paper's alphabetic letters — because the two DISAGREE at the tail. The dataset stores the Mie DIRECTIONALITY
    // in the ninth slot and the zenith-gradient term in the eighth, while the paper's A..I naming puts the
    // directionality eighth; binding the letters in order therefore transposes the two and produces a plausible sky
    // with an inverted circumsolar band and a mis-scaled horizon, which no extent gate and no finiteness gate can
    // see. Role names make that binding checkable at the read. Term COUNT is the asset's declared extent, so a fit
    // shipped with more terms admits and the extra slots go unread by this expansion.
    private const int TermGradationScale = 0, TermGradationExponent = 1, TermConstant = 2,
        TermForwardScatter = 3, TermForwardDecay = 4, TermRayleigh = 5,
        TermMieScale = 6, TermZenith = 7, TermMieDirectionality = 8, TermRadiance = 9;

    // Cie is the CIE arm's ONE mint and the producer the chromatic escape needed: the standard sky publishes a
    // LUMINANCE ratio, so a coloured overcast or a warm low sun reaches it as an ADMITTED photometric#PHOTOMETRIC
    // EmissionInput whose Radiance is unit-Y chromaticity by construction — the tint is therefore a hue with the
    // energy left on ZenithRadiance where the ratio distributes it, and every EmissionSpectrum arm (blackbody,
    // standard illuminant, datasheet chromaticity, measured SPD, constant colour) reaches this case through the one
    // admission rather than through a colour path minted here. Absent chroma is the achromatic sky at unit tint,
    // the degenerate case of the same algebra rather than a second construction.
    // TOTAL: both arms succeed, so no Fin envelope and no Op key — a rail with no failure to carry correlates
    // nothing. The tint crosses the page's own Finite.Spectrum gate rather than a bare Math.Max, because
    // Math.Max(0.0, NaN) is NaN and the validated mint would take it.
    public static SkyModel Cie(CieSkyType type, Option<EmissionInput> chroma) =>
        new CieStandard(type, chroma
            .Map(static input => input.Radiance.RgbLinear.Triplet switch {
                var t => Finite.Spectrum([t.First, t.Second, t.Third]),
            })
            .IfNone(RgbSpectrum.White));

    public string WireKey => Switch(
        hosekWilkie: static _ => "hosek-wilkie",
        cieStandard: static c => c.Type.WireKey);

    // Disc resolves the direct beam BOTH cases answer, on the atmosphere's own admitted angular diameter. The
    // Hosek-Wilkie arm reads its solar asset across the disc; the CIE arm evaluates its own distribution at zero
    // angular distance from the sun — the circumsolar peak the indicatrix group already carries — so the standard
    // sky's ratio law and its direct beam are ONE algebra and no second solar model enters beside it.
    public SolarDisc Disc(WorldDirection sun, SkyAtmosphere atmosphere) => Switch(
        state: (Sun: sun.Normalize(), Atmosphere: atmosphere),
        hosekWilkie: static (s, h) => new SolarDisc(s.Sun, s.Atmosphere.HalfAngleCosine, s.Atmosphere.SolidAngle,
            radius => Limb(h.Solar, s.Sun, s.Atmosphere, radius)),
        cieStandard: static (s, c) => Circumsolar(c, s.Sun, s.Atmosphere) switch {
            var beam => new SolarDisc(s.Sun, s.Atmosphere.HalfAngleCosine, s.Atmosphere.SolidAngle, _ => beam),
        });

    // Both arms are pure over (direction, sun, atmosphere) and the render fold owns the sweep, so a per-model
    // entrypoint cannot exist.
    internal RgbSpectrum Radiance(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) =>
        direction.CosZenith >= 0.0
            ? Sky(direction, sun, atmosphere)
            : Ground(sun, atmosphere);

    RgbSpectrum Sky(WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) => Switch(
        state: (Direction: direction, Sun: sun, Atmosphere: atmosphere),
        hosekWilkie: static (s, h) => Fitted(h.Coefficients, s.Direction, s.Sun, s.Atmosphere),
        cieStandard: static (s, c) => Standard(c, s.Direction, s.Sun, s.Atmosphere));

    // Ground reads the model's own radiance at the horizon on the sun's azimuth and multiplies by the spectral
    // albedo: one evaluation, both cases, no second lighting model and no flat authored constant. A zenith sun has
    // no azimuth — its horizon probe seats on +X rather than normalizing a zero vector.
    RgbSpectrum Ground(WorldDirection sun, SkyAtmosphere atmosphere) {
        WorldDirection horizon = Math.Abs(sun.X) + Math.Abs(sun.Y) > 1e-12
            ? new WorldDirection(sun.X, sun.Y, 0.0).Normalize()
            : new WorldDirection(1.0, 0.0, 0.0);
        return Sky(horizon, sun, atmosphere).Mul(atmosphere.GroundAlbedo);
    }

    // Nine configuration terms plus the radiance term per channel over the fitted lattice, with the anisotropic Mie phase term carried by the h coefficient. The Mie
    // numerator is (1 + cos²γ) and the gradation exponent divides by (cos θ + 0.01) — the published model's own
    // horizon offset, never a clamp standing in for it. GroundAlbedo reads PER CHANNEL and the solar elevation drives
    // the Bezier axis, so a spectrally tinted bounce reaches each channel independently and a low sun reads its own
    // fitted configuration rather than the zenith's. Elevation is the fit's own cube-root parameterization and it
    // measures FROM THE HORIZON: `asin` of the zenith cosine IS the altitude above the horizon, normalized by pi/2
    // and cube-rooted, because the distribution changes abruptly at low sun and the cube root spreads those changes
    // evenly along the control lattice. Reading that parameter from the ZENITH inverts the axis and answers every
    // low sun with the zenith's own fitted configuration — plausible output, never the right one.
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

    // Standard distributes the CIE standard sky's LUMINANCE ratio and paints it with the case's own admitted Tint,
    // so the arm carries a hue without a second colour path: the ratio owns the distribution, ZenithRadiance the
    // energy, and the unit-Y tint the chromaticity Cie admitted off the photometric gate.
    static RgbSpectrum Standard(CieStandard sky, WorldDirection direction, WorldDirection sun, SkyAtmosphere atmosphere) {
        double level = sky.Type.Relative(
            Math.Max(1e-4, direction.CosZenith),
            Math.Acos(Math.Clamp(direction.Dot(sun), -1.0, 1.0)),
            Math.Acos(Math.Clamp(sun.CosZenith, -1.0, 1.0))) * atmosphere.ZenithRadiance * atmosphere.Exposure;
        Span<double> channels = [level * sky.Tint.R, level * sky.Tint.G, level * sky.Tint.B];
        return Finite.Spectrum(channels);
    }

    // Circumsolar reads the SAME distribution at zero angular distance from the sun, which is the disc's own
    // radiance: the indicatrix group peaks there by construction, so the standard sky's direct beam is its own
    // circumsolar limit rather than a solar model bolted beside it.
    static RgbSpectrum Circumsolar(CieStandard sky, WorldDirection sun, SkyAtmosphere atmosphere) =>
        Standard(sky, sun, sun, atmosphere);

    // Limb reads the solar asset across the disc at the sun's own elevation and turbidity. Radius is the profile's
    // one parameter, so the centre reads the full polynomial and the rim its constant term.
    static RgbSpectrum Limb(SolarCoefficients solar, WorldDirection sun, SkyAtmosphere atmosphere, UnitInterval radius) {
        double elevation = Elevation(sun);
        Span<double> channels = stackalloc double[solar.Channels];
        for (int c = 0; c < solar.Channels; c++) {
            channels[c] = solar.Radiance(c, atmosphere.Turbidity, elevation, radius);
        }
        return SkySpectrum.ToScene(channels, atmosphere.Exposure);
    }

    // Elevation is the fit's own cube-root parameterization, derived ONCE so the diffuse expansion and the disc read
    // one axis: both assets index their control lattice on it, and two derivations would let a low sun read the
    // sky at one elevation and its own disc at another.
    static double Elevation(WorldDirection sun) =>
        Math.Cbrt(Math.Clamp(Math.Asin(Math.Clamp(sun.CosZenith, -1.0, 1.0)) / (Math.PI / 2.0), 0.0, 1.0));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The apparent azimuth/altitude ANGLES resolve at the kernel (nutation, quadratic mean longitude, pressure-corrected
// refraction), and this owner projects them into the frozen +X-north/+Y-west/+Z-up frame.
// The kernel SunPosition.Direction (+Y-north survey frame) is deliberately unread here.
// CONSEQUENCE for a daylight consumer: the kernel's closed-form fold holds arc-minute apparent position across the
// four centuries around J2000 and degrades gradually outside that window, so a study inside it is bounded by the
// atmosphere rather than by the almanac — real pressure, temperature, and humidity variance near the horizon exceeds
// the whole algorithmic budget, which is why site ELEVATION is a parameter here and a higher-order ephemeris is not.
// A study that genuinely needs sub-arc-second position across millennia is a kernel-tier request, never a fold added
// beside this adapter.
public static class SolarFrame {
    // SolarSite owns the accumulated range proof; this demanding operation supplies the key when generated factory
    // evidence crosses the kernel acceptance bridge. Sea level remains the default for a caller without height data.
    public static Fin<WorldDirection> Of(
        double latitudeDegrees, double longitudeDegrees, Instant instant, Op key, double elevationM = 0.0) =>
        // This frame anchors at UTC: the almanac's true-solar-minutes fold cancels the offset, so the site takes
        // Offset.Zero and a caller holding a civil zone converts at its own clock rather than here.
        from site in key.AcceptValidated(SolarSite.Validate(
            latitudeDeg: latitudeDegrees,
            longitudeDeg: longitudeDegrees,
            timezone: Offset.Zero,
            elevationM: elevationM,
            out SolarSite? admitted), admitted)
        select Project(SolarPosition.At(site, instant));

    // Azimuth measures from north INCREASING EASTWARD; the frozen local frame is +X north / +Y WEST, so the
    // east component seats on −Y — a morning sun lands ESE, never in the west.
    static WorldDirection Project(SunPosition sun) {
        double azimuth = sun.AzimuthDeg * (Math.PI / 180.0);
        double apparent = sun.AltitudeDeg * (Math.PI / 180.0);
        return new WorldDirection(Math.Cos(apparent) * Math.Cos(azimuth), -(Math.Cos(apparent) * Math.Sin(azimuth)), Math.Sin(apparent)).Normalize();
    }
}

// SkySpectrum is the ONE colour path both fitted assets cross. The published Hosek-Wilkie datasets carry a
// SOLAR-RADIANCE block in SPECTRAL FORM ONLY — no tristimulus solar fit was ever published — while the
// three-channel diffuse block is fitted against sRGB primaries, so admitting the tristimulus block beside the
// spectral one would put a dome and its own sun on two colour paths and hand the dome a primaries re-base no
// SkyCoefficients column declares. Both assets therefore admit their ELEVEN-BAND spectral fit and integrate here.
// The integration is a DERIVED band→scene matrix built once at type init: each band's unit basis reconstructs onto
// the Spd sampling grid through the same monotone-cubic reconstruction surface#SPECTRAL_UPSAMPLE runs (no overshoot,
// so a basis bounded in [0,1] cannot acquire a negative lobe at its own edges), crosses Wacton's observer
// integration through the ONE PortValue.SceneLinear working space, and lands as that band's scene-linear
// contribution. Per texel the whole integration is then the linear combination the expansion already produced —
// no interpolant rebuilds per sample, and the CIE observer, the white point, and the AP1 primaries all arrive from
// the one owner that publishes them rather than from a re-base this page would have to declare.
internal static class SkySpectrum {
    // The published spectral grid: 320 nm to 720 nm at a uniform 40 nm step.
    public const int BandStartNm = 320, BandStepNm = 40, BandCount = 11;

    // A band's scene-linear column may carry NEGATIVE channels — a narrow spectral basis lies outside AP1, and that
    // is the honest reading. Only the SUM over a real spectrum is a radiance, so the floor-and-finiteness gate runs
    // once on the combination rather than per column, where it would bias every integration it touched.
    private static readonly (double R, double G, double B)[] Basis = [.. Enumerable.Range(0, BandCount).Select(static band =>
        new Unicolour(PortValue.SceneLinear, Reconstruct(band)).RgbLinear.Triplet switch {
            var t => (t.First, t.Second, t.Third),
        })];

    // One monotone-cubic interpolant per band, built eleven times at class load and never again.
    private static Spd Reconstruct(int band) =>
        Interpolate.CubicSplineMonotone(
            [.. Enumerable.Range(0, BandCount).Select(static i => (double)(BandStartNm + (i * BandStepNm)))],
            [.. Enumerable.Range(0, BandCount).Select(i => i == band ? 1.0 : 0.0)]) switch {
            var curve => new Spd(SpectralUpsample.SampleStart, SpectralUpsample.SampleStep,
                [.. Enumerable.Range(0, SpectralUpsample.SampleCount).Select(sample =>
                    Math.Max(0.0, curve.Interpolate(SpectralUpsample.SampleStart + (sample * SpectralUpsample.SampleStep))))]),
        };

    // Per texel the combination costs eleven multiply-adds over the derived columns.
    public static RgbSpectrum ToScene(ReadOnlySpan<double> bands, double exposure) {
        (double r, double g, double b) = (0.0, 0.0, 0.0);
        for (int band = 0; band < bands.Length; band++) {
            (double br, double bg, double bb) = Basis[band];
            (r, g, b) = (r + (bands[band] * br), g + (bands[band] * bg), b + (bands[band] * bb));
        }
        Span<double> channels = [r * exposure, g * exposure, b * exposure];
        return Finite.Spectrum(channels);
    }

    // The ground albedo authored as an RgbSpectrum resolves onto the fit's OWN band grid once per atmosphere, never
    // per texel: the upsample's sampled reflectance IS the curve, read at each band centre off the page's declared
    // extent. Below the visible grid the reflectance flat-extrapolates from its first sample — a reflectance carries
    // no structure at 320 and 360 nm that a visible-range upsample could have invented — and the two lattice bands
    // there read that value rather than a fabricated one or a zero the fit would take as a black ground.
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
    // The sky owner supplies the per-texel RADIANCE and NOTHING ELSE. Raster/press#PRESS_PLAN PressSubject.Sky
    // carries this closure, partitions the dome under PressProgram.Dome, and owns the extent, the layer arrangement
    // (its equirect band arm reading LayerLaw.CubeFaces for a cube dome), cancellation, the receipt, and the
    // accelerator lane. A page-local partition beside that engine was the press spelled twice — the two would drift
    // the first time one grew a governance column the other lacked, and only the press-written plane is a plane a
    // TextureSet can carry. What survives here is the model: the radiance law and its rows, which is the part no
    // press could own.
    // The closure crosses the host Vector3d the press subject declares and lifts to this page's WorldDirection at
    // this ONE site — the confined [KERNEL_EDGE] the folder law admits, countable and retiring the day the press
    // subject goes host-neutral. The sun normalizes ONCE, outside the closure, so a whole-dome sweep pays it once.
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
- Law: the equirect correspondence is FROZEN and single-sourced here — `u = 0.5 + atan2(d.Y, d.X) / 2π`, `v = acos(clamp(d.Z, −1, 1)) / π`, `v = 0` at `+Z`, `u` increasing counter-clockwise viewed from `+Z` — so the sky sweep, the prefilter, the CDF, and the `EnvironmentLight` lookup all address one mapping and a consumer re-deriving it forks the seam this owner exists to hold. `WorldDirection` is the PRODUCER-OWNED world carrier of that basis — the same `+Z`-up convention the `bsdf#SHADING_FRAME` `LocalVector<T>` tangent triple declares for its own frame, split into a DISTINCT type here so a tangent-frame vector cannot reach a dome read and the frame law needs no consumer-side prose; a Y-up runtime remaps the DIRECTION BASIS at its own read and never rewrites a plane.
- Law: the sampler lift is PER LAYER. `Raster/plane#TEXTURE_PYRAMID` `AsImage` carries one layer by construction, so a six-face cube admitted as one sampler refuses the bridge and leaves the layered arms declared capability that cannot run. Each layer extracts as its own scene-linear plane, folds its own Kaiser pyramid, and lifts one `TextureSource.Image`; the map HOLDS each pyramid beside its sampler — the bridge's levels window the pyramid's arenas, so the chain releases at the map's own `Dispose` and a lift-time dispose reads freed memory on the first tap.
- Growth: a new arrangement is one `MapLayout` row binding its layer count, `LayerLaw`, aspect, and the three coordinate delegates — the `Project` fold and the per-layer lift read all four off the row, so a dual-paraboloid or lat-long cube-cross lands as a row with zero new surface; a new admission gate is one predicate on `Of`.
- Boundary: `EnvironmentMap` NEVER decodes a container — `Raster/codec#RASTER_CODEC` sniffs the magic and produces the plane, so a Radiance `.hdr`, an OpenEXR half plane, and a synthesized sky reach ONE admission. `linear`, `pq`, and `hlg` are the ADMITTED transfers and this is the one corpus surface where the display-referred pair is legal; `srgb` and `raw` refuse, because an sRGB dome cannot carry a sun and a raw dome declares no light quantity. `Lift` lowers whatever admits to scene-linear ONCE through the arena's `Read` rail, so no consumer re-applies a transfer.
- Boundary: rotation and intensity are READ-TIME values, so a re-oriented or re-exposed dome re-keys no blob and re-runs no prefilter. Intensity is ADMITTED evidence, never a bare multiplier: `Of` composes `Photometric.Admit` on the authored shape and re-seats an already-admitted receipt on the `Project` shape, so a reprojection never double-coerces. Every read scales by `RadiometricSi`, and the dimensionless case admits as `PhotometricQuantity.Radiance` whose `Borrowed` coercion leaves `RadiometricSi == Measure.CanonicalValue` — one construction, no branch downstream. `MaterialFault` rails a wrong-aspect plane, a layer count contradicting the row, an integer-depth plane, a refused transfer, and an out-of-range read policy.

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
    EmissionEvidence Intensity, double Rotation, Op Key) : IDisposable {

    // Admitted rows the three transfers. pq and hlg are legal HERE and nowhere else in the corpus: an environment map
    // is the one display-referred ingest a scene-linear pipeline admits, and the per-layer lift lowers it once.
    static readonly Seq<PlaneTransfer> Admitted = Seq(PlaneTransfer.Linear, PlaneTransfer.Pq, PlaneTransfer.Hlg);

    // ONE admission over TWO input shapes, discriminated by the value the caller holds rather than by a name: a
    // caller holding an AUTHORED magnitude and its unit crosses Photometric.Admit here — the folder's second and
    // last such site, so an HDRI authored in lux and one authored as a bare multiplier stay distinguishable at every
    // read and on the wire mirror — while a caller holding already-admitted evidence (the Project re-seat) passes
    // the receipt straight through. Re-admitting an admitted receipt would double the coercion; refusing the
    // authored shape would leave the folder law with no fence behind it.
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
        // Integer depth cannot carry a dome: an 8- or 16-bit plane clips the sun by orders of magnitude and the
        // prefilter then integrates a truncated distribution no downstream gate can recover.
        from ____ in guard(plane.Format.Depth == ChannelDtype.Float16 || plane.Format.Depth == ChannelDtype.Float32,
                new MaterialFault.Parameter(key, $"<environment-not-hdr:{plane.Format.Key}>"))
        from _____ in guard(Admitted.Exists(t => t == plane.Transfer),
                new MaterialFault.Parameter(key, $"<environment-transfer:{plane.Transfer.Key}>"))
        // Intensity crosses as ADMITTED unit evidence, never a bare multiplier: the exposure of a dome authored
        // in lux and one authored as a dimensionless gain are different physical claims, and only the receipt
        // tells them apart. Every read scales by RadiometricSi, so the gate reads the same field the shading
        // path does rather than a magnitude the row also carries in its original unit.
        from ______ in guard(double.IsFinite(intensity.RadiometricSi) && intensity.RadiometricSi >= 0.0
                          && double.IsFinite(rotation) && rotation is >= 0.0 and < (2.0 * Math.PI),
                new MaterialFault.Parameter(key, $"<environment-read-policy:{intensity.Measure.CanonicalUnit}:{intensity.RadiometricSi:R},{rotation:R}>"))
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

    // One row read from the source layer, one row written to the linear face, the arena owning both directions so
    // no transfer or lane arithmetic is re-spelled here.
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
        Stored(Rotated(direction.Normalize(), -Rotation), lod).Scale(Intensity.RadiometricSi);

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
    // It is a WHOLE-FIELD sweep, so it consumes the budget's governance on the same seam and an
    // abandoned reprojection releases its target arena before railing.
    public Fin<EnvironmentMap> Project(MapLayout target, Dimension edge, RenderBudget budget, Op key) {
        if (target.Key == Layout.Key) { return Fin.Succ(this); }
        (Dimension width, Dimension height) = target.Extent(edge);
        return TexturePlane.Of(PlaneFormat.Rgba32F, width, height, PlaneTransfer.Linear, AlphaMode.None, key,
                    layers: Some(Dimension.Create(target.Layers)))
                .Bind(plane => budget
                    .Sweep(height.Value * target.Layers, new ProjectSweep(this, target, plane), key)
                    .Map(_ => plane)
                    .Rollback(plane))
                // The mint's own guards (aspect, layer law, transfer, HDR depth, read policy) each refuse, so the
                // arena releases on that arm too — the sweep's abandonment path is not the only one that owns it.
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

    // Each target texel inverts to a direction through the TARGET row and reads the source through the SOURCE row,
    // so the two coordinate laws meet on the direction and never on an index formula.
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

- Owner: `IblPrefilter` the reduction fold; `ShBand` the nine-row spherical-harmonic basis table; `Sh9` the twenty-seven-value irradiance carrier; `IblPolicy` the sampling-budget row; `IblProducts` the CPU receipt and `IblProduct` the lane-product split over it; the kernel `Deterministic.Hammersley` the composed low-discrepancy draw.
- Entry: `public static Fin<IblProduct> Prefilter(EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key, Option<PressDevice> device = default)` — ONE reduction producing every product, because the pyramid, the SH projection, and the CDF each sweep the same field and three entrypoints sweep it three times; governance rides the budget's own `Governance` column rather than a token-and-sink tail, so the page's longest operation is abortable and watchable with no signature widened, the MIP LADDER is the reported unit (the policy's own declared level count, each level a whole-dome sweep), and an abandoned run disposes every level it already integrated before railing `Errors.Cancelled`; `Sh9.Project(EnvironmentMap, RenderBudget, Op)` and `Sh9.Irradiance(WorldDirection)` are the projection and reconstruction halves of ONE correspondence on one owner; `IblProducts.SpecularLevel(UnitInterval)` maps a roughness onto the fractional mip the level set encodes.
- Law: every product integrates the map's STORED frame. Rotation and intensity apply at the `[05]` read, so a re-oriented or re-exposed dome reuses the same content-addressed blobs and a rotation is never a re-bake.
- Law: `ShBand` IS the FROZEN SH9 spelling — real orthonormal harmonics through `l = 2` in the right-handed `+Z`-up basis, band-major with RGB interleaved at `i·3 + c`, carrying the Lambertian convolution constants `Â₀ = π`, `Â₁ = 2π/3`, `Â₂ = π/4`. The normalization constants DERIVE from `sqrt((2l+1)/4π)` folded with each polynomial's own factor, so four expressions serve nine rows and the `Raster/gpu#WGSL_KERNEL` `irradianceSh` float literals are a DECLARED transcription of this primary (branch RULINGS: a WGSL twin transcribes its CPU owner's own members). `Sh9.Of` refuses a channel-major layout and any length other than twenty-seven.
- Law: `ShGolden.All` rosters two EVALUATED fixtures and `ShGolden.Prove` runs them against three admitted `Tolerance` bars. `L(ω) = 1` yields `sh_0 = √(4π)` with every other band zero and `E(n) = π`; `L(ω) = ω·ẑ` yields `sh_2 = √(4π/3)` with every other band zero and `E(+ẑ) = 2π/3` — both expectations DERIVED from their own analytic projections, so a Y-up transcription lands the axial energy at `sh_1`/`sh_3` and fails, and the reconstruction probe fails a wrong `Â` set the projection alone cannot see. The same proof sums each `MapLayout.SolidAngle` closed form to `4π` over its own grid, and it TRAVERSES both rosters on the rail so a broken layout and a broken band each name themselves.
- Law: every GGX integral here reads the `bsdf#MICROFACET_KERNEL` kernel — `Microfacet<double>.SampleVisibleNormal` draws the half-vector, `Microfacet<double>.VisibleNormalPdf` supplies the density, `Microfacet<double>.MaskingShadowing` the Smith term — the generic kernel at this page's own instantiation — so the prefiltered dome and the shaded surface integrate the SAME distribution and a re-minted importance sampler is the deleted form. Sampling composes the kernel `Deterministic.Hammersley` equidistributed pair — the low-discrepancy member family the deterministic-draw owner carries BESIDE its splitmix64 stream, because splitmix64 clustering leaves visible prefilter noise at a bounded tap budget — so this page authors no sampling kernel of its own.
- Law: the specular tap reads the SOURCE mip whose solid angle matches the sample density. That term is the firefly suppression a bounded tap budget requires and is a declared column rather than a hidden clamp: a blown highlight spreads across the taps it covers instead of being clipped out of the integral.
- Law: the luminance guide resolves the DOME ALONE. The `[02]` synthesis keeps the disc out of the plane, so the guide never has to import-sample a source that spans one texel and out-shines its neighbours by four decades — the firefly no tap budget resolves and the quadrature error the SH would carry forever — and the `[05]` row prices the two arms against each other from the guide's own total and the disc's area-averaged radiance instead. An ingested HDRI with a baked sun carries that sun in its guide, which is exactly the arrangement the split is honest about: its disc is absent, its selection is zero, and the guide is the only structure there is.
- Law: every SOURCE-DOMAIN sweep partitions and reads by INDEX. `Sh9.Project` runs a commutative reduction, so each row accumulates its own band vector under the budget's governed `Sweep` and one fold sums them; the luminance guide's per-row conditional mass is likewise independent, and only the marginal prefix over row masses is sequential. This law forecloses a serial full-plane sweep beside partitioned siblings — at a four-thousand-texel dome the two reductions are the campaign's heaviest single-threaded folds.
- Growth: a new prefilter product is one column on `IblProducts` filled inside the one sweep; a new sampling budget is one `IblPolicy` column; a new execution lane is one `press#PRESS_PLAN` `PressBackend` row the policy already carries; a new mip ladder is a `MipPolicy` row on the pyramid owner. `BrdfLut` stays environment-INDEPENDENT and view-independent — a pure function of `(N·V, roughness)` — so it computes once per `IblPolicy` and a second environment reuses the same blob by content address.
- Boundary: prefiltering NEVER writes a file; `IblProducts` carries planes and the egress name grammar belongs to `Raster/set#TEXTURE_SET`. Plane bytes are always CPU-minted, so the GPU arm is an accelerator whose output is never content-addressed — STRUCTURALLY, not by rule: `IblProduct` splits `Minted` from `Preview` and `EnvironmentBlobs` is reachable only from the minted case. The preview omits the BRDF LUT and the luminance guide honestly rather than partially, since the LUT is environment-independent and the guide's marginal prefix is this page's declared sequential step, so neither has a kernel row to fill it. `IblPolicy` carries the `PressBackend` row and `Prefilter` takes the `Option<PressDevice>` its arm reads, so the lane is selected by data the plan already models; a degenerate all-black dome REFUSES the CDF rather than returning a flat table that samples uniformly while claiming importance.
- Boundary: a GPU prefilter arm writes the SAME equirect arrangement this fold does — an accelerator that changes the product's own layout is a second product, not a faster one. `Raster/gpu#WGSL_KERNEL` `prefilterSpecular` therefore inverts the frozen equirect correspondence per output texel and takes a plane extent, and `IblProducts.Specular` needs no arrangement column for a lane to fill it; the cube arrangement stays `equirectToCube`'s, the one kernel whose product IS a cube.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// ShBand rows the frozen SH9 basis: (l, m), the normalization constant, the basis polynomial, and the Lambertian
// convolution coefficient per band. Projection AND irradiance reconstruction are ONE fold over Items, so band order,
// normalization, and convolution can never disagree between the two directions.
[SmartEnum<int>]
public sealed partial class ShBand {
    // The normalization constants DERIVE from their own closed form — sqrt((2l+1)/4π) folded with each polynomial's
    // own factor — rather than standing as nine transcribed decimals nobody can check against the band they claim to
    // normalize. Four distinct values serve nine rows, which is itself the reading a decimal table hides: the three
    // l=1 rows share one constant and the l=2 rows share one up to the m=2 half and the m=0 form. The
    // Raster/gpu#WGSL_KERNEL `irradianceSh` twin pins these as float literals because a shader constant cannot read a
    // managed static — that is the declared transcription peer, and this expression is the primary it transcribes,
    // so a divergence between them is nameable at one site instead of being two hand-typed tables' disagreement.
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

// --- [MODELS] ------------------------------------------------------------------------------
// Twenty-seven values, band-major with RGB interleaved: index i·3 + c holds band i channel c. A channel-major
// layout is the decode fork this owner forecloses, and the length gate is structural at Of.
public sealed record Sh9(ReadOnlyMemory<double> Bands) {
    public const int Slots = 27;

    public static Fin<Sh9> Of(ReadOnlyMemory<double> bands, Op key) =>
        bands.Length == Slots && Finite.All(bands.Span)
            ? Fin.Succ(new Sh9(bands))
            : new MaterialFault.Parameter(key, $"<sh9-layout:{bands.Length}!={Slots}>");

    // Radiant answers the whole-sphere radiance integral the band-zero coefficient ALREADY carries: Y0 is the
    // constant 1/(2√π), so the DC coefficient times 2√π IS that integral. The [05] arm split prices the dome
    // against the disc through it, so the selection reads a number the projection already produced rather than
    // re-reducing the field — and a dome with no luminance guide still prices, because the SH always projects.
    public RgbSpectrum Radiant {
        get {
            ReadOnlySpan<double> bands = Bands.Span;
            double scale = 2.0 * Math.Sqrt(Math.PI);
            Span<double> channels = [bands[0] * scale, bands[1] * scale, bands[2] * scale];
            return Finite.Spectrum(channels);
        }
    }

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
        return budget.Sweep(h * layers, new ProjectSweep(map, rows, w, h), key).Bind(_ => {
            double[] bands = new double[Slots];
            for (int row = 0; row < h * layers; row++) {
                for (int slot = 0; slot < Slots; slot++) { bands[slot] += rows[(row * Slots) + slot]; }
            }
            return Of(bands, key);
        });
    }

    // Each texel's solid angle comes from the layout's own inverse — the equirect
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

// IblPolicy rows the whole prefilter budget: taps per specular texel, LUT taps, the two extents, the pyramid depth, the
// importance-sampling arm, and the EXECUTION LANE. Every knob a caller could reach for is a column, never a
// parameter tail. Backend is the press#PRESS_PLAN row rather than a second lane vocabulary, so the content-identity
// law this folder settles once — ContentAuthoritative as ROW DATA — reads identically at a bake and at a prefilter.
public readonly record struct IblPolicy(
    int SpecularTaps, int LutTaps, Dimension LutExtent, Dimension SpecularEdge, int Mips, bool ImportanceSampled, PressBackend Backend) {
    public static readonly IblPolicy Default = new(SpecularTaps: 1024, LutTaps: 512, LutExtent: Dimension.Create(256),
        SpecularEdge: Dimension.Create(256), Mips: 6, ImportanceSampled: true, Backend: PressBackend.Cpu);

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

    // Running mass is non-decreasing by construction, so the search is total and needs no equality tolerance.
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

// IblProduct splits what a LANE may produce, mirroring the press#PRESS_RECEIPT PressProduct split at this owner's
// grain. Minted carries the content-addressable receipt [05] hangs blob custody off; Preview carries the same
// integrated planes with NO digest, NO key, and no BRDF LUT or luminance guide — the two products the accelerator
// lane genuinely cannot produce, since the LUT is environment-independent and the guide's marginal prefix is
// sequential. Because EnvironmentBlobs is reachable only from Minted, a GPU product is STRUCTURALLY unable to be
// content-addressed rather than refused by a rule someone has to enforce, which is the same shape the folder's
// CPU-minted-bytes law already takes at the press: f32 cannot reproduce the f64 fold, so a GPU-keyed plane would
// fork the content key at its preimage.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IblProduct : IDisposable {
    private IblProduct() { }

    public sealed record Minted(IblProducts Products) : IblProduct;
    public sealed record Preview(Sh9 Irradiance, Seq<TexturePlane> Specular, Seq<double> RoughnessPerMip) : IblProduct;

    public void Dispose() => Switch(
        minted:  static m => m.Products.Dispose(),
        preview: static p => p.Specular.Iter(static level => level.Dispose()));
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class IblPrefilter {
    // ONE entry over BOTH lanes, splitting on the policy row exactly as TexturePress.Press does — the backend row
    // decides, never a caller flag. The device is the accelerator arm's own evidence rather than a knob: a leased
    // device carries capability a policy value cannot reconstruct, so every (backend, device) pairing has a stated
    // outcome and none is silently ignored. A device handed to the authoritative lane refuses rather than going
    // unread, and the accelerator lane without one refuses before it integrates anything.
    public static Fin<IblProduct> Prefilter(
        EnvironmentMap map, IblPolicy policy, RenderBudget budget, Op key, Option<PressDevice> device = default) =>
        (policy.Backend.ContentAuthoritative, device.Case) switch {
            (true, null) => Mint(map, policy, budget, key).Map(static products => (IblProduct)new IblProduct.Minted(products)),
            (true, _) => new MaterialFault.Parameter(key, $"<ibl-device-on-authoritative-backend:{policy.Backend.Key}>"),
            // The accelerator lane ARMS: `gpu#WGSL_KERNEL` `prefilterSpecular` and `irradianceSh` write this page's
            // own equirect arrangement, and the product they fill is the PREVIEW case — planes with no digest and no
            // key — so the lane produces something real while remaining structurally unable to reach
            // EnvironmentBlobs. The BRDF LUT and the luminance guide correctly carry no kernel row (the LUT is
            // environment-independent and the guide's marginal prefix is sequential), which is exactly why the
            // preview case omits both rather than filling them from a lane that cannot compute them.
            (false, PressDevice lease) => Accelerate(map, policy, lease, key),
            (false, _) => new MaterialFault.Parameter(key, $"<ibl-accelerator-without-device:{policy.Backend.Key}>"),
        };

    // The accelerator arm dispatches the two kernels that carry one and folds their receipts into the preview
    // product. Every level lands EQUIRECT exactly as the CPU lane's does — an accelerator that changed the product's
    // arrangement would be a second product rather than a faster one — and the roughness ladder is the policy's own,
    // so a shading read addresses a preview level by the same `SpecularLevel` inverse it addresses a minted one by.
    // The SH partials arrive already folded by the row's own `KernelReduce.PartialSum` reduction, so this owner
    // re-derives no tail; the twenty-seven slots cross the same `Sh9.Of` gate the CPU projection crosses, which is
    // what keeps one decode law over both lanes.
    static Fin<IblProduct> Accelerate(EnvironmentMap map, IblPolicy policy, PressDevice device, Op key) =>
        from irradiance in device.Dispatch(WgslKernel.IrradianceSh, ShBinding(map), key)
            .Bind(receipt => Sh9.Of(Widen(receipt.Output), key))
        from specular in toSeq(Enumerable.Range(0, policy.Mips))
            .Fold(Fin.Succ(Seq<TexturePlane>()), (acc, mip) => acc.Bind(levels =>
                device.Dispatch(WgslKernel.PrefilterSpecular, LevelBinding(map, policy, mip), key)
                    .Bind(receipt => Decode(policy, mip, receipt.Output, key))
                    .Map(level => levels.Add(level))
                    .Rollback([.. levels])))
        select (IblProduct)new IblProduct.Preview(irradiance, specular.Strict(),
            toSeq(Enumerable.Range(0, policy.Mips).Select(policy.RoughnessAt)));

    // The device returns f32 and every product on this page is f64: the widen is the ONE place that conversion
    // happens, so a preview slot and a minted slot differ by their lane's precision alone rather than by where each
    // one narrowed.
    static ReadOnlyMemory<double> Widen(ReadOnlyMemory<float> output) =>
        (ReadOnlyMemory<double>)[.. output.ToArray().Select(static lane => (double)lane)];

    // Both bindings carry the SAME source: the map's base level as one flat RGBA run, the row's uniform words, and a
    // write buffer sized to the product. The uniform block goes through the gpu page's OWN KernelUniform writer
    // rather than a float array cast here, because that block interleaves f32 with u32 and a float-typed carrier
    // writing an extent into a u32 slot hands the shader a billion.
    // The uniform block mirrors the kernel's Params WORD FOR WORD — width, height, groups, pad — because `groups`
    // is the grid-stride the accumulation loop divides the texel range by (a missing groups word is a zero stride),
    // and the write buffer carries PER-WORKGROUP partials (groups x 27), never the folded 27 — the row's
    // KernelReduce.PartialSum folds them host-side in workgroup-index order.
    static KernelBinding ShBinding(EnvironmentMap map) =>
        ((uint)Math.Max(1, ((map.Plane.Width.Value * map.Plane.Height.Value) + 63) / 64)) switch {
            var groups => new KernelBinding(
                Seq(KernelUniform.Empty.Extent(map.Plane.Width, map.Plane.Height).U32(groups).U32(0u).Block,
                    new KernelBuffer.Read(Flatten(map)),
                    new KernelBuffer.Write((int)groups * Sh9.Slots)),
                GroupsX: groups, GroupsY: 1u, GroupsZ: 1u),
        };

    // Word-for-word against the kernel's Params — dest extent, then SOURCE extent, then roughness as f32, then the
    // tap count as u32, then the two pad words. The uniform carries ROUGHNESS, never alpha: the kernel spells
    // Microfacet.AlphaOf itself (its own 1e-4 floor), so a pre-applied alpha here would square the remap.
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

    // The level ladder is the CPU lane's own — halved per mip off the policy edge at the frozen 2:1 equirect aspect —
    // so a preview level and a minted level at one roughness address the same extent and the parity workload compares
    // texel for texel rather than resampling one side first.
    static (int Width, int Height) Extent(IblPolicy policy, int mip) =>
        Math.Max(1, policy.SpecularEdge.Value >> mip) switch { var edge => (edge * 2, edge) };

    static ReadOnlyMemory<float> Flatten(EnvironmentMap map) =>
        (ReadOnlyMemory<float>)[.. Enumerable.Range(0, map.Plane.Height.Value)
            .SelectMany(y => Enumerable.Range(0, map.Plane.Width.Value)
                .SelectMany(x => map.Texel(0, x, y) switch {
                    var texel => new[] { (float)texel.R, (float)texel.G, (float)texel.B, 1.0f },
                }))];

    // The device's flat f32 run lands through the arena's own Write rail, so the preview plane is a TexturePlane
    // like every other and the transfer, the lane gather, and the disposal are the arena's rather than re-spelled.
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
        // BrdfSource reads through the SAME sampler the dome does — a single-level pyramid bridged by AsImage — so the
        // split-sum read and the environment read share one reconstruction and no point-read surface is minted. The
        // pyramid rides the receipt: its arenas back the sampler, so it releases with the products, never here.
        from lutPyramid in TexturePyramid.Of(lut, MipPolicy.None, key)
        from lutSource in lutPyramid.AsImage(key)
        from cdf in policy.ImportanceSampled ? Guide(map, budget, key).Map(Some) : Fin.Succ(Option<LuminanceCdf>.None)
        select new IblProducts(irradiance, specular, toSeq(Enumerable.Range(0, policy.Mips).Select(policy.RoughnessAt)), lut, lutPyramid, lutSource, cdf);

    // Each mip integrates the dome against the VNDF at that mip's roughness with the view direction pinned to the
    // normal — the split-sum approximation's declared simplification, stated here rather than discovered downstream.
    // THE MIP IS THE GOVERNANCE UNIT: the level count is the policy's own declared column and every level sweeps the
    // whole dome, so the ladder is the one boundary whose fraction means something to a caller. The prefilter is the
    // page's longest operation, so the sweep opens each level on the budget's publish-and-check seam and an abandoned
    // run rails Errors.Cancelled BEFORE renting the next level's arena rather than after the whole ladder.
    // A Traverse cannot short-circuit on a token, so the ladder folds — which is also what keeps the abandoned arm
    // from holding every level it already integrated.
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

    // Filtered importance sampling selects the mip through the lod term —
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

    // BrdfLut integrates a pure function of (N·V, roughness), so one plane serves every environment and re-keys only
    // on an IblPolicy change. R carries the Fresnel scale, G the bias — the two-component rg16 plane is the declared
    // storage, never an rgba plane wasting half its texels.
    static Fin<TexturePlane> BrdfLut(IblPolicy policy, RenderBudget budget, Op key) =>
        TexturePlane.Of(PlaneFormat.Rg16, policy.LutExtent, policy.LutExtent, PlaneTransfer.Raw, AlphaMode.None, key)
            .Bind(plane => budget
                .Sweep(policy.LutExtent.Value, new LutSweep(policy, plane), key)
                .Map(_ => plane)
                .Rollback(plane));

    // Under the VNDF sampler the per-tap estimator COLLAPSES to
    // G₂/G₁(view) — the D·(V·H)/(N·H·N·V) Karis weight belongs to D-proportional half-vector sampling and pairing it
    // with SampleVisibleNormal double-counts the visible-normal density, biasing exactly the grazing band the LUT
    // exists to correct. Both Smith terms are the kernel's own, so the LUT and the shaded surface cannot drift; both
    // outputs land in [0,1] by construction, which is why the rg16 normalized depth carries them without a scale column.
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
        return budget.Sweep(h, new GuideSweep(map, conditional, marginal, w, h), key).Bind(_ => {
            double total = 0.0;
            for (int y = 0; y < h; y++) { total += marginal[y]; marginal[y] = total; }
            return total > 0.0
                ? Fin.Succ(new LuminanceCdf(conditional, marginal, total, w, h))
                : new MaterialFault.Parameter(key, "<environment-zero-luminance>");
        });
    }

    // Running mass IS the searchable structure, so a fold into a fresh array per row would allocate h times to
    // produce the same monotone spine. The row mass
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
    // Three bars ride here as PUBLISHED FIGURES the kernel `Domain/context` lanes admit — a bare
    // double here is a gate nobody can move, where a lane value carries its band, its dimension signature, and the
    // `Context.Override` a project tightening the proof reaches it by. `Spectral` is the band bar's lane (the
    // dimensionless spectral-agreement gate), `Irradiance` the reconstruction bar's (the convolution amplifies the
    // quadrature error onto a radiometric magnitude), and `Conservation` the measure-closure bar's, since summing a
    // layout's own solid angles to 4pi is a conservation residual rather than a length.
    // Each figure states the midpoint quadrature's measured behaviour over the equirect grid from 2048 rows
    // up; each admits ONCE at Prove, so the proof cannot run on an unadmitted scalar.
    const double BandBar = 1e-6, IrradianceBar = 1e-5, MeasureBar = 1e-4;
    public const int ProofRows = 2048;

    // Bars admits the three published figures onto their kernel lanes as ONE accumulating fan — a proof whose gates
    // are themselves out of band reports all three at once rather than the first, which is the whole reason the
    // admission is applicative and not a bind chain.
    static Fin<(Tolerance Band, Tolerance Irradiance, Tolerance Measure)> Bars(Op key) =>
        (Tolerance.Of(ToleranceLane.Spectral, BandBar, key).ToValidation(),
         Tolerance.Of(ToleranceLane.Irradiance, IrradianceBar, key).ToValidation(),
         Tolerance.Of(ToleranceLane.Conservation, MeasureBar, key).ToValidation())
            .Apply(static (band, irradiance, measure) => (Band: band, Irradiance: irradiance, Measure: measure))
            .As().ToFin();

    // Both expectations DERIVE from their own analytic projections and neither is a transcribed decimal: a uniform
    // field projects onto Y0 as sqrt(4π), and an axial cosine projects onto Y1^0 as (4π/3)·sqrt(3/4π) = sqrt(4π/3).
    // Writing them as expressions is what makes the fixture a PROOF rather than a second hand-typed table that could
    // drift into agreement with a broken basis.
    public static readonly Seq<ShGolden> All = Seq(
        new ShGolden("uniform", static _ => 1.0, Band: 0, Expected: Math.Sqrt(4.0 * Math.PI), IrradianceAtZenith: Math.PI),
        new ShGolden("axial-cosine", static d => d.Z, Band: 2, Expected: Math.Sqrt(4.0 * Math.PI / 3.0), IrradianceAtZenith: 2.0 * Math.PI / 3.0));

    public static Fin<Unit> Prove(Op key) =>
        from bars in Bars(key)
        from measures in toSeq(MapLayout.Items).Traverse(layout =>
            MeasureSum(layout, ProofRows) switch {
                var sum when Math.Abs(sum - (4.0 * Math.PI)) <= bars.Measure.Value => Fin.Succ(unit),
                var sum => Fin.Fail<Unit>(new MaterialFault.Parameter(key, $"<sh-golden-measure:{layout.Key}:{sum:R}>")),
            }).As()
        from bands in All.Traverse(row => row.Project(bars, key)).As()
        select unit;

    // One projection per fixture row over the equirect grid at the layout's own measure, then the nine-band gate,
    // then the +Z reconstruction through the SAME Sh9 owner the wire carries, so the fixture proves the shipping
    // fold and never a private re-derivation.
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
                return new MaterialFault.Parameter(key, $"<sh-golden-band:{Name}:{slot}:{bands[slot]:R}>");
            }
        }
        double[] interleaved = new double[Sh9.Slots];
        for (int slot = 0; slot < 9; slot++) {
            (interleaved[slot * 3], interleaved[(slot * 3) + 1], interleaved[(slot * 3) + 2]) = (bands[slot], bands[slot], bands[slot]);
        }
        return Sh9.Of(interleaved, key).Bind(sh =>
            Math.Abs(sh.Irradiance(WorldDirection.Zenith).R - IrradianceAtZenith) <= bars.Irradiance.Value
                ? Fin.Succ(unit)
                : (Fin<Unit>)new MaterialFault.Parameter(key, $"<sh-golden-irradiance:{Name}:{sh.Irradiance(WorldDirection.Zenith).R:R}>"));
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

- Owner: `EnvironmentLight` the resolved row the render seam consumes, its admission gates, and the reads it publishes; `SkySource` the synthesized-dome provenance; `EnvironmentSample` `[Union]` (`Dome` · `Sun`) the drawn direction with its arm.
- Entry: `public static Fin<EnvironmentLight> Of(string lightKey, EnvironmentMap map, IblProducts products, EnvironmentBlobs blobs, Option<SkySource> sky, Op key)` admits the resolved row once and resolves the dome's `SolarDisc` from that provenance; `Radiance(WorldDirection)`, `Irradiance(WorldDirection)`, `Sample(u0, u1)`, `Pdf(WorldDirection)`, `SpecularLevel(UnitInterval)`, `SplitSum(cosView, roughness)`, and the `Sun` disc row are the reads a path-trace integrator and a raster shading pass share — every direction-typed read takes the producer's own `WorldDirection`, so a tangent-frame query is a compile error rather than a frame-law violation.
- Law: the SUN is the row's own arm. `Radiance` composes the dome field with the disc so a camera ray through the sun is never black; `Sample` splits between the guided dome and a uniform cap draw on a power ratio DERIVED from the SH band-zero integral and the disc's own area-averaged radiance; every returned `Pdf` is the combined balance density, so an integrator MIS-weights the two arms and its own BSDF draw against one number and never learns the split exists. `SunSelection` is reachable for a receipt and decides nothing a caller passes.
- Law: the products are STORED-frame and every read applies the dome's rotation and intensity HERE. `Irradiance` un-rotates the queried normal before reconstructing the SH; the guided draw rotates the sampled direction into world; the density reads the STORED luminance the guide's own total was built from, so a re-exposed dome does not skew a multiple-importance weight by its own intensity factor. One read policy, applied at one altitude, over blobs no policy edit re-keys.
- Receipt: the row IS the branch receipt and supplies the generated `Set.Ibl` projection at `interchange#TEXTURE_EGRESS` — stored product references, SH bands, roughness ladder, and read-time intensity/rotation cross there without a second environment message. `SkyModelKey` carries the generated set's optional `source` for a synthesized dome and stays absent for an ingested HDRI; `CoefficientKey`, `SolarKey`, the authored intensity evidence, and the source transfer remain domain and analytics facts because the corpus carries no such columns. A revised Hosek-Wilkie fit still re-keys the light through those domain digests, while the peer reads only the resolved stored products and policy the generated document proves.
- Boundary: `Rasm.AppUi/Render/pathtrace#LIGHT_RIG` `LightSource.Environment` carries THIS row as its dome VALUE over the `[BOUNDARY]` seam — the render arm answers directional radiance, importance draw, SH irradiance, specular level, split-sum, and the SUN DISC (direction, cap, and radiance profile) on the owner that prefiltered the map, while Materials keeps the whole mapping, sampling, and prefilter algebra and the consumer re-derives no equirect correspondence, SH band order, roughness ladder, or solar geometry. `Sample` returns direction, radiance, combined density, and arm TOGETHER so a multiple-importance-sampling integrator balances with no second query, and an absent CDF answers the uniform-dome density as a declared degradation the row states rather than a silent fallback.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// EnvironmentBlobs carries the four content addresses a resolved light holds. One carrier rather than four positional
// keys keeps the Of signature honest and makes a missing CDF a typed absence rather than an empty-string sentinel.
public readonly record struct EnvironmentBlobs(ContentAddress Equirect, ContentAddress Specular, ContentAddress BrdfLut, Option<ContentAddress> LuminanceCdf);

// SkySource carries a SYNTHESIZED dome's whole provenance — the model, the atmosphere it distributed, and the sun
// direction it was rendered at — because the light row needs all three to state a disc and only the model to state a
// key. Passing the model alone left the row unable to resolve a sun at all, which is what made the direct beam
// unreachable from the render seam; an ingested HDRI carries absence and answers with no disc.
public readonly record struct SkySource(SkyModel Model, SkyAtmosphere Atmosphere, WorldDirection Sun);

// One directional sample carrying the ARM that drew it. The two arms are distinct EVIDENCE shapes, not one shape
// wearing a flag: a disc draw knows where on the disc it landed and a dome draw has no disc to be on, so the limb
// profile a consumer re-reads is recoverable from the Sun case and unrepresentable on the Dome one. Pdf is the
// COMBINED balance-heuristic density on both arms — the arm-selection probability times each arm's own density,
// summed — so a MIS integrator weights against its BSDF density with no second query and no knowledge of the split.
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
        // The wire's equirectKey names an EQUIRECT plane and the freeze admits it 2:1: a cube or octahedral dome
        // projects through the map's own layout relation BEFORE this row resolves — the gate makes the conversion
        // step structural rather than a consumer discovering a square blob behind an equirect field name.
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

    // Radiance at a WORLD direction is the dome field PLUS the disc wherever the direction lands on it. The [02] law
    // keeps the sun OUT of the synthesized plane so a bounded guide never has to import-sample a half-degree source,
    // which means a camera ray through the sun reads black unless the two terms compose HERE — one read, so no
    // consumer assembles the beam from parts and no path double-counts it.
    public RgbSpectrum Radiance(WorldDirection direction) =>
        Map.Radiance(direction, lod: 0.0).Add(SunRadiance(direction.Normalize()));

    RgbSpectrum SunRadiance(WorldDirection direction) =>
        Sun.Map(disc => disc.Radiance(direction).Scale(Map.Intensity.RadiometricSi)).IfNone(RgbSpectrum.Black);

    public double SpecularLevel(UnitInterval roughness) => Products.SpecularLevel(roughness);

    // Irradiance at a WORLD normal. The SH vector is stored-frame, so the normal un-rotates before reconstruction
    // and intensity scales after — the same two-step the directional read applies, on one owner, so a rotated dome
    // lights a surface correctly without a re-projection the blob key would then have to carry.
    public RgbSpectrum Irradiance(WorldDirection normal) =>
        Products.Irradiance.Irradiance(EnvironmentMap.Rotated(normal.Normalize(), -Map.Rotation)).Scale(Map.Intensity.RadiometricSi);

    // SplitSum answers the pair a shading pass multiplies its F0 by: X the Fresnel scale, Y the bias, read off the LUT the
    // prefilter integrated with the SAME Smith visibility the surface shades under, through the SAME sampler. That
    // LUT is environment-independent, so neither rotation nor intensity touches this read.
    public (double Scale, double Bias) SplitSum(UnitInterval cosView, UnitInterval roughness) =>
        TextureUv.Sample(Products.BrdfSource, new UvSample(cosView, roughness, Vector3d.Zero, Vector3d.ZAxis, 0.0), LutSampler, Map.Key)
            .Match(Succ: static f => f.IsFinite ? (f.X, f.Y) : (0.0, 0.0), Fail: static _ => (0.0, 0.0));

    static readonly SamplerState LutSampler = new(AddressMode.Clamp, AddressMode.Clamp, FilterMode.Bilinear, UvFrame.Identity);

    // SunSelection is the arm-selection probability, DERIVED from the two arms' own radiant power rather than
    // authored: the dome's power is the SH band-zero integral the prefilter already produced, the disc's is its
    // area-averaged radiance over the cap it subtends, and the split is the disc's share. A fixed split would
    // under-sample a clear sky's sun by orders of magnitude and over-sample an overcast one's, and a guide-derived
    // dome power would leave an unguided dome with no price at all.
    public double SunSelection =>
        (Products.Irradiance.Radiant.Luminance, SunPower) switch {
            var (dome, sun) => sun + dome > 0.0 ? sun / (sun + dome) : 0.0,
        };

    double SunPower =>
        Sun.Map(static disc => disc.Mean.Luminance * disc.SolidAngle).IfNone(0.0)
        * Map.Intensity.RadiometricSi;

    // ONE draw over BOTH arms. The selection consumes u0 and RESCALES it back onto the unit interval, so the arm the
    // draw lands in still receives a stratified pair and a two-dimensional sampler loses no dimension to the choice.
    // Every returned Pdf is the COMBINED density, so a MIS integrator weights correctly without knowing a split
    // exists — and the disc arm carries its own radius, which is what lets a limb-darkened sun answer at the point
    // it was drawn rather than at its centre.
    public EnvironmentSample Sample(UnitInterval u0, UnitInterval u1) =>
        (Sun.Case, SunSelection) switch {
            (SolarDisc disc, > 0.0 and var p) when u0.Value < p =>
                Solar(disc, UnitInterval.Create(Math.Min(u0.Value / p, 1.0 - 1e-12)), u1),
            (SolarDisc _, > 0.0 and var p) =>
                Dome(UnitInterval.Create(Math.Min((u0.Value - p) / (1.0 - p), 1.0 - 1e-12)), u1),
            _ => Dome(u0, u1),
        };

    // Pdf answers the density every MIS weight reads: the balance of both arms at the queried direction, so a BSDF
    // draw that happens to land on the sun is weighted against the same combined density the light's own draw was.
    // One member, both arms, so a caller never branches on Option and never learns the selection exists.
    public double Pdf(WorldDirection direction) =>
        direction.Normalize() switch {
            var d => SunSelection switch {
                var p => (p * SunDensity(d)) + ((1.0 - p) * DomeDensity(d)),
            },
        };

    // A uniform draw over the disc's own cap has the reciprocal of that cap's solid angle as its density, and zero
    // off the disc — the delta-free density that makes a half-degree source MIS-balanceable at all.
    double SunDensity(WorldDirection direction) =>
        Sun.Map(disc => disc.Contains(direction) && disc.SolidAngle > 0.0 ? 1.0 / disc.SolidAngle : 0.0).IfNone(0.0);

    // The guide's own texel density at the STORED luminance its mass was built from, so intensity never skews a
    // weight; with no guide the density is the uniform sphere and SAYS SO — a declared degradation, not a fallback.
    double DomeDensity(WorldDirection direction) =>
        Products.Cdf
            .Map(guide => guide.Density(Map.Stored(EnvironmentMap.Rotated(direction, -Map.Rotation), lod: 0.0).Luminance))
            .IfNone(1.0 / (4.0 * Math.PI));

    // Solar draws uniformly over the cap: the zenith cosine is LINEAR in the draw between the rim cosine and one,
    // which is the area-preserving parameterization on a sphere — a naive angle draw clusters at the centre and
    // reads a limb-darkened rim as noise. The completion rotates the cap-local triple onto the sun's own direction
    // through WorldDirection's one Oriented crossing.
    EnvironmentSample Solar(SolarDisc disc, UnitInterval u0, UnitInterval u1) {
        double cosTheta = 1.0 - (u0.Value * (1.0 - disc.CosHalfAngle));
        double sinTheta = Math.Sqrt(Math.Max(0.0, 1.0 - (cosTheta * cosTheta))), phi = 2.0 * Math.PI * u1.Value;
        WorldDirection direction = disc.Direction.Oriented(sinTheta * Math.Cos(phi), sinTheta * Math.Sin(phi), cosTheta);
        UnitInterval radius = disc.Radius(direction);
        return new EnvironmentSample.Sun(
            direction, disc.Limb(radius).Scale(Map.Intensity.RadiometricSi), Pdf(direction), radius);
    }

    // ONE guided draw. Every guide grid is equirect — a non-equirect dome projected before accumulation — so the drawn
    // texel lifts through the frozen equirect inverse and the radiance reads the SOURCE map BY DIRECTION, never by a
    // texel index the guide's grid and the map's grid would have to share. With no guide the draw is the uniform
    // sphere on the same return shape.
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

    // UniformDirection draws area-preserving over the sphere: z is linear in u1 so the shell measure is uniform, which
    // a naive (theta, phi) grid draw is not — it clusters at the poles exactly where the equirect texels already do.
    static WorldDirection UniformDirection(UnitInterval u0, UnitInterval u1) {
        double z = 1.0 - (2.0 * u1.Value), r = Math.Sqrt(Math.Max(0.0, 1.0 - (z * z))), phi = 2.0 * Math.PI * u0.Value;
        return new WorldDirection(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }
}
```

## [06]-[RESEARCH]

(none)
